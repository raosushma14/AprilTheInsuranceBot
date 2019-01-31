using ApriltheInsuranceBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.Services
{
    public class VinDecoder
    {
        private const string RouteTemplate = "/api/vehicles/decodevin/{0}?format=json";
        private string _baseUrl { get; set; }
        public VinDecoder(string baseUrl)
        {
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<VehicleInfoModel> DecodeVINAsync(string vin)
        {
            if(vin == null)
            {
                throw new ArgumentNullException(nameof(vin));
            }

            HttpClient client = new HttpClient();
            var route = _baseUrl + string.Format(RouteTemplate, vin);

            var response = await client.GetAsync(route);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<VinDecodeResponse>(json);
                var model = new VehicleInfoModel
                {
                    Make = (results.Results.Where(p => p.Variable == "Make")?.FirstOrDefault()?.Value) ?? "Unavailable",
                    Model = (results.Results.Where(p => p.Variable == "Model")?.FirstOrDefault()?.Value) ?? "Unavailable",
                    Year = (results.Results.Where(p => p.Variable == "Model Year")?.FirstOrDefault()?.Value) ?? "Unavailable"
                };
                return model;
            }
            else
            {
                throw new ApplicationException($"API Call Failed for VIN {vin}");
            }

        }

    }
}
