#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <curl/curl.h>


struct MemoryStruct {
  char *memory;
  size_t size;
};



 
static size_t
WriteMemoryCallback(void *contents, size_t size, size_t nmemb, void *userp)
{
  size_t realsize = size * nmemb;
  struct MemoryStruct *mem = (struct MemoryStruct *)userp;
 
  mem->memory = realloc(mem->memory, mem->size + realsize + 1);
  if(mem->memory == NULL) {
    /* out of memory! */ 
    printf("not enough memory (realloc returned NULL)\n");
    return 0;
  }
 
  memcpy(&(mem->memory[mem->size]), contents, realsize);
  mem->size += realsize;
  mem->memory[mem->size] = 0;
 
  return realsize;
}

char* concat(const char *s1, const char *s2)
{
    char *result = malloc(strlen(s1)+strlen(s2)+1);//+1 for the null-terminator
    //in real code you would check for errors in malloc here
    strcpy(result, s1);
    strcat(result, s2);
    return result;
}


 
char * GetCert(int reqid)
{
    CURL *curl_handle;
    CURLcode res;
    char * cert;
    char * url;
    char *str;
    struct MemoryStruct chunk;
    chunk.memory = malloc(1);  /* will be grown as needed by the realloc above */ 
    chunk.size = 0;    /* no data at this point */ 
    char * streqid;    
    curl_global_init(CURL_GLOBAL_ALL);
 
  /* init the curl session */ 
    curl_handle = curl_easy_init();
    sprintf(str, "%d", reqid);
    url=concat("http://34.201.109.145:50026/api/Cert/GetCert?reqid=",str);    
    /* specify URL to get */ 
    curl_easy_setopt(curl_handle, CURLOPT_URL,url);
 
  /* send all data to this function  */ 
    curl_easy_setopt(curl_handle, CURLOPT_WRITEFUNCTION, WriteMemoryCallback);
 
  /* we pass our 'chunk' struct to the callback function */ 
    curl_easy_setopt(curl_handle, CURLOPT_WRITEDATA, (void *)&chunk);
 
  /* some servers don't like requests that are made without a user-agent
     field, so we provide one */ 
    curl_easy_setopt(curl_handle, CURLOPT_USERAGENT, "libcurl-agent/1.0");
 
  /* get it! */ 
    res = curl_easy_perform(curl_handle);
 
  /* check for errors */ 
    if(res != CURLE_OK) 
    {
        fprintf(stderr, "curl_easy_perform() failed: %s\n",
        curl_easy_strerror(res));
    }
    else
    {
        cert= chunk.memory;
    }
 
  /* cleanup curl stuff */ 
    curl_easy_cleanup(curl_handle);
    free(chunk.memory);
 
  /* we're done with libcurl, so clean it up */ 
    curl_global_cleanup();
 `
    return cert;
}

int  requestCert(char* hostname)
{
  CURL *curl_handle;
  CURLcode res;
  char * cert;
  struct MemoryStruct chunk;
  int  reqid;
  char * url;
  const char *data = "data to send";
  chunk.memory = malloc(1);  /* will be grown as needed by the realloc above */ 
  chunk.size = 0;    /* no data at this point */ 

  curl_global_init(CURL_GLOBAL_ALL);

/* init the curl session */ 
  curl_handle = curl_easy_init();


/* init the server url*/
  url= concat("http://34.201.109.145:50026/api/Cert/Createreq?hostname=",hostname);

 

/* specify URL to get */ 
  curl_easy_setopt(curl_handle, CURLOPT_URL,url);

  curl_easy_setopt(curl_handle, CURLOPT_POSTFIELDS, data);

/* send all data to this function  */ 
  curl_easy_setopt(curl_handle, CURLOPT_WRITEFUNCTION, WriteMemoryCallback);

/* we pass our 'chunk' struct to the callback function */ 
  curl_easy_setopt(curl_handle, CURLOPT_WRITEDATA, (void *)&chunk);

/* some servers don't like requests that are made without a user-agent
   field, so we provide one */ 
  curl_easy_setopt(curl_handle, CURLOPT_USERAGENT, "libcurl-agent/1.0");

/* get it! */ 
  res = curl_easy_perform(curl_handle);

/* check for errors */ 
  if(res != CURLE_OK) 
  {
      fprintf(stderr, "curl_easy_perform() failed: %s\n",
      curl_easy_strerror(res));
  }
  else
  {
    cert=chunk.memory;     
    reqid=atoi(cert);
  }

/* cleanup curl stuff */ 
  curl_easy_cleanup(curl_handle);
  free(chunk.memory);
  /* we're done with libcurl, so clean it up */ 
  curl_global_cleanup();
  return reqid;
}
 

int main(void)
{
    int  reqid;
    int i;
    int j;
    char * cert;
    char * certfix;
     FILE *fptr;
     FILE *fp;

 
    reqid=requestCert("ironscriptFix");
    cert=GetCert(reqid);
    fptr = fopen("cert.cer", "w");
    if(fptr == NULL)
     {
      printf("Error!");
      return 1;
      }
    fprintf(fptr,"%s\r\n",cert);
    fclose(fptr);

    return 0;
}