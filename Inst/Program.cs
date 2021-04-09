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
        private static IInstaApi api;

        static void Main(string[] args)
        {

            user = new UserSessionData();
            Console.Write("UserName: ");
            user.UserName = Console.ReadLine();
            Console.Write("Password: ");
            user.Password = Console.ReadLine();
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
            var userResult = api.GetUserFollowingAsync(user.UserName, PaginationParameters.MaxPagesToLoad(252));

            if (!userResult.Result.Succeeded)
            {
                throw new Exception("get followers error");
            }
            var users = userResult
                .Result
                .Value
                .Select(user => user.UserName)
                .ToArray();
            
            for(var i = 0; i < users.Count(); i = i + 2)
            {
                if(i + 1 >= users.Count())
                {
                    break;
                }
                int timeOut = rnd.Next(1000, 2000);
                Thread.Sleep(timeOut);
                var commentResult = await api.CommentMediaAsync("2545956103990970313_7853189738", $"@{users[i]} @{users[i + 1]}");
                
                if (!commentResult.Succeeded)
                {
                    Console.WriteLine($"{DateTime.Now} post message error: @{users[i]} @{users[i + 1]}; time out - {timeOut}");
                    continue;
                }
                Console.WriteLine($"{DateTime.Now}: @{users[i]} @{users[i + 1]}; time out - {timeOut}");
            }
        }
    }
}
