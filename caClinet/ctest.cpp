#include <fstream>
#include <iostream>
#include <string>
#include <curl/curl.h>
using namespace std;

size_t WriteCallback(char *contents, size_t size, size_t nmemb, void *userp)
{
    ((std::string*)userp)->append((char*)contents, size * nmemb);
    return size * nmemb;
}


int     requestCert(std::string   hostname)
{
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init();
    std::string readBuffer;
    std::string url;
    std::string cert;
    
    const char *data = "data to send";
    int i;

    url="http://52.207.133.36:50026/api/Cert/Createreq?hostname=" + hostname;

    curl_easy_setopt(easyhandle, CURLOPT_URL,url.c_str()); 
    curl_easy_setopt(easyhandle, CURLOPT_POSTFIELDS,data); 
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);
    curl_easy_perform(easyhandle);
    //printf("%d\n", readBuffer[1]);
   // std::ofstream out("cert.txt");
    //out << readBuffer.c_str();
   // out.close();     
  
    int x=std::atoi(readBuffer.c_str());   
    return x;
}


std::string  getCertificate(int   reqID)
{
 curl_global_init(CURL_GLOBAL_ALL);

    CURL* easyhandle = curl_easy_init();
    std::string readBuffer;
    std::string url;
    std::string cert;
    int i;

    url="http://52.207.133.36:50026/api/Cert/GetCert?reqid="+std::to_string(reqID);

    

    curl_easy_setopt(easyhandle, CURLOPT_URL,url.c_str()); 
    curl_easy_setopt(easyhandle, CURLOPT_WRITEFUNCTION, WriteCallback);
    curl_easy_setopt(easyhandle, CURLOPT_WRITEDATA, &readBuffer);
    curl_easy_perform(easyhandle);
    std::ofstream out("cert.cer");
    out << readBuffer.c_str();
    out.close();

    return readBuffer.c_str();
}

int main(int argc, char * argv[])
{
    int reqid;

    reqid=requestCert("cpptst");
    getCertificate(reqid);
     return 0;
}