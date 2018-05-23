using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CertificateAdmin;
using Newtonsoft.Json.Linq;
using static System.Net.WebRequestMethods;

namespace CertificateAdmin
{
     [Authorize]
  // [RequireHttps]

    [RoutePrefix("api/Cert")]
    public class CertController : ApiController
    {

        // return the certifacte status
        // GET api/Cert/GetStatus?reqid=79
        [Route("GetStatus")]
        [HttpGet]

        public int GetRequestStatus(int reqid, string hostname)
        {
            Certificate cert = new Certificate();
            return cert.RetrieveRequestStatus(reqid, hostname);
        }





        // return the certifacte status
        // GET api/Cert/GetStatus?reqid=79



        [Route("GetCert")]
        [HttpGet]
        //return the issue certifcate 
        //GET api/Cert/GetCert? reqid = 79
        public HttpResponseMessage GetCertificate(int reqid)
        {
            Certificate cert = new Certificate();           
            string cerificate = cert.GetCertificate(reqid);
            var resp = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(cerificate, System.Text.Encoding.UTF8, "application/json")
            };
            return resp;
        }



        // create certifcate request
        // POST /api/Cert/Createreq? hostname = asaf
        [Route("Createreq")]
        [HttpPost]
        public int CreateCertifcate(string hostname)
        {
            string CertID;
            int requestID=0;
            Certificate cert = new Certificate();

            try
            {
                CertID = cert.CreateCertifcate(hostname);
                if (String.Equals(CertID, "Exsits") == true)
                {
                    return -2;
                }

                if (String.Equals(CertID, "Issued") == true)
                {
                    return -3;
                }

                if (CertID.Contains("Error") == true)
                {
                    return 0;
                }
                requestID = cert.SubmitRequest(CertID, hostname);
                return requestID;
            }
            catch (Exception ex)
            {
                Database db = new Database();
                db.InsertToErrorMessageTable(hostname, requestID, ex.Message, "CreateCertifcateController");//insert Error Message into The Error Table Log In The DataBase
                Console.Write(ex.Message);
                return 0;
            }

        }


        // unlock certifcate
        // POST /api/Cert/unlockCert? hostname=asaf&clientid=1234
        [Route("unlockCert")]
        [HttpPost]
        public int UnlockCertificate(string hostname)
        {
            Certificate cert = new Certificate();
            try
            {
                return cert.UnlockCertifcate(hostname);
            }
            catch (Exception ex)
            {
                Database db = new Database();
                db.InsertToErrorMessageTable(hostname,0, ex.Message, "UnlockCertificateController");//insert Error Message into The Error Table Log In The DataBase
                Console.Write(ex.Message);
                return -1;
            }
        }

   
        // Renew Expired Certifcate
        // Get /api/Cert/renewCert? hostname=asaf
        [Route("renewCert")]
        [HttpGet]
        public int RenewCertificate(string hostname)
        {
            int reqid=0;
            string cerificate;
            Database db = new Database();
            try
            {

               
                var certReturn = db.ReturnCertificateInformation(hostname);
                Certificate cert = new Certificate();
                cerificate = cert.GetCertificate(certReturn.RequestId);
                JObject obj = JObject.Parse(cerificate);
                string name = (string)obj["CertValue"];
                reqid = cert.RenewCert(name, certReturn.RequestId);
                return reqid;
            }
            catch (Exception ex)
            {
                db.InsertToErrorMessageTable(hostname,reqid, ex.Message, "RenewCertificateController");//insert Error Message into The Error Table Log In The DataBase
                Console.Write(ex.Message);
                return 0;
            }
        }


        // Renew All Expired Certifcate
        // POST /api/Cert/renewAllCert
        [Route("renewAllCert")]
        [HttpPost]
        public HttpResponseMessage RenewAllCertificates()
        {
            int reqid=0;
            Database db = new Database();
            try
            {
                var jsonObject = new JObject();
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
              

                List<cert> certs = db.GetAllExpiredCertificates();
                if (certs.Count>0)
                {
                    for (var i = 0; i < certs.Count; i++)
                    {

                        jsonObject.Add("Host" + i, certs[i].HostName);

                        reqid = RenewCertificate(certs[i].HostName);

                        jsonObject.Add("reqid" + i, reqid);
                    }


                    resp.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject), System.Text.Encoding.UTF8, "application/json");
                    return resp;
                }
                   
                else
                {
                    resp.Content = new StringContent("No Certificates to Renew", System.Text.Encoding.UTF8, "application/json");
                    return resp;
                }
                    
                        
              
            }
            catch (Exception ex)
            {
                db.InsertToErrorMessageTable("",0, ex.Message, "RenewAllCertificatesController");//insert Error Message into The Error Table Log In The DataBase
                var resp = new HttpResponseMessage(HttpStatusCode.ExpectationFailed)
                {
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ex.Message), System.Text.Encoding.UTF8, "application/json")
                };
                return resp;
            }
            
        }

        // create certifcate request
        // Get /api/Cert/revokeCert? hostname = asaf
        [Route("revokeCert")]
        [HttpGet]
        public string RevokCertifcate(string hostname)
        {
            Database db = new Database();
            Certificate cert = new Certificate();

            try
            {
                  var certReturn= db.ReturnCertificateInformation(hostname);              
                  db.DeleteCertificateRecordFromDb(certReturn.RequestId);
                  cert.RevokeCertificate(certReturn.serialnumber);
                return "SUCCESS";
            }

            catch (Exception ex)
            {
                db.InsertToErrorMessageTable(hostname, 0, ex.Message, "RevokCertifcateController");//insert Error Message into The Error Table Log In The DataBase
                return ex.Message;
            }

        }

        // create certifcate request
        // Get /api/Cert/ReCreateCert? hostname = asaf
        [Route("ReCreateCert")]
        [HttpGet]
        public int RecreateCertifcate(string hostname)
        {
          
            Certificate cert = new Certificate();

            try
            {
                RevokCertifcate(hostname);
                return CreateCertifcate(hostname);
            }

            catch (Exception ex)
            {
                Database db = new Database();
                db.InsertToErrorMessageTable(hostname, 0, ex.Message, "RecreateCertifcateController");//insert Error Message into The Error Table Log In The DataBase
                Console.Write(ex.Message);
                return 0;
            }

        }

    }
}