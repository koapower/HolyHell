public readonly struct BuffDefinition
{
    public readonly string Id;
    public readonly string Parameter;
    public readonly int StackCount;
    public readonly int Duration;
    public BuffDefinition(string id, string parameter, int stack, int duration)
    {
        Id = id;
        Parameter = parameter;
        StackCount = stack;
        Duration = duration;
    }
}