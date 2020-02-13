# Kakadu 
is a simple project to emulate the behavior of REST and SOAP APIs, generally called [service virtualization](https://en.wikipedia.org/wiki/Service_virtualization). Typical usage scenarios include integration tests, where one would want predictable answers from 3rd party API providers. 

Once set up, Kakadu will act as a proxy between your app-under-development and API providers to capture all your REST and SOAP requests/responses and store them in a local database. Once recorded, one could rely solely on responses provided by Kakadu - they will be the same.

### Features
 - support REST calls
 - support SOAP calls
 - capturing requests/respones to local db
 - pass-through mode allowing pass the call to original APIs if route is not matched with local db
 - global variables (TBD)
 - session variables (TBD)
 - extract variables from responses (TBD)
 - dockerfiles for easy deployment (TBD)
 - configuration web app (TBD)

### discaimer
This is a work in progress, so not everything works OOB.

### setup
once cloned, one would need to provide the following environment variables
 - ASPNETCORE_ENVIRONMENT
 - JwtSettings__Secret
 - REACT_APP_API_URL

vscode's launch.json is set up to read those values from following files
 - envvariables
 - reactenvvariables