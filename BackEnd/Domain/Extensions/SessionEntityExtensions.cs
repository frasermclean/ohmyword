using OhMyWord.Core.Models;
using OhMyWord.Integrations.Models.Entities;

namespace OhMyWord.Domain.Extensions;

public static class SessionEntityExtensions
{
    public static SessionEntity ToEntity(this Session session) => new()
    {
        Id = session.Id.ToString(),
        RoundCount = session.RoundCount,
        StartDate = session.StartDate,
        EndDate = session.EndDate
    };
}
