namespace LibrarySystem.Services

open LibrarySystem.Models.Book
open LibrarySystem.Services.LibraryCrud

module BorrowReturn =

    // Borrow a book by Id
    let borrowBook id =
        let bookOpt = library |> List.tryFind (fun b -> b.Id = id)
        match bookOpt with
        | Some b when b.Status = Available ->
            // Update status to Borrowed
            library <- library |> List.map (fun x ->
                if x.Id = id then { x with Status = Borrowed } else x)
            // Return the updated book
            library |> List.tryFind (fun b -> b.Id = id)
        | Some _ ->
            // Book already borrowed
            None
        | None ->
            // Book not found
            None

    // Return a book by Id
    let returnBook id =
        let bookOpt = library |> List.tryFind (fun b -> b.Id = id)
        match bookOpt with
        | Some b when b.Status = Borrowed ->
            // Update status to Available
            library <- library |> List.map (fun x ->
                if x.Id = id then { x with Status = Available } else x)
            // Return the updated book
            library |> List.tryFind (fun b -> b.Id = id)
        | Some _ ->
            // Book was not borrowed
            None
        | None ->
            // Book not found
            None

