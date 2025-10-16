// Program.cs

using SocialNetwork.DataInitializer;
using SocialNetwork.Models;
using SocialNetwork.Services;
using System;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Driver;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.UTF8;

        var db = new DatabaseService();
        var auth = new AuthService(db);
        var social = new SocialNetworkService(db);

      
        SeedService.SeedData(db);

        Console.WriteLine("\n=== Social Network 2.0 (C# / MongoDB) ===");

        
        User currentUser = null;
        while (currentUser == null)
        {
            Console.WriteLine("\n--- ВХІД ---");
            Console.Write("Email: ");
            string email = Console.ReadLine();
            Console.Write("Password : ");
            string password = Console.ReadLine();

            currentUser = auth.Login(email, password);
            if (currentUser == null)
            {
                Console.WriteLine("❌ Невірні облікові дані. Спробуйте ще раз.");
            }
        }

        Console.WriteLine($"✅ Ласкаво просимо, {currentUser.FirstName} {currentUser.LastName}!");

      
        bool running = true;
        while (running)
        {
            currentUser = social.GetUserById(currentUser.Id);

            Console.WriteLine("\n--- МЕНЮ ---");
            Console.WriteLine("1. Стрічка (News Feed)");
            Console.WriteLine("2. Створити пост");
            Console.WriteLine("3. Пошук та Управління друзями");
            Console.WriteLine("4. Коментувати пост");
            Console.WriteLine("5. Реагувати на пост (Like/Unlike)");
            Console.WriteLine("6. Переглянути мій профіль");
            Console.WriteLine("0. Вихід");
            Console.Write($"Ваш вибір ({currentUser.FirstName}): ");

            string choice = Console.ReadLine();
            try
            {
                switch (choice)
                {
                    case "1":
                        ShowStream(social);
                        break;

                    case "2":
                        Console.Write("Введіть вміст посту: ");
                        string content = Console.ReadLine();
                        social.CreatePost(currentUser.Id, content);
                        Console.WriteLine("✅ Пост створено.");
                        break;

                    case "3":
                        HandleFriendship(social, currentUser);
                        break;

                    case "4":
                        Console.Write("Введіть ID власника посту: ");
                        string ownerId = Console.ReadLine();
                        Console.Write("Введіть PostId: ");
                        string postId = Console.ReadLine();
                        Console.Write("Введіть коментар: ");
                        string comment = Console.ReadLine();
                        social.AddComment(currentUser.Id, ownerId, postId, comment);
                        Console.WriteLine("✅ Коментар додано.");
                        break;

                    case "5":
                        Console.Write("Введіть ID власника посту: ");
                        string likeOwner = Console.ReadLine();
                        Console.Write("Введіть PostId: ");
                        string likePostId = Console.ReadLine();
                        // Мінімум одна реакція - "like"
                        social.TogglePostReaction(currentUser.Id, likeOwner, likePostId, "like");
                        Console.WriteLine("✅ Реакцію змінено (Like/Unlike).");
                        break;

                    case "6":
                        ShowUserProfile(currentUser);
                        break;

                    case "0":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("❌ Неправильний вибір.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n--- ПОМИЛКА ---: {ex.Message}");
            }
        }
    }

 
    static void HandleFriendship(SocialNetworkService social, User user)
    {
        Console.Write("Введіть ID користувача (або частину імені) для пошуку: ");
        string query = Console.ReadLine();
        var foundUsers = social.SearchUsers(query);

        if (!foundUsers.Any())
        {
            Console.WriteLine("Користувачів не знайдено.");
            return;
        }

        Console.WriteLine("\nЗнайдені користувачі:");
        foreach (var u in foundUsers.Where(u => u.Id != user.Id)) 
        {
            bool isFriend = user.Friends.Contains(u.Id);
            Console.WriteLine($"[ID: {u.Id.Substring(0, 8)}...] {u.FirstName} {u.LastName} - Статус: {(isFriend ? "ДРУГ" : "НЕ ДРУГ")}");
        }

        Console.Write("Введіть повний ID користувача для зміни статусу дружби: ");
        string friendId = Console.ReadLine();
        var targetUser = foundUsers.FirstOrDefault(u => u.Id == friendId);

        if (targetUser == null || targetUser.Id == user.Id)
        {
            Console.WriteLine("❌ Користувача з таким ID не знайдено або це ви.");
            return;
        }

        if (user.Friends.Contains(friendId))
        {
            social.RemoveFriend(user.Id, friendId);
            Console.WriteLine($"✅ Користувача {targetUser.FirstName} видалено з друзів.");
        }
        else
        {
            social.AddFriend(user.Id, friendId);
            Console.WriteLine($"✅ Користувача {targetUser.FirstName} додано в друзі.");
        }
    }

   
    static void ShowUserProfile(User user)
    {
        Console.WriteLine("\n--- ВАШ ПРОФІЛЬ ---");
        Console.WriteLine($"ID: {user.Id}");
        Console.WriteLine($"Ім'я: {user.FirstName} {user.LastName}");
        Console.WriteLine($"Email: {user.Email}");
        Console.WriteLine($"Інтереси: {string.Join(", ", user.Interests)}");
        Console.WriteLine($"Друзі ({user.Friends.Count}): {string.Join(", ", user.Friends.Select(f => f.Substring(0, 8) + "..."))}");
        Console.WriteLine($"Кількість постів: {user.Posts.Count}");
        Console.WriteLine("---------------------");
    }

    static void ShowStream(SocialNetworkService social)
    {
        var posts = social.GetStream();
        Console.WriteLine($"\n--- СТРІЧКА ПОСТІВ (Всього: {posts.Count}) ---");

       
        var userCache = social._users.Find(_ => true).ToList().ToDictionary(u => u.Id, u => u);

        foreach (var post in posts)
        {
            userCache.TryGetValue(post.AuthorId, out var author);
            string authorName = author != null ? $"{author.FirstName} {author.LastName}" : "Невідомий автор";

            int likeCount = post.Reactions.Count(r => r.Type == "like");

            Console.WriteLine("\n----------------------------------------------------");
            Console.WriteLine($"[{post.PostId.Substring(0, 8)}...] \"{post.Content}\"");
            Console.WriteLine($"  👤 {authorName} | ⏰ {post.CreatedAt.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  👍 Лайки: {likeCount}, Коментарі: {post.Comments.Count}");

           
            foreach (var c in post.Comments)
            {
                userCache.TryGetValue(c.AuthorId, out var commentAuthor);
                string commentAuthorName = commentAuthor != null ? $"{commentAuthor.FirstName} {commentAuthor.LastName}" : "Невідомий";


                int commentLikes = c.Reactions.Count(r => r.Type == "like");
                string reactionInfo = commentLikes > 0 ? $" ({commentLikes} 👍)" : "";

                Console.WriteLine($"    -> {commentAuthorName}: {c.Content}{reactionInfo}");
            }
        }
        Console.WriteLine("----------------------------------------------------");
    }
}

