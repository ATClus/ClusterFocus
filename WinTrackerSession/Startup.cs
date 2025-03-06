using Hub.Application;
using Hub.Infrastructure.Repositories;
using Hub.Infrastructure;
using Hub.Presentation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WinTrackerSession
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddDbContext<HubDbContext>(options =>
                options.UseSqlite("Data Source=hub.db"));
            services.AddScoped<ITabRepository, TabRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddCors(options =>
            {
                options.AddPolicy("BrowserPolicy", policy =>
                {
                    policy.SetIsOriginAllowed(origin => origin.StartsWith("moz-extension://"))
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
            // Se necessário, adicione outros serviços ou hosted services
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<HubDbContext>();
                dbContext.Database.Migrate();
            }

            // Configure o pipeline HTTP
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseOpenApi();
            }

            app.UseCors("BrowserPolicy");
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TabFocus>("/tabfocused");
                endpoints.MapHub<AppFocusHub>("/appfocused");
            });
        }
    }
}
