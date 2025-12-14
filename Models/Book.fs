namespace LibrarySystem.Models

module Book =

    // --- ERROR TYPES ---
    type LibraryError =
        | BookNotFound of string
        | DuplicateBookDetails of string * string
        | BorrowLimitExceeded of string * int
        | BookAlreadyBorrowed of string
        | InvalidInput of string

    module ErrorHelpers =
        let getMessage error =
            match error with
            | BookNotFound t -> $"Book '{t}' was not found."
            | DuplicateBookDetails (t, a) -> $"Book '{t}' by {a} already exists."
            | BorrowLimitExceeded (n, c) -> $"{n} has reached the limit of 2 books (Current: {c})."
            | BookAlreadyBorrowed n -> $"This book is borrowed by {n}."
            | InvalidInput msg -> $"Input Error: {msg}"

    // --- DATA TYPES ---
    type BorrowerInfo = {
        Name: string
        PhoneNumber: string
        BorrowedDate: System.DateTime
    }

    type Book = {
        Title: string
        Author: string
        Year: int
        TotalQuantity: int
        Borrowers: BorrowerInfo list
    }

    // Create a new book
    let createBook title author year quantity =
        { Title = title; Author = author; Year = year; TotalQuantity = quantity; Borrowers = [] }
