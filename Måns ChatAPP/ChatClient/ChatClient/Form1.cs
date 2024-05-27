using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ChatClient
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient("127.0.0.1", 5000);
                writer = new StreamWriter(client.GetStream()) { AutoFlush = true };
                reader = new StreamReader(client.GetStream());
                Log("Connected to server...");
                Thread readThread = new Thread(ReadMessages);
                readThread.Start();
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string message = textBoxMessage.Text;
            writer.WriteLine(message);
            Log("You: " + message);
            textBoxMessage.Clear();
        }

        private void ReadMessages()
        {
            try
            {
                while (true)
                {
                    string message = reader.ReadLine();
                    if (message == null) break;
                    Log(message);
                }
            }
            catch (Exception ex)
            {
                Log("Error: " + ex.Message);
            }
        }

        private void Log(string message)
        {
            listBoxMessages.Invoke((MethodInvoker)delegate {
                listBoxMessages.Items.Add(message);
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            client.Close();
        }
    }
}
