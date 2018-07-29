using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using SmartParkingLiFi.Data.Model;

using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.Hardware.Camera2;
using Hoho.Android.UsbSerial.Driver;
using Hoho.Android.UsbSerial.Util;
using Plugin.CurrentActivity;
using System.Threading;
using System.Threading.Tasks;

using SkiaSharp;
using Microcharts;



namespace SmartParkingLiFi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabbedMainPage
    {
        HttpRequests Requests;
        Client ConnectedClient;
        private List<Reservation> Reservations;

        enum ReservationStatus { None, Validate, InProgress, Guest };
        ReservationStatus ClientReservationStatus = new ReservationStatus(); // Application status

        // LiFi Receiver
        private string ReceivedSerialData = "";
        IUsbSerialPort LiFiReceiverPort;
        SerialInputOutputManager SerialIOManager;
        Activity AppActivity = CrossCurrentActivity.Current.Activity;

        bool TimerEnabled = true;


        public TabbedMainPage(Client _ConnectedClient)
        {
            InitializeComponent();
            
            Requests = new HttpRequests(this, Children);
            ClientReservationStatus = new ReservationStatus();
            
            if (_ConnectedClient != null)
            {
                ConnectedClient = _ConnectedClient;
                Reservations = new List<Reservation>();
                FillProfil();
                BeginDatePicker.MinimumDate = DateTime.Now.AddMinutes(1); // Update possible date
                EndDatePicker.MinimumDate = DateTime.Now.AddMinutes(30);
                BeginTimePicker.Time = DateTime.Now.AddMinutes(30).TimeOfDay;
                EndTimePicker.Time = DateTime.Now.AddMinutes(90).TimeOfDay;
                AccessButton.Text = "Accès avec réservation";
            }
            else
            {
                ClientReservationStatus = ReservationStatus.Guest;
                CurrentPage = LiFiNavPage;
                this.Children.Remove(ReservationNavPage);
                this.Children.Remove(ProfilNavPage);
                AccessButton.Text = "Accès sans réservation";
            }

            MainAppThreadAsync();
            new Thread(MainThread).Start();
            //Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            //{
            //   BeginDatePicker.MaximumDate = DateTime.Now.AddDays(1);
            //   EndDatePicker.MaximumDate = DateTime.Now.AddDays(1);
            //   if (BeginDatePicker.Date > DateTime.Now.AddHours(1) && EndDatePicker.Date > DateTime.Now.AddHours(1))
            //   {
            //        BeginDatePicker.MinimumDate = DateTime.Now.AddHours(1);
            //        EndDatePicker.MinimumDate = DateTime.Now.AddHours(1);
            //    }
            //   if (TimerEnabled)
            //       MainAppThreadAsync();

            //   return true; // True = Repeat again, False = Stop the timer
            //});
        }



        // Main Thread de l'application pour la synchroniser avec le temps et les données de la BDD
        private async void MainThread()
        {
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    Thread.Sleep(900);
                    if (i == 0) { Thread.Sleep(500); }
                    AppActivity.RunOnUiThread(() =>
                    {
                        foreach (var Page in Children)
                        {
                            Page.ToolbarItems.Clear();
                            Page.ToolbarItems.Add(new ToolbarItem("AutoUpdateIndicator", "Process" + i.ToString() + ".png", () => { }));
                        }
                    });
                }
                Thread.Sleep(900);

                AppActivity.RunOnUiThread(() =>
                {
                    BeginDatePicker.MaximumDate = DateTime.Now.AddDays(1);
                    EndDatePicker.MaximumDate = DateTime.Now.AddDays(1);
                    if (BeginDatePicker.Date > DateTime.Now.AddMinutes(1) && EndDatePicker.Date > DateTime.Now.AddMinutes(30))
                    {
                        BeginDatePicker.MinimumDate = DateTime.Now.AddMinutes(1);
                        EndDatePicker.MinimumDate = DateTime.Now.AddMinutes(30);
                    }
                    if (TimerEnabled)
                        MainAppThreadAsync();
                });
            }
        }



        protected override bool OnBackButtonPressed() { return true; } // Disable Android Physical Back Button

        #region ProfilPage

        // Fill user informations in TableView profil page
        private void FillProfil()
        {
            NameSurnameTextCell.Detail = $"{ConnectedClient.Prenom} {ConnectedClient.Nom}";
            AddressTextCell.Detail = ConnectedClient.Adresse;
            CityPCTextCell.Detail = $"{ConnectedClient.Code_Postal} {ConnectedClient.Ville}";
            PhoneNumberTextCell.Detail = ConnectedClient.Num_Tel;
            EmailTextCell.Detail = ConnectedClient.Mail;
            int PasswordLength = ConnectedClient.Mot_de_Passe.Length;
            for (int i = 0; i < ConnectedClient.Mot_de_Passe.Length; i++) { PasswordTextCell.Detail += "•"; }
        }


        // Draw history chart : number of réservation by month
        private void DrawHistoryChart()
        {
            var HistoryChartEntries = new List<Microcharts.Entry>();

            int Max = 0;
            for (var i = 4; i >= 0; i--)
            {
                int NumberOfSeance = 0;

                var Month = DateTime.Now.AddMonths(-i).ToString("MMMM");

                foreach (var Reservation in Reservations)
                {
                    if (Reservation.Debut_Reservation.ToString("MMMM") == Month)
                        NumberOfSeance++;
                }

                Max = Math.Max(Max, NumberOfSeance);

                HistoryChartEntries.Add(new Microcharts.Entry(NumberOfSeance)
                {
                    Label = Month,
                    ValueLabel = NumberOfSeance.ToString(),
                    Color = SKColor.Parse("#FFFFFF"),
                    TextColor = SKColor.Parse("#FFFFFF")
                });
            }

            var HistoryChart = new BarChart()
            {
                Entries = HistoryChartEntries,
                BackgroundColor = SkiaSharp.Views.Forms.Extensions.ToSKColor(Graphics.MainColor),
                LabelColor = SKColor.Parse("#FFFFFF"),
                ValueLabelOrientation = Orientation.Horizontal,
                LabelOrientation = Orientation.Horizontal,
                IsAnimated = true,
                AnimationDuration = TimeSpan.FromMilliseconds(2000),
                PointMode = PointMode.Circle,
                LabelTextSize = 40,
                PointSize = 23,
                MinValue = 0,
                MaxValue = Max
            };

            HistoryChartView.Chart = HistoryChart;
        }


        // Disconnect current user, remove auto login informations and back to login page
        private async void DisconnectButton_ClickedAsync(object sender, EventArgs e)
        {
            Graphics.ButtonFadeAnimation(sender as Button);
            await Data.Settings.Write("null", "null", "null");
            if (Device.RuntimePlatform == Device.Android)
                await DisplayAlert("Attention", "[Android Emulator] Vous serez déconnecté et il faudra redémarrer l'application.", "OK");
            await Navigation.PopAsync(true);
        }


        // Conditions
        private void ConditionsButton_Clicked(object sender, EventArgs e)
        {
            Graphics.ButtonFadeAnimation(sender as Button);
            DisplayAlert("Conditions", "Application développée par Lancelot POULIN\nlancelotpoulin.com © 2018", "OK");
        }

        #endregion



        #region LiFiPage

        // Réception de données Li-Fi avec port série micro USB
        private async Task OpenLiFiReceiverPort()
        {
            UsbManager UsbSerialManager = AppActivity.ApplicationContext.GetSystemService(Context.UsbService) as UsbManager;

            var Table = UsbSerialProber.DefaultProbeTable;
            Table.AddProduct(0x1b4f, 0x0008, Java.Lang.Class.FromType(typeof(CdcAcmSerialDriver))); // IOIO OTG
            var Prober = new UsbSerialProber(Table);
            var Drivers = await Prober.FindAllDriversAsync(UsbSerialManager);

            LiFiReceiverPort = null;
            foreach (var Driver in Drivers) // On cherche notre driver (le récepteur Li-Fi)
            {
                foreach (var Port in Driver.Ports)
                {
                    if (HexDump.ToHexString((short)Port.Driver.Device.VendorId) == "0403" && HexDump.ToHexString((short)Port.Driver.Device.ProductId) == "6015")
                        LiFiReceiverPort = Port;
                }
            }

            if (LiFiReceiverPort == null) // Si il n'est pas branché on affiche un message
            {
                AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Récepteur Li-Fi absent"; });
            }
            else
            {
                var IsPermissionGranted = await UsbSerialManager.RequestPermissionAsync(LiFiReceiverPort.Driver.Device, AppActivity.ApplicationContext);
                if (IsPermissionGranted) // On demande la permission à l'utilisateur d'utiliser le récepteur (Android)
                {
                    SerialIOManager = new SerialInputOutputManager(LiFiReceiverPort) // Configuration du port série
                    {
                        BaudRate = 115200,
                        DataBits = 8,
                        StopBits = StopBits.One,
                        Parity = Parity.None
                    };

                    SerialIOManager.DataReceived += (source, args) => // Thread de réception de données
                    {
                        ReceivedSerialData = Encoding.UTF8.GetString(args.Data); // Données recu
                    };

                    SerialIOManager.ErrorReceived += (source, args) => // Thread si il y a une erreur
                    {
                        AppActivity.RunOnUiThread(() =>
                        {
                            ReceiverStatus.Text = "Récepteur Li-Fi absent"; // On affiche un message de débranchement
                            SerialIOManager.Close();
                        });
                    };

                    try
                    {
                        SerialIOManager.Open(UsbSerialManager); // On ouvre le port
                        AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Récepteur Li-Fi opérationnel"; });
                    }
                    catch (Java.IO.IOException Exception)
                    {
                        AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Erreur récepteur Li-Fi: " + Exception.Message; });
                    }
                }
                else
                {
                    AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Permission requise"; });
                }
            }
        }


        // Run the access method in Task (non-blocking UI mode)
        private void AccessButton_Clicked(object sender, EventArgs e)
        {
            Graphics.ButtonFadeAnimation(sender as Button);
            AccessButton.IsEnabled = false;
            TimerEnabled = false;
            Task.Run(() => { Access(); });
        }


        // Méthode d'accès au parking
        private async void Access()
        {
            bool IsReserved = false;
            if (ClientReservationStatus == ReservationStatus.Validate) // Si réservation validée
            {
                if (BeginDatePicker.Date.Add(BeginTimePicker.Time) > DateTime.Now.AddMinutes(30)) // Si en avance sur réservation
                {
                    AppActivity.RunOnUiThread(() => { DisplayAlert("Attention", "Vous avez plus de 30 minutes d'avance sur votre réservation, veuillez modifier votre réservation.", "OK"); AccessButton.IsEnabled = false; });
                    return;
                }
                else IsReserved = true;
            }

            await OpenLiFiReceiverPort(); // Check si Recepteur présent
            if (SerialIOManager != null && SerialIOManager.IsOpen)
            //if (true)
            {
                // Envoie reference réservation ou code invite
                AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Veillez a bien orienter le flash vers la borne."; });
                Thread.Sleep(1000);
                if (IsReserved)
                    await SendFlashLiFiData(Reference.Text);
                else await SendFlashLiFiData("1000"); // + durée 1001 1002 etc

                // Reception: Reference|Etage|Allee|Place
                AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Veillez a bien placer le récepteur en dessous de la lumière."; });

                int Tries = 100;
                while (Tries != 0)
                {
                    // ReceivedSerialData = "6698|1|B|1"; // Test
                    string[] SplittedData = ReceivedSerialData.Split('|');
                    if (SplittedData.Length == 4) // Si bonne réception
                    {
                        AppActivity.RunOnUiThread(() => {
                            ReferenceLabel.Text = SplittedData[0];
                            FloorLabel.Text = SplittedData[1];
                            LaneLabel.Text = SplittedData[2];
                            PlaceLabel.Text = SplittedData[3];
                            ReceiverStatus.Text = "Garez votre voiture à la place indiquée.";
                        });
                        Tries = 0; // Stop loop

                        if (ClientReservationStatus == ReservationStatus.Guest) 
                        {
                            await Data.Settings.Write("null", "null", SplittedData[0]); // Reference à sauvegardé dans config
                        }
                    }
                    else // Si mauvaise réception -> loop
                    {
                        Thread.Sleep(150);
                        AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Veillez a bien placer le récepteur en dessous de la lumière."; });
                        Tries--;
                        if (Tries == 0) { AppActivity.RunOnUiThread(() => { ReceiverStatus.Text = "Données Li-Fi non reçu, veuillez réessayez."; }); }
                    }
                }
                SerialIOManager.Close();
            }
            ReceivedSerialData = "";
            TimerEnabled = true;
        }



        // Envoie de données via le flash de la caméra de smartphone
        private async Task SendFlashLiFiData(string Data)
        {
            CameraManager CameraFlashManager = AppActivity.ApplicationContext.GetSystemService(Context.CameraService) as CameraManager;

            for (int i = 0; i < Data.Length; i++)
            {
                int NumberFlashDelay = Convert.ToInt32(Data.Substring(i, 1)) * 100; // 1 -> 100ms | 2 -> 200ms | ...
                if (NumberFlashDelay == 0) // 0 = 1000ms
                    NumberFlashDelay = 1000;

                CameraFlashManager.SetTorchMode(CameraFlashManager.GetCameraIdList()[0], true); // On allume le flash pendant le temps calculé
                Thread.Sleep(NumberFlashDelay);
                CameraFlashManager.SetTorchMode(CameraFlashManager.GetCameraIdList()[0], false);

                Thread.Sleep(50); // Delai entre chaque nombre envoyé
            }
        }


        #endregion



        #region ReservationPage


        // Valide une réservation
        private async void ValidateDeleteReservationButton_ClickedAsync(object sender, EventArgs e)
        {
            Graphics.ButtonFadeAnimation(ValidateDeleteReservation);
            ValidateDeleteReservation.IsEnabled = false;
            TimerEnabled = false;
            if ((ValidateDeleteReservation).Text == " Valider ") // Si pas de réservation en cours
            {
                // Check date possible
                if (BeginDatePicker.Date.Add(BeginTimePicker.Time).AddMinutes(25) > EndDatePicker.Date.Add(EndTimePicker.Time) || BeginDatePicker.Date.Add(BeginTimePicker.Time) < DateTime.Now.AddMinutes(-5))
                {
                    DisplayAlert("Attention", "Désolé, mais les dates sélectionnées sont impossibles.", "OK");
                    TimerEnabled = true; return;
                }

                // Check si il y a de la place entre ces heures
                int NbDispo = await Requests.CountDispoPlace(BeginDatePicker.Date.Add(BeginTimePicker.Time), EndDatePicker.Date.Add(EndTimePicker.Time));
                if (NbDispo == 0) { DisplayAlert("Attention", "Désolé, mais il n'y a plus de place à cette heure.", "OK"); TimerEnabled = true; return; }
                if (NbDispo == -1) { TimerEnabled = true; return; }

                var BeginDateTime = BeginDatePicker.Date.Add(BeginTimePicker.Time);
                var EndDateTime = EndDatePicker.Date.Add(EndTimePicker.Time);

                // Insertion réservation
                var Content = new StringContent(JsonConvert.SerializeObject(new Reservation()
                {
                    Client_ID = ConnectedClient.ID,
                    Debut_Reservation = new DateTime(BeginDateTime.Year, BeginDateTime.Month, BeginDateTime.Day, BeginDateTime.Hour, BeginDateTime.Minute, BeginDateTime.Second),
                    Fin_Reservation = new DateTime(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, EndDateTime.Hour, EndDateTime.Minute, EndDateTime.Second)
                }), Encoding.UTF8, "application/json");

                await Requests.InsertReserv(Content);
            }
            else // Si réservation en cours alors "ANNULER"
            {
                await Requests.DeleteReserv(Reference.Text);
            }
            TimerEnabled = true;
        }



        // Algorithme principale de l'application (timer threading)
        private async Task MainAppThreadAsync()
        {
            TimerEnabled = false;
            if (ClientReservationStatus == ReservationStatus.Guest) // Si invité
            {
                string Reference = (await Data.Settings.Read())[2];
                if (Reference != "null") // Si référence enregistrée
                {
                    string JsonDataGuest = await Requests.SelectReservInvite(Reference); // On récupère la réservation
                    if (JsonDataGuest == "")
                    {
                        await Data.Settings.Write("null", "null", "null");
                        TimerEnabled = true;
                        return;
                    }
                    JsonDataGuest = JsonDataGuest.Replace("null", $"\"1998-05-26 14:12:34\""); // On évite les champs null dans la déserialisation
                    JsonDataGuest = JsonDataGuest.Replace($"\"Place_ID\":\"1998-05-26 14:12:34\"", $"\"Place_ID\":0");
                    var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(JsonDataGuest)[0];

                    if (Reservation.Depart != new DateTime(1998, 5, 26, 14, 12, 34)) // Si réservation finie
                    {
                        AccessButton.IsEnabled = true; // Peut accéder
                        ReferenceLabel.Text = "-";
                        FloorLabel.Text = "-";
                        LaneLabel.Text = "-";
                        PlaceLabel.Text = "-";
                        RemainingTime.Text = "Pas de réservation en cours";

                        await Data.Settings.Write("null", "null", "null");
                    }
                    else
                    {
                        AccessButton.IsEnabled = false; // Ne peut pas re-acceder au parking 
                        if (Reservation.Fin_Reservation < DateTime.Now) // Si dépassement de + de 15 minutes = alerte
                        {
                            DisplayAlert("Attention", "Vous avez dépassé l'heure de fin de réservation. Vous devez sortir du parking.", "OK");
                            RemainingTime.Text = "Temps dépassé";
                        }
                        else
                        {
                            RemainingTime.Text = (Reservation.Fin_Reservation - DateTime.Now).Hours.ToString() + " heure(s) et " + (Reservation.Fin_Reservation - DateTime.Now).Minutes.ToString() + " minute(s) ";
                        }

                        var CurrentPlace = await Requests.SelectPlace(Reference); // On récupère la place où le client est garé
                        if (CurrentPlace != null)
                        {
                            ReferenceLabel.Text = Reference;
                            FloorLabel.Text = CurrentPlace.Etage.ToString();
                            LaneLabel.Text = CurrentPlace.Allee;
                            PlaceLabel.Text = CurrentPlace.Numero.ToString();
                        }
                    }
                }
                else
                {
                    AccessButton.IsEnabled = true; // Peut accéder
                    foreach (var Page in Children)
                    {
                        Page.ToolbarItems.Clear();
                        Page.ToolbarItems.Add(new ToolbarItem("AutoUpdateIndicator", "ProcessOK.png", () => { }));
                    }
                }
            }
            else
            {
                string JsonData = await Requests.SelectReserv(ConnectedClient.ID); // On récupère les réservations du client
                if (JsonData != "")
                {
                    if (JsonData == "No result found") // Pas de réservation
                    {
                        UpdateNoReservation(); // Refactorisation
                        TimerEnabled = true;
                        return;
                    }

                    JsonData = JsonData.Replace("null", $"\"1998-05-26 14:12:34\""); // On évite les champs null dans la déserialisation
                    JsonData = JsonData.Replace($"\"Place_ID\":\"1998-05-26 14:12:34\"", $"\"Place_ID\":0");
                    Reservations = JsonConvert.DeserializeObject<List<Reservation>>(JsonData);
                    Reservations.Sort((x, y) => x.Fin_Reservation.CompareTo(y.Fin_Reservation)); // On trie les réservations depuis plus récente
                    Reservations.Reverse();

                    // Reservations[0] est la dernière réservation effectuée
                    if (Reservations[0].Depart != new DateTime(1998, 5, 26, 14, 12, 34)) // Si pas de reservation validé
                    {
                        UpdateNoReservation(); // Refactorisation
                    }
                    else if (Reservations[0].Arrivee != new DateTime(1998, 5, 26, 14, 12, 34)) // Si reservation en cours (dans le parking)
                    {
                        ClientReservationStatus = ReservationStatus.InProgress; // Mis a jour du status de la réservation client
                        ValidateDeleteReservation.Text = "Annuler";
                        ValidateDeleteReservation.IsEnabled = false; // Ne peut pas annuler sa réservation
                        AccessButton.IsEnabled = false; // Ne peut pas re-acceder au parking
                        Reference.Text = Reservations[0].Reference;
                        UpdatePickers(Reservations[0].Debut_Reservation, Reservations[0].Fin_Reservation);
                        var CurrentPlace = await Requests.SelectPlace(Reservations[0].Reference); // On récupère la place où le client est garé
                        if (CurrentPlace != null)
                        {
                            ReferenceLabel.Text = Reservations[0].Reference;
                            FloorLabel.Text = CurrentPlace.Etage.ToString();
                            LaneLabel.Text = CurrentPlace.Allee;
                            PlaceLabel.Text = CurrentPlace.Numero.ToString();
                        }
                        if (Reservations[0].Fin_Reservation < DateTime.Now) // Si dépassement de + de 15 minutes = alerte
                        {
                            DisplayAlert("Attention", "Vous avez dépassé l'heure de fin de réservation. Vous devez sortir du parking.", "OK");
                            RemainingTime.Text = "Temps dépassé";
                        }
                        else
                        {
                            RemainingTime.Text = (Reservations[0].Fin_Reservation - DateTime.Now).Hours.ToString() + " heure(s) et " + (Reservations[0].Fin_Reservation - DateTime.Now).Minutes.ToString() + " minute(s) ";
                        }
                        Reservations.Remove(Reservations[0]); // On affichera pas la réservation en cours dans l'historique
                    }
                    else // Si reservation valider (hors parking)
                    {
                        ClientReservationStatus = ReservationStatus.Validate; // Mis a jour du status de la réservation client
                        ValidateDeleteReservation.Text = "Annuler";
                        ValidateDeleteReservation.IsEnabled = true; // Peut annuler sa réservation
                        AccessButton.IsEnabled = true; // Peut acceder
                        Reference.Text = Reservations[0].Reference;
                        RemainingTime.Text = "Pas de réservation en cours";
                        UpdatePickers(Reservations[0].Debut_Reservation, Reservations[0].Fin_Reservation);
                        if (Reservations[0].Debut_Reservation.AddMinutes(15) < DateTime.Now) // Si dépassement de + de 15 minutes = annule
                        {
                            DisplayAlert("Attention", "Vous avez dépassé l'heure de début de réservation. La réservation est annulé.", "OK");
                            if (await Requests.DeleteReserv(Reference.Text))
                            {
                                Reference.Text = "-";
                                ClientReservationStatus = ReservationStatus.None;
                                ValidateDeleteReservation.Text = " Valider ";
                            }
                        }
                        Reservations.Remove(Reservations[0]);// On affichera pas la réservation en cours dans l'historique
                    }

                    // Affichage de l'historique des réservations de l'utilisateur avec tri par mois
                    var ReservationList = new List<ReservationCellGroup>();
                    var CurrentEventCellGroup = new ReservationCellGroup() { Month = "Aucune réservation" };
                    foreach (var Reservation in Reservations)
                    {
                        if (Reservation.Fin_Reservation.Date.ToString("MMMM yyyy") != CurrentEventCellGroup.Month)
                        {
                            if (CurrentEventCellGroup.Count != 0) { ReservationList.Add(CurrentEventCellGroup); }
                            CurrentEventCellGroup = new ReservationCellGroup() { Month = Reservation.Fin_Reservation.Date.ToString("MMMM yyyy") };
                        }

                        CurrentEventCellGroup.Add(new ReservationCell()
                        {
                            ID = Reservation.ID,
                            Date = "du " + Reservation.Debut_Reservation.ToString("ddd d à HH:mm") + " au " + Reservation.Fin_Reservation.ToString("ddd d à HH:mm"),
                            Duration = "Durée: " + (Reservation.Fin_Reservation - Reservation.Debut_Reservation).ToString(@"hh\:mm"),
                            Reference = $"Reference {Reservation.Reference}"
                        });
                    }
                    ReservationList.Add(CurrentEventCellGroup);
                    ReservationHistoryListView.ItemsSource = ReservationList;
                }
            }
            TimerEnabled = true;
        }


        // Refactorisation MainAppThreadAsync() lorsqu'il n'y a pas de réservation
        private void UpdateNoReservation()
        {
            ClientReservationStatus = ReservationStatus.None; // Mis a jour du status de la réservation client
            ValidateDeleteReservation.Text = " Valider ";
            ValidateDeleteReservation.IsEnabled = true; // Il peut valider une nouvelle réservation
            AccessButton.IsEnabled = false; // Ne peut pas acceder
            Reference.Text = "-";
            ReferenceLabel.Text = "-";
            FloorLabel.Text = "-";
            LaneLabel.Text = "-";
            PlaceLabel.Text = "-";
            RemainingTime.Text = "Pas de réservation validée";
        }


        // Maj des pickers avec la date de la réservation validée (Refactorisation MainAppThreadAsync())
        private void UpdatePickers(DateTime BeginDate, DateTime EndDate)
        {
            BeginDatePicker.Date = BeginDate.Date;
            BeginTimePicker.Time = BeginDate.TimeOfDay;
            EndDatePicker.Date = EndDate.Date;
            EndTimePicker.Time = EndDate.TimeOfDay;
        }


        // // Informations supplémentaires d'une ancienne réservation 
        private void HistoryListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Informations supplémentaires
        }


        #endregion



        //  
        private void ProfilPage_Appearing(object sender, EventArgs e)
        {
            DrawHistoryChart();
        }


        // 
        private void LiFiPage_Appearing(object sender, EventArgs e)
        {

        }


        // 
        private void ReservationPage_Appearing(object sender, EventArgs e)
        {

        }

    }
}