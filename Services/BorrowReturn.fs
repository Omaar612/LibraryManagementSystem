namespace LibrarySystem.Services

open LibrarySystem.Models.Book
open LibrarySystem.Services.LibraryCrud
open System.Text.RegularExpressions // Required for Regex

module BorrowReturn =

    // Helper.
    let private equalsIgnoreCase (a: string) (b: string) =
        System.String.Equals(a, b, System.StringComparison.OrdinalIgnoreCase)

    let borrowBook title name phone =
        // 1. Validate Name
        if System.String.IsNullOrWhiteSpace(name) then 
            Error (InvalidInput "Name required")
        else if not (Regex.IsMatch(name, @"^[\p{L}\s]+$")) then
            Error (InvalidInput "Borrower name cannot contain digits or symbols.")

        // 2. Validate Phone
        else if System.String.IsNullOrWhiteSpace(phone) then 
            Error (InvalidInput "Phone required")
        // Check: Starts with 01, exactly 11 digits total (01 + 9 digits)
        else if not (Regex.IsMatch(phone, @"^01\d{9}$")) then
            Error (InvalidInput "Phone must be 11 digits, start with '01', and contain only numbers.")

        else
            // Check Identity
            let isPhoneUsedByOther = 
                library |> List.exists (fun book -> 
                    book.Borrowers |> List.exists (fun b -> b.PhoneNumber = phone && b.Name <> name))
            
            if isPhoneUsedByOther then 
                Error (InvalidInput "This phone number is used by another name.")
            else
                // Check Limit
                let currentBorrows = 
                    library |> List.sumBy (fun book -> 
                        book.Borrowers |> List.filter (fun b -> b.PhoneNumber = phone) |> List.length)

                if currentBorrows >= 2 then
                    Error (BorrowLimitExceeded (name, currentBorrows))
                else
                    // Find book (Case Insensitive)
                    match library |> List.tryFind (fun b -> equalsIgnoreCase b.Title title) with
                    | Some book ->
                        if book.Borrowers.Length >= book.TotalQuantity then
                            Error (InvalidInput "No copies available.")
                        else
                            let newB = { Name = name; PhoneNumber = phone; BorrowedDate = System.DateTime.Now }
                            let updated = { book with Borrowers = book.Borrowers @ [newB] }
                            // Update using the EXACT title found in the match, not the user input string
                            library <- library |> List.map (fun b -> if b.Title = book.Title then updated else b)
                            Ok updated
                    | None -> Error (BookNotFound title)

    let returnBook title phone =
        // Find book (Case Insensitive)
        match library |> List.tryFind (fun b -> equalsIgnoreCase b.Title title) with
        | Some book ->
            let exists = book.Borrowers |> List.exists (fun b -> b.PhoneNumber = phone)
            if exists then
                let rec removeFirst list =
                    match list with
                    | [] -> []
                    | h :: t -> if h.PhoneNumber = phone then t else h :: removeFirst t
                
                let updated = { book with Borrowers = removeFirst book.Borrowers }
                library <- library |> List.map (fun b -> if b.Title = book.Title then updated else b)
                Ok updated
            else
                Error (InvalidInput "Borrower not found for this book.")
        | None -> Error (BookNotFound title)
