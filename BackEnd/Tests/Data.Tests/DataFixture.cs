using OhMyWord.Data;
using OhMyWord.Data.Entities;

namespace Data.Tests;

public class DataFixture
{
    public WordEntity TestWord { get; } = new() { Id = "test", DefinitionCount = 1, Timestamp = 123 };    
}
