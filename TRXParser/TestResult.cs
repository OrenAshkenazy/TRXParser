using System;

namespace TRXParser
{
    public class TestResult
    {
        public string TestName { get; set; }
        public TestOutcome Outcome { get; set; }
        public string TestCategory { get; set; }
        public ErrorDetails ErrorInfo { get; set; } // Null if not failed
        public TimeSpan Duration { get; set; } // Optional: if available per test
        public DateTime StartTime { get; set; } // Optional: if available per test
        public DateTime EndTime { get; set; } // Optional: if available per test
    }

    public enum TestOutcome
    {
        Passed,
        Failed,
        Timeout,
        Aborted,
        NotExecuted,
        Warning, // Or other relevant outcomes from TRX
        Other
    }
}
