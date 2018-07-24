#!/bin/bash

for i in {1..1000} 
do

wget -O/dev/null -q --post-data="{format:json}" http://localhost:5000/list
echo "$i\n"
done
