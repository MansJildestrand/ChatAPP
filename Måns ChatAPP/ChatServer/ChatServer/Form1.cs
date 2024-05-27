using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private TcpListener server;
        private List<TcpClient> clients = new List<TcpClient>();
        private StreamWriter logWriter;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartServer();
        }

        private void StartServer()
        {
            logWriter = new StreamWriter("chatlog.txt", true);
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Log("Server started...");
            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private void AcceptClients()
        {
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                clients.Add(client);
                Log("Client connected: " + client.Client.RemoteEndPoint);
                listBoxClients.Invoke((MethodInvoker)delegate {
                    listBoxClients.Items.Add(client.Client.RemoteEndPoint.ToString());
                });
                Thread readThread = new Thread(() => ReadMessages(client));
                readThread.Start();
            }
        }

        private void ReadMessages(TcpClient client)
        {
            StreamReader reader = new StreamReader(client.GetStream());
            try
            {
                while (true)
                {
                    string message = reader.ReadLine();
                    if (message == null) break;
                    Log("Received: " + message);
                    BroadcastMessage(message);
                }
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
            }
            finally
            {
                client.Close();
                clients.Remove(client);
                listBoxClients.Invoke((MethodInvoker)delegate {
                    listBoxClients.Items.Remove(client.Client.RemoteEndPoint.ToString());
                });
                Log("Client disconnected: " + client.Client.RemoteEndPoint);
            }
        }

        private void BroadcastMessage(string message)
        {
            foreach (var client in clients)
            {
                try
                {
                    StreamWriter writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                    writer.WriteLine(message);
                }
                catch (Exception ex)
                {
                    Log("Error sending message: " + ex.Message);
                }
            }
        }

        private void Log(string message)
        {
            textBoxLog.Invoke((MethodInvoker)delegate {
                textBoxLog.AppendText(message + Environment.NewLine);
            });
            logWriter.WriteLine(message);
            logWriter.Flush();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Stop();
            logWriter.Close();
        }
    }
}
