using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

using Microsoft.AspNet.Server.Kestrel;
using Microsoft.AspNet.StaticFiles;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;

namespace CameraService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // init
            System.IO.DirectoryInfo root = System.IO.Directory.GetParent( env.WebRootPath );
            String embed = System.IO.Path.Combine(root.FullName, "embed");
            String searchPath = System.IO.Path.Combine(embed, "bin");
            



            String oldPath = System.Environment.GetEnvironmentVariable("Path");
            System.Environment.SetEnvironmentVariable("Path", searchPath + ";" + oldPath);

    
            Camera.AForgeStillWrapper VC = new Camera.AForgeStillWrapper(env.WebRootPath);
            
        }

        ~Startup()
        {

        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            // Uncomment the following line to add Web API services which makes it easier to port Web API 2 controllers.
            // You will also need to add the Microsoft.AspNet.Mvc.WebApiCompatShim package to the 'dependencies' section of project.json.
            // services.AddWebApiConventions();

            
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            // Add the platform handler to the request pipeline.
            //app.UseIISPlatformHandler();

            // Configure the HTTP request pipeline.
            String staticFilePath = env.WebRootPath + "\\static";
            // Add MyStaticFiles static files to the request pipeline.
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(staticFilePath),
                RequestPath = new PathString("/static")
            });
            app.UseDirectoryBrowser();

            // Add MVC to the request pipeline.
            app.UseMvc();
            // Add the following route for porting Web API 2 controllers.
            // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");


        }
    }
}
