module LibrarySystemTests

open System
open Xunit
open LibrarySystem.Models.Book
open LibrarySystem.Services

// This class holds all the tests
type LibraryTests() =

    // --- SETUP ---
    // Clears the library before every test to ensure isolation
    let setup() =
        LibraryCrud.clearLibrary()

    // --- BOOK MANAGEMENT TESTS ---

    [<Fact>]
    member this.``Add Book - Success`` () =
        setup()
        let result = LibraryCrud.addBook "The Hobbit" "JRR Tolkien" 1937 5
        
        match result with
        | Ok book -> 
            Assert.Equal("The Hobbit", book.Title)
            Assert.Equal(1, LibraryCrud.listBooks().Length)
        | Error e -> Assert.Fail($"Expected Success, but got Error: {e}")

    // Replaces individual author validation tests with a single Theory
    [<Theory>]
    [<InlineData("Author1")>]
    [<InlineData("Author!")>]
    [<InlineData("Author@Home")>]
    member this.``Add Book - Fails for various invalid authors`` (invalidAuthor: string) =
        setup()
        let result = LibraryCrud.addBook "Title" invalidAuthor 2020 5
        
        match result with
        | Error (InvalidInput msg) -> Assert.Contains("digits or symbols", msg)
        | _ -> Assert.Fail($"Author '{invalidAuthor}' should have been rejected.")

    [<Fact>]
    member this.``Add Book - Fails for Future Year`` () =
        setup()
        let futureYear = DateTime.Now.Year + 1
        let result = LibraryCrud.addBook "Future Book" "Author" futureYear 5
        
        match result with
        | Error (InvalidInput msg) -> Assert.Contains("future", msg)
        | _ -> Assert.Fail("Should fail for future year")

    [<Fact>]
    member this.``Add Book - Fails for Duplicate`` () =
        setup()
        // Add first copy
        LibraryCrud.addBook "Harry Potter" "Rowling" 2000 2 |> ignore
        // Try adding the exact same book again
        let result = LibraryCrud.addBook "Harry Potter" "Rowling" 2000 2
        
        match result with
        | Error (DuplicateBookDetails _) -> () // Pass
        | _ -> Assert.Fail("Expected DuplicateBookDetails error")

    [<Fact>]
    member this.``Add Book - Trims and cleans extra spaces`` () =
        setup()
        // Input has mess of spaces. 
        // NOTE: Removed '.' from "F. Scott" to comply with "No Symbols" validation rule.
        let result = LibraryCrud.addBook "  The   Great    Gatsby  " "  F   Scott   Fitzgerald  " 1925 5
        
        match result with
        | Ok book -> 
            // Assert it was stored cleanly
            Assert.Equal("The Great Gatsby", book.Title)
            Assert.Equal("F Scott Fitzgerald", book.Author)
        | Error e -> Assert.Fail($"Should succeed but failed: {e}")

    // --- BORROWING VALIDATION TESTS ---

    // Replaces individual phone validation tests with a single Theory
    [<Theory>]
    [<InlineData("01234")>]          // Too short
    [<InlineData("02123456789")>]    // Doesn't start with 01
    [<InlineData("0123456789a")>]    // Contains letter
    [<InlineData("0123456789!")>]    // Contains symbol
    member this.``Borrow - Fails for various invalid phones`` (invalidPhone: string) =
        setup()
        LibraryCrud.addBook "Book" "Author" 2020 5 |> ignore
        
        let result = BorrowReturn.borrowBook "Book" "User" invalidPhone
        
        match result with
        | Error (InvalidInput _) -> () // Pass
        | _ -> Assert.Fail($"Phone number '{invalidPhone}' should have been rejected.")

    [<Fact>]
    member this.``Borrow - Fails for Identity Mismatch (Phone Theft)`` () =
        setup()
        LibraryCrud.addBook "Book 1" "Author" 2020 5 |> ignore
        LibraryCrud.addBook "Book 2" "Author" 2020 5 |> ignore

        // 1. John borrows a book with his phone
        BorrowReturn.borrowBook "Book 1" "John Doe" "01122334455" |> ignore
        
        // 2. Jane tries to use John's phone
        let result = BorrowReturn.borrowBook "Book 2" "Jane Doe" "01122334455"
        
        match result with
        | Error (InvalidInput msg) -> Assert.Equal("This phone number is used by another name.", msg)
        | _ -> Assert.Fail("Expected identity mismatch error")

    [<Fact>]
    member this.``Borrow - Fails when Borrow Limit Exceeded (Max 2)`` () =
        setup()
        LibraryCrud.addBook "Book 1" "Author" 2020 5 |> ignore
        LibraryCrud.addBook "Book 2" "Author" 2020 5 |> ignore
        LibraryCrud.addBook "Book 3" "Author" 2020 5 |> ignore
        
        let user = "Reader"
        let phone = "01000000001"

        // Borrow 2 books (Limit reached)
        BorrowReturn.borrowBook "Book 1" user phone |> ignore
        BorrowReturn.borrowBook "Book 2" user phone |> ignore
        
        // Try Borrowing 3rd book
        let result = BorrowReturn.borrowBook "Book 3" user phone
        
        match result with
        | Error (BorrowLimitExceeded _) -> () // Pass
        | _ -> Assert.Fail("Expected BorrowLimitExceeded error")

    // --- STOCK & RETURN TESTS ---

    [<Fact>]
    member this.``Borrow - Decreases Available Copies`` () =
        setup()
        // Add book with exactly 1 copy
        LibraryCrud.addBook "Rare Book" "Author" 2020 1 |> ignore
        
        // Borrow it
        BorrowReturn.borrowBook "Rare Book" "User One" "01234567891" |> ignore
        
        // Try to borrow the same book (Stock should be 0)
        let result = BorrowReturn.borrowBook "Rare Book" "User Two" "01234567892"
        
        match result with
        | Error (InvalidInput msg) -> Assert.Equal("No copies available.", msg)
        | _ -> Assert.Fail("Should fail because no copies are left")

    [<Fact>]
    member this.``Return - Success`` () =
        setup()
        LibraryCrud.addBook "Book" "Author" 2020 5 |> ignore
        BorrowReturn.borrowBook "Book" "User" "01234567890" |> ignore
        
        let result = BorrowReturn.returnBook "Book" "01234567890"
        
        match result with
        | Ok updatedBook -> Assert.Equal(0, updatedBook.Borrowers.Length)
        | Error e -> Assert.Fail($"Return failed: {e}")

    [<Fact>]
    member this.``Return - Fails if user did not borrow the book`` () =
        setup()
        LibraryCrud.addBook "Book A" "Author" 2020 5 |> ignore
        
        // Try to return without borrowing first
        let result = BorrowReturn.returnBook "Book A" "01234567899"
        
        match result with
        | Error (InvalidInput msg) -> Assert.Equal("Borrower not found for this book.", msg)
        | _ -> Assert.Fail("Should fail because this phone number never borrowed the book")

    // --- REMOVE & SEARCH TESTS ---

    [<Fact>]
    member this.``Remove Copy - Fails when all copies are borrowed`` () =
        setup()
        // 1. Add a book with only 1 copy
        LibraryCrud.addBook "Rare Item" "Author" 2020 1 |> ignore
        
        // 2. Borrow that single copy
        BorrowReturn.borrowBook "Rare Item" "User" "01234567890" |> ignore
        
        // 3. Try to remove the copy from the library
        let result = LibraryCrud.removeCopy "Rare Item"
        
        match result with
        | Error (InvalidInput msg) -> Assert.Contains("borrowed", msg)
        | _ -> Assert.Fail("Should not allow removing a copy if it is currently borrowed out")

    [<Fact>]
    member this.``Remove Copy - Success when copy is available`` () =
        setup()
        LibraryCrud.addBook "Common Book" "Author" 2020 5 |> ignore
        
        let result = LibraryCrud.removeCopy "Common Book"
        
        match result with
        | Ok book -> Assert.Equal(4, book.TotalQuantity) // Should drop from 5 to 4
        | Error _ -> Assert.Fail("Should successfully remove a copy")

    [<Fact>]
    member this.``Search - Finds book case-insensitively`` () =
        setup()
        LibraryCrud.addBook "Harry Potter" "Rowling" 2000 5 |> ignore
        
        // Search with lowercase 'p'
        let results = Search.searchByTitle "potter"
        
        Assert.Equal(1, results.Length)
        Assert.Equal("Harry Potter", results.Head.Title)

    // --- INTEGRATION SCENARIO ---

    [<Fact>]
    member this.``Scenario: Stock runs out and becomes available again`` () =
        setup()
        // 1. Librarrian adds a book with only 1 copy
        LibraryCrud.addBook "Rare Book" "Author" 2020 1 |> ignore
        
        // 2. User A borrows it (Success)
        let res1 = BorrowReturn.borrowBook "Rare Book" "User A" "01000000001"
        Assert.True(Result.isOk res1, "User A should borrow successfully")

        // 3. User B tries to borrow it (Fail - No stock)
        let res2 = BorrowReturn.borrowBook "Rare Book" "User B" "01000000002"
        match res2 with
        | Error (InvalidInput msg) -> Assert.Contains("No copies", msg)
        | _ -> Assert.Fail("User B should fail because stock is empty")

        // 4. User A returns the book
        BorrowReturn.returnBook "Rare Book" "01000000001" |> ignore

        // 5. User B tries to borrow it again (Success - Stock is back)
        let res3 = BorrowReturn.borrowBook "Rare Book" "User B" "01000000002"
        Assert.True(Result.isOk res3, "User B should now succeed")
