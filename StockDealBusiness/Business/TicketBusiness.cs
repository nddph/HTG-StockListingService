using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
using StockDealBusiness.RequestDB;
using StockDealDal.Dto;
using StockDealDal.Dto.EventBus;
using StockDealDal.Dto.Ticket;
using StockDealDal.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemSettingSharing.Entities;

namespace StockDealBusiness.Business
{
    public class TicketBusiness : BaseBusiness
    {
        public async Task<BaseResponse> CountTicketAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            listTicketDto.IsPaging = true;
            listTicketDto.CurrentPage = 1;
            listTicketDto.PerPage = 1;

            listTicketDto.TicketType = (int)TicketType.Buy;
            var listBuyTicket = await TicketDB.ListBuyTicketAsync(listTicketDto, loginContactId);

            listTicketDto.TicketType = (int)TicketType.Sale;
            var listSaleTicket = await TicketDB.ListSaleTicketAsync(listTicketDto, loginContactId);

            return SuccessResponse(new CountTicketResponseDto 
            { 
                CountBuyTicket = listBuyTicket.Select(e => e.TotalCount).FirstOrDefault(),
                CountSaleTicket = listSaleTicket.Select(e => e.TotalCount).FirstOrDefault()
            });
        }



        /// <summary>
        /// Xóa nhiều tin mua bán hoặc tất cả bằng storeproduce
        /// </summary>
        /// <param name="deleteTicketsDto"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> DeleteTicketsAsync(DeleteTicketsDto deleteTicketsDto, Guid userId)
        {
            await TicketDB.DeleteTickets(deleteTicketsDto.IsAll.GetValueOrDefault(), userId, deleteTicketsDto.ListTicket);
            return SuccessResponse();
        }



        /// <summary>
        /// thay đổi trạng thái tin mua bán
        /// </summary>
        /// <param name="changeStatusTicket"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ChangeTicketStatusAsync(ChangeStatusTicketDto changeStatusTicket, Guid userId)
        {
            await TicketDB.UpdateTicketStatusAsync(changeStatusTicket.IsAll.GetValueOrDefault(), changeStatusTicket.Status.GetValueOrDefault(), userId, changeStatusTicket.ListTicket);
            return SuccessResponse();
        }



        /// <summary>
        /// số ngày tin đăng đc hiển thị từ systemsetting
        /// </summary>
        /// <returns></returns>
        private async Task<int> GetTicketExpDateAsync()
        {
            var syscontext = new SystemSettingContext();
            var res = await syscontext.SystemSettings.Where(e => e.Key == "Ticket.ExpDate").FirstOrDefaultAsync();
            if (res == null) return 0;
            return int.Parse(res.Value);
        }



        /// <summary>
        /// tạo tin bán cổ phiếu
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="loginContactName"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateSaleTicketAsync(CreateSaleTicketDto saleTicketDto, Guid loginContactId, string loginContactName)
        {
            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            var stockLimit = await CallEventBus.GetStockHolderLimitAsync(loginContactId, saleTicketDto.StockId.Value, saleTicketDto.StockTypeId.Value);
            if (saleTicketDto.Quantity.Value > stockLimit) return BadRequestResponse($"quantity_ERR_INVALID_VALUE");

            var stockInfo = await CallEventBus.GetStockDetailById(saleTicketDto.StockId.Value);
            if (stockInfo == null) return BadRequestResponse($"stockId_ERR_INVALID_VALUE");
            else saleTicketDto.StockCode = stockInfo.StockCode;

            var stockTypeInfo = await CallEventBus.GetStockTypeDetailOrDefault(saleTicketDto.StockTypeId);
            if (stockTypeInfo == null) return BadRequestResponse($"stockTypeId_ERR_INVALID_VALUE");
            else saleTicketDto.StockTypeName = stockTypeInfo.Name;


            var context = new StockDealServiceContext();
            var ticket = context.Add(new SaleTicket
            {
                Id = Guid.NewGuid(),
                FullName = loginContactName,
                CreatedBy = loginContactId,
                Status = 1,
                ExpDate = DateTime.Now.AddDays(await GetTicketExpDateAsync()),
                Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}",
                Email = stockHolderInfo.WorkingEmail,
                EmployeeCode = stockHolderInfo.EmployeeCode,
                Phone = stockHolderInfo.Phone
            });

            ticket.CurrentValues.SetValues(saleTicketDto);

            await context.SaveChangesAsync();

            #region gửi thông báo có tin đăng liên quan
            TicketSearchCriteria ticketSearch = new()
            {
                IsPaging = false,
                ByUserType = 2,
                ByNewer = true,
                TicketType = 1,
                DelTicketStatus = 1,
                ExpTicketStatus = 1,
                QuantityStatus = 2,
                StockCodes = new List<string>() { saleTicketDto.StockCode }
            };
            var listUserRes = await ListTicketsAsync(ticketSearch, loginContactId);
            if (listUserRes.StatusCode != 200) return listUserRes;
            var listUser = (listUserRes.Data as PaginateDto).Data as List<ViewBuyTickets>;

            SuggestTicketDto suggestTicketDto = new()
            {
                UserId = loginContactId,
                TicketId = ticket.Entity.Id,
                ListReceiverUser = listUser.Select(e => e.CreatedBy.Value).Distinct().ToList()
            };
            await EventBus.CallEventBus.NotificationSuggestTicketAsync(suggestTicketDto, false);
            #endregion

            return SuccessResponse(data: ticket.Entity.Id);
        }



        /// <summary>
        /// Tạo tin mua cổ phiếu
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="loginContactName"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateBuyTicketAsync(CreateBuyTicketDto buyTicketDto, Guid loginContactId, string loginContactName)
        {
            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            var context = new StockDealServiceContext();
            var ticket = context.Add(new BuyTicket
            {
                Id = Guid.NewGuid(),
                FullName = loginContactName,
                CreatedBy = loginContactId,
                Status = 1,
                ExpDate = DateTime.Now.AddDays(await GetTicketExpDateAsync()),
                Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}",
                Email = stockHolderInfo.WorkingEmail,
                EmployeeCode = stockHolderInfo.EmployeeCode,
                Phone = stockHolderInfo.Phone,
                StockCodes = string.Join(",", buyTicketDto.StockCode)
            });

            ticket.CurrentValues.SetValues(buyTicketDto);

            await context.SaveChangesAsync();

            #region gửi thông báo có tin đăng liên quan
            TicketSearchCriteria ticketSearch = new()
            {
                IsPaging = false,
                ByUserType = 2,
                ByNewer = true,
                TicketType = 2,
                DelTicketStatus = 1,
                ExpTicketStatus = 1,
                QuantityStatus = 2,
                StockCodes = buyTicketDto.StockCode
            };
            var listUserRes = await ListTicketsAsync(ticketSearch, loginContactId);
            if (listUserRes.StatusCode != 200) return listUserRes;
            var listUser = (listUserRes.Data as PaginateDto).Data as List<ViewSaleTickets>;

            SuggestTicketDto suggestTicketDto = new()
            {
                UserId = loginContactId,
                TicketId = ticket.Entity.Id,
                ListReceiverUser = listUser.Select(e => e.CreatedBy.Value).Distinct().ToList()
            };
            await EventBus.CallEventBus.NotificationSuggestTicketAsync(suggestTicketDto, false);
            #endregion

            return SuccessResponse(data: ticket.Entity.Id);
        }



        /// <summary>
        /// Cập nhật tin bán cổ phiếu
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <returns></returns>
        public async Task<BaseResponse> UpdateSaleTicketAsync(UpdateSaleTicketDto saleTicketDto, Guid loginContactId)
        {
            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            var stockLimit = await CallEventBus.GetStockHolderLimitAsync(loginContactId, saleTicketDto.StockId.Value, saleTicketDto.StockTypeId.Value);
            if (saleTicketDto.Quantity.Value > stockLimit) return BadRequestResponse($"quantity_ERR_INVALID_VALUE");

            var stockInfo = await CallEventBus.GetStockDetailById(saleTicketDto.StockId.Value);
            if (stockInfo == null) return BadRequestResponse($"stockId_ERR_INVALID_VALUE");
            else saleTicketDto.StockCode = stockInfo.StockCode;

            var stockTypeInfo = await CallEventBus.GetStockTypeDetailOrDefault(saleTicketDto.StockTypeId);
            if (stockTypeInfo == null) return BadRequestResponse($"stockTypeId_ERR_INVALID_VALUE");
            else saleTicketDto.StockTypeName = stockTypeInfo.Name;


            var context = new StockDealServiceContext();
            var ticket = await context.SaleTickets
                .Where(e => e.CreatedBy == loginContactId)
                .Where(e => e.Id == saleTicketDto.Id.Value)
                .FirstOrDefaultAsync();

            if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

            ticket.ModifiedBy = loginContactId;
            ticket.ModifiedDate = DateTime.Now;

            var ticketDb = context.Update(ticket);
            ticketDb.CurrentValues.SetValues(saleTicketDto);

            await context.SaveChangesAsync();

            return SuccessResponse(data: saleTicketDto.Id);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> UpdateBuyTicketAsync(UpdateBuyTicketDto buyTicketDto, Guid loginContactId)
        {
            var context = new StockDealServiceContext();
            var ticket = await context.BuyTickets
                .Where(e => e.CreatedBy == loginContactId)
                .Where(e => e.Id == buyTicketDto.Id.Value)
                .FirstOrDefaultAsync();

            if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

            ticket.StockCodes = string.Join(",", buyTicketDto.StockCode);
            ticket.ModifiedBy = loginContactId;
            ticket.ModifiedDate = DateTime.Now;

            var ticketDb = context.Update(ticket);
            ticketDb.CurrentValues.SetValues(buyTicketDto);

            await context.SaveChangesAsync();

            return SuccessResponse(data: buyTicketDto.Id);
        }



        /// <summary>
        /// lấy chi tiết tin bằng id
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <returns></returns>
        public async Task<BaseResponse> GetTicketAsync(Guid ticketId)
        {
            var context = new StockDealServiceContext();
            var ticket = await context.Tickets
                .Where(e => e.Id == ticketId)
                .FirstOrDefaultAsync();

            if (ticket == null) return NotFoundResponse();

            return SuccessResponse(data: ticket);
        }



        /// <summary>
        /// Xóa tin đăng
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> DeleteTicketAsync(Guid ticketId, Guid loginContactId)
        {
            var context = new StockDealServiceContext();
            var ticket = await context.Tickets
                .Where(e => !e.DeletedDate.HasValue)
                .Where(e => e.CreatedBy == loginContactId)
                .Where(e => e.Id == ticketId)
                .FirstOrDefaultAsync();

            if (ticket == null) return NotFoundResponse();

            ticket.DeletedDate = DateTime.Now;
            ticket.DeletedBy = loginContactId;

            await context.SaveChangesAsync();

            return SuccessResponse(data: ticketId);
        }



        /// <summary>
        /// Danh sách tin mua bán
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ListTicketsAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {

            PaginateDto paginate = new();

            if (listTicketDto.TicketType == (int)TicketType.Buy)
            {
                var query = await TicketDB.ListBuyTicketAsync(listTicketDto, loginContactId);

                paginate.TotalItems = query.Select(e => e.TotalCount).FirstOrDefault();
                paginate.Data = query;
            }
            else
            {
                var query = await TicketDB.ListSaleTicketAsync(listTicketDto, loginContactId);

                paginate.TotalItems = query.Select(e => e.TotalCount).FirstOrDefault();
                paginate.Data = query;
            }

            paginate.CurrentPage = listTicketDto.IsPaging ? listTicketDto.CurrentPage : 1;
            paginate.PerPage = listTicketDto.IsPaging ? listTicketDto.PerPage : paginate.TotalItems;


            return SuccessResponse(data: paginate);
        }

    }
}
