using api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace api.Repositories
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<usuario>>GetAll();

        Task<usuario> GetById(int id);
        Task<usuario> GetByEmail(string email);

        Task<usuario> Create(usuario usuario);
        Task Update(usuario usuario);
        Task DeleteById(int id);
    }
}
