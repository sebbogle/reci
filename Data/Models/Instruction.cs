namespace Reci.Data.Models;

public record Instruction : IGroupable
{
    public required string Text { get; set; }

    public string? Group { get; set; }
}

public static class InstructionExtensions
{
    public static bool IsEmpty(this Instruction? instruction)
    {
        if (instruction is null)
        {
            return true;
        }
        return string.IsNullOrEmpty(instruction.Text);
    }
}
