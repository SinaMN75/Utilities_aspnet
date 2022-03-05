using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Utilities_aspnet.Utilities;

public static class StartupExtension
{
    public static void AddUtilitiesServices<T>(this IServiceCollection services, string connectionStrings) where T : DbContext
    {
        services.AddScoped<DbContext, T>();
        services.AddDbContext<T>(options => options.UseSqlServer(connectionStrings).EnableSensitiveDataLogging());
        services.AddControllersWithViews();
        services.AddRazorPages();
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));
        services.AddUtilitiesSwagger();
        services.AddUtilitiesIdentity();

        services.AddMvc(option => option.EnableEndpointRouting = false)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
            });
    }

    public static void UseUtilitiesServices(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseUtilitiesSwagger();
        }

        app.UseStaticFiles();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapAreaControllerRoute(name: "Areas", areaName: "Areas",
                "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            endpoints.MapRazorPages();
        });
    }
}