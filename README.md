# tapeworm

## Purpose
tapeworm serves flat files as structured content through an UI or webAPI

## Visualization
![tapeworm demo](https://raw.githubusercontent.com/chris17453/tapeworm/master/media/tapeworm-demo.gif)

## Features
- Column ordering through ordinals
- sorting per colum
- filtering per column
- combined search across multi search enabled columns
- hiding whitespace
- hiding comments '#'
- starting data read at file line x
- strongly typing data per column
- output to json yaml or raw
- docker for simple deployment
- built on dotnetcore, buildable directly on mac,pc or linux

## Limitations
- records fail if strongly typed conversion is invalid
- CRUD not full exposed at the moment
- high cpu use for read, parallellism TODO





