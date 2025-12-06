namespace LibrarySystem.Services

open LibrarySystem.Models.Book
open System.Text.RegularExpressions // Needed for the space replacement logic

module LibraryCrud =

    let mutable library : Book list = []

    // Helper for case-insensitive comparison
    let private equalsIgnoreCase (a: string) (b: string) =
        System.String.Equals(a, b, System.StringComparison.OrdinalIgnoreCase)

    // --- NEW: SANITIZATION HELPER ---
    // automatically removes extra spaces
    let private sanitize (text: string) =
        if System.String.IsNullOrWhiteSpace(text) then ""
        else
            // 1. Trim start and end spaces
            let trimmed = text.Trim()
            // 2. Replace any sequence of whitespace (\s+) with a single space " "
            Regex.Replace(trimmed, @"\s+", " ")

    // 1. Add New Book (Now Sanitizes Input Automatically)
    let addBook (rawTitle: string) (rawAuthor: string) (year: int) (quantity: int) =
        
        // A. CLEAN THE INPUTS AUTOMATICALLY
        let title = sanitize rawTitle
        let author = sanitize rawAuthor

        // B. VALIDATIONS (Using the cleaned strings)
        
        // 1. Check if Author is empty (after cleaning)
        if System.String.IsNullOrWhiteSpace(author) then
            Error (InvalidInput "Author cannot be empty.")
        
        // 2. Check Year
        else if year > System.DateTime.Now.Year then
            Error (InvalidInput $"Year cannot be in the future (Max: {System.DateTime.Now.Year}).")
        
        // 3. Check Quantity
        else if quantity <= 0 then
            Error (InvalidInput "Quantity must be > 0.")
        
        // 4. Check Title (after cleaning)
        else if System.String.IsNullOrWhiteSpace(title) then
            Error (InvalidInput "Title cannot be empty.")
        
        // 5. Check Duplicates (using cleaned strings)
        else if library |> List.exists (fun b -> equalsIgnoreCase b.Title title && equalsIgnoreCase b.Author author) then
            Error (DuplicateBookDetails (title, author))
        else
            // Success: Create using the CLEAN title/author
            let newBook = createBook title author year quantity
            library <- library @ [newBook]
            Ok newBook

    // 2. Add Copies (Also sanitizes title search)
    let addCopies (rawTitle: string) quantity =
        let title = sanitize rawTitle // Clean the search term too

        if quantity <= 0 then Error (InvalidInput "Quantity must be > 0")
        else
            match library |> List.tryFind (fun b -> equalsIgnoreCase b.Title title) with
            | Some book ->
                let updated = { book with TotalQuantity = book.TotalQuantity + quantity }
                library <- library |> List.map (fun b -> if b.Title = book.Title then updated else b)
                Ok updated
            | None -> Error (BookNotFound title)

    // 3. Remove Copy (Feature: Delete a copy if available)
    let removeCopy (title: string) =
        match library |> List.tryFind (fun b -> equalsIgnoreCase b.Title title) with
        | Some book ->
            // Calculate available copies
            let available = book.TotalQuantity - book.Borrowers.Length
            
            // Constraint: Can only remove if we have a physical copy on the shelf
            if available > 0 then
                // Logic: Decrease TotalQuantity by exactly 1
                let updated = { book with TotalQuantity = book.TotalQuantity - 1 }
                library <- library |> List.map (fun b -> if b.Title = book.Title then updated else b)
                Ok updated
            else
                Error (InvalidInput "Cannot remove copy: All copies are currently borrowed or stock is 0.")
        | None -> Error (BookNotFound title)

    // Prepare data for UI
    let getBooksForGrid () =
        library |> List.map (fun b ->
            let borrowedCount = b.Borrowers.Length
            let availableCount = b.TotalQuantity - borrowedCount
            let borrowerList = b.Borrowers |> List.map (fun x -> x.Name, x.PhoneNumber)
            (b.Title, b.Author, b.Year, b.TotalQuantity, availableCount, borrowedCount, borrowerList)
        )

    let getBookTitles () = library |> List.map (fun b -> b.Title) |> List.distinct
    let listBooks () = library
    let clearLibrary () = library <- []