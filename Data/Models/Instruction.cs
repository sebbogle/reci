namespace Reci.Data.Models;

using Reci.Interfaces;

public class Instruction : IGroupable
{
    public required string Text { get; set; }

    public Guid? GroupId { get; set; }
}

public static class InstructionExtensions
{
    public static bool IsEmpty(this Instruction? instruction)
    {
        if (instruction is null)
        {
            return true;
        }
        return string.IsNullOrEmpty(instruction.Text)
            && instruction.GroupId is null;
    }

    public static bool IsEqualTo(this Instruction? first, Instruction? second)
    {
        if (first is null && second is null)
        {
            return true;
        }
        if (first is null || second is null)
        {
            return false;
        }
        return first.Text == second.Text
            && first.GroupId == second.GroupId;
    }
}
