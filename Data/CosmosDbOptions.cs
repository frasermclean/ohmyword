﻿using System.ComponentModel.DataAnnotations;

namespace OhMyWord.Data;

public class CosmosDbOptions
{
    [Required]
    public string Endpoint { get; set; } = default!;

    [Required]
    public string PrimaryKey { get; set; } = default!;

    [Required]
    public string DatabaseId { get; set; } = default!;
}
