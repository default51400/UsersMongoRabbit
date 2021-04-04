using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Models;
using BLL.Services.Interfaces;
using DAL.Models.Entities;
using DAL.Repositories.Interfaces;

namespace BLL.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository usersRepository;

        public UsersService(IUsersRepository usersRepository) => this.usersRepository = usersRepository;

        public async Task Create(UserDto userDto)
        {
            var mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<UserDto, User>()));
            User user = mapper.Map<UserDto, User>(userDto);

            await usersRepository.Create(user);
        }

        public async Task<IEnumerable<UserDto>> GetAll()
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDto>()).CreateMapper();

            return mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(await usersRepository.GetAll());
            //return await usersRepository.GetAll();
        }
    }
}