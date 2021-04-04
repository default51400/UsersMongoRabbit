using System;
using MongoDB.Bson.Serialization.Attributes;

namespace DAL.Models.Entities
{
    public class User
    {
        [BsonId]
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}