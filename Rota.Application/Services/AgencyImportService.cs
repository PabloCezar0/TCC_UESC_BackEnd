using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Rota.Application.DTOs;
using Rota.Domain.Entities;

namespace Rota.Application.Services
{
    public class AgencyImportService
    {
        private readonly HttpClient _httpclient;  
        const string url = "http://localhost:3000/agencies";
        public AgencyImportService(HttpClient httpClient) 
        {
            _httpclient = httpClient;
        }

        public async Task<List<Agency>> ImportAgenciesAsync()


        {
            try
            {
                var response = await _httpclient.GetAsync(url); 
                response.EnsureSuccessStatusCode(); 

                var json = await response.Content.ReadAsStringAsync(); 
                var agenciesDto = JsonSerializer.Deserialize<List<AgencyJsonServerDTO>>(json, new JsonSerializerOptions 
                {
                    PropertyNameCaseInsensitive = true 
                });

                if (agenciesDto == null)
                {
                    return new List<Agency>(); 
                }

                return agenciesDto.Select(dto => new Agency(
                    dto.ExternalId,
                    dto.CorporateName,
                    dto.CNPJ,
                    dto.Address,
                    dto.City,
                    dto.State,
                    dto.AddressNumber,
                    dto.AddressComment,
                    dto.PhoneNumberOne,
                    dto.PhoneNumberTwo,
                    dto.Email
                )).ToList();
            } 

            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erro ao acessar a URL: {ex.Message}");
                return new List<Agency>();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Erro ao desserializar o JSON: {ex.Message}");
                return new List<Agency>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro inesperado: {ex.Message}");
                return new List<Agency>();
            }
        }
    }
}