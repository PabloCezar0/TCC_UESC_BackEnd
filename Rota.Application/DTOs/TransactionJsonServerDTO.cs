using System.Text.Json.Serialization;

namespace Rota.Application.DTOs
{
    public class TransactionJsonServerDTO
    {
        [JsonPropertyName("transaction_id")]
        public int TransactionId { get; set; }

        [JsonPropertyName("paid_price")]
        public decimal PaidPrice { get; set; }

        [JsonPropertyName("extra_charge_price")]
        public decimal ExtraChargePrice { get; set; }

        [JsonPropertyName("insurance_amount")]
        public decimal InsuranceAmount { get; set; }

        [JsonPropertyName("sale_datetime_h")]
        public DateTime SaleDatetimeH { get; set; }

        [JsonPropertyName("agency_id")]
        public int AgencyId { get; set; }

        [JsonPropertyName("category_id")]
        public int CategoryId { get; set; }

        [JsonPropertyName("cancellation_flag")]
        public int CancellationFlag { get; set; }

        [JsonPropertyName("sale_type_id")]
        public int SaleTypeId { get; set; }



    }
}