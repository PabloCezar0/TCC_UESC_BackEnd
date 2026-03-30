using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Rota.Application.Converters;

namespace Rota.Application.DTOs
{
    public class AgencyJsonServerDTO
    {
        [JsonPropertyName("agency_id")]
        [JsonConverter(typeof(JsonStringConverter))]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("agency_name")]
        public string CorporateName { get; set; } = string.Empty;
        public string CNPJ { get; set; } = string.Empty;

        [JsonPropertyName("street_address")]
        public string Address { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;


        [JsonPropertyName("address_number")]
        [JsonConverter(typeof(JsonStringConverter))]
        public string AddressNumber { get; set; } = string.Empty;

        [JsonPropertyName("address_complement")]
        public string AddressComment { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_one")]
        public string PhoneNumberOne { get; set; } = string.Empty;

        [JsonPropertyName("phone_number_two")]
        public string PhoneNumberTwo { get; set; } = string.Empty;

        [JsonPropertyName("email_description")]
        public string Email { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }
}