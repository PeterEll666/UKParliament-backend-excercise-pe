using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UKParliament.CodeTest.Services;
using UKParliament.CodeTest.Services.Models;

namespace UKParliament.CodeTest.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonController : RoomBookingsControllerBase
    {
        private readonly IPersonService _personService;

        public PersonController(IPersonService personService)
        {
            _personService = personService;
        }

        [HttpGet("{personId}")]
        public async Task<ActionResult<PersonInfo>> Get(int personId)
        {
            return await _personService.GetAsync(personId) ?? new ActionResult<PersonInfo>(NotFound()); 
        }

        [Route("Search")]
        [HttpGet]
        public async Task<ActionResult<List<PersonInfo>>> Search(string searchName)
        {
            var response = await _personService.SearchAsync(searchName);
            return HandleResponseBadRequest(response.ErrorMessage, response.Response);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Add([FromBody()] AddPersonInfo person)
        {
            var id = await _personService.AddAsync(person);
            return id;
        }

        [HttpPut]
        public async Task<ActionResult<int>> Update([FromBody()] PersonInfo person)
        {
            return HandleResponseNotFound(await _personService.UpdateAsync(person));
        }

        [HttpDelete("{personId}")]
        public async Task<ActionResult> Delete(int personId, bool deleteBookings)
        {
            return HandleResponseBadRequest(await _personService.DeleteAsync(personId, deleteBookings));
        }

    }
}
