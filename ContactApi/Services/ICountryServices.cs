using ContactApi.Dto;
using System.Diagnostics.Metrics;
using Microsoft.EntityFrameworkCore;

namespace ContactApi.Services
{
    public interface ICountryServices
    {
 
    Task<IEnumerable<CountryDto>> GetAllCountriesAsync();
    Task<CountryDto?> GetCountryByNameAsync(string name);


    }

}
