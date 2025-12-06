namespace LibrarySystem.Storage

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open LibrarySystem.Models.Book
open LibrarySystem.Services.LibraryCrud

module FileStorage =

    let private filePath = "library.json"

    // 1. OLD FORMAT (To read your current file)
    type OldBook = {
        Id: int
        Title: string
        Author: string
        Year: int
        Status: string
        BorrowerName: string option
        BorrowerPhone: string option
    }

    // 2. JSON Options (Handles F# types)
    let getJsonOptions () =
        let options = JsonSerializerOptions(WriteIndented = true)
        options.Converters.Add(JsonFSharpConverter())
        options

    let saveLibrary () =
        let json = JsonSerializer.Serialize(library, getJsonOptions())
        File.WriteAllText(filePath, json)

    // 3. CONVERSION LOGIC
    let convertOldToNew (oldBooks: OldBook list) : Book list =
        oldBooks
        // Group by Title/Author/Year to merge duplicates into Quantity
        |> List.groupBy (fun b -> b.Title, b.Author, b.Year)
        |> List.map (fun ((t, a, y), copies) ->
            let totalQty = copies.Length
            // Collect borrowers from the old copies
            let borrowers = 
                copies |> List.choose (fun b ->
                    match b.BorrowerName, b.BorrowerPhone with
                    | Some name, Some phone when b.Status = "Borrowed" -> 
                        Some { Name = name; PhoneNumber = phone; BorrowedDate = DateTime.Now }
                    | _ -> None
                )
            { Title = t; Author = a; Year = y; TotalQuantity = totalQty; Borrowers = borrowers }
        )

    let loadLibrary () =
        if File.Exists(filePath) then
            try
                let json = File.ReadAllText(filePath)
                try
                    // A. Try loading NEW format first
                    let books = JsonSerializer.Deserialize<Book list>(json, getJsonOptions())
                    library <- books
                with _ ->
                    // B. If failed, try loading OLD format and convert
                    try
                        let oldBooks = JsonSerializer.Deserialize<OldBook list>(json, getJsonOptions())
                        let newBooks = convertOldToNew oldBooks
                        library <- newBooks
                        // Automatically save the new format so next time it loads fast
                        saveLibrary() 
                    with _ ->
                        library <- [] // If everything fails, start empty
            with
            | ex -> printfn "Error loading file: %s" ex.Message