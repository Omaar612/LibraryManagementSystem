namespace LibrarySystem.Services

open LibrarySystem.Models.Book

module LibraryCrud =

    // The library will be a list of books
    let mutable library : Book list = []

    // Add a book
    let addBook (book: Book) =
        library <- library @ [book]
        book

    // Remove a book by Id
    let removeBook (id: int) =
        let exists = library |> List.exists (fun b -> b.Id = id)
        if exists then
            library <- library |> List.filter (fun b -> b.Id <> id)
            true
        else
            false

    // Update a book by Id (title, author, year)
    let updateBook (id: int) (title: string) (author: string) (year: int) =
        library <-
            library
            |> List.map (fun b ->
                if b.Id = id then { b with Title = title; Author = author; Year = year }
                else b)

    // List all books
    let listBooks () =
        library

    // NEW: Clears the library (Useful for testing)
    let clearLibrary () =
        library <- []