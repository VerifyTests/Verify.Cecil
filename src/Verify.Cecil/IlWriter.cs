// Writes Cecil structures as ILAsm-like text.
// The output is deterministic: no RVAs, metadata tokens, MVIDs or source revisions,
// and instruction offsets are computed from the instructions rather than read from the file.
class IlWriter
{
    StringBuilder builder = new();
    int indent;

    public override string ToString() =>
        builder.ToString();

    void Line(string text)
    {
        builder.Append(' ', indent * 2);
        builder.Append(text);
        builder.Append('\n');
    }

    void BlankLine() =>
        builder.Append('\n');

    void Open()
    {
        Line("{");
        indent++;
    }

    void Close()
    {
        indent--;
        Line("}");
    }

    public void Assembly(AssemblyDefinition assembly)
    {
        AssemblyHeader(assembly);
        foreach (var module in assembly.Modules)
        {
            BlankLine();
            ModuleContents(module);
        }
    }

    public void Module(ModuleDefinition module)
    {
        var assembly = module.Assembly;
        if (assembly != null &&
            module == assembly.MainModule)
        {
            AssemblyHeader(assembly);
            BlankLine();
        }

        ModuleContents(module);
    }

    void AssemblyHeader(AssemblyDefinition assembly)
    {
        foreach (var reference in assembly.MainModule.AssemblyReferences.OrderBy(_ => _.Name, StringComparer.Ordinal))
        {
            Line($".assembly extern {Names.Identifier(reference.Name)} {Version(reference.Version)}");
        }

        BlankLine();
        var name = assembly.Name;
        Line($".assembly {Names.Identifier(name.Name)}");
        Open();
        CustomAttributes(assembly);
        var token = name.PublicKeyToken;
        if (token is {Length: > 0})
        {
            Line($".publickeytoken = ({Names.Hex(token)})");
        }

        Line($".ver {Version(name.Version)}");
        Close();
    }

    static string Version(Version version) =>
        $"{version.Major}:{version.Minor}:{version.Build}:{version.Revision}";

    void ModuleContents(ModuleDefinition module)
    {
        Line($".module {Names.Identifier(module.Name)}");
        CustomAttributes(module);

        foreach (var type in module.Types)
        {
            if (IsEmptyModuleType(type))
            {
                continue;
            }

            BlankLine();
            Type(type);
        }
    }

    // <Module> always exists. Only show it when something has been added to it, eg a module initializer.
    static bool IsEmptyModuleType(TypeDefinition type) =>
        type.Name == "<Module>" &&
        type.Namespace.Length == 0 &&
        type is
        {
            HasMethods: false,
            HasFields: false,
            HasProperties: false,
            HasEvents: false,
            HasNestedTypes: false,
            HasCustomAttributes: false
        };

    public void Type(TypeDefinition type)
    {
        var name = Names.Identifier(type.Name);
        if (type.DeclaringType == null)
        {
            name = Names.TypeName(type);
        }

        Line($".class {TypeFlags(type)}{name}{GenericParameters(type)}");
        indent++;
        if (type.BaseType != null)
        {
            Line($"extends {Names.Token(type.BaseType)}");
        }

        foreach (var implementation in type.Interfaces)
        {
            Line($"implements {Names.Token(implementation.InterfaceType)}");
        }

        indent--;
        Open();
        CustomAttributes(type);
        if (type.HasLayoutInfo)
        {
            if (type.PackingSize >= 0)
            {
                Line($".pack {type.PackingSize}");
            }

            if (type.ClassSize >= 0)
            {
                Line($".size {type.ClassSize}");
            }
        }

        foreach (var implementation in type.Interfaces.Where(_ => _.HasCustomAttributes))
        {
            Line($".interfaceimpl type {Names.Token(implementation.InterfaceType)}");
            indent++;
            CustomAttributes(implementation);
            indent--;
        }

        GenericParameterAttributes(type);

        foreach (var field in type.Fields)
        {
            Field(field);
        }

        foreach (var method in type.Methods)
        {
            BlankLine();
            Method(method);
        }

        foreach (var property in type.Properties)
        {
            BlankLine();
            Property(property);
        }

        foreach (var @event in type.Events)
        {
            BlankLine();
            Event(@event);
        }

        foreach (var nested in type.NestedTypes)
        {
            BlankLine();
            Type(nested);
        }

        Close();
    }

    static string TypeFlags(TypeDefinition type)
    {
        var builder = new StringBuilder();
        var attributes = type.Attributes;
        if (type.IsInterface)
        {
            builder.Append("interface ");
        }

        builder.Append(
            (attributes & TypeAttributes.VisibilityMask) switch
            {
                TypeAttributes.Public => "public ",
                TypeAttributes.NestedPublic => "nested public ",
                TypeAttributes.NestedPrivate => "nested private ",
                TypeAttributes.NestedFamily => "nested family ",
                TypeAttributes.NestedAssembly => "nested assembly ",
                TypeAttributes.NestedFamANDAssem => "nested famandassem ",
                TypeAttributes.NestedFamORAssem => "nested famorassem ",
                _ => "private "
            });
        Append(builder, type.IsAbstract, "abstract");
        builder.Append(
            (attributes & TypeAttributes.LayoutMask) switch
            {
                TypeAttributes.SequentialLayout => "sequential ",
                TypeAttributes.ExplicitLayout => "explicit ",
                _ => "auto "
            });
        builder.Append(
            (attributes & TypeAttributes.StringFormatMask) switch
            {
                TypeAttributes.UnicodeClass => "unicode ",
                TypeAttributes.AutoClass => "autochar ",
                _ => "ansi "
            });
        Append(builder, type.IsSealed, "sealed");
        Append(builder, type.IsImport, "import");
        Append(builder, type.IsSerializable, "serializable");
        Append(builder, type.IsWindowsRuntime, "windowsruntime");
        Append(builder, type.IsSpecialName, "specialname");
        Append(builder, type.IsRuntimeSpecialName, "rtspecialname");
        Append(builder, type.IsBeforeFieldInit, "beforefieldinit");
        return builder.ToString();
    }

    static void Append(StringBuilder builder, bool condition, string flag)
    {
        if (condition)
        {
            builder.Append(flag);
            builder.Append(' ');
        }
    }

    static string GenericParameters(IGenericParameterProvider provider)
    {
        if (!provider.HasGenericParameters)
        {
            return "";
        }

        var parameters = provider.GenericParameters.Select(GenericParameter);
        return $"<{string.Join(", ", parameters)}>";
    }

    static string GenericParameter(GenericParameter parameter)
    {
        var builder = new StringBuilder();
        Append(builder, parameter.IsCovariant, "+");
        Append(builder, parameter.IsContravariant, "-");
        Append(builder, parameter.HasReferenceTypeConstraint, "class");
        Append(builder, parameter.HasNotNullableValueTypeConstraint, "valuetype");
        Append(builder, parameter.HasDefaultConstructorConstraint, ".ctor");
        if (parameter.HasConstraints)
        {
            var constraints = parameter.Constraints.Select(_ => Names.Signature(_.ConstraintType));
            builder.Append($"({string.Join(", ", constraints)}) ");
        }

        builder.Append(Names.Identifier(parameter.Name));
        return builder.ToString();
    }

    void GenericParameterAttributes(IGenericParameterProvider provider)
    {
        foreach (var parameter in provider.GenericParameters)
        {
            var constraints = parameter.Constraints.Where(_ => _.HasCustomAttributes).ToList();
            if (!parameter.HasCustomAttributes &&
                constraints.Count == 0)
            {
                continue;
            }

            Line($".param type {Names.Identifier(parameter.Name)}");
            indent++;
            CustomAttributes(parameter);
            foreach (var constraint in constraints)
            {
                Line($".param constraint {Names.Identifier(parameter.Name)}, {Names.Signature(constraint.ConstraintType)}");
                indent++;
                CustomAttributes(constraint);
                indent--;
            }

            indent--;
        }
    }

    public void Field(FieldDefinition field)
    {
        var builder = new StringBuilder(".field ");
        if (field.HasLayoutInfo)
        {
            builder.Append($"[{field.Offset}] ");
        }

        builder.Append(
            (field.Attributes & FieldAttributes.FieldAccessMask) switch
            {
                FieldAttributes.Private => "private ",
                FieldAttributes.FamANDAssem => "famandassem ",
                FieldAttributes.Assembly => "assembly ",
                FieldAttributes.Family => "family ",
                FieldAttributes.FamORAssem => "famorassem ",
                FieldAttributes.Public => "public ",
                _ => "privatescope "
            });
        Append(builder, field.IsStatic, "static");
        Append(builder, field.IsInitOnly, "initonly");
        Append(builder, field.IsLiteral, "literal");
        Append(builder, field.IsNotSerialized, "notserialized");
        Append(builder, field.IsSpecialName, "specialname");
        Append(builder, field.IsRuntimeSpecialName, "rtspecialname");
        Append(builder, field.IsPInvokeImpl, "pinvokeimpl");
        if (field.HasMarshalInfo)
        {
            builder.Append($"marshal({Marshal(field.MarshalInfo)}) ");
        }

        builder.Append($"{Names.Signature(field.FieldType)} {Names.Identifier(field.Name)}");
        if (field.HasConstant)
        {
            builder.Append($" = {Constant(field.Constant)}");
        }

        Line(builder.ToString());
        indent++;
        if (field.InitialValue is {Length: > 0} initialValue)
        {
            Line($"// initial value: ({Names.Hex(initialValue)})");
        }

        CustomAttributes(field);
        indent--;
    }

    static string Marshal(MarshalInfo info) =>
        info.NativeType.ToString().ToLowerInvariant();

    static string Constant(object? value)
    {
        switch (value)
        {
            case null:
                return "nullref";
            case string text:
                return Names.Quote(text);
            case bool boolean:
                return $"bool({boolean.ToString().ToLowerInvariant()})";
            case char ch:
                return $"char(0x{(int) ch:X4})";
            case sbyte:
                return $"int8({Names.Number(value)})";
            case byte:
                return $"uint8({Names.Number(value)})";
            case short:
                return $"int16({Names.Number(value)})";
            case ushort:
                return $"uint16({Names.Number(value)})";
            case int:
                return $"int32({Names.Number(value)})";
            case uint:
                return $"uint32({Names.Number(value)})";
            case long:
                return $"int64({Names.Number(value)})";
            case ulong:
                return $"uint64({Names.Number(value)})";
            case float:
                return $"float32({Names.Number(value)})";
            case double:
                return $"float64({Names.Number(value)})";
            default:
                return Names.Number(value);
        }
    }

    public void Method(MethodDefinition method)
    {
        Line($".method {MethodFlags(method)}{Names.CallingConvention(method)}{ReturnType(method.MethodReturnType)} {Names.Identifier(method.Name)}{GenericParameters(method)}({MethodParameters(method)}) {ImplementationFlags(method)}");
        Open();

        if (method.Module?.EntryPoint == method)
        {
            Line(".entrypoint");
        }

        foreach (var overridden in method.Overrides)
        {
            Line($".override method {Names.Method(overridden)}");
        }

        CustomAttributes(method);
        ParameterAttributes(method);
        GenericParameterAttributes(method);

        if (method.HasBody)
        {
            Body(method.Body);
        }

        Close();
    }

    static string ReturnType(MethodReturnType returnType)
    {
        var type = Names.Signature(returnType.ReturnType);
        if (returnType.HasMarshalInfo)
        {
            return $"{type} marshal({Marshal(returnType.MarshalInfo)})";
        }

        return type;
    }

    static string MethodFlags(MethodDefinition method)
    {
        var builder = new StringBuilder();
        builder.Append(
            (method.Attributes & MethodAttributes.MemberAccessMask) switch
            {
                MethodAttributes.Private => "private ",
                MethodAttributes.FamANDAssem => "famandassem ",
                MethodAttributes.Assembly => "assembly ",
                MethodAttributes.Family => "family ",
                MethodAttributes.FamORAssem => "famorassem ",
                MethodAttributes.Public => "public ",
                _ => "privatescope "
            });
        Append(builder, method.IsHideBySig, "hidebysig");
        Append(builder, method.IsNewSlot, "newslot");
        Append(builder, method.IsSpecialName, "specialname");
        Append(builder, method.IsRuntimeSpecialName, "rtspecialname");
        Append(builder, method.IsAbstract, "abstract");
        Append(builder, method.IsVirtual, "virtual");
        Append(builder, method.IsFinal, "final");
        Append(builder, method.IsCheckAccessOnOverride, "strict");
        Append(builder, method.IsStatic, "static");
        Append(builder, method.IsUnmanagedExport, "unmanagedexp");
        Append(builder, (method.Attributes & MethodAttributes.RequireSecObject) != 0, "reqsecobj");
        if (method.IsPInvokeImpl)
        {
            var info = method.PInvokeInfo;
            if (info == null)
            {
                builder.Append("pinvokeimpl() ");
            }
            else
            {
                builder.Append($"pinvokeimpl({Names.Quote(info.Module.Name)} as {Names.Quote(info.EntryPoint)}) ");
            }
        }

        return builder.ToString();
    }

    static string ImplementationFlags(MethodDefinition method)
    {
        var builder = new StringBuilder();
        builder.Append(
            (method.ImplAttributes & MethodImplAttributes.CodeTypeMask) switch
            {
                MethodImplAttributes.Native => "native ",
                MethodImplAttributes.OPTIL => "optil ",
                MethodImplAttributes.Runtime => "runtime ",
                _ => "cil "
            });
        if (method.IsUnmanaged)
        {
            builder.Append("unmanaged");
        }
        else
        {
            builder.Append("managed");
        }

        var attributes = method.ImplAttributes;
        AppendFlag(builder, attributes, MethodImplAttributes.ForwardRef, "forwardref");
        AppendFlag(builder, attributes, MethodImplAttributes.PreserveSig, "preservesig");
        AppendFlag(builder, attributes, MethodImplAttributes.InternalCall, "internalcall");
        AppendFlag(builder, attributes, MethodImplAttributes.Synchronized, "synchronized");
        AppendFlag(builder, attributes, MethodImplAttributes.NoInlining, "noinlining");
        AppendFlag(builder, attributes, MethodImplAttributes.AggressiveInlining, "aggressiveinlining");
        AppendFlag(builder, attributes, MethodImplAttributes.NoOptimization, "nooptimization");
        AppendFlag(builder, attributes, MethodImplAttributes.AggressiveOptimization, "aggressiveoptimization");
        return builder.ToString();
    }

    static void AppendFlag(StringBuilder builder, MethodImplAttributes attributes, MethodImplAttributes flag, string name)
    {
        if ((attributes & flag) == flag)
        {
            builder.Append(' ');
            builder.Append(name);
        }
    }

    static string MethodParameters(MethodDefinition method) =>
        string.Join(", ", method.Parameters.Select(Parameter));

    static string Parameter(ParameterDefinition parameter)
    {
        var builder = new StringBuilder();
        Append(builder, parameter.IsIn, "[in]");
        Append(builder, parameter.IsOut, "[out]");
        Append(builder, parameter.IsOptional, "[opt]");
        builder.Append(Names.Signature(parameter.ParameterType));
        if (parameter.HasMarshalInfo)
        {
            builder.Append($" marshal({Marshal(parameter.MarshalInfo)})");
        }

        if (!string.IsNullOrEmpty(parameter.Name))
        {
            builder.Append(' ');
            builder.Append(Names.Identifier(parameter.Name));
        }

        return builder.ToString();
    }

    void ParameterAttributes(MethodDefinition method)
    {
        var returnType = method.MethodReturnType;
        if (returnType.HasCustomAttributes)
        {
            Line(".param [0]");
            indent++;
            CustomAttributes(returnType);
            indent--;
        }

        foreach (var parameter in method.Parameters)
        {
            if (!parameter.HasConstant &&
                !parameter.HasCustomAttributes)
            {
                continue;
            }

            var line = $".param [{parameter.Index + 1}]";
            if (parameter.HasConstant)
            {
                line += $" = {Constant(parameter.Constant)}";
            }

            Line(line);
            indent++;
            CustomAttributes(parameter);
            indent--;
        }
    }

    void Body(MethodBody body)
    {
        var offsets = Offsets.Compute(body, out var end);

        // .maxstack is not written. Cecil recomputes it whenever a module is written.
        if (body.HasVariables)
        {
            if (body.InitLocals)
            {
                Line(".locals init (");
            }
            else
            {
                Line(".locals (");
            }

            indent++;
            var variables = body.Variables;
            for (var index = 0; index < variables.Count; index++)
            {
                var suffix = "";
                if (index < variables.Count - 1)
                {
                    suffix = ",";
                }

                Line($"[{index}] {Names.Signature(variables[index].VariableType)}{suffix}");
            }

            indent--;
            Line(")");
            BlankLine();
        }

        foreach (var instruction in body.Instructions)
        {
            var operand = Operand(body, instruction, offsets, end);
            var label = Offsets.Label(offsets[instruction]);
            if (operand.Length == 0)
            {
                Line($"{label}: {instruction.OpCode.Name}");
            }
            else
            {
                Line($"{label}: {instruction.OpCode.Name} {operand}");
            }
        }

        if (!body.HasExceptionHandlers)
        {
            return;
        }

        BlankLine();
        foreach (var handler in body.ExceptionHandlers)
        {
            Line(Handler(handler, offsets, end));
        }
    }

    static string Handler(ExceptionHandler handler, Dictionary<Instruction, int> offsets, int end)
    {
        var tryRange = $"{Label(handler.TryStart, offsets, end)} to {Label(handler.TryEnd, offsets, end)}";
        var handlerRange = $"{Label(handler.HandlerStart, offsets, end)} to {Label(handler.HandlerEnd, offsets, end)}";
        switch (handler.HandlerType)
        {
            case ExceptionHandlerType.Catch:
                var catchType = "";
                if (handler.CatchType != null)
                {
                    catchType = Names.Token(handler.CatchType) + " ";
                }

                return $".try {tryRange} catch {catchType}handler {handlerRange}";
            case ExceptionHandlerType.Filter:
                return $".try {tryRange} filter {Label(handler.FilterStart, offsets, end)} handler {handlerRange}";
            case ExceptionHandlerType.Finally:
                return $".try {tryRange} finally handler {handlerRange}";
            default:
                return $".try {tryRange} fault handler {handlerRange}";
        }
    }

    static string Label(Instruction? instruction, Dictionary<Instruction, int> offsets, int end)
    {
        if (instruction == null)
        {
            return Offsets.Label(end);
        }

        if (offsets.TryGetValue(instruction, out var offset))
        {
            return Offsets.Label(offset);
        }

        return $"<not in body: {instruction}>";
    }

    static string Operand(MethodBody body, Instruction instruction, Dictionary<Instruction, int> offsets, int end)
    {
        switch (instruction.Operand)
        {
            case null:
                return "";
            case Instruction target:
                return Label(target, offsets, end);
            case Instruction[] targets:
                return $"({string.Join(", ", targets.Select(_ => Label(_, offsets, end)))})";
            case string text:
                return Names.Quote(text);
            case VariableDefinition variable:
                return $"V_{variable.Index}";
            case ParameterDefinition parameter:
                return ParameterOperand(body, parameter);
            case TypeReference type:
                return Names.Token(type);
            case MethodReference method:
                return Names.Method(method);
            case FieldReference field:
                return Names.Field(field);
            case CallSite callSite:
                return $"{Names.CallingConvention(callSite)}{Names.Signature(callSite.ReturnType)}({Names.Parameters(callSite)})";
            default:
                return Names.Number(instruction.Operand);
        }
    }

    static string ParameterOperand(MethodBody body, ParameterDefinition parameter)
    {
        if (parameter == body.ThisParameter)
        {
            return "this";
        }

        if (string.IsNullOrEmpty(parameter.Name))
        {
            var sequence = parameter.Index;
            if (body.Method.HasThis)
            {
                sequence++;
            }

            return $"A_{sequence}";
        }

        return Names.Identifier(parameter.Name);
    }

    public void Property(PropertyDefinition property)
    {
        var builder = new StringBuilder(".property ");
        Append(builder, property.IsSpecialName, "specialname");
        Append(builder, property.IsRuntimeSpecialName, "rtspecialname");
        Append(builder, property.HasThis, "instance");
        var parameters = string.Join(", ", property.Parameters.Select(_ => Names.Signature(_.ParameterType)));
        builder.Append($"{Names.Signature(property.PropertyType)} {Names.Identifier(property.Name)}({parameters})");
        if (property.HasConstant)
        {
            builder.Append($" = {Constant(property.Constant)}");
        }

        Line(builder.ToString());
        Open();
        CustomAttributes(property);
        Accessor(".get", property.GetMethod);
        Accessor(".set", property.SetMethod);
        foreach (var other in property.OtherMethods)
        {
            Accessor(".other", other);
        }

        Close();
    }

    public void Event(EventDefinition @event)
    {
        var builder = new StringBuilder(".event ");
        Append(builder, @event.IsSpecialName, "specialname");
        Append(builder, @event.IsRuntimeSpecialName, "rtspecialname");
        builder.Append($"{Names.Token(@event.EventType)} {Names.Identifier(@event.Name)}");
        Line(builder.ToString());
        Open();
        CustomAttributes(@event);
        Accessor(".addon", @event.AddMethod);
        Accessor(".removeon", @event.RemoveMethod);
        Accessor(".fire", @event.InvokeMethod);
        foreach (var other in @event.OtherMethods)
        {
            Accessor(".other", other);
        }

        Close();
    }

    void Accessor(string kind, MethodDefinition? method)
    {
        if (method != null)
        {
            Line($"{kind} {Names.Method(method)}");
        }
    }

    void CustomAttributes(ICustomAttributeProvider provider)
    {
        if (!provider.HasCustomAttributes)
        {
            return;
        }

        foreach (var attribute in provider.CustomAttributes)
        {
            Line($".custom {Attributes.Format(attribute)}");
        }
    }
}
