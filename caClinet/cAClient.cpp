#include <fstream>
#include <iostream>
#include <string>
#include <curl/curl.h>
using namespace std;


   char url[100];
   char fileloc[100];
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

int  getRuestStatus(int   reqid)
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
  
    /*open A file to Write -- cert.cert*/
    std::ofstream out(fileloc);

    /*Write The Certificate To the File*/
    out << readBuffer.c_str();
    /*Close the File */
    out.close();
    
}




int main(int argc, char * argv[])
{
  int reqid;
  int  status;
 
  

  ifstream in("conf.txt");
  if(!in) {
    cout << "Cannot open input file.\n";
    return 1;
  }
  
    in.getline(url, 100); 
    in.getline(fileloc, 100); 
    in.close();
    reqid=requestCert(argv[1]);
    status=getRuestStatus(reqid);
  
    if (status==3) 
    {
        getCertificate(reqid);
    }

    else
    {
        cout<<"The Certificate Is Yet Not Issued"<<endl;
    }

    return 0;
}