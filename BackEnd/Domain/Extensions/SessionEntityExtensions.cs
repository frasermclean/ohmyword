using OhMyWord.Domain.Models;
using OhMyWord.Infrastructure.Models.Entities;

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
