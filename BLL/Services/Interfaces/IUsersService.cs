using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Models;

namespace BLL.Services.Interfaces
{
    public interface IUsersService
    {
        public Task<IEnumerable<UserDto>> GetAll();
        public Task Create(UserDto user);
    }
}