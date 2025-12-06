namespace LibrarySystemAvalonia

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Layout
open Avalonia.Media

// A simple dialog window to ask for the Borrower's Name
type BorrowDialog() as this =
    inherit Window()

    // Controls
    let txtName = TextBox(Watermark = "Enter Borrower Name")
    let btnConfirm = Button(Content = "Confirm Borrow", HorizontalAlignment = HorizontalAlignment.Right)
    let btnCancel = Button(Content = "Cancel")
    let lblError = TextBlock(Foreground = Brushes.Red, Margin = Thickness(0.0, 5.0, 0.0, 0.0))

    do
        // Window properties
        this.Title <- "Borrower Authentication"
        this.Width <- 400.0
        this.Height <- 200.0
        this.WindowStartupLocation <- WindowStartupLocation.CenterOwner
        this.CanResize <- false

        // Layout
        let mainPanel = StackPanel(Margin = Thickness(20.0), Spacing = 15.0)
        
        mainPanel.Children.Add(TextBlock(Text = "Please enter the borrower's name to authenticate:", FontWeight = FontWeight.Bold))
        mainPanel.Children.Add(txtName)
        mainPanel.Children.Add(lblError)

        let buttonPanel = StackPanel(Orientation = Orientation.Horizontal, Spacing = 10.0, HorizontalAlignment = HorizontalAlignment.Right)
        buttonPanel.Children.Add(btnCancel)
        buttonPanel.Children.Add(btnConfirm)
        
        mainPanel.Children.Add(buttonPanel)
        this.Content <- mainPanel

        // Events
        btnConfirm.Click.Add(fun _ -> 
            if String.IsNullOrWhiteSpace(txtName.Text) then
                lblError.Text <- "Name cannot be empty."
            else
                // Return the typed name as the result
                this.Close(txtName.Text)
        )

        btnCancel.Click.Add(fun _ -> 
            // Return null if cancelled
            this.Close(null)
        )