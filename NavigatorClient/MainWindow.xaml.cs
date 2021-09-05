////////////////////////////////////////////////////////////////////////////
// NavigatorClient.xaml.cs - Demonstrates Remote analysis  in WPF App     //
// ver 1.0                                                                //
// Xiao Tan   , CSE681 - Software Modeling and Analysis, Dec 8 2018       //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines WPF application processing by the client.  The client
 * displays a local FileFolder view, and a remote FileFolder view.  It supports
 * navigating into subdirectories, both locally and in the remote Server.
 * 
 *                            GUI Guide
 * ---------------------------------------------------------------------------
    //implementations:
    //1.remote Dirs "up" button to get to the ServerFiles dir
    //2.double Click the remote files to select them as target files, send their filename and show them on "selectfiles" field    
    //3. "add all" button choose all the files with ".cs" to server
    //4. "clear" button to clean all the selected files
    //5. double click the selcted files to delete the files from the target files, and remove it from the "selectfiles" field 
    //6."Whole Analysis" button start a whole analysis on the selected files
    //7 "Dependencies" and "StrongComponent" button show the result of them
    //8."AutoTest" button to operate to meet the requirement
    -->
 * 
 * Maintenance History:
 * --------------------
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MessagePassingComm;

namespace Navigator
{
    public partial class MainWindow : Window
    {
        private IFileMgr fileMgr { get; set; } = null;  // note: Navigator just uses interface declarations
        Comm comm { get; set; } = null;
        Dictionary<string, Action<CommMessage>> messageDispatcher = new Dictionary<string, Action<CommMessage>>();
        Thread rcvThread = null;

        public MainWindow()
        {
            InitializeComponent();
            initializeEnvironment();
            Console.Title = "remember to add files by \"add all\" or double click the file";
            fileMgr = FileMgrFactory.create(FileMgrType.Local); // uses Environment
            getTopFiles();
            comm = new Comm(ClientEnvironment.address, ClientEnvironment.port);
            initializeMessageDispatcher();
            rcvThread = new Thread(rcvThreadProc);
            rcvThread.Start();
        }
        //----< make Environment equivalent to ClientEnvironment >-------

        void initializeEnvironment()
        {
            Environment.root = ClientEnvironment.root;
            Environment.address = ClientEnvironment.address;
            Environment.port = ClientEnvironment.port;
            Environment.endPoint = ClientEnvironment.endPoint;
        }
        //----< define how to process each message command >-------------

        void initializeMessageDispatcher()
        {
            // load remoteFiles listbox with files from root

            messageDispatcher["getTopFiles"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
            };
            // load remoteDirs listbox with dirs from root

            messageDispatcher["getTopDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
            };
            // load remoteFiles listbox with files from folder

            messageDispatcher["moveIntoFolderFiles"] = (CommMessage msg) =>
            {
                remoteFiles.Items.Clear();
                foreach (string file in msg.arguments)
                {
                    remoteFiles.Items.Add(file);
                }
            };
            // load remoteDirs listbox with dirs from folder

            messageDispatcher["moveIntoFolderDirs"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    remoteDirs.Items.Add(dir);
                }
            };
            //output the selected Files in remote files

            messageDispatcher["SendMessage"] = (CommMessage msg) =>
            {
                //OutputResults.Items.Clear();
                foreach (string dir in msg.arguments)
                {
                    if (OutputResults.Items.Contains(dir))
                        continue;
                    OutputResults.Items.Add(dir);
                }
            };
            //show the files with a popup window

            messageDispatcher["ShowFiles"] = (CommMessage msg) =>
            {
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = msg.content;
                popup.Show();
            };

            messageDispatcher["startAnalyzing"] = (CommMessage msg) =>
            {

            };
            //show the ancestor path in remoteDirs

            messageDispatcher["ancestorPath"] = (CommMessage msg) =>
            {
                remoteDirs.Items.Clear();
                remoteDirs.Items.Add(msg.arguments[0]);

            };
            //remove a choosen item in the selected files

            messageDispatcher["Remove"] = (CommMessage msg) =>
            {
                OutputResults.Items.Remove(msg.arguments[0].ToString());

            };
            //remove all the items in the selected files

            messageDispatcher["Clear"] = (CommMessage msg) =>
            {
                Result.Text = null;
                OutputResults.Items.Clear();

            };
            //add all the ".cs" files in the remote dirs

            messageDispatcher["addall"] = (CommMessage msg) =>
            {
                for (int i = 0; i < msg.arguments.Count; i++)
                {
                    if(!OutputResults.Items.Contains(msg.arguments[i].ToString()))//if this output already have one item then do not add the item
                    OutputResults.Items.Add(msg.arguments[i].ToString());
                }

            };
            // show the result on a textbox

            messageDispatcher["ShowFinalResult"] = (CommMessage msg) =>
            {
                Result.Text = null;
                  Result.Text = msg.content;
            };


        }
        //----< define processing for GUI's receive thread >-------------

        void rcvThreadProc()
        {
            Console.Write("\n  starting client's receive thread");
            while (true)
            {
                CommMessage msg = comm.getMessage();
                msg.show();
                if (msg.command == null)
                    continue;

                // pass the Dispatcher's action value to the main thread for execution

                Dispatcher.Invoke(messageDispatcher[msg.command], new object[] { msg });
            }
        }
        //----< shut down comm when the main window closes >-------------

        private void Window_Closed(object sender, EventArgs e)
        {
            comm.close();

            // The step below should not be nessary, but I've apparently caused a closing event to 
            // hang by manually renaming packages instead of getting Visual Studio to rename them.

            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        //----< not currently being used >-------------------------------

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }
        //----< show files and dirs in root path >-----------------------

        public void getTopFiles()
        {
            List<string> files = fileMgr.getFiles().ToList<string>();
            localFiles.Items.Clear();
            foreach (string file in files)
            {
                localFiles.Items.Add(file);
            }
            List<string> dirs = fileMgr.getDirs().ToList<string>();
            localDirs.Items.Clear();
            foreach (string dir in dirs)
            {
                localDirs.Items.Add(dir);
            }
        }
        //----< move to directory root and display files and subdirs >---

        private void localTop_Click(object sender, RoutedEventArgs e)
        {
            fileMgr.currentPath = "";
            getTopFiles();
        }
        //----< show selected file in code popup window >----------------

        private void localFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = localFiles.SelectedValue as string;
            try
            {
                string path = System.IO.Path.Combine(ClientEnvironment.root, fileName);
                string contents = File.ReadAllText(path);
                CodePopUp popup = new CodePopUp();
                popup.codeView.Text = contents;
                popup.Show();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }
        //----< move to parent directory and show files and subdirs >----

        private void localUp_Click(object sender, RoutedEventArgs e)
        {
            if (fileMgr.currentPath == "")
                return;
            fileMgr.currentPath = fileMgr.pathStack.Peek();
            fileMgr.pathStack.Pop();
            getTopFiles();
        }
        //----< move into subdir and show files and subdirs >------------

        private void localDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string dirName = localDirs.SelectedValue as string;
            fileMgr.pathStack.Push(fileMgr.currentPath);
            fileMgr.currentPath = dirName;
            getTopFiles();
        }
        //----< move to root of remote directories >---------------------
        /*
         * - sends a message to server to get files from root
         * - recv thread will create an Action<CommMessage> for the UI thread
         *   to invoke to load the remoteFiles listbox
         */
        private void RemoteTop_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Xiao Tan";
            msg1.command = "getTopFiles";
            msg1.arguments.Add("");
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "getTopDirs";
            comm.postMessage(msg2);
        }
        //----< download file and display source in popup window >-------

        private void remoteFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string fileName = remoteFiles.SelectedValue as string;
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Xiao Tan";
            msg1.command = "SendMessage";
            msg1.arguments.Add(fileName);
            comm.postMessage(msg1);
;
        }
        //----< move to parent directory of current remote path >--------

        private void RemoteUp_Click(object sender, RoutedEventArgs e)
        {
       
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Xiao Tan";
            msg1.command = "getTopDirs";
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "getTopFiles";
            comm.postMessage(msg2);
        

        }
        //----< move into remote subdir and display files and subdirs >--
        /*
         * - sends messages to server to get files and dirs from folder
         * - recv thread will create Action<CommMessage>s for the UI thread
         *   to invoke to load the remoteFiles and remoteDirs listboxs
         */




        // double click the selsected value to send the file name to the remote server and add it as a target file in server, waiting for operations
        private void remoteDirs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "moveIntoFolderFiles";
            msg1.arguments.Add(remoteDirs.SelectedValue as string);// add the filename
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "moveIntoFolderDirs";// refresh both dirs and files
            comm.postMessage(msg2);
        }


        //Analysis Button that start a whole analysis on the selected files(including all the analysis),show the results on textbox
        private void Analysis_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Xiao Tan";
            msg1.command = "startAnalyzing";
            msg1.Atype = "AnalylizeAll";
            for(int i=0;i<OutputResults.Items.Count;i++)//send all the selected files
            {
                msg1.arguments.Add(OutputResults.Items[i].ToString());
            }
            comm.postMessage(msg1);
            CommMessage msg2= msg1.clone();
            msg2.command = "ShowFinalResult";
            comm.postMessage(msg2);


        }

        //delete files from the selected files
        private void OutputResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "Remove";
            msg1.arguments.Add(OutputResults.SelectedValue as string);
            comm.postMessage(msg1);

        }


        //clean all the selected files
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "Clear";
            msg1.arguments.Add(OutputResults.SelectedValue as string);
            comm.postMessage(msg1);
        }

        //add all the files with a ".cs"
        private void AddAll_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.command = "addall";
            for (int i = 0; i < remoteFiles.Items.Count; i++)
            {
                string a = remoteFiles.Items[i].ToString().Substring(remoteFiles.Items[i].ToString().Length-3);//choose the last 3 char, if equal ".cs",then add it
                if(a==".cs")
                msg1.arguments.Add(remoteFiles.Items[i].ToString());
            }
            comm.postMessage(msg1);
        }

        // a button that start dependency analysis,and show the result on textbox
        private void DepAnalysis_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Xiao Tan";
            msg1.command = "startAnalyzing";
            msg1.Atype = "AnalylizeDep";
            for (int i = 0; i < OutputResults.Items.Count; i++)
            {
                msg1.arguments.Add(OutputResults.Items[i].ToString());
            }

            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "ShowFinalResult";
            comm.postMessage(msg2);
        }

        //start a strong component anaalysis 
        private void StrAnalysis_Click(object sender, RoutedEventArgs e)
        {
            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Xiao Tan";
            msg1.command = "startAnalyzing";
            msg1.Atype = "AnalylizeStr";
            for (int i = 0; i < OutputResults.Items.Count; i++)
            {
                msg1.arguments.Add(OutputResults.Items[i].ToString());
            }
            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "ShowFinalResult";
            comm.postMessage(msg2);
        }


        private void AutoTest_Click(object sender, RoutedEventArgs e)
        {
            object a = new object();
            RoutedEventArgs b= new RoutedEventArgs();
            RemoteTop_Click(a,b);
            AddAll_Click(a, b);
           


            DepAnalysis_Click(a,b);

            StrAnalysis_Click(a,b);

            CommMessage msg1 = new CommMessage(CommMessage.MessageType.request);
            msg1.from = ClientEnvironment.endPoint;
            msg1.to = ServerEnvironment.endPoint;
            msg1.author = "Xiao Tan";
            msg1.command = "startAnalyzing";
            msg1.Atype = "AutoTest";
            for (int i = 0; i < OutputResults.Items.Count; i++)
            {
                msg1.arguments.Add(OutputResults.Items[i].ToString());
            }

            comm.postMessage(msg1);
            CommMessage msg2 = msg1.clone();
            msg2.command = "ShowFinalResult";
            comm.postMessage(msg2);


        }
    }
}
