using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CertificateAdmin;
using Newtonsoft.Json.Linq;

namespace CertificateAdmin
{
  // [Authorize]
  // [RequireHttps]

    [RoutePrefix("api/Cert")]
    public class CertController : ApiController
    {

       
     


        // return the certifacte status
        // GET api/Cert/GetStatus?reqid=79
        [Route("GetStatus")]
        [HttpGet]

        public int GetCertStatus(int reqid,string hostname)
        {
            Certificate cert = new Certificate();
            return cert.retrieveStatus(reqid,hostname);          
        }


        [Route("GetCert")]
        [HttpGet]
        //return the issue certifcate 
        //GET api/Cert/GetCert? reqid = 79
        public HttpResponseMessage GetCertificate(int reqid)
        {
            Certificate cert = new Certificate();           
            string cerificate = cert.getCertificate(reqid);
            var resp = new HttpResponseMessage(HttpStatusCode.OK);          
            resp.Content = new StringContent(cerificate, System.Text.Encoding.UTF8, "application/json");            
            return resp;
        }



        // create certifcate request
        // POST /api/Cert/Createreq? hostname = asaf
        [Route("Createreq")]
        [HttpPost]
        public int CreateCertifcate(string hostname)
        {
            string CertID;
            int requestID;
            Certificate cert = new Certificate();

            try
            {
                CertID = cert.createCertifcate(hostname);
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
                requestID = cert.submitRequest(CertID, hostname);
                return requestID;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return 0;
            }

        }


        // unlock certifcate
        // POST /api/Cert/unlockCert? hostname=asaf&clientid=1234
        [Route("unlockCert")]
        [HttpPost]
        public int unlockCertFlag(string hostname)
        {
            Certificate cert = new Certificate();
            try
            {
                return cert.unlockCert(hostname);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return -1;
            }
        }

   
        // Renew Expired Certifcate
        // Get /api/Cert/renewCert? hostname=asaf
        [Route("renewCert")]
        [HttpGet]
        public int renewCert(string hostname)
        {
            int reqid;
            string cerificate;
            try
            {

                database db = new database();
                var certReturn = db.returnCertInfo(hostname);
                Certificate cert = new Certificate();
                cerificate = cert.getCertificate(certReturn.RequestId);
                JObject obj = JObject.Parse(cerificate);
                string name = (string)obj["CertValue"];
                reqid = cert.RenewCert(name, certReturn.RequestId);
                return reqid;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return 0;
            }
        }


        // Renew All Expired Certifcate
        // POST /api/Cert/renewAllCert
        [Route("renewAllCert")]
        [HttpPost]
        public HttpResponseMessage renewAllCerts()
        {
            int reqid;
            try
            {
                var jsonObject = new JObject();
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                database db = new database();

                List<cert> certs = db.certExpired();
                if (certs.Count>0)
                {
                    for (var i = 0; i < certs.Count; i++)
                    {

                        jsonObject.Add("Host" + i, certs[i].HostName);

                        reqid = renewCert(certs[i].HostName);

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
                var resp = new HttpResponseMessage(HttpStatusCode.ExpectationFailed);
                resp.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(ex.Message), System.Text.Encoding.UTF8, "application/json");
                return resp;
            }
            
        }

        // create certifcate request
        // Get /api/Cert/revokeCert? hostname = asaf
        [Route("revokeCert")]
        [HttpGet]
        public string revokCertifcate(string hostname)
        {
            database db = new database();
            Certificate cert = new Certificate();

            try
            {
                  var certReturn= db.returnCertInfo(hostname);              
                  db.deleteCertRecord(certReturn.RequestId);
                  cert.revokeCert(certReturn.serialnumber);
                return "SUCCESS";
            }

            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        // create certifcate request
        // Get /api/Cert/ReCreateCert? hostname = asaf
        [Route("ReCreateCert")]
        [HttpGet]
        public int recreateCertifcate(string hostname)
        {
          
            Certificate cert = new Certificate();

            try
            {
                revokCertifcate(hostname);
                return CreateCertifcate(hostname);
            }

            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return 0;
            }

        }

    }
}