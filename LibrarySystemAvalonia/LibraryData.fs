namespace LibrarySystemAvalonia

open LibrarySystem.Models
open LibrarySystem.Services

module LibraryData =
    // FIX: Changed from LibraryService.getLibrary() to LibraryCrud.listBooks()
    let allBooks = LibraryCrud.listBooks()

    let searchBooks (query: string) =
        allBooks
        |> List.filter (fun b -> 
            b.Title.Contains(query, System.StringComparison.OrdinalIgnoreCase) || 
            b.Author.Contains(query, System.StringComparison.OrdinalIgnoreCase))