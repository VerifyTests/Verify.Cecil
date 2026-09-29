.assembly extern System.Console 8:0:0:0
.assembly extern System.ObjectModel 8:0:0:0
.assembly extern System.Runtime 8:0:0:0
.assembly extern System.Threading 8:0:0:0

.assembly AssemblyToProcess
{
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = (8)
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = (property WrapNonExceptionThrows = true)
  .custom instance void [System.Runtime]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [System.Runtime]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ([System.Runtime]System.Diagnostics.DebuggableAttribute/DebuggingModes(263))
  .custom instance void [System.Runtime]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = (".NETCoreApp,Version=v8.0", property FrameworkDisplayName = ".NET 8.0")
  .custom instance void [System.Runtime]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ("AssemblyToProcess")
  .custom instance void [System.Runtime]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = ("Debug")
  .custom instance void [System.Runtime]System.Reflection.AssemblyCopyrightAttribute::.ctor(string) = ("Copyright")
  .custom instance void [System.Runtime]System.Reflection.AssemblyDescriptionAttribute::.ctor(string) = ("Sample assembly")
  .custom instance void [System.Runtime]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = ("1.0.0")
  .custom instance void [System.Runtime]System.Reflection.AssemblyInformationalVersionAttribute::.ctor(string) = ("1.0.0")
  .custom instance void [System.Runtime]System.Reflection.AssemblyProductAttribute::.ctor(string) = ("AssemblyToProcess")
  .custom instance void [System.Runtime]System.Reflection.AssemblyTitleAttribute::.ctor(string) = ("AssemblyToProcess")
  .publickeytoken = (C7 A3 45 12 EC D6 90 90)
  .ver 1:0:0:0
}

.module AssemblyToProcess.dll
.custom instance void [System.Runtime]System.Runtime.CompilerServices.RefSafetyRulesAttribute::.ctor(int32) = (11)

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

.class public abstract auto ansi sealed beforefieldinit AssemblyToProcess.Samples
  extends [System.Runtime]System.Object
{
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.NullableContextAttribute::.ctor(uint8) = (1)
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.NullableAttribute::.ctor(uint8) = (0)
  .field public static literal int32 Answer = int32(42)
  .field private static initonly int32[] primes

  .method public hidebysig static int32 Prime(int32 index) cil managed
  {
    IL_0000: ldsfld int32[] AssemblyToProcess.Samples::primes
    IL_0005: ldarg.0
    IL_0006: ldelem.i4
    IL_0007: ret
  }

  .method public hidebysig static string Describe(int32 value) cil managed
  {
    .locals init (
      [0] int32,
      [1] int32,
      [2] string
    )

    IL_0000: nop
    IL_0001: ldarg.0
    IL_0002: stloc.1
    IL_0003: ldloc.1
    IL_0004: stloc.0
    IL_0005: ldloc.0
    IL_0006: switch (IL_0019, IL_0021, IL_0029)
    IL_0017: br.s IL_0031
    IL_0019: ldstr "zero"
    IL_001e: stloc.2
    IL_001f: br.s IL_0039
    IL_0021: ldstr "one"
    IL_0026: stloc.2
    IL_0027: br.s IL_0039
    IL_0029: ldstr "two"
    IL_002e: stloc.2
    IL_002f: br.s IL_0039
    IL_0031: ldstr "many"
    IL_0036: stloc.2
    IL_0037: br.s IL_0039
    IL_0039: ldloc.2
    IL_003a: ret
  }

  .method public hidebysig static int32 Parse(string text) cil managed
  {
    .locals init (
      [0] int32
    )

    IL_0000: nop
    IL_0001: nop
    IL_0002: ldarg.0
    IL_0003: call int32 [System.Runtime]System.Int32::Parse(string)
    IL_0008: stloc.0
    IL_0009: leave.s IL_001f
    IL_000b: pop
    IL_000c: nop
    IL_000d: ldc.i4.m1
    IL_000e: stloc.0
    IL_000f: leave.s IL_001f
    IL_0011: nop
    IL_0012: ldstr "parsed"
    IL_0017: call void [System.Console]System.Console::WriteLine(string)
    IL_001c: nop
    IL_001d: nop
    IL_001e: endfinally
    IL_001f: ldloc.0
    IL_0020: ret

    .try IL_0001 to IL_000b catch [System.Runtime]System.FormatException handler IL_000b to IL_0011
    .try IL_0001 to IL_0011 finally handler IL_0011 to IL_001f
  }

  .method public hidebysig static !!T First<class T>(class [System.Runtime]System.Collections.Generic.IEnumerable`1<!!T> items) cil managed
  {
    .locals init (
      [0] class [System.Runtime]System.Collections.Generic.IEnumerator`1<!!T>,
      [1] !!T,
      [2] !!T
    )

    IL_0000: nop
    IL_0001: nop
    IL_0002: ldarg.0
    IL_0003: callvirt instance class [System.Runtime]System.Collections.Generic.IEnumerator`1<!0> class [System.Runtime]System.Collections.Generic.IEnumerable`1<!!T>::GetEnumerator()
    IL_0008: stloc.0
    IL_0009: br.s IL_0017
    IL_000b: ldloc.0
    IL_000c: callvirt instance !0 class [System.Runtime]System.Collections.Generic.IEnumerator`1<!!T>::get_Current()
    IL_0011: stloc.1
    IL_0012: nop
    IL_0013: ldloc.1
    IL_0014: stloc.2
    IL_0015: leave.s IL_0032
    IL_0017: ldloc.0
    IL_0018: callvirt instance bool [System.Runtime]System.Collections.IEnumerator::MoveNext()
    IL_001d: brtrue.s IL_000b
    IL_001f: leave.s IL_002c
    IL_0021: ldloc.0
    IL_0022: brfalse.s IL_002b
    IL_0024: ldloc.0
    IL_0025: callvirt instance void [System.Runtime]System.IDisposable::Dispose()
    IL_002a: nop
    IL_002b: endfinally
    IL_002c: newobj instance void [System.Runtime]System.InvalidOperationException::.ctor()
    IL_0031: throw
    IL_0032: ldloc.2
    IL_0033: ret

    .try IL_0009 to IL_0021 finally handler IL_0021 to IL_002c
  }

  .method public hidebysig static class [System.Runtime]System.Func`2<int32, int32> Adder(int32 amount) cil managed
  {
    .locals init (
      [0] class AssemblyToProcess.Samples/'<>c__DisplayClass6_0'
    )

    IL_0000: newobj instance void AssemblyToProcess.Samples/'<>c__DisplayClass6_0'::.ctor()
    IL_0005: stloc.0
    IL_0006: ldloc.0
    IL_0007: ldarg.0
    IL_0008: stfld int32 AssemblyToProcess.Samples/'<>c__DisplayClass6_0'::amount
    IL_000d: ldloc.0
    IL_000e: ldftn instance int32 AssemblyToProcess.Samples/'<>c__DisplayClass6_0'::'<Adder>b__0'(int32)
    IL_0014: newobj instance void class [System.Runtime]System.Func`2<int32, int32>::.ctor(object, native int)
    IL_0019: ret
  }

  .method public hidebysig static string Legacy([opt] int32 value) cil managed
  {
    .custom instance void [System.Runtime]System.ComponentModel.EditorBrowsableAttribute::.ctor(valuetype [System.Runtime]System.ComponentModel.EditorBrowsableState) = ([System.Runtime]System.ComponentModel.EditorBrowsableState(1))
    .custom instance void [System.Runtime]System.ObsoleteAttribute::.ctor(string) = ("Use Describe", property DiagnosticId = "SAMPLE1")
    .param [1] = int32(42)
    IL_0000: ldarg.0
    IL_0001: call string AssemblyToProcess.Samples::Describe(int32)
    IL_0006: ret
  }

  .method private hidebysig specialname rtspecialname static void .cctor() cil managed
  {
    IL_0000: ldc.i4.8
    IL_0001: newarr [System.Runtime]System.Int32
    IL_0006: dup
    IL_0007: ldtoken valuetype '<PrivateImplementationDetails>'/'__StaticArrayInitTypeSize=32' '<PrivateImplementationDetails>'::'98EABD9827104D89AE70F68772FBB047D8952984C012EA68C7F209AFFB589AF1'
    IL_000c: call void [System.Runtime]System.Runtime.CompilerServices.RuntimeHelpers::InitializeArray(class [System.Runtime]System.Array, valuetype [System.Runtime]System.RuntimeFieldHandle)
    IL_0011: stsfld int32[] AssemblyToProcess.Samples::primes
    IL_0016: ret
  }

  .class nested private auto ansi sealed beforefieldinit '<>c__DisplayClass6_0'
    extends [System.Runtime]System.Object
  {
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    .field public int32 amount

    .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
    {
      IL_0000: ldarg.0
      IL_0001: call instance void [System.Runtime]System.Object::.ctor()
      IL_0006: nop
      IL_0007: ret
    }

    .method assembly hidebysig instance int32 '<Adder>b__0'(int32 _) cil managed
    {
      IL_0000: ldarg.1
      IL_0001: ldarg.0
      IL_0002: ldfld int32 AssemblyToProcess.Samples/'<>c__DisplayClass6_0'::amount
      IL_0007: add
      IL_0008: ret
    }
  }
}

.class public sequential ansi sealed beforefieldinit AssemblyToProcess.Point
  extends [System.Runtime]System.ValueType
{
  .field private initonly int32 '<X>k__BackingField'
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    .custom instance void [System.Runtime]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [System.Runtime]System.Diagnostics.DebuggerBrowsableState) = ([System.Runtime]System.Diagnostics.DebuggerBrowsableState(0))
  .field private initonly int32 '<Y>k__BackingField'
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    .custom instance void [System.Runtime]System.Diagnostics.DebuggerBrowsableAttribute::.ctor(valuetype [System.Runtime]System.Diagnostics.DebuggerBrowsableState) = ([System.Runtime]System.Diagnostics.DebuggerBrowsableState(0))

  .method public hidebysig specialname rtspecialname instance void .ctor(int32 x, int32 y) cil managed
  {
    IL_0000: ldarg.0
    IL_0001: ldarg.1
    IL_0002: stfld int32 AssemblyToProcess.Point::'<X>k__BackingField'
    IL_0007: ldarg.0
    IL_0008: ldarg.2
    IL_0009: stfld int32 AssemblyToProcess.Point::'<Y>k__BackingField'
    IL_000e: ret
  }

  .method public hidebysig specialname instance int32 get_X() cil managed
  {
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor()
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    IL_0000: ldarg.0
    IL_0001: ldfld int32 AssemblyToProcess.Point::'<X>k__BackingField'
    IL_0006: ret
  }

  .method public hidebysig specialname instance int32 get_Y() cil managed
  {
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor()
    .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
    IL_0000: ldarg.0
    IL_0001: ldfld int32 AssemblyToProcess.Point::'<Y>k__BackingField'
    IL_0006: ret
  }

  .property instance int32 X()
  {
    .get instance int32 AssemblyToProcess.Point::get_X()
  }

  .property instance int32 Y()
  {
    .get instance int32 AssemblyToProcess.Point::get_Y()
  }

  .class nested public auto ansi beforefieldinit Nested
    extends [System.Runtime]System.Object
  {

    .method public hidebysig specialname rtspecialname instance void .ctor() cil managed
    {
      IL_0000: ldarg.0
      IL_0001: call instance void [System.Runtime]System.Object::.ctor()
      IL_0006: nop
      IL_0007: ret
    }
  }
}

.class private auto ansi sealed '<PrivateImplementationDetails>'
  extends [System.Runtime]System.Object
{
  .custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilerGeneratedAttribute::.ctor()
  .field assembly static initonly valuetype '<PrivateImplementationDetails>'/'__StaticArrayInitTypeSize=32' '98EABD9827104D89AE70F68772FBB047D8952984C012EA68C7F209AFFB589AF1'
    // initial value: (02 00 00 00 03 00 00 00 05 00 00 00 07 00 00 00 0B 00 00 00 0D 00 00 00 11 00 00 00 13 00 00 00)

  .class nested assembly explicit ansi sealed '__StaticArrayInitTypeSize=32'
    extends [System.Runtime]System.ValueType
  {
    .pack 1
    .size 32
  }
}
