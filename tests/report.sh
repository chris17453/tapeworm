#!/bin/bash

for i in {1..1000} 
do

wget -O/dev/null -q --post-data="{"uid":"et_machine","format":"json","paginate":true,"page":0,"page_length":10,"reload":false,"hide_blanks":false,"hide_errors":false,"hide_comments":false,"multi_search":"","filter":[],"sort":[]}
" http://localhost:5000/fetch
echo "$i\n"
done
