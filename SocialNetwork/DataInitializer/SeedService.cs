// DataInitializer/SeedService.cs

using MongoDB.Bson;
using MongoDB.Driver;
using SocialNetwork.Models;
using SocialNetwork.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetwork.DataInitializer
{
    public static class SeedService
    {
        public static void SeedData(DatabaseService db)
        {
            var usersCollection = db.Users;

            if (usersCollection.CountDocuments(new BsonDocument()) >= 10)
            {
                Console.WriteLine("База даних вже має мінімальну кількість користувачів. Пропуск ініціалізації.");
                return;
            }

            Console.WriteLine("Ініціалізація мінімальних даних (10 користувачів, 20+ постів)...");

            var users = new List<User>
            {
                new User { FirstName = "Давид", LastName = "Алієв", Email = "alievdavid@.com", Password = "123", Interests = new List<string> { "IT", "Clash Royal" } },
                new User { FirstName = "Максим", LastName = "Солтис", Email = "maksymsoltys@.com", Password = "456", Interests = new List<string> { "Dota2", "Art" } },
                new User { FirstName = "Богдан", LastName = "Охрімчук", Email = "bohdanohrimchuk@.com", Password = "789", Interests = new List<string> { "Sport", "Music" } },
                new User { FirstName = "Юліна", LastName = "Досяк", Email = "yulianadosiak@.com", Password = "901", Interests = new List<string> { "Books", "History" } },
                new User { FirstName = "Назарій", LastName = "Біль", Email = "nazariibil@.com", Password = "012", Interests = new List<string> { "Cars", "Design" } },
                new User { FirstName = "Ольга", LastName = "Рицарь", Email = "olgarycar@.com", Password = "321", Interests = new List<string> { "Movies", "Tech" } },
                new User { FirstName = "Анатолій", LastName = "Священко", Email = "anatoludiak@.com", Password = "735", Interests = new List<string> { "Fishing", "Nature" } },
                new User { FirstName = "Ксенія", LastName = "Кухарська", Email = "kseniakuharska@.com", Password = "749", Interests = new List<string> { "Fashion", "Blogging" } },
                new User { FirstName = "Денис", LastName = "Павленко", Email = "denispavlenko@.com", Password = "979", Interests = new List<string> { "Programming", "Games" } },
                new User { FirstName = "Юлія", LastName = "Сидорак", Email = "yuliasidorak.com", Password = "443", Interests = new List<string> { "Yoga", "Healthy" } }
            };


            usersCollection.DeleteMany(new BsonDocument());
            usersCollection.InsertMany(users);

            var allUsers = usersCollection.Find(_ => true).ToList();
            var socialService = new SocialNetworkService(db);
            int postCount = 0;

            for (int i = 0; i < allUsers.Count; i++)
            {
                var user = allUsers[i];
              
                for (int j = 1; j <= (i % 3) + 2; j++)
                {
                  
                    var newPost = socialService.CreatePost(
                        user.Id,
                        $"Пост #{j} від {user.FirstName}. Тема: {user.Interests.FirstOrDefault()}"
                    );

                 
                    if (j % 2 == 0 && allUsers.Count > 1)
                    {
                        var commentAuthor = allUsers[(i + 1) % allUsers.Count]; 
                        socialService.AddComment(commentAuthor.Id, user.Id, newPost.PostId, $"Гарна думка, {user.FirstName}! 👏");

                        var liker = allUsers[(i + 2) % allUsers.Count];
                        socialService.TogglePostReaction(liker.Id, user.Id, newPost.PostId, "like");
                    }
                    postCount++;
                }
            }

           
            if (allUsers.Count >= 2)
            {
                socialService.AddFriend(allUsers[0].Id, allUsers[1].Id);
                socialService.AddFriend(allUsers[0].Id, allUsers[2].Id);
                socialService.AddFriend(allUsers[1].Id, allUsers[3].Id);
            }

            Console.WriteLine($"✅ Ініціалізація завершена. Створено {allUsers.Count} користувачів та {postCount} постів.");
            Console.WriteLine("--------------------------------------------------------------------------");
        }
    }
}