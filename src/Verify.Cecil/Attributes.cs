static class Attributes
{
    public static string Format(CustomAttribute attribute)
    {
        var constructor = Names.Method(attribute.Constructor);
        List<string> arguments;
        try
        {
            arguments = Arguments(attribute);
        }
        catch
        {
            // Decoding the blob can require resolving enum types in assemblies that cannot be found
            return $"{constructor} = ({Names.Hex(attribute.GetBlob())})";
        }

        if (arguments.Count == 0)
        {
            return constructor;
        }

        return $"{constructor} = ({string.Join(", ", arguments)})";
    }

    static List<string> Arguments(CustomAttribute attribute)
    {
        var arguments = new List<string>();
        var isInformationalVersion = attribute.AttributeType.FullName == "System.Reflection.AssemblyInformationalVersionAttribute";
        foreach (var argument in attribute.ConstructorArguments)
        {
            if (isInformationalVersion &&
                argument.Value is string version)
            {
                arguments.Add(Names.Quote(RemoveSourceRevision(version)));
                continue;
            }

            arguments.Add(Argument(argument));
        }

        foreach (var field in attribute.Fields)
        {
            arguments.Add($"field {Names.Identifier(field.Name)} = {Argument(field.Argument)}");
        }

        foreach (var property in attribute.Properties)
        {
            arguments.Add($"property {Names.Identifier(property.Name)} = {Argument(property.Argument)}");
        }

        return arguments;
    }

    // 1.0.0+3a5b1c... changes on every commit
    static string RemoveSourceRevision(string version)
    {
        var index = version.IndexOf('+');
        if (index < 0)
        {
            return version;
        }

        return version.Substring(0, index);
    }

    static string Argument(CustomAttributeArgument argument)
    {
        while (true)
        {
            var value = argument.Value;
            switch (value)
            {
                case null:
                    return "null";
                case CustomAttributeArgument boxed:
                    argument = boxed;
                    continue;
                case CustomAttributeArgument[] items:
                    return $"[{string.Join(", ", items.Select(Argument))}]";
                case TypeReference type:
                    return $"typeof({Names.Token(type)})";
                case string text:
                    return Names.Quote(text);
                case bool boolean:
                    return boolean.ToString().ToLowerInvariant();
                case char ch:
                    return $"char(0x{(int) ch:X4})";
            }

            var number = Names.Number(value);
            var argumentType = argument.Type;
            if (argumentType.IsPrimitive)
            {
                return number;
            }

            // enums: show the enum type with the underlying value
            return $"{Names.Token(argumentType)}({number})";
        }
    }
}
