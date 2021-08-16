# Serilog Microservice

The fuzzy-engine is named from github, thanks for name by github :)


fuzzy-engine is a ASP.NET CORE logging micro service by Serilog. It use with Elasticsearch & Kibana and running on the Docker container.


<b>FOR WINDOWS USERS</b>

Firstly, you must install docker desktop. Then you must clone the repository and you will run the docker-compose.yml on the cmd or powershell at your project directory.

<b>In that;</b>

```
docker-compose up -d
```

> **NOTE**: The fuzzy-engine is a minimal API by ASP.NET CORE 5. It is not contains any controllers folder. It is running on the middleware.

> **NOTE**:If you want to see the serilog logs from kibana, you must set up the kibana index pattern with star("*")


Then, when you do starting docker, you can start debugging the fuzzy-engine. when you did the request, Kibana will show the logs

If you like this, give me a star :)
