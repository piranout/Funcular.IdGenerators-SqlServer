using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Funcular.IdGenerators.Base36
{
    [CompilerGenerated]
    public static class ConcurrentStopwatch
    {
        private static readonly DateTime _utcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly object _lock = new object();

        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        private static readonly long[] _lastMicroseconds = new long[1] {0};

        private static readonly long _timeZeroMicroseconds;

        static ConcurrentStopwatch()
        {
            var lastInitialized = DateTime.UtcNow;
            var timeZero = lastInitialized.Subtract(_utcEpoch);
            _timeZeroMicroseconds = timeZero.Ticks / 10;
        }

        /// <summary>
        /// Returns the Unix time in microseconds (µ″ since UTC epoch)
        /// </summary>
        /// <returns></returns>
        public static long GetMicroseconds()
        {
            lock (_lock)
            {
                long microseconds = 0;
                do
                {
                    microseconds = _timeZeroMicroseconds + (_sw.Elapsed.Ticks/10);
                } while (microseconds <= _lastMicroseconds[0]);

                _lastMicroseconds[0] = microseconds;
                return microseconds;
            }

        }
    }
}