using ContactApi.Data;
using ContactApi.Models;
using ContactApi.Dto;
using Microsoft.EntityFrameworkCore;

namespace ContactApi.Repository
{
    public class ContactRepository     : IContactRepository
    {
        private readonly AppDbContext _context;

        public ContactRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddContactFormAsync(ContactDto contactForm)
        {
            if (contactForm == null)
            {
                throw new ArgumentNullException("contact does not exist");
            }
            var contact = new ContactForm
            {
                Name = contactForm.Name,
                Email = contactForm.Email,
                PhoneNumber = contactForm.PhoneNumber,
                Address = contactForm.Address
            };

            _context.ContactForms.Add(contact);
            await _context.SaveChangesAsync();
        }
        public async Task<List<ContactDto>> GetAllContactFormsAsync()
        {
            return await _context.ContactForms
                .Select(c => new ContactDto
                {
                    Name = c.Name,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Address = c.Address
                })
                .ToListAsync();
        }
        public async Task<ContactDto?> GetContactFormByIdAsync(int id)
        {
            var contact = await _context.ContactForms.FindAsync(id);
            if (contact == null)
            {
                return null;
            }
            return new ContactDto
            {
                Name = contact.Name,
                Email = contact.Email,
                PhoneNumber = contact.PhoneNumber,
                Address = contact.Address
            };
        }
        public async Task UpdateContactFormAsync(int id, ContactDto contactForm)
        {
            var contact = await _context.ContactForms.FindAsync(id);
            if (contact == null)
            {
                throw new ArgumentException("Contact form not found");
            }
            contact.Name = contactForm.Name;
            contact.Email = contactForm.Email;
            contact.PhoneNumber = contactForm.PhoneNumber;
            contact.Address = contactForm.Address;

            _context.ContactForms.Update(contact);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteContactFormAsync(int id)
        {
            var contact = await _context.ContactForms.FindAsync(id);
            if (contact == null)
            {
                throw new ArgumentException("Contact form not found");
            }
            _context.ContactForms.Remove(contact);
            await _context.SaveChangesAsync();
        }
    }
}
