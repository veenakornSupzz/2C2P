# Project Overview

## Introduction
The project consists of two main components:
- **APP**: A web application developed using ASP.NET MVC 8.
- **API**: A RESTful API built with ASP.NET Core API 8.

## Third-Party Libraries
- **Bootstrap 5**: For responsive and modern UI components.
- **jQuery**: Simplifies DOM manipulation and event handling.
- **Flatpickr**: Provides user-friendly date and time pickers.
- **SweetAlert2**: Enhances alert messages with customizable and stylized pop-ups.

## Design Patterns Implemented

### Strategy Pattern
Handles different file formats (CSV and XML). Each file type has its own processing strategy implementing a common interface:
- **IFileProcessingStrategy**: An interface defining the contract for file processing.
- **CSVFileProcessingStrategy** and **XMLFileProcessingStrategy**: Concrete implementations for CSV and XML file formats, respectively.

### Factory Pattern
Used alongside the Strategy Pattern to instantiate the appropriate file processing strategy based on the file extension:
- **FileProcessingStrategyFactory**: Determines and returns the correct strategy object for processing the uploaded file.

## Best Practices and Design Patterns

### Separation of Concerns
Divided the application into layers to enhance maintainability and scalability:
- **Controllers**: Handle HTTP requests and responses.
- **Services**: Contain business logic and interact with repositories.
- **Repositories**: Manage data access and database operations.
- **Data Transfer Objects (DTOs)**: Used to transfer data between the App and API.
- **API Response Models**: Standardized API responses using a generic `ApiResponse<T>`.

### Model Validation
- Implemented data annotations and custom validation logic for file uploads.

### Error Handling and Logging
- Comprehensive `try-catch` blocks and logging mechanisms are in place to capture exceptions and log meaningful messages.

---

# Functionalities

## Search Capabilities
The project supports various search options for transaction data, making it easy to filter and retrieve relevant information:

1. **General Transaction Retrieval**:
   - Endpoint: `GET /api/transaction`
   - Retrieves a list of all transactions.

2. **Search by Currency**:
   - Endpoint: `GET /api/transaction/currency/{currencyCode}`
   - Searches for transactions by currency code.

3. **Search by Date Range**:
   - Endpoint: `GET /api/transaction/date`
   - Searches for transactions within a specified date range. Accepts `startDate` and `endDate` as parameters.

4. **Search by Status**:
   - Endpoint: `GET /api/transaction/status/{status}`
   - Searches transactions by status. Supports different status keywords (e.g., `A` for Approved, `R` for Rejected).

5. **File Upload for Transaction Processing**:
   - Endpoint: `POST /api/transaction/upload`
   - Allows uploading a file containing transaction data (e.g., CSV, XML). Processes and validates each transaction entry in the uploaded file.

## APP Search Functionality
- **Currency**: Filters transactions by currency.
- **Status**: Filters transactions by transaction status.
- **Date Range**: Filters transactions within a specific date range.
- **Pagination**: Supports paginated views for easier navigation of transaction data.

---

# Database Setup and Configuration

## SQL Table Scripts
Ensure the following SQL scripts are executed on your SQL Server database to set up the required tables for this project. The scripts are attached as files:
- **Transactions.sql**: Defines the table structure for handling transaction data.
- **TransactionLogs.sql**: Sets up logging for all transaction activities.

## Connection String Configuration
In the `appsettings.json` file of the API project, configure the connection string to connect to your local SQL Server. Update the values based on your server credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=2C2P;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
