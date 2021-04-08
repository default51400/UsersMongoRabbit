using System;
using System.Collections.Generic;
using System.Text;
using BLL.Models;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Infrastructure.Extensions
{
    public static class AuthOptionsExtension
    {
        public static SymmetricSecurityKey GetSymmetricSecurityKey(this AuthOptions authOptions)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authOptions.KEY));
        }
    }
}
