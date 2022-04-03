﻿using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Api.Options;

public class GameCoordinatorOptions
{
    public const string SectionName = "GameCoordinator";

    [Range(5, 120, ErrorMessage = "Invalid round length.")]
    public int RoundLength { get; set; }
}