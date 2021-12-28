using Microsoft.AspNetCore.Mvc;
using api.Repositories;
using api.Models;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class UsuarioController : Controller
    {       
        private readonly IUsuarioRepository _usuarioRepository;

        public UsuarioController(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        [Authorize]
        [HttpGet]
        public async Task<IEnumerable<usuario>> GetUsuarios()
        {
            return await _usuarioRepository.GetAll();
        }
        
        [HttpGet("/id/{id}")]
        public async Task<ActionResult<usuario>> GetUsuarioById(int id)
        {
            return await _usuarioRepository.GetById(id);
        }
        [HttpGet("/email/{email}")]
        public async Task<ActionResult<usuario>> GetByEmail(string email)
        {
            return await _usuarioRepository.GetByEmail(email);
        }

        [HttpPost]
        public async Task<ActionResult<usuario>>PostUsuario([FromBody] usuario usuario)
        {
            usuario patata = await _usuarioRepository.Create(usuario);
            return CreatedAtAction(nameof(GetUsuarios), new{ id = patata.Id });
        }

        [HttpPut]
        public async Task<ActionResult> PutUsuario([FromBody] usuario usuario)
        { 
            await _usuarioRepository.Update(usuario);
            
          
            return NoContent(); //No tenemos por qué devolver nada cuando hemos modificado
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteUsuario([FromBody] usuario usuario)
        {
            if (await _usuarioRepository.GetById(usuario.Id) == null)
            {
                return BadRequest();
            }
            await _usuarioRepository.DeleteById(usuario.Id);   
            return NoContent();
        }
    }
}
