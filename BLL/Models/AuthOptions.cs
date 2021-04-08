using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BLL.Models
{
    public class AuthOptions
    {
        public string ISSUER { get; set; } = "MyAuthServer";
        public string AUDIENCE { get; set; } = "MyAuthClient";
        public string KEY { get; set; } = "mysupersecret_secretkey!123";
        public int LIFETIME { get; set; } = 1;
    }
}
