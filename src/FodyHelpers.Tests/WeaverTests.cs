public class WeaverTests
{
    #region WeaverUsage

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

    #endregion

    #region BrokenWeaver

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

    #endregion
}
