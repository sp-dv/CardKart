using LiteDB;
using System;

namespace CardKartServer.Schemas
{
    internal static class Users
    {
        private static ILiteCollection<UserEntry> UserCollection { get; set; }

        public static UserEntry[] AllUsers => UserCollection.Query().ToArray();

        public static void Load(ILiteCollection<UserEntry> users)
        {
            UserCollection = users;
            UserCollection.EnsureIndex(user => user.UserID);
        }

        public static DBQuery<UserEntry> CreateUser()
        {
            var user = new UserEntry();

            var rand = new Random();
            var randomBytes = new byte[32];

            while (true)
            {
                rand.NextBytes(randomBytes);
                var newUserID = Convert.ToBase64String(randomBytes);

                if (UserCollection.Query().Where(user => user.UserID == newUserID).Count() == 0)
                {
                    user.UserID = newUserID;
                    break;
                }
            }
            UserCollection.Insert(user);
            return user;
        }
    }

    internal class UserEntry
    {
        public int Id { get; set; }
        public string UserID { get; set; }
    }
}
