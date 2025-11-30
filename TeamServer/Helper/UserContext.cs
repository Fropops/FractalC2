
using TeamServer.Models;

namespace TeamServer.Helper
{
    public class UserContext
    {
        public UserContext(User user, string session)
        {
            User = user;
            Session = session;
        }
        public User User { get; set; }
        public string Session { get; private set; }
    }
}
