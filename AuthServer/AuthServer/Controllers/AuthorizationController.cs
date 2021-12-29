using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace AuthServer.Controllers
{
    public class AuthorizationController : Controller
    {
        //Endpoint para recibir el token
        [HttpPost("~/connect/token")]
        public  async Task<IActionResult> Exchange()
        {
            //Hacmeos una petición al servidor de OpenIddict
            var peticion = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("La petición de OpenID Connect no se ha podido realizar");

            ClaimsPrincipal claimsPrincipal;

            //Comprobamos que la petición tiene de grant type client_credentials, o lo que es lo mismo que sigue el flow Client Credentials
            if (peticion.IsClientCredentialsGrantType())
            {
                //IMPORTANTE: si:
                //              client_id 
                //              client_secret 
                //              Si alguno es inválido, peta, la petición presenta el siguiente formato:
                //POST / token HTTP / 1.1
                //Host: authorization.server.com
                //Content - Type: application / x - www - form - urlencoded
                //grant_type = client_credentials
                //& client_id = 213f6de8 - f232 - 4854 - 8c20 - 80a9b385cca7
                //& client_secret = nH7TbHkgsjOWIAtb4NV78RQD5EOtOH16nIusaVzZ4EI =
                //&scope = returngis_api.read api2.readAndWrite

                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

                //Coloca el Claim SUB (CLIENT ID), que es obligatorio
                identity.AddClaim(OpenIddictConstants.Claims.Subject, peticion.ClientId ?? throw new InvalidOperationException());
                //Colocamos el Claim NAME (USERNAME), no es obligatorio
                //identity.AddClaim(OpenIddictConstants.Claims.Name, peticion.Username);

                claimsPrincipal = new ClaimsPrincipal(identity);

                claimsPrincipal.SetScopes(peticion.GetScopes());
            } 
            else if (peticion.IsAuthorizationCodeGrantType()){
                //Sacamos los claims guardados en el código de autorización
                claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
            }else{
                throw new InvalidOperationException("The specified grant type is not supported");
            }
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        [IgnoreAntiforgeryToken] //
        public async Task<IActionResult> Authorize()
        {
            var peticion = HttpContext.GetOpenIddictServerRequest() ?? throw new InvalidOperationException("The openID Connect request cannot be retrieved");

            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //Si no se puede sacar el usuario , se le devuelve a la página de login con un Challenge
            if (!result.Succeeded)
            {
                return Challenge(
                    authenticationSchemes: CookieAuthenticationDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties
                    {
                        RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                            Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                    });

            }

            //Creamos unos nuevos claims para la respuesta

            var claims = new List<Claim>
            {
                new Claim(OpenIddictConstants.Claims.Subject, result.Principal.Identity.Name),
                new Claim("some claim", "some value").SetDestinations(OpenIddictConstants.Destinations.AccessToken),
                new Claim(OpenIddictConstants.Claims.Email, "some@email").SetDestinations(OpenIddictConstants.Destinations.IdentityToken)
            };

            var claimsIdentity = new ClaimsIdentity(claims, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            //Los scopes solicitados por el cliente se dan todos en esta llamada porque no implementamos una pantalla para que demos el consentimiento, estaría bien hacer
            //una página para que se puedan elegir
            claimsPrincipal.SetScopes(peticion.GetScopes());

            //Devolvemos el signing in con el esquema de autenticación de OpenIddict para poder cambiar el esquema de autenticación por un token 
            //El Signin hace que openIddict mande un codigo de autorización para que después poueda ser intercambiado por el token 
            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        }
    }
}
