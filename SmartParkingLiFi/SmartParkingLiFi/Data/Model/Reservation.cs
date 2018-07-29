// Reservation database model

using System;

namespace SmartParkingLiFi.Data.Model
{
    public class Reservation
    {
        public int ID { get; set; }
        public string Reference { get; set; }
        public DateTime Debut_Reservation { get; set; }
        public DateTime Fin_Reservation { get; set; }
        public DateTime Arrivee { get; set; }
        public DateTime Depart { get; set; }
        public int Place_ID { get; set; }
        public int Client_ID { get; set; }
    }
}
