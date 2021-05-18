using Microsoft.AspNetCore.Mvc;

namespace UKParliament.CodeTest.Web.Controllers
{
    public class RoomBookingsControllerBase : ControllerBase
    {
 
        protected ActionResult HandleResponseBadRequest(string errorMessage, object value = null )
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                return Ok(value);
            }
            else
            {
                return BadRequest(errorMessage);
            }

        }

        protected ActionResult HandleResponseNotFound(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                return Ok();
            }
            else
            {
                return NotFound(errorMessage);
            }

        }

    }
}
