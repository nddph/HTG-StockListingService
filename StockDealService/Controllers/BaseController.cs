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



        protected string LoginedContactFullName
        {
            get
            {
                return _currentUser?.Claims?.FirstOrDefault(claim => claim.Type == "fullName")?.Value;
            }
        }



        protected ObjectResult SuccessResponse(BaseResponse response)
        {
            return StatusCode(StatusCodes.Status200OK, response);
        }

        protected ObjectResult BadRequestResponse(BaseResponse response)
        {
            return StatusCode(StatusCodes.Status400BadRequest, response);
        }

        protected ObjectResult NotFoundResponse(BaseResponse response)
        {
            return StatusCode(StatusCodes.Status404NotFound, response);
        }

        protected ObjectResult CatchErrorResponse(Exception e, ILogger logger)
        {
            var _logger = logger;
            _logger.LogError(e.ToString());

            return StatusCode(StatusCodes.Status500InternalServerError, createModel(null, e.Message, 500));
        }

        protected ObjectResult ReturnData(BaseResponse response)
        {
            switch (response.StatusCode)
            {
                case 200:
                    {
                        return SuccessResponse(response);
                    };
                case 400:
                    {
                        return BadRequestResponse(response);
                    };
                case 404:
                    {
                        return NotFoundResponse(response);
                    };
                default: return null;
            }
        }

        protected BaseResponse createModel(object data = null, string message = "", int statusCode = 200)
        {
            return new BaseResponse()
            {
                StatusCode = statusCode,
                Data = data,
                Message = message,
            };
        }

    }
}
