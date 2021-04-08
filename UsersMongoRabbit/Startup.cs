using BLL.Infrastructure.Extensions;
using BLL.Middlewares;
using BLL.Models;
using DAL.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace UsersMongoRabbit
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AuthOptions>(Configuration.GetSection("AuthOptions"));
            services.Configure<UserSecrets>(Configuration.GetSection("UserSecrets"));
            services.Configure<RabbitMqConnection>(Configuration.GetSection("RabbitMqConnection"));
            services.Configure<MongoConnection>(Configuration.GetSection("MongoConnection"));
            services.Configure<Settings>(options =>
            {
                options.ConnectionString = Configuration.GetSection("MongoConnection:ConnectionString").Value;
                options.Database = Configuration.GetSection("MongoConnection:Database").Value;
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                      .AddJwtBearer(options =>
                      {
                          options.RequireHttpsMetadata = true;
                          options.TokenValidationParameters = new TokenValidationParameters
                          {
                              ValidateIssuer = true,
                              ValidIssuer = Configuration.Get<AuthOptions>().ISSUER,
                              ValidateAudience = true,
                              ValidAudience = Configuration.Get<AuthOptions>().AUDIENCE,
                              ValidateLifetime = true,
                              IssuerSigningKey = Configuration.Get<AuthOptions>().GetSymmetricSecurityKey(),
                              ValidateIssuerSigningKey = true,
                          };
                      });

            services.AddControllers();
            services.CustomConfigureServices();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseMiddleware<TokenMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
