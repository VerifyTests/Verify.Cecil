static class Offsets
{
    // Computed rather than read from Instruction.Offset, so a body modified in memory
    // gets the offsets it will have when written.
    public static Dictionary<Instruction, int> Compute(MethodBody body, out int end)
    {
        var offsets = new Dictionary<Instruction, int>();
        var offset = 0;
        foreach (var instruction in body.Instructions)
        {
            offsets[instruction] = offset;
            offset += Size(instruction);
        }

        end = offset;
        return offsets;
    }

    static int Size(Instruction instruction)
    {
        var opCode = instruction.OpCode;
        return opCode.Size + OperandSize(opCode.OperandType, instruction.Operand);
    }

    static int OperandSize(OperandType operandType, object? operand)
    {
        switch (operandType)
        {
            case OperandType.InlineNone:
                return 0;
            case OperandType.ShortInlineBrTarget:
            case OperandType.ShortInlineI:
            case OperandType.ShortInlineVar:
            case OperandType.ShortInlineArg:
                return 1;
            case OperandType.InlineVar:
            case OperandType.InlineArg:
                return 2;
            case OperandType.InlineI8:
            case OperandType.InlineR:
                return 8;
            case OperandType.InlineSwitch:
                if (operand is Instruction[] targets)
                {
                    return 4 + 4 * targets.Length;
                }

                return 4;
            default:
                return 4;
        }
    }

    public static string Label(int offset) =>
        $"IL_{offset:x4}";
}
