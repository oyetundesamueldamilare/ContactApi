using ContactApi.Dto;

namespace ContactApi.Repository
{
    public interface IContactRepository
    {
        Task AddContactFormAsync(ContactDto contactForm);

        Task<List<ContactDto>> GetAllContactFormsAsync();

        Task<ContactDto?> GetContactFormByIdAsync(int id);

        Task UpdateContactFormAsync(int id, ContactDto contactForm);

        Task DeleteContactFormAsync(int id);
    }
}