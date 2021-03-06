﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;
using Simple.Data.Firebird.Test.Properties;

namespace Simple.Data.Firebird.Test
{
    public class DbHelper
    {
        private static readonly Lazy<bool> _dbCreated = new Lazy<bool>(CreateDatabaseIfNeeded);
        public static string DbCreationScript { get; set; }

        public dynamic OpenDefault()
        {
            if (_dbCreated.Value) return Database.Opener.OpenConnection(GetConnectionString(), "FirebirdSql.Data.FirebirdClient");

            throw new InvalidOperationException("Database file is not created.");
        }

        private static bool CreateDatabaseIfNeeded()
        {
            string connectionString = GetConnectionString();

            if (File.Exists(GetDatabaseFilePath()))
            {
                FbConnection.DropDatabase(connectionString);
            }

            FbConnection.CreateDatabase(connectionString);

            ExecuteDbCreationScript();

            return true;
        }

        private static void ExecuteDbCreationScript()
        {
            using (var connection = new FbConnection(GetConnectionString()))
            {
                var batchExecution = new FbBatchExecution(connection);
                var script = new FbScript(DbCreationScript ?? Resources.create_db);
                script.Parse();
                batchExecution.AppendSqlStatements(script);
                batchExecution.Execute();
            }
        }

        private static string GetDatabaseFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testdatabase.fdb");
        }

        public static string GetConnectionString()
        {
            return String.Format(ConfigurationManager.AppSettings["connectionStringTemplate"] ?? "User=SYSDBA;Password=masterkey;Database={0};DataSource=localhost; Port=3050;Dialect=3;Charset=UTF8;Connection lifetime=15;Pooling=true; MinPoolSize=0;MaxPoolSize=50;Packet Size=8192;ServerType=0;", GetDatabaseFilePath());
        }
    }
}
