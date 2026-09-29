# <img src="/src/icon.png" height="30px"> Verify.Cecil

[![Discussions](https://img.shields.io/badge/Verify-Discussions-yellow?svg=true&label=)](https://github.com/orgs/VerifyTests/discussions)
[![Build status](https://github.com/VerifyTests/Verify.Cecil/actions/workflows/build.yml/badge.svg)](https://github.com/VerifyTests/Verify.Cecil/actions/workflows/build.yml)
[![NuGet Status](https://img.shields.io/nuget/v/Verify.Cecil.svg?label=Verify.Cecil)](https://www.nuget.org/packages/Verify.Cecil/)
[![NuGet Status](https://img.shields.io/nuget/v/Verify.Cecil.FodyHelpers.svg?label=Verify.Cecil.FodyHelpers)](https://www.nuget.org/packages/Verify.Cecil.FodyHelpers/)

Extends [Verify](https://github.com/VerifyTests/Verify) to allow snapshot testing and structural validation of [Mono.Cecil](https://github.com/jbevain/cecil) modules, types and members. An alternative to PEVerify.<!-- singleLineInclude: intro. path: /docs/intro.include.md -->

**See [Milestones](../../milestones?state=closed) for release notes.**


## Sponsors

### Entity Framework Extensions<!-- include: sponsors. path: /docs/sponsors.include.md -->

[Entity Framework Extensions](https://entityframework-extensions.net/?utm_source=simoncropp&utm_medium=Verify.Cecil) is a major sponsor and is proud to contribute to the development this project.

[![Entity Framework Extensions](https://raw.githubusercontent.com/VerifyTests/Verify.Cecil/refs/heads/main/docs/zzz.png)](https://entityframework-extensions.net/?utm_source=simoncropp&utm_medium=Verify.Cecil)

### Developed using JetBrains IDEs

[![JetBrains logo.](https://raw.githubusercontent.com/VerifyTests/Verify.Cecil/main/docs/jetbrains.png)](https://jb.gg/OpenSourceSupport)<!-- endInclude -->


## NuGet

There are two packages, built from the same source. Use the one that matches the Mono.Cecil already in the test project.

 * [Verify.Cecil](https://nuget.org/packages/Verify.Cecil) targets the [Mono.Cecil](https://nuget.org/packages/Mono.Cecil) package.
 * [Verify.Cecil.FodyHelpers](https://nuget.org/packages/Verify.Cecil.FodyHelpers) targets the Mono.Cecil bundled in [FodyHelpers](https://nuget.org/packages/FodyHelpers). Use this for Fody weaver tests.

The Mono.Cecil bundled in FodyHelpers is signed with a different key to the Mono.Cecil package, so the two cannot be mixed. A test project that references FodyHelpers should also avoid the `TUnit` metapackage, since it brings in the Mono.Cecil package. Reference `TUnit.Core`, `TUnit.Engine` and `TUnit.Assertions` instead.


## Usage

<!-- snippet: enable -->
<a id='snippet-enable'></a>
```cs
[ModuleInitializer]
public static void Init() =>
    VerifyCecil.Initialize();
```
<sup><a href='/src/Tests/ModuleInitializer.cs#L3-L9' title='Snippet source file'>snippet source</a> | <a href='#snippet-enable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The following Cecil types can be verified: `AssemblyDefinition`, `ModuleDefinition`, `TypeDefinition`, `MethodDefinition`, `FieldDefinition`, `PropertyDefinition` and `EventDefinition`.

The output is ILAsm-like text. It is deterministic: RVAs, metadata tokens, MVIDs and the source revision in `AssemblyInformationalVersionAttribute` are excluded, and instruction offsets are computed from the instructions. So a method modified in memory is shown with the offsets it will have when written.

Given the following type:

<!-- snippet: Target.cs -->
<a id='snippet-Target.cs'></a>
```cs
public class Target :
    INotifyPropertyChanged
{
    void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new(propertyName));

    public event PropertyChangedEventHandler? PropertyChanged;

    string? property;

    public string? Property
    {
        get => property;
        set
        {
            property = value;
            OnPropertyChanged();
        }
    }
}
```
<sup><a href='/src/AssemblyToProcess/Target.cs#L1-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-Target.cs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Verify a type

<!-- snippet: TypeUsage -->
<a id='snippet-TypeUsage'></a>
```cs
[Test]
public async Task TypeUsage()
{
    using var module = ModuleDefinition.ReadModule(assemblyPath);
    await Verify(module.GetType("Target"));
}
```
<sup><a href='/src/Tests/Tests.cs#L7-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-TypeUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Result:

<!-- snippet: Tests.TypeUsage.verified.il -->
<a id='snippet-Tests.TypeUsage.verified.il'></a>
```il
.class public auto ansi beforefieldinit Target
  extends [System.Runtime]System.Object
  implements [System.ObjectModel]System.ComponentModel.INotifyPropertyChanged
{
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = (2)
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = (0)
  .interfaceimpl type [System.ObjectModel]System.ComponentModel.INotifyPropertyChanged
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = (0)
  .field private class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler PropertyChanged
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    .custom instance void [System.Runtime]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [System.Runtime]System.Diagnostics.DebuggerBrowsableState) = ([System.Runtime]System.Diagnostics.DebuggerBrowsableState(0))
  .field private string property

  .method private hidebysig instance void OnPropertyChanged([opt] string propertyName) cil managed
  {
    .param [1] = nullref
      .custom instance void [System.Runtime]System.Runtime.CompilerServices.CallerMemberNameAttribute::.ctor()
    IL_0000: ldarg.0
    IL_0001: ldfld class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler Target::PropertyChanged
    IL_0006: dup
    IL_0007: brtrue.s IL_000c
    IL_0009: pop
    IL_000a: br.s IL_0019
    IL_000c: ldarg.0
    IL_000d: ldarg.1
    IL_000e: newobj instance void [System.ObjectModel]System.ComponentModel.PropertyChangedEventArgs::.ctor(string)
    IL_0013: callvirt instance void [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler::Invoke(object, class [System.ObjectModel]System.ComponentModel.PropertyChangedEventArgs)
    IL_0018: nop
    IL_0019: ret
  }

  .method public hidebysig newslot specialname virtual final instance void add_PropertyChanged(class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler value) cil managed
  {
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    .locals init (
      [0] class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler,
      [1] class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler,
      [2] class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler
    )

    IL_0000: ldarg.0
    IL_0001: ldfld class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler Target::PropertyChanged
    IL_0006: stloc.0
    IL_0007: ldloc.0
    IL_0008: stloc.1
    IL_0009: ldloc.1
    IL_000a: ldarg.1
    IL_000b: call class [System.Runtime]System.Delegate [System.Runtime]System.Delegate::Combine(class [System.Runtime]System.Delegate, class [System.Runtime]System.Delegate)
    IL_0010: castclass [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler
    IL_0015: stloc.2
    IL_0016: ldarg.0
    IL_0017: ldflda class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler Target::PropertyChanged
    IL_001c: ldloc.2
    IL_001d: ldloc.1
    IL_001e: call !!0 [System.Threading]System.Threading.Interlocked::CompareExchange<class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler>(!!0&, !!0, !!0)
    IL_0023: stloc.0
    IL_0024: ldloc.0
    IL_0025: ldloc.1
    IL_0026: bne.un.s IL_0007
    IL_0028: ret
  }

  .method public hidebysig newslot specialname virtual final instance void remove_PropertyChanged(class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler value) cil managed
  {
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    .locals init (
      [0] class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler,
      [1] class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler,
      [2] class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler
    )

    IL_0000: ldarg.0
    IL_0001: ldfld class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler Target::PropertyChanged
    IL_0006: stloc.0
    IL_0007: ldloc.0
    IL_0008: stloc.1
    IL_0009: ldloc.1
    IL_000a: ldarg.1
    IL_000b: call class [System.Runtime]System.Delegate [System.Runtime]System.Delegate::Remove(class [System.Runtime]System.Delegate, class [System.Runtime]System.Delegate)
    IL_0010: castclass [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler
    IL_0015: stloc.2
    IL_0016: ldarg.0
    IL_0017: ldflda class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler Target::PropertyChanged
    IL_001c: ldloc.2
    IL_001d: ldloc.1
    IL_001e: call !!0 [System.Threading]System.Threading.Interlocked::CompareExchange<class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler>(!!0&, !!0, !!0)
    IL_0023: stloc.0
    IL_0024: ldloc.0
    IL_0025: ldloc.1
    IL_0026: bne.un.s IL_0007
    IL_0028: ret
  }

  .method public hidebysig specialname instance string get_Property() cil managed
  {
    IL_0000: ldarg.0
    IL_0001: ldfld string Target::property
    IL_0006: ret
  }

  .method public hidebysig specialname instance void set_Property(string value) cil managed
  {
    IL_0000: nop
    IL_0001: ldarg.0
    IL_0002: ldarg.1
    IL_0003: stfld string Target::property
    IL_0008: ldarg.0
    IL_0009: ldstr "Property"
    IL_000e: call instance void Target::OnPropertyChanged(string)
    IL_0013: nop
    IL_0014: ret
  }

  .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
  {
    IL_0000: ldarg.0
    IL_0001: call instance void [System.Runtime]System.Object::.ctor()
    IL_0006: nop
    IL_0007: ret
  }

  .property instance string Property()
  {
    .get instance string Target::get_Property()
    .set instance void Target::set_Property(string)
  }

  .event [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler PropertyChanged
  {
    .addon instance void Target::add_PropertyChanged(class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler)
    .removeon instance void Target::remove_PropertyChanged(class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler)
  }
}
```
<sup><a href='/src/Tests/Tests.TypeUsage.verified.il#L1-L133' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.TypeUsage.verified.il' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Verify a method

<!-- snippet: MethodUsage -->
<a id='snippet-MethodUsage'></a>
```cs
[Test]
public async Task MethodUsage()
{
    using var module = ModuleDefinition.ReadModule(assemblyPath);
    var type = module.GetType("Target");
    await Verify(type.Methods.Single(_ => _.Name == "OnPropertyChanged"));
}
```
<sup><a href='/src/Tests/Tests.cs#L18-L28' title='Snippet source file'>snippet source</a> | <a href='#snippet-MethodUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Result:

<!-- snippet: Tests.MethodUsage.verified.il -->
<a id='snippet-Tests.MethodUsage.verified.il'></a>
```il
.method private hidebysig instance void OnPropertyChanged([opt] string propertyName) cil managed
{
  .param [1] = nullref
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CallerMemberNameAttribute::.ctor()
  IL_0000: ldarg.0
  IL_0001: ldfld class [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler Target::PropertyChanged
  IL_0006: dup
  IL_0007: brtrue.s IL_000c
  IL_0009: pop
  IL_000a: br.s IL_0019
  IL_000c: ldarg.0
  IL_000d: ldarg.1
  IL_000e: newobj instance void [System.ObjectModel]System.ComponentModel.PropertyChangedEventArgs::.ctor(string)
  IL_0013: callvirt instance void [System.ObjectModel]System.ComponentModel.PropertyChangedEventHandler::Invoke(object, class [System.ObjectModel]System.ComponentModel.PropertyChangedEventArgs)
  IL_0018: nop
  IL_0019: ret
}
```
<sup><a href='/src/Tests/Tests.MethodUsage.verified.il#L1-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.MethodUsage.verified.il' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Verify a property

<!-- snippet: PropertyUsage -->
<a id='snippet-PropertyUsage'></a>
```cs
[Test]
public async Task PropertyUsage()
{
    using var module = ModuleDefinition.ReadModule(assemblyPath);
    var type = module.GetType("Target");
    await Verify(type.Properties.Single(_ => _.Name == "Property"));
}
```
<sup><a href='/src/Tests/Tests.cs#L30-L40' title='Snippet source file'>snippet source</a> | <a href='#snippet-PropertyUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Result:

<!-- snippet: Tests.PropertyUsage.verified.il -->
<a id='snippet-Tests.PropertyUsage.verified.il'></a>
```il
.property instance string Property()
{
  .get instance string Target::get_Property()
  .set instance void Target::set_Property(string)
}
```
<sup><a href='/src/Tests/Tests.PropertyUsage.verified.il#L1-L5' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.PropertyUsage.verified.il' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Verify a module

<!-- snippet: ModuleUsage -->
<a id='snippet-ModuleUsage'></a>
```cs
[Test]
public async Task ModuleUsage()
{
    using var module = ModuleDefinition.ReadModule(assemblyPath);
    await Verify(module);
}
```
<sup><a href='/src/Tests/Tests.cs#L58-L67' title='Snippet source file'>snippet source</a> | <a href='#snippet-ModuleUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Validation

A snapshot detects changes, but not IL that is invalid. So before converting, the IL is checked, and verification fails with a `CecilValidationException` listing the problems. This covers the problems PEVerify commonly reports for woven assemblies:

 * Branch and switch targets that are not in the method body.
 * Short branches whose target is out of range. Cecil silently writes a wrong offset for these.
 * Stack underflow, differing stack depths where paths merge, items left on the stack at `ret`, and a non-empty stack on entry to a `try`.
 * Control falling through the end of the method body, and empty method bodies.
 * Exception handlers with boundaries missing or outside the body, empty or overlapping ranges, and catch handlers without a catch type.
 * Operands of the wrong type, arguments and locals that do not exist, and variables or parameters from another method.
 * Method, field and type references that do not resolve, and references that do not match the definition: static vs instance fields and methods, `newobj` on a method that is not a constructor, and `callvirt` on a static method.
 * Abstract methods in types that are not abstract.

References are only checked when the assembly containing them can be found. If it cannot, the check is skipped rather than reported.

<!-- snippet: ValidationThrows -->
<a id='snippet-ValidationThrows'></a>
```cs
[Test]
public async Task ValidationThrows()
{
    using var module = ModuleDefinition.ReadModule(assemblyPath);
    var method = AddMethod(
        module,
        module.TypeSystem.Void,
        il =>
        {
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        });

    await Assert.ThrowsAsync<CecilValidationException>(
        async () => await Verify(method));
}
```
<sup><a href='/src/Tests/ValidationTests.cs#L157-L176' title='Snippet source file'>snippet source</a> | <a href='#snippet-ValidationThrows' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To snapshot without validating:

<!-- snippet: DisableCecilValidation -->
<a id='snippet-DisableCecilValidation'></a>
```cs
[Test]
public async Task DisableCecilValidation()
{
    using var module = ModuleDefinition.ReadModule(assemblyPath);
    var method = AddMethod(
        module,
        module.TypeSystem.Void,
        il =>
        {
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Ret);
        });

    await Verify(method)
        .DisableCecilValidation();
}
```
<sup><a href='/src/Tests/ValidationTests.cs#L178-L197' title='Snippet source file'>snippet source</a> | <a href='#snippet-DisableCecilValidation' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`CecilValidator` can also be used directly:

```cs
IReadOnlyList<string> problems = CecilValidator.Validate(module);
CecilValidator.ThrowIfInvalid(module);
```


## Replacing PEVerify in Fody weaver tests

PEVerify only runs on .NET Framework and needs the Windows SDK installed. Verify.Cecil.FodyHelpers runs on any platform and target framework.

Disable PEVerify in `ExecuteTestRun` and verify the woven types:

<!-- snippet: WeaverUsage -->
<a id='snippet-WeaverUsage'></a>
```cs
[Test]
public async Task WeaverUsage()
{
    var weaver = new ModuleWeaver();
    var result = weaver.ExecuteTestRun(
        "AssemblyToProcess.dll",
        // Verify.Cecil replaces PEVerify
        runPeVerify: false);

    using var module = ModuleDefinition.ReadModule(result.AssemblyPath);
    await Verify(module.GetType("Target"));
}
```
<sup><a href='/src/FodyHelpers.Tests/WeaverTests.cs#L3-L18' title='Snippet source file'>snippet source</a> | <a href='#snippet-WeaverUsage' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

A weaver that produces invalid IL fails the test:

<!-- snippet: BrokenWeaver -->
<a id='snippet-BrokenWeaver'></a>
```cs
[Test]
public async Task BrokenWeaver()
{
    var weaver = new ModuleWeaver
    {
        Broken = true
    };
    var result = weaver.ExecuteTestRun(
        "AssemblyToProcess.dll",
        runPeVerify: false,
        assemblyName: "BrokenWeaver");

    using var module = ModuleDefinition.ReadModule(result.AssemblyPath);
    var exception = await Assert.ThrowsAsync<CecilValidationException>(
        async () => await Verify(module.GetType("Target")));

    await Assert.That(exception!.Problems)
        .IsEquivalentTo(
        [
            "System.String Target::Injected(): " +
            "IL_0006: ret: stack must be empty after ret, but has 1 left"
        ]);
}
```
<sup><a href='/src/FodyHelpers.Tests/WeaverTests.cs#L20-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-BrokenWeaver' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Icon

[Helmet](https://thenounproject.com/term/helmet/9554/) designed by [Leonidas Ikonomou](https://thenounproject.com/alterego) from [The Noun Project](https://thenounproject.com).
