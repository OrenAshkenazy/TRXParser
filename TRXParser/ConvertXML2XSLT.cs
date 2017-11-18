using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace ConsoleApplication27
{
    public class ConvertXML2XSLT
    {

        public static void XMLHelper()
        {
            List<string> TestCat = new List<string>();
            List<string> TestNameList = new List<string>();
            List<string> ResList = new List<string>();
            List<string> ErrMsgsList = new List<string>();
            List<string> STList = new List<string>();
            string category = string.Empty;
            string startTime = string.Empty;
            string endTime = string.Empty;
            string totalTime = string.Empty;
            string StartTimeStr = string.Empty;
            string EndTimeStr = string.Empty;
           

            XmlDocument doc = new XmlDocument();

            Console.Write("\n\nPlease type TRX file name: ");

            var tempInput = Console.ReadLine();

            string htmlFileName = tempInput.Replace("trx", "html");


            if (tempInput == string.Empty)
            {

                Console.Write("Could not find specified TRX file. Please try again: ");
            }

            else
            {

                doc.Load(tempInput);

                XmlNodeList xnList = doc.GetElementsByTagName("TestCategoryItem");

                // Get Test Result Node List
                XmlNodeList NL = doc.GetElementsByTagName("UnitTestResult");

                // get first start time value
                startTime = NL[0].Attributes["startTime"].Value;

                // get last end time value 
                var item = NL[NL.Count - 1];
                endTime = item.Attributes["endTime"].Value;
                DateTime EndTime = DateTime.Parse(endTime);
                DateTime StartTime = DateTime.Parse(startTime);

                // parse DateTime to hh:mm string format

                StartTimeStr = StartTime.ToString("hh:mm:ss");
                EndTimeStr = EndTime.ToString("hh:mm:ss");

                // get total time 

                TimeSpan sum = EndTime - StartTime;


                // parse DateTime to hh:mm string format

                totalTime = sum.ToString("h'h 'm'm 's's'");
             

                for (int i = 0; i < NL.Count; i++)
                {

                 string testName = NL[i].Attributes["testName"].Value;
                 string result = NL[i].Attributes["outcome"].Value;
                   
                    if (!xnList.Count.Equals(0))
                    {

                        try
                        {
                            category = xnList[i].Attributes["TestCategory"].Value;

                        }
                        catch (Exception)

                        {

                            category = "";

                        }
                    }


                    TestNameList.Add(testName);
                    ResList.Add(result);
                    TestCat.Add(category);


                    if (result == "Failed")
                    {

                        string error = NL[i].InnerXml;
                        string msgPattern = (@"<Message>[\s\S]*?<\/Message>");
                        string stPattern = (@"<StackTrace>[\s\S]*?<\/StackTrace>");

                        Match msgMatch = Regex.Match(error, msgPattern);
                        Match stMatch = Regex.Match(error, stPattern);

                        string errorMsg = msgMatch.Value;
                        string errorMsgFixed = errorMsg.Replace("<Message>", "").Replace("</Message>", "");

                        string stackTrace = stMatch.Value;
                        string stackTraceFixed = stackTrace.Replace("<StackTrace>", "").Replace("</StackTrace>", "");

                        ErrMsgsList.Add(errorMsgFixed);
                        STList.Add(stackTraceFixed);


                    }


                }

                HtmlTable.ConvertDataTableToHTML(TestNameList, ResList, TestCat, ErrMsgsList, STList, htmlFileName, StartTimeStr, EndTimeStr, totalTime);


            }

        }

    }

}