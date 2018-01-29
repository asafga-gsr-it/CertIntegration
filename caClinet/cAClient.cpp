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

//copy the value return from the HTTP Get to the memorty
size_t WriteCallback(char *contents, size_t size, size_t nmemb, void *userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}



//Function to Request  Certificate Request to Send To  the Microsoft CA
int requestCert(std::string   hostname,std::string  token)
{
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init();  /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
     std::string  header;
    int res;
   
    const char *data = "data to send";
  
    header="Authorization: Bearer "+token;
    struct curl_slist *chunk = NULL;

    serverurl=url;
    
   //init the Url -For Create Ca Request 
    serverurl+= "/api/Cert/Createreq?hostname=" + hostname;
   
    chunk = curl_slist_append(chunk,header.c_str());
    
      /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_HTTPHEADER, chunk);
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 

    /* Create Post Request */ 
    curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS,data);

    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);
    curl_easy_setopt(easyhandle, CURLOPT_TIMEOUT, 60L);

    /*Perform Http Rest Post */
    res=curl_easy_perform(easyhandle);
    if (res==0)
    {
      /*Save the RequestID that was created in the ca of microsot we will use the reqid to get the certficate when it will be issued*/
      int reqid=std::atoi(readBuffer.c_str());   
      return reqid;
    }
    return -1;
}

std::string requestToken(std::string clientid,std::string clientSecret)
{
     curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init();  /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
     std::string data;
    json_error_t error;
    json_t *root;
    const char * token;
    int res;
   
    data = "client_id="+clientid+"&client_secret="+clientSecret+"&grant_type=client_credentials";
  
    serverurl=url;
    serverurl+= "/Token";
   //init the Url -For Create Ca Request 
    //serverurl+= "/Createreq?hostname=" + hostname;
      /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 

    /* Create Post Request */ 
    curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS,data.c_str());

    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);
    curl_easy_setopt(easyhandle, CURLOPT_TIMEOUT, 30L);
    /*Perform Http Rest Post */
    res =curl_easy_perform(easyhandle);
    if (res!=0)
    {
       return "Error Connecting";
    }
  
    root = json_loads(readBuffer.c_str(), 0, &error);
    token = json_string_value(json_object_get(root, "access_token"));
    if (token==NULL) 
    {
      token = json_string_value(json_object_get(root, "error_description"));
    }
 

    return token;
}

int  getCertStatus(int   reqid,std::string hostName,std::string  token)
{
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init(); /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
    std::string  header;
    int status;
    int res;
    struct curl_slist *chunk = NULL;

    header="Authorization: Bearer "+token;
    serverurl=url;
    /*init the Url -For Getting the  Ca */
    serverurl+="/api/Cert/GetStatus?reqid="+std::to_string(reqid)+"&"+"hostname="+hostName;    
    
    chunk = curl_slist_append(chunk,header.c_str());
    curl_easy_setopt(easyhandle, CURLOPT_HTTPHEADER, chunk);
    /* specify URL to get */     
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 
    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);    
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);
    curl_easy_setopt(easyhandle, CURLOPT_TIMEOUT, 60L);
    /*Perform Http Rest Get */
    res=curl_easy_perform(easyhandle);
    if (res==0)
    {
      status=std::atoi(readBuffer.c_str());   
      return status;
    }

        return -1;
    
}


/*Function to Return the Certificate That Was Issued by Microsoft CA */
int  getCertificate(int   reqid,std::string  token)
{
  const char * cert;
  json_error_t error;
  json_t *root;
  curl_global_init(CURL_GLOBAL_ALL);
  int res;
  std::string  header;
  struct curl_slist *chunk = NULL;

    header="Authorization: Bearer "+token;
    CURL* easyhandle = curl_easy_init(); /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
    /*init the Url -For Getting the  Ca */
       serverurl=url;
    serverurl+="/api/Cert/GetCert?reqid="+std::to_string(reqid);    
    chunk = curl_slist_append(chunk,header.c_str());
    curl_easy_setopt(easyhandle, CURLOPT_HTTPHEADER, chunk);
    /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 
    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);    
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);
    curl_easy_setopt(easyhandle, CURLOPT_TIMEOUT, 60L);

    /*Perform Http Rest Get */
    res=curl_easy_perform(easyhandle);

    if (res==0) 
    {
      root = json_loads(readBuffer.c_str(), 0, &error);
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

void hashOuid()
{
  FILE *fp;
  char hashtemp[300];
  char ouidtemp[300];

  /* Open the command for reading. */
  fp = popen("./sha.sh", "r");
  if (fp == NULL) {
    printf("Failed to run command\n" );
    exit(1);
  }

  /* Read the output a line at a time - output it. */
  fgets(hashtemp, sizeof(hashtemp)-1, fp);
  fgets(ouidtemp, sizeof(ouidtemp)-1, fp);
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
  int i=0;

  const char *  client;
  const char * errorMessage; 
  const char* reqidStatus;
  std::string  certPath;
  std::string token;
  struct stat st;
  FILE* f;
  json_error_t error;
  json_t *root, *obj;
  const char *strText;
  hashOuid();

  
  if (argv[2]!=NULL) 
  {
     reqid=std::atoi(argv[2]);
  }
  

    f = fopen ("caw.conf" , "r");

   
    root = json_loadf(f, 0, &error);

    //obj = json_object_get(root, "url");
     url = json_string_value(json_object_get(root, "url"));
     fileloc=json_string_value(json_object_get(root, "file"));
     //client=json_string_value(json_object_get(root, "client"));
     fileloc+=argv[1];
     fileloc+=".cer";


      token=requestToken(hashg,ouidg);
     if (token.find("Error",0)==0) 
     {
           errorMessage=token.c_str();
           goto status;
     }
    

    if (argv[2]==NULL) 
    {
       
        reqid=requestCert(argv[1],token.c_str());
         
        if (reqid==-1)
         {
            errorMessage="There is network Problem";
            goto status;

         }
          else if (reqid==-2)
         {
           
             errorMessage="The Certificate Was Allreday Requested";
             goto status;
         }
            else if (reqid==-3)
         {
            errorMessage="The Certificate Was Allreday Issued and Used";
            reqidStatus="Consumed";
            goto status;
         }
          else if (reqid==0)
         {
            errorMessage="The Certificate Was Not Create Due To a Problem";
            goto status;
         }
    }
 
 

    status=getCertStatus(reqid,argv[1],token.c_str());
     if (status==3) 
    {
         reqidStatus="Issued";
         certPath=fileloc;
        certstatus=getCertificate(reqid,token.c_str());
        if (certstatus==-1)
        {
          errorMessage="There is network Problem";
           goto status;
        }
    
    }
       else if (status==-6)
    {
       errorMessage="The Reqid is not belong to the hostname";
       
        goto status;
    }
    else if (status==-3)
    {
       errorMessage="The Certificate Was Allreday Issued and used ";
       reqidStatus="Consumed";
       certPath=fileloc;      
        goto status;
    }
    else if (status==-1)
    {
        errorMessage="There is network Problem";
       goto status;
    }
      else if (status==2)
    {
        reqidStatus="Denied";
        goto status;
    }  
    else if (status==5)
    {
        reqidStatus="Pending";
        goto status;
    }  
    status:
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
          json_object_set(obj, "CertPath", json_string(certPath.c_str()));
            
          }
          json_object_set(obj, "ErrorMessage", json_string(errorMessage));
          char* result=json_dumps(obj,0);
          cout<<result<<endl;
           json_object_clear(obj);
    return 0;
}