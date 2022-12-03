# OhMyWord

OhMyWord is a game in which players compete with other players around the world
to guess a randomly selected word.

## Application Components

The application is made up of 2 major components.

* Frontend - This is an Angular application that forms the main user interface for the game. It communicates using HTTP requests and SignalR to the backend.
* API Backend - This is a ASP.Net Core application that responds to user events and maintains a connection to the database.
* Database - The applications makes use of Azure Cosmos DB as its permanent data store.
