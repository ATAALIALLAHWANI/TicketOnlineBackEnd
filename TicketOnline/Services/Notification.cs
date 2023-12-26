namespace TicketOnline.Services
{
    public class Notification : ServiceNot
    {
        public  void Alert (String Message)
        {
            this.notifyServices(Message);
           
        }
    }
}
