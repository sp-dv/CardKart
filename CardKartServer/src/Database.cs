using CardKartServer.Schemas;
using CardKartShared.Util;
using LiteDB;
using System;

namespace CardKartServer
{

    internal static class Database
    {

        private static LiteDatabase LiteDB { get; set; }

        public static void Load()
        {
            try
            {
                LiteDB = new LiteDatabase(CardKartServer.Config.DBFilePath);
            }
            catch
            {
                Logging.Log(LogLevel.Error, "Configuration error: invalid DB file path.");
                return;
            }

            Users.Load(LiteDB.GetCollection<UserEntry>("users"));
            LoginInfo.Load(LiteDB.GetCollection<LoginInfoEntry>("loginInfo"));

            Logging.Log(LogLevel.Info, "Database loaded successfully.");
        }

    }
    
    internal class DBQuery<T>
    {
        public T Result { get; private set; }
        public DBError DBError { get; private set; }

        public DBQuery(T result)
        {
            Result = result;
        }

        public DBQuery(DBError dBError)
        {
            DBError = dBError;
        }

        public DBQuery<T> Then(Action<T> func)
        {
            if (Result != null) { func(Result); }
            return this;
        }

        public DBQuery<T> Err(Action<DBError> func)
        {
            if (DBError != null) { func(DBError); }
            return this;
        }

        public static implicit operator DBQuery<T>(T t) => new DBQuery<T>(t);
        public static implicit operator DBQuery<T>(DBError error) => 
            new DBQuery<T>(error);
    }

    internal class DBError
    {
        public string ErrorMessage { get; set; }

        public DBError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
