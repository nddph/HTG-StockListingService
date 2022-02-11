using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
using StockDealBusiness.RequestDB;
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
        public async Task<BaseResponse> ListStockDealDetailByTimeAsync(Guid stockDealId, DateTime nextQueryTime, int perPage, Guid loginedContactId)
        {
            var context = new StockDealServiceContext();

            var list = await context.StockDealDetails
                .Where(e => e.StockDealId == stockDealId)
                .Where(e => e.CreatedDate <= nextQueryTime)
                .Where(e => e.Type != (int)TypeStockDealDetail.WaitingForResponse || loginedContactId == e.CreatedBy)
                .OrderByDescending(e => e.CreatedDate)
                .Take(perPage+1).ToListAsync();

            var nextQueryTimeNew = list.Take(perPage).LastOrDefault()?.CreatedDate?.AddMilliseconds(-1);

            if (list.Count <= perPage)
            {
                nextQueryTimeNew = null;
            }

            return SuccessResponse(new
            {
                IsLastPage = nextQueryTimeNew == null,
                NextQueryTime = nextQueryTimeNew,
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
            var stockDeal = (await StockDealDB.ListStockDealAsync(new()
            {
                LoginedContactId = loginedContactId,
                StockDealId = stockDealId,
                IncludeEmptyDeal = true,
                PerPage = 1,
                CurrentPage = 1
            })).FirstOrDefault();

            if (stockDeal == null) return NotFoundResponse();

            return SuccessResponse(new StockDealResponseDto(stockDeal));
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
            if (input.SenderId == input.ReceiverId) return BadRequestResponse("receiverId_ERR_DUPLICATE");

            var senderInfo = await CallEventBus.GetStockHolderDetail(input.SenderId.GetValueOrDefault());
            if (senderInfo == null) return BadRequestResponse($"senderId_ERR_INVALID_VALUE");
            else input.ReceiverName = senderInfo.FullName;

            var receiverInfo = await CallEventBus.GetStockHolderDetail(input.ReceiverId.GetValueOrDefault());
            if (receiverInfo == null) return BadRequestResponse($"receiverId_ERR_INVALID_VALUE");
            else input.ReceiverName = receiverInfo.FullName;

            var context = new StockDealServiceContext();

            if (input.TicketId != null && !context.Tickets.Any(e => e.Id == input.TicketId))
                return BadRequestResponse($"tickeId_ERR_INVALID_VALUE");

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

            return SuccessResponse(stockDeal.Id);
        }



        /// <summary>
        /// Danh sách stock deal
        /// </summary>
        /// <param name="loginedContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ListStockDealAsync(StockDealSearchCriteria stockDealSearch)
        {

            var list = await StockDealDB.ListStockDealAsync(stockDealSearch);
            var listResult = list.Select(e => new StockDealResponseDto(e));

            var paging = new PaginateDto
            {
                CurrentPage = stockDealSearch.CurrentPage,
                PerPage = stockDealSearch.PerPage,
                TotalItems = list.Select(e => e.TotalCount).FirstOrDefault(),
                Data = new
                {
                    TotalUnread = list.Select(e => e.TotalUnread).FirstOrDefault(),
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

            var stockDeal = await context.StockDeals
                .Include(e => e.Ticket)
                .Where(e => e.Id == stockDealId)
                .Where(e => e.TicketId.HasValue)
                .FirstOrDefaultAsync();

            if (stockDeal == null) return BadRequestResponse("stockDealId_ERR_INVALID_VALUE");
            if (stockDeal.Ticket.DeletedDate.HasValue) return BadRequestResponse("ticketId_ERR_INACTIVE");

            var senderInfo = await CallEventBus.GetStockHolderDetail(stockDeal.SenderId);
            if (senderInfo == null) return BadRequestResponse($"senderId_ERR_INVALID_VALUE");
            else if (stockDeal.SenderName != senderInfo.FullName)
            {
                stockDeal.SenderName = senderInfo.FullName;
                stockDeal.ModifiedBy = senderId;
                stockDeal.ModifiedDate = DateTime.Now;
            }

            var receiverInfo = await CallEventBus.GetStockHolderDetail(stockDeal.ReceiverId);
            if (receiverInfo == null) return BadRequestResponse($"receiverId_ERR_INVALID_VALUE");
            else if (stockDeal.ReceiverName != receiverInfo.FullName)
            {
                stockDeal.ReceiverName = receiverInfo.FullName;
                stockDeal.ModifiedBy = senderId;
                stockDeal.ModifiedDate = DateTime.Now;
            }

            var stockDetail = new StockDealDetail()
            {
                Id = Guid.NewGuid(),
                StockDealId = stockDealId,
                CreatedBy = senderId,
            };

            var stockDetailDb = context.Add(stockDetail);
            stockDetailDb.CurrentValues.SetValues(input);

            await context.SaveChangesAsync();

            return SuccessResponse(data: stockDetail.Id);
        }

    }
}
