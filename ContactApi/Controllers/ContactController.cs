using ContactApi.Dto;
using ContactApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ContactApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactRepository _contactRepository;

        public ContactController(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContacts()
        {
            var contacts = await _contactRepository.GetAllContactFormsAsync();
            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContactById(int id)
        {
            var contact = await _contactRepository.GetContactFormByIdAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]

        public async Task<string> CreateContact([FromBody] ContactDto contactForm)
        {
            if (contactForm == null)
            {
                return ("Contact form data is required.");
            }
            await _contactRepository.AddContactFormAsync(contactForm);
            return "Contact created";
        }

        [HttpPut]
        public async Task<IActionResult> UpdateContact(int id, [FromBody] ContactDto contactForm)
        {
            if (contactForm == null)
            {
                return BadRequest("Contact form data is required.");
            }
            try
            {
                await _contactRepository.UpdateContactFormAsync(id, contactForm);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            try
            {
                await _contactRepository.DeleteContactFormAsync(id);
                return NoContent();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }
    }
}
