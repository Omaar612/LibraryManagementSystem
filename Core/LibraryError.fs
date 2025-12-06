namespace LibrarySystem.Core

type LibraryError =
    | BookNotFound of int
    | DuplicateBookId of int
    // NEW: Error for when Title and Author match an existing book
    | DuplicateBookDetails of string * string 
    | BorrowLimitExceeded of string * int 
    | BookAlreadyBorrowed of string       
    | InvalidInput of string              

module ErrorHelpers =
    let getMessage error =
        match error with
        | BookNotFound id -> $"Book with ID {id} was not found."
        | DuplicateBookId id -> $"Error: A book with ID {id} already exists."
        // NEW: Message for the new error
        | DuplicateBookDetails (title, author) -> $"Error: The book '{title}' by {author} is already in the library."
        | BorrowLimitExceeded (name, count) -> $"Limit Reached: {name} already has {count} books. (Max 2)"
        | BookAlreadyBorrowed name -> $"This book is already borrowed by {name}."
        | InvalidInput msg -> $"Input Error: {msg}"