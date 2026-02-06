# Ladral ToDo Application

## Introduction

Welcome to the Ladral ToDo Application – a modern, full-stack task management solution built with ASP.NET Core and Blazor, implementing the Backend for Frontend (BFF) architectural pattern.
## Architecture Overview

The Ladral ToDo application follows the Backend for Frontend (BFF) pattern, providing a tailored backend service specifically designed to serve the needs of the frontend client. This architecture ensures optimal performance and security.

The application consists of three main parts:

- **The Client (Ladral.ToDo.WebApp.Client)** – A responsive Blazor application that communicates exclusively with the BFF layer, ensuring a seamless and optimized user experience for task management
- **The BFF Layer (Ladral.Todo.WebApp)** – A specialized ASP.NET Core Web API that acts as an intermediary between the frontend and various backend services, aggregating data, handling authentication, and providing endpoints tailored specifically for the Blazor client's needs
- **The API (Ladral.Todo.Api)** – A robust ASP.NET Core Web API that handles all backend operations, data processing, and business logic

### Benefits of the BFF Pattern

- **Enhanced Security** – Centralized authentication and authorization handling with reduced attack surface

## Features

### Completed
- Secure authentication handled by the BFF

### Planed
- Create, update, and delete tasks
- Organize tasks with categories and priorities
- Real-time updates through the BFF layer
- Responsive design that works on desktop and mobile devices


## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker installed and running on your host machine.
- An OIDC-compliant Identity Provider (Logto is recommended for local development).


### Certificates
To get started with development, ensure you have trusted HTTPS certificates:

```bash
dotnet dev-certs https --trust
```

### Rider Project Setup
Select the Ladral.Todo.WebApp.Bff and Ladral.ToDo.Api projects and then right-click and select "Run Multiple Projects" to run BFF with the hosted client application together with the API backend.


### Logto setup
#### Start Logto Identity Provider (IdP)
1. Ensure Docker is installed and running.
2. Open your terminal and navigate to the /mock/logto directory.
3. Start Logto IdP: `docker compose up -d`
4. Access the Logto admin console at http://localhost:3002/console.
5. Create an admin account if this is your first time.

#### Configure Logto for the Todo Application
1. In the Logto admin console, go to the "Applications" sidebar menu.
2. Create a new application:
    - Select .NET Core (Blazor Server) as the application type.
      - Note: While the application uses Blazor interactive auto rendering, the Blazor Server template is used for OIDC compatibility (per the BFF pattern).
    - Application name: LadralTodoBFF
3. Set the redirect URIs:
    - Redirect URI: https://localhost:5001/signin-oidc
    - Post sign-out redirect URI: https://localhost:5001/signout-oidc
4. Save the application. Copy the Client ID and Client Secret from the Logto admin console.

#### Configure the WebApp application
1. In the Ladral.ToDo.WebApp.Bff project, open appsettings.Development.json.
2. Update the Authentication settings (Authority, ClientId, ClientSecret) using the values from the Logto application you just created:

        ClientId: Logto App ID (Client ID)
        ClientSecret: Logto App secret (client secret)
        Authority: Logto Issuer endpoint

#### Configure Logto for the Todo API
1. In the Logto admin console, go to the "API resource" sidebar menu.
2. Create a new API resource:
    - API name: LadralTodoAPI
    - API Identifier: https://todo-api.local.ch
3. Create permissions for the API:
    - Permission name: read:todo
    - Permission name: write:todo

#### Configure the API application
1. In the Ladral.ToDo.API project, open appsettings.Development.json.
2. Update the Authentication settings (Authority, Audience) using the values from the Logto application you just created:

        Authority: Logto Issuer endpoint of Application LadralTodoBFF
        Audience: https://todo-api.local.ch


