using FastEndpoints;
using OhMyWord.Core.Models;

namespace OhMyWord.Api.Events.GameStateChanged;

public record GameStateChangedEvent(GameState GameState) : IEvent;
