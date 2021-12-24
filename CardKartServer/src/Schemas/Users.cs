using LiteDB;

namespace CardKartServer
{
    internal static class Users
    {
        private static ILiteCollection<UserEntry> UserCollection { get; set; }

        public static void Load(ILiteCollection<UserEntry> users)
        {
            UserCollection = users;
            
            UserCollection.EnsureIndex(user => user.Name);
        }

        public static DBQuery<UserEntry> RegisterUser(string username)
        {
            if (UserCollection.FindOne(user => user.Name == username) != null)
            {
                return new DBError("A user with that name already exists");
            }

            var user = new UserEntry();
            user.Name = username;
            UserCollection.Insert(user);
            return user;
        }

        public static DBQuery<UserEntry> GetUser(string username)
        {
            var user = UserCollection.FindOne(user => user.Name == username);
            if (user == null)
            {
                return new DBError("No user with that name exists.");
            }
            else { return user; }
        }
    }

    internal class UserEntry
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
