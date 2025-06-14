using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRXParser; // Namespace of the project under test
using System.IO;
using System.Linq;
using System; // For DateTime
using System.Globalization; // For CultureInfo in DateTime parsing for assertions

namespace TRXParser.Tests
{
    [TestClass]
    public class TrxFileParserTests
    {
        private static string _sampleTrxFilePath;
        private static TestRunDetails _parsedRunDetails;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Assuming Sample.trx is copied to the output directory of the test project
            _sampleTrxFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sample.trx");

            if (!File.Exists(_sampleTrxFilePath))
            {
                // Try one directory up if running in a nested output structure (e.g. /bin/Debug/netX.Y)
                string altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Sample.trx");
                 if(File.Exists(altPath))
                 {
                    _sampleTrxFilePath = altPath;
                 } else {
                    altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Sample.trx");
                    if (File.Exists(altPath))
                    {
                        _sampleTrxFilePath = altPath;
                    } else {
                         altPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Sample.trx");
                         if (File.Exists(altPath)) {
                            _sampleTrxFilePath = altPath;
                         } else {
                            throw new FileNotFoundException($"Sample.trx not found. Looked in {Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sample.trx")} and parent directories. Ensure it is copied to the output directory.", _sampleTrxFilePath);
                         }
                    }
                 }
            }

            TrxFileParser parser = new TrxFileParser();
            _parsedRunDetails = parser.Parse(_sampleTrxFilePath);
        }

        [TestMethod]
        public void Parse_Should_CorrectlyParseOverallRunTimes()
        {
            Assert.IsNotNull(_parsedRunDetails, "Parsed run details should not be null.");
            // Expected: <Times creation="2023-01-15T10:00:00.000Z" finish="2023-01-15T10:05:00.000Z" />
            DateTime expectedStartTime = DateTime.ParseExact("2023-01-15T10:00:00.000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            DateTime expectedEndTime = DateTime.ParseExact("2023-01-15T10:05:00.000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            TimeSpan expectedDuration = expectedEndTime - expectedStartTime;

            Assert.AreEqual(expectedStartTime, _parsedRunDetails.StartTime, "Start time does not match.");
            Assert.AreEqual(expectedEndTime, _parsedRunDetails.EndTime, "End time does not match.");
            Assert.AreEqual(expectedDuration, _parsedRunDetails.TotalDuration, "Total duration does not match.");
        }

        [TestMethod]
        public void Parse_Should_CorrectlyParseTestCounts()
        {
            Assert.IsNotNull(_parsedRunDetails, "Parsed run details should not be null.");
            Assert.AreEqual(4, _parsedRunDetails.TotalTests, "Total tests count does not match.");
            Assert.AreEqual(2, _parsedRunDetails.PassedCount, "Passed tests count does not match.");
            Assert.AreEqual(1, _parsedRunDetails.FailedCount, "Failed tests count does not match.");
            Assert.AreEqual(1, _parsedRunDetails.TimeoutCount, "Timeout tests count does not match.");
        }

        [TestMethod]
        public void Parse_Should_ParsePassingTestCorrectly()
        {
            var passingTest = _parsedRunDetails.TestResults.FirstOrDefault(r => r.TestName == "PassingTest1");
            Assert.IsNotNull(passingTest, "PassingTest1 not found.");
            Assert.AreEqual(TestOutcome.Passed, passingTest.Outcome, "PassingTest1 outcome should be Passed.");
            Assert.IsTrue(string.IsNullOrEmpty(passingTest.TestCategory), $"PassingTest1 category should be empty, but was '{passingTest.TestCategory}'.");
        }

        [TestMethod]
        public void Parse_Should_ParseFailingTestCorrectly()
        {
            var failingTest = _parsedRunDetails.TestResults.FirstOrDefault(r => r.TestName == "FailingTest1");
            Assert.IsNotNull(failingTest, "FailingTest1 not found.");
            Assert.AreEqual(TestOutcome.Failed, failingTest.Outcome, "FailingTest1 outcome should be Failed.");
            Assert.IsNotNull(failingTest.ErrorInfo, "FailingTest1 ErrorInfo should not be null.");
            Assert.AreEqual("Assertion failed: Expected true but was false.", failingTest.ErrorInfo.Message, "Error message does not match.");
            Assert.AreEqual("at TestProject.MyTests.FailingTest1() in C:\\path\\to\\tests.cs:line 42", failingTest.ErrorInfo.StackTrace, "Stack trace does not match.");
        }

        [TestMethod]
        public void Parse_Should_ParseTimeoutTestCorrectly()
        {
            var timeoutTest = _parsedRunDetails.TestResults.FirstOrDefault(r => r.TestName == "TimeoutTest1");
            Assert.IsNotNull(timeoutTest, "TimeoutTest1 not found.");
            Assert.AreEqual(TestOutcome.Timeout, timeoutTest.Outcome, "TimeoutTest1 outcome should be Timeout.");
        }

        [TestMethod]
        public void Parse_Should_ParseTestWithCategoriesCorrectly()
        {
            var categorizedTest = _parsedRunDetails.TestResults.FirstOrDefault(r => r.TestName == "TestWithCategory");
            Assert.IsNotNull(categorizedTest, "TestWithCategory not found.");
            Assert.AreEqual(TestOutcome.Passed, categorizedTest.Outcome, "TestWithCategory outcome should be Passed.");
            // Expected categories from Sample.trx: <Owner name="TeamA"/>, <TestCategoryItem TestCategory="SmokeTest" />, <TestCategoryItem TestCategory="UI" />
            // Parser joins them with ", "
            string[] expectedCategoriesArray = { "TeamA", "SmokeTest", "UI" };
            string[] actualCategoriesArray = categorizedTest.TestCategory.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            CollectionAssert.AreEquivalent(expectedCategoriesArray, actualCategoriesArray, "Categories do not match.");
        }

        [TestMethod]
        public void Parse_Should_SetHtmlFileNameCorrectly()
        {
            Assert.IsNotNull(_parsedRunDetails, "Parsed run details should not be null.");
            // With the correction, HtmlFileName should be "Sample.html"
            Assert.AreEqual("Sample.html", _parsedRunDetails.HtmlFileName, "HTML file name is not set correctly based on the TRX file name.");
        }

        [TestMethod]
        public void Parse_IndividualTestTimings_ShouldBeParsed()
        {
            var passingTest = _parsedRunDetails.TestResults.FirstOrDefault(r => r.TestName == "PassingTest1");
            Assert.IsNotNull(passingTest, "PassingTest1 not found for timing check.");
            // duration="00:00:01.500" startTime="2023-01-15T10:00:05.000Z" endTime="2023-01-15T10:00:06.500Z"
            DateTime expectedStartTime = DateTime.ParseExact("2023-01-15T10:00:05.000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            DateTime expectedEndTime = DateTime.ParseExact("2023-01-15T10:00:06.500Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            TimeSpan expectedDuration = TimeSpan.FromMilliseconds(1500);

            Assert.AreEqual(expectedStartTime, passingTest.StartTime, "PassingTest1 StartTime incorrect.");
            Assert.AreEqual(expectedEndTime, passingTest.EndTime, "PassingTest1 EndTime incorrect.");
            Assert.AreEqual(expectedDuration, passingTest.Duration, "PassingTest1 Duration incorrect.");

            var failingTest = _parsedRunDetails.TestResults.FirstOrDefault(r => r.TestName == "FailingTest1");
            Assert.IsNotNull(failingTest, "FailingTest1 not found for timing check.");
            // duration="00:00:02.000" startTime="2023-01-15T10:01:00.000Z" endTime="2023-01-15T10:01:02.000Z"
            expectedStartTime = DateTime.ParseExact("2023-01-15T10:01:00.000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            expectedEndTime = DateTime.ParseExact("2023-01-15T10:01:02.000Z", "o", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            expectedDuration = TimeSpan.FromSeconds(2);

            Assert.AreEqual(expectedStartTime, failingTest.StartTime, "FailingTest1 StartTime incorrect.");
            Assert.AreEqual(expectedEndTime, failingTest.EndTime, "FailingTest1 EndTime incorrect.");
            Assert.AreEqual(expectedDuration, failingTest.Duration, "FailingTest1 Duration incorrect.");
        }
    }
}
