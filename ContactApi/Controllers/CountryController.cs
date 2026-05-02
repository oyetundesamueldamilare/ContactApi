using ContactApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContactApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountryController : ControllerBase
    {
        private readonly ICountryServices _countryServices;

        public CountryController(ICountryServices countryServices)
        {
            _countryServices = countryServices;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCountries()
        {
            var countries = await _countryServices.GetAllCountriesAsync();

            // If the list is empty, you might want to return 204 No Content or 404
            if (countries == null || !countries.Any())
            {
                return NoContent(); // Returns 204 instead of 200 with []
            }

            return Ok(countries);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetCountry(string name)
        {
            var country = await _countryServices.GetCountryByNameAsync(name);
            return Ok(country);
        }
    }

}
