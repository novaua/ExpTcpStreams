using System;
using System.Diagnostics;

namespace ExpTcpCommon
{
    public sealed class AutoStopwatch: IDisposable
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private long _totalData;

        public AutoStopwatch(long total = -1)
        {
            _totalData = total;
            _stopwatch.Start();
        }

        public void Restart(long newLength = -1)
        {
            if (newLength > 0)
                _totalData = newLength;

            _stopwatch.Restart();
        }

        public string Report()
        {
            var result = "INF";
            var elapses = _stopwatch.Elapsed.TotalSeconds;

            if (_totalData <= 0)
            {
                return $"Time: {elapses} s";
            }

            if (elapses > 0)
            {
                var speed = (long)(_totalData / elapses);
                result = speed.ToFileSize();
            }
            
            return $"Speed: {result}/s";
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Console.WriteLine(Report());
        }
    }
}
