# tapeworm docker

## requirements 
- the tapeworm docker requires 2 volume mounts
- /app/data              # where the data lives
- /app/config            # where the 'yaml' config files for your data live. Sub dirs=ok

## to build and run the docker
```
./run
```

#to view logs
- logs are only sunted to stdio, nothing is saved to disk
```
./logs
```


