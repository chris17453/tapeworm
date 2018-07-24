# tapeworm

## Purpose
tapeworm serves flat files as structured content through an UI or webAPI

## Visualization
![tapeworm demo](https://raw.githubusercontent.com/chris17453/tapeworm/master/media/tapeworm-demo.gif)

## Features
### Columns 
- column ordering through ordinals
- column naming       
- column Display Name 
- strongly typed data per column
- enable/disable sorting per colum 
- enable/disable filtering per column
- combined search across multi search enabled columns
### Rows 
- hiding whitespace
- hiding comments '#'
- starting data read at file line x
### UI
- hidden overflow for columns
- fixed width columns
- expandable columns
- browsable data
- paginated data
### WebAPI
- discoverable data streams
- paginatable data streams
### Output 
- output to json yaml or raw
### Deployment
- docker for simple deployment
- built on dotnetcore, buildable directly on mac,pc or linux


## Limitations
- records fail if strongly typed conversion is invalid
- CRUD not full exposed at the moment
- high cpu use for read, parallellism TODO





