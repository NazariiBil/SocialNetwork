using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace SocialNetwork.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

   
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        
        public List<string> Interests { get; set; } = new List<string>();

     
        public List<string> Friends { get; set; } = new List<string>();

      
        public List<Post> Posts { get; set; } = new List<Post>();
    }

    public class Post
    {
        [BsonElement("PostId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PostId { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public List<Reaction> Reactions { get; set; } = new List<Reaction>();
        public List<Comment> Comments { get; set; } = new List<Comment>();

        [BsonIgnore]
        public string AuthorId { get; set; }
    }

    public class Comment
    {
        [BsonElement("CommentId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CommentId { get; set; }

        public string AuthorId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

   
        public List<Reaction> Reactions { get; set; } = new List<Reaction>();
    }

    public class Reaction
    {
        public string UserId { get; set; }
        
        public string Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}