using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.AddressValidator.EasyPost.Services.Models
{
    public class GeocodingResponse
    {
        public GeocodingResponse()
        {
            Verification = new VerificationModel();
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("name")]
        public object Name { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("street1")]
        public string Street1 { get; set; }

        [JsonProperty("street2")]
        public string Street2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("email")]
        public object Email { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("carrier_facility")]
        public object CarrierFacility { get; set; }

        [JsonProperty("residential")]
        public bool? Residential { get; set; }

        [JsonProperty("federal_tax_id")]
        public object FederalTaxId { get; set; }

        [JsonProperty("state_tax_id")]
        public object StateTaxId { get; set; }

        [JsonProperty("verifications")]
        public VerificationModel Verification { get; set; }

        public class VerificationModel
        {
            public VerificationModel()
            {
                Delivery = new DeliveryModel();
            }

            [JsonProperty("delivery")]
            public DeliveryModel Delivery { get; set; }
        }

        public class DeliveryModel
        {
            public DeliveryModel()
            {
                Details = new DetailsModel();
                Errors = new List<ErrorModel>();
            }

            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("errors")]
            public List<ErrorModel> Errors { get; set; }

            [JsonProperty("details")]
            public DetailsModel Details { get; set; }
        }

        public class DetailsModel
        {
            [JsonProperty("latitude")]
            public double Latitude { get; set; }

            [JsonProperty("longitude")]
            public double Longitude { get; set; }

            [JsonProperty("time_zone")]
            public string TimeZone { get; set; }
        }

        public class ErrorModel
        {
            [JsonProperty("suggestion")]
            public object Suggestion { get; set; }

            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("field")]
            public string Field { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }
    }
}
