using Microsoft.Extensions.Options;
using OhMyWord.Domain.Models;
using OhMyWord.Domain.Options;
using OhMyWord.Infrastructure.Models.Entities;
using System.Net;

namespace OhMyWord.Domain.Tests;

public class TestDataFixture
{
    public IOptions<RoundOptions> CreateOptions() =>
        Microsoft.Extensions.Options.Options.Create(new RoundOptions
        {
            LetterHintDelay = 3, PostRoundDelay = 5, GuessLimit = 3
        });

    public Word CreateWord() => new()
    {
        Id = "test",
        Definitions = new[] { new Definition { PartOfSpeech = PartOfSpeech.Noun, Value = "Test noun" } },
        Frequency = 5,
        LastModifiedBy = Guid.NewGuid()
    };

    public Round CreateRound()
    {
        var players = new[] { CreatePlayer(), CreatePlayer(), CreatePlayer() };
        var round = new Round(CreateWord(), TimeSpan.FromSeconds(CreateOptions().Value.LetterHintDelay),
            players.Select(player => player.Id)) { Number = 1, GuessLimit = 3, SessionId = Guid.NewGuid() };

        foreach (var playerId in players.Select(p => p.Id))
        {
            round.ProcessGuess(playerId, round.Id, "test");
        }

        round.EndRound(RoundEndReason.AllPlayersGuessed);

        return round;
    }

    public static Player CreatePlayer(Guid playerId = default) => new()
    {
        Id = playerId == default ? Guid.NewGuid() : playerId,
        Name = Guid.NewGuid().ToString()[..4],
        ConnectionId = Guid.NewGuid().ToString()[..8],
        UserId = Guid.NewGuid(),
        Score = Random.Shared.Next(500),
        RegistrationCount = Random.Shared.Next(5),
        VisitorId = Guid.NewGuid().ToString()[..8],
        GeoLocation = new GeoLocation
        {
            IpAddress = IPAddress.Loopback,
            CountryCode = "AU",
            CountryName = "Australia",
            City = "Melbourne",
            LastUpdated = DateTime.Now
        }
    };
}
