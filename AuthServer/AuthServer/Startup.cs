using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthorizationServer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AuthServer
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = "/account/login";
                });
            
            
            services.AddDbContext<DbContext>(options =>
            {
                //Usando almacenamiento en memoria, esto hay que cambiarlo por un almacenamiento en base de datos
                options.UseInMemoryDatabase(nameof(DbContext));

                //Registra las ser de entidades necesarios para OpenIddict
                options.UseOpenIddict();
            });

            services.AddOpenIddict()
                //Registra los componentes básicos de OpenIddict
                .AddCore(options =>
                {
                    //Configura OpenIddict para usar el EFCore 
                    options.UseEntityFrameworkCore()
                        .UseDbContext<DbContext>();
                })

                //Registra los componentes del servidor OpenIddict
                .AddServer(options =>
                {
                    //Añadimos que el servidor soporte el "Client Credentials" Flow
                    options.AllowClientCredentialsFlow();

                    //Añadimos que el servidor soporte el "Authorization Code Pkce" Flow , el RequireProof es para asegurar que todos los clientes usan PKCE 
                    options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();
                    //Colocamos los endpoints para hacer las peticiones para coger las autorizaciones y los token
                    options.SetAuthorizationEndpointUris("/connect/authorize").SetTokenEndpointUris("/connect/token");

                    //Indicamos el endpoint desde el cual se obtiene el token ES NECESARIO PARA EL CLIENT CREDENTIALS FLOW
                    options.SetTokenEndpointUris("/connect/token");

                    options.SetUserinfoEndpointUris("/connect/userinfo");
                   
                    //Encriptamos y firmamos los tokens
                    options.
                        AddEphemeralEncryptionKey().
                        AddEphemeralSigningKey().
                        DisableAccessTokenEncryption();
                        
                    //Registra el scope
                    options.RegisterScopes("api");
                    //Registra el ASPNET Core Host y configura ASP.NET
                    options
                        .UseAspNetCore()
                        //Configura ASPNET para que el token lo gestione OpenIddict y podamos crear nosotros el endpoint con un controlador
                        .EnableTokenEndpointPassthrough()
                        //Configuramos ASPNET para que la autenticación la gestione OpenIddict
                        .EnableAuthorizationEndpointPassthrough()
                        //Configuramos ASPNET para que el userinfo lo gestiones Openiddict
                        .EnableUserinfoEndpointPassthrough();
                });

            services.AddHostedService<TestData>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
