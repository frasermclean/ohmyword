namespace WhatTheWord.Data.Models;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();
}
