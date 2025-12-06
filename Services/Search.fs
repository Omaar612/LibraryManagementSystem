namespace LibrarySystem.Services

open LibrarySystem.Models.Book
open LibrarySystem.Services.LibraryCrud

module Search =

    // Helper for case-insensitive check
    let private containsIgnoreCase (text: string) (query: string) =
        if text = null then false
        else text.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0

    // Search by title (partial match)
    let searchByTitle query =
        library |> List.filter (fun b -> containsIgnoreCase b.Title query)

    // Search by author (partial match)
    let searchByAuthor query =
        library |> List.filter (fun b -> containsIgnoreCase b.Author query)