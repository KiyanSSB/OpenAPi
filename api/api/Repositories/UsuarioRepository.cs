using api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using api.Datos;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {

        private readonly usuarioContext _usuarioContext;
        public UsuarioRepository(usuarioContext context)
        {
            this._usuarioContext = context;
        }
        public async Task<usuario> Create(usuario usuario)
        {
            _usuarioContext.Add(usuario);
            await _usuarioContext.SaveChangesAsync();

            return usuario;
        }

        public async Task DeleteById(int id)
        {
            var usuarioAborrar = await _usuarioContext.usuarios.FindAsync(id);
            _usuarioContext.Remove(usuarioAborrar);
            await _usuarioContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<usuario>> GetAll()
        {
            return await _usuarioContext.usuarios.ToListAsync();
        }

        public async Task<usuario> GetByEmail(string email)
        {
            return await _usuarioContext.usuarios.FindAsync(email);
        }

        public async Task<usuario> GetById(int id)
        {
            return await _usuarioContext.usuarios.FindAsync(id);
        }

        public async Task Update(usuario usuario)
        {
            _usuarioContext.Entry(usuario).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            await _usuarioContext.SaveChangesAsync();
        }

        async Task<usuario> IUsuarioRepository.Create(usuario usuario)
        {
            _usuarioContext.Add(usuario);
            await _usuarioContext.SaveChangesAsync();

            return usuario;
        }
    }
}
