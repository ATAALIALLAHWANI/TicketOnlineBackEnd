namespace TicketOnline.Services
{
    public abstract class ServiceNot 
    {
        private List <IServiceNot> services = new List<IServiceNot>();

        public void attach(IServiceNot serviceNot)
        {
            services.Add(serviceNot);
        }
        public void detach(IServiceNot serviceNot)
        {
            services.Remove(serviceNot);
        }
        protected void notifyServices(string Message)
        {
            if (services.Count > 0) return;
            foreach (IServiceNot serviceNot in services)
            {
                serviceNot.Update(Message);
            }
        }
    }
}
