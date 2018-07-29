using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Newtonsoft.Json;

using SmartParkingLiFi.Data.Model;


// Entry page of the application, user login in or open sign in page

namespace SmartParkingLiFi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LogInPage : ContentPage
    {
        HttpRequests Requests;
        private bool IsAlreadyClicked = false; // Anti Spam Button bug
        private bool IsBGRunning = true; // For stopping/replay background (performance)

        public LogInPage()
        {
            InitializeComponent();

            Requests = new HttpRequests(this, null);

            AutoLogIn();
        }

        private void ContentPage_Appearing(object sender, EventArgs e) { IsBGRunning = true; } // ReRun BG Video when disconnect

        // Try connecting with saved settings at launching
        private async void AutoLogIn()
        {
            string[] Content = await Data.Settings.Read(); // Read Settings file to get all entries
            if (Content.Length != 3) { return; }
            if (Content[0] != "null" && Content[1] != "null") // If there are entries
            {
                await LogIn(Content[0], Content[1]);
            }
            else if (Content[2] != "null")
            {
                IsBGRunning = false; // Stop BG Video for performance
                await Navigation.PushAsync(new TabbedMainPage(null)); //  Push new page
            }
        }


        // Try connect webservice and ask for user infos : write new settings file + push event page if login in success
        private async Task LogIn(string Mail, string Password)
        {
            var Client = await Requests.LogIn(Mail, Password);
            if (Client != null)
            {
                IsBGRunning = false; // Stop BG Video for performance
                await Data.Settings.Write(Mail, Password, "null"); // Write new entries in Settings for futur (auto login)
                await Navigation.PushAsync(new TabbedMainPage(Client)); //  Push new page
                MailEntry.Text = ""; PasswordEntry.Text = "";
            }
        }


        // Log in button click event
        private async void LogInButton_Clicked(object sender, EventArgs e)
        {
            if (!IsAlreadyClicked)
            {
                IsAlreadyClicked = true;
                Graphics.ButtonFadeAnimation(sender as Button);
                await LogIn(MailEntry.Text, PasswordEntry.Text);
            }
            IsAlreadyClicked = false;
        }


        // 
        private async void LogInAsGuestButton_Clicked(object sender, EventArgs e)
        {
            Graphics.ButtonFadeAnimation(sender as Button);

            // Ask for a possible reference ?

            IsBGRunning = false; // Stop BG Video for performance
            await Navigation.PushAsync(new TabbedMainPage(null)); //  Push new page
        }
    }
}