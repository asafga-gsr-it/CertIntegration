#include <fstream>
#include <iostream>
#include <string>
#include <curl/curl.h>
#include <sys/stat.h>
#include <cstring>
#include "/usr/include/jansson.h"
#include <stdio.h>

using namespace std;

int main() {
    FILE* f;
    json_error_t error;
    json_t *root, *obj;
    int i;
    const char *json_text  = "{ \"foo\": \"bar\", \"foo2\": \"bar2\", \"foo3\": \"bar3\" }";
      const char *strText;
       f = fopen ("conf.json" , "r");

  
     root = json_loadf(f, 0, &error);
    //root =json_loadf(in, 0, &error);
    obj = json_object_get(root, "color");
     strText = json_string_value(obj);
       printf(strText);
    //for(int i = 0; i< json_array_size(obj); i++){
      // const char *strText;
      //  json_t *obj_txt;

      //  obj_txt = json_array_get(obj, i);
      //  if( NULL==obj_txt || !json_is_string(obj_txt) ) continue;
      //  strText = json_string_value(obj_txt);
      //  printf(strText);
   // }
    return 0;
}

