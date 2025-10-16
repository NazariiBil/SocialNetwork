// Services/SocialNetworkService.cs
using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetwork.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetwork.Services
{
    public class SocialNetworkService
    {
        public readonly IMongoCollection<User> _users;

        public SocialNetworkService(DatabaseService db)
        {
            _users = db.Users;
        }

      
        public User GetUserById(string id)
        {
            return _users.Find(u => u.Id == id).FirstOrDefault();
        }

        
        public List<User> SearchUsers(string query)
        {
            var filter = Builders<User>.Filter.Where(u =>
                u.FirstName.Contains(query) ||
                u.LastName.Contains(query) ||
                u.Email.Contains(query)
            );
            return _users.Find(filter).ToList();
        }

 
        public void AddFriend(string currentUserId, string friendId)
        {
            var updateFriend = Builders<User>.Update.AddToSet(u => u.Friends, friendId);
            _users.UpdateOne(u => u.Id == currentUserId, updateFriend);

            var updateMutual = Builders<User>.Update.AddToSet(u => u.Friends, currentUserId);
            _users.UpdateOne(u => u.Id == friendId, updateMutual);
        }

        public void RemoveFriend(string currentUserId, string friendId)
        {
            var updateFriend = Builders<User>.Update.Pull(u => u.Friends, friendId);
            _users.UpdateOne(u => u.Id == currentUserId, updateFriend);

            var updateMutual = Builders<User>.Update.Pull(u => u.Friends, currentUserId);
            _users.UpdateOne(u => u.Id == friendId, updateMutual);
        }

     
        public Post CreatePost(string authorId, string content)
        {
            var newPost = new Post
            {
                PostId = ObjectId.GenerateNewId().ToString(),
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            var filter = Builders<User>.Filter.Eq(u => u.Id, authorId);
            var update = Builders<User>.Update.Push(u => u.Posts, newPost);
            _users.UpdateOne(filter, update);

            return newPost;
        }

        public void AddComment(string authorId, string postOwnerId, string postId, string content)
        {
            var newComment = new Comment
            {
                CommentId = ObjectId.GenerateNewId().ToString(),
                AuthorId = authorId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            var filter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, postOwnerId),
                Builders<User>.Filter.Eq("Posts.PostId", postId)
            );

            var update = Builders<User>.Update.Push("Posts.$.Comments", newComment);
            _users.UpdateOne(filter, update);
        }

   
        public void TogglePostReaction(string reactorId, string postOwnerId, string postId, string reactionType)
        {
            var newReaction = new Reaction { UserId = reactorId, Type = reactionType, CreatedAt = DateTime.UtcNow };

            var postFilter = Builders<User>.Filter.And(
                Builders<User>.Filter.Eq(u => u.Id, postOwnerId),
                Builders<User>.Filter.Eq("Posts.PostId", postId)
            );

         
            var pullUpdate = Builders<User>.Update.PullFilter(
                "Posts.$.Reactions",
                Builders<Reaction>.Filter.Eq(r => r.UserId, reactorId)
            );
            var result = _users.UpdateOne(postFilter, pullUpdate);

            if (result.ModifiedCount == 0)
            {
               
                var pushUpdate = Builders<User>.Update.Push("Posts.$.Reactions", newReaction);
                _users.UpdateOne(postFilter, pushUpdate);
            }
            
        }

        
        public List<Post> GetStream()
        {
    
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$unwind", "$Posts"),
                new BsonDocument("$sort", new BsonDocument("Posts.CreatedAt", -1)),
                new BsonDocument("$project", new BsonDocument
                {
                    {"_id", 0},
                    {"PostId", "$Posts.PostId"},
                    {"Content", "$Posts.Content"},
                    {"CreatedAt", "$Posts.CreatedAt"},
                    {"Comments", "$Posts.Comments"},
                    {"Reactions", "$Posts.Reactions"},
                    {"AuthorId", "$_id"}
                })
            };

            var postsBson = _users.Aggregate<BsonDocument>(pipeline).ToList();

            var allPosts = new List<Post>();
            foreach (var doc in postsBson)
            {
                var post = new Post
                {
                    PostId = doc["PostId"].AsString,
                    Content = doc["Content"].AsString,
                    CreatedAt = doc["CreatedAt"].ToUniversalTime(),
                    AuthorId = doc["AuthorId"].AsObjectId.ToString(),
                };

           
                if (doc.Contains("Reactions"))
                {
                    post.Reactions.AddRange(doc["Reactions"].AsBsonArray.Select(r =>
                         MongoDB.Bson.Serialization.BsonSerializer.Deserialize<Reaction>(r.AsBsonDocument)));
                }

                if (doc.Contains("Comments"))
                {
                    post.Comments.AddRange(doc["Comments"].AsBsonArray.Select(c =>
                        MongoDB.Bson.Serialization.BsonSerializer.Deserialize<Comment>(c.AsBsonDocument)));
                }
                allPosts.Add(post);
            }
            return allPosts;
        }
    }
}