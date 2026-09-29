// Renders references in ILAsm syntax.
static class Names
{
    // Signature position: class and valuetype keywords are required
    public static string Signature(TypeReference type) =>
        Format(type, true);

    // Token position (extends, box, castclass, declaring types): the scoped name, with keywords only where ILAsm needs them
    public static string Token(TypeReference type) =>
        Format(type, false);

    static string Format(TypeReference type, bool keyword)
    {
        switch (type)
        {
            case GenericParameter parameter:
                return GenericParameter(parameter);
            case ByReferenceType byReference:
                return Signature(byReference.ElementType) + "&";
            case PointerType pointer:
                return Signature(pointer.ElementType) + "*";
            case PinnedType pinned:
                return Signature(pinned.ElementType) + " pinned";
            case ArrayType array:
                return Signature(array.ElementType) + Dimensions(array);
            case RequiredModifierType required:
                return $"{Format(required.ElementType, keyword)} modreq({Token(required.ModifierType)})";
            case OptionalModifierType optional:
                return $"{Format(optional.ElementType, keyword)} modopt({Token(optional.ModifierType)})";
            case SentinelType sentinel:
                return $"..., {Signature(sentinel.ElementType)}";
            case FunctionPointerType pointer:
                return $"method {CallingConvention(pointer)}{Signature(pointer.ReturnType)} *({Parameters(pointer)})";
            case GenericInstanceType instance:
                var arguments = string.Join(", ", instance.GenericArguments.Select(Signature));
                return $"{Format(instance.ElementType, true)}<{arguments}>";
        }

        var name = Scope(type) + TypeName(type);
        if (!keyword)
        {
            // The scope matters here. eg [System.Runtime]System.Object vs [System.Private.CoreLib]System.Object
            return name;
        }

        var primitive = Primitive(type.MetadataType);
        if (primitive != null)
        {
            return primitive;
        }

        if (type.IsValueType)
        {
            return "valuetype " + name;
        }

        return "class " + name;
    }

    static string GenericParameter(GenericParameter parameter)
    {
        // Cecil names parameters it creates for references by position, eg !0 and !!0
        var name = parameter.Name;
        if (name.Length > 0 && name[0] == '!')
        {
            return name;
        }

        if (parameter.Type == GenericParameterType.Method)
        {
            return "!!" + Identifier(name);
        }

        return "!" + Identifier(name);
    }

    static string Dimensions(ArrayType array)
    {
        if (array.IsVector)
        {
            return "[]";
        }

        var dimensions = array.Dimensions.Select(
            _ =>
            {
                if (_.LowerBound == null)
                {
                    return "";
                }

                if (_.UpperBound == null)
                {
                    return $"{_.LowerBound}...";
                }

                return $"{_.LowerBound}...{_.UpperBound}";
            });
        return $"[{string.Join(",", dimensions)}]";
    }

    static string? Primitive(MetadataType type) =>
        type switch
        {
            MetadataType.Void => "void",
            MetadataType.Boolean => "bool",
            MetadataType.Char => "char",
            MetadataType.SByte => "int8",
            MetadataType.Byte => "uint8",
            MetadataType.Int16 => "int16",
            MetadataType.UInt16 => "uint16",
            MetadataType.Int32 => "int32",
            MetadataType.UInt32 => "uint32",
            MetadataType.Int64 => "int64",
            MetadataType.UInt64 => "uint64",
            MetadataType.Single => "float32",
            MetadataType.Double => "float64",
            MetadataType.String => "string",
            MetadataType.IntPtr => "native int",
            MetadataType.UIntPtr => "native uint",
            MetadataType.Object => "object",
            MetadataType.TypedByReference => "typedref",
            _ => null
        };

    static string Scope(TypeReference type)
    {
        while (type.DeclaringType != null)
        {
            type = type.DeclaringType;
        }

        switch (type.Scope)
        {
            case AssemblyNameReference assembly:
                return $"[{Identifier(assembly.Name)}]";
            case ModuleReference module and not ModuleDefinition:
                return $"[.module {Identifier(module.Name)}]";
            default:
                return "";
        }
    }

    public static string TypeName(TypeReference type)
    {
        if (type.DeclaringType != null)
        {
            return $"{TypeName(type.DeclaringType)}/{Identifier(type.Name)}";
        }

        if (string.IsNullOrEmpty(type.Namespace))
        {
            return Identifier(type.Name);
        }

        return Identifier($"{type.Namespace}.{type.Name}");
    }

    public static string CallingConvention(IMethodSignature method)
    {
        var builder = new StringBuilder();
        if (method.HasThis)
        {
            builder.Append("instance ");
        }

        if (method.ExplicitThis)
        {
            builder.Append("explicit ");
        }

        switch (method.CallingConvention)
        {
            case MethodCallingConvention.VarArg:
                builder.Append("vararg ");
                break;
            case MethodCallingConvention.C:
                builder.Append("unmanaged cdecl ");
                break;
            case MethodCallingConvention.StdCall:
                builder.Append("unmanaged stdcall ");
                break;
            case MethodCallingConvention.ThisCall:
                builder.Append("unmanaged thiscall ");
                break;
            case MethodCallingConvention.FastCall:
                builder.Append("unmanaged fastcall ");
                break;
        }

        return builder.ToString();
    }

    public static string Parameters(IMethodSignature method) =>
        string.Join(", ", method.Parameters.Select(_ => Signature(_.ParameterType)));

    public static string Method(MethodReference method)
    {
        var element = method;
        var genericArguments = "";
        if (method is GenericInstanceMethod instance)
        {
            element = instance.ElementMethod;
            genericArguments = $"<{string.Join(", ", instance.GenericArguments.Select(Signature))}>";
        }

        return $"{CallingConvention(element)}{Signature(element.ReturnType)} {DeclaringType(element.DeclaringType)}::{Identifier(element.Name)}{genericArguments}({Parameters(element)})";
    }

    public static string Field(FieldReference field) =>
        $"{Signature(field.FieldType)} {DeclaringType(field.DeclaringType)}::{Identifier(field.Name)}";

    static string DeclaringType(TypeReference type)
    {
        if (type is GenericInstanceType)
        {
            return Signature(type);
        }

        return Token(type);
    }

    public static string Identifier(string name)
    {
        if (IsSimple(name))
        {
            return name;
        }

        return $"'{name.Replace("\\", @"\\").Replace("'", @"\'")}'";
    }

    static bool IsSimple(string name)
    {
        if (name.Length == 0)
        {
            return false;
        }

        // .ctor and .cctor
        if (name[0] == '.')
        {
            return name is ".ctor" or ".cctor";
        }

        if (char.IsDigit(name[0]))
        {
            return false;
        }

        foreach (var ch in name)
        {
            if (!char.IsLetterOrDigit(ch) &&
                ch is not '_' and not '$' and not '@' and not '`' and not '?' and not '.')
            {
                return false;
            }
        }

        return true;
    }

    public static string Quote(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');
        foreach (var ch in value)
        {
            switch (ch)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append(@"\\");
                    break;
                case '\n':
                    builder.Append(@"\n");
                    break;
                case '\r':
                    builder.Append(@"\r");
                    break;
                case '\t':
                    builder.Append(@"\t");
                    break;
                case '\0':
                    builder.Append(@"\0");
                    break;
                default:
                    if (char.IsControl(ch))
                    {
                        builder.Append($@"\u{(int) ch:x4}");
                    }
                    else
                    {
                        builder.Append(ch);
                    }

                    break;
            }
        }

        builder.Append('"');
        return builder.ToString();
    }

    public static string Number(object value)
    {
        switch (value)
        {
            case float single:
                return single.ToString("R", CultureInfo.InvariantCulture);
            case double @double:
                return @double.ToString("R", CultureInfo.InvariantCulture);
            case IFormattable formattable:
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            default:
                return value.ToString() ?? "";
        }
    }

    public static string Hex(byte[] bytes) =>
        string.Join(" ", bytes.Select(_ => _.ToString("X2")));
}
