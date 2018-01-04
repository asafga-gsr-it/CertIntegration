// Author: Tigran Gasparian
// This sample is part Part One of the 'Getting Started with SQLite in C#' tutorial at http://www.blog.tigrangasparian.com/

using System;
using System.Data.SQLite;

namespace SQLiteSamples
{
    class Program
    {
        // Holds our connection with the database
        SQLiteConnection m_dbConnection;

     

        // Creates an empty database file
        void createNewDatabase()
        {
            SQLiteConnection.CreateFile("MyDatabase.sqlite");
        }

        // Creates a connection with our database file.
        void connectToDatabase()
        {
            m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            m_dbConnection.Open();
        }

        // Creates a table named 'highscores' with two columns: name (a string of max 20 characters) and score (an int)
        void createTable()
        {
            string sql = "create table Certificate (certname varchar(50), stauts int)";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        // Inserts some values in the highscores table.
        // As you can see, there is quite some duplicate code here, we'll solve this in part two.
        void insertTable(string hostname,int status)
        {
            string sql = "insert into Certificate (name, score) values ("+hostname+","+status+")";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();     
        }
     
    }
}