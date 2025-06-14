using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace TRXParser
{
    public class TrxFileParser
    {
        public TestRunDetails Parse(string trxFilePath)
        {
            if (string.IsNullOrEmpty(trxFilePath))
            {
                throw new ArgumentNullException(nameof(trxFilePath), "TRX file path cannot be null or empty.");
            }

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(trxFilePath);
            }
            catch (Exception e)
            {
                throw new XmlException($"Could not load TRX file: {trxFilePath}. Encountered error: {e.Message}", e);
            }

            TestRunDetails testRunDetails = new TestRunDetails();
            testRunDetails.HtmlFileName = System.IO.Path.GetFileName(trxFilePath).Replace(".trx", ".html", StringComparison.OrdinalIgnoreCase);

            XmlNode timesNode = doc.SelectSingleNode("//TestRun/Times");
            if (timesNode?.Attributes["creation"] != null && timesNode?.Attributes["finish"] != null)
            {
                try
                {
                    testRunDetails.StartTime = DateTime.Parse(timesNode.Attributes["creation"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                    testRunDetails.EndTime = DateTime.Parse(timesNode.Attributes["finish"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                    testRunDetails.TotalDuration = testRunDetails.EndTime - testRunDetails.StartTime;
                }
                catch (FormatException fe)
                {
                    Console.WriteLine($"Warning: Could not parse overall run times from <Times> node: {fe.Message}. Attempting to derive from test results.");
                }
            }

            XmlNodeList unitTestResultNodes = doc.GetElementsByTagName("UnitTestResult");
            XmlNodeList testDefinitionNodes = doc.GetElementsByTagName("UnitTest");

            Dictionary<string, List<string>> testIdToCategories = new Dictionary<string, List<string>>();
            foreach (XmlNode testDefNode in testDefinitionNodes)
            {
                string testId = testDefNode.Attributes["id"]?.Value;
                if (!string.IsNullOrEmpty(testId))
                {
                    XmlNodeList categoryItems = testDefNode.SelectNodes("Owners/Owner | TestCategory/TestCategoryItem");
                     if (categoryItems != null && categoryItems.Count > 0)
                    {
                        if (!testIdToCategories.ContainsKey(testId))
                        {
                            testIdToCategories[testId] = new List<string>();
                        }
                        foreach (XmlNode catItem in categoryItems)
                        {
                            string categoryValue = catItem.Attributes["TestCategory"]?.Value ?? catItem.Attributes["Name"]?.Value;
                            if (!string.IsNullOrEmpty(categoryValue))
                            {
                                testIdToCategories[testId].Add(categoryValue);
                            }
                        }
                    }
                }
            }

            DateTime firstTestStartTime = DateTime.MaxValue;
            DateTime lastTestEndTime = DateTime.MinValue;

            foreach (XmlNode resultNode in unitTestResultNodes)
            {
                TestResult testResult = new TestResult
                {
                    TestName = resultNode.Attributes["testName"]?.Value,
                    Outcome = ParseTestOutcome(resultNode.Attributes["outcome"]?.Value)
                };

                string currentTestId = resultNode.Attributes["testId"]?.Value;
                if (!string.IsNullOrEmpty(currentTestId) && testIdToCategories.TryGetValue(currentTestId, out List<string> categories))
                {
                    testResult.TestCategory = string.Join(", ", categories.Distinct());
                }
                else
                {
                    testResult.TestCategory = string.Empty;
                }

                testResult.StartTime = GetDateTimeAttribute(resultNode, "startTime", DateTime.MinValue);
                testResult.EndTime = GetDateTimeAttribute(resultNode, "endTime", DateTime.MinValue);

                if (testResult.StartTime != DateTime.MinValue && testResult.StartTime < firstTestStartTime)
                {
                    firstTestStartTime = testResult.StartTime;
                }
                if (testResult.EndTime != DateTime.MinValue && testResult.EndTime > lastTestEndTime)
                {
                    lastTestEndTime = testResult.EndTime;
                }

                if (testResult.StartTime != DateTime.MinValue && testResult.EndTime != DateTime.MinValue && testResult.EndTime >= testResult.StartTime)
                {
                    testResult.Duration = testResult.EndTime - testResult.StartTime;
                }

                if (testResult.Outcome == TestOutcome.Failed)
                {
                    testResult.ErrorInfo = new ErrorDetails();
                    XmlNode outputNode = resultNode.SelectSingleNode("Output");
                    if (outputNode != null)
                    {
                        XmlNode errorInfoNode = outputNode.SelectSingleNode("ErrorInfo");
                        if (errorInfoNode != null)
                        {
                            testResult.ErrorInfo.Message = errorInfoNode.SelectSingleNode("Message")?.InnerText.Trim();
                            testResult.ErrorInfo.StackTrace = errorInfoNode.SelectSingleNode("StackTrace")?.InnerText.Trim();
                        }

                        if (string.IsNullOrEmpty(testResult.ErrorInfo.Message))
                        {
                            Match msgMatch = Regex.Match(outputNode.InnerXml, @"<Message>([\s\S]*?)<\/Message>");
                            if (msgMatch.Success) testResult.ErrorInfo.Message = msgMatch.Groups[1].Value.Trim();
                        }
                        if (string.IsNullOrEmpty(testResult.ErrorInfo.StackTrace))
                        {
                            Match stMatch = Regex.Match(outputNode.InnerXml, @"<StackTrace>([\s\S]*?)<\/StackTrace>");
                            if (stMatch.Success) testResult.ErrorInfo.StackTrace = stMatch.Groups[1].Value.Trim();
                        }
                    }
                }
                testRunDetails.TestResults.Add(testResult);
            }

            if (testRunDetails.StartTime == DateTime.MinValue && firstTestStartTime != DateTime.MaxValue)
            {
                testRunDetails.StartTime = firstTestStartTime;
            }
            if (testRunDetails.EndTime == DateTime.MinValue && lastTestEndTime != DateTime.MinValue)
            {
                testRunDetails.EndTime = lastTestEndTime;
            }
            if (testRunDetails.TotalDuration == TimeSpan.Zero && testRunDetails.StartTime != DateTime.MinValue && testRunDetails.EndTime != DateTime.MinValue && testRunDetails.EndTime >= testRunDetails.StartTime)
            {
                testRunDetails.TotalDuration = testRunDetails.EndTime - testRunDetails.StartTime;
            }

            return testRunDetails;
        }

        private TestOutcome ParseTestOutcome(string outcomeString)
        {
            if (string.IsNullOrEmpty(outcomeString)) return TestOutcome.Other;
            switch (outcomeString.ToLowerInvariant())
            {
                case "passed": return TestOutcome.Passed;
                case "failed": return TestOutcome.Failed;
                case "timeout": return TestOutcome.Timeout;
                case "aborted": return TestOutcome.Aborted;
                case "notexecuted": return TestOutcome.NotExecuted;
                case "disconnected": return TestOutcome.Aborted;
                case "inconclusive": return TestOutcome.Warning;
                case "warning": return TestOutcome.Warning;
                case "pending": return TestOutcome.Other;
                default:
                    Console.WriteLine($"Warning: Unknown test outcome '{outcomeString}'. Defaulting to 'Other'.");
                    return TestOutcome.Other;
            }
        }

        private DateTime GetDateTimeAttribute(XmlNode node, string attributeName, DateTime defaultValue)
        {
            XmlAttribute attr = node.Attributes[attributeName];
            if (attr != null && DateTime.TryParse(attr.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out DateTime val))
            {
                return val;
            }
            if (attr != null && DateTime.TryParse(attr.Value, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime valNoTz))
            {
                 Console.WriteLine($"Warning: DateTime attribute '{attributeName}' for test '{node.Attributes["testName"]?.Value}' was parsed without explicit timezone. Assuming UTC if not specified otherwise in source.");
                return valNoTz;
            }
            return defaultValue;
        }
    }
}
