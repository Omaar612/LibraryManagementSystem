# Library Management System (F# & Avalonia)

## 📌 Project Overview
A desktop application for managing library inventory, tracking availability, and processing book loans. This project is built using **F#**, **Avalonia UI**, and **.NET 9**, demonstrating functional programming principles in a real-world scenario.

---

## 🚀 Features
* **Inventory Management:** Add new books with ID, Title, Author, and Year.
* **Search Engine:** Filter books instantly by Title (case-insensitive).
* **Circulation System:**
    * **Borrow:** Check out books (updates status to "Borrowed").
    * **Return:** Return books (updates status to "Available").
* **Availability Tracking:** Visual indicators in the grid show if a book is available or borrowed.
* **Data Persistence:** Automatically saves and loads library data to a JSON file (`library.json`).
* **Modern UI:** Clean, responsive interface built with Avalonia XAML (Light Theme).
* **Automated Testing:** Core logic verified via xUnit tests.

---

## 📂 Project Architecture
The solution is divided into three projects to verify the separation of concerns:

```text
LibrarySystem/
├── LibrarySystem/             # CORE LIBRARY (Backend Logic)
│   ├── Models/Book.fs         # Data Model (Records & Unions)
│   ├── Services/              # Business Logic Modules
│   │   ├── LibraryCrud.fs     # Add, List, Remove logic
│   │   ├── Search.fs          # Search/Filter logic
│   │   └── BorrowReturn.fs    # Status update logic
│   └── Storage/FileStorage.fs # JSON Persistence logic
│
├── LibrarySystemAvalonia/     # UI APPLICATION (Frontend)
│   ├── MainWindow.axaml       # XAML Layout (Grid, Toolbar, Sidebar)
│   ├── MainWindow.axaml.fs    # F# Code-behind (Event Handling)
│   └── App.axaml              # App Entry & Styles
│
└── LibrarySystemTests/        # QUALITY ASSURANCE (Testing)
    └── Tests.fs               # xUnit Automated Tests