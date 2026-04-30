using ContactApi.Dto;
using ContactApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactQueryController : ControllerBase
    {
        private readonly IContactQueryRepository _contactQuery;
        private readonly ILogger<ContactQueryController> _logger;

        public ContactQueryController(IContactQueryRepository contactQuery, ILogger<ContactQueryController> logger)
        {
            _contactQuery = contactQuery;
            _logger = logger;
        }


        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PagedResult<ContactDto>>> GetAll([FromQuery] ContactQueryParams queryParams)
        {
            _logger.LogInformation(
                "GetAll contacts — page: {Page}, size: {Size}, sortBy: {SortBy}, sortDesc: {SortDesc}, name: {Name}, email: {Email}, contactId: {ContactId}",
                queryParams.PageNumber, queryParams.PageSize, queryParams.SortBy, queryParams.SortDesc,
                queryParams.Name, queryParams.Email, queryParams.Id);

            var result = await _contactQuery.GetAllAsync(queryParams);

            _logger.LogInformation("Returned {Count} of {Total} contacts.", result.Items.Count, result.TotalCount);
            return Ok(result);
        }
    }
}

