module LibrarySystemTests

open System
open Xunit
open LibrarySystem.Models.Book
open LibrarySystem.Services

// This class holds the tests
type LibraryTests() =

    // specific Setup function isn't always needed in simple F# tests, 
    // but we should clear the library at the start of every test 
    // to ensure a clean slate.
    let setup() =
        LibraryCrud.clearLibrary()

    [<Fact>]
    member this.``Test 1: Add Book increases library count`` () =
        // 1. Arrange (Setup)
        setup()
        let book = createBook 1 "Test Book" "Tester" 2025
        
        // 2. Act (Do the action)
        LibraryCrud.addBook book |> ignore
        let currentLibrary = LibraryCrud.listBooks()

        // 3. Assert (Check the result)
        Assert.Equal(1, List.length currentLibrary)
        Assert.Equal("Test Book", currentLibrary.Head.Title)

    [<Fact>]
    member this.``Test 2: Borrow Book changes status to Borrowed`` () =
        // 1. Arrange
        setup()
        let book = createBook 1 "Test Book" "Tester" 2025
        LibraryCrud.addBook book |> ignore

        // 2. Act
        BorrowReturn.borrowBook 1 |> ignore
        
        // Retrieve the book again to check its status
        let updatedBook = LibraryCrud.listBooks() |> List.head

        // 3. Assert
        Assert.Equal(Borrowed, updatedBook.Status)

    [<Fact>]
    member this.``Test 3: Search finds the correct book`` () =
        // 1. Arrange
        setup()
        LibraryCrud.addBook (createBook 1 "Harry Potter" "Rowling" 2000) |> ignore
        LibraryCrud.addBook (createBook 2 "Lord of the Rings" "Tolkien" 1954) |> ignore

        // 2. Act
        let results = Search.searchByTitle "Potter"

        // 3. Assert
        Assert.Equal(1, List.length results)
        Assert.Equal("Harry Potter", results.Head.Title)