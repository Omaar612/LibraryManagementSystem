namespace LibrarySystem.Storage

open System
open System.IO
open System.Text.Json
open LibrarySystem.Models.Book
open LibrarySystem.Services.LibraryCrud

module FileStorage =

    let private filePath = "library.json"

    // Convert Status DU to string
    let private statusToString = function
        | Available -> "Available"
        | Borrowed -> "Borrowed"

    let private statusFromString = function
        | "Available" -> Available
        | "Borrowed" -> Borrowed
        | _ -> Available

    // Helper type for serialization
    type SerializableBook = {
        Id: int
        Title: string
        Author: string
        Year: int
        Status: string
    }

    // Save the current library to a JSON file
    let saveLibrary () =
        let serializableBooks = library |> List.map (fun b ->
            { Id = b.Id; Title = b.Title; Author = b.Author; Year = b.Year; Status = statusToString b.Status })
        let json = JsonSerializer.Serialize(serializableBooks, JsonSerializerOptions(WriteIndented = true))
        File.WriteAllText(filePath, json)

    // Load the library from a JSON file
    let loadLibrary () =
        if File.Exists(filePath) then
            try
                let json = File.ReadAllText(filePath)
                let books = JsonSerializer.Deserialize<SerializableBook list>(json)
                library <- books |> List.map (fun b ->
                    { Id = b.Id; Title = b.Title; Author = b.Author; Year = b.Year; Status = statusFromString b.Status })
            with
            | _ -> ()  // If deserialization fails (empty or invalid file), do nothing
