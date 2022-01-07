using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
using StockDealCommon;
using StockDealDal.Dto;
using StockDealDal.Dto.StockDeal;
using StockDealDal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.Business
{
    public class StockDealCoreBusiness : BaseBusiness
    {
        public async Task<BaseResponse> ListStockDealDetailByTimeAsync(Guid stockDealId, DateTime nextPage, int perPage, Guid loginedContactId)
        {
            var context = new StockDealServiceContext();

            var list = await context.StockDealDetails
                .Where(e => e.StockDealId == stockDealId)
                .Where(e => e.CreatedDate <= nextPage)
                .Where(e => e.Type != (int)TypeStockDealDetail.WaitingForResponse || loginedContactId == e.CreatedBy)
                .OrderByDescending(e => e.CreatedDate)
                .Take(perPage+1).ToListAsync();

            var nextPageNew = list.Take(perPage).LastOrDefault()?.CreatedDate?.AddMilliseconds(-1);

            if (list.Count <= perPage)
            {
                nextPageNew = null;
            }

            return SuccessResponse(new
            {
                IsLastPage = nextPageNew == null,
                NextPage = nextPageNew,
                List = list.Take(perPage)
            });
        }



        /// <summary>
        /// đánh dấu tin nhắn đã đọc
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <param name="loginedContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ReadStockDealDetailAsync(Guid stockDealId, Guid loginedContactId)
        {
            var context = new StockDealServiceContext();
            var stockDeal = await context.StockDeals.FindAsync(stockDealId);
            if (stockDeal == null) return NotFoundResponse();

            string sql = @"UPDATE [dbo].[ST_StockDealDetail]
                        SET [{0}] = 1
                        WHERE StockDealId = '{1}' and ({0} is null or {0} <> 1)";

            if (stockDeal.SenderId == loginedContactId)
            {
                sql = string.Format(sql, "SenderRead", stockDealId.ToString());
            } else
            {
                sql = string.Format(sql, "ReceiverRead", stockDealId.ToString());
            }

            await context.Database.ExecuteSqlRawAsync(sql);

            return SuccessResponse();
        }



        /// <summary>
        /// Xóa stock detail
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <param name="loginedContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> DeleteStockDetailAsync(Guid stockDetailId, Guid loginedContactId)
        {
            var context = new StockDealServiceContext();
            var stockDetail = await context.StockDealDetails
                .Where(e => !e.DeletedDate.HasValue)
                .Where(e => e.CreatedBy == loginedContactId)
                .Where(e => e.Id == stockDetailId)
                .FirstOrDefaultAsync();
                
            if (stockDetail == null) return NotFoundResponse();

            stockDetail.DeletedDate = DateTime.Now;
            stockDetail.DeletedBy = loginedContactId;

            await context.SaveChangesAsync();

            return SuccessResponse(data: stockDetailId);
        }



        /// <summary>
        /// lấy thông tin Stock Deal
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> GetStockDealAsync(Guid stockDealId, Guid loginedContactId)
        {
            var context = new StockDealServiceContext();
            var stockDeal = await context.StockDeals
                .Include(e => e.Ticket)
                .Where(e => e.Id == stockDealId)
                .FirstOrDefaultAsync();

            if (stockDeal == null) return NotFoundResponse();

            return SuccessResponse(data: new GetStockDealResponseDto
            {
                Id = stockDeal.Id,
                ReceiverId = stockDeal.ReceiverId,
                ReceiverName = stockDeal.ReceiverName,
                SenderId = stockDeal.SenderId,
                SenderName = stockDeal.SenderName,
                Ticket = stockDeal.Ticket,
                TicketId = stockDeal.TicketId,
                CreatedDate = stockDeal.CreatedDate,
                CreatedBy = stockDeal.CreatedBy,
                ModifiedDate = stockDeal.ModifiedDate,
                ModifiedBy = stockDeal.ModifiedBy,
                DeletedDate = stockDeal.DeletedDate,
                DeletedBy = stockDeal.DeletedBy,
                ReceiverType = ( (stockDeal.Ticket is SaleTicket && stockDeal.Ticket?.CreatedBy != loginedContactId)
                                || (stockDeal.Ticket is BuyTicket && stockDeal.Ticket?.CreatedBy == loginedContactId) ) ? 1 : 2
            });
        }



        /// <summary>
        /// tạo stock deal
        /// </summary>
        /// <param name="loginContact"></param>
        /// <param name="receiverId"></param>
        /// <param name="tickeId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateStockDealAsync(CreateStockDealDto input)
        {
            var receiverInfo = await CallEventBus.GetStockHolderDetail(input.ReceiverId.Value);
            if (receiverInfo == null) return BadRequestResponse($"receiverId_ERR_INVALID_VALUE");
            else input.ReceiverName = receiverInfo.FullName;

            var context = new StockDealServiceContext();

            if (input.TicketId != null && !context.Tickets.Any(e => e.Id == input.TicketId))
                return BadRequestResponse($"tickeId_ERR_INVALID_VALUE");

            var transaction = await context.Database.BeginTransactionAsync();

            var stockDeal = await context.StockDeals.FirstOrDefaultAsync(e =>
                ((e.SenderId == input.SenderId && e.ReceiverId == input.ReceiverId)
                || (e.SenderId == input.ReceiverId && e.ReceiverId == input.SenderId))
                && e.TicketId == input.TicketId);

            if (stockDeal == null)
            {
                stockDeal = new StockDeal
                {
                    Id = Guid.NewGuid(),
                    SenderId = input.SenderId.Value,
                    ReceiverId = input.ReceiverId.Value,
                    TicketId = input.TicketId,
                    SenderName = input.SenderName,
                    ReceiverName = input.ReceiverName
                };
                context.StockDeals.Add(stockDeal);
                await context.SaveChangesAsync();
            }

            if (input.StockDetail != null)
            {
                await CreateStockDealDetailAsync(stockDeal.Id, input.SenderId.Value, input.StockDetail);
            }

            await transaction.CommitAsync();

            return SuccessResponse(stockDeal.Id);
        }



        /// <summary>
        /// Danh sách stock deal
        /// </summary>
        /// <param name="loginedContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ListStockDealAsync(Guid loginedContactId, int currentPage, int perPage, bool includeEmptyDeal = false)
        {
            var context = new StockDealServiceContext();
            var sql = string.Format(@"EXECUTE [GetListStockDeals] @userId = '{0}', @currentPage = {1}, @pageSize = {2}, @includeEmptyDeal = {3}",
                loginedContactId, currentPage, perPage, includeEmptyDeal);

            var list = await context.ViewListStockDeals.FromSqlRaw(sql).ToListAsync();
            var listResult = list.Select(e => new StockDealResponseDto
            {
                Id = e.Id,
                SenderId = e.SenderId,
                ReceiverId = e.ReceiverId,
                SenderName = e.SenderName,
                ReceiverName = e.ReceiverName,
                ReceiverType = e.ReceiverType,
                LastUpdate = e.LastUpdate,
                CountUnread = e.CountUnread,
                Ticket = new()
                {
                    Id = e.TicketId,
                    Code = e.TicketCode,
                    TicketType = e.TicketType,
                    Title = e.TicketTitle,
                    IsNegotiate = e.TicketIsNegotiate,
                    PriceFrom = e.TicketPrice,
                    Quantity = e.TicketQuantity,
                    StockTypeName = e.TicketStockTypeName,
                    StockCode = e.TicketStockCode,
                    StockCodes = e.TicketStockCodes
                },
                LastDealDetail = new()
                {
                    LastStockDetailId = e.LastStockDetailId,
                    Description = e.Description,
                    Quantity = e.Quantity,
                    TotalPrice = e.TotalPrice,
                    UnitPrice = e.UnitPrice,
                    IsDeletedDealDetail = e.IsDeletedDealDetail,
                    IsOnwerLastDealDetail = e.IsOnwerLastDealDetail,
                    StockDetailType = e.StockDetailType
                }
            });

            var paging = new PaginateDto
            {
                CurrentPage = currentPage,
                PerPage = perPage,
                TotalItems = list.FirstOrDefault() == null ? 0 : list.FirstOrDefault().TotalCount,
                Data = new
                {
                    TotalUnread = list.FirstOrDefault()?.TotalUnread ?? 0,
                    List = listResult
                }
            };
            return SuccessResponse(paging);
        }



        /// <summary>
        /// Danh sách StockDealDetail
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <param name="loginedContactId"></param>
        /// <param name="isPaging"></param>
        /// <param name="currentPage"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ListStockDealDetailAsync(Guid stockDealId, Guid loginedContactId, bool isPaging, int? currentPage = null, int perPage = 20)
        {
            var context = new StockDealServiceContext();
            var stockDeal = context.StockDeals
                .Where(e => !e.DeletedDate.HasValue)
                .Where(e => e.Id == stockDealId)
                .Where(e => e.SenderId == loginedContactId || e.ReceiverId == loginedContactId);

            if (await stockDeal.FirstOrDefaultAsync() == null) return NotFoundResponse();

            var list = context.StockDealDetails
                .Where(e => e.StockDealId == stockDealId)
                .OrderBy(e => e.CreatedDate);

            var paging = new PaginateDto();
            if (isPaging)
            {
                paging.TotalItems = await list.CountAsync();
                paging.PerPage = perPage;
                paging.CurrentPage = currentPage ?? paging.TotalPages;
                paging.Data = await list.Skip( (paging.CurrentPage + (paging.CurrentPage > 0 ? -1 : 0) ) * perPage).Take(perPage).ToListAsync();
            }
            else
            {
                paging.CurrentPage = 1;
                paging.Data = await list.ToListAsync();
                paging.TotalItems = await list.CountAsync();
                paging.PerPage = paging.TotalItems;
            }

            return SuccessResponse(paging);
        }



        /// <summary>
        /// tạo stock deal detail
        /// </summary>
        /// <param name="stockDealId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateStockDealDetailAsync(Guid stockDealId, Guid senderId, CreateStockDetailDto input)
        {
            var context = new StockDealServiceContext();

            var stockDetail = new StockDealDetail()
            {
                Id = Guid.NewGuid(),
                StockDealId = stockDealId,
                CreatedBy = senderId
            };

            var stockDetailDb = context.Add(stockDetail);
            stockDetailDb.CurrentValues.SetValues(input);

            await context.SaveChangesAsync();

            return SuccessResponse(data: stockDetail.Id);
        }

    }
}
