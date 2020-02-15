﻿
#region Usings

using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Funcular.IdGenerators.BaseConversion;
using Funcular.IdGenerators.Enums;
#endregion

namespace Funcular.IdGenerators.Base36
{
    [CompilerGenerated]
    public class Base36IdGenerator
    {
        #region Private fields
        #region Static
        [CompilerGenerated]
        private static readonly object _randomLock = new object();
        [CompilerGenerated]
        private static readonly string[] _hostHash = new string[1];
        [CompilerGenerated]
        private static readonly Random _random = new Random();
        /// <summary>
        ///     This is UTC Epoch. In shorter Id implementations it was configurable, to allow
        ///     one to milk more longevity out of a shorter series of timestamps.
        /// </summary>
        [CompilerGenerated]
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [CompilerGenerated]
        private static readonly StringBuilder _sb = new StringBuilder();
        [CompilerGenerated]
        private static readonly byte[] _randomBuffer = new byte[8];
        [CompilerGenerated]
        private static readonly string[] _hashBase36 = new string[1];

        #endregion



        #region Instance

        private readonly string _delimiter;
        private readonly int[] _delimiterPositions;
        private readonly long _maxRandom;
        private readonly int _numRandomCharacters;
        private readonly int _numServerCharacters;
        private readonly int _numTimestampCharacters;
        private readonly string _reservedValue;

        #endregion
        #endregion



        #region Public properties
        #endregion



        #region Constructors

        /// <summary>
        ///     Static constructor
        /// </summary>
        static Base36IdGenerator()
        {
        }

        ///     The default Id format is 11 characters for the timestamp (4170 year lifespan),
        ///     4 for the server hash (1.6m hashes), 5 for the random value (60m combinations),
        ///     and no reserved character. The default delimited format will be four dash-separated
        ///     groups of 5.
        public Base36IdGenerator()
            : this(11, 4, 5, null, "-")
        {
            this._delimiterPositions = new[] { 15, 10, 5 };
        }

        /// <summary>
        ///     The layout is Timestamp + Server Hash [+ Reserved] + Random.
        /// </summary>
        public Base36IdGenerator(int numTimestampCharacters = 11, int numServerCharacters = 4, int numRandomCharacters = 5, string reservedValue = "", string delimiter = "-", int[] delimiterPositions = null)
        {
            // throw if any argument would cause out-of-range exceptions
            ValidateConstructorArguments(numTimestampCharacters, numServerCharacters, numRandomCharacters);

            this._numTimestampCharacters = numTimestampCharacters;
            this._numServerCharacters = numServerCharacters;
            this._numRandomCharacters = numRandomCharacters;
            this._reservedValue = reservedValue;
            this._delimiter = delimiter;
            this._delimiterPositions = delimiterPositions?.OrderByDescending(x=>x).ToArray();

            this._maxRandom = (long)Math.Pow(36d, numRandomCharacters);
            _hostHash[0] = ComputeHostHash();

            Debug.WriteLine("Instance constructor fired");

            InitStaticMicroseconds();
        }

        #endregion



        #region Public methods

        /// <summary>
        ///     Generates a unique, sequential, Base36 string. If this instance was instantiated using 
        ///     the default constructor, it will be 20 characters long.
        ///     The first 11 characters are the microseconds elapsed since the InService DateTime
        ///     (Epoch by default).
        ///     The next 4 characters are the SHA1 of the hostname in Base36.
        ///     The last 5 characters are random Base36 number between 0 and 36 ^ 5.
        /// </summary>
        /// <returns>Returns a unique, sequential, 20-character Base36 string</returns>
        public string NewId()
        {
            return NewId(false);
        }

        /// <summary>
        ///     Generates a unique, sequential, Base36 string; you control the len
        ///     The first 10 characters are the microseconds elapsed since the InService DateTime
        ///     (constant field you hard-code in this file).
        ///     The next 2 characters are a compressed checksum of the MD5 of this host.
        ///     The next 1 character is a reserved constant of 36 ('Z' in Base36).
        ///     The last 3 characters are random number less than 46655 additional for additional uniqueness.
        /// </summary>
        /// <returns>Returns a unique, sequential, 16-character Base36 string</returns>
        public string NewId(bool delimited)
        {
            // Keep access sequential so threads cannot accidentally
            // read another thread's values within this method:

            // Microseconds since InService (using Stopwatch) provides the 
            // first n chars (n = _numTimestampCharacters):
            lock (_sb)
            {
                _sb.Clear();
                long microseconds = ConcurrentStopwatch.GetMicroseconds();
                string base36Microseconds = Base36Converter.FromLong(microseconds);
                if (base36Microseconds.Length > this._numTimestampCharacters)
                    base36Microseconds = base36Microseconds.Substring(0, this._numTimestampCharacters);
                else if (base36Microseconds.Length < this._numTimestampCharacters)
                    base36Microseconds = base36Microseconds.PadLeft(this._numTimestampCharacters, '0');
                _sb.Append(base36Microseconds);

                _sb.Append(_hostHash[0]);

                if (!string.IsNullOrWhiteSpace(this._reservedValue))
                {
                    _sb.Append(this._reservedValue);
                }
                // Add the random component:
                _sb.Append(GetRandomBase36DigitsSafe());

                if (!delimited || string.IsNullOrWhiteSpace(_delimiter) || this._delimiterPositions == null)
                    return _sb.ToString();
                foreach (var pos in this._delimiterPositions)
                {
                    _sb.Insert(pos, this._delimiter);
                }
                return _sb.ToString();
            }
        }

        /// <summary>
        /// Given a non-delimited Id, format it with the current instance’s
        /// delimiter and delimiter positions. If Id already contains delimiter,
        /// or is null or empy, returns Id unmodified.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string Format(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Contains(_delimiter))
                return id;
            StringBuilder sb = new StringBuilder(id);
            foreach (var pos in this._delimiterPositions)
            {
                sb.Insert(pos, _delimiter);
            }
            return sb.ToString();
        }

        /// <summary>
        ///     Base36 representation of the SHA1 of the hostname. The constructor argument
        ///     numServerCharacters controls the maximum length of this hash.
        /// </summary>
        /// <returns>2 character Base36 checksum of MD5 of hostname</returns>
        public string ComputeHostHash(string hostname = null)
        {
            if (_hashBase36[0]?.Length > 0)
                return _hashBase36[0];
            if (string.IsNullOrWhiteSpace(hostname))
                hostname = Environment.MachineName;
            string hashHex;
            using (var sha1 = new SHA1Managed())
            {
                hashHex = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(hostname)));
                if (hashHex.Length > 14) // > 14 chars overflows int64
                    hashHex = hashHex.Substring(0, 14);
            }
            string hashBase36 = Base36Converter.FromHex(hashHex);
            if (hashBase36.Length > this._numServerCharacters)
                hashBase36 = hashBase36.Substring(0, this._numServerCharacters);
            else if (hashBase36.Length < this._numServerCharacters)
                hashBase36 = hashBase36.PadLeft(_numServerCharacters, '0');
            _hashBase36[0] = hashBase36;
            return _hashBase36[0];
        }

        /// <summary>
        ///     Gets a random Base36 string of the specified <paramref name="length"/>.
        /// </summary>
        /// <returns></returns>
        public string GetRandomString(int length)
        {
            if (length < 1 || length > 12)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 12; 36^13 overflows Int64.MaxValue");
            lock (_randomLock)
            {
                var maxRandom = (long)Math.Pow(36, length);
                _random.NextBytes(_randomBuffer);
                var random = Math.Abs(BitConverter.ToInt64(_randomBuffer, 0) % maxRandom);
                string encoded = Base36Converter.FromLong(random);
                return encoded.Length > length ?
                    encoded.Substring(0, length) :
                    encoded.PadLeft(length, '0');
            }
        }

        /// <summary>
        /// Get a Base36 encoded timestamp string, based on Epoch. Use for disposable
        /// strings where global/universal uniqueness is not critical. If using the 
        /// default resolution of Microseconds, 5 character values are exhausted in 1 minute.
        /// 6 characters = ½ hour. 7 characters = 21 hours. 8 characters = 1 month.
        /// 9 characters = 3 years. 10 characters = 115 years. 11 characters = 4170 years.
        /// 12 characteres = 150 thousand years.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="resolution"></param>
        /// <param name="sinceUtc">Defaults to Epoch</param>
        /// <param name="strict">If false (default), overflow values will use the 
        /// value modulus 36. Otherwise it will throw an overflow exception.</param>
        /// <returns></returns>
        public string GetTimestamp(int length, TimestampResolution resolution = TimestampResolution.Microsecond, DateTime? sinceUtc = null, bool strict = false)
        {
            if (length < 1 || length > 12)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 1 and 12; 36^13 overflows Int64.MaxValue");
            var origin = sinceUtc ?? _epoch;
            var elapsed = DateTime.UtcNow.Subtract(origin);
            long intervals;
            switch (resolution)
            {
                case TimestampResolution.Day:
                    intervals = elapsed.Days;
                    break;
                case TimestampResolution.Hour:
                    intervals = Convert.ToInt64(elapsed.TotalHours);
                    break;
                case TimestampResolution.Minute:
                    intervals = Convert.ToInt64(elapsed.TotalMinutes);
                    break;
                case TimestampResolution.Second:
                    intervals = Convert.ToInt64(elapsed.TotalSeconds);
                    break;
                case TimestampResolution.Millisecond:
                    intervals = Convert.ToInt64(elapsed.TotalMilliseconds);
                    break;
                case TimestampResolution.Microsecond:
                    intervals = (long)(elapsed.TotalMilliseconds * 1000.0); // elapsed.TotalMicroseconds();
                    break;
                case TimestampResolution.Ticks:
                    intervals = elapsed.Ticks;
                    break;
                case TimestampResolution.None:
                default:
                    throw new ArgumentOutOfRangeException(nameof(resolution));
            }
            var combinations = Math.Pow(36, length);
            if (combinations < intervals)
            {
                if (strict)
                {
                    throw new OverflowException(string.Format("At resolution {0}, value is greater than {1}-character timestamps can express.", resolution.ToString(), length));
                }
                intervals %= 36;
            }
            string encoded = Base36Converter.FromLong(intervals);
            return encoded.Length > length ?
                encoded.Substring(0, length) :
                encoded.PadLeft(length, '0');
        }



        #endregion



        #region Nonpublic methods

        private static void ValidateConstructorArguments(int numTimestampCharacters, int numServerCharacters, int numRandomCharacters)
        {
            if ((numTimestampCharacters > 12)||(numServerCharacters > 12)||(numRandomCharacters > 12))
                throw new ArgumentOutOfRangeException(nameof(numRandomCharacters), "The maximum characters in any component is 12.");

            if ((numTimestampCharacters < 0)|| (numServerCharacters < 0)||(numRandomCharacters < 0))
                throw new ArgumentOutOfRangeException(nameof(numRandomCharacters), "Number must not be negative.");
        }

        /// <summary>
        ///     Returns value with all non base 36 characters removed. Uses mutex.
        /// </summary>
        /// <returns></returns>
        /// <summary>
        ///     Return the elapsed microseconds since the in-service DateTime; will never
        ///     return the same value twice, even across multiple processes.
        /// </summary>
        /// <returns></returns>
        internal static long GetMicrosecondsSafe()
        {
            return ConcurrentStopwatch.GetMicroseconds();
        }

        /// <summary>
        ///     Return the elapsed microseconds since the in-service DateTime; will never
        ///     return the same value twice. Uses a high-resolution Stopwatch (not DateTime.Now)
        ///     to measure durations.
        /// </summary>
        /// <returns></returns>
        internal static long GetMicroseconds()
        {
            return ConcurrentStopwatch.GetMicroseconds();
        }

        private static void InitStaticMicroseconds()
        {
            Console.WriteLine(GetMicroseconds());
        }

        /// <summary>
        ///     Gets random component of Id, pre trimmed and padded to the correct length.
        /// </summary>
        /// <returns></returns>
        private string GetRandomBase36DigitsSafe()
        {
            lock (_randomLock)
            {
                byte[] buffer = new byte[8];
                _random.NextBytes(buffer);
                var number = Math.Abs(BitConverter.ToInt64(buffer, 0) % this._maxRandom);
                string encoded = Base36Converter.FromLong(number);
                return
                    encoded.Length == this._numRandomCharacters
                        ? encoded
                        : encoded.Length > this._numRandomCharacters
                            ? encoded.Substring(0, _numRandomCharacters)
                            : encoded.PadLeft(this._numRandomCharacters, '0');
            }
        }

        #endregion
    }
}

namespace Funcular.IdGenerators.BaseConversion
{
}

// ReSharper restore RedundantCaseLabel
