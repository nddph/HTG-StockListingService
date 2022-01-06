using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockDealBusiness.Business;
using StockDealDal.Dto.StockDeal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockDealService.Controllers
{
    [Authorize]
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
        /// lấy danh sách chi tiết thương lượng theo thời gian
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <param name="nextPage"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        [HttpGet("v1/ListStockDealDetailByTime")]
        public async Task<ObjectResult> ListStockDealDetailByTimeAsync([Required] Guid? stockDetailId, DateTime? nextPage, int perPage = 20)
        {
            try
            {
                var result = await _stockDealBusiness.ListStockDealDetailByTimeAsync(stockDetailId.Value, nextPage ?? DateTime.Now, perPage, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Đánh dấu tin nhắn đã đọc
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <returns></returns>
        [HttpGet("v1/ReadStockDealDetail/{stockDetailId}")]
        public async Task<ObjectResult> ReadStockDealDetailAsync(Guid stockDetailId)
        {
            try
            {
                var result = await _stockDealBusiness.ReadStockDealDetailAsync(stockDetailId, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// xóa deal detail
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <returns></returns>
        [HttpDelete("v1/DeleteStockDetail/{stockDetailId}")]
        public async Task<ObjectResult> DeleteStockDetailAsync(Guid stockDetailId)
        {
            try
            {
                var result = await _stockDealBusiness.DeleteStockDetailAsync(stockDetailId, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Lấy chi tiết stock deal
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <returns></returns>
        [HttpGet("v1/GetStockDeal/{stockDealId}")]
        public async Task<ObjectResult> GetStockDealAsync(Guid stockDealId)
        {
            try
            {
                var result = await _stockDealBusiness.GetStockDealAsync(stockDealId, LoginedContactId);

                return ReturnData(result);

            } catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Tạo StockDeal
        /// </summary>
        /// <param name="receiverId"></param>
        /// <param name="tickeId"></param>
        /// <returns></returns>
        [HttpPost("v1/CreateStockDeal")]
        public async Task<ObjectResult> CreateStockDealAsync(CreateStockDealDto input)
        {
            try
            {
                input.SenderId = LoginedContactId;
                input.SenderName = LoginedContactFullName;

                var result = await _stockDealBusiness.CreateStockDealAsync(input);

                return ReturnData(result);

            } catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Danh sách stock deal cho user
        /// </summary>
        /// <param name="isPaging"></param>
        /// <param name="currentPage"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        [HttpGet("v1/ListStockDeal")]
        public async Task<ObjectResult> ListStockDealAsync(int currentPage = 1, int perPage = 20, bool includeEmptyDeal = false)
        {
            try
            {
                var result = await _stockDealBusiness.ListStockDealAsync(LoginedContactId, currentPage, perPage, includeEmptyDeal);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }


        
        /// <summary>
        /// Danh sách stock deal detail
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <param name="isPaging"></param>
        /// <param name="currentPage"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        [HttpGet("v1/ListStockDealDetail")]
        public async Task<ObjectResult> ListStockDealDetailAsync([Required] Guid? stockDealId, bool isPaging = true, [Range(1, int.MaxValue)] int? currentPage = null, int perPage = 20)
        {
            try
            {
                var result = await _stockDealBusiness.ListStockDealDetailAsync(stockDealId.Value, LoginedContactId, isPaging, currentPage, perPage);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }
    }
}
