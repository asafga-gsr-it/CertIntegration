﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CERTENROLLLib;
using CERTCLILib;
using CERTADMINLib;
using System.IO;
using SQLite;
using System.Security.Cryptography.X509Certificates;

namespace CertificateAdmin
{
    public  class Certificate
    {

        private const int CC_DEFAULTCONFIG = 0;
        private const int CR_IN_BASE64 = 0x1;
        private const int CR_OUT_BASE64 = 0x0;
        private const int CR_OUT_CHAIN = 0x100;
        public string CertValue { get; set; }

        // create the certifcate request
        public string createCertifcate(string hostName)
        {

            //  Create all the objects that will be required
            CX509CertificateRequestPkcs10 objPkcs10 = new CX509CertificateRequestPkcs10();
            CX509PrivateKey objPrivateKey = new CX509PrivateKey();
            CCspInformation objCSP = new CCspInformation();
            CCspInformations objCSPs = new CCspInformations();
            CX500DistinguishedName objDN = new CX500DistinguishedName();
            CX509Enrollment objEnroll = new CX509Enrollment();
            CObjectIds objObjectIds = new CObjectIds();
            CObjectId objObjectId = new CObjectId();
            CX509ExtensionKeyUsage objExtensionKeyUsage = new CX509ExtensionKeyUsage();
            CX509ExtensionEnhancedKeyUsage objX509ExtensionEnhancedKeyUsage = new CX509ExtensionEnhancedKeyUsage();
            string CertifcateStr;

            try
            {
                SqlLite sql = new SqlLite();        
               /*Check if there is allready request for the hostname so we dont need to create new one*/
                if (sql.checkCertExsits(hostName)==1)
                {                  
                    return "Exsits";
                }

                if (sql.checkCertExsits(hostName) == 2)
                {
                    return "Issued";
                }
                //create the private key (CX509CertificateRequestPkcs10 will initilizae from the private key)
                objCSP.InitializeFromName("Microsoft Enhanced Cryptographic Provider v1.0");
                objCSPs.Add(objCSP);
                objPrivateKey.Length = 1024;
                objPrivateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE;
                objPrivateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_ALL_USAGES;
                objPrivateKey.MachineContext = false;
                objPrivateKey.CspInformations = objCSPs;
                objPrivateKey.Create();


                //create  pkc10 object from the privaet key
                objPkcs10.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, objPrivateKey, "");
                objExtensionKeyUsage.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_NON_REPUDIATION_KEY_USAGE |
                                                       CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE | CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_DATA_ENCIPHERMENT_KEY_USAGE);

               // objPkcs10.X509Extensions.Add((CX509Extension)objExtensionKeyUsage);
               // objObjectId.InitializeFromValue("1.3.6.1.5.5.7.3.2");
               // objObjectIds.Add(objObjectId);

              //  objX509ExtensionEnhancedKeyUsage.InitializeEncode(objObjectIds);
               // objPkcs10.X509Extensions.Add((CX509Extension)objX509ExtensionEnhancedKeyUsage);

                objDN.Encode("CN=" + hostName, X500NameFlags.XCN_CERT_NAME_STR_NONE); //create DistinguishedName
                objPkcs10.Subject = objDN;  //initial the  DistinguishedName              
                objEnroll.InitializeFromRequest(objPkcs10);  //init enrollement request             
                CertifcateStr = objEnroll.CreateRequest(EncodingType.XCN_CRYPT_STRING_BASE64); //Certifcate  Request
                return CertifcateStr;

            }
            catch (Exception ex)
            {
                return "Error"+ex.Message;
            }
        }


        //submit the request  that created in the createCertifcate to the CA
        public int submitRequest(string certrequest,string hostname)
        {                  
            CCertConfig objCertConfig = new CCertConfig(); 
            CCertRequest objCertRequest = new CCertRequest();
            CCertAdmin objCertAdmin = new CCertAdmin();
            string strCAConfig;
            int iDisposition;
            int requestID;
            string errorStatus;

            try
            {                
                strCAConfig = objCertConfig.GetConfig(CC_DEFAULTCONFIG);//connect to the ca                              
                iDisposition = objCertRequest.Submit(CR_IN_BASE64, certrequest, null, strCAConfig);   //submit the certiface request to the ca
                requestID = objCertRequest.GetRequestId(); //get the requestid that was created -the certifacte is in pending status
                SqlLite sql = new SqlLite();
                sql.insertTable(hostname, iDisposition, requestID); //insert first certificate information
           //   objCertAdmin.ResubmitRequest(strCAConfig, requestID);
                return requestID; //return the reqid that was created for the certificate request in the pending queue 
            }

            catch (Exception ex)
            {
                errorStatus = ex.Message;
                return 0;
            }
     }
        
        //get the certifacte status from the ca
        public int retrieveStatus(int requestID,string hostname)
        {
            int iDisposition;
            string strCAConfig;
            CCertConfig objCertConfig = new CCertConfig();
            CCertRequest objCertRequest = new CCertRequest();
            try
            {

                SqlLite sql = new SqlLite();
                /*Cheking if host name and req is belong to each other*/
                if (sql.checkHostnameWithreqID(requestID, hostname))
                {
                    return -6;
                }
                if (sql.checkcertFlag(requestID)) //checking if the client allreay consumed the certificate
                {
                    return -3;
                }
          
 
              
                strCAConfig = objCertConfig.GetConfig(CC_DEFAULTCONFIG);   //connect to the ca
                iDisposition = objCertRequest.RetrievePending(requestID, strCAConfig); //retrive the certifcate status  from the ca 
                sql.updateTable(iDisposition, requestID);  //updat certificate table with more information about the cert         
                return iDisposition;//return cert status
            }

            catch (Exception ex)
            {
                return -2;
            }
        }

        //get the issue Certificate from the ca
        public string getCertificate(int requestID)
        {

            int iDisposition;
            int status;
            string strCAConfig;
            string pstrCertificate;
            SqlLite sql = new SqlLite();
            pstrCertificate = null;
            CCertConfig objCertConfig = new CCertConfig();
            CCertRequest objCertRequest = new CCertRequest();
            
            try
            {
                
                strCAConfig = objCertConfig.GetConfig(CC_DEFAULTCONFIG);//connect to the ca     
                iDisposition = objCertRequest.RetrievePending(requestID, strCAConfig); //getting certificate stauts must before getting the cert
                pstrCertificate = objCertRequest.GetCertificate(CR_OUT_BASE64);     //retrive the Certificate                 
                status =sql.updateCertInfo(pstrCertificate, requestID); //update cert with more information
                if (status == 0) 
                {
                    Certificate cert = new Certificate { CertValue = pstrCertificate }; //creatre cert with JSON type
                    string certJson = Newtonsoft.Json.JsonConvert.SerializeObject(cert);//creatre cert with JSON type
                    return certJson; //return certificate
                }

                else
                {                    
                    return "error Update Certificate Table";
                }
            }

            catch (Exception ex)
            {
                return "error"+ex.Message;
            }
        }
        //unlock certificate in stauts consumed that client can get it one more time
        public int unlockCert(string hostname)
        {
            SqlLite sql = new SqlLite();
            string status;

            try
            {
                sql.updateCertFlag(hostname);    //unlock certificate             
                return 0;
            }

            catch (Exception ex)
            {
                status = ex.Message;
                return -1;
            }

        }
        //rennew certficiate that expired
        public int RenewCert(string Cert,int reqid)
        {
            int iDisposition;
            string CertifcateStr;
            string status;
            string HostName;
            CX509CertificateRequestPkcs10 objPkcs10 = new CX509CertificateRequestPkcs10();
            CX509Enrollment objEnroll = new CX509Enrollment();
            CCertConfig objCertConfig = new CCertConfig();
            CX500DistinguishedName objDN = new CX500DistinguishedName();
            string strCAConfig;
            var inheritOptions = X509RequestInheritOptions.InheritPrivateKey |X509RequestInheritOptions.InheritSubjectFlag | X509RequestInheritOptions.InheritExtensionsFlag | X509RequestInheritOptions.InheritSubjectAltNameFlag; 
     
            try
            {
                strCAConfig = objCertConfig.GetConfig(CC_DEFAULTCONFIG);//connect to the  ca
                InstallCert(Cert);
                objPkcs10.InitializeFromCertificate(X509CertificateEnrollmentContext.ContextUser, Cert, EncodingType.XCN_CRYPT_STRING_BASE64HEADER, inheritOptions);//create new cert request from exists expired cert
                objDN = objPkcs10.Subject;//getting old cert subject (hostname)
                HostName = objDN.Name.ToString().Substring(3);
                objEnroll.InitializeFromRequest(objPkcs10);//create enroll rquest
                CertifcateStr = objEnroll.CreateRequest(EncodingType.XCN_CRYPT_STRING_BASE64);//crearte  new cert request
                iDisposition=submitRequest(CertifcateStr,HostName);//submit cert to the ca

                if  (iDisposition>0)//if cert was created delete the old cert from the table
                {
                    SqlLite sql = new SqlLite();
                    sql.deleteCertRecord(reqid);
                    deleteFromStore(objDN.Name.ToString());
                    return iDisposition;
                }
                 return 0;
            }

            catch (Exception ex)
            {
                status = ex.Message;
                return 1;
            }

        }
        /*Remove Certificate From CA Store -After renew Expired Certificate
         *  */
        public void  deleteFromStore(string subjectName)
        {
             X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite | OpenFlags.IncludeArchived);

            // You could also use a more specific find type such as X509FindType.FindByThumbprint
            X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, subjectName, false);

            foreach (var cert in col)
            {

                revokeCert(cert.SerialNumber);
                // Remove the certificate
                store.Remove(cert);
            }
            store.Close();
        }

        /*Install Certificate On the Machine For future Renew Expired Certificate */
        public  int InstallCert(string Cert)
        {

            //  Create all the objects that will be required
            CX509Enrollment objEnroll = new CX509Enrollment();
            try
            {
                // Install the certificate
                objEnroll.Initialize(X509CertificateEnrollmentContext.ContextUser);
                objEnroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedRoot,Cert,EncodingType.XCN_CRYPT_STRING_BASE64HEADER,null);
                return 0;
            }

            catch (Exception ex)

            {

                return 1;

            }

        }

        /*Revock Certificate */

        public int revokeCert(string serialNumber)
        {
             
            CCertConfig objCertConfig = new CCertConfig();
            CCertAdmin objCertAdmin = new CCertAdmin();
            try
            {
                string strCAConfig = objCertConfig.GetConfig(CC_DEFAULTCONFIG);//connect to the ca     
                objCertAdmin.RevokeCertificate(strCAConfig, serialNumber, 0, DateTime.Now);
                return 0;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

    }

    }
;