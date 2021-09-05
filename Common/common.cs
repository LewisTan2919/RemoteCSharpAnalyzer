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
 * acted as a global function to write down the result
 */
/* Required Files:
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 5 Dec 2018
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Common
{
    public static class common
    {
        public static StringBuilder output = new StringBuilder("");

        //define the regulated output with a right number of " "
        public static string print(string s)
        {
            
            int i = s.Length;
            StringBuilder a = new StringBuilder();
            if (i <= 10)
            {
                for (int j = 0; j < 10 - s.Length; j++)
                {
                    a.Append(" ");
                }
            }
            else if (i <= 25 && i >10)
            {
                for (int j = 0; j< 25 - s.Length; j++)
                {
                    a.Append(" ");
                }
            }
            else return "";
            return a.ToString();

        }

    }
}
