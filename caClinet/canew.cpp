#include <fstream>
#include <iostream>
#include <string>
#include <curl/curl.h>
#include <sys/stat.h>
#include "/usr/include/jansson.h"
#include <stdio.h>
#include <cstring>
using namespace std;
std::string url; 
std::string fileloc;
std::string ouidg;
std::string  hashg;

/*copy the value return from the HTTP Get to the memorty*/
size_t WriteCallback(char *contents, size_t size, size_t nmemb, void *userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}
/*Create And Excute Rest Request*/
std::string RestUrl(std::string opreation,std::string  url,std::string  token,std::string data)
{
    curl_global_init(CURL_GLOBAL_ALL); /*Global libcurl initialisation*/
    std::string readBuffer;
    std::string serverurl;
    std::string  header;
    int res;
      
    header="Authorization: Bearer "+token;
    struct curl_slist *chunk = NULL;
      
    CURL* easyhandle = curl_easy_init();  /* init the curl session */ 
    if (easyhandle)
    {
        chunk = curl_slist_append(chunk,header.c_str());
    
     
        curl_easy_setopt(easyhandle, CURLOPT_HTTPHEADER, chunk);   /*specify Header to Request*/ 
        curl_easy_setopt(easyhandle, CURLOPT_URL,url.c_str());     /*specify URL to Request */ 
        curl_easy_setopt(easyhandle, CURLOPT_SSL_VERIFYPEER, 0L);   /*verify the peer's SSL certificate*/

        if  (opreation=="POST")  /* Create for Post Request only */ 
        {
          curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS,data.c_str()); /*specify data to POST to server*/
        }
         
        curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);   /* send all data to this function  to save the Return Value */  
        curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);/* the Data Return from the request will be writting in readBuffer */
        curl_easy_setopt(easyhandle, CURLOPT_TIMEOUT, 60L); /* specify TimeOut  to Request */ 

        
        res=curl_easy_perform(easyhandle); /*Perform Http Rest Get or Post */

        curl_easy_cleanup(easyhandle);  /* End a libcurl easy handle */ 
        if (res!=0)  /* Error Occured return -1 the handler the error */ 
        {
           return "-1";
        }
        return readBuffer.c_str();
    }
    return "-1";
}

/*Create Certificate Request to Send To  the Microsoft CA*/
int requestCert(std::string   hostname,std::string  token)
{
 
    std::string serverurl;
    std::string res;
    serverurl=url;
 
    serverurl+= "/api/Cert/Createreq?hostname=" + hostname; /* Create  the Url For The rest request */
    res=RestUrl("POST",serverurl,token,"data_to_send");/* Call  the Rest Function */

    if (res!="-1") /* Success Request  */
    {
      return std::atoi(res.c_str());        
    }
    return -1; 
}
/*Getting  Token For the Rest Requests from the server*/
std::string requestToken(std::string clientid,std::string clientSecret)
{
    std::string serverurl;
    std::string data;
    json_error_t error;
    json_t *root;
    const char * token;
    std::string res;
   
    data = "client_id="+clientid+"&client_secret="+clientSecret+"&grant_type=client_credentials"; /*Data For the Post Filed for  Rest Request */  
    serverurl=url;
    serverurl+= "/Token";  /* Create  the Url For The rest request */ 
    
    res=RestUrl("POST",serverurl,token,data.c_str()); /* Call  the Rest Function */
    if (res=="-1") /*Error With the Rest Request*/
    {
      return "Error Connecting";
    }       
    root = json_loads(res.c_str(), 0, &error); /*Create Json Obeject from the json Result from the server */
    token = json_string_value(json_object_get(root, "access_token"));/*Geting  The Token that Return from the server from json Reply*/
    if (token==NULL)  /*Server didnt Return Token --Dont have permission*/
    {
      token = json_string_value(json_object_get(root, "error_description"));
    }
     return token; 
}
/*Get Certificate Status*/
int  getCertStatus(int   reqid,std::string hostName,std::string  token)
{
    std::string serverurl;
    std::string res;
   
    serverurl=url;
    
    serverurl+="/api/Cert/GetStatus?reqid="+std::to_string(reqid)+"&"+"hostname="+hostName;     /* Create  the Url For The rest request */ 
    res=RestUrl("GET",serverurl,token,"data_to_send"); /* Call  the Rest Function */
   if (res!="-1")
    {
      return std::atoi(res.c_str());          
    }
   return -1;
    
}

/*Return the Certificate That Was Issued by The Server*/
int  getCertificate(int   reqid,std::string  token)
{
  const char * cert;
  json_error_t error;
  json_t *root;
  std::string res;
  std::string serverurl;
  serverurl=url;
  serverurl+="/api/Cert/GetCert?reqid="+std::to_string(reqid);     /* Create  the Url For The rest request */   
  res=RestUrl("GET",serverurl,token,"data_to_send"); /* Call  the Rest Function */
  if (res!="-1" and  not(strstr(res.c_str(),"error"))) /*Error Getting the Certificate from the server*/
  {
      root = json_loads(res.c_str(), 0, &error);
      cert = json_string_value(json_object_get(root, "CertValue"));
      /*open A file to Write -- cert.cert*/
      std::ofstream out(fileloc);
      /*Write The Certificate To the File*/
      out << cert;
      /*Close the File */
      out.close();
      return 0;
    }
    return -1;
}
/*Getting Machine Info To Validate the machine for the token Request*/
void createSigniture()
{
  FILE *fp;
  char hashtemp[300];
  char ouidtemp[300];

  fp = popen("./sha.sh", "r");  /* Open the command for reading. */
  if (fp == NULL) {
    printf("Failed to run command\n" );
    exit(1);
  }
  
  fgets(hashtemp, sizeof(hashtemp)-1, fp);  /* Read the output a line at a time - output it. */
  fgets(ouidtemp, sizeof(ouidtemp)-1, fp);  /* Read the output a line at a time - output it. */
      /* close */
  pclose(fp);
    hashtemp[strcspn(hashtemp, "\n")] = 0;
    ouidtemp[strcspn(ouidtemp, "\n")] = 0;

    hashg=hashtemp;
    ouidg=ouidtemp;
}


int main(int argc, char * argv[])
{
 
  int reqid;
  int  status;
  int certstatus;
  const char * errorMessage; 
  const char* reqidStatus;
  std::string token;
  struct stat st;
  FILE* f;
  json_error_t error;
  json_t *root, *obj;

  createSigniture(); /*Getting Machine Info For the Token Request*/
  if (argv[2]!=NULL)  /*Checking if The Script Send Cert Reqid*/
  {
     reqid=std::atoi(argv[2]);
  }
  /*Getting information from the Configuration file --url and file location to save the Certificate */
  f = fopen ("caw.conf" , "r");    
  root = json_loadf(f, 0, &error);
  url = json_string_value(json_object_get(root, "url"));
  fileloc=json_string_value(json_object_get(root, "file"));
  fileloc+=argv[1];
  fileloc+=".cer";


  token=requestToken(hashg,ouidg);  /*Getting the Token*/
     if (token.find("Error",0)==0)  /*Error Getting Token Cannot Continue with out the token End Script With Error */
     {
           errorMessage=token.c_str();
           goto result;
     }
    
      /*Create New Certificat Request -Didnt Get Reqid From the script*/
    if (argv[2]==NULL) 
    {
       
        reqid=requestCert(argv[1],token.c_str()); 
         /*Error Handling */
        if (reqid==-1)
         {
            errorMessage="There is network Problem";
            goto result;

         }
          else if (reqid==-2)
         {
           
             errorMessage="The Certificate Was Allreday Requested";
             goto result;
         }
            else if (reqid==-3)
         {
            errorMessage="The Certificate Was Allreday Issued and Used";
            reqidStatus="Consumed";
            goto result;
         }
          else if (reqid==0)
         {
            errorMessage="The Certificate Was Not Create Due To a Problem";
            goto result;
         }
    }
 
 
    /*Getting Certificate Status*/
    status=getCertStatus(reqid,argv[1],token.c_str());

    /*Handling  Certificate Status + Error*/
     if (status==3) 
    {
         reqidStatus="Issued";
         certstatus=getCertificate(reqid,token.c_str());
        if (certstatus==-1)
        {
          errorMessage="There is network Problem";
           goto result;
        }
    
    }
       else if (status==-6)
    {
       errorMessage="The Reqid is not belong to the hostname";
       
        goto result;
    }
    else if (status==-3)
    {
       errorMessage="The Certificate Was Allreday Issued and used ";
       reqidStatus="Consumed";
        goto result;
    }
    else if (status==-1)
    {
        errorMessage="There is network Problem";
       goto result;
    }
      else if (status==2)
    {
        reqidStatus="Denied";
        goto result;
    }  
    else if (status==5)
    {
        reqidStatus="Pending";
        goto result;
    }  
     else if (status==-2)
    {
        errorMessage="There is Problem Getting Cert Status";
        goto result;
    }  

    /*Creating The Script Result in JSON Fromat*/
    result:
          if (argv[2]!=NULL) 
          {
            reqid=std::atoi(argv[2]);
          }
          obj = json_object();
          if (reqid>0) 
          {
            json_object_set(obj, "HostName", json_string(argv[1]));
            json_object_set(obj, "Reqid", json_integer(reqid));       
            json_object_set(obj, "CertStatus", json_string(reqidStatus));
            json_object_set(obj, "CertPath", json_string(fileloc.c_str()));            
          }
          json_object_set(obj, "ErrorMessage", json_string(errorMessage));
          char* result=json_dumps(obj,0);
          cout<<result<<endl;
          json_object_clear(obj);
    return 0;
}