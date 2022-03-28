using OhMyWord.Data.Models;

namespace OhMyWord.Api.Responses.Game;

public class RegisterPlayerResponse
{
    public string PlayerId { get; }

    public RegisterPlayerResponse(Player player)
    {
        PlayerId = player.Id;
    }
}
