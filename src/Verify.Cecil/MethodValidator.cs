class MethodValidator(MethodDefinition method, List<string> problems)
{
    MethodBody body = null!;
    Dictionary<Instruction, int> offsets = null!;
    Dictionary<Instruction, int> indexes = null!;
    int end;

    public void Run()
    {
        var type = method.DeclaringType;
        if (method.IsAbstract && !type.IsAbstract && !type.IsInterface)
        {
            Add("abstract method in a type that is not abstract");
        }

        if (!method.HasBody)
        {
            return;
        }

        body = method.Body;
        var instructions = body.Instructions;
        if (instructions.Count == 0)
        {
            Add("method body has no instructions");
            return;
        }

        offsets = Offsets.Compute(body, out end);
        indexes = new(instructions.Count);
        for (var index = 0; index < instructions.Count; index++)
        {
            indexes[instructions[index]] = index;
        }

        foreach (var instruction in instructions)
        {
            CheckOperand(instruction);
        }

        CheckHandlers();
        CheckStack();
    }

    void Add(string message) =>
        problems.Add($"{method.FullName}: {message}");

    void Add(Instruction instruction, string message) =>
        problems.Add($"{method.FullName}: {Offsets.Label(offsets[instruction])}: {instruction.OpCode.Name}: {message}");

    void WrongOperand(Instruction instruction, string expected)
    {
        var actual = instruction.Operand?.GetType().Name ?? "null";
        Add(instruction, $"operand is {actual}, expected {expected}");
    }

    void CheckOperand(Instruction instruction)
    {
        var operand = instruction.Operand;
        switch (instruction.OpCode.OperandType)
        {
            case OperandType.InlineNone:
                CheckMacro(instruction);
                break;
            case OperandType.InlineBrTarget:
                CheckTarget(instruction, operand);
                break;
            case OperandType.ShortInlineBrTarget:
                CheckShortTarget(instruction, operand);
                break;
            case OperandType.InlineSwitch:
                if (operand is Instruction[] targets)
                {
                    foreach (var target in targets)
                    {
                        CheckTarget(instruction, target);
                    }
                }
                else
                {
                    WrongOperand(instruction, "Instruction[]");
                }

                break;
            case OperandType.InlineVar:
            case OperandType.ShortInlineVar:
                CheckVariable(instruction, operand);
                break;
            case OperandType.InlineArg:
            case OperandType.ShortInlineArg:
                CheckParameter(instruction, operand);
                break;
            case OperandType.InlineMethod:
                if (operand is MethodReference methodReference)
                {
                    CheckMethod(instruction, methodReference);
                }
                else
                {
                    WrongOperand(instruction, "MethodReference");
                }

                break;
            case OperandType.InlineField:
                if (operand is FieldReference fieldReference)
                {
                    CheckField(instruction, fieldReference);
                }
                else
                {
                    WrongOperand(instruction, "FieldReference");
                }

                break;
            case OperandType.InlineType:
                if (operand is TypeReference typeReference)
                {
                    CheckType(instruction, typeReference);
                }
                else
                {
                    WrongOperand(instruction, "TypeReference");
                }

                break;
            case OperandType.InlineTok:
                switch (operand)
                {
                    case TypeReference tokenType:
                        CheckType(instruction, tokenType);
                        break;
                    case MethodReference tokenMethod:
                        CheckMethod(instruction, tokenMethod);
                        break;
                    case FieldReference tokenField:
                        CheckField(instruction, tokenField);
                        break;
                    default:
                        WrongOperand(instruction, "TypeReference, MethodReference or FieldReference");
                        break;
                }

                break;
            case OperandType.InlineString:
                CheckOperandType<string>(instruction, "String");
                break;
            case OperandType.InlineSig:
                CheckOperandType<CallSite>(instruction, "CallSite");
                break;
            case OperandType.InlineI:
                CheckOperandType<int>(instruction, "Int32");
                break;
            case OperandType.ShortInlineI:
                if (operand is not sbyte and not byte)
                {
                    WrongOperand(instruction, "SByte or Byte");
                }

                break;
            case OperandType.InlineI8:
                CheckOperandType<long>(instruction, "Int64");
                break;
            case OperandType.InlineR:
                CheckOperandType<double>(instruction, "Double");
                break;
            case OperandType.ShortInlineR:
                CheckOperandType<float>(instruction, "Single");
                break;
        }
    }

    void CheckOperandType<T>(Instruction instruction, string expected)
    {
        if (instruction.Operand is not T)
        {
            WrongOperand(instruction, expected);
        }
    }

    void CheckMacro(Instruction instruction)
    {
        var code = instruction.OpCode.Code;
        if (code is >= Code.Ldarg_0 and <= Code.Ldarg_3)
        {
            var index = code - Code.Ldarg_0;
            var count = method.Parameters.Count;
            if (method.HasThis)
            {
                count++;
            }

            if (index >= count)
            {
                Add(instruction, $"argument {index} does not exist. The method has {count} arguments");
            }

            return;
        }

        if (code is >= Code.Ldloc_0 and <= Code.Ldloc_3)
        {
            CheckMacroVariable(instruction, code - Code.Ldloc_0);
            return;
        }

        if (code is >= Code.Stloc_0 and <= Code.Stloc_3)
        {
            CheckMacroVariable(instruction, code - Code.Stloc_0);
        }
    }

    void CheckMacroVariable(Instruction instruction, int index)
    {
        var count = body.Variables.Count;
        if (index >= count)
        {
            Add(instruction, $"local {index} does not exist. The method has {count} locals");
        }
    }

    bool CheckTarget(Instruction instruction, object? operand)
    {
        if (operand is not Instruction target)
        {
            WrongOperand(instruction, "Instruction");
            return false;
        }

        if (!indexes.ContainsKey(target))
        {
            Add(instruction, $"branch target ({target.OpCode.Name}) is not in the method body");
            return false;
        }

        return true;
    }

    void CheckShortTarget(Instruction instruction, object? operand)
    {
        if (!CheckTarget(instruction, operand))
        {
            return;
        }

        var target = (Instruction) operand!;
        var next = offsets[instruction] + instruction.OpCode.Size + 1;
        var delta = offsets[target] - next;
        if (delta is < sbyte.MinValue or > sbyte.MaxValue)
        {
            Add(instruction, $"short branch to {Offsets.Label(offsets[target])} is {delta} bytes, outside the range of a short branch. Use the long form, or call OptimizeMacros");
        }
    }

    void CheckVariable(Instruction instruction, object? operand)
    {
        if (operand is not VariableDefinition variable)
        {
            WrongOperand(instruction, "VariableDefinition");
            return;
        }

        if (!body.Variables.Contains(variable))
        {
            Add(instruction, $"local V_{variable.Index} is not declared in this method");
            return;
        }

        if (instruction.OpCode.OperandType == OperandType.ShortInlineVar &&
            variable.Index > byte.MaxValue)
        {
            Add(instruction, $"local V_{variable.Index} is out of range of the short form");
        }
    }

    void CheckParameter(Instruction instruction, object? operand)
    {
        if (operand is not ParameterDefinition parameter)
        {
            WrongOperand(instruction, "ParameterDefinition");
            return;
        }

        if (method.HasThis && parameter == body.ThisParameter)
        {
            return;
        }

        if (!method.Parameters.Contains(parameter))
        {
            Add(instruction, $"parameter `{parameter.Name}` does not belong to this method");
            return;
        }

        var sequence = parameter.Index;
        if (method.HasThis)
        {
            sequence++;
        }

        if (instruction.OpCode.OperandType == OperandType.ShortInlineArg &&
            sequence > byte.MaxValue)
        {
            Add(instruction, $"parameter `{parameter.Name}` is out of range of the short form");
        }
    }

    void CheckMethod(Instruction instruction, MethodReference reference)
    {
        if (IsStaticMismatch(instruction, reference))
        {
            return;
        }

        var definition = Resolve(instruction, reference, reference.Resolve);
        if (definition == null)
        {
            return;
        }

        var code = instruction.OpCode.Code;
        if (code == Code.Newobj)
        {
            if (!definition.IsConstructor || definition.IsStatic)
            {
                Add(instruction, $"`{reference.FullName}` is not an instance constructor");
            }

            return;
        }

        if (code == Code.Callvirt && definition.IsStatic)
        {
            Add(instruction, $"`{reference.FullName}` is static and cannot be called with callvirt");
        }
    }

    // Cecil does not resolve a reference whose HasThis differs from the definition.
    // Find that case first, since it is a more useful message than "cannot resolve".
    bool IsStaticMismatch(Instruction instruction, MethodReference reference)
    {
        if (reference is MethodSpecification ||
            !CanResolve(reference))
        {
            return false;
        }

        TypeDefinition? type;
        try
        {
            type = reference.DeclaringType.Resolve();
        }
        catch
        {
            return false;
        }

        if (type == null)
        {
            return false;
        }

        var parameterCount = reference.Parameters.Count;
        var matches = type.Methods
            .Where(_ => _.Name == reference.Name &&
                        _.Parameters.Count == parameterCount)
            .ToList();
        if (matches.Count == 0 ||
            matches.Any(_ => _.HasThis == reference.HasThis))
        {
            return false;
        }

        Add(instruction, $"`{reference.FullName}` is referenced as {Kind(reference.HasThis)} but the method is {Kind(!reference.HasThis)}");
        return true;
    }

    static string Kind(bool hasThis)
    {
        if (hasThis)
        {
            return "instance";
        }

        return "static";
    }

    void CheckField(Instruction instruction, FieldReference reference)
    {
        var definition = Resolve(instruction, reference, reference.Resolve);
        if (definition == null)
        {
            return;
        }

        var code = instruction.OpCode.Code;
        if (code is Code.Ldfld or Code.Ldflda or Code.Stfld &&
            definition.IsStatic)
        {
            Add(instruction, $"`{reference.FullName}` is static. Use the static form of the instruction");
            return;
        }

        if (code is Code.Ldsfld or Code.Ldsflda or Code.Stsfld &&
            !definition.IsStatic)
        {
            Add(instruction, $"`{reference.FullName}` is an instance field. Use the instance form of the instruction");
        }
    }

    void CheckType(Instruction instruction, TypeReference reference) =>
        Resolve(instruction, reference, reference.Resolve);

    T? Resolve<T>(Instruction instruction, MemberReference reference, Func<T?> resolve)
        where T : class
    {
        if (!CanResolve(reference))
        {
            return null;
        }

        T? result;
        try
        {
            result = resolve();
        }
        catch
        {
            // The assembly that should contain the reference could not be found or read.
            // That is a problem with the environment running the test, not with the IL.
            return null;
        }

        if (result == null)
        {
            Add(instruction, $"cannot resolve `{reference.FullName}`");
        }

        return result;
    }

    static bool CanResolve(MemberReference reference)
    {
        switch (reference)
        {
            case TypeReference type:
                return CanResolve(type);
            case MethodReference method:
                if (method.CallingConvention == MethodCallingConvention.VarArg)
                {
                    return false;
                }

                return CanResolve(method.DeclaringType);
            case FieldReference field:
                return CanResolve(field.DeclaringType);
            default:
                return false;
        }
    }

    // Array methods (Get, Set, Address, .ctor) are provided by the runtime,
    // and generic parameters and function pointers have no definition to resolve to.
    static bool CanResolve(TypeReference type)
    {
        if (type is ArrayType)
        {
            return false;
        }

        var element = type.GetElementType();
        return element is not GenericParameter and not FunctionPointerType;
    }

    void CheckHandlers()
    {
        if (!body.HasExceptionHandlers)
        {
            return;
        }

        for (var index = 0; index < body.ExceptionHandlers.Count; index++)
        {
            var handler = body.ExceptionHandlers[index];
            var name = $"exception handler {index} ({handler.HandlerType})";
            var valid = CheckBoundary(name, "TryStart", handler.TryStart, false) &
                        CheckBoundary(name, "TryEnd", handler.TryEnd, true) &
                        CheckBoundary(name, "HandlerStart", handler.HandlerStart, false) &
                        CheckBoundary(name, "HandlerEnd", handler.HandlerEnd, true);

            if (handler.HandlerType == ExceptionHandlerType.Filter)
            {
                valid &= CheckBoundary(name, "FilterStart", handler.FilterStart, false);
            }

            if (handler.HandlerType == ExceptionHandlerType.Catch &&
                handler.CatchType == null)
            {
                Add($"{name}: catch handler has no CatchType");
            }

            if (!valid)
            {
                continue;
            }

            var tryStart = Offset(handler.TryStart);
            var tryEnd = Offset(handler.TryEnd);
            var handlerStart = Offset(handler.HandlerStart);
            var handlerEnd = Offset(handler.HandlerEnd);
            if (tryStart >= tryEnd)
            {
                Add($"{name}: try block is empty or ends before it starts");
            }

            if (handlerStart >= handlerEnd)
            {
                Add($"{name}: handler block is empty or ends before it starts");
            }

            if (tryStart < handlerEnd && handlerStart < tryEnd)
            {
                Add($"{name}: try block and handler block overlap");
            }
        }
    }

    bool CheckBoundary(string handler, string name, Instruction? instruction, bool allowEnd)
    {
        if (instruction == null)
        {
            if (allowEnd)
            {
                return true;
            }

            Add($"{handler}: {name} is null");
            return false;
        }

        if (indexes.ContainsKey(instruction))
        {
            return true;
        }

        Add($"{handler}: {name} ({instruction.OpCode.Name}) is not in the method body");
        return false;
    }

    int Offset(Instruction? instruction)
    {
        if (instruction == null)
        {
            return end;
        }

        return offsets[instruction];
    }

    void CheckStack()
    {
        var instructions = body.Instructions;
        var depths = new Dictionary<Instruction, int>();
        var tryStarts = new HashSet<Instruction>();
        var pending = new Stack<(Instruction instruction, int depth)>();
        pending.Push((instructions[0], 0));

        foreach (var handler in body.ExceptionHandlers)
        {
            if (handler.TryStart != null)
            {
                tryStarts.Add(handler.TryStart);
            }

            var type = handler.HandlerType;
            if (handler.HandlerStart != null &&
                indexes.ContainsKey(handler.HandlerStart))
            {
                // catch and filter handlers start with the exception on the stack
                var depth = 0;
                if (type is ExceptionHandlerType.Catch or ExceptionHandlerType.Filter)
                {
                    depth = 1;
                }

                pending.Push((handler.HandlerStart, depth));
            }

            if (type == ExceptionHandlerType.Filter &&
                handler.FilterStart != null &&
                indexes.ContainsKey(handler.FilterStart))
            {
                pending.Push((handler.FilterStart, 1));
            }
        }

        var reported = new HashSet<Instruction>();
        while (pending.Count > 0)
        {
            var (instruction, depth) = pending.Pop();
            Walk(instruction, depth, instructions, depths, tryStarts, reported, pending);
        }
    }

    void Walk(
        Instruction instruction,
        int depth,
        Collection<Instruction> instructions,
        Dictionary<Instruction, int> depths,
        HashSet<Instruction> tryStarts,
        HashSet<Instruction> reported,
        Stack<(Instruction instruction, int depth)> pending)
    {
        while (true)
        {
            if (depths.TryGetValue(instruction, out var existing))
            {
                if (existing != depth &&
                    reported.Add(instruction))
                {
                    Add(instruction, $"stack depth is {existing} on one path and {depth} on another");
                }

                return;
            }

            depths[instruction] = depth;

            if (depth != 0 &&
                tryStarts.Contains(instruction))
            {
                Add(instruction, $"stack must be empty on entry to a try block, but has {depth}");
            }

            if (!TryGetStackEffect(instruction, depth, out var pop, out var push))
            {
                return;
            }

            if (pop > depth)
            {
                Add(instruction, $"stack underflow. Pops {pop} but the stack has {depth}");
                return;
            }

            depth = depth - pop + push;

            var opCode = instruction.OpCode;
            if (opCode.Code == Code.Ret &&
                depth != 0)
            {
                Add(instruction, $"stack must be empty after ret, but has {depth} left");
            }

            switch (opCode.FlowControl)
            {
                case FlowControl.Branch:
                    if (instruction.Operand is Instruction target &&
                        indexes.ContainsKey(target))
                    {
                        instruction = target;
                        continue;
                    }

                    return;
                case FlowControl.Cond_Branch:
                    switch (instruction.Operand)
                    {
                        case Instruction conditionalTarget when indexes.ContainsKey(conditionalTarget):
                            pending.Push((conditionalTarget, depth));
                            break;
                        case Instruction[] targets:
                            foreach (var switchTarget in targets)
                            {
                                if (switchTarget != null &&
                                    indexes.ContainsKey(switchTarget))
                                {
                                    pending.Push((switchTarget, depth));
                                }
                            }

                            break;
                    }

                    break;
                case FlowControl.Return:
                case FlowControl.Throw:
                    return;
            }

            if (opCode.Code == Code.Jmp)
            {
                return;
            }

            var next = indexes[instruction] + 1;
            if (next == instructions.Count)
            {
                Add(instruction, "control falls through the end of the method body");
                return;
            }

            instruction = instructions[next];
        }
    }

    bool TryGetStackEffect(Instruction instruction, int depth, out int pop, out int push)
    {
        var opCode = instruction.OpCode;
        push = 0;
        pop = 0;
        switch (opCode.StackBehaviourPop)
        {
            case StackBehaviour.Pop0:
                break;
            case StackBehaviour.Pop1:
            case StackBehaviour.Popi:
            case StackBehaviour.Popref:
                pop = 1;
                break;
            case StackBehaviour.Pop1_pop1:
            case StackBehaviour.Popi_pop1:
            case StackBehaviour.Popi_popi:
            case StackBehaviour.Popi_popi8:
            case StackBehaviour.Popi_popr4:
            case StackBehaviour.Popi_popr8:
            case StackBehaviour.Popref_pop1:
            case StackBehaviour.Popref_popi:
                pop = 2;
                break;
            case StackBehaviour.Popi_popi_popi:
            case StackBehaviour.Popref_popi_popi:
            case StackBehaviour.Popref_popi_popi8:
            case StackBehaviour.Popref_popi_popr4:
            case StackBehaviour.Popref_popi_popr8:
            case StackBehaviour.Popref_popi_popref:
                pop = 3;
                break;
            case StackBehaviour.PopAll:
                pop = depth;
                break;
            case StackBehaviour.Varpop:
                if (!TryGetVarPop(instruction, out pop))
                {
                    return false;
                }

                break;
        }

        switch (opCode.StackBehaviourPush)
        {
            case StackBehaviour.Push0:
                break;
            case StackBehaviour.Push1_push1:
                push = 2;
                break;
            case StackBehaviour.Varpush:
                if (!TryGetVarPush(instruction, out push))
                {
                    return false;
                }

                break;
            default:
                push = 1;
                break;
        }

        return true;
    }

    bool TryGetVarPop(Instruction instruction, out int pop)
    {
        pop = 0;
        switch (instruction.OpCode.Code)
        {
            case Code.Ret:
                if (!IsVoid(method.ReturnType))
                {
                    pop = 1;
                }

                return true;
            case Code.Newobj:
                if (instruction.Operand is not MethodReference constructor)
                {
                    return false;
                }

                pop = constructor.Parameters.Count;
                return true;
            case Code.Call:
            case Code.Callvirt:
                if (instruction.Operand is not IMethodSignature signature)
                {
                    return false;
                }

                pop = SignaturePop(signature);
                return true;
            case Code.Calli:
                if (instruction.Operand is not CallSite callSite)
                {
                    return false;
                }

                // the function pointer
                pop = SignaturePop(callSite) + 1;
                return true;
            default:
                return true;
        }
    }

    static int SignaturePop(IMethodSignature signature)
    {
        var pop = signature.Parameters.Count;
        if (signature.HasThis && !signature.ExplicitThis)
        {
            pop++;
        }

        return pop;
    }

    static bool TryGetVarPush(Instruction instruction, out int push)
    {
        push = 0;
        switch (instruction.Operand)
        {
            case MethodReference when instruction.OpCode.Code == Code.Newobj:
                push = 1;
                return true;
            case IMethodSignature signature:
                if (!IsVoid(signature.ReturnType))
                {
                    push = 1;
                }

                return true;
            default:
                return false;
        }
    }

    static bool IsVoid(TypeReference type)
    {
        while (type is IModifierType modifier)
        {
            type = modifier.ElementType;
        }

        return type.MetadataType == MetadataType.Void;
    }
}
