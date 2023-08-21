﻿using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Integrations.Options;

public class RapidApiOptions
{
    public const string SectionName = "RapidApi";

    [Required] public string ApiKey { get; init; } = string.Empty;
}
