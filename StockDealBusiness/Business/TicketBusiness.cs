using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
using StockDealBusiness.RequestDB;
using StockDealDal.Dto;
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
        /// <summary>
        /// thay đổi trạng thái tin mua bán
        /// </summary>
        /// <param name="changeStatusTicket"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ChangeTicketStatusAsync(ChangeStatusTicketDto changeStatusTicket, Guid userId)
        {
            await TicketDB.UpdateTicketStatusAsync(changeStatusTicket.IsAll.Value, changeStatusTicket.Status.Value, userId, changeStatusTicket.ListTicket);
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
            var context = new StockDealServiceContext();

            var sql = string.Format(@"EXECUTE [GetListTickets_test] @ticketType = {0},
                        @stockCodes = N'{1}', @status = {2}, @ownerId = '{3}', @byUserType = {4}, @priceFrom = {5}, @priceTo = {6},
                        @quantityFrom = {7}, @quantityTo = {8}, @byNewer = {9},
                        @expTicketStatus = {10}, @includeDelTicket = {11},
                        @currentPage = {12}, @pageSize = {13}, @hiddenTicketStatus = {14}, @searchText = N'{15}', @orderByPriceType = {16},
                        @stockTypeIds = N'{17}'",
                        listTicketDto.TicketType,
                        string.Join(",", listTicketDto.StockCodes),
                        listTicketDto.Status,
                        loginContactId,
                        listTicketDto.ByUserType,
                        listTicketDto.PriceFrom, listTicketDto.PriceTo,
                        listTicketDto.QuantityFrom, listTicketDto.QuantityTo,
                        listTicketDto.byNewer ? 1 : 0,
                        listTicketDto.ExpTicketStatus,
                        listTicketDto.IncludeDelTicket ? 1 : 0,
                        listTicketDto.CurrentPage,
                        listTicketDto.PerPage,
                        listTicketDto.HiddenTicketStatus,
                        (listTicketDto.searchText ?? "").Trim(),
                        listTicketDto.OrderByPriceType,
                        string.Join(",", listTicketDto.StockTypeIds)
                        );


            PaginateDto paginate = new();
            paginate.CurrentPage = listTicketDto.CurrentPage;
            paginate.PerPage = listTicketDto.PerPage;

            if (listTicketDto.TicketType == (int)TicketType.Buy)
            {
                var query = await context.ViewBuyTickets.FromSqlRaw(sql).AsNoTracking().ToListAsync();

                paginate.TotalItems = query.Count == 0 ? 0 : query.FirstOrDefault().TotalCount;
                paginate.Data = query;
            }
            else
            {
                var query = await context.ViewSaleTickets.FromSqlRaw(sql).AsNoTracking().ToListAsync();

                paginate.TotalItems = query.Count == 0 ? 0 : query.FirstOrDefault().TotalCount;
                paginate.Data = query;
            }

            return SuccessResponse(data: paginate);
        }

    }
}
