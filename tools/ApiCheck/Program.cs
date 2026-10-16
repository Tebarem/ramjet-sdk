// The SDK leak check of Mc.Scripting.Api, the reference assembly the Ramjet.Api package ships. Two modes:
//
//   pack:   ApiCheck --stub <stripped dll> --real <built dll> [--real <worker build> ...] --xml-in <compiler XML> --xml-out <XML to ship>
//                    [--surface-out <listing>]
//           Where the API is built (it needs the real assemblies). Writes the XML docs filtered to the public surface (the compiler
//           documents internal members too; crefs to them become plain text) and, with --surface-out, the public surface as text.
//   verify: ApiCheck --verify --stub <dll> --xml <xml> --surface <listing>
//           Where the package is packed and published (the public SDK repository's CI): the committed files only.
//
// Both fail (exit 1) when the assembly holds anything but public API (a non-public type, a method body, a non-public member,
// InternalsVisibleTo, a reference to another Mc.Scripting assembly, a resource), or when a file carries a build trace (a git commit, a
// path of the build machine, a match of a --deny pattern). pack also fails when the public surface differs from a real build; verify
// when it differs from the listing, or when the XML documents something that is not public.

using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var opts = Args.Parse(args);
if (opts is null)
{
    Console.Error.WriteLine("usage: ApiCheck --stub <dll> --real <dll> [--real <dll> ...] --xml-in <xml> --xml-out <xml> [--surface-out <txt>]");
    Console.Error.WriteLine("       ApiCheck --verify --stub <dll> --xml <xml> --surface <txt>");
    return 2;
}

var errors = new List<string>();
CheckMetadata(opts.Stub, errors);

var framework = FrameworkReferences();
var stubApi = ApiSurface.Of(opts.Stub, framework);

if (opts.Verify)
{
    ScanForBuildTraces(opts.Stub, errors, opts.Deny);
    ScanForBuildTraces(opts.XmlIn, errors, opts.Deny);
    // every documented member is public API (the pack mode filtered the compiler's XML; a hand edit must not add internal names)
    foreach (var member in XDocument.Load(opts.XmlIn).Root!.Element("members")!.Elements("member"))
    {
        string id = (string?)member.Attribute("name") ?? "";
        if (!stubApi.DocIds.Contains(id)) errors.Add($"the XML documents '{id}', which is not public API of the assembly");
    }
    var listed = File.ReadAllLines(opts.Surface!).Where(l => l.Length > 0).ToHashSet(StringComparer.Ordinal);
    foreach (string m in stubApi.Entries.Except(listed).Order(StringComparer.Ordinal).Take(40)) errors.Add($"not in {Path.GetFileName(opts.Surface)}: {m}");
    foreach (string m in listed.Except(stubApi.Entries).Order(StringComparer.Ordinal).Take(40)) errors.Add($"in {Path.GetFileName(opts.Surface)} but not in the assembly: {m}");
    if (errors.Count > 0)
    {
        foreach (string e in errors) Console.Error.WriteLine($"ApiCheck: error: {e}");
        Console.Error.WriteLine($"ApiCheck: {errors.Count} problem(s); do not publish this package");
        return 1;
    }
    Console.WriteLine($"ApiCheck: {stubApi.Entries.Count} public API entries, as listed in {Path.GetFileName(opts.Surface)}; no internals, no IL, no build traces");
    return 0;
}
foreach (string real in opts.Real)
{
    var realApi = ApiSurface.Of(real, framework);
    var missing = realApi.Entries.Except(stubApi.Entries).Order(StringComparer.Ordinal).ToList();
    var extra = stubApi.Entries.Except(realApi.Entries).Order(StringComparer.Ordinal).ToList();
    foreach (string m in missing.Take(40)) errors.Add($"missing from the stub (in {Path.GetFileName(Path.GetDirectoryName(real))}/{Path.GetFileName(real)}): {m}");
    foreach (string m in extra.Take(40)) errors.Add($"in the stub but not in {real}: {m}");
    if (missing.Count + extra.Count > 80) errors.Add($"... {missing.Count} missing and {extra.Count} extra entries in all");
}

var (kept, dropped, rewritten) = FilterDocs(opts.XmlIn, opts.XmlOut!, stubApi.DocIds);
ScanForBuildTraces(opts.Stub, errors, opts.Deny);
ScanForBuildTraces(opts.XmlOut!, errors, opts.Deny);

if (errors.Count > 0)
{
    foreach (string e in errors) Console.Error.WriteLine($"ApiCheck: error: {e}");
    Console.Error.WriteLine($"ApiCheck: {errors.Count} problem(s); the package must not be built from this assembly");
    return 1;
}
if (opts.SurfaceOut is { } listing)
{
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(listing))!);
    File.WriteAllLines(listing, stubApi.Entries.Order(StringComparer.Ordinal));
}
Console.WriteLine($"ApiCheck: {stubApi.Entries.Count} public API entries, identical in {opts.Real.Count} build(s); no internals, no IL");
Console.WriteLine($"ApiCheck: XML docs: {kept} kept, {dropped} non-public dropped, {rewritten} cref(s) to non-public symbols made plain text");
return 0;

// ---- the stripped assembly holds public API only ----

static void CheckMetadata(string path, List<string> errors)
{
    using var stream = File.OpenRead(path);
    using var pe = new PEReader(stream);
    var md = pe.GetMetadataReader();

    foreach (var th in md.TypeDefinitions)
    {
        var type = md.GetTypeDefinition(th);
        string name = TypeName(md, type);
        if (name == "<Module>") continue;
        var vis = type.Attributes & TypeAttributes.VisibilityMask;
        if (vis is not (TypeAttributes.Public or TypeAttributes.NestedPublic or TypeAttributes.NestedFamily or TypeAttributes.NestedFamORAssem))
            errors.Add($"non-public type {name} ({vis})");

        foreach (var mh in type.GetMethods())
        {
            var method = md.GetMethodDefinition(mh);
            string member = $"{name}.{md.GetString(method.Name)}";
            if (method.RelativeVirtualAddress != 0) errors.Add($"method with an IL body: {member}");
            var access = method.Attributes & MethodAttributes.MemberAccessMask;
            bool explicitImpl = md.GetString(method.Name).Contains('.') && !md.GetString(method.Name).StartsWith('.');
            if (access is not (MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem) && !explicitImpl)
                errors.Add($"non-public method {member} ({access})");
        }
        foreach (var fh in type.GetFields())
        {
            var field = md.GetFieldDefinition(fh);
            var access = field.Attributes & FieldAttributes.FieldAccessMask;
            if (access is FieldAttributes.Public or FieldAttributes.Family or FieldAttributes.FamORAssem) continue;
            // Refasmer's placeholder (empty and non-empty types stay distinct); any other non-public field would carry a real name
            string fieldName = md.GetString(field.Name);
            if (fieldName != "<SyntheticNonEmptyStructMarker>") errors.Add($"non-public field {name}.{fieldName}");
        }
    }

    foreach (var ah in md.GetAssemblyDefinition().GetCustomAttributes())
    {
        string attr = AttributeTypeName(md, md.GetCustomAttribute(ah));
        if (attr.EndsWith("InternalsVisibleToAttribute", StringComparison.Ordinal)) errors.Add("assembly has InternalsVisibleTo");
    }
    foreach (var rh in md.AssemblyReferences)
    {
        string reference = md.GetString(md.GetAssemblyReference(rh).Name);
        if (reference.StartsWith("Mc.Scripting", StringComparison.Ordinal)) errors.Add($"references {reference}");
    }
    if (md.ManifestResources.Count > 0) errors.Add($"{md.ManifestResources.Count} manifest resource(s)");
}

static string TypeName(MetadataReader md, TypeDefinition type)
{
    string name = md.GetString(type.Name);
    if (type.GetDeclaringType() is { IsNil: false } outer) return TypeName(md, md.GetTypeDefinition(outer)) + "+" + name;
    string ns = md.GetString(type.Namespace);
    return ns.Length == 0 ? name : ns + "." + name;
}

static string AttributeTypeName(MetadataReader md, CustomAttribute attr)
{
    if (attr.Constructor.Kind == HandleKind.MemberReference)
    {
        var parent = md.GetMemberReference((MemberReferenceHandle)attr.Constructor).Parent;
        if (parent.Kind == HandleKind.TypeReference) return md.GetString(md.GetTypeReference((TypeReferenceHandle)parent).Name);
    }
    else if (attr.Constructor.Kind == HandleKind.MethodDefinition)
    {
        var declaring = md.GetMethodDefinition((MethodDefinitionHandle)attr.Constructor).GetDeclaringType();
        return md.GetString(md.GetTypeDefinition(declaring).Name);
    }
    return "";
}

// What a build can leave in a file besides the API: a git commit (Source Link, +<sha> in AssemblyInformationalVersion) or a path of the
// build machine (PDB paths, CallerFilePath defaults). --deny adds patterns of the build's own: where the API is built they name the
// internal docs and source folders that public doc comments must not point into.
static void ScanForBuildTraces(string path, List<string> errors, IReadOnlyList<string> deny)
{
    byte[] bytes = File.ReadAllBytes(path);
    foreach (var text in new[] { System.Text.Encoding.UTF8.GetString(bytes), System.Text.Encoding.Unicode.GetString(bytes) })
    {
        if (System.Text.RegularExpressions.Regex.Match(text, "(?<![0-9a-fA-F])[0-9a-f]{40}(?![0-9a-fA-F])") is { Success: true } sha)
            errors.Add($"{Path.GetFileName(path)} holds a 40-hex string (a git commit?): {sha.Value}");
        if (System.Text.RegularExpressions.Regex.Match(text, @"[A-Za-z]:[\/]Users[\/]|/(home/[a-z]|root/)") is { Success: true } local)
            errors.Add($"{Path.GetFileName(path)} holds a path of the build machine near '{local.Value}'");
        foreach (string pattern in deny)
        {
            if (System.Text.RegularExpressions.Regex.Match(text, pattern) is { Success: true } inner)
                errors.Add($"{Path.GetFileName(path)} matches the denied pattern '{pattern}' near '{inner.Value}'");
        }
    }
}

// The runtime's own assemblies (the checker runs on the plugins' target framework) resolve the API's framework types.
static ImmutableArray<MetadataReference> FrameworkReferences()
{
    string dir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    var refs = ImmutableArray.CreateBuilder<MetadataReference>();
    foreach (string dll in Directory.EnumerateFiles(dir, "*.dll"))
    {
        try
        {
            using var s = File.OpenRead(dll);
            using var pe = new PEReader(s);
            if (!pe.HasMetadata) continue;
        }
        catch (BadImageFormatException) { continue; }
        refs.Add(MetadataReference.CreateFromFile(dll));
    }
    return refs.ToImmutable();
}

// ---- XML docs: only the shipped surface ----

static (int Kept, int Dropped, int Rewritten) FilterDocs(string input, string output, HashSet<string> api)
{
    var doc = XDocument.Load(input, LoadOptions.PreserveWhitespace);
    var members = doc.Root!.Element("members")!;
    int kept = 0, dropped = 0, rewritten = 0;
    foreach (var member in members.Elements("member").ToList())
    {
        if (api.Contains((string?)member.Attribute("name") ?? ""))
        {
            kept++;
            continue;
        }
        // the whitespace before the element goes with it
        if (member.PreviousNode is XText ws && string.IsNullOrWhiteSpace(ws.Value)) ws.Remove();
        member.Remove();
        dropped++;
    }
    foreach (var element in members.Descendants().Where(e => e.Attribute("cref") is not null).ToList())
    {
        string cref = (string)element.Attribute("cref")!;
        if (api.Contains(cref) || !IsOwnSymbol(cref)) continue;
        rewritten++;
        if (element.Name == "seealso")
        {
            element.Remove();
            continue;
        }
        string text = element.Value.Length > 0 ? element.Value : ShortName(cref);
        element.ReplaceWith(new XElement("c", text));
    }
    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);
    doc.Save(output);
    return (kept, dropped, rewritten);
}

// A cref into the API's own namespaces (Mc.Scripting...): unresolved ("!:") or not in the public surface means it names an internal.
static bool IsOwnSymbol(string cref)
{
    int colon = cref.IndexOf(':');
    string body = colon >= 0 ? cref[(colon + 1)..] : cref;
    return cref.StartsWith("!:", StringComparison.Ordinal) || body.StartsWith("Mc.Scripting", StringComparison.Ordinal);
}

static string ShortName(string cref)
{
    string body = cref[(cref.IndexOf(':') + 1)..];
    int paren = body.IndexOf('(');
    if (paren >= 0) body = body[..paren];
    int dot = body.LastIndexOf('.');
    return dot >= 0 ? body[(dot + 1)..] : body;
}

// pack: Real, XmlIn (the compiler's XML), XmlOut, SurfaceOut (optional). verify: XmlIn (the shipped XML) and Surface. Both: Deny.
sealed record Args(bool Verify, string Stub, List<string> Real, string XmlIn, string? XmlOut, string? SurfaceOut, string? Surface,
    List<string> Deny)
{
    public static Args? Parse(string[] args)
    {
        bool verify = args.Length > 0 && args[0] == "--verify";
        string? stub = null, xmlIn = null, xmlOut = null, surfaceOut = null, surface = null;
        var real = new List<string>();
        var deny = new List<string>();
        int start = verify ? 1 : 0;
        if ((args.Length - start) % 2 != 0) return null;
        for (int i = start; i + 1 < args.Length; i += 2)
        {
            switch (args[i])
            {
                case "--stub": stub = args[i + 1]; break;
                case "--real" when !verify: real.Add(args[i + 1]); break;
                case "--xml-in" when !verify: xmlIn = args[i + 1]; break;
                case "--xml-out" when !verify: xmlOut = args[i + 1]; break;
                case "--surface-out" when !verify: surfaceOut = args[i + 1]; break;
                case "--xml" when verify: xmlIn = args[i + 1]; break;
                case "--surface" when verify: surface = args[i + 1]; break;
                case "--deny": deny.Add(args[i + 1]); break;
                default: return null;
            }
        }
        if (stub is null || xmlIn is null) return null;
        if (verify) return surface is null ? null : new Args(true, stub, real, xmlIn, null, null, surface, deny);
        if (real.Count == 0 || xmlOut is null) return null;
        return new Args(false, stub, real, xmlIn, xmlOut, surfaceOut, null, deny);
    }
}

// The public surface of an assembly as Roslyn sees it from a consumer: every type and member a plugin can name (public, protected,
// protected internal), with its modifiers, signature, constraints, base types and interfaces. Accessors count only when a plugin can
// call them, so an internal setter is not part of the surface.
sealed class ApiSurface
{
    public HashSet<string> Entries { get; } = new(StringComparer.Ordinal);
    public HashSet<string> DocIds { get; } = new(StringComparer.Ordinal);

    private static readonly SymbolDisplayFormat Format = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints | SymbolDisplayGenericsOptions.IncludeVariance,
        memberOptions: SymbolDisplayMemberOptions.IncludeParameters | SymbolDisplayMemberOptions.IncludeType | SymbolDisplayMemberOptions.IncludeModifiers
            | SymbolDisplayMemberOptions.IncludeAccessibility | SymbolDisplayMemberOptions.IncludeExplicitInterface | SymbolDisplayMemberOptions.IncludeRef
            | SymbolDisplayMemberOptions.IncludeConstantValue | SymbolDisplayMemberOptions.IncludeContainingType,
        kindOptions: SymbolDisplayKindOptions.IncludeTypeKeyword | SymbolDisplayKindOptions.IncludeMemberKeyword,
        parameterOptions: SymbolDisplayParameterOptions.IncludeParamsRefOut | SymbolDisplayParameterOptions.IncludeType | SymbolDisplayParameterOptions.IncludeName
            | SymbolDisplayParameterOptions.IncludeDefaultValue | SymbolDisplayParameterOptions.IncludeExtensionThis | SymbolDisplayParameterOptions.IncludeOptionalBrackets,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier
            | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    public static ApiSurface Of(string path, ImmutableArray<MetadataReference> framework)
    {
        var reference = MetadataReference.CreateFromFile(path);
        var compilation = CSharpCompilation.Create("ApiCheckProbe", references: framework.Add(reference),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        var assembly = (IAssemblySymbol)(compilation.GetAssemblyOrModuleSymbol(reference)
            ?? throw new InvalidOperationException($"{path} is not an assembly"));
        var surface = new ApiSurface();
        surface.Walk(assembly.GlobalNamespace);
        return surface;
    }

    private void Walk(INamespaceSymbol ns)
    {
        foreach (var child in ns.GetNamespaceMembers()) Walk(child);
        foreach (var type in ns.GetTypeMembers()) Walk(type);
    }

    private static bool Visible(ISymbol s) =>
        s.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal;

    private void Walk(INamedTypeSymbol type)
    {
        if (!Visible(type)) return;
        string bases = type.BaseType is { SpecialType: not (SpecialType.System_Object or SpecialType.System_ValueType or SpecialType.System_Enum) } b
            ? " : " + b.ToDisplayString(Format) : "";
        string interfaces = string.Join(", ", type.Interfaces.Where(Visible).Select(i => i.ToDisplayString(Format)).Order(StringComparer.Ordinal));
        string modifiers = (type.IsStatic ? "static " : type.IsAbstract && type.TypeKind == TypeKind.Class ? "abstract " : "")
            + (type.IsSealed && type.TypeKind == TypeKind.Class && !type.IsStatic ? "sealed " : "")
            + (type.IsReadOnly ? "readonly " : "") + (type.IsRefLikeType ? "ref " : "");
        Add(type, $"{modifiers}{type.ToDisplayString(Format)}{bases}{(interfaces.Length > 0 ? " [" + interfaces + "]" : "")}{Attributes(type)}");
        if (type.TypeKind == TypeKind.Enum && type.EnumUnderlyingType is { } underlying) Entries.Add($"{type.ToDisplayString(Format)} enum : {underlying.ToDisplayString(Format)}");

        foreach (var member in type.GetMembers())
        {
            switch (member)
            {
                case INamedTypeSymbol nested:
                    Walk(nested);
                    break;
                case IMethodSymbol method when Visible(method) && method.MethodKind is MethodKind.Ordinary or MethodKind.Constructor
                    or MethodKind.UserDefinedOperator or MethodKind.Conversion or MethodKind.Destructor:
                    Add(method, method.ToDisplayString(Format) + Attributes(method));
                    break;
                case IPropertySymbol property when Visible(property):
                    string get = property.GetMethod is { } g && Visible(g) ? " get;" : "";
                    string set = property.SetMethod is { } s && Visible(s) ? (s.IsInitOnly ? " init;" : " set;") : "";
                    Add(property, $"{property.ToDisplayString(Format)} {{{get}{set} }}{Attributes(property)}");
                    break;
                case IEventSymbol ev when Visible(ev):
                    Add(ev, ev.ToDisplayString(Format) + Attributes(ev));
                    break;
                case IFieldSymbol field when Visible(field):
                    Add(field, field.ToDisplayString(Format) + (field.IsReadOnly ? " readonly" : "") + (field.IsVolatile ? " volatile" : "") + Attributes(field));
                    break;
            }
        }
    }

    // only the attributes that change what a plugin may do with a symbol
    private static string Attributes(ISymbol symbol)
    {
        var names = symbol.GetAttributes().Select(a => a.AttributeClass?.ToDisplayString() ?? "")
            .Where(n => n is "System.ObsoleteAttribute" or "System.FlagsAttribute" or "System.AttributeUsageAttribute"
                or "System.Diagnostics.CodeAnalysis.ExperimentalAttribute" or "System.ComponentModel.EditorBrowsableAttribute")
            .Order(StringComparer.Ordinal).ToList();
        return names.Count == 0 ? "" : " @" + string.Join(",", names);
    }

    private void Add(ISymbol symbol, string entry)
    {
        Entries.Add(entry);
        if (symbol.GetDocumentationCommentId() is { } id) DocIds.Add(id);
    }
}
