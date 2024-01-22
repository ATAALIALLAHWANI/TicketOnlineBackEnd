namespace TicketOnline.Models
{
    public class PassengerQrCode
    {
        public int Id { get; set; }
        public string PassengerPhone { get; set; }
        public List<string> PassQrCode { get; set; }
    }
}
