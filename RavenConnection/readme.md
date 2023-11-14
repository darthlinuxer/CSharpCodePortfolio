# RavenDb Template


## Linux

Requirements: WSL with Ubuntu installed and Docker running

```
> bash ./Scripts_Linux_Cli/start.sh
```

When the menu appears, create a RavenDb container (option 1)  
Answer questions:  
a) Is container running in the cloud ? (no)  
b) Choose the node hostname : Raven_A   
**(Raven_A is the default name of the host as defined in the appsettings.json)**  
c) choose the port : default is 8080  
d) choose the TCP port: default is 38888

After that, IF docker service is running then a Raven container will be running in the WSL  

-> EXIT THE MENU with option 0 and then execute:
```
> dotnet run
```

a) Open Swagger: http://localhost:5000/swagger   
b) POPULATE the DATABASE 
c) Open Raven Server: http://localhost:8080 and check if the UserDatabase is created


Enjoy!