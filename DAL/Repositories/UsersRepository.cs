﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DAL.Contexts;
using DAL.Models;
using DAL.Models.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DAL.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly UsersContext _context = null;

        public UsersRepository(IOptions<Settings> settings) => _context = new UsersContext(settings);

        public async Task<IEnumerable<User>> GetAll() => await _context.Users.Find(_ => true).ToListAsync();

        public async Task Create(User user)
        {
            user.CreatedDate = DateTime.UtcNow.AddHours(2);
            await _context.Users.InsertOneAsync(user);
        }

    }
}