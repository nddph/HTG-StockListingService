using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockDealBusiness.Business;
using StockDealDal.Dto;
using StockDealDal.Dto.Ticket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockDealService.Controllers
{
    [Authorize]
    public class TicketController : BaseController
    {
        private readonly ILogger _logger;
        private readonly TicketBusiness _ticketBusiness;

        public TicketController(ILogger<TicketController> logger)
        {
            _logger = logger;
            _ticketBusiness = new();
        }

        [HttpGet]
        public BaseResponse Health()
        {
            return new BaseResponse()
            {
                Message = "Healthy"
            };
        }


        /// <summary>
        /// Đếm số lượng tin mua bán
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        [HttpPost("v1/CountTicket")]
        public async Task<ObjectResult> CountTicketAsync(TicketSearchCriteria listTicketDto)
        {
            try
            {
                var result = await _ticketBusiness.CountTicketAsync(listTicketDto, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Xóa nhiều tin mua bán cùng lúc
        /// API dùng cho Admin
        /// </summary>
        /// <param name="deleteTicketsDto"></param>
        /// <returns></returns>
        [HttpPost("v1/DeleteTickets")]
        [Authorize(Roles = "APP_TICKET")]
        public async Task<ObjectResult> DeleteTicketsAsync(UpdateTicketsStatusDto deleteTicketsDto)
        {
            try
            {
                deleteTicketsDto.IsDelete = true;
                var result = await _ticketBusiness.UpdateTicketStatusAsync(deleteTicketsDto, LoginedContactId);
                return ReturnData(result);
            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Tạo tin bán cổ phiếu
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <returns></returns>
        [HttpPost("v1/CreateSaleTicket")]
        public async Task<ObjectResult> CreateSaleTicketAsync(CreateSaleTicketDto saleTicketDto)
        {
            try
            {
                var result = await _ticketBusiness.CreateSaleTicketAsync(saleTicketDto, LoginedContactId);

                return ReturnData(result);

            } catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Tạo tin mua cổ phiếu
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <returns></returns>
        [HttpPost("v1/CreateBuyTicket")]
        public async Task<ObjectResult> CreateBuyTicketAsync(CreateBuyTicketDto buyTicketDto)
        {
            try
            {
                var result = await _ticketBusiness.CreateBuyTicketAsync(buyTicketDto, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// cập nhật tin bán
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <returns></returns>
        [HttpPost("v1/UpdateSaleTicket")]
        public async Task<ObjectResult> UpdateSaleTicketAsync(UpdateSaleTicketDto saleTicketDto)
        {
            try
            {
                var result = await _ticketBusiness.UpdateSaleTicketAsync(saleTicketDto, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Cập nhật tin mua
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <returns></returns>
        [HttpPost("v1/UpdateBuyTicket")]
        public async Task<ObjectResult> UpdateBuyTicketAsync(UpdateBuyTicketDto buyTicketDto)
        {
            try
            {
                var result = await _ticketBusiness.UpdateBuyTicketAsync(buyTicketDto, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// lấy chi tiết tin tức
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        [HttpGet("v1/GetTicket/{ticketId}")]
        public async Task<ObjectResult> GetTicketAsync([Required] Guid? ticketId)
        {
            try
            {
                var result = await _ticketBusiness.GetTicketAsync(ticketId.Value, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// xóa tin
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        [HttpDelete("v1/DeleteTicket/{ticketId}")]
        public async Task<ObjectResult> DeleteTicketAsync([Required] Guid? ticketId)
        {
            try
            {
                var result = await _ticketBusiness.DeleteTicketAsync(ticketId.Value, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }


        /// <summary>
        /// lấy danh sách tin đăng
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <returns></returns>
        [HttpPost("v1/ListTickets")]
        public async Task<ObjectResult> ListTicketsAsync(TicketSearchCriteria listTicketDto)
        {
            try
            {
                var result = await _ticketBusiness.ListTicketsAsync(listTicketDto, LoginedContactId);

                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }



        /// <summary>
        /// Cập nhật trạng thái cho nhiều tin mua bán cùng lúc
        /// API dùng cho Admin
        /// </summary>
        /// <param name="updateTicketsDto"></param>
        /// <returns></returns>
        [HttpPost("v1/UpdateTicketStatus")]
        public async Task<ObjectResult> UpdateTicketStatusAsync(UpdateTicketsStatusDto updateTicketsDto)
        {
            try
            {
                updateTicketsDto.IsDelete = false;
                var result = await _ticketBusiness.UpdateTicketStatusAsync(updateTicketsDto, LoginedContactId);
                return ReturnData(result);
            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }


        /// <summary>
        /// lấy danh sách tin đăng cho phía admin
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <returns></returns>
        [HttpPost("v1/ListTicketsWeb")]
        public async Task<ObjectResult> ListTicketsWebAsync(TicketSearchCriteria listTicketDto)
        {
            try
            {
                //set các giá trị lọc cho phía web
                listTicketDto.ByNewer = true;
                listTicketDto.ByUserType = -1;
                listTicketDto.Status = 1;
                listTicketDto.DelTicketStatus = 1;
                listTicketDto.ExpTicketStatus = 1;
                listTicketDto.QuantityStatus = 2;
                listTicketDto.IsHidden = false;
                var result = await _ticketBusiness.ListTicketsAsync(listTicketDto, LoginedContactId);
                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }

        /// <summary>
        /// lấy chi tiết tin tức cho phía web
        /// </summary>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        [HttpGet("v1/GetTicketWeb")]
        public async Task<ObjectResult> GetTicketWebAsync(Guid ticketId)
        {
            try
            {
                if (ticketId == Guid.Empty)
                {
                    return BadRequestResponse(createModel(null, "ticketId_ERR_REQUIRED", 400));
                }
                var result = await _ticketBusiness.GetTicketAsync(ticketId, LoginedContactId);
                return ReturnData(result);

            }
            catch (Exception e)
            {
                return CatchErrorResponse(e, _logger);
            }
        }


    }
}
