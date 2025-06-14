using System;
using System.Xml; // For XmlException
using System.IO;  // For Path, File, FileNotFoundException

namespace TRXParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || (args.Length == 1 && (args[0] == "-h" || args[0] == "--help" || args[0] == "/?")))
            {
                PrintUsage();
                return;
            }

            if (args.Length > 1)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Warning: Too many arguments provided. Only the first argument will be used as the TRX file path.");
                Console.ResetColor();
            }

            string trxFilePath = args[0];

            if (!File.Exists(trxFilePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: The file specified does not exist: {trxFilePath}");
                Console.ResetColor();
                PrintUsage();
                return;
            }

            if (!Path.GetExtension(trxFilePath).Equals(".trx", StringComparison.OrdinalIgnoreCase))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: The file specified does not have a .trx extension: {Path.GetFileName(trxFilePath)}");
                Console.ResetColor();
                // Decide if this should be a hard error or just a warning. For now, a warning.
            }

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                TrxFileParser parser = new TrxFileParser();
                TestRunDetails testRun = parser.Parse(trxFilePath);

                Console.WriteLine($"Parsing complete. Found {testRun.TotalTests} tests.");
                Console.WriteLine($"Passed: {testRun.PassedCount}, Failed: {testRun.FailedCount}, Timeout: {testRun.TimeoutCount}");
                if (testRun.StartTime != DateTime.MinValue)
                    Console.WriteLine($"Test Run Start Time: {testRun.StartTime:yyyy-MM-dd HH:mm:ss} UTC");
                if (testRun.EndTime != DateTime.MinValue)
                    Console.WriteLine($"Test Run End Time: {testRun.EndTime:yyyy-MM-dd HH:mm:ss} UTC");
                if (testRun.TotalDuration != TimeSpan.Zero)
                    Console.WriteLine($"Test Run Duration: {testRun.TotalDuration:g}");

                string templatePath = Path.Combine(baseDirectory, "ReportTemplate.html");
                string cssPath = Path.Combine(baseDirectory, "ReportStyles.css");
                string jsChartPath = Path.Combine(baseDirectory, "ReportChart.js");

                string outputHtmlPath = testRun.HtmlFileName; // This is now just the filename e.g. "MyRun.html"

                // Place output HTML in the same directory as the input TRX file by default
                string inputTrxDirectory = Path.GetDirectoryName(trxFilePath);
                if (string.IsNullOrEmpty(inputTrxDirectory))
                {
                    // If trxFilePath was just a name (e.g. "MyRun.trx" and CWD is where it is),
                    // Path.GetDirectoryName will be empty. Use Environment.CurrentDirectory.
                    inputTrxDirectory = Environment.CurrentDirectory;
                }
                outputHtmlPath = Path.Combine(inputTrxDirectory, outputHtmlPath);

                // Ensure the directory for the output HTML file exists
                string outputDirectory = Path.GetDirectoryName(outputHtmlPath);
                if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                HtmlReportGenerator.GenerateReport(testRun, templatePath, cssPath, jsChartPath, outputHtmlPath);

                Console.WriteLine($"HTML report generated: {outputHtmlPath}");
            }
            catch (FileNotFoundException fnfEx) // Catches issues with template/resource files primarily
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"File Error: {fnfEx.Message}.");
                Console.WriteLine("Ensure ReportTemplate.html, ReportStyles.css, and ReportChart.js are in the application directory:");
                Console.WriteLine($"- {Path.Combine(baseDirectory, "ReportTemplate.html")}");
                Console.WriteLine($"- {Path.Combine(baseDirectory, "ReportStyles.css")}");
                Console.WriteLine($"- {Path.Combine(baseDirectory, "ReportChart.js")}");
                Console.ResetColor();
            }
            catch (DirectoryNotFoundException dnfEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Directory Error: {dnfEx.Message}. Could not create directory for the report.");
                Console.ResetColor();
            }
            catch (ArgumentNullException anex) // Can be thrown by Path operations if a path is null
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Argument/Path Error: {anex.Message}");
                Console.ResetColor();
            }
            catch (XmlException xmlex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"TRX File Parsing Error: {xmlex.Message}");
                if (xmlex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {xmlex.InnerException.Message}");
                }
                Console.ResetColor();
            }
            catch (IOException ioEx) // General IO errors (e.g. disk full, permissions for writing report/copying resources)
            {
                 Console.ForegroundColor = ConsoleColor.Red;
                 Console.WriteLine($"IO Error: {ioEx.Message}");
                 Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                Console.ResetColor();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("\nTRX Test Result HTML Parser");
            Console.WriteLine("---------------------------");
            Console.WriteLine("Converts a .trx test result file into an HTML report.");
            Console.WriteLine("\nUsage: TRXParser <path_to_trx_file>");
            Console.WriteLine("   Example: TRXParser C:\\TestResults\\MyRun.trx");
            Console.WriteLine("   Example: TRXParser MyLocalRun.trx");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  -h, --help, /?    Show this help message.");
            Console.WriteLine("\nThe HTML report will be generated in the same directory as the input .trx file,");
            Console.WriteLine("with the same name but a .html extension.");
            Console.WriteLine("\nThe following template and resource files must be present in the application's directory:");
            // AppDomain.CurrentDomain.BaseDirectory can be long, especially in test/debug scenarios.
            // For user display, it might be cleaner to state "application's directory" or show relative paths if simple.
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"- {Path.Combine(appDir, "ReportTemplate.html")}");
            Console.WriteLine($"- {Path.Combine(appDir, "ReportStyles.css")}");
            Console.WriteLine($"- {Path.Combine(appDir, "ReportChart.js")}");
            Console.WriteLine();
        }
    }
}
