namespace VerifyTests.Cecil;

public class CecilValidationException(IReadOnlyList<string> problems) :
    Exception(BuildMessage(problems))
{
    public IReadOnlyList<string> Problems { get; } = problems;

    static string BuildMessage(IReadOnlyList<string> problems)
    {
        var builder = new StringBuilder("IL validation failed:");
        foreach (var problem in problems)
        {
            builder.Append("\n - ");
            builder.Append(problem);
        }

        return builder.ToString();
    }
}
