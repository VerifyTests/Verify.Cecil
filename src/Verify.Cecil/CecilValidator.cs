namespace VerifyTests.Cecil;

/// <summary>
/// Structural checks of IL, covering the class of problems that PEVerify reports for woven assemblies:
/// branch targets, exception handler ranges, stack balance, operands, and references that do not resolve.
/// </summary>
public static class CecilValidator
{
    public static IReadOnlyList<string> Validate(AssemblyDefinition assembly) =>
        Run(_ => _.Assembly(assembly));

    public static IReadOnlyList<string> Validate(ModuleDefinition module) =>
        Run(_ => _.Module(module));

    public static IReadOnlyList<string> Validate(TypeDefinition type) =>
        Run(_ => _.Type(type));

    public static IReadOnlyList<string> Validate(MethodDefinition method) =>
        Run(_ => _.Method(method));

    public static void ThrowIfInvalid(ModuleDefinition module) =>
        Throw(Validate(module));

    public static void ThrowIfInvalid(TypeDefinition type) =>
        Throw(Validate(type));

    public static void ThrowIfInvalid(MethodDefinition method) =>
        Throw(Validate(method));

    static void Throw(IReadOnlyList<string> problems)
    {
        if (problems.Count > 0)
        {
            throw new CecilValidationException(problems);
        }
    }

    static IReadOnlyList<string> Run(Action<Validator> action)
    {
        var validator = new Validator();
        action(validator);
        return validator.Problems;
    }
}
