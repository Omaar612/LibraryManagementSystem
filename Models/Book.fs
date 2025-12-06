namespace LibrarySystem.Models

module Book =

    type Status =
        | Available
        | Borrowed

    type Book = {
        Id: int
        Title: string
        Author: string
        Year: int
        Status: Status
    }

    // Create a new book with default status = Available
    let createBook id title author year =
        {
            Id = id
            Title = title
            Author = author
            Year = year
            Status = Available
        }

    // Turn book into user-friendly text
    let toString (book: Book) =
        let statusStr =
            match book.Status with
            | Available -> "Available"
            | Borrowed -> "Borrowed"

        $"[{book.Id}] {book.Title} by {book.Author} ({book.Year}) - {statusStr}"
