using Newtonsoft.Json;
using OnlineExamSystem.Models;

namespace OnlineExamSystem.Common
{
    public static class SessionHelper
    {
        private const string SessionKey = "UserSession";

        public static void SetSessionData(ISession session, SessionData data)
        {
            if (session == null || data == null) return;
            var json = JsonConvert.SerializeObject(data);
            session.SetString(SessionKey, json);
        }

        public static SessionData? GetSessionData(ISession session)
        {
            if (session == null) return null;
            var json = session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonConvert.DeserializeObject<SessionData>(json);
            }
            catch
            {
                return null;
            }
        }

        public static void ClearSession(ISession session)
        {
            session?.Remove(SessionKey);
        }
    }
}
