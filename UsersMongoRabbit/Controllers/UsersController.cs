using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BLL.Models;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UsersMongoRabbit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {

        private readonly ILogger<UsersController> log;
        private readonly IUsersService usersService;

        public UsersController(IUsersService usersService, ILogger<UsersController> logger)
        {
            this.usersService = usersService;
            this.log = logger;
        }

        [HttpGet]
        public async Task<GenericResponse<IEnumerable<UserDto>>> GetAll()
        {
            try
            {
                log.LogInformation("Called GetAll");
                var users = await usersService.GetAll();
                var resp = new GenericResponse<IEnumerable<UserDto>>()
                    .Success($"Success", users);

                return resp;
            }
            catch (Exception ex)
            {
                log.LogError($"Exception: {ex.Message}");
                return new GenericResponse<IEnumerable<UserDto>>()
                    .Error($"Unhandled error: {ex.Message}", null); ;
            }
        }

        [HttpPost]
        public async Task Post(UserDto user)
        {
            try
            {
                log.LogInformation($"Called Post user: {JsonSerializer.Serialize(user)}");

                await usersService.Create(user);

                log.LogInformation($"Success created user: {JsonSerializer.Serialize(user)}");
            }
            catch (Exception ex)
            {
                log.LogError($"Exception: {ex.Message}");
            }
        }
    }
}