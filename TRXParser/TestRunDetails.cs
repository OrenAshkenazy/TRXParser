using System;
using System.Collections.Generic;
using System.Linq;

namespace TRXParser
{
    public class TestRunDetails
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public string HtmlFileName { get; set; }
        public List<TestResult> TestResults { get; set; } = new List<TestResult>();

        // Summary properties
        public int TotalTests => TestResults.Count;
        public int PassedCount => TestResults.Count(r => r.Outcome == TestOutcome.Passed);
        public int FailedCount => TestResults.Count(r => r.Outcome == TestOutcome.Failed);
        public int TimeoutCount => TestResults.Count(r => r.Outcome == TestOutcome.Timeout);
        // Add other outcomes as needed, e.g., Aborted, NotExecuted
    }
}
