using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TimeDataViewer.Core
{
    public static class StringHelper
    {
        /// <summary>
        /// The formatting expression.
        /// </summary>
        private static readonly Regex FormattingExpression = new Regex("{(?<Property>.+?)(?<Format>\\:.*?)?}");

        /// <summary>
        /// Creates a valid format string on the form "{0:###}".
        /// </summary>
        /// <param name="input">The input format string.</param>
        /// <returns>The corrected format string.</returns>
        public static string CreateValidFormatString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "{0}";
            }

            if (input.Contains("{"))
            {
                return input;
            }

            return string.Concat("{0:", input, "}");
        }

        /// <summary>
        /// Formats each item in a sequence by the specified format string and property.
        /// </summary>
        /// <param name="source">The source target.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="formatString">The format string. The format argument {0} can be used for the value of the property in each element of the sequence.</param>
        /// <param name="provider">The format provider.</param>
        /// <exception cref="System.InvalidOperationException">Could not find property.</exception>
        public static IEnumerable<string> Format(this IEnumerable source, string propertyName, string formatString, IFormatProvider provider)
        {
            var fs = CreateValidFormatString(formatString);
            if (string.IsNullOrEmpty(propertyName))
            {
                foreach (var element in source)
                {
                    yield return string.Format(provider, fs, element);
                }
            }
            else
            {
                var reflectionPath = new ReflectionPath(propertyName);
                foreach (var element in source)
                {
                    var value = reflectionPath.GetValue(element);
                    yield return string.Format(provider, fs, value);
                }
            }
        }
    }
}
