using System;
using System.IO;
using System.Text;
using System.Web; // For HttpUtility.HtmlEncode, might need to ensure this assembly is referenced if not by default in a console app.
                 // If System.Web is problematic, consider System.Net.WebUtility.HtmlEncode (available in .NET Framework 4.5+ and .NET Core/5+)
                 // For older .NET Framework, HttpUtility is common.
using System.Linq; // Added for TestResults.Any() check, though not strictly necessary if testRun.TestResults is guaranteed non-null

namespace TRXParser
{
    public class HtmlReportGenerator
    {
        public static void GenerateReport(TestRunDetails testRun, string templatePath, string cssPath, string jsChartPath, string outputPath)
        {
            string htmlTemplate;
            string cssContent;
            string jsChartContent;

            try {
                htmlTemplate = File.ReadAllText(templatePath);
                // cssContent = File.ReadAllText(cssPath); // CSS is linked, not embedded
                // jsChartContent = File.ReadAllText(jsChartPath); // JS is linked, not embedded
            }
            catch(IOException ex) {
                Console.WriteLine($"Error reading template file: {ex.Message}");
                throw; // Re-throw to be caught by Program.cs for user feedback
            }

            // Prepare data for chart script injection
            string chartDataInjection = $@"const passedCount = {testRun.PassedCount};
const failedCount = {testRun.FailedCount};
const timeoutCount = {testRun.TimeoutCount};
const reportDate = ""{DateTime.Now:dd/MM/yyyy HH:mm}"";";
            // Note: Other outcomes like Aborted, NotExecuted could be added to the chart if desired.

            htmlTemplate = htmlTemplate.Replace("{{ChartScriptDataInjection}}", chartDataInjection);

            // Populate general information
            htmlTemplate = htmlTemplate.Replace("{{ReportDate}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
            htmlTemplate = htmlTemplate.Replace("{{StartTime}}", testRun.StartTime != DateTime.MinValue ? testRun.StartTime.ToString("yyyy-MM-dd HH:mm:ss UTC") : "N/A");
            htmlTemplate = htmlTemplate.Replace("{{EndTime}}", testRun.EndTime != DateTime.MinValue ? testRun.EndTime.ToString("yyyy-MM-dd HH:mm:ss UTC") : "N/A");
            htmlTemplate = htmlTemplate.Replace("{{TotalDuration}}", (testRun.TotalDuration != TimeSpan.Zero) ? $"{(int)testRun.TotalDuration.TotalHours}h {testRun.TotalDuration.Minutes}m {testRun.TotalDuration.Seconds}s" : "N/A");

            htmlTemplate = htmlTemplate.Replace("{{TotalTests}}", testRun.TotalTests.ToString());
            htmlTemplate = htmlTemplate.Replace("{{PassedCount}}", testRun.PassedCount.ToString());
            htmlTemplate = htmlTemplate.Replace("{{FailedCount}}", testRun.FailedCount.ToString());
            htmlTemplate = htmlTemplate.Replace("{{TimeoutCount}}", testRun.TimeoutCount.ToString());
            // Consider adding other counts like Aborted, NotExecuted to the summary paragraph if they are parsed and significant

            // Build test results table
            StringBuilder testResultsHtml = new StringBuilder();
            if (testRun.TestResults != null && testRun.TestResults.Any())
            {
                foreach (var result in testRun.TestResults)
                {
                    testResultsHtml.AppendLine("<tr>");
                    testResultsHtml.AppendLine($"  <td>{HttpUtility.HtmlEncode(result.TestName ?? string.Empty)}</td>");
                    testResultsHtml.AppendLine($"  <td class=\"result-{result.Outcome.ToString().ToLowerInvariant()}\">{HttpUtility.HtmlEncode(result.Outcome.ToString())}</td>");
                    testResultsHtml.AppendLine($"  <td>{HttpUtility.HtmlEncode(result.TestCategory ?? string.Empty)}</td>");
                    testResultsHtml.AppendLine($"  <td>{result.Duration.TotalSeconds:F2}</td>"); // Duration in seconds

                    if (result.Outcome == TestOutcome.Failed && result.ErrorInfo != null)
                    {
                        testResultsHtml.AppendLine($"  <td><pre>{HttpUtility.HtmlEncode(result.ErrorInfo.Message ?? string.Empty)}</pre></td>");
                        testResultsHtml.AppendLine($"  <td><pre>{HttpUtility.HtmlEncode(result.ErrorInfo.StackTrace ?? string.Empty)}</pre></td>");
                    }
                    else
                    {
                        testResultsHtml.AppendLine("  <td></td>"); // Empty cell for error message
                        testResultsHtml.AppendLine("  <td></td>"); // Empty cell for stack trace
                    }
                    testResultsHtml.AppendLine("</tr>");
                }
            }
            else
            {
                testResultsHtml.AppendLine("<tr><td colspan=\"6\">No test results found.</td></tr>");
            }
            htmlTemplate = htmlTemplate.Replace("{{TestResultsTable}}", testResultsHtml.ToString());

            try {
                File.WriteAllText(outputPath, htmlTemplate);

                string outputDir = Path.GetDirectoryName(outputPath);
                if (outputDir != null) {
                    // Ensure target directory exists for copying resources, though for outputPath it should exist.
                    // For cssPath and jsChartPath, they are relative to baseDirectory in Program.cs
                    // So, Path.GetFileName is sufficient.
                    string cssFileName = Path.GetFileName(cssPath);
                    string jsChartFileName = Path.GetFileName(jsChartPath);

                    File.Copy(cssPath, Path.Combine(outputDir, cssFileName), true);
                    File.Copy(jsChartPath, Path.Combine(outputDir, jsChartFileName), true);
                }
            }
            catch(IOException ex) {
                 Console.WriteLine($"Error writing HTML report or copying resources: {ex.Message}");
                 throw; // Re-throw
            }
        }
    }
}
