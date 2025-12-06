namespace LibrarySystemAvalonia

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml
open LibrarySystem.Models
open LibrarySystem.Services
open LibrarySystem.Storage

// Helper type to make the Grid display "Available" instead of fancy types
type UIBook = { Id: int; Title: string; Author: string; Year: int; Status: string }

type MainWindow() as this =
    inherit Window()

    // Define mutable variables to hold the controls
    let mutable booksGrid: DataGrid = null
    let mutable txtSearch: TextBox = null
    let mutable inputId: TextBox = null
    let mutable inputTitle: TextBox = null
    let mutable inputAuthor: TextBox = null
    let mutable inputYear: TextBox = null

    do
        this.InitializeComponent()
        this.FindControls()
        this.SetupEvents()
        
        // 1. Try to load existing data from file first
        FileStorage.loadLibrary()

        // 2. If data is still empty, Create Sample Books
        if List.isEmpty (LibraryCrud.listBooks()) then
            LibraryCrud.addBook (Book.createBook 1 "1984" "George Orwell" 1949) |> ignore
            LibraryCrud.addBook (Book.createBook 2 "Brave New World" "Aldous Huxley" 1932) |> ignore
            LibraryCrud.addBook (Book.createBook 3 "Animal Farm" "George Orwell" 1945) |> ignore
            LibraryCrud.addBook (Book.createBook 4 "The Great Gatsby" "F. Scott Fitzgerald" 1925) |> ignore
            
            // Save them immediately so they stick
            FileStorage.saveLibrary()

        // 3. Show data on screen
        this.RefreshGrid(LibraryCrud.listBooks())

    member private this.FindControls() =
        booksGrid <- this.FindControl<DataGrid>("BooksDataGrid")
        txtSearch <- this.FindControl<TextBox>("TxtSearch")
        inputId <- this.FindControl<TextBox>("InputId")
        inputTitle <- this.FindControl<TextBox>("InputTitle")
        inputAuthor <- this.FindControl<TextBox>("InputAuthor")
        inputYear <- this.FindControl<TextBox>("InputYear")

    member private this.RefreshGrid(books: Book.Book list) =
        if booksGrid <> null then
            let uiBooks = 
                books 
                |> List.map (fun b -> 
                    { 
                        Id = b.Id
                        Title = b.Title
                        Author = b.Author
                        Year = b.Year
                        Status = match b.Status with Book.Available -> "Available" | Book.Borrowed -> "Borrowed"
                    })
            booksGrid.ItemsSource <- uiBooks

    member private this.SetupEvents() =
        // --- SEARCH ---
        let btnSearch = this.FindControl<Button>("BtnSearch")
        let btnReset = this.FindControl<Button>("BtnReset")

        btnSearch.Click.Add(fun _ -> 
            let query = txtSearch.Text
            if not (String.IsNullOrWhiteSpace(query)) then
                let results = Search.searchByTitle query
                this.RefreshGrid(results)
        )

        btnReset.Click.Add(fun _ -> 
            this.RefreshGrid(LibraryCrud.listBooks())
            txtSearch.Text <- ""
        )

        // --- BORROW / RETURN ---
        let btnBorrow = this.FindControl<Button>("BtnBorrow")
        let btnReturn = this.FindControl<Button>("BtnReturn")

        let getSelectedId () =
            if booksGrid.SelectedItem <> null then
                let selected = booksGrid.SelectedItem :?> UIBook
                Some selected.Id
            else
                None

        btnBorrow.Click.Add(fun _ -> 
            match getSelectedId() with
            | Some id -> 
                BorrowReturn.borrowBook id |> ignore
                this.RefreshGrid(LibraryCrud.listBooks())
            | None -> ()
        )

        btnReturn.Click.Add(fun _ -> 
            match getSelectedId() with
            | Some id -> 
                BorrowReturn.returnBook id |> ignore
                this.RefreshGrid(LibraryCrud.listBooks())
            | None -> ()
        )

        // --- ADD BOOK ---
        let btnAdd = this.FindControl<Button>("BtnAdd")
        
        btnAdd.Click.Add(fun _ ->
            let successId, id = Int32.TryParse(inputId.Text)
            let successYear, year = Int32.TryParse(inputYear.Text)
            
            if successId && successYear then
                let newBook = Book.createBook id inputTitle.Text inputAuthor.Text year
                LibraryCrud.addBook newBook |> ignore
                
                // Clear inputs
                inputId.Text <- ""
                inputTitle.Text <- ""
                inputAuthor.Text <- ""
                inputYear.Text <- ""
                
                this.RefreshGrid(LibraryCrud.listBooks())
        )

        // --- SAVE / LOAD ---
        let btnSave = this.FindControl<Button>("BtnSave")
        let btnLoad = this.FindControl<Button>("BtnLoad")

        btnSave.Click.Add(fun _ -> FileStorage.saveLibrary())
        
        btnLoad.Click.Add(fun _ -> 
            FileStorage.loadLibrary()
            this.RefreshGrid(LibraryCrud.listBooks())
        )

    member this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)