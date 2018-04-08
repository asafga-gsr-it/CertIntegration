using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;


namespace CertificateAdmin
{
    public class Database
    {
    

        // Inserts First info For The Certificate 
        public void   InsertToCertificateTable(string hostname, int status, int reqid)
        {
            using (caProjectEntities context = new caProjectEntities())
            {
               
                cert cetificate = new cert()
                {
                    CreationDate= DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.GetCultureInfo("en-US")),
                    RequestId = reqid,
                    HostName = hostname,
                    status = status                   
                };
                context.certs.Add(cetificate);
                context.SaveChanges();
            }
           
        }

        // Inserts First info For The Certificate 
        public void InsertToSigntureTable(string ClientId, string Hash)
            {
            using (caProjectEntities context = new caProjectEntities())
            {

                signature signature = new signature()
                {
                    hash = Hash,
                    uuid= ClientId

                };
                context.signatures.Add(signature);
                context.SaveChanges();
            }

        }

        // update  more info For The Certificate 
        public int UpdateCertificateInfo(string Cert, int reqid)
        {

            StreamWriter objFile = null;            
            try
            {
                objFile = File.CreateText("c:\\" + reqid + ".cer");
                objFile.Write(Cert);
                objFile.Close();
                X509Certificate2 cert = new X509Certificate2("c:\\" + reqid + ".cer");
                File.Delete("c:\\" + reqid + ".cer");

                using (caProjectEntities context = new caProjectEntities())
                {
                    
                    cert cetificate=context.certs.FirstOrDefault(r => r.RequestId == reqid);
                    cetificate.ExpiredDate = Convert.ToDateTime(cert.NotAfter.ToString("yyyy/MM/dd hh:mm:ss tt"));
                    cetificate.Issuedby = cert.Issuer.ToString();
                    cetificate.serialnumber = cert.GetSerialNumberString();
                    context.SaveChanges();                    
                }
                  return 0;
            }
            catch (Exception ex)
            {
                if (objFile!=null)
                {
                    objFile.Close();
                }
                Console.Write(ex.Message);
                return -1;
            }
        }

        //update Certification Status and cert Flag
        public void UpdateUnlockFlagAndStatus(int status, int reqid)
        {
            using (caProjectEntities context = new caProjectEntities())
            {

               cert cetificate = context.certs.FirstOrDefault(r => r.RequestId == reqid);
               cetificate.status = status;
               if (status==3) //issued
               {
                    cetificate.CertFlag = "true";
               }
               context.SaveChanges();
            }     
        }

        //checking if there is allready Request for the hostname
        public int CheckIfCertificateExists(string hostname)
        {
            using (caProjectEntities context = new caProjectEntities())
            {

                cert cetificate = context.certs.FirstOrDefault(r => r.HostName == hostname);
                if (cetificate != null)
                {

                   if (cetificate.Issuedby==null)
                   {
                        return 1;
                    }
                    return 2;
                }
                return 0;
            }
           
        }


        //check if the cerrtficate allready consumed
        public bool CheckIfCertificateConsumed(int reqid)
        {
            try
            {
                using (caProjectEntities context = new caProjectEntities())
                {
                    cert cetificate = context.certs.FirstOrDefault(r => r.RequestId == reqid);
                    if (cetificate.CertFlag == null)
                    {
                        return false;
                    }
                    return true;
                }
            }
           
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return false;
            }

        }

        //check if the hostnameand redid belong to each other
        public bool CheckIfReqIDBelongToHost(int reqid, string hostname)
        {

            try
            {
                using (caProjectEntities context = new caProjectEntities())
                {
                    cert cetificate = context.certs.FirstOrDefault(r => r.RequestId == reqid);
                    if (cetificate.HostName == hostname)
                    {
                        return  false;
                    }
                    return true;
                }
            }

            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return true;
            }           
          
        }

        //update certFlag for unlock
        public void UnlockCetificate(string hostname)
        {

            using (caProjectEntities context = new caProjectEntities())
            {

                cert cetificate = context.certs.FirstOrDefault(r => r.HostName == hostname);
                cetificate.CertFlag = null;
                context.SaveChanges();
            }
            
        }

        public cert ReturnCertificateInformation(string hostname)
        {
           
                using (caProjectEntities context = new caProjectEntities())
                {
                    cert cetificate = context.certs.FirstOrDefault(r => r.HostName == hostname);
                    return cetificate;
                } 
            
        }

        //delete cert record for expired certificate 
        public void DeleteCertificateRecordFromDb(int reqid)
        {

            using (caProjectEntities context = new caProjectEntities())
            {
                cert cetificate = context.certs.FirstOrDefault(r => r.RequestId == reqid);
                context.certs.Remove(cetificate);
                context.SaveChanges();
            }

        }

         //return all the expired certificates
        public List<cert> GetAllExpiredCertificates()
        {
            List<cert> result = null;
             try
            {
                using (caProjectEntities context = new caProjectEntities())
                {
                    result = context.certs.Where(certificate => certificate.ExpiredDate.HasValue && (certificate.ExpiredDate.Value < DateTime.Now)).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return null;
            }

            return result;
        }

    }
}