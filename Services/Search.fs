namespace LibrarySystem.Services

open LibrarySystem.Models.Book
open LibrarySystem.Services.LibraryCrud

module Search =

    // Search by ID
    let searchById id =
        library |> List.tryFind (fun b -> b.Id = id)

    // Case-insensitive contains helper
    let private containsIgnoreCase (text: string) (query: string) =
        text.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0

    // Search by title (partial match)
    let searchByTitle query =
        library |> List.filter (fun b -> containsIgnoreCase b.Title query)

    // Search by author (partial match)
    let searchByAuthor query =
        library |> List.filter (fun b -> containsIgnoreCase b.Author query)
