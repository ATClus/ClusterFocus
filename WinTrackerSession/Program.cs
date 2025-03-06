using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace WinTrackerSession
{
    static class Program
    {
        [STAThread]
        static async Task Main()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("https://localhost:7131");
                })
                .Build();

            var hostTask = host.RunAsync();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            await hostTask;
        }
    }
}
