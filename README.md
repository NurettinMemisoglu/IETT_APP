# IETT Operation - Corporate Logistics Management System

![.NET](https://img.shields.io/badge/.NET%209-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Clean Architecture](https://img.shields.io/badge/Clean%20Architecture-blue?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-RealTime-lightgrey?style=for-the-badge)

## 📖 Overview

**IETT Operation** is a web-based Corporate Logistics Management System designed to digitize operational processes. The project unifies three critical roles—**Planner, Chief, and Driver**—under a single scalable platform using **Clean Architecture**.

The system ensures real-time coordination between field personnel and the command center, featuring live vehicle tracking, dynamic route management, and secure role-based access.

## 🔄 System Workflow (How it Works)

The system operates on a hierarchical flow:

1.  **Planner:** Creates a new transportation route and assigns it to a vehicle pool.
2.  **Chief:** Reviews the planned route. If approved, assigns a specific **Driver** and **Vehicle**.
3.  **Driver:** Receives a real-time notification (SignalR) on their dashboard.
4.  **Tracking:** Once the journey starts, the admin panel tracks the vehicle's location live via Google Maps.

## 🏗 Architecture & Project Structure

The solution follows the **Onion / Clean Architecture** principles, ensuring separation of concerns and maintainability. Below is the breakdown of layers and their key dependencies.

### 1. Core Layer (Domain)
* **Namespace:** `IETT_APP.Domain`
* **Focus:** Contains enterprise logic, entities, and interfaces. It has no external dependencies on other layers.
* **Key Packages:**
  * `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
  * `Microsoft.EntityFrameworkCore`

### 2. Application Layer
* **Namespace:** `IETT_APP.Application`
* **Focus:** Contains business logic, CQRS handlers, DTOs, and validation rules.
* **Key Packages:**
  * `MediatR` (CQRS Pattern)
  * `FluentValidation` (Validation Pipeline)
  * `AutoMapper` (Object Mapping)
  * `Microsoft.AspNetCore.Http.Features`

### 3. Infrastructure Layer
* **Namespace:** `IETT_APP.Infrastructure`
* **Focus:** Implementation of external services (Email, Database, Background Jobs).
* **Key Packages:**
  * `Hangfire.SqlServer` (Background Jobs)
  * `MailKit` (Email Service)
  * `Microsoft.EntityFrameworkCore.SqlServer`
  * `Microsoft.Extensions.Configuration`

### 4. Presentation Layers (Web API & MVC)

#### 🌐 Web API (`IETT_APP.WebAPI`)
Serves as the backend service, providing endpoints for the client applications.
* **Key Packages:**
  * `Scalar.AspNetCore` (Next-gen API Documentation)
  * `Microsoft.AspNetCore.Authentication.JwtBearer` (JWT Auth)
  * `Hangfire.AspNetCore`
  * `Microsoft.AspNetCore.OpenApi`

#### 🖥 Web MVC (`IETT_APP.WebMVC`)
The user-facing web application with dashboards for Admins, Chiefs, and Drivers.
* **Key Packages:**
  * `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
  * `Microsoft.Identity.Web` & `Microsoft.Identity.Web.UI`
  * `Microsoft.VisualStudio.Web.CodeGeneration.Design`

## 🚀 Key Features

* **Role-Based Management:** Distinct Areas (`Admin`, `Chief`, `Driver`, `Planner`) managed via Identity.
* **CQRS & Mediator Pattern:** Decoupled read/write operations for high performance.
* **Real-Time Tracking:** Live monitoring using **SignalR**.
* **Background Jobs:** Recurring tasks and reporting handled by **Hangfire**.
* **Modern API Docs:** Interactive API testing via **Scalar**.
* **Map Integration:** Google Maps API integration for route visualization.

## ⚙️ Getting Started

Follow these instructions to get a local copy of the project up and running.

### Prerequisites
* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
* Visual Studio 2022

### Installation

1.  **Clone the repository**
    ```bash
    git clone [https://github.com/NurettinMemisoglu/IETT_APP.git](https://github.com/NurettinMemisoglu/IETT_APP.git)
    cd IETT_APP
    ```

2.  **Database Configuration**
    Update the connection strings in `appsettings.json` for both **WebAPI** and **WebMVC** projects.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER;Database=IETT_OperationDB;Trusted_Connection=True;TrustServerCertificate=True"
    }
    ```

3.  **User Secrets (Optional but Recommended)**
    The project uses User Secrets for security. You may need to initialize them if explicit IDs are required:
    ```bash
    dotnet user-secrets init --project src/IETT_APP.WebAPI
    dotnet user-secrets init --project src/IETT_APP.WebMVC
    ```

4.  **Run Migrations**
    Apply the database schema via the API or Infrastructure layer:
    ```bash
    dotnet ef database update --project src/IETT_APP.Infrastructure --startup-project src/IETT_APP.WebAPI
    ```

5.  **Run the Application**
    You can run multiple startup projects (API + MVC) or run them individually.
    ```bash
    dotnet run --project src/IETT_APP.WebAPI
    dotnet run --project src/IETT_APP.WebMVC
    ```

6.  **Access the Docs**
    * API Documentation (Scalar): `https://localhost:5001/scalar/v1` (Check your specific port)
    * Hangfire Dashboard: `https://localhost:5001/hangfire`

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
