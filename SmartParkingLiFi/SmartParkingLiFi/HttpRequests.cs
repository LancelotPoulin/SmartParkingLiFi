using Newtonsoft.Json;
using SmartParkingLiFi.Data.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SmartParkingLiFi
{
    public class HttpRequests
    {
        Page RequesterPage;
        IList<Page> Children;
        HttpClient Client;
        public const string ServerAddress = "projetlifi.alwaysdata.net";
        

        public HttpRequests(Page _Page, IList<Page> _Children)
        {
            RequesterPage = _Page;
            Children = _Children;
            Client = new HttpClient();
        }



        public async Task<bool> DeleteReserv(string Reference)
        {
            try
            {
                HttpResponseMessage Response = await Client.GetAsync($"http://{ServerAddress}/DeleteReserv.php?Reference={Reference}");
                if (Response.IsSuccessStatusCode)
                {
                    return true; // Success
                }
                else { RequesterPage.DisplayAlert("Erreur", "Impossible d'annuler la réservation. [DR]", "OK"); }
            }
            catch { RequesterPage.DisplayAlert("Erreur", "Aucune réponse du serveur. Veuillez réessayer ultérieurement. [DR]", "OK"); }
            return false;
        }



        public async Task InsertReserv(StringContent Content)
        {
            try
            {
                HttpResponseMessage Response = await Client.PostAsync($"http://{ServerAddress}/InsertReserv.php", Content);
                if (Response.IsSuccessStatusCode)
                {
                    return; // Success
                }
                else { RequesterPage.DisplayAlert("Erreur", "Impossible de valider la réservation, veuillez réessayer ultérieurement. [IR]", "OK"); }
            }
            catch { RequesterPage.DisplayAlert("Erreur", "Aucune réponse du serveur. Veuillez réessayer ultérieurement. [IR]", "OK"); }
            return;
        }


        
        public async Task<string> SelectReserv(int ConnectedClientID)
        {
            try
            {
                HttpResponseMessage Response = await Client.GetAsync($"http://{ServerAddress}/SelectReserv.php?Client_ID={ConnectedClientID}");
                if (Response.IsSuccessStatusCode)
                {
                    foreach (var Page in Children)
                    {
                        Page.ToolbarItems.Clear();
                        Page.ToolbarItems.Add(new ToolbarItem("AutoUpdateIndicator", "ProcessOK.png", () => { }));
                    }
                    return await Response.Content.ReadAsStringAsync();
                }
                else { RequesterPage.DisplayAlert("Erreur", "Impossible de charger les réservations, veuillez réessayer ultérieurement. [SR]", "OK"); }
            }
            catch
            {
                foreach (var Page in Children)
                {
                    Page.ToolbarItems.Clear();
                    Page.ToolbarItems.Add(new ToolbarItem("AutoUpdateIndicator", "ProcessError.png", () => { }));
                }
            }
            return "";
        }



        public async Task<string> SelectReservInvite(string Reference)
        {
            try
            {
                HttpResponseMessage Response = await Client.GetAsync($"http://{ServerAddress}/SelectReservInvite.php?Reference={Reference}");
                if (Response.IsSuccessStatusCode)
                {
                    foreach (var Page in Children)
                    {
                        Page.ToolbarItems.Clear();
                        Page.ToolbarItems.Add(new ToolbarItem("AutoUpdateIndicator", "ProcessOK.png", () => { }));
                    }
                    return await Response.Content.ReadAsStringAsync();
                }
                else { RequesterPage.DisplayAlert("Erreur", "Impossible de récupérer les informations de réservation, veuillez réessayer ultérieurement. [SRI]", "OK"); }
            }
            catch
            {
                foreach (var Page in Children)
                {
                    Page.ToolbarItems.Clear();
                    Page.ToolbarItems.Add(new ToolbarItem("AutoUpdateIndicator", "ProcessError.png", () => { }));
                }
            }
            return "";
        }



        public async Task<Client> LogIn(string Mail, string Password)
        {
            try
            {
                HttpResponseMessage Response = await Client.GetAsync($"http://{ServerAddress}/LogIn.php?Mail={Mail}&Mot_de_Passe={Password}");
                if (Response.IsSuccessStatusCode)
                {
                    string JsonData = await Response.Content.ReadAsStringAsync();
                    if (JsonData != "Aucun résultat")
                        return JsonConvert.DeserializeObject<List<Client>>(JsonData)[0];
                }
                else { RequesterPage.DisplayAlert("Erreur", "Mail ou mot de passe incorrect, veuillez réessayer.  [LI]", "OK"); }
            }
            catch { RequesterPage.DisplayAlert("Erreur", "Aucune réponse du serveur. Veuillez réessayer ultérieurement. [LI]", "OK"); }
            return null;
        }



        public async Task<int> CountDispoPlace(DateTime Debut_Reservation, DateTime Fin_Reservation)
        {
            try
            {
                HttpResponseMessage Response = await Client.GetAsync($"http://{ServerAddress}/CountDispoPlace.php?Debut_Reservation={Debut_Reservation}&Fin_Reservation={Fin_Reservation}");
                if (Response.IsSuccessStatusCode)
                {
                    return Convert.ToInt32(await Response.Content.ReadAsStringAsync());
                }
                else { RequesterPage.DisplayAlert("Erreur", "Impossible de valider la réservation, veuillez réessayer ultérieurement. [CDP]", "OK"); }
            }
            catch { RequesterPage.DisplayAlert("Erreur", "Aucune réponse du serveur. Veuillez réessayer ultérieurement. [CDP]", "OK"); }
            return -1;
        }



        public async Task<Place> SelectPlace(string Reference)
        {
            try
            {
                HttpResponseMessage Response = await Client.GetAsync($"http://{ServerAddress}/SelectPlace.php?Reference={Reference}");
                if (Response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Place>>(await Response.Content.ReadAsStringAsync())[0];
                }
                else { RequesterPage.DisplayAlert("Erreur", "Impossible de récupérer les informations de réservation, veuillez réessayer ultérieurement. [SP]", "OK"); }
            }
            catch { RequesterPage.DisplayAlert("Erreur", "Aucune réponse du serveur. Veuillez réessayer ultérieurement. [SP]", "OK"); }
            return null;
        }
    }
}
