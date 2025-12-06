open System
open LibrarySystem.Models.Book
open LibrarySystem.Services.LibraryCrud
open LibrarySystem.Services.Search
open LibrarySystem.Services.BorrowReturn
open LibrarySystem.Storage.FileStorage

[<EntryPoint>]
let main argv =

    // Load existing library
    loadLibrary ()

    // If empty, add sample books
    if List.isEmpty (listBooks ()) then
        addBook (createBook 1 "1984" "George Orwell" 1949) |> ignore
        addBook (createBook 2 "Brave New World" "Aldous Huxley" 1932) |> ignore
        addBook (createBook 3 "Animal Farm" "George Orwell" 1945) |> ignore

    printfn "Books before borrowing:"
    listBooks () |> List.iter (fun b -> printfn "%s" (toString b))

    // Borrow a book
    match borrowBook 2 with
    | Some b -> printfn "\nBorrowed: %s" (toString b)
    | None -> printfn "\nCannot borrow book with ID 2"

    // Return a book
    match returnBook 2 with
    | Some b -> printfn "\nReturned: %s" (toString b)
    | None -> printfn "\nCannot return book with ID 2"

    printfn "\nBooks after borrow/return operations:"
    listBooks () |> List.iter (fun b -> printfn "%s" (toString b))

    // Save library to file
    saveLibrary ()

    0
