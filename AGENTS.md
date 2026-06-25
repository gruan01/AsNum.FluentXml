# Project Guidelines

## Build and Test

```bash
# Build the library (multi-target: net48 + net8.0 + net10.0)
dotnet build AsNum.FluentXml/AsNum.FluentXml.csproj

# Run tests (MSTest v1, .NET Framework 4.8 — requires msbuild or VS)
dotnet test AsNum.FluentXml.Test/AsNum.FluentXml.Test.csproj
```

## Architecture

**AsNum.FluentXml** generates XML from C# anonymous objects using a fluent API — no POCOs or XElement trees needed. Nested anonymous objects become nested XML. Extension methods on `object` (e.g., `.AsAttribute()`, `.AsElement()`) mark how each value renders into XML. Everything lives in the `AsNum.FluentXml` namespace.

The pipeline:

1. User describes XML structure as a nested anonymous object, wrapping values with extension methods
2. `FluentXmlHelper.Build(object, name, ns)` recursively walks properties via reflection, dispatching on type (`FluentXmlBase`, `IEnumerable`, XElement, primitives, or nested objects)
3. `FluentXmlHelper.GetXml(data, root, ns)` wraps the output in an `XDocument` and serializes to string

### Key Types

| Type | Purpose |
|------|---------|
| `FluentXmlHelper` | Static entry point: `GetXml()`, `GetXmlData()`, and the `Build()` recursion engine. All extension methods are defined here. |
| `FluentXmlBase` / `FluentXmlBase<T>` | Abstract base. Stores `Name`, `NS`, `Format`, `NullVisible`. Generic variant adds `Value` and implicit conversion to `T`. |
| `FluentXmlAttribute<T>` | Renders `Value` as `XAttribute` |
| `FluentXmlElement<T>` | Renders `Value` as `XElement`; supports `AdditionalNamespace` via `.AddNameSpace(prefix, ns)` |
| `FluentXmlElementValue<T>` | Renders `Value` as `XCData` (CDATA section) |
| `FluentXmlEmptyElement` | Singleton-ish factory: `FluentXmlEmptyElement.Create()` → self-closing `<tag />` |
| `FluentXmlIgnore<T>` | Skips the value entirely in output (`BuildXml` returns null) |

### Key APIs

```csharp
// Extension methods on object (defined in FluentXmlHelper)
value.AsAttribute(format, name, ns)       // → XAttribute
value.AsElement(name, format, ns)          // → XElement
value.AsElementValue(format)               // → XCData (CDATA)
value.AsIgnore()                           // → skip
enumerable.AsElementArray(itemName, ...)   // → IEnumerable<FluentXmlElement<T>>

// Fluent configuration (chainable)
.SetFormat(string)      .SetNullVisible(bool)
.SetNameSpace(XNamespace)                          // on FluentXmlBase
.AddNameSpace(prefix, ns)                            // on FluentXmlElement<T>

// Entry points
FluentXmlEmptyElement.Create()
FluentXmlHelper.GetXml(data, rootName, ns?, settings?)
FluentXmlHelper.GetXmlData(data, rootName, ns?, settings?)
```

## Conventions

- **Null produces no output**: `FluentXmlBase<T>.Build()` returns null when `Value` is null, unless `NullVisible` is explicitly set (or using `FluentXmlEmptyElement` which sets `NullVisible = true`).
- **Default encoding**: UTF-8 without BOM (`new UTF8Encoding(false)`).
- **Anonymous object detection**: `Build()` uses reflection on anonymous types — property names become XML element names. Nested anonymous objects become nested XML.
- **Chinese comments in csproj** explain the multi-targeting rationale (net452 exists for legacy project compatibility).
- **SDK-style library csproj**, legacy non-SDK test csproj (MSTest v1, .NET Framework 4.8).

## Code Style

- C# 9.0 (`<LangVersion>9.0</LangVersion>`)
- All public types in `AsNum.FluentXml` namespace
- PascalCase throughout
- Empty `<summary>` XML doc comment blocks on most public members (placeholders)
- Extension methods defined on `object` for maximum discoverability
