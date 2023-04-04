using OhMyWord.Domain.Models;

namespace OhMyWord.Api.Events.GameStateChanged;

public record GameStateChangedEvent(GameState GameState) : IEvent;
