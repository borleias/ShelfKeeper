// <copyright file="StringExtensions.cs" company="ShelfKeeper">
// Copyright (c) ShelfKeeper. All rights reserved.
// </copyright>

using System.Text.RegularExpressions;

namespace ShelfKeeper.Infrastructure.Extensions
{
    /// <summary>
    /// Provides extension methods for string manipulation.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the input string to snake_case.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The snake_case representation of the input string.</returns>
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return Regex.Replace(input, "([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }

        /// <summary>
        /// Pluralizes the input string by adding 's' if it doesn't already end with 's'.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The pluralized string.</returns>
        public static string Pluralize(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            // Simple pluralization rule: add 's' if not ending in 's'
            if (input.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                return input;
            }
            return input + "s";
        }
    }
}