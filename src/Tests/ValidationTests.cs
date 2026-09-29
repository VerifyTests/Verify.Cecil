public class ValidationTests
{
    static readonly string assemblyPath = typeof(Target).Assembly.Location;

    [Test]
    public Task StackUnderflow() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, _) =>
            {
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ret);
            });

    [Test]
    public Task ExtraItemsOnReturn() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, _) =>
            {
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Ret);
            });

    [Test]
    public Task MissingReturnValue() =>
        VerifyProblems(
            _ => _.TypeSystem.Int32,
            (il, _) => il.Emit(OpCodes.Ret));

    [Test]
    public Task FallsOffEnd() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, _) => il.Emit(OpCodes.Nop));

    [Test]
    public Task EmptyBody() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (_, _) =>
            {
            });

    [Test]
    public Task BranchTargetNotInBody() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, _) =>
            {
                il.Emit(OpCodes.Br, Instruction.Create(OpCodes.Nop));
                il.Emit(OpCodes.Ret);
            });

    [Test]
    public Task StackDepthMismatch() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, _) =>
            {
                var end = il.Create(OpCodes.Ret);
                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Brtrue_S, end);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Append(end);
            });

    [Test]
    public Task ShortBranchOutOfRange() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, _) =>
            {
                var end = il.Create(OpCodes.Ret);
                il.Emit(OpCodes.Br_S, end);
                for (var i = 0; i < 200; i++)
                {
                    il.Emit(OpCodes.Nop);
                }

                il.Append(end);
            });

    [Test]
    public Task ArgumentDoesNotExist() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, _) =>
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ret);
            });

    [Test]
    public Task UnresolvableMethod() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, module) =>
            {
                var missing = new MethodReference("Missing", module.TypeSystem.Void, module.GetType("Target"));
                il.Emit(OpCodes.Call, missing);
                il.Emit(OpCodes.Ret);
            });

    [Test]
    public Task StaticFieldInstructionOnInstanceField() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, module) =>
            {
                var field = module.GetType("Target").Fields.Single(_ => _.Name == "property");
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ret);
            });

    [Test]
    public Task InstanceReferenceToStaticMethod() =>
        VerifyProblems(
            _ => _.TypeSystem.String,
            (il, module) =>
            {
                var samples = module.GetType("AssemblyToProcess.Samples");
                var describe = new MethodReference("Describe", module.TypeSystem.String, samples)
                {
                    HasThis = true,
                    Parameters =
                    {
                        new(module.TypeSystem.Int32)
                    }
                };
                il.Emit(OpCodes.Ldnull);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Call, describe);
                il.Emit(OpCodes.Ret);
            });

    [Test]
    public Task InvalidExceptionHandler() =>
        VerifyProblems(
            _ => _.TypeSystem.Void,
            (il, module) =>
            {
                var ret = il.Create(OpCodes.Ret);
                il.Emit(OpCodes.Nop);
                il.Append(ret);
                il.Body.ExceptionHandlers.Add(
                    new(ExceptionHandlerType.Catch)
                    {
                        TryStart = ret,
                        TryEnd = ret,
                        HandlerStart = Instruction.Create(OpCodes.Nop)
                    });
            });

    #region ValidationThrows

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

    #endregion

    #region DisableCecilValidation

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

    #endregion

    // Methods added in memory are snapshot with the offsets they will have when written
    [Test]
    public async Task InMemory()
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        var type = module.GetType("Target");
        var method = AddMethod(
            module,
            module.TypeSystem.String,
            il =>
            {
                var constructor = type.Methods.Single(_ => _.IsConstructor && !_.IsStatic);
                var getter = type.Properties.Single().GetMethod;
                var stringType = module.TypeSystem.String;
                var concat = new MethodReference("Concat", stringType, stringType)
                {
                    Parameters =
                    {
                        new(stringType),
                        new(stringType)
                    }
                };
                il.Emit(OpCodes.Ldstr, "Property: ");
                il.Emit(OpCodes.Newobj, constructor);
                il.Emit(OpCodes.Callvirt, getter);
                il.Emit(OpCodes.Call, concat);
                il.Emit(OpCodes.Ret);
            });

        await Verify(method);
    }

    static async Task VerifyProblems(Func<ModuleDefinition, TypeReference> returnType, Action<ILProcessor, ModuleDefinition> build)
    {
        using var module = ModuleDefinition.ReadModule(assemblyPath);
        var method = AddMethod(module, returnType(module), il => build(il, module));
        await Verify(CecilValidator.Validate(method));
    }

    static MethodDefinition AddMethod(ModuleDefinition module, TypeReference returnType, Action<ILProcessor> build)
    {
        var method = new MethodDefinition("Invalid", MethodAttributes.Public | MethodAttributes.Static, returnType);
        module.GetType("Target").Methods.Add(method);
        build(method.Body.GetILProcessor());
        return method;
    }
}
