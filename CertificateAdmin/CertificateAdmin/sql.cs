// Author: Tigran Gasparian
// This sample is part Part One of the 'Getting Started with SQLite in C#' tutorial at http://www.blog.tigrangasparian.com/

using System;
using System.Data.SQLite;
using System.IO;

namespace SQLiteSamples
{
    public class  SqlLite    
    {
        // Holds our connection with the database
        SQLiteConnection m_dbConnection;

     

        // Creates an empty database file
        public void createNewDatabase()
        {
            if (File.Exists("MyDatabase.sqlite"))
            {
                return;             
            }
            else
            {
                SQLiteConnection.CreateFile("MyDatabase.sqlite");
                connectToDatabase();
                createTable();
                m_dbConnection.Close();
            }
        }

        // Creates a connection with our database file.
        public  void connectToDatabase()
        {
            m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            m_dbConnection.Open();
        }

        public void closeConnection()
        {
           
            m_dbConnection.Close();
        }

        // Creates a table named 'highscores' with two columns: name (a string of max 20 characters) and score (an int)
        public  void createTable()
        {
            string sql = "create table Certificate (certname varchar(50), status int,reqid int,RequestDate varchar(50))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        // Inserts some values in the highscores table.
        // As you can see, there is quite some duplicate code here, we'll solve this in part two.
        public  void insertTable(string hostname,int status,int reqid)
        {
            string sql = "insert into Certificate (certname, status,reqid,RequestDate) values (" + "'"+hostname+"'"+","+ status + ","+reqid +","+"'"+ DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.GetCultureInfo("en-US")) +"'"+")";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();     
        }


        public void updateTable(int status, int reqid)
        {
            string sql = "update Certificate set status="+status+" where reqid="+reqid;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        public void printTable()
        {
            string sql = "select * from Certificate";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine("Name: " + reader["certname"] + "\tScore: " + reader["status"] + reader["reqid"] + reader["RequestDate"]);
            }
        }
     
    }
}