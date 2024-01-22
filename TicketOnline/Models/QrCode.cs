using Newtonsoft.Json;

namespace TicketOnline.Models
{
    public class QrCode
    {
        [JsonProperty("id")] // This attribute is important for Cosmos DB
        public string IdQrcode { get; set; }

        // Other properties
        public string DateQrCode { get; set; }
        public string DateExpierDate { get; set; }
        public int IdScanner { get; set; }
        public List<string> QrCodeList { get; set; }
    }

}
