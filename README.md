# FashionApi 🚀

**FashionApi** is the high-performance backend engine powering the **HoaiThu.Vn** e-commerce ecosystem. Built with **.NET 8** and **Entity Framework Core**, it delivers a robust, secure, and scalable API architecture handling everything from user authentication to complex order processing.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120.svg?style=flat-square&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4.svg?style=flat-square&logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927.svg?style=flat-square&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)

---

## 🏗 Technology Architecture

### Core Stack

- **Framework**: [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (LTS)
- **Language**: [C# 12](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12)
- **Architecture**: Clean Architecture with Repository Pattern

### Data Ecosystem

- **ORM**: [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/) (Code-First Approach)
- **Database**: SQL Server (Production) / SQLite (Dev Support)
- **Caching**: `IMemoryCache` (In-Memory Caching)
- **Storage**: Local File System (Static Assets)

### Security & Compliance

- **Authentication**: JWT (JSON Web Tokens) Bearer Auth
- **Password Security**: PBKDF2 / BCrypt Hashing standards
- **API Security**: CORS policies, Rate Limiting support
- **Validation**: FluentValidation & Data Annotations

---

## ⚙️ System Requirements

To ensure a smooth development environment, the following prerequisites are **mandatory**:

| Requirement  | Version            | Note                       |
| :----------- | :----------------- | :------------------------- |
| **.NET SDK** | `8.0` (LTS)        | Core runtime & build tools |
| **Database** | SQL Server 2019+   | Or LocalDB for development |
| **IDE**      | Visual Studio 2022 | Or VS Code with C# Dev Kit |
| **Postman**  | v10+               | For external API testing   |

---

## 🚀 Getting Started

### 1. Environmental Setup

Clone the repository and restore dependencies:

```bash
# Clone repository
git clone <repository_url>
cd FashionApi

# Restore NuGet packages
dotnet restore
```

### 2. Configuration

Update `appsettings.json` with your database connection string:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=FashionDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Database Migration

Apply the Code-First migrations to initialize your database schema:

```bash
# Update local database
dotnet ef database update
```

### 4. Execution

Launch the API server:

```bash
# Build and Run
dotnet run
```

Access the **Swagger UI** Documentation at:
👉 `https://localhost:7088/swagger/index.html` (Port may vary based on `launchSettings.json`)

---

## ☁️ Azure Deployment Guide

Follow these steps to deploy **FashionApi** as an Azure Web App.

### 1. Prepare Release Artifact

Run the following command to compile and publish the application:

```bash
# Publish to ./publish directory
dotnet publish -c Release -o ./publish
```

### 2. Configure `web.config`

Azure IIS requires a specific `web.config` to handle the .NET 8 hosting execution. Ensure the following file exists in your `./publish` output folder. If not, create it manually:

**File:** `./publish/web.config`

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments="FashionApi.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess" />
    </system.webServer>
  </location>
</configuration>
<!--ProjectGuid: D150987F-80F5-40F9-B5E5-AA6FF10C09DF-->
```

> [!IMPORTANT]
> The `web.config` is critical for Azure IIS to correctly bootstrap `FashionApi.dll` via the `dotnet` process.

### 3. Upload to Azure (Kudu)

1.  Navigate to your **Azure Web App** resource in the Azure Portal.
2.  Go to **Development Tools** > **Advanced Tools (Kudu)** > **Go**.
3.  In the Kudu console top menu, select **Debug Console** > **CMD**.
4.  Navigate to: `site` > `wwwroot`.
5.  **Zip** the contents of your local `./publish` folder (select all files inside -> Compress/Zip).
6.  Drag and drop the `.zip` file into the Kudu file manager area to upload and unzip.

---

## 📂 Engineering Structure

We adhere to a strict modular architecture to ensure separation of concerns.

- **`Controllers/`**: RESTful API endpoints organized by domain (Users, Products, Orders).
- **`Services/`**: Business logic layer implementing core interfaces.
- **`Repository/`**: Data access abstraction layer.
- **`Models/`**: Entity definitions and DTOs (Data Transfer Objects).
  - `Create/`: POST request payloads.
  - `Edit/`: PUT/PATCH request payloads.
  - `View/`: Response DTOs.
- **`Data/`**: DbContext configuration and Data Seeding logic.

---

## 🔑 Key Features

- **User System**: Full Auth flow (Register, Login, Forgot Password), Profile Management, Avatar Upload.
- **Product Catalog**: Categories, Products, Images, Inventory Tracking.
- **Order Processing**: Cart management, Checkout flow, Order history, Status tracking.
- **Admin Dashboard APIs**: Comprehensive CMS endpoints for resource management.
- **Search & Filter**: Advanced LINQ-based querying for products.

---

## 🤝 Contribution Guidelines

1.  **Branching Strategy**: Use `feature/` or `fix/` prefixes (e.g., `feature/payment-integration`).
2.  **Code Style**: Follow standard C# Coding Conventions.
3.  **Migrations**: Always create a new migration for model changes (`dotnet ef migrations add <Name>`).

---

## 📝 Licensing

This project is proprietary. All rights reserved.
