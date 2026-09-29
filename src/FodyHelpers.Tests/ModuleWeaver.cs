// Adds `public static string Injected()` to Target.
// When Broken, the method also leaves an extra item on the stack.
public class ModuleWeaver :
    BaseModuleWeaver
{
    public bool Broken { get; init; }

    public override void Execute()
    {
        var type = ModuleDefinition.GetType("Target");
        var method = new MethodDefinition(
            "Injected",
            MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
            TypeSystem.StringReference);
        var il = method.Body.GetILProcessor();
        if (Broken)
        {
            il.Emit(OpCodes.Ldc_I4_1);
        }

        il.Emit(OpCodes.Ldstr, "Hello");
        il.Emit(OpCodes.Ret);
        type.Methods.Add(method);
    }

    public override IEnumerable<string> GetAssembliesForScanning() => [];
}
