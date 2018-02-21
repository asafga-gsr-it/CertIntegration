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
int unlockflag(std::string  token,std::string hostname,std::string clientid)
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
    serverurl+= "/api/Cert/unlockCert?hostname="+hostname+"&"+"clientid="+clientid;

    

    
   
    chunk = curl_slist_append(chunk,header.c_str());
    
      /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_HTTPHEADER, chunk);
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 
    curl_easy_setopt(easyhandle, CURLOPT_SSL_VERIFYPEER, 0L);
    /* Create Post Request */ 
    curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS,data);

    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);
    curl_easy_setopt(easyhandle, CURLOPT_TIMEOUT, 360L);

    /*Perform Http Rest Post */
    res=curl_easy_perform(easyhandle);
    return  stoi(readBuffer.c_str());   
   
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
    curl_easy_setopt(easyhandle, CURLOPT_SSL_VERIFYPEER, 0L);
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

  
  f = fopen ("caw.conf" , "r");

    root = json_loadf(f, 0, &error);

    //obj = json_object_get(root, "url");
     url = json_string_value(json_object_get(root, "url"));
    

    
      token=requestToken(hashg,ouidg);
     if (token.find("Error",0)==0) 
     {
           errorMessage=token.c_str();
           
     }
    cout<<unlockflag(token,argv[1],ouidg)<<endl;

    
    return 0;
}