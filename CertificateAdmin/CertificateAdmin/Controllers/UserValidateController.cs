using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.DirectoryServices.AccountManagement;
//using SQLite;
namespace CertificateAdmin.Controllers
{
    [RequireHttps]
    [RoutePrefix("api/User")]
    public class UserValidateController : ApiController
    {
        [Route("InsertMachineInfo")]
        [HttpGet]
        ///api/User/InsertMachineInfo?userName=asaf&password=1234&clientid=1234&hash=1234
        public HttpResponseMessage InsertMachineInfo(string userName,string password,string clientid,string hash)
        {
           var resp = new HttpResponseMessage(HttpStatusCode.OK);
           PrincipalContext pc = new PrincipalContext(ContextType.Domain, "YOURDOMAIN");//connect to the AD server           
           bool isValid = pc.ValidateCredentials(userName,password);   //validate the credentials from the Active Directory
            if (isValid) //if the user and password valid insert the new machine info
            {
                Database db = new Database();
                db.InsertToSigntureTable(clientid, hash);
                resp.Content = new StringContent("Success", System.Text.Encoding.UTF8, "application/xml");
                return resp;
            }
            else //the user and password not valid return error and dont insert the machine info
            {
                resp.Content = new StringContent("Error", System.Text.Encoding.UTF8, "application/xml");
                return resp;
            }
         
        }
    }
}
