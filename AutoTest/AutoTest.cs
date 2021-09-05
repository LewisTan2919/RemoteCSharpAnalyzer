///////////////////////////////////////////////////////////////////////
// Executive.cs - Demonstrate Prototype Code Analyzer                //
// ver 2.0                                                           //
// Language:    C#, 2018, .Net Framework 4.7.1                       //
// Platform:    Dell Precision T8900, Win10                          //
// Application: Demonstration for CSE681, Project #3, Fall 2018      //
// Author:      Xiao Tan,               Syracuse University          //
//              (315) 450-9913, xtan19@syr.edu                       //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 */
/* Required Files:
 * Maintenance History:
 * --------------------
 * ver 1.0 : 5 Dec 2018
 */
using CodeAnalysis;
using Common;
using CsGraph;
using Lexer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTest
{
    public class Runtst
    {
        
        static void Main(string[] args)
        {
            common.output.Append("\r\n------------------------------------------------------------------------------------------------");
            common.output.Append("\r\n  If you are the first time to press the AutoTest Button, please press it least 3 times to make it work");
            common.output.Append("\r\n  Because there are some functions are postponed  for some reason\r\n");
            common.output.Append("\r\n  If this program broke, please run it again");
            common.output.Append("\r\n------------------------------------------------------------------------------------------------");
            common.output.Append("\r\n  The Auto Test mainly Do auto operations");
            common.output.Append("\r\n  1.connect the client and server");
            common.output.Append("\r\n  2.add all the \".cs\"  files to the \"Selected Files\" field   ");
            common.output.Append("\r\n  3.run the Dependency Analysis,and show them on the result");
            common.output.Append("\r\n  4.run the StrongComponent Analysis,and show them on the result");
            common.output.Append("\r\n ");
            common.output.Append("\r\n ");



            common.output.Append("\r\n++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            common.output.Append("\r\n                                                        GUI Guide                                             ");
            common.output.Append("\r\n++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            common.output.Append("\r\n1.remote Dirs \"up\" button to get to the ServerFiles dir");
            common.output.Append("\r\n2.double Click the remote files to select them as target files, send their filename and show them on \"selectfiles\" field   ");
            common.output.Append("\r\n3.\"add all\" button choose all the files with \".cs\" to server ");
            common.output.Append("\r\n4.\"clear\" button to clean all the selected files ");
            common.output.Append("\r\n5.double click the selcted files to delete the files from the target files, and remove it from the \"selectfiles\" field ");
            common.output.Append("\r\n6.\"Whole Analysis\" button start a whole analysis on the selected files");
            common.output.Append("\r\n7.\"Dependencies\" and \"StrongComponent\" button show the result of them");
            common.output.Append("\r\n8.\"AutoTest\" button to operate to meet the requirement");








            string path = Path.GetFullPath("Result.txt");
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(common.output.ToString());
            common.output.Clear();
            sw.Close();
            fs.Close();
            

        }
    }
}
