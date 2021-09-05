////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //
// ver 2.0                                                                //
// Xiao Tan   , CSE681 - Software Modeling and Analysis, Dec 5 2018       //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Instances of class SpawnProc start process by loading and executing
 * a specified application.  It provides a createProcess method that
 * accepts a fileSpecification, and an exit handler called childExited.
 * 
 * Required files:
 * ---------------
 * SpawnProc.cs
 * Application(s) to start
 * 
 * Maintenance History:
 * --------------------
 * ver 2.0 : 06 Nov 2018
 * - added exit event handler
 * ver 1.0 : 19 Oct 2017
 * - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SpawnProc
{
    class SpawnProc
    {
        List<Process> childProcs = new List<Process>();
        static StringBuilder _path = new StringBuilder("../../../ServerFiles/");
        
         



        public void childExited(object sender, System.EventArgs e)
        {
            Console.Write("\n  child process exited");
        }

        public void addPath(string fileName)//add the file name with dir to the path
        {
            _path.Append(fileName+" ");
        }

        public void Clear()
        {
            _path.Clear();
            _path.Append("../../../ServerFiles/");
        }

        public bool createProcess(string fileName)
        {
            Process proc = new Process();
            childProcs.Add(proc);
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = _path.ToString();//pass the commading lines
            proc.EnableRaisingEvents = true;
            //proc.Exited += new EventHandler(childExited);
            proc.Exited += childExited;

            Console.Write("\n  attempting to start {0}", fileName);
            try
            {
          
                proc.Start();
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }
        public static void startAnalyzing()
        {
            
        }
    }
}
