# 📚 Library Management System

A robust, functional Library Management System built with **F\#** and **Avalonia UI**. This application allows librarians to manage book inventory, track borrowed items, and handle user returns with strict validation rules and persistent data storage.

-----

## 📋 Table of Contents

1.  [Prerequisites](https://www.google.com/search?q=%23-prerequisites)
2.  [Installation & Setup](https://www.google.com/search?q=%23-installation--setup)
3.  [How to Run the Application](https://www.google.com/search?q=%23-how-to-run-the-application)
4.  [Running Tests](https://www.google.com/search?q=%23-running-tests)
5.  [Project Architecture](https://www.google.com/search?q=%23-project-architecture)
6.  [Features](https://www.google.com/search?q=%23-features)

------

## 🛠 Prerequisites

Before you begin, ensure you have the following installed on your machine:

1.  **The .NET 9.0 SDK**

      * **Download:** [Download .NET 9.0](https://dotnet.microsoft.com/download/dotnet/9.0)
      * **Verify:** Run `dotnet --version` in your terminal.

2.  **Git** (Optional)

      * **Download:** [git-scm.com](https://www.google.com/search?q=https://git-scm.com/downloads)

3.  **Code Editor** (Recommended)

      * **Visual Studio Code** with the **Ionide for F\#** extension.
      * Or **Visual Studio 2022**.

-----

## 📥 Installation & Setup

### 1\. Download the Project

Clone the repository or download the ZIP file.

```bash
git clone https://github.com/omaar612/librarymanagementsystem.git
cd librarymanagementsystem
```

### 2\. Restore Dependencies

Download necessary libraries (Avalonia, xUnit, etc.).

```bash
dotnet restore
```

-----

## 🚀 How to Run the Application

The application uses **Avalonia UI** for its desktop interface.

1.  **Run the Command**:

    ```bash
    dotnet run --project LibrarySystemAvalonia
    ```

2.  **Using the App**:

      * **Add Books**: Use the top panel to enter Title, Author, Year, and Quantity.
      * **Borrow**: Click the "Borrow" button on a row. Enter a valid name and phone (11 digits, starts with '01').
      * **Return**: Click "Return" to restore stock.
      * **Search**: Filter the grid using the search bar.

-----

## 🧪 Running Tests

Verify the business logic using the **xUnit** test suite.

1.  **Run Tests**:

    ```bash
    dotnet test
    ```

2.  **Output**:

      * **Passed**: The feature is working correctly.
      * **Failed**: The error message will indicate what logic was violated (e.g., "Expected phone validation error").

-----

## 📂 Project Architecture

The solution is split into three distinct projects to strictly separate the Backend (Core), Frontend (UI), and Testing.

```text
LibrarySystem/
├── LibrarySystem/               # CORE LIBRARY (Backend Logic & Data)
│   ├── LibrarySystem.fsproj     # Project Configuration
│   ├── Core/
│   │   └── LibraryError.fs      # Error Handling (Union types for Failures)
│   ├── Models/
│   │   └── Book.fs              # Data Models (Book Record, Borrower Info)
│   ├── Services/
│   │   ├── LibraryCrud.fs       # Inventory Logic (Add, Sanitization, Stock)
│   │   ├── BorrowReturn.fs      # Transaction Logic (Borrow rules, Return)
│   │   └── Search.fs            # Search Filtering Logic
│   └── Storage/
│       └── FileStorage.fs       # JSON Persistence (Save/Load Data)
│
├── LibrarySystemAvalonia/       # UI APPLICATION (Frontend)
│   ├── LibrarySystemAvalonia.fsproj
│   ├── Program.fs               # Application Entry Point
│   ├── App.axaml                # Global App Styles & Resources
│   ├── App.axaml.fs             # App Lifecycle Management
│   ├── MainWindow.axaml         # Main UI Layout (Grid, Toolbar, Sidebar)
│   ├── MainWindow.axaml.fs      # UI Logic & Event Handlers
│   ├── BorrowDialog.fs          # Custom Popup Window for User Input
│   ├── LibraryData.fs           # Observable Collection (Data Binding)
│   ├── app.manifest             # Windows Application Manifest
│   └── library.json             # Runtime Data Store (Generated)
│
└── LibrarySystemTests/          # QUALITY ASSURANCE (Testing)
    ├── LibrarySystemTests.fsproj
    └── Tests.fs                 # xUnit Automated Test Scenarios
```

### Architecture Breakdown

1.  **Core Layer (`LibrarySystem`)**: Contains pure F\# code. It defines *what* a Book is and *how* transactions work. It has no knowledge of the UI.
2.  **UI Layer (`LibrarySystemAvalonia`)**: Handles the visuals. It collects user input and calls the Core Layer to perform actions. It uses `LibraryData.fs` to ensure the screen updates automatically when data changes.
3.  **Test Layer (`LibrarySystemTests`)**: A separate project that imports the Core Layer to verify its logic in isolation, ensuring bugs are caught before the app is run.

-----

## ✨ Features

  * **Strict Input Validation**:
      * **Authors**: Rejects names containing digits or symbols.
      * **Phones**: Must be 11 digits and start with "01".
  * **Inventory Tracking**: Real-time calculation of "Available" vs "Total" copies.
  * **Smart Borrowing**:
      * Limit of 2 books per user.
      * Identity verification (One name per phone number).
  * **Search**: Case-insensitive filtering.
  * **Persistence**: Data survives application restarts via JSON storage.
