# Introduction 
This project has been created to test different implementations related to the creation of a Rest API with .NET.

Functionally, the project is a basic task manager, in which there are lists and tasks associated to those lists. Moreover, these lists belong to a single user.

Some of the implementations carried out are:
- JWT with Identity
- Refresh token
- Hosted services
- Versioning by url and header
- API Key management with service filter attribute
- EF soft delete
- EF concurrency control
- Tests with MSTest

# Getting Started
1.	Install dotNet (the project is currently using dotNet 8)
2.	Fill the appsettings with your own configuracion
3.  Change DataBaseConnection in appsettings.Testing.json as it is used to generate the mock data base connection strings
4.  Install Docker

# Build and Test
1.  Run the script TasksWebApi/tools/local-development/up.sh
    -   It creates all the needed containers used in the application running the docker-compose.yaml file and executing different script to configure some containers:
        -   MSSQL: Create the SQL Server that host the data base
        -   REDIS: Create the REDIS cache server
        -   VAULT: Create the vault server that contains the app secrets.
            -   NOTE: As vault is running in dev mode, the keys are stored in memory, so they will be remove once the container is stopped.

2.  Once all the containers have been created, you can run the applicacion and a swagger screen will be loaded with all the endpoints

3.  There is a test project. To run all test in it, ensure that the connection string (DataBaseConnection) in the appsettings.Testing.json is a valid one. It is going to be used to create mock databases while test runs.
