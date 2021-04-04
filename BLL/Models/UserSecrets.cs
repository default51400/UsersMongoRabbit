using System.Text.Json.Serialization;

namespace BLL.Models
{
    public class UserSecrets
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}