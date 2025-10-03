# Ladral ToDo Application

## Introduction

Welcome to the Ladral ToDo Application - a modern, full-stack task management solution built with ASP.NET Core and Blazor, implementing the Backend for Frontend (BFF) architectural pattern.
## Architecture Overview

The Ladral ToDo application follows the Backend for Frontend (BFF) pattern, providing a tailored backend service specifically designed to serve the needs of the frontend client. This architecture ensures optimal performance and security.

The application consists of three main components:

- **The Client (Ladral.ToDo.WebApp.Client)** - A responsive Blazor application that communicates exclusively with the BFF layer, ensuring a seamless and optimized user experience for task management
- **The BFF Layer (Ladral.Todo.WebApp)** - A specialized ASP.NET Core Web API that acts as an intermediary between the frontend and various backend services, aggregating data, handling authentication, and providing endpoints tailored specifically for the Blazor client's needs
- **The API (Ladral.Todo.Api)** - A robust ASP.NET Core Web API that handles all backend operations, data processing, and business logic

## Benefits of the BFF Pattern

- **Enhanced Security** - Centralized authentication and authorization handling with reduced attack surface

## Features

### Completed
- Secure authentication handled by the BFF

### Planed
- Create, update, and delete tasks
- Organize tasks with categories and priorities
- Real-time updates through the BFF layer
- Responsive design that works on desktop and mobile devices


## Getting Started


### Certificates
To get started with development, ensure you have trusted HTTPS certificates:

```bash
dotnet dev-certs https --trust
```

### Rider Project Setup
Select the Ladral.Todo.WebApp and Ladral.ToDo.Api projects and then right-click and select "Run Multiple Projects" to run BFF with hosted client application together with the API backend.

