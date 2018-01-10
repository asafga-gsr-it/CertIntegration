// Author: Tigran Gasparian
// This sample is part Part One of the 'Getting Started with SQLite in C#' tutorial at http://www.blog.tigrangasparian.com/

using System;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography.X509Certificates;


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
            string sql = "create table Certificate (certname varchar(50), status int,reqid int,RequestDate varchar(50),ExpirationDate varchar(50),Issuedby varchar(50),Issuedto varchar(50),certFlag varchar(50))";
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

        public void updateCertInfo(string Cert, int reqid)
        {

            StreamWriter objFile = null;
            objFile = File.CreateText(reqid + ".cer");
            objFile.Write(Cert);
            objFile.Close();
            X509Certificate2 cert = new X509Certificate2(reqid + ".cer");
            string expirtationdate = cert.NotAfter.ToString();
            string issuedby = cert.Issuer.ToString(); 
            string issuedto = cert.Subject.ToString();
            File.Delete(reqid + ".cer");

            string sql = "update Certificate set ExpirationDate=" +"'"+ expirtationdate +"'"+ ","+ "Issuedby=" + "'"+issuedby +"'"+"," + "Issuedto="+ "'"+issuedto+"'"+ 
            " where reqid=" + reqid;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }


        public void updateTable(int status, int reqid)
        {
            string sql;
            if (status== 3)
            {
               sql = "update Certificate set status=" + status + "," +"certFlag='true'" +" where reqid=" + reqid;
            }
            else
            {
                 sql = "update Certificate set status=" + status + " where reqid=" + reqid;
            }     
                 
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
        public int checkCertExsits(string hostname)
        {

            string sql = "select * from Certificate where certname="+"'"+hostname+"'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if  (reader.Read())
            {
                if (string.IsNullOrEmpty(reader["Issuedby"].ToString()))
                {
                    return 1;
                }
                return 2;
            }

            return 0;
        }

        public bool checkcertFlag(int reqid)
        {

            string sql = "select * from Certificate where reqid=" + reqid ;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (string.IsNullOrEmpty(reader["certFlag"].ToString()))
            {
                return false;
            }

            return true;
        }


    }
}