# Product Manager Mini API

## Table of Contents

- [Description](#description)
- [Technologies Used](#technologies-used)
- [Installation](#installation)
- [Usage](#usage)
- [How It Works](#how-it-works)
- [Security](#security)
- [What I Learned](#what-i-learned)
- [Contributing](#contributing)
- [License](#license)
- [Author](#author)

## Description

The **Product Manager Mini API** is a lightweight RESTful API developed using ASP.NET Core minimal APIs. It enables users to perform CRUD (Create, Read, Update, Delete) operations on product data stored in a JSON file. The API is designed for simplicity and serves as an educational project to explore modern .NET development practices, including dependency injection, data validation, and API documentation.

The project is hosted on [GitHub](https://github.com/ersinisgor/Product-Manager-Mini-API) and is ideal for learning how to build and structure a basic API.

## Technologies Used

- **Programming Language**: C#
- **Framework**: .NET 8.0
- **API Style**: Minimal APIs
- **Data Storage**: JSON file
- **Validation**: System.ComponentModel.DataAnnotations
- **Dependency Injection**: For services like `IFileService` and `IProductService`
- **Serialization**: System.Text.Json
- **Documentation**: Swagger (Swashbuckle.AspNetCore)

## Installation

To run the project locally, follow these steps:

1. Clone the repository from [GitHub](https://github.com/ersinisgor/Product-Manager-Mini-API):
   ```bash
   git clone https://github.com/ersinisgor/Product-Manager-Mini-API.git
   ```
2. Ensure you have the [.NET SDK](https://dotnet.microsoft.com/download) installed (version 6.0 or later).
3. Navigate to the project directory:
   ```bash
   cd Product-Manager-Mini-API
   ```
4. Restore the dependencies:
   ```bash
   dotnet restore
   ```
5. Build the project:
   ```bash
   dotnet build
   ```
6. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `https://localhost:7279` (or the port specified in the launch settings).

## Usage

The API provides the following endpoints for managing products:

- **GET /products**: Retrieves all products.
- **GET /products/{id}**: Retrieves a product by its ID.
- **POST /products**: Creates a new product. Requires a JSON body with `name`, `price`, and `category`.
- **PUT /products/{id}**: Updates an existing product. Accepts a JSON body with optional fields to update.
- **DELETE /products/{id}**: Deletes a product by its ID.

### Example Requests

- **Create a Product (POST /products)**:

  ```json
  {
    "name": "Laptop",
    "price": 1500.99,
    "category": "Electronics"
  }
  ```

  **Response**: 201 Created with the created product and `Location: /products/{id}` header.

- **Update a Product (PUT /products/1)**:
  ```json
  {
    "name": "Updated Laptop",
    "price": 1600.0
  }
  ```
  **Response**: 200 OK with the updated product.

API documentation is available at `/swagger` when the application is running, providing an interactive interface to test the endpoints.

## How It Works

The application is structured with a focus on modularity and separation of concerns:

- **`IFileService`**: An interface that defines methods for reading and writing product data to a JSON file (`products.json`). The `FileService` class implements this interface, handling file I/O operations.
- **`IProductService`**: An interface that defines the business logic for product management, including CRUD operations. The `ProductService` class implements this interface, using `IFileService` for data access.
- **Dependency Injection**: Both services are registered in `Program.cs` using ASP.NET Core’s dependency injection, ensuring loose coupling and testability.
- **Data Validation**: `CreateProductDTO` and `UpdateProductDTO` use DataAnnotations to enforce input validation, ensuring data integrity.
- **Error Handling**: Comprehensive error handling is implemented for JSON parsing errors, file access issues, and unexpected exceptions, returning standardized HTTP responses.

The use of interfaces (`IFileService` and `IProductService`) allows for easy swapping of implementations (e.g., replacing file storage with a database) without modifying the core logic.

## Security

The API includes authorization middleware (`app.UseAuthorization()`), but no authentication mechanism is implemented. For production use, you should add authentication (e.g., JWT or OAuth) to secure the endpoints and protect against unauthorized access.

## What I Learned

This project was a significant learning experience, providing hands-on exposure to modern .NET development practices. Key takeaways include:

- **ASP.NET Core Minimal APIs**: Learned how to build lightweight, efficient APIs with minimal boilerplate code.
- **Dependency Injection**: Mastered the use of dependency injection to create modular, testable code by injecting services like `IFileService` and `IProductService`.
- **Interfaces and Abstraction**: Understood the importance of interfaces for defining contracts, enabling flexibility and testability.
- **Data Validation**: Gained experience with `System.ComponentModel.DataAnnotations` for validating input data, ensuring robust API behavior.
- **JSON Serialization**: Learned to handle JSON data using `System.Text.Json`, including serialization and deserialization of complex objects.
- **File I/O Operations**: Developed skills in reading and writing data to a JSON file, managing file access and error handling.
- **Error Handling**: Implemented comprehensive error handling for API endpoints, returning standardized HTTP responses for various error scenarios.
- **Swagger Integration**: Learned to integrate and use Swagger for API documentation, improving developer experience and testing.

This project deepened my understanding of building RESTful APIs and applying software engineering principles like separation of concerns and dependency injection.

## Author

This project was created by [ersinisgor](https://ersinisgor.netlify.app/).
