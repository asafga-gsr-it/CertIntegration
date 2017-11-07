using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CertificateAdmin;

namespace CertificateAdmin
{
    [Authorize]
   [RoutePrefix("api/Cert")]
    public class CertController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        // GET api/Cert/GetStatus?reqid=79
        [Route("GetStatus")]
        [HttpGet]

        public string GetCertStatus(int reqid)
        {
            string status;
            Certificate cert = new Certificate();
            status=cert.retrieveStatus(reqid);
            return status;


        }

        //public string GetCertifcate(string hostname)
        //{
        //    string certID;
        //   Certificate cert = new Certificate();
        //    certID = cert.createCertifcate(hostname);
        //    return certID;

        //}

        // POST api/<controller>
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