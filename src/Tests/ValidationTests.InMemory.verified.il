.method public static string Invalid() cil managed
{
  IL_0000: ldstr "Property: "
  IL_0005: newobj instance void Target::.ctor()
  IL_000a: callvirt instance string Target::get_Property()
  IL_000f: call string [System.Runtime]System.String::Concat(string, string)
  IL_0014: ret
}
