# Moon Books - Library Management System

Moon Books is a modern, responsive Web Application built using ASP.NET MVC (.NET Framework 4.8) designed to streamline library operations. The system features separate portals for Librarians and Members, automated fine calculations, activity logging, and an integrated secure external payment gateway simulator.

## Key Features

- **Dual-Portal Access**: Dedicated dashboards and layouts for Librarians and Members.
- **Book & Category Management**: Full CRUD operations for library inventory and classifications.
- **Automated Fine System**: Automatic calculation of late return fines (Rp 2,000 per day) upon return.
- **Secure Payment Simulation**: A clean, tab-switching external payment gateway (Moon Pay) with real-time status synchronization and interactive success visual effects (confetti).
- **Activity Log System**: Tracks and records all critical actions performed by users for administrative auditing.
- **Email Notifications**: Automated email notifications sent to members when borrowing requests are submitted, approved, or rejected.

---

## Technologies Used

- **Framework**: ASP.NET MVC ( .NET Framework 4.8 )
- **Database**: Microsoft SQL Server & Entity Framework (Database First / DbContext)
- **Frontend**: Bootstrap 5, jQuery, Google Fonts (Plus Jakarta Sans, Playfair Display)
- **Authentication**: Session-Based Authentication & Role-Based Authorization
- **Utilities**: SMTP Email Sender (EmailHelper), CSS Confetti particle system

---

## Prerequisites

Before running the project, ensure you have the following installed:

1. **Visual Studio 2022** (with *ASP.NET and web development* workload enabled)
2. **.NET Framework 4.8 SDK & Runtime**
3. **Microsoft SQL Server** (LocalDB, Express, or Developer edition)
4. **SQL Server Management Studio (SSMS)**

---

## Setup & Installation Instructions

### 1. Database Setup
1. Open **SQL Server Management Studio (SSMS)** and connect to your SQL Server instance.
2. Create a new database named `LibraryManagementSystemDb`.
3. Open the provided database export file (e.g., `database.sql`).
4. Execute the SQL script in your query window to generate all tables, relationships, and populate seed data.

### 2. Configure the Connection String
1. Open the project folder and locate the `Web.config` file in the root directory.
2. Find the `<connectionStrings>` section.
3. Update the `connectionString` attribute to match your local SQL Server instance:
   ```xml
   <add name="LibraryDbContext" connectionString="metadata=res://*/Models.Entity.LibraryModel.csdl|res://*/Models.Entity.LibraryModel.ssdl|res://*/Models.Entity.LibraryModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=YOUR_SERVER_NAME;initial catalog=LibraryManagementSystemDb;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />
   ```
   *Replace `YOUR_SERVER_NAME` with your SQL Server instance name (e.g., `(localdb)\MSSQLLocalDB` or `.` or `localhost`).*

### 3. Open and Restore Project in Visual Studio
1. Double-click the solution file `LibraryManagementSystem.sln` to open it in Visual Studio 2022.
2. Right-click the **Solution** node in the Solution Explorer and select **Restore NuGet Packages** to download all dependencies.
3. Build the solution by pressing `Ctrl + Shift + B` to ensure there are no compilation errors.

### 4. Running the Application
1. Set the startup project to `LibraryManagementSystem` (if not already set).
2. In the top toolbar, select **IIS Express** as the target runner.
3. Press **F5** (or click the green Play button) to compile and launch the application in your web browser.

---

## Test Credentials

Use these pre-populated credentials to test the various portals in the system:

### A. Librarian Portal (Pustakawan)
- **Username**: `librarian`
- **Password**: `password`
- *Access: Full management of books, categories, borrowing approvals, returns, activity logs, and fine payments.*

### B. Member Portal (Anggota)
- **Username**: `member`
- **Password**: `password`
- *Access: Book catalog browsing, requesting book borrowings, viewing personal borrowing history, and profile management.*
