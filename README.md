Intelligent Inventory Dashboard

ASP.NET Core inventory dashboard built with .NET 10, SQL Server, Entity Framework Core, and vanilla JavaScript.

Features
Add vehicles to inventory
View stock, price, arrival date, and inventory status
Request a vehicle price update
Show vehicles awaiting manager approval
Search vehicles by chassis number, make, or model
Collapse and expand the vehicle submission form

Run the Application
Run the project from Visual Studio or  use PowerShell Inside Project folder:
dotnet run --project IntelligentInventorydashboard/IntelligentInventoryDashboard.csproj

Open the HTTPS URL shown by the application, for example 'https://localhost:7151'.
Price Approval Process
Submit a price update from the inventory list.
The vehicle status changes to 'PendingUpdate'.
A user with the 'SeniorManager' role approves or rejects the request through the approval API.
On approval, the new price is saved and the vehicle returns to 'Approved'.
On rejection, the original price remains unchanged.

Run the Application Xunit Test
use PowerShell Inside Project folder
dotnet test IntelligentInventorydashboard\IntelligentInventoryDashboard.csproj

Project Structure
Features - API controllers and handlers
Entities - Database entity classes
Database - Entity Framework database context
Migrations - Database migration files
wwwroot - HTML, CSS, and JavaScript files

AI Features
Manager-Facing AI (Inventory Assistant Agent)
Implemented as a lightweight asynchronous chat widget in the UI (wwwroot/js/app.js)
Translates manager queries into optimized Entity Framework Core LINQ queries
Instantly fetches and renders matching vehicle details
Developer-Facing AI (OpenCode and Terminal Workflows)
Uses OpenCode as a terminal-native AI engineering partner with full repository awareness
Automates backend service scaffolding, Entity Framework migrations, and feature refactoring
Runs autonomous build-and-fix loops through shell commands to resolve compiler errors

Future Enhancements
Require JWT authentication with the 'SeniorManager' role for the manager approval API
Add a '/dev-token' endpoint in the development environment for local testing
Install a local Qwen 32B (or larger) model to support more agentic features, such as Q&A sessions in the UI and code debugging
API Key to enable more RAG for a given chunk of data.
