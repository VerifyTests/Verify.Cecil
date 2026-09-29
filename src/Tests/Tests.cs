namespace Tests;

public class Tests
{
    static readonly string assemblyPath = typeof(Target).Assembly.Location;

    #region TypeUsage

    [Test]
    public async Task TypeUsage()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        await Verify(module.GetType("Target"));
    }

    #endregion

    #region MethodUsage

    [Test]
    public async Task MethodUsage()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        var type = module.GetType("Target");
        await Verify(type.Methods.Single(_ => _.Name == "OnPropertyChanged"));
    }

    #endregion

    #region PropertyUsage

    [Test]
    public async Task PropertyUsage()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        var type = module.GetType("Target");
        await Verify(type.Properties.Single(_ => _.Name == "Property"));
    }

    #endregion

    [Test]
    public async Task FieldUsage()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        var type = module.GetType("Target");
        await Verify(type.Fields.Single(_ => _.Name == "property"));
    }

    [Test]
    public async Task EventUsage()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        var type = module.GetType("Target");
        await Verify(type.Events.Single());
    }

    #region ModuleUsage

    [Test]
    public async Task ModuleUsage()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        await Verify(module);
    }

    #endregion

    [Test]
    public async Task AssemblyUsage()
    {
        using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
        await Verify(assembly);
    }

    [Test]
    public async Task Samples()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        await Verify(module.GetType("AssemblyToProcess.Samples"));
    }

    [Test]
    public async Task Struct()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        await Verify(module.GetType("AssemblyToProcess.Point"));
    }

    // Validation should find nothing wrong in assemblies produced by the compiler
    [Test]
    [Arguments(typeof(Target))]
    [Arguments(typeof(ModuleDefinition))]
    [Arguments(typeof(VerifySettings))]
    [Arguments(typeof(Argon.JsonConvert))]
    public async Task NoFalsePositives(Type type)
    {
        using var module = ModuleDefinition.ReadModule(type.Assembly.Location);
        var problems = CecilValidator.Validate(module);
        await Assert.That(problems).IsEmpty();
    }
}
