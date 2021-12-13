﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockDealBusiness.Business;
using StockDealDal.Dto;
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
                var result = await _stockDealBusiness.GetStockDealAsync(stockDealId);

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
        /// <param name="curPage"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
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


        
        /// <summary>
        /// Danh sách stock deal detail
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <param name="isPaging"></param>
        /// <param name="curPage"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        [HttpGet("v1/ListStockDealDetail")]
        public async Task<ObjectResult> ListStockDealDetailAsync([Required] Guid? stockDetailId, bool isPaging = true, [Range(1, int.MaxValue)] int? curPage = null, int perPage = 20)
        {
            try
            {
                var result = await _stockDealBusiness.ListStockDealDetailAsync(stockDetailId.Value, LoginedContactId, isPaging, curPage, perPage);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }
    }
}
