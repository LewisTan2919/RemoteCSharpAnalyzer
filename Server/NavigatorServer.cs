////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //
// ver 2.0                                                                //
// Xiao Tan   , CSE681 - Software Modeling and Analysis, Dec 5 2018       //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * Maintanence History:
 * --------------------
 * ver 2.0 - 24 Oct 2017
 * - added message dispatcher which works very well - see below
 * - added these comments
 * ver 1.0 - 22 Oct 2017
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePassingComm;
using SpawnProc;
using System.IO;

namespace Navigator
{
    public class NavigatorServer
    {
        IFileMgr localFileMgr { get; set; } = null;
        Comm comm { get; set; } = null;
        static List<string> targetFiles = new List<string>();

        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
          new Dictionary<string, Func<CommMessage, CommMessage>>();

        /*----< initialize server processing >-------------------------*/

        public NavigatorServer()
        {
            initializeEnvironment();
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
        }
        /*----< set Environment properties needed by server >----------*/

        void initializeEnvironment()
        {
            Environment.root = ServerEnvironment.root;
            Environment.address = ServerEnvironment.address;
            Environment.port = ServerEnvironment.port;
            Environment.endPoint = ServerEnvironment.endPoint;
        }
        /*----< define how each message will be processed >------------*/

        void initializeDispatcher()
        {
            Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopFiles"] = getTopFiles;

            Func<CommMessage, CommMessage> getTopDirs = (CommMessage msg) =>
            {
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "getTopDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["getTopDirs"] = getTopDirs;

            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;

            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;

            Func<CommMessage, CommMessage> SendMessage = (CommMessage msg) =>// recieve the msg with selected filename
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "SendMessage";
                reply.arguments = msg.arguments;
                return reply;
            };
            messageDispatcher["SendMessage"] = SendMessage;

            Func<CommMessage, CommMessage> ShowFiles = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = Path.Combine(ServerEnvironment.root,msg.arguments[0]);
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "ShowFiles";
                reply.arguments = msg.arguments;
                reply.content = File.ReadAllText(localFileMgr.currentPath as string);
                return reply;
            };
            messageDispatcher["ShowFiles"] = ShowFiles;

            Func<CommMessage, CommMessage> startAnalyzing = (CommMessage msg) =>//start the execution of a child process
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "startAnalyzing";
                targetFiles.Clear();
                targetFiles = msg.arguments.ToList();
                call(targetFiles,msg.Atype);// call the child process, with filenames and analysis type
                return reply;
            };
            messageDispatcher["startAnalyzing"] = startAnalyzing;

            Func<CommMessage, CommMessage> Remove = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "Remove";
                reply.arguments = msg.arguments;
                targetFiles.Remove(msg.arguments[0].ToString());//remove the files

                    return reply;
            };
            messageDispatcher["Remove"] = Remove;

            Func<CommMessage, CommMessage> Clear = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "Clear";
                reply.arguments = msg.arguments;
                targetFiles.Clear();//clear all the files

                return reply;
            };
            messageDispatcher["Clear"] = Clear;

            Func<CommMessage, CommMessage> addall = (CommMessage msg) =>
            {
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "addall";
                reply.arguments = msg.arguments;
                targetFiles.Clear();
                for (int i = 0; i < msg.arguments.Count; i++)//add all the files with a ".cs", if already included,then do not add
                {
                    if (!targetFiles.Contains(msg.arguments[i].ToString()))
                        targetFiles.Add(msg.arguments[i].ToString());
                    else continue;
                }
                return reply;
            };
            messageDispatcher["addall"] = addall;


            Func<CommMessage, CommMessage> ShowFinalResult = (CommMessage msg) =>//show the final result
            {
                System.Threading.Thread.Sleep(1000);//wait for the child process to finishing writing the file "Result.txt"
                if (msg.arguments.Count() == 1)
                    localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "ShowFinalResult";
                StreamReader sr = new StreamReader("Result.txt", Encoding.Default);
                String line;
                StringBuilder st = new StringBuilder();
                while ((line = sr.ReadLine()) != null)
                {
                    st.Append(line.ToString());
                    st.Append("\n");
                }
                reply.content = st.ToString();// read all the content from "Result.txt" and pass them through msg.content
   
                sr.Close();
                return reply;
            };
            messageDispatcher["ShowFinalResult"] = ShowFinalResult;
        }


 
        //a function that choose to execute a child process
        public void call(List<string> targetFiles,string type)
        {
            Console.Title = "SpawnProc";
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkBlue;

            Console.Write("\n  Demo Parent Process");
            Console.Write("\n =====================");

            SpawnProc.SpawnProc sp = new SpawnProc.SpawnProc();
            string fileName="";
            //start different precess depending on the type
            if (type =="AnalylizeAll")
            fileName = "..\\..\\..\\DemoExecutive\\bin\\debug\\DemoExecutive.exe";
            if (type == "AnalylizeDep")
            fileName = "..\\..\\..\\RunDependency\\bin\\debug\\RunDependency.exe";
            if (type == "AnalylizeStr")
            fileName = "..\\..\\..\\RunStrongComponent\\bin\\debug\\RunStrongComponent.exe";
            if (type == "AutoTest")
            fileName = "..\\..\\..\\AutoTest\\bin\\debug\\AutoTest.exe";
            string absFileSpec = Path.GetFullPath(fileName);
            sp.Clear();
            for (int i = 0; i < targetFiles.Count; i++)//add all the selected files to commanding line
            {
                if (targetFiles[i].ToString().Contains('\\'))// if the file has other parent dir, we need to emphasis the information
                {
                    string filename = Path.GetFileName(targetFiles[i].ToString());
                    string directory = targetFiles[i].ToString().Replace(filename,"");
      
                        sp.addPath(directory+" ");
                    sp.addPath(filename);
                }
        
                else sp.addPath(" "+Path.GetFileName(targetFiles[i].ToString()));
            }
            if (sp.createProcess(absFileSpec))// start the process
            {
                Console.Write(" - succeeded");
            }
            else
            {
                Console.Write(" - failed");
            }
            Console.Write("\n  Press key to exit");
            Console.Write("\n  ");
        }













        static void Main(string[] args)
        {




            TestUtilities.title("Starting Navigation Server", '=');
            try
            {
                NavigatorServer server = new NavigatorServer();
                server.initializeDispatcher();
                server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);

                while (true)
                {
                    CommMessage msg = server.comm.getMessage();
                    if (msg.type == CommMessage.MessageType.closeReceiver)
                        break;
                    msg.show();
                    if (msg.command == null)
                        continue;
                    CommMessage reply = server.messageDispatcher[msg.command](msg);
                    reply.show();
                    server.comm.postMessage(reply);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
            }

           
        }
    }
}
