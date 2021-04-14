using System;
using System.Linq;
using System.Threading;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Logger;

namespace Inst
{
    class Program
    {
        private static UserSessionData user;
        private static string uri;
        private static IInstaApi api;

        static void Main(string[] args)
        {

            user = new UserSessionData();
            Console.Write("UserName: ");
            user.UserName = Console.ReadLine();
            Console.Write("Password: ");
            user.Password = Console.ReadLine();
            Console.Write("Uri: ");
            uri = Console.ReadLine();
            api = InstaApiBuilder.CreateBuilder()
                .SetUser(user)
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(RequestDelay.FromSeconds(2, 2))
                .Build();
            AuthAndPost();
            Console.Read();
        }

        private static async void AuthAndPost()
        {
            var rnd = new Random();
            var loginResult = await api.LoginAsync();

            if (!loginResult.Succeeded)
            {
                throw new Exception("auth error");
            }
            Console.WriteLine("authorized");
            var mediaResult = await api.GetMediaIdFromUrlAsync(new Uri(uri));

            if (!mediaResult.Succeeded)
            {
                throw new Exception("get media id error");
            }
            var mediaId = mediaResult.Value;
            Console.WriteLine($"media id: {mediaId}");
            var userResult = await api.GetUserFollowingAsync(user.UserName, PaginationParameters.MaxPagesToLoad(500));

            if (!userResult.Succeeded)
            {
                throw new Exception("get followers error");
            }
            var users = userResult
                .Value
                .Select(user => user.UserName)
                .ToArray();
            Console.WriteLine($"user count: {users.Count()}");

            for (var i = 0; i < users.Count(); i++)
            {
                int timeOut = rnd.Next(30000, 60000);
                var commentResult = await api.CommentMediaAsync(mediaId, $"@{users[i]}");
                
                if (!commentResult.Succeeded)
                {
                    Console.WriteLine($"{DateTime.Now}. post message error: @{users[i]}. time out - {timeOut}. error - {commentResult.Info.Exception}");
                    break;
                }
                Console.WriteLine($"{DateTime.Now}. @{users[i]}. time out - {timeOut}");
                Thread.Sleep(timeOut);
            }
        }
    }
}
