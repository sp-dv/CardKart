using LiteDB;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CardKartServer.Schemas
{
    internal class LoginInfo
    {
        private static ILiteCollection<LoginInfoEntry> Collection { get; set; }

        public static LoginInfoEntry[] All => Collection.Query().ToArray();

        public static void Load(ILiteCollection<LoginInfoEntry> loginInfos)
        {
            Collection = loginInfos;
            
            Collection.EnsureIndex(info => info.UserID);
            Collection.EnsureIndex(info => info.Username);
        }

        public static DBQuery<LoginInfoEntry> RegisterUser(string username, string password)
        {
            if (Collection.FindOne(user => user.Username == username) != null)
            {
                return new DBError("A user with that name already exists");
            }
            
            var userEntry = Users.CreateUser();
            if (userEntry.Result != null)
            {
                var loginInfo = new LoginInfoEntry();
                var user = userEntry.Result;
                loginInfo.UserID = user.UserID;
                loginInfo.Username = username;
                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                loginInfo.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, salt);

                Collection.Insert(loginInfo);

                return loginInfo;
            }
            else
            {
                return userEntry.DBError;
            }
        }

        public static DBQuery<LoginInfoEntry> VerifyUser(string username, string password)
        {
            var loginInfo = Collection.FindOne(info => info.Username == username);

            if (loginInfo != null && loginInfo.PasswordHash != null && 
                BCrypt.Net.BCrypt.Verify(password, loginInfo.PasswordHash))
            {
                return loginInfo;
            }

            return new DBError("Incorrect username or password.");
        }
    }

    public class LoginInfoEntry
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
