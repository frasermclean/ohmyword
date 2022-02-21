using System.ComponentModel.DataAnnotations;

namespace WhatTheWord.Data;

public class CosmosDbOptions
{
    [Required]
    public string Endpoint { get; set; } = default!;

    [Required]
    public string PrimaryKey { get; set; } = default!;

    [Required]
    public string DatabaseId { get; set; } = default!;
}
