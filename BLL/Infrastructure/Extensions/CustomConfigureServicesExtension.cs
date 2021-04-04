using System;
using System.Collections.Generic;
using System.Text;
using BLL.Infrastructure.RabbitMq;
using BLL.Services;
using BLL.Services.Interfaces;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Infrastructure.Extensions
{
    public static class CustomConfigureServicesExtension
    {
        public static void CustomConfigureServices(this IServiceCollection services)
        {
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IUsersService, UsersService>();


            services.AddHostedService<ReceiverService>();
        }
    }
}
