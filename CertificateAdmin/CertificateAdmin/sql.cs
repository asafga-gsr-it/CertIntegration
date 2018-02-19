﻿// Author: Tigran Gasparian
// This sample is part Part One of the 'Getting Started with SQLite in C#' tutorial at http://www.blog.tigrangasparian.com/

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace SQLiteSamples
{
    public class SqlLite
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
                createTableHost();
                m_dbConnection.Close();
            }
        }

        // Creates a connection with our database file.
        public void connectToDatabase()
        {
            m_dbConnection = new SQLiteConnection("Data Source=MyDatabase.sqlite;Version=3;");
            m_dbConnection.Open();
        }

        public void closeConnection()
        {

            m_dbConnection.Close();
        }

        // Creates a table named 'highscores' with two columns: name (a string of max 20 characters) and score (an int)
        public void createTable()
        {
            string sql = "create table Certificate (certname varchar(50), status int,reqid int,RequestDate varchar(50),ExpirationDate DATETIME,Issuedby varchar(50),Issuedto varchar(50),certFlag varchar(50))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        public void createTableHost()
        {
            string sql = "create table Hosts (HostID varchar(100),Hash varchar(100))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "insert into Hosts (HostID,Hash) values (" + "'" + "EC222391-ED2E-D25C-FAC8-E00E41AC8030" + "'" + "," + "'" + "3aba57e77256c95043152b8006264c3ccbf88413037044fc9fcc029146932616" + "'" + ")";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        // Inserts some values in the highscores table.
        // As you can see, there is quite some duplicate code here, we'll solve this in part two.
        public void insertTable(string hostname, int status, int reqid)
        {
            connectToDatabase();
            string sql = "insert into Certificate (certname, status,reqid,RequestDate) values (" + "'" + hostname + "'" + "," + status + "," + reqid + "," + "'" + DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "'" + ")";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void updateCertInfo(string Cert, int reqid)
        {
            string status;
            StreamWriter objFile = null;
            objFile = File.CreateText(reqid + ".cer");
            objFile.Write(Cert);
            objFile.Close();
            X509Certificate2 cert = new X509Certificate2(reqid + ".cer");
            string expirtationdate = cert.NotAfter.ToString();
            string createddate = Convert.ToDateTime(expirtationdate).ToString("yyyy-MM-dd HH:mm:ss");
          //  DateTime oDate = Convert.ToDateTime(expirtationdate);
         //   oDate = DateTime.ParseExact(expirtationdate,"yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string issuedby = cert.Issuer.ToString();
            string issuedto = cert.Subject.ToString();
            File.Delete(reqid + ".cer");
            connectToDatabase();
            string sql = "update Certificate set ExpirationDate="+"'"+ createddate + "'"+"," + "Issuedby=" + "'" + issuedby + "'" + "," + "Issuedto=" + "'" + issuedto + "'" +
            " where reqid=" + reqid;
            try
            {
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                closeConnection();
            }
            catch (Exception ex)

            {
                status = ex.Message;


            }
        }


        public void updateTable(int status, int reqid)
        {
            connectToDatabase();
            string sql;
            if (status == 3)
            {
                sql = "update Certificate set status=" + status + "," + "certFlag='true'" + " where reqid=" + reqid;
            }
            else
            {
                sql = "update Certificate set status=" + status + " where reqid=" + reqid;
            }

            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public void printTable()
        {
            connectToDatabase();
            string sql = "select * from Certificate";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(reader["ExpirationDate"].ToString());
                //Console.WriteLine("Name: " + reader["certname"] + "\tScore: " + reader["status"] + reader["reqid"] + reader["RequestDate"] + reader["ExpirationDate"] + reader["Issuedby"] + reader["Issuedto"] + reader["certFlag"]);
            }
            closeConnection();
        }
        public int checkCertExsits(string hostname)
        {
            connectToDatabase();
            string sql = "select * from Certificate where certname=" + "'" + hostname + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                if (string.IsNullOrEmpty(reader["Issuedby"].ToString()))
                {
                    closeConnection();
                    return 1;
                }
                closeConnection();
                return 2;
            }
            closeConnection();
            return 0;
        }

        public bool checkcertFlag(int reqid)
        {
            connectToDatabase();
            string sql = "select * from Certificate where reqid=" + reqid;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (string.IsNullOrEmpty(reader["certFlag"].ToString()))
            {
                closeConnection();
                return false;
            }
            closeConnection();
            return true;
        }

        public bool checkHostnameWithreqID(int reqid, string hostname)
        {
            connectToDatabase();
            string sql = "select * from Certificate where reqid=" + reqid + " and certname=" + "'" + hostname + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (string.IsNullOrEmpty(reader["certname"].ToString()))
            {
                closeConnection();
                return true;
            }
            closeConnection();
            return false;
        }

        public bool checkClientWithHash(string clientID, string hash)
        {
            connectToDatabase();
            string sql = "select * from Hosts where HostID=" + "'" + clientID + "'" + " and Hash=" + "'" + hash + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (string.IsNullOrEmpty(reader["HostID"].ToString()))
            {
                closeConnection();
                return true;
            }
            closeConnection();
            return false;
        }


        public bool checkHostExists(string clientID)
        {
            connectToDatabase();
            string sql = "select * from Hosts where HostID=" + "'" + clientID + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            if (string.IsNullOrEmpty(reader["HostID"].ToString()))
            {
                closeConnection();
                return false;
            }
            closeConnection();
            return true;
        }

        public void updateCertFlag(string hostname)
        {
            string sql;
            connectToDatabase();
            sql = "update Certificate set certFlag=NULL" + " where certname=" + "'" + hostname + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            closeConnection();
        }

        public int returnCertInfo(string hostname)
        {
            int reqid;
            connectToDatabase();
            string sql = "select * from Certificate where certname=" + "'" + hostname + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            reqid = Int32.Parse(reader["reqid"].ToString());
            closeConnection();
            return reqid;
        }

        public void deleteCertRecord(int reqid)
        {

            connectToDatabase();
            string sql = "delete from  Certificate where reqid=" + reqid;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            closeConnection();

        }

        public List<string> certExpired()
        {
            string status;
            List<string> list = new List<string>();
            string s = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+2;                       
          //  DateTime s = DateTime.Now;
            connectToDatabase();
            try
            {
               // printTable();
              string sql = "select * from Certificate where ExpirationDate<date('"+s+"')";
                //string sql = "select * from Certificate where ExpirationDate<date('2018-02-19 10:00:00')";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();


                while (reader.Read())
                {
                    list.Add(reader["certname"].ToString());

                }

                closeConnection();
                return list;
            }
            catch (Exception ex)

            {
                status = ex.Message;
               

            }
            return list;
        }

    }

   
}