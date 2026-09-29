using EmptyFiles;

namespace VerifyTests;

public static class VerifyCecil
{
    const string validateKey = "VerifyCecil.Validate";

    public static bool Initialized { get; private set; }

    public static void Initialize()
    {
        if (Initialized)
        {
            throw new("Already Initialized");
        }

        Initialized = true;

        FileExtensions.AddTextExtension("il");
        VerifierSettings.RegisterFileConverter<AssemblyDefinition>(
            (target, context) => Convert(context, _ => _.Assembly(target), _ => _.Assembly(target)));
        VerifierSettings.RegisterFileConverter<ModuleDefinition>(
            (target, context) => Convert(context, _ => _.Module(target), _ => _.Module(target)));
        VerifierSettings.RegisterFileConverter<TypeDefinition>(
            (target, context) => Convert(context, _ => _.Type(target), _ => _.Type(target)));
        VerifierSettings.RegisterFileConverter<MethodDefinition>(
            (target, context) => Convert(context, _ => _.Method(target), _ => _.Method(target)));
        VerifierSettings.RegisterFileConverter<FieldDefinition>(
            (target, context) => Convert(context, _ => _.Field(target), _ => { }));
        VerifierSettings.RegisterFileConverter<PropertyDefinition>(
            (target, context) => Convert(context, _ => _.Property(target), _ => _.Property(target)));
        VerifierSettings.RegisterFileConverter<EventDefinition>(
            (target, context) => Convert(context, _ => _.Event(target), _ => _.Event(target)));
    }

    /// <summary>
    /// Snapshot the IL without first checking it with <see cref="CecilValidator"/>.
    /// </summary>
    public static void DisableCecilValidation(this VerifySettings settings) =>
        settings.Context[validateKey] = false;

    /// <summary>
    /// Snapshot the IL without first checking it with <see cref="CecilValidator"/>.
    /// </summary>
    public static SettingsTask DisableCecilValidation(this SettingsTask settings)
    {
        settings.CurrentSettings.DisableCecilValidation();
        return settings;
    }

    static bool ShouldValidate(IReadOnlyDictionary<string, object> context)
    {
        if (context.TryGetValue(validateKey, out var value))
        {
            return (bool) value;
        }

        return true;
    }

    static ConversionResult Convert(IReadOnlyDictionary<string, object> context, Action<IlWriter> write, Action<Validator> validate)
    {
        if (ShouldValidate(context))
        {
            var validator = new Validator();
            validate(validator);
            if (validator.Problems.Count > 0)
            {
                throw new CecilValidationException(validator.Problems);
            }
        }

        var writer = new IlWriter();
        write(writer);
        return new(null, "il", writer.ToString());
    }
}
