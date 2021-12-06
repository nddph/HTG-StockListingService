using StockDealDal.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.Business
{
    public class BaseBusiness
    {


        protected BaseResponse SuccessResponse(object data = null, string message = null)
        {
            return new BaseResponse()
            {
                StatusCode = 200,
                Data = data,
                Message = message,
            };
        }



        protected BaseResponse BadRequestResponse(string message = "")
        {
            return new BaseResponse()
            {
                StatusCode = 400,
                Message = message,
            };
        }



        protected BaseResponse NotFoundResponse(string message = "")
        {
            return new BaseResponse()
            {
                StatusCode = 404,
                Message = message,
            };
        }
    }
}
