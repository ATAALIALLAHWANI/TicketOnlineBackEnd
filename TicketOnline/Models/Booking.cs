﻿namespace TicketOnline.Models
{
    public class Booking
    {
        public int IdBooking { get; set; }
        
        public string DateBook { get; set; } 

        public string StatusBooking { get; set; } 

        public double TickitPrice { get; set; }

        public int SeatsBooking { get; set; }

        public string PhonePassenger { get; set; }
        
        public Journey JourneyoBo { get; set; } 

        

    }
}
