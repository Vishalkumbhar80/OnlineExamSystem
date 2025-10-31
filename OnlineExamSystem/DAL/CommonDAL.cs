using Newtonsoft.Json;
using OnlineExamSystem.Models;
using Microsoft.AspNetCore.Http;

namespace OnlineExamSystem.DAL
{
    public static class CommonDAL
    {
        private static IHttpContextAccessor _httpContextAccessor;

        // This must be called once during app startup (Program.cs or Startup.cs)
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static int GetCurrentUserId()
        {
            if (_httpContextAccessor == null)
                return 0;

            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                return 0;

            var userJson = session.GetString("UserDetails");
            if (string.IsNullOrEmpty(userJson))
                return 0;

            try
            {
                var user = JsonConvert.DeserializeObject<UserModel>(userJson);
                return user?.UserId ?? 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}
