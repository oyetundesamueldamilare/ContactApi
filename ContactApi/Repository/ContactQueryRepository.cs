using ContactApi.Data;
using ContactApi.Dto;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace ContactApi.Repository
{
    public class ContactQueryRepository       : IContactQueryRepository
    {

        private readonly AppDbContext _context;     
       
        public ContactQueryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ContactDto>> GetAllAsync(ContactQueryParams q)
        {
            var query = _context.ContactForms.AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(q.Name))
                query = query.Where(e => e.Name.Contains(q.Name));
            if (!string.IsNullOrWhiteSpace(q.Email))
                query = query.Where(e => e.Email.Contains(q.Email));
            if (q.Id.HasValue)
                query = query.Where(e => e.Id == q.Id.Value);

            var totalCount = await query.CountAsync();

            // Sorting
            query = q.SortBy?.ToLower() switch
            {
                "name" => q.SortDesc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
                "email" => q.SortDesc ? query.OrderByDescending(e => e.Email) : query.OrderBy(e => e.Email),
                "phoneNumber" => q.SortDesc ? query.OrderByDescending(e => e.PhoneNumber) : query.OrderBy(e => e.PhoneNumber),
                _ => query.OrderBy(e => e.Id)
            };

            // Paging
            var items = await query
                .Skip((q.PageNumber - 1) * q.PageSize)
                .Take(q.PageSize)
                .Select(e => new ContactDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Email = e.Email,
                    PhoneNumber = e.PhoneNumber,
                     })
                .ToListAsync();

        
            return new PagedResult<ContactDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = q.PageNumber,
                PageSize = q.PageSize
            };
        }
    }
}
