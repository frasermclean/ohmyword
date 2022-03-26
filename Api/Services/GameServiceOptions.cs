﻿using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Services;

public class GameServiceOptions
{
    [Range(5, 120, ErrorMessage = "Invalid round length.")]
    public int RoundLength { get; set; }
}