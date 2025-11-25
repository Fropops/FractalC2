using System.Collections.Generic;
using System.Linq;
using TeamServer.Models;
using TeamServer.Services;

public interface IUserService
{
    User GetUser(string userId);
    List<User> GetAllUsers();
    void AddUser(User user);
}

public class UserService : IUserService
{
    private Dictionary<string, User> users = new Dictionary<string, User>();
    public User GetUser(string userId)
    {
        if (!users.ContainsKey(userId))
            return null;

        return users[userId];
    }

    public List<User> GetAllUsers()
    {
        return this.users.Values.ToList();
    }

    public void AddUser(User user)
    {
        this.users.Add(user.Id, user);
    }
}

