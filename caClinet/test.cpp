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

std::string getCmdOutput(const std::string& mStr)
{
    std::string result, file;
    FILE* pipe{popen(mStr.c_str(), "r")};
    char buffer[256];

    while(fgets(buffer, sizeof(buffer), pipe) != NULL)
    {
        file = buffer;
        result += file.substr(0, file.size() - 1);
    }

    pclose(pipe);
    return result;
}


int main(int argc, char * argv[])
{

 std::string root,uniqid,timeset,timesetnumber,tm,hashtmp,hash;  
  
 root=getCmdOutput(R"(df -h /mnt | grep / | awk {'print $1'})");
 uniqid=getCmdOutput(R"(/usr/sbin/dmidecode --string system-uuid  2>/dev/null | egrep -io "^[0-9A-F]{8}-[0-9A-F]{4}-[1-4][0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$" || $(which dmidecode 2>/dev/null) --string system-uuid  2>/dev/null |egrep -io "^[0-9A-F]{8}-[0-9A-F]{4}-[1-4][0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$" || cat /sys/devices/virtual/dmi/id/product_uuid 2>/dev/null)");
 timeset="/sbin/tune2fs -l " + root+R"(| grep "Filesystem created")"+R"(|sed 's/.*created://;s/[ \t]*//')";
 timesetnumber=R"(date -d ")"+getCmdOutput(timeset)+R"(" "+%s")";
 tm=getCmdOutput(timesetnumber);
 hashtmp="echo "+tm+uniqid+R"(| sha256sum |awk '{print $1}')";
 hash =getCmdOutput(hashtmp);
 cout<<hash<<endl;
 return 0;
}