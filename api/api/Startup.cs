using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//
using api.Datos;
using Microsoft.EntityFrameworkCore;
using api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace api
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
            var key = "HolaQuetal123456";
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddDbContext<usuarioContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                    options.JsonSerializerOptions.WriteIndented = true;

                });
            services
                .AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //Esquema de autenticación
                    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme; //Cuando se solicita un punto que requiere autenticación de manera anónima, invoca un desafío con los esquemas de autenticación especificados, en este caso se devuelve el esquema del JWT devuelve la cabecera www-authenticate: Bearer , 
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; 
                    options.SaveToken = true;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                    };
                });
            services.AddAuthorization();
            services.AddSingleton<IJwtAuthenticationService>(new JwtAuthenticationService(key));
            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin", c => c.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });


            //services.addAutomapper(typeof(Startup));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "api", Version = "v1" });

                //Definimos una configuración de seguridad
                c.AddSecurityDefinition("bearerJWT", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Use the Token given by the Authentication endpoint",
                    Scheme = "bearer",
                    BearerFormat = JwtBearerDefaults.AuthenticationScheme,
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http, //Si se usa HTTP , coloca "Bearer" en el token pasado sin necesidad de ponerlo dentro de la caja del swagger, es decir , lee tokens de la siguiente manera "0000.0000.0000"
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            Scopes = new Dictionary<string, string>
                            {
                                { "readAccess", "Access read operations" },
                                { "writeAccess", "Access write operations" }
                            }
                        }
                    }
                });

                //Definimos el ámbito en el que se pide el requerimiento de seguridad 
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "bearerJWT"
                            }
                        },
                        new string[] {}
                    }
                });


            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "api v1"));
            }
            app.UseHttpsRedirection();
            app.UseCors("AllowOrigin");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
        }
    }
}
