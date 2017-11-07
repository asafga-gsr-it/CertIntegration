using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CERTENROLLLib;
using CERTCLILib;

namespace CertificateAdmin
{
    public class Certificate
    {

        private const int CC_DEFAULTCONFIG = 0;

        private const int CC_UIPICKCONFIG = 0x1;

        private const int CR_IN_BASE64 = 0x1;

        private const int CR_IN_FORMATANY = 0;

        private const int CR_IN_PKCS10 = 0x100;

        private const int CR_DISP_ISSUED = 0x3;

        private const int CR_DISP_UNDER_SUBMISSION = 0x5;

        private const int CR_OUT_BASE64 = 0x1;

        private const int CR_OUT_CHAIN = 0x100;

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

                objCSP.InitializeFromName("Microsoft Enhanced Cryptographic Provider v1.0");
                objCSPs.Add(objCSP);
                objPrivateKey.Length = 1024;
                objPrivateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE;
                objPrivateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_ALL_USAGES;
                objPrivateKey.MachineContext = false;
                objPrivateKey.CspInformations = objCSPs;
                objPrivateKey.Create();
                objPkcs10.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, objPrivateKey, "");
                objExtensionKeyUsage.InitializeEncode(X509KeyUsageFlags.XCN_CERT_DIGITAL_SIGNATURE_KEY_USAGE | X509KeyUsageFlags.XCN_CERT_NON_REPUDIATION_KEY_USAGE |
                                                       X509KeyUsageFlags.XCN_CERT_KEY_ENCIPHERMENT_KEY_USAGE | X509KeyUsageFlags.XCN_CERT_DATA_ENCIPHERMENT_KEY_USAGE);

                objPkcs10.X509Extensions.Add((CX509Extension)objExtensionKeyUsage);
                objObjectId.InitializeFromValue("2.5.6.1.5.5.7.3.3");
                objObjectIds.Add(objObjectId);

                objX509ExtensionEnhancedKeyUsage.InitializeEncode(objObjectIds);
                objPkcs10.X509Extensions.Add((CX509Extension)objX509ExtensionEnhancedKeyUsage);

                objDN.Encode("CN=" + hostName, X500NameFlags.XCN_CERT_NAME_STR_NONE);
                objPkcs10.Subject = objDN;


                objEnroll.InitializeFromRequest(objPkcs10);

                CertifcateStr = objEnroll.CreateRequest(EncodingType.XCN_CRYPT_STRING_BASE64);

                return CertifcateStr;

            }
            catch (Exception ex)

            {

                return ex.Message;

            }
        }



        public int submitRequest(string certrequest)
        {


            CCertConfig objCertConfig = new CCertConfig();
            CCertRequest objCertRequest = new CCertRequest();

            string strCAConfig;
            int iDisposition;
            int requestID;
            string errorStatus;


            try
            {

                strCAConfig = objCertConfig.GetConfig(CC_DEFAULTCONFIG);
                iDisposition = objCertRequest.Submit(CR_IN_BASE64, certrequest, null, strCAConfig);
                requestID = objCertRequest.GetRequestId();


                return objCertRequest.GetRequestId();
            }

            catch (Exception ex)

            {
                errorStatus = ex.Message;
                return -1;

            }

        }

        public string retrieveStatus(int requestID)
        {

            int iDisposition;
            string strCAConfig;
            string strDisposition;
            CCertConfig objCertConfig = new CCertConfig();
            CCertRequest objCertRequest = new CCertRequest();
            try
            {
                strCAConfig = objCertConfig.GetConfig(CC_DEFAULTCONFIG);
                iDisposition = objCertRequest.RetrievePending(requestID, strCAConfig);
                strDisposition = objCertRequest.GetDispositionMessage();

                return strDisposition;
            }

            catch (Exception ex)

            {

                return ex.Message;

            }

        }

    }

    }
