using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Models;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace UserMongoRabbit.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private readonly IUsersService usersService;

        public UsersController(IUsersService usersService)
        {
            this.usersService = usersService;
        }

        [HttpGet]
        public async Task<GenericResponse<IEnumerable<UserDto>>> GetAll()
        {
            try
            {
                var users = await usersService.GetAll();
                var resp = new GenericResponse<IEnumerable<UserDto>>()
                    .Success($"Success", users);

                return resp;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return new GenericResponse<IEnumerable<UserDto>>()
                    .Error($"Unhandled error: {ex.Message}", null); ;
            }
        }

        [HttpPost]
        public async Task Post(UserDto user)
        {
            try
            {
                await usersService.Create(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}