using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace InterpolatedStringFormatter
{
    public sealed class StringValuesFormatter
    {
        private const string NullValue = "(null)";
        private readonly static object[] EmptyArray = new object[0];
        private readonly static char[] FormatDelimiters = { ',', ':' };
        private readonly string _format;
        private readonly List<string> _valueNames = new List<string>();

        public StringValuesFormatter(string format)
        {
            OriginalFormat = format;

            var sb = new StringBuilder();
            var scanIndex = 0;
            var endIndex = format.Length;

            while (scanIndex < endIndex)
            {
                var openBraceIndex = FindBraceIndex(format, '{', scanIndex, endIndex);
                var closeBraceIndex = FindBraceIndex(format, '}', openBraceIndex, endIndex);

                // Format item syntax : { index[,alignment][ :formatString] }.
                var formatDelimiterIndex = FindIndexOfAny(format, FormatDelimiters, openBraceIndex, closeBraceIndex);

                if (closeBraceIndex == endIndex)
                {
                    sb.Append(format, scanIndex, endIndex - scanIndex);
                    scanIndex = endIndex;
                }
                else
                {
                    sb.Append(format, scanIndex, openBraceIndex - scanIndex + 1);
                    var valueName = format.Substring(openBraceIndex + 1, formatDelimiterIndex - openBraceIndex - 1);
                    var index = _valueNames.IndexOf(valueName);

                    if (index >= 0)
                    {
                        sb.Append(index.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        sb.Append(_valueNames.Count.ToString(CultureInfo.InvariantCulture));
                        _valueNames.Add(valueName);
                    }
                    sb.Append(format, formatDelimiterIndex, closeBraceIndex - formatDelimiterIndex + 1);

                    scanIndex = closeBraceIndex + 1;
                }
            }

            _format = sb.ToString();
        }

        public string OriginalFormat { get; private set; }
        public List<string> ValueNames => _valueNames;

        private static int FindBraceIndex(string format, char brace, int startIndex, int endIndex)
        {
            // Example: {{prefix{{{Argument}}}suffix}}.
            var braceIndex = endIndex;
            var scanIndex = startIndex;
            var braceOccurenceCount = 0;

            while (scanIndex < endIndex)
            {
                if (braceOccurenceCount > 0 && format[scanIndex] != brace)
                {
                    if (braceOccurenceCount % 2 == 0)
                    {
                        // Even number of '{' or '}' found. Proceed search with next occurence of '{' or '}'.
                        braceOccurenceCount = 0;
                        braceIndex = endIndex;
                    }
                    else
                    {
                        // An unescaped '{' or '}' found.
                        break;
                    }
                }
                else if (format[scanIndex] == brace)
                {
                    if (brace == '}')
                    {
                        if (braceOccurenceCount == 0)
                            // For '}' pick the first occurence.
                            braceIndex = scanIndex;
                    }
                    else
                    {
                        // For '{' pick the last occurence.
                        braceIndex = scanIndex;
                    }

                    braceOccurenceCount++;
                }

                scanIndex++;
            }

            return braceIndex;
        }

        private static int FindIndexOfAny(string format, char[] chars, int startIndex, int endIndex)
        {
            var findIndex = format.IndexOfAny(chars, startIndex, endIndex - startIndex);
            return findIndex == -1 ? endIndex : findIndex;
        }

        public string Format(object[] values)
        {
            if (values != null)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = FormatArgument(values[i]);
                }
            }

            return string.Format(CultureInfo.InvariantCulture, _format, values ?? EmptyArray);
        }

        public KeyValuePair<string, object> GetValue(object[] values, int index)
        {
            if (index < 0 || index > _valueNames.Count)
                throw new IndexOutOfRangeException(nameof(index));

            if (_valueNames.Count > index)
                return new KeyValuePair<string, object>(_valueNames[index], values[index]);

            return new KeyValuePair<string, object>("{OriginalFormat}", OriginalFormat);
        }

        private object FormatArgument(object value)
        {
            if (value == null)
                return NullValue;

            // since 'string' implements IEnumerable, special case it
            if (value is string)
                return value;

            // if the value implements IEnumerable, build a comma separated string.
            var enumerable = value as IEnumerable;
            if (enumerable != null)
                return string.Join(", ", enumerable.Cast<object>().Select(o => o ?? NullValue));

            return value;
        }

    }
}
