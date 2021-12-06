using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockDealBusiness.Business;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockDealService.Controllers
{
    public class StockDealController : BaseController
    {
        private readonly ILogger _logger;
        private readonly StockDealCoreBusiness _stockDealBusiness;
        

        public StockDealController(ILogger<StockDealController> logger)
        {
            _logger = logger;
            _stockDealBusiness = new();
        }



        /// <summary>
        /// Tạo StockDeal
        /// </summary>
        /// <param name="receiverId"></param>
        /// <param name="tickeId"></param>
        /// <returns></returns>
        [HttpPost("v1/CreateStockDeal")]
        public async Task<ObjectResult> CreateStockDealAsync([Required] Guid? receiverId, Guid? tickeId)
        {
            try
            {
                var result = await _stockDealBusiness.CreateStockDealAsync(LoginedContactId, receiverId.Value, tickeId);

                return ReturnData(result);

            } catch(Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        [HttpGet("v1/ListStockDeal")]
        public async Task<ObjectResult> ListStockDealAsync(bool isPaging = true, int curPage = 1, int perPage = 20)
        {
            try
            {
                var result = await _stockDealBusiness.ListStockDealAsync(LoginedContactId, isPaging, curPage, perPage);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }
    }
}
