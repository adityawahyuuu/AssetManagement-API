using API.Common;
using API.Constants;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return HandleFailure(result);
        }

        protected IActionResult HandleResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok();
            }

            return HandleFailure(result);
        }

        private IActionResult HandleFailure(Result result)
        {
            if (result.Errors.Any())
            {
                return BadRequest(new
                {
                    type = ResponseMessages.Failed,
                    message = result.Error,
                    detail = result.Errors
                });
            }

            return BadRequest(new
            {
                type = ResponseMessages.Failed,
                message = result.Error,
                detail = string.Empty
            });
        }
    }
}
