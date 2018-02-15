using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CertificateAdmin;
using SQLiteSamples;
using Newtonsoft.Json.Linq;

namespace CertificateAdmin
{
    //[Authorize]
    [RequireHttps]

    [RoutePrefix("api/Cert")]
    public class CertController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // return the certifacte status
        // GET api/Cert/GetStatus?reqid=79
        [Route("GetStatus")]
        [HttpGet]

        public int GetCertStatus(int reqid,string hostname)
        {
            int  status;
            Certificate cert = new Certificate();
            status=cert.retrieveStatus(reqid,hostname);
          //  var resp = new HttpResponseMessage(HttpStatusCode.OK);
//resp.Content = new StringContent(status, System.Text.Encoding.UTF8, "text/plain");
            return status;
        }


        [Route("GetCert")]
        [HttpGet]
        //return the issue certifcate 
        //GET api/Cert/GetCert? reqid = 79
        public HttpResponseMessage GetCertificate(int reqid)
        {
            string cerificate;

            Certificate cert = new Certificate();
            cerificate = cert.getCertificate(reqid);
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            // resp.Content = new StringContent(cerificate, System.Text.Encoding.UTF8, "text/plain");
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
            CertID = cert.createCertifcate(hostname);
            if (String.Equals(CertID,"Exsits")==true)
            {
                return -2;
            }

            if (String.Equals(CertID, "Issued") == true)
            {
                return -3;
            }
            requestID = cert.submitRequest(CertID, hostname);
            return requestID;

        }


        // unlock certifcate
        // POST /api/Cert/unlockCert? hostname=asaf&clientid=1234
        [Route("unlockCert")]
        [HttpPost]
        public int unlockCertFlag(string hostname,string clientid)
        {
            int status;
            Certificate cert = new Certificate();
            status = cert.unlockCert(hostname, clientid);
            return status;

        }


        // Renew certifcate
        // POST /api/Cert/renewCert? hostname=asaf
        [Route("renewCert")]
        [HttpPost]
        public int renewCert(string hostname)
        {
            int reqid;
            string cerificate;
            SqlLite sql = new SqlLite();
            reqid =sql.returnCertInfo(hostname);
            Certificate cert = new Certificate();
            cerificate= cert.getCertificate(reqid);
            JObject obj = JObject.Parse(cerificate);            
            string name = (string)obj["CertValue"];
            reqid =cert.RenewCert(name,reqid);
            return reqid;
        }


        // Renew certifcate
        // POST /api/Cert/renewAllCert
        [Route("renewAllCert")]
        [HttpGet]
        public void renewAllCerts()
        {
          
            SqlLite sql = new SqlLite();
            List<string> list = sql.certExpired();

            for (var i = 0; i < list.Count; i++)
            {
                renewCert(list[i]);
            }

        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}