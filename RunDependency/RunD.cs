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
 * This package defines the class:
 *  RunStr:
 *   - uses Parser, RulesAndActions, Semi, and Toker to perform type-based
 *     dependency analyzes
 */
/* Required Files:
 *   Executive.cs
 *   FileMgr.cs
 *   Parser.cs
 *   IRulesAndActions.cs, RulesAndActions.cs, ScopeStack.cs, Elements.cs
 *   ITokenCollection.cs, Semi.cs, Toker.cs
 *   Display.cs
 *   CsGraph.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.0 : 5 Dec 2018
 * its goal is to write down Dependencies result in file
 * - first release
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using CodeAnalysis;
using System.IO;
using CsGraph;
using Lexer;

namespace RunDependency
{
    class RunD
    {
        List<string> files { get; set; } = new List<string>();

        //----< process commandline to verify path >---------------------

        static List<string> ProcessCommandline(string[] args)
        {
            List<string> files = new List<string>();
            if (args.Length < 2)
            {

                common.output.Append("\r\n  Please enter path and file(s) to analyze\r\n\r\n");

                return files;
            }
            string path = args[0];
            if (!Directory.Exists(path))
            {
                common.output.Append("\r\n  invalid path \"" + System.IO.Path.GetFullPath(path) + "\"");




                return files;
            }
            path = Path.GetFullPath(path);
            for (int i = 1; i < args.Length; ++i)
            {
                string filename = Path.GetFileName(args[i]);
                files.AddRange(Directory.GetFiles(path, filename));
            }
            return files;
        }
        //----< show arguments on command line >-------------------------

        public static void ShowCommandLine(string[] args)
        {
            common.output.Append("\r\n  Commandline args are:\r\n  ");
            foreach (string arg in args)
            {
                common.output.Append("  " + arg);
            }
            common.output.Append("\r\n  current directory: " + System.IO.Directory.GetCurrentDirectory());
            common.output.Append("\r\n");
        }
        //----< build type table by parsing files for type defs >--------

        public void typeAnalysis(List<string> files)
        {
            


            ITokenCollection semi = Factory.create();
            BuildTypeAnalyzer builder = new BuildTypeAnalyzer(semi);
            Parser parser = builder.build();
            Repository repo = Repository.getInstance();

            foreach (string file in files)
            {
                if (file.Contains("TemporaryGeneratedFile"))
                    continue;
                if (!semi.open(file as string))
                {
                    //Common.output.Append(“\r\n  Can't open {0}\n\n", args[0]);
                    continue;
                }

                repo.currentFile = file;
                repo.locations.Clear();

                try
                {
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    common.output.Append("\r\n\r\n  " + ex.Message + "\n");
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
               
                common.output.Append("\r\n");

                semi.close();
            }
        }
        //----< build dependency table by parsing for type usage >-------

        public void dependencyAnalysis(List<string> files)
        {
            Repository repo = Repository.getInstance();
            ITokenCollection semi = Factory.create();
            BuildDepAnalyzer builder2 = new BuildDepAnalyzer(semi);
            Parser parser = builder2.build();
            repo.locations.Clear();

            foreach (string file in files)
            {
                //Common.output.Append(“\r\n  file: {0}", file);
                if (file.Contains("TemporaryGeneratedFile") || file.Contains("AssemblyInfo"))
                    continue;

                if (!semi.open(file as string))
                {
                    common.output.Append("\r\n  Can't open " + file + "\r\n\r\n");
                    break;
                }
                List<string> deps = new List<string>();
                repo.dependencyTable.addParent(file);

                repo.currentFile = file;

                try
                {
                    while (semi.get().Count > 0)
                    {
                        //semi.show();
                        parser.parse(semi);
                    }
                }
                catch (Exception ex)
                {
                    common.output.Append("\r\n\r\n  " + ex.Message + "\r\n");
                }
            }
        }
        //----< build dependency graph from dependency table >-----------

        CsGraph<string, string> buildDependencyGraph()
        {
            Repository repo = Repository.getInstance();

            CsGraph<string, string> graph = new CsGraph<string, string>("deps");
            foreach (var item in repo.dependencyTable.dependencies)
            {
                string fileName = item.Key;
                fileName = System.IO.Path.GetFileName(fileName);

                CsNode<string, string> node = new CsNode<string, string>(fileName);
                graph.addNode(node);
            }

            DependencyTable dt = new DependencyTable();
            foreach (var item in repo.dependencyTable.dependencies)
            {
                string fileName = item.Key;
                fileName = System.IO.Path.GetFileName(fileName);
                if (!dt.dependencies.ContainsKey(fileName))
                {
                    List<string> deps = new List<string>();
                    dt.dependencies.Add(fileName, deps);
                }
                foreach (var elem in item.Value)
                {
                    string childFile = elem;
                    childFile = System.IO.Path.GetFileName(childFile);
                    dt.dependencies[fileName].Add(childFile);
                }
            }

            foreach (var item in graph.adjList)
            {
                CsNode<string, string> node = item;
                List<string> children = dt.dependencies[node.name];
                foreach (var child in children)
                {
                    int index = graph.findNodeByName(child);
                    if (index != -1)
                    {
                        CsNode<string, string> dep = graph.adjList[index];
                        node.addChild(dep, "edge");
                    }
                }
            }
            return graph;
        }
        static void Main(string[] args)
        {

            RunD exec = new RunD();

            ShowCommandLine(args);
            List<string> files = ProcessCommandline(args);
            exec.typeAnalysis(files);
 

            Repository repo = Repository.getInstance();
            //repo.typeTable.show();
            common.output.Append("\r\n");
            common.output.Append("\r\n  Dependency Analysis: ");
            common.output.Append("\r\n----------------------");

            exec.dependencyAnalysis(files);


            CsGraph<string, string> graph = exec.buildDependencyGraph();
            graph.showDependencies();
            string path = Path.GetFullPath("Result.txt");
            FileStream fs = new FileStream(path, FileMode.Create);


            StreamWriter sw = new StreamWriter(fs);
            sw.Write(common.output.ToString());
            Console.Write(common.output.ToString());
            common.output.Clear();
            sw.Close();
            fs.Close();
        }
    }
}
