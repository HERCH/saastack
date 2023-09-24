﻿using Infrastructure.WebApi.Interfaces;

namespace Infrastructure.WebApi.Common;

/// <summary>
///     Options for a SEARCH request
/// </summary>
public class HasSearchOptions : IHasSearchOptions
{
    public int? Limit { get; set; }

    public int? Offset { get; set; }

    public string? Sort { get; set; }

    public string? Filter { get; set; }

    public string? Embed { get; set; }

    public string? Distinct { get; set; }
}