namespace LibrarySystemAvalonia

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media
open Avalonia.Markup.Xaml
open LibrarySystem.Models
open LibrarySystem.Services
open LibrarySystem.Storage

// ---------------------------------------------------------
// 1. VIEW MODELS & DIALOGS
// ---------------------------------------------------------

// View Model for the Grid
type GroupedBookVM = { 
    Title: string
    Author: string
    Year: int
    Quantity: int
    StatusDisplay: string
    Borrowers: (string * string) list // Name, Phone
}

// ---------------- DIALOGS -----------------
type MessageBox(title: string, message: string) as this =
    inherit Window()
    do
        this.Title <- title
        this.Width <- 300.0; this.SizeToContent <- SizeToContent.Height
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        let s = StackPanel(Margin=Thickness(20.0), Spacing=10.0)
        s.Children.Add(TextBlock(Text=message, TextWrapping=TextWrapping.Wrap))
        let b = Button(Content="OK", HorizontalAlignment=HorizontalAlignment.Center)
        b.Click.Add(fun _ -> this.Close())
        s.Children.Add(b)
        this.Content <- s

// Simple Yes/No Confirmation Dialog
type ConfirmDialog(message: string) as this =
    inherit Window()
    let btnYes = Button(Content="Yes", HorizontalAlignment=HorizontalAlignment.Center, Background=Brushes.LightGreen)
    let btnNo = Button(Content="No", HorizontalAlignment=HorizontalAlignment.Center, Background=Brushes.LightSalmon)
    
    do
        this.Title <- "Confirmation"
        this.Width <- 300.0
        this.SizeToContent <- SizeToContent.Height
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        
        let s = StackPanel(Margin=Thickness(20.0), Spacing=15.0)
        s.Children.Add(TextBlock(Text=message, TextWrapping=TextWrapping.Wrap, HorizontalAlignment=HorizontalAlignment.Center))
        
        let buttonPanel = StackPanel(Orientation=Orientation.Horizontal, Spacing=20.0, HorizontalAlignment=HorizontalAlignment.Center)
        buttonPanel.Children.Add(btnYes)
        buttonPanel.Children.Add(btnNo)
        s.Children.Add(buttonPanel)
        
        this.Content <- s

        // Return true if Yes, false if No
        btnYes.Click.Add(fun _ -> this.Close(true))
        btnNo.Click.Add(fun _ -> this.Close(false))

// UPDATED: Now handles validation for blank quantity
type AddExistingDialog(titles: string list) as this =
    inherit Window()
    let cmb = ComboBox(Width=200.0, ItemsSource=titles, PlaceholderText="Select a Book...")
    let txt = TextBox(Watermark="Qty", Width=100.0)
    let btn = Button(Content="Add", HorizontalAlignment=HorizontalAlignment.Right)
    // NEW: Error Label
    let err = TextBlock(Foreground=Brushes.Red, FontWeight=FontWeight.Bold, TextWrapping=TextWrapping.Wrap)

    do
        this.Title <- "Add Copies"
        this.Width <- 300.0; this.SizeToContent <- SizeToContent.Height
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        let s = StackPanel(Margin=Thickness(20.0), Spacing=15.0)
        s.Children.Add(TextBlock(Text="Select Book:"))
        s.Children.Add(cmb)
        s.Children.Add(TextBlock(Text="Quantity:"))
        s.Children.Add(txt)
        // Add error label to layout
        s.Children.Add(err)
        s.Children.Add(btn)
        this.Content <- s
        
        btn.Click.Add(fun _ -> 
            // Reset error
            err.Text <- ""

            if cmb.SelectedItem = null then
                err.Text <- "Error: Please select a book."
            else if String.IsNullOrWhiteSpace(txt.Text) then
                err.Text <- "Error: Quantity cannot be blank."
            else
                let ok, q = Int32.TryParse(txt.Text)
                if ok && q > 0 then 
                    this.Close((cmb.SelectedItem.ToString(), q))
                else 
                    err.Text <- "Error: Invalid Quantity (Must be a number > 0)."
        )

type BorrowInfoDialog() as this =
    inherit Window()
    let tName = TextBox(Watermark="Name", Width=200.0)
    let tPhone = TextBox(Watermark="Phone", Width=200.0)
    let btn = Button(Content="Borrow", HorizontalAlignment=HorizontalAlignment.Right)
    let err = TextBlock(Foreground=Brushes.Red, FontWeight=FontWeight.Bold, TextWrapping=TextWrapping.Wrap)
    do
        this.Title <- "Borrow"
        this.Width <- 300.0; this.SizeToContent <- SizeToContent.Height
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        let s = StackPanel(Margin=Thickness(20.0), Spacing=15.0)
        s.Children.Add(TextBlock(Text="Name:"))
        s.Children.Add(tName)
        s.Children.Add(TextBlock(Text="Phone:"))
        s.Children.Add(tPhone)
        s.Children.Add(err)
        s.Children.Add(btn)
        this.Content <- s
        btn.Click.Add(fun _ -> 
            err.Text <- ""
            if String.IsNullOrWhiteSpace(tName.Text) then err.Text <- "Name Required!"
            else if String.IsNullOrWhiteSpace(tPhone.Text) then err.Text <- "Phone Required!"
            else this.Close((tName.Text, tPhone.Text)))

type ReturnDialog(borrowers: (string * string) list) as this =
    inherit Window()
    let list = borrowers |> List.map (fun (n,p) -> $"{n} (Phone: {p})")
    let cmb = ComboBox(Width=250.0, ItemsSource=list, PlaceholderText="Select Borrower...")
    let btn = Button(Content="Return", HorizontalAlignment=HorizontalAlignment.Right)
    do
        this.Title <- "Return"
        this.Width <- 350.0; this.SizeToContent <- SizeToContent.Height
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        let s = StackPanel(Margin=Thickness(20.0), Spacing=15.0)
        s.Children.Add(TextBlock(Text="Who is returning?"))
        s.Children.Add(cmb); s.Children.Add(btn)
        this.Content <- s
        if list.Length > 0 then cmb.SelectedIndex <- 0
        btn.Click.Add(fun _ -> 
            if cmb.SelectedIndex >= 0 && cmb.SelectedIndex < borrowers.Length then
                let (_, p) = borrowers.[cmb.SelectedIndex]
                this.Close(p)
            else this.Close(null))

// ---------------- MAIN WINDOW -----------------
type MainWindow() as this =
    inherit Window()

    let mutable grid: DataGrid = null
    let mutable inputTitle: TextBox = null
    let mutable inputAuthor: TextBox = null
    let mutable inputYear: TextBox = null
    let mutable inputQty: TextBox = null 
    let mutable btnAddCopies: Button = null 

    let showError msg = (MessageBox("Error", msg)).ShowDialog(this) |> ignore

    do
        this.InitializeComponent()
        this.FindControls()
        this.SetupEvents()
        FileStorage.loadLibrary()
        this.RefreshGrid()

    member private this.FindControls() =
        grid <- this.FindControl<DataGrid>("BooksDataGrid")
        // We map "InputId" from XAML to our Quantity logic
        inputQty <- this.FindControl<TextBox>("InputId") 
        if inputQty <> null then inputQty.Watermark <- "Qty"

        inputTitle <- this.FindControl<TextBox>("InputTitle")
        inputAuthor <- this.FindControl<TextBox>("InputAuthor")
        inputYear <- this.FindControl<TextBox>("InputYear")
        btnAddCopies <- this.FindControl<Button>("BtnAddCopies")

    member private this.RefreshGrid() =
        if grid <> null then
            let rawData = LibraryCrud.getBooksForGrid()
            let vms = rawData |> List.map (fun (t, a, y, tot, avail, borr, list) -> 
                { 
                    Title = t
                    Author = a
                    Year = y
                    Quantity = tot
                    StatusDisplay = $"Available: {avail}, Borrowed: {borr}"
                    Borrowers = list 
                })
            grid.ItemsSource <- vms

    member private this.SetupEvents() =
        // ADD NEW BOOK
        let btnAdd = this.FindControl<Button>("BtnAdd")
        btnAdd.Click.Add(fun _ ->
            let okQty, qty = Int32.TryParse(inputQty.Text)
            let okYear, year = Int32.TryParse(inputYear.Text)
            
            if okQty && okYear && qty > 0 then
                match LibraryCrud.addBook inputTitle.Text inputAuthor.Text year qty with
                | Ok _ -> 
                    inputQty.Text <- ""
                    inputTitle.Text <- ""; inputAuthor.Text <- ""
                    this.RefreshGrid()
                | Error e -> showError (Book.ErrorHelpers.getMessage e)
            else showError "Invalid Year or Quantity (Must be > 0)"
        )

        // ADD EXISTING
        if btnAddCopies <> null then
            btnAddCopies.Click.Add(fun _ ->
                async {
                    let d = AddExistingDialog(LibraryCrud.getBookTitles())
                    let! res = d.ShowDialog<(string*int)>(this) |> Async.AwaitTask
                    try 
                        let (t, q) = res
                        if not(String.IsNullOrEmpty(t)) then 
                             match LibraryCrud.addCopies t q with
                             | Ok _ -> this.RefreshGrid()
                             | Error e -> showError (Book.ErrorHelpers.getMessage e)
                    with _ -> ()
                } |> Async.StartImmediate)

        // BORROW
        let btnBorrow = this.FindControl<Button>("BtnBorrow")
        btnBorrow.Click.Add(fun _ ->
            if grid.SelectedItem <> null then
                let vm = grid.SelectedItem :?> GroupedBookVM
                async {
                    let d = BorrowInfoDialog()
                    let! res = d.ShowDialog<(string*string)>(this) |> Async.AwaitTask
                    try
                        let (n, p) = res
                        if not(String.IsNullOrEmpty(n)) then
                             match BorrowReturn.borrowBook vm.Title n p with
                             | Ok _ -> this.RefreshGrid()
                             | Error e -> showError (Book.ErrorHelpers.getMessage e)
                    with _ -> ()
                } |> Async.StartImmediate)

        // RETURN
        let btnReturn = this.FindControl<Button>("BtnReturn")
        btnReturn.Click.Add(fun _ ->
            if grid.SelectedItem <> null then
                let vm = grid.SelectedItem :?> GroupedBookVM
                if vm.Borrowers.Length = 0 then showError "No borrowers for this book."
                else
                    async {
                        let d = ReturnDialog(vm.Borrowers)
                        let! p = d.ShowDialog<string>(this) |> Async.AwaitTask
                        if not(String.IsNullOrEmpty(p)) then
                             match BorrowReturn.returnBook vm.Title p with
                             | Ok _ -> this.RefreshGrid()
                             | Error e -> showError (Book.ErrorHelpers.getMessage e)
                    } |> Async.StartImmediate)

        // DELETE COPY
        let btnDeleteCopy = this.FindControl<Button>("BtnDeleteCopy")
        btnDeleteCopy.Click.Add(fun _ ->
            // Ensure a book is selected
            if grid.SelectedItem <> null then
                let vm = grid.SelectedItem :?> GroupedBookVM
                
                async {
                    // 1. Ask for confirmation
                    let dialog = ConfirmDialog($"Are you sure you want to delete 1 copy of '{vm.Title}'?")
                    let! isConfirmed = dialog.ShowDialog<bool>(this) |> Async.AwaitTask
                    
                    if isConfirmed then
                        // 2. Call the backend
                        match LibraryCrud.removeCopy vm.Title with
                        | Ok _ -> 
                            // Success: Refresh to show new quantity
                            this.RefreshGrid() 
                        | Error e -> 
                            // Failure: Show error (e.g., if book is borrowed)
                            showError (Book.ErrorHelpers.getMessage e)
                } |> Async.StartImmediate
            else
                showError "Please select a book first."
        )

        // Save/Load
        let btnSave = this.FindControl<Button>("BtnSave")
        btnSave.Click.Add(fun _ -> FileStorage.saveLibrary())
        let btnLoad = this.FindControl<Button>("BtnLoad")
        btnLoad.Click.Add(fun _ -> FileStorage.loadLibrary(); this.RefreshGrid())
        
        // Search
        let btnSearch = this.FindControl<Button>("BtnSearch")
        let txtSearch = this.FindControl<TextBox>("TxtSearch")
        let btnReset = this.FindControl<Button>("BtnReset")
        btnSearch.Click.Add(fun _ -> 
             if not(String.IsNullOrEmpty(txtSearch.Text)) then
                let raw = LibraryCrud.getBooksForGrid()
                let filtered = raw |> List.filter (fun (t,_,_,_,_,_,_) -> t.ToLower().Contains(txtSearch.Text.ToLower()))
                grid.ItemsSource <- filtered |> List.map (fun (t, a, y, tot, avail, borr, list) -> 
                    { 
                        Title = t
                        Author = a
                        Year = y
                        Quantity = tot
                        StatusDisplay = $"Available: {avail}, Borrowed: {borr}"
                        Borrowers = list
                    })
        )
        btnReset.Click.Add(fun _ -> txtSearch.Text <- ""; this.RefreshGrid())

    member this.InitializeComponent() = AvaloniaXamlLoader.Load(this)