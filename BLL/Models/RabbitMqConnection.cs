using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Models
{
    public class RabbitMqConnection
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 6572;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}
