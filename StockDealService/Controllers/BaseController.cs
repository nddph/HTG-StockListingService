using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockDealDal.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace StockDealService.Controllers
{
    [Route("stockdeal")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private ClaimsPrincipal _currentUser
        {
            get { return HttpContext.User; }
        }



        protected Guid LoginedContactId
        {
            get
            {
                var id = Guid.Empty;
                if (_currentUser.Identity.IsAuthenticated)
                {
                    Guid.TryParse(_currentUser.Claims.FirstOrDefault(claim => claim.Type == "contactId")?.Value, out id);
                }
                return id;
            }
        }



        protected ObjectResult SuccessResponse(object data = null)
        {
            return StatusCode(StatusCodes.Status200OK, data);
        }



        protected ObjectResult BadRequestResponse(string message = null)
        {
            return StatusCode(StatusCodes.Status400BadRequest, createModel(message));
        }



        protected ObjectResult NotFoundResponse(string message = null)
        {
            return StatusCode(StatusCodes.Status404NotFound, createModel(message));
        }



        protected ObjectResult CatchErrorResponse(Exception e, ILogger logger)
        {
            var _logger = logger;
            _logger.LogError(e.ToString());

            return StatusCode(StatusCodes.Status500InternalServerError, createModel(e.Message));
        }



        protected ObjectResult ReturnData(BaseResponse response)
        {
            switch (response.StatusCode)
            {
                case 200:
                    {
                        return SuccessResponse(response.Data);
                    };
                case 400:
                    {
                        return BadRequestResponse(response.Message?.ToString());
                    };
                case 404:
                    {
                        return NotFoundResponse(response.Message?.ToString());
                    };
                case 415:
                    {
                        return StatusCode(response.StatusCode, response.Message?.ToString());
                    };
                default: return null;
            }
        }



        private ErrorMessage createModel(string message)
        {
            var errMessage = new ErrorMessage()
            {
                error = message,
                error_description = message
            };
            return errMessage;
        }

    }
}
