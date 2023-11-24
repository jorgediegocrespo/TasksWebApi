# Introduction 
This project has been created to test different implementations related to the creation of a Rest API with .NET.

Functionally, the project is a basic task manager, in which there are lists and tasks associated to those lists. Moreover, these lists belong to a single user.

Some of the implementations carried out are:
- JWT with Identity
- Refresh token
- Hosted services
- Versioning by url and header
- API Key management with service filter attribute
- EF aoft delete
- EF concurrency control
- Tests with MSTest

# Getting Started
1.	Install dotNet (the project is currently using dotNet 7)
2.	Create a Microsoft SQL Server database
3.	Fill the appsettings with your own configuracion

# Build and Test
To build and run the main project (TasksWebApi) just build it and run with your own configuration.

There is a test project. To run all test in it, ensure that the connection string (DataBaseConnection) in the testappsettings json is a valid one. It is going to be used to create a mock database while test runs.
