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
