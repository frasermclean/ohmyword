namespace OhMyWord.Api.Tests.Priority;

public class TestPriorityAttribute : Attribute
{
    public TestPriorityAttribute(int priority)
    {
        Priority = priority;
    }

    public int Priority { get; }
}
