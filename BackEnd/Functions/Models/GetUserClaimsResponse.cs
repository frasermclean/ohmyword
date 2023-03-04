using OhMyWord.Data.Entities;
using System.Text.Json.Serialization;

namespace OhMyWord.Functions.Models;

public class GetUserClaimsResponse
{
    public Role Role { get; init; }
}
