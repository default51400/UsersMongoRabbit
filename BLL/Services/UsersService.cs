using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Infrastructure.RabbitMq;
using BLL.Models;
using BLL.Services.Interfaces;
using DAL.Models.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository usersRepository;
        private IConfiguration configuration;
        private readonly ILogger<UsersService> logger;
        private Sender sender;

        public UsersService(IUsersRepository usersRepository, ILogger<UsersService> logger, IConfiguration configuration)
        {
            this.usersRepository = usersRepository;
            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task Create(UserDto userDto)
        {
            var mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<UserDto, User>()));
            User user = mapper.Map<UserDto, User>(userDto);

            SendMessage(user);
            //await usersRepository.Create(user);
        }

        public async Task<IEnumerable<UserDto>> GetAll()
        {
            var mapper = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDto>()).CreateMapper();

            return mapper.Map<IEnumerable<User>, IEnumerable<UserDto>>(await usersRepository.GetAll());
            //return await usersRepository.GetAll();
        }

        private void SendMessage(User user)
        {
            try
            {
                string json = JsonSerializer.Serialize(user);
                sender = new Sender(configuration);
                var response = sender.Call(json);
                user = JsonSerializer.Deserialize<User>(response);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                sender.Close();
            }
        }
    }
}
