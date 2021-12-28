using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using authProject.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;



namespace authProject
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            //Si la base de datos no está creada la inicializa
            await context.Database.EnsureCreatedAsync(cancellationToken);

            await RegisterApplicationAsync(scope.ServiceProvider);
            await RegisterScopeAsyns(scope.ServiceProvider);


            static async Task RegisterApplicationAsync(IServiceProvider serviceProvider)
            {
                var manager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

                //Cliente con VUE
                if(await manager.FindByClientIdAsync("vueclient") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "vueclient",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "Vue client PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("es-ES")] = "Application client MVC"
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://localhost:4200")
                        },
                        RedirectUris =
                        {
                            new Uri("https://localhost:4200")
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,
                            Permissions.Endpoints.Revocation,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.ResponseTypes.Code,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "dataEventRecords"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }


                // API
                if (await manager.FindByClientIdAsync("rs_dataEventRecordsApi") is null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "rs_dataEventRecordsApi",
                        ClientSecret = "dataEventRecordsSecret",
                        Permissions =
                        {
                            Permissions.Endpoints.Introspection
                        }
                    };

                    await manager.CreateAsync(descriptor);
                }
            }

            static async Task RegisterScopeAsyns(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<IOpenIddictScopeManager>();
                if (await manager.FindByNamesAsync("dataEventRecords") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        DisplayName = "dataEventRecords API access",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("es-ES")] = "Acceso a la API de demo"

                        },
                        Name = "dataEventRecords",
                        Resources =
                        {
                            "rs_dataEventRecordsApi"
                        }
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            
        }
    }
}
