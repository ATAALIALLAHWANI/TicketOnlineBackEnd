using System.ComponentModel.DataAnnotations;

namespace TicketOnline.Models
{
    public class Passenger
    {
       
        public int IdPassenger { get; set; }
        public string PassengerName { get; set; }
        public string PassengerPhone { get; set; } 
        public string PassengerEmail { get; set; }
        public string PassengerPassword { get; set; }

        public int Blocked { get; set; } = 0;


    }
}
