using System;
using System.Globalization;

namespace CSS.Connector.FileProcessing.Parsing.Parser
{
    /// <summary>
    /// Contains name conversion methods
    /// </summary>
    public static class NameConvert
    {
        /// <summary>
        /// Converts the first letter of a string to upper case.
        /// </summary>
        /// <param name="input">The string to convert</param>
        /// <returns>string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "string.IsNullOrEmpty")]
        public static string ToPascalCase(string input)
        {
            if (!string.IsNullOrEmpty(input) && Char.IsLower(input, 0))
            {
                return Char.ToUpper(input[0], CultureInfo.CurrentCulture) + input.Substring(1);
            }
            return input;
        }

        /// <summary>
        /// Converts the first letter of a string to lower case.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "string.IsNullOrEmpty")]
        public static string ToCamelCase(string input)
        {
            if (!string.IsNullOrEmpty(input) && Char.IsUpper(input, 0))
            {
                return Char.ToLower(input[0], CultureInfo.CurrentCulture) + input.Substring(1);
            }
            return input;
        }

        /// <summary>
        /// Converts a singular name to plural form
        /// </summary>
        /// <param name="input">The name to convert.</param>
        /// <returns>string</returns>
        public static string ToPluralForm(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                if (input.EndsWith("y", StringComparison.CurrentCulture))
                {
                    return input.Substring(0, input.Length - 1) + "ies";
                }
                else if (input.EndsWith("s", StringComparison.CurrentCulture))
                {
                    return input + "es";
                }
                else
                {
                    return input + "s";
                }
            }
            return input;
        }
    }
}