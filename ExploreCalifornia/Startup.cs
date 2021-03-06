using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExploreCalifornia.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExploreCalifornia
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSignalR();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Login";
            });

            services.AddSingleton<IChatRoomService, InMemoryChatRoomService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            //If the client app is on a seperate domain
            app.UseCors(builder =>
            {
                builder.WithOrigins("https://www.example.com") //clientside server
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
            });

            app.UseAuthentication();

            //It's important that the UseSignalR line is below any lines that deal with https or authentication or security.
            //we have to provide a route configuration.and then tell it what end point we want this Hub to listen on. 
            //This can be any end point that we want. We'll just make one up and say slash chatHub. 
            app.UseSignalR(routes =>
            {
                routes.MapHub<ChatHub>("/chatHub");
                routes.MapHub<AgentHub>("/agentHub");
            });

            app.UseMvc();
        }
    }
}
