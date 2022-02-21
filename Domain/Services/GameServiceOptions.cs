using System.ComponentModel.DataAnnotations;

namespace WhatTheWord.Domain.Services;

public class GameServiceOptions
{
    [Range(5, 120, ErrorMessage = "Invalid round length.")]
    public int RoundLength { get; set; }
}
