﻿using CardKartShared.Util;
using LiteDB;
using System;

namespace CardKartServer
{

    internal static class Database
    {

        private static LiteDatabase LiteDB { get; set; }

        public static void Load()
        {
            LiteDB = new LiteDatabase(@"./MyData.db");

            Users.Load((LiteDB.GetCollection<UserEntry>("users")));

            Logging.Log(LogLevel.Info, "Database loaded successfully.");
        }

    }
    
    internal class DBQuery<T>
    {
        public T Result { get; private set; }
        private DBError DBError;

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
