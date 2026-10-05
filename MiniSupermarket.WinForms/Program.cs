using System;
using System.Net;
using System.Windows.Forms;

namespace MiniSupermarket.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            try
            {
                // Cấu hình mạng cho môi trường phát triển (chấp nhận SSL localhost tự ký và TLS 1.2)
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.log");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Application started\n");

                FormLogin formLogin = null;
                try
                {
                    System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Instantiating FormLogin\n");
                    formLogin = new FormLogin();
                    System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: FormLogin created successfully, calling ShowDialog\n");
                    var result = formLogin.ShowDialog();
                    System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: FormLogin dialog result = {result}, Token = {SessionManager.JwtToken}\n");
                    if (result == DialogResult.OK && !string.IsNullOrEmpty(SessionManager.JwtToken))
                    {
                        System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Starting FormMainShell\n");
                        Application.Run(new FormMainShell());
                    }
                }
                catch (Exception formEx)
                {
                    System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: FormLogin EXCEPTION: {formEx}\n");
                }
                finally
                {
                    formLogin?.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("crash.log", ex.ToString());
                MessageBox.Show("Khởi động ứng dụng lỗi:\n" + ex.Message, "Lỗi khởi động", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
