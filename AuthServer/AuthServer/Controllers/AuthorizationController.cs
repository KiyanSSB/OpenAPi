using System;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace AuthServer.Controllers
{
    public class AuthorizationController : Controller
    {

        [HttpPost("~/connect/token")]
        public IActionResult Exchange()
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

                //Coloca el Claim SUB, que es obligatorio
                identity.AddClaim(OpenIddictConstants.Claims.Subject, peticion.ClientId) ?? throw new InvalidOperationException());
                //Colocamos el Claim NAME, no es obligatorio
                identity.AddClaim(OpenIddictConstants.Claims.Name, peticion.)

            
            
            
            
            
            }
        
        }
    }
}
