﻿namespace OhMyWord.Integrations.RapidApi.Models.IpGeoLocation;

public class Country
{
    public string? Name { get; set; } = string.Empty;
    public string? Code { get; set; } = string.Empty;
    public Flag Flag { get; set; } = new();
}
