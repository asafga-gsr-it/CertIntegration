using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace SQLite
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

        // Creates a table Certificate
        public void createTable()
        {
            string sql = "create table Certificate (certname varchar(50), status int,reqid int,RequestDate varchar(50),ExpirationDate DATETIME,Issuedby varchar(50),Issuedto varchar(50),certFlag varchar(50),serialnumber varchar(200))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }
        // Creates a table Host
        public void createTableHost()
        {
            string sql = "create table Hosts (HostID varchar(100),Hash varchar(100))";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            sql = "insert into Hosts (HostID,Hash) values (" + "'" + "EC222391-ED2E-D25C-FAC8-E00E41AC8030" + "'" + "," + "'" + "3aba57e77256c95043152b8006264c3ccbf88413037044fc9fcc029146932616" + "'" + ")";
            command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        // Inserts First info For The Certificate 
        public void insertTable(string hostname, int status, int reqid)
        {
            connectToDatabase();
            string sql = "insert into Certificate (certname, status,reqid,RequestDate) values (" + "'" + hostname + "'" + "," + status + "," + reqid + "," + "'" + DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "'" + ")";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            closeConnection();
        }

        // Inserts into Tables Hoosts 
        public void insertTableHost(string hostname,string hash)
        {
            connectToDatabase();
            string sql = "insert into Hosts  (HostID,Hash) values (" + "'" + hostname + "'" + "," + "'"+ hash +"'"+ ")";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            closeConnection();
        }
        // update  more info For The Certificate 
        public int  updateCertInfo(string Cert, int reqid)
        {
            string status;
            StreamWriter objFile = null;
            objFile = File.CreateText(reqid + ".cer");
            objFile.Write(Cert);
            objFile.Close();
            X509Certificate2 cert = new X509Certificate2(reqid + ".cer");
            string expirtationdate = cert.NotAfter.ToString();
            string expiredDate = Convert.ToDateTime(expirtationdate).ToString("yyyy-MM-dd HH:mm:ss");
            string issuedby = cert.Issuer.ToString();
            string issuedto = cert.Subject.ToString();
            string serialnumber = cert.GetSerialNumberString();
            File.Delete(reqid + ".cer");
            connectToDatabase();
            string sql = "update Certificate set ExpirationDate="+"'"+ expiredDate + "'"+"," + "Issuedby=" + "'" + issuedby + "'" + "," + "Issuedto=" + "'" + issuedto + "'" +","+ "serialnumber="+"'"+ serialnumber + "'"+
            " where reqid=" + reqid;
            try
            {
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                closeConnection();
                return 0;
            }
            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();
                return -1;
            }
        }

        //update Certification Status and cert Flag
        public void updateTable(int status, int reqid)
        {

            string sql;

            try
            {
                connectToDatabase();

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
            catch (Exception ex)
            {
               
                closeConnection();
          
            }
        }
        //checking if there is allready Request for the hostname
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
        //check if the cerrtficate allready consumed
        public bool checkcertFlag(int reqid)
        {
            string sql;
            string status;
            try
            {

                connectToDatabase();
                sql = "select * from Certificate where reqid=" + reqid;
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
            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();
                return false;
            }

        }
        //check if the hostnameand redid belong to each other
        public bool checkHostnameWithreqID(int reqid, string hostname)
        {
            string sql;
            string status;
            try
            {
                connectToDatabase();
                sql = "select * from Certificate where reqid=" + reqid + " and certname=" + "'" + hostname + "'";
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
            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();
                return true;
            }
        }
        //check if the client is approve to get token --check signture
        public bool checkClientWithHash(string clientID, string hash)
        {
            string sql;
            string status;

            try
            {
                connectToDatabase();
                sql = "select * from Hosts where HostID=" + "'" + clientID + "'" + " and Hash=" + "'" + hash + "'";
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
            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();
                return true;
            }
        }

        
        //update certFlag for unlock
        public void updateCertFlag(string hostname)
        {
            string sql;
            string status;
            try
            {
                connectToDatabase();
                sql = "update Certificate set certFlag=NULL" + " where certname=" + "'" + hostname + "'";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                command.ExecuteNonQuery();
                closeConnection();
            }
            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();             
            }
        }
        public string returnCertSerialnumber(string hostname)
        {
            string status;
            string serialnumber;        
            try
            {
                connectToDatabase();
                string sql = "select * from Certificate where certname=" + "'" + hostname + "'";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                serialnumber = reader["serialnumber"].ToString();
                closeConnection();
                return serialnumber;
            }

            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();
                return status;
            }
        }
        public int returnCertInfo(string hostname)
        {
            string status;            
            int reqid;
            try
            {
                connectToDatabase();
                string sql = "select * from Certificate where certname=" + "'" + hostname + "'";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                reqid = Int32.Parse(reader["reqid"].ToString());
                closeConnection();
                return reqid;
            }
               
            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();
                return 0;
            }
        }
        //delete cert record for expired certificate 
        public void deleteCertRecord(int reqid)
        {
            string status;
            try
            {
                connectToDatabase();
                string sql = "delete from  Certificate where reqid=" + reqid;
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                closeConnection();
            }
            catch (Exception ex)

            {
                status = ex.Message;
                closeConnection();
            }

        }
        //return all the expired certificates
        public List<string> certExpired()
        {
            string status;
            List<string> list = new List<string>();
            string s = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");                       
          //  DateTime s = DateTime.Now;
            connectToDatabase();
            try
            {
               // printTable();
                string sql = "select * from Certificate where ExpirationDate>date('"+s+"')";
           //    string sql = "select * from Certificate where ExpirationDate>date('2018-02-23 06:00:00')";
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
                closeConnection();
                status = ex.Message;
                return null;


            }
            
        }

    }

   
}