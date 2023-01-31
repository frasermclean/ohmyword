using FastEndpoints;
using OhMyWord.Api.Models;

namespace OhMyWord.Api.Events.GameStateChanged;

public record GameStateChangedEvent(GameState GameState) : IEvent;
