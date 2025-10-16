// Services/AuthService.cs
using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetwork.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetwork.Services
{
    public class AuthService
    {
        private readonly IMongoCollection<User> _users;

        public AuthService(DatabaseService db)
        {
            _users = db.Users;
        }

    
        public User Login(string email, string password)
        {
            return _users.Find(u => u.Email == email && u.Password == password).FirstOrDefault();
        }

        
        public User Register(string email, string password, string firstName, string lastName, List<string> interests)
        {
            if (_users.Find(u => u.Email == email).Any())
            {
                throw new InvalidOperationException($"Користувач з email '{email}' вже існує.");
            }

            var newUser = new User
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                Interests = interests
            };

            _users.InsertOne(newUser);
            return newUser;
        }
    }
}