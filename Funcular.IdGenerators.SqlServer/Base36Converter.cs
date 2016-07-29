using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Funcular.IdGenerators.BaseConversion
{
    internal static class Base36Converter
    {
        private const int BITS_IN_LONG = 64;
        private const int BASE = 36;
        private const string CHAR_LIST = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly char[] _digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        static Base36Converter()
        {
            BaseConverter.SetCharList(CHAR_LIST);
        }

        /// <summary>
        ///     The character set for encoding. Defaults to upper-case alphanumerics 0-9, A-Z.
        /// </summary>
        public static string CharList => CHAR_LIST;

        public static string FromHex(string hex)
        {
            return BaseConverter.Convert(hex.ToUpper().Replace("-", ""), 16, 36);
        }

        public static string FromGuid(Guid guid)
        {
            return BaseConverter.Convert(guid.ToString("N"), 16, 36);
        }

        public static string FromInt32(int int32)
        {
            return BaseConverter.Convert(int32.ToString(CultureInfo.InvariantCulture), 10, 36);
        }

        public static string FromInt64(long int64)
        {
            return BaseConverter.Convert(number: int64.ToString(CultureInfo.InvariantCulture), fromBase: 10, toBase: 36);
        }

        /// <summary>
        /// Converts the given decimal number to the numeral system with the
        /// specified radix (in the range [2, 36]).
        /// </summary>
        /// <param name="decimalNumber">The number to convert.</param>
        /// <returns></returns>
        public static string FromLong(long decimalNumber)
        {
            unchecked
            {
                int index = BITS_IN_LONG - 1;

                if (decimalNumber == 0)
                    return "0";

                long currentNumber = Math.Abs(decimalNumber);

                var fromLongBuffer = new char[BITS_IN_LONG];
                {
                    while (currentNumber != 0)
                    {
                        int remainder = (int)(currentNumber % BASE);
                        fromLongBuffer[index--] = _digits[remainder];
                        currentNumber = currentNumber / BASE;
                    }
                    return new string(fromLongBuffer, index + 1, BITS_IN_LONG - index - 1);
                }
            }
        }

        /// <summary>
        ///     Encode the given number into a Base36 string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static String Encode(long input)
        {
            unchecked
            {
                //char[] clistarr = CharList.ToCharArray();
                var result = new Stack<char>();
                while (input != 0)
                {
                    result.Push(_digits[input % 36]);
                    input /= 36;
                }
                return new string(result.ToArray());
            }
        }

        /// <summary>
        ///     Decode the Base36 Encoded string into a number
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Int64 Decode(string input)
        {
            unchecked
            {
                IEnumerable<char> reversed = input.ToUpper().Reverse();
                long result = 0;
                int pos = 0;
                foreach (var c in reversed)
                {
                    result += CharList.IndexOf(c) * (long)Math.Pow(36, pos);
                    pos++;
                }
                return result;
            }
        }
    }
}