using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CertificateAdmin;

namespace CertificateAdmin
{
  //  [Authorize]
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

        public int GetCertStatus(int reqid)
        {
            int  status;
            Certificate cert = new Certificate();
            status=cert.retrieveStatus(reqid);
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
            resp.Content = new StringContent(cerificate, System.Text.Encoding.UTF8, "text/plain");
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
            requestID = cert.submitRequest(CertID);
            return requestID;

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