using System;

namespace BLL.Models
{
    public class UserDto
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}