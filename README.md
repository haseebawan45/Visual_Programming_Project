# ASP.NET Core 7.0 Authentication Project

This is an ASP.NET Core 7.0 MVC application with user authentication using ASP.NET Core Identity.

## Features

- User Authentication (Login & Signup)
- Secure password hashing
- User profile page after login
- Entity Framework Core with SQL Server
- LINQ to query the database

## Requirements

- .NET 7.0 SDK
- Visual Studio Code
- SQL Server (LocalDB is sufficient)

## Setup and Run Instructions

1. Clone this repository or download the project files.

2. Open the project folder in Visual Studio Code.

3. Open a terminal in Visual Studio Code and navigate to the project directory.

4. Verify that you have the necessary .NET SDK installed:
   ```
   dotnet --version
   ```

5. Restore the project dependencies:
   ```
   dotnet restore
   ```

6. Install the Entity Framework Core tools if not already installed:
   ```
   dotnet tool install --global dotnet-ef
   ```

7. Create the initial database migration:
   ```
   dotnet ef migrations add InitialCreate
   ```

8. Apply the migration to create the database:
   ```
   dotnet ef database update
   ```

9. Run the application:
   ```
   dotnet run
   ```

10. Open your browser and navigate to:
    ```
    https://localhost:5001
    or
    http://localhost:5000
    ```

## Project Structure

- **Models**: Contains ApplicationUser model inheriting from IdentityUser
- **ViewModels**: Contains view models for login and registration
- **Controllers**: Contains HomeController and AccountController 
- **Views**: Contains Razor views for the application
- **Data**: Contains the database context (AppDbContext)

## Notes

- The application uses SQL Server LocalDB by default. You can modify the connection string in `appsettings.json` if needed.
- All passwords are securely hashed using ASP.NET Core Identity's password hasher.
- The application includes validation for email, password, and confirm password fields. 