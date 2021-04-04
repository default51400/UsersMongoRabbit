using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.Models.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        public Task<IEnumerable<User>> GetAll();
        public Task Create(User user);
    }
}