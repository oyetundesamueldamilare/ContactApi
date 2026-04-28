using ContactApi.Dto;

namespace ContactApi.Repository
{
    public interface IContactQueryRepository
    {
        Task<PagedResult<ContactDto>> GetAllAsync(ContactQueryParams q);
    }
}
