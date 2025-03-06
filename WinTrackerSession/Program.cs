using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinTrackerSession
{
    static class Program
    {
        [STAThread]
        static async Task Main()
        {
            // Cria e configura o host para a aplicação web (Hub)
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Utiliza a classe Startup para configurar os serviços e middlewares
                    webBuilder.UseStartup<Startup>();
                    // Define a URL e porta (ajuste conforme necessário)
                    webBuilder.UseUrls("https://localhost:7131");
                })
                .Build();

            // Inicia o host web em segundo plano
            var hostTask = host.RunAsync();

            // Inicializa a aplicação Windows Forms (que, por sua vez, contém o NotifyIcon, etc.)
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            // Aguarda a finalização do host (após fechar o formulário, por exemplo)
            await hostTask;
        }
    }
}
