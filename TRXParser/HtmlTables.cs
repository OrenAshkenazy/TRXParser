using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;


public partial class HtmlTable : System.Web.UI.Page
{


    

     public static void ConvertDataTableToHTML(List<string> TestNames, List<string>Results, List<string> Categories, List<string> ErrorMessages, List<string>StackTraces, string HtmlFileName, string StartTime, string EndTime, string TotalTime)
     {


         // Intialize number of possible result statuses 

         int numOfPassed = 0;
         int numOfFailed = 0;
         int numOfTimedOut = 0;

         // Get result occurrences  (e.g - Passed, Failed, Timeout)

         Dictionary<String, int> freqs = Results.GroupBy(item => item)
   .ToDictionary(item => item.Key,
                 item => item.Count());

         int value;

         if (freqs.TryGetValue("Passed", out value))

         {
             numOfPassed = freqs["Passed"];
         }

         if (freqs.TryGetValue("Failed", out value))
         {
             numOfFailed = freqs["Failed"];
         }


         if (freqs.TryGetValue("Timeout", out value))

         {

             numOfTimedOut = freqs["Timeout"];
         
         }
         
         string DT = DateTime.Now.ToString("dd/MM/yyyy");

         StringBuilder sb = new StringBuilder();
         sb.AppendLine("<html>");
         sb.AppendLine("<head>");

         #region Google Charts API (For more info on how to customize google charts https://developers.google.com/chart/interactive/docs/quick_start

         sb.AppendLine(@"<script type=""text/javascript"" src=""https://www.google.com/jsapi""></script>");
         sb.AppendLine(@"<script type=""text/javascript"">");
         sb.AppendLine(@"google.load('visualization', '1.0', { 'packages': ['corechart'] });");
         sb.AppendLine(@"google.setOnLoadCallback(drawChart);");
         sb.AppendLine(@" function drawChart() {");
         sb.AppendLine(@"var data = new google.visualization.DataTable();");
         sb.AppendLine(@"data.addColumn('string', 'Results');");
         sb.AppendLine(@"data.addColumn('number', 'Results');");
         sb.AppendLine(@" data.addRows([
              ['Passed', " + numOfPassed + @"],
              ['Failed', " + numOfFailed + @"],
              ['Timeout', " + numOfTimedOut + @"],
             
            ]);");

         sb.AppendLine(@" var options = {
                'title': 'Test Report Date " + DT + @" ',
                'width': 400,
                'height': 300
            };");
         sb.AppendLine("var chart = new google.visualization.PieChart(document.getElementById('chart_div'));");
         sb.AppendLine("chart.draw(data, options);}");
         sb.AppendLine("</script>");
#endregion


         sb.AppendLine("<title>Test Report Date: " + DT + " </title>");

         #region HTML Table CSS
         sb.AppendLine(@"<style> body {
    width: 900px;
    margin: 40px auto;
    font-family: 'trebuchet MS', 'Lucida sans', Arial;
    font-size: 14px;
    color: #444;
}

table {
    *border-collapse: collapse; /* IE7 and lower */
    border-spacing: 0;
    width: 100%;    
    margin-top: 50px;
}

.bordered {
  
    border: solid #ccc 1px;
    -moz-border-radius: 6px;
    -webkit-border-radius: 6px;
    border-radius: 6px;
    -webkit-box-shadow: 0 1px 1px #ccc; 
    -moz-box-shadow: 0 1px 1px #ccc; 
    box-shadow: 0 1px 1px #ccc;         
}

.bordered tr:hover {
    background: #fbf8e9;
    -o-transition: all 0.1s ease-in-out;
    -webkit-transition: all 0.1s ease-in-out;
    -moz-transition: all 0.1s ease-in-out;
    -ms-transition: all 0.1s ease-in-out;
    transition: all 0.1s ease-in-out;     
}    
    
.bordered td, .bordered th {
    border-left: 1px solid #ccc;
    border-top: 1px solid #ccc;
    padding: 10px;
    text-align: left;    
}

.bordered th {
    background-color: #dce9f9;
    background-image: -webkit-gradient(linear, left top, left bottom, from(#ebf3fc), to(#dce9f9));
    background-image: -webkit-linear-gradient(top, #ebf3fc, #dce9f9);
    background-image:    -moz-linear-gradient(top, #ebf3fc, #dce9f9);
    background-image:     -ms-linear-gradient(top, #ebf3fc, #dce9f9);
    background-image:      -o-linear-gradient(top, #ebf3fc, #dce9f9);
    background-image:         linear-gradient(top, #ebf3fc, #dce9f9);
    -webkit-box-shadow: 0 1px 0 rgba(255,255,255,.8) inset; 
    -moz-box-shadow:0 1px 0 rgba(255,255,255,.8) inset;  
    box-shadow: 0 1px 0 rgba(255,255,255,.8) inset;        
    border-top: none;
    text-shadow: 0 1px 0 rgba(255,255,255,.5); 
}

.bordered td:first-child, .bordered th:first-child {
    border-left: none;
}

.bordered th:first-child {
    -moz-border-radius: 6px 0 0 0;
    -webkit-border-radius: 6px 0 0 0;
    border-radius: 6px 0 0 0;
}

.bordered th:last-child {
    -moz-border-radius: 0 6px 0 0;
    -webkit-border-radius: 0 6px 0 0;
    border-radius: 0 6px 0 0;
}

.bordered th:only-child{
    -moz-border-radius: 6px 6px 0 0;
    -webkit-border-radius: 6px 6px 0 0;
    border-radius: 6px 6px 0 0;
}

.bordered tr:last-child td:first-child {
    -moz-border-radius: 0 0 0 6px;
    -webkit-border-radius: 0 0 0 6px;
    border-radius: 0 0 0 6px;
}

.bordered tr:last-child td:last-child {
    -moz-border-radius: 0 0 6px 0;
    -webkit-border-radius: 0 0 6px 0;
    border-radius: 0 0 6px 0;
}



/*----------------------*/

.zebra td, .zebra th {
    padding: 10px;
    border-bottom: 1px solid #f2f2f2;    
}

.zebra tbody tr:nth-child(even) {
    background: #f5f5f5;
    -webkit-box-shadow: 0 1px 0 rgba(255,255,255,.8) inset; 
    -moz-box-shadow:0 1px 0 rgba(255,255,255,.8) inset;  
    box-shadow: 0 1px 0 rgba(255,255,255,.8) inset;        
}

.zebra th {
    text-align: left;
    text-shadow: 0 1px 0 rgba(255,255,255,.5); 
    border-bottom: 1px solid #ccc;
    background-color: #eee;
    background-image: -webkit-gradient(linear, left top, left bottom, from(#f5f5f5), to(#eee));
    background-image: -webkit-linear-gradient(top, #f5f5f5, #eee);
    background-image:    -moz-linear-gradient(top, #f5f5f5, #eee);
    background-image:     -ms-linear-gradient(top, #f5f5f5, #eee);
    background-image:      -o-linear-gradient(top, #f5f5f5, #eee); 
    background-image:         linear-gradient(top, #f5f5f5, #eee);
}

.zebra th:first-child {
    -moz-border-radius: 6px 0 0 0;
    -webkit-border-radius: 6px 0 0 0;
    border-radius: 6px 0 0 0;  
}

.zebra th:last-child {
    -moz-border-radius: 0 6px 0 0;
    -webkit-border-radius: 0 6px 0 0;
    border-radius: 0 6px 0 0;
}

.zebra th:only-child{
    -moz-border-radius: 6px 6px 0 0;
    -webkit-border-radius: 6px 6px 0 0;
    border-radius: 6px 6px 0 0;
}

.zebra tfoot td {
    border-bottom: 0;
    border-top: 1px solid #fff;
    background-color: #f1f1f1;  
}

.zebra tfoot td:first-child {
    -moz-border-radius: 0 0 0 6px;
    -webkit-border-radius: 0 0 0 6px;
    border-radius: 0 0 0 6px;
}

.zebra tfoot td:last-child {
    -moz-border-radius: 0 0 6px 0;
    -webkit-border-radius: 0 0 6px 0;
    border-radius: 0 0 6px 0;
}

.zebra tfoot td:only-child{
    -moz-border-radius: 0 0 6px 6px;
    -webkit-border-radius: 0 0 6px 6px
    border-radius: 0 0 6px 6px
}
  
    .auto-style1 {
        width: 208px;
    }
    .auto-style3 {
        text-align: left;
        direction: ltr;
        font-family: Calibri;
        color: black;
        font-weight: bold;
        vertical-align: baseline;
        font-size: medium;
        margin-left: 0in;
        margin-top: 0pt;
        margin-bottom: 0pt;
    }
    .auto-style4 {
        width: 179px;
    }
    .auto-style5 {
        width: 200px;
    }
  
    .auto-style6 {
        width: 179px;
        height: 39px;
    }
    .auto-style7 {
        width: 208px;
        height: 39px;
    }
    .auto-style8 {
        width: 200px;
        height: 39px;
    }
  .auto-style9 {
        background-color: #00FF00;
        font-weight :bold;
    }
  .auto-style10 {
        background-color: #FF4A4A;
        font-weight :bold;
    }

 .auto-style11 {
        background-color: #FF6600;
        font-weight :bold;
    }
  
</style>");

#endregion
         sb.AppendLine("</head>");


         sb.AppendLine("<body>");
         sb.AppendLine(@"<div id=""chart_div""></div>");
         sb.AppendLine(@"<table class=""bordered""><thead>");
         sb.AppendLine("<tr>");
         sb.AppendLine(@"<th class=""auto-style4"">");
         sb.AppendLine(@"<p class=""auto-style3");
         sb.AppendLine(@"""style=""language: en-US; unicode-bidi: embed; mso-line-break-override: none; word-break: normal; punctuation-wrap: hanging; mso-ascii-font-family: Calibri; mso-bidi-font-family: +mn-cs; mso-ascii-theme-font: minor-latin; mso-fareast-theme-font: minor-fareast; mso-bidi-theme-font: minor-bidi; mso-color-index: 14; mso-font-kerning: 12.0pt; mso-text-raise: 0%; mso-style-textfill-type: solid; mso-style-textfill-fill-themecolor: light1; mso-style-textfill-fill-color: white; mso-style-textfill-fill-alpha: 100.0%;"">");
         sb.AppendLine("Running Version</p>");
         sb.AppendLine(@"</th> <th class=""auto-style1"">");
         sb.AppendLine(@"<p style=""language:en-US;margin-top:0pt;margin-bottom:0pt;margin-left:0in;");
         sb.AppendLine(@"text-align:left;direction:ltr;unicode-bidi:embed;mso-line-break-override:none;");
         sb.AppendLine(@"word-break:normal;punctuation-wrap:hanging"">");
         sb.AppendLine(@"<span style=""font-size:medium;""");
         sb.AppendLine("font-family:Calibri;mso-ascii-font-family:Calibri;mso-fareast-font-family:+mn-ea;");
         sb.AppendLine("mso-bidi-font-family:+mn-cs;mso-ascii-theme-font:minor-latin;mso-fareast-theme-font:");
         sb.AppendLine("black; mso-bidi-theme-font:minor-bidi;color:white;mso-color-index:14;");
         sb.AppendLine("mso-font-kerning:12.0pt;language:en-US;font-weight:bold;mso-style-textfill-type:");
         sb.AppendLine("solid;mso-style-textfill-fill-themecolor:light1;mso-style-textfill-fill-color:");
         sb.AppendLine(@"white;mso-style-textfill-fill-alpha:100.0%"">Current date</span></p>");
         sb.AppendLine(@"<th class=""auto-style5"">Start date</th>");
         sb.AppendLine(@"<th class=""auto-style5"">End date</th>");
         sb.AppendLine(@"<th class=""auto-style5"">Duration</th>");
         sb.AppendLine("</tr></thead>");
         sb.AppendLine(string.Format(@"<tr><td>{0}</td>",
                HttpUtility.HtmlEncode("")));
         sb.AppendLine(string.Format(@"<td>{0}</td>",
         HttpUtility.HtmlEncode(DT)));
         sb.AppendLine(string.Format(@"<td>{0}</td>",
         HttpUtility.HtmlEncode(StartTime)));
         sb.AppendLine(string.Format(@"<td>{0}</td>",
         HttpUtility.HtmlEncode(EndTime)));
         sb.AppendLine(string.Format(@"<td>{0}</td></tr>",
         HttpUtility.HtmlEncode(TotalTime)));
         sb.AppendLine("</table>");
         sb.AppendLine(@"<table class=""bordered""><thead>");
         sb.AppendLine("<tr>");
         sb.AppendLine(@"<th class=""auto-style4"">");
         sb.AppendLine(@"<p class=""auto-style3");
         sb.AppendLine(@"""style=""language: en-US; unicode-bidi: embed; mso-line-break-override: none; word-break: normal; punctuation-wrap: hanging; mso-ascii-font-family: Calibri; mso-bidi-font-family: +mn-cs; mso-ascii-theme-font: minor-latin; mso-fareast-theme-font: minor-fareast; mso-bidi-theme-font: minor-bidi; mso-color-index: 14; mso-font-kerning: 12.0pt; mso-text-raise: 0%; mso-style-textfill-type: solid; mso-style-textfill-fill-themecolor: light1; mso-style-textfill-fill-color: white; mso-style-textfill-fill-alpha: 100.0%;"">");
         sb.AppendLine("Test name</p>");
         sb.AppendLine(@"</th> <th class=""auto-style1"">");
         sb.AppendLine(@"<p style=""language:en-US;margin-top:0pt;margin-bottom:0pt;margin-left:0in;");
         sb.AppendLine(@"text-align:left;direction:ltr;unicode-bidi:embed;mso-line-break-override:none;");
         sb.AppendLine(@"word-break:normal;punctuation-wrap:hanging"">");
         sb.AppendLine(@"<span style=""font-size:medium;""");
         sb.AppendLine("font-family:Calibri;mso-ascii-font-family:Calibri;mso-fareast-font-family:+mn-ea;");
         sb.AppendLine("mso-bidi-font-family:+mn-cs;mso-ascii-theme-font:minor-latin;mso-fareast-theme-font:");
         sb.AppendLine("black; mso-bidi-theme-font:minor-bidi;color:white;mso-color-index:14;");
         sb.AppendLine("mso-font-kerning:12.0pt;language:en-US;font-weight:bold;mso-style-textfill-type:");
         sb.AppendLine("solid;mso-style-textfill-fill-themecolor:light1;mso-style-textfill-fill-color:");
         sb.AppendLine(@"white;mso-style-textfill-fill-alpha:100.0%"">Result</span></p>");
         sb.AppendLine(" </th>");
         sb.AppendLine(@"<th class=""auto-style5"">Test Category</th>");
         sb.AppendLine(@"<th class=""auto-style6"">Error Message</th>");
         sb.AppendLine(@"<th class=""auto-style7"">Comments</th>");
         sb.AppendLine("</tr></thead>");


         for (int i = 0; i < TestNames.Count; ++i)
         {
            



             sb.AppendLine(string.Format(@"<tr><td>{0}</td>",
                 HttpUtility.HtmlEncode(TestNames[i])));

             if (Results[i] == "Timeout")

             {

                 sb.AppendLine(string.Format(@"<td class=""auto-style11"">{0}</td>",
                   HttpUtility.HtmlEncode(Results[i])));
             
             }


             if (Results[i] == "Failed")
             {
                 sb.AppendLine(string.Format(@"<td class=""auto-style10"">{0}</td>",
                   HttpUtility.HtmlEncode(Results[i])));
             }

             if (Results[i] == "Passed")

             {

                 sb.AppendLine(string.Format(@"<td class=""auto-style9"">{0}</td>",
                    HttpUtility.HtmlEncode(Results[i])));
             
             }
             sb.AppendLine(string.Format(@"<td>{0}</td>",
             HttpUtility.HtmlEncode(Categories[i])));



             if (Results[i] == "Failed")
             {
                 for (int j = 0; j < ErrorMessages.Count; j++)
                 {

                     sb.AppendLine(string.Format(@"<td>{0}</td>",
                     HttpUtility.HtmlEncode(ErrorMessages[j])));

                     sb.AppendLine(string.Format(@"<td>{0}</td>",
                     HttpUtility.HtmlEncode("")));

                     //sb.AppendLine(string.Format("<td>\"{0}\"</td>",
                     //HttpUtility.HtmlEncode(StackTraces[j])));

                     ErrorMessages.RemoveAt(j);
                     break;
                 }


             }

             else

             {

                 // Create an empty row for passed test case (No Error Message)

                 sb.AppendLine(string.Format(@"<td>{0}</td>",
                        HttpUtility.HtmlEncode("")));

                 // Create an empty row for comments (Added Manually)


                 sb.AppendLine(string.Format(@"<td>{0}</td>",
                        HttpUtility.HtmlEncode("")));

             }
            
         }
         sb.AppendLine("</tr>");
         sb.AppendLine("</table>");
         sb.AppendLine("</body>");
         sb.AppendLine("</html>");
         string result = sb.ToString();

         using (FileStream fs = File.Create(HtmlFileName, 1024))
         {
             Byte[] info = new UTF8Encoding(true).GetBytes(result);
            

             fs.Write(info, 0, info.Length);
         }

         // Open the stream and read it back.
         using (StreamReader sr = File.OpenText(HtmlFileName))
         {
             string s = "";
             while ((s = sr.ReadLine()) != null)
             {
                 Console.WriteLine(s);
             }
         }


         Console.WriteLine("");
       
    }
    }
