using OnlineExamSystem.Models;

namespace OnlineExamSystem.Common
{
    /// <summary>
    /// DI-friendly session wrapper. Read and write via properties; underlying session store is updated automatically.
    /// Register as scoped.
    /// </summary>
    public class SessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        public SessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Get whole session data (from session store). If none exists, returns null.
        /// </summary>
        public SessionData? Data
        {
            get => SessionHelper.GetSessionData(Session);
            set
            {
                if (value == null)
                    SessionHelper.ClearSession(Session);
                else
                    SessionHelper.SetSessionData(Session, value);
            }
        }

        public int UserId
        {
            get => Data?.UserId ?? 0;
            set
            {
                var d = Data ?? new SessionData();
                d.UserId = value;
                Data = d;
            }
        }

        public string UserName
        {
            get => Data?.UserName ?? string.Empty;
            set
            {
                var d = Data ?? new SessionData();
                d.UserName = value;
                Data = d;
            }
        }

        public string FullName
        {
            get => Data?.FullName ?? string.Empty;
            set
            {
                var d = Data ?? new SessionData();
                d.FullName = value;
                Data = d;
            }
        }

        public string Email
        {
            get => Data?.Email ?? string.Empty;
            set
            {
                var d = Data ?? new SessionData();
                d.Email = value;
                Data = d;
            }
        }

        public string Role
        {
            get => Data?.Role ?? "Guest";
            set
            {
                var d = Data ?? new SessionData();
                d.Role = value;
                Data = d;
            }
        }

        public DateTime LastLogin
        {
            get => Data?.LastLogin ?? DateTime.MinValue;
            set
            {
                var d = Data ?? new SessionData();
                d.LastLogin = value;
                Data = d;
            }
        }

        public void Clear()
        {
            SessionHelper.ClearSession(Session);
        }
    }
}
