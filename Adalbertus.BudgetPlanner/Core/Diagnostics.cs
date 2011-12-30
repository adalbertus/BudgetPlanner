using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Adalbertus.BudgetPlanner.Core
{
    public static class Diagnostics
    {
        private class DiagnosticStopwatch
        {
            public Stopwatch Stopwatch { get; private set; }
            public TimeSpan Elapsed { get { return Stopwatch.Elapsed; } }
            public string Description { get; set; }
            public string MethodName { get; set; }

            public static DiagnosticStopwatch GetDiagnosticStopwatch(string description = null, string methodName = null)
            {
                return new DiagnosticStopwatch
                {
                    Description = description,
                    Stopwatch = Stopwatch.StartNew(),
                    MethodName = methodName,
                };
            }

            public void Stop()
            {
                Stopwatch.Stop();
            }
        }

        private readonly static Stack<DiagnosticStopwatch> _stopwatches = new Stack<DiagnosticStopwatch>();

        private readonly static StringBuilder _logger = new StringBuilder();
        //private readonly static StackTrace _stackTrace = new StackTrace();

        public static void Start(string description = null)
        {
#if DEBUG
            var _stackTrace = new StackTrace();
            var frame = _stackTrace.GetFrame(1);
            var method = frame.GetMethod();
            var methodName = string.Format("{0}.{1}", method.DeclaringType.Name, method.Name);
            _stopwatches.Push(DiagnosticStopwatch.GetDiagnosticStopwatch(description, methodName));
#endif
        }

        public static TimeSpan Stop()
        {
#if DEBUG
            var currentStopwatch = _stopwatches.Pop();
            currentStopwatch.Stop();
            var elapsed = currentStopwatch.Elapsed;
            if (string.IsNullOrWhiteSpace(currentStopwatch.Description))
            {
                Log("{0}: {1}ms", currentStopwatch.MethodName, elapsed.TotalMilliseconds);
            }
            else
            {
                Log("{0}: {1}ms [{2}]", currentStopwatch.MethodName, elapsed.TotalMilliseconds, currentStopwatch.Description);
            }
            return elapsed;
#else
            return default(TimeSpan);
#endif
        }

        public static string GetLog(bool isMarkupEnabled = true)
        {
            if (isMarkupEnabled)
            {
                Log("###########################################");
            }
            return _logger.ToString();
        }

        public static string GetLogAndClear()
        {
            string log = GetLog();
            _logger.Clear();
            return log;
        }

        public static void ClearLog()
        {
            _logger.Clear();
        }

        public static void Log(string format, params object[] args)
        {
            _logger.AppendLine(string.Format(format, args));
        }
    }
}
