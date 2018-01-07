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

//copy the value return from the HTTP Get to the memorty
size_t WriteCallback(char *contents, size_t size, size_t nmemb, void *userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}



//Function to Request  Certificate Request to Send To  the Microsoft CA
int requestCert(std::string   hostname)
{
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init();  /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
   
    const char *data = "data to send";
  
    serverurl=url;

   //init the Url -For Create Ca Request 
    serverurl+= "/Createreq?hostname=" + hostname;
      /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 

    /* Create Post Request */ 
    curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS,data);

    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);

    /*Perform Http Rest Post */
    curl_easy_perform(easyhandle);

    /*Save the RequestID that was created in the ca of microsot we will use the reqid to get the certficate when it will be issued*/
    int reqid=std::atoi(readBuffer.c_str());   
    return reqid;
}

int requestToken(int clientid)
{
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init();  /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
    json_error_t error;
    json_t *root;
    const char * token;
   
    const char *data = "client_id=1234&grant_type=client_credentials";
  
    //serverurl=url;

   //init the Url -For Create Ca Request 
    //serverurl+= "/Createreq?hostname=" + hostname;
      /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_URL,"http:/52.90.34.6:50026/Token"); 

    /* Create Post Request */ 
    curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS,data);

    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);

    /*Perform Http Rest Post */
    curl_easy_perform(easyhandle);
    root = json_loads(readBuffer.c_str(), 0, &error);
    token = json_string_value(json_object_get(root, "access_token"));
    if (token==NULL) 
    {
      token = json_string_value(json_object_get(root, "error_description"));
    }
    cout<<token<<endl;
    /*Save the RequestID that was created in the ca of microsot we will use the reqid to get the certficate when it will be issued*/
  //  int reqid=std::atoi(readBuffer.c_str());   
    return 1;
}

int  getCertStatus(int   reqid)
{
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init(); /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
    int status;

    serverurl=url;
    /*init the Url -For Getting the  Ca */
    serverurl+="/GetStatus?reqid="+std::to_string(reqid);    
    
    /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 
    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);    
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);

    /*Perform Http Rest Get */
    curl_easy_perform(easyhandle);
   // cout<<readBuffer.c_str()<<endl;
    status=std::atoi(readBuffer.c_str());   
    return status;
    
}


/*Function to Return the Certificate That Was Issued by Microsoft CA */
void getCertificate(int   reqid)
{
   const char * cert;
    json_error_t error;
  json_t *root;
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init(); /* init the curl session */ 
    std::string readBuffer;
    std::string serverurl;
    /*init the Url -For Getting the  Ca */
       serverurl=url;
    serverurl+="/GetCert?reqid="+std::to_string(reqid);    
    
    /* specify URL to get */ 
    curl_easy_setopt(easyhandle, CURLOPT_URL,serverurl.c_str()); 
    /* send all data to this function  to save the Return Value */  
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);    
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);

    /*Perform Http Rest Get */
    curl_easy_perform(easyhandle);

    root = json_loads(readBuffer.c_str(), 0, &error);
    cert = json_string_value(json_object_get(root, "CertValue"));
    /*open A file to Write -- cert.cert*/

    std::ofstream out(fileloc);

    /*Write The Certificate To the File*/
    out << cert;
    /*Close the File */
    out.close();
    
}




int main(int argc, char * argv[])
{
 
  int reqid;
  int  status;
  const char * urltmp;
  const char *  fileloctmp;

  struct stat st;
  FILE* f;
    json_error_t error;
    json_t *root, *obj;
 const char *strText;
     


  if (argv[2]!=NULL) 
  {
     reqid=std::atoi(argv[2]);
  }
    requestToken(1234);
    f = fopen ("caw.conf" , "r");
  //ifstream in("caw.conf");
 // if(!in) {
  //  cout << "Cannot open input file.\n";
  //  return 1;
//  }
  
 //   in.getline(urltmp, 100); 
  //  in.getline(fileloctmp, 100); 
 //   in.close();

    root = json_loadf(f, 0, &error);
    //obj = json_object_get(root, "url");
     urltmp = json_string_value(json_object_get(root, "url"));
     fileloctmp=json_string_value(json_object_get(root, "file"));
     url=urltmp;
     fileloc=fileloctmp;
    fileloc+=argv[1];
    fileloc+=".cer";
    url=url.substr(url.find("-") + 1);
    fileloc=fileloc.substr(fileloc.find("-") + 1) ;
      if(stat(fileloc.c_str(),&st) == 0)
      {
        cout<<"The Certificate Is allready Issued-"<<fileloc.c_str()<<endl;
        return 0;
      }


    if (argv[2]==NULL) 
    {
       
        reqid=requestCert(argv[1]);
    }
      

    status=getCertStatus(reqid);
    if (status==3) 
    {
        getCertificate(reqid);
    }

    else
    {
        cout<<"The Certificate ReqId-"<<reqid<<" Is Yet Not Issued"<<endl;
    }  

    return 0;
}