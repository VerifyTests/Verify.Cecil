public static class ModuleInit
{
    [ModuleInitializer]
    public static void Init() =>
        VerifyCecil.Initialize();

    [ModuleInitializer]
    public static void InitOther() =>
        VerifierSettings.InitializePlugins();
}
