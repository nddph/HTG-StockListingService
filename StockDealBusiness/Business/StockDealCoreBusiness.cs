﻿using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
using StockDealDal.Dto;
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
        public async Task<BaseResponse> GetStockDealAsync(Guid stockDealId)
        {
            var context = new StockDealServiceContext();
            var stockDeal = await context.StockDeals.Include(e => e.Ticket).FirstOrDefaultAsync(e => e.Id == stockDealId);

            if (stockDeal == null) return NotFoundResponse();

            return SuccessResponse(data: stockDeal);
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

            if (input.TickeId != null && !context.Tickets.Any(e => e.Id == input.TickeId))
                return BadRequestResponse($"tickeId_ERR_INVALID_VALUE");

            var transaction = await context.Database.BeginTransactionAsync();

            var stockDeal = await context.StockDeals.FirstOrDefaultAsync(e =>
                ((e.SenderId == input.SenderId && e.ReceiverId == input.ReceiverId)
                || (e.SenderId == input.ReceiverId && e.ReceiverId == input.SenderId))
                && e.TicketId == input.TickeId);

            if (stockDeal == null)
            {
                stockDeal = new StockDeal
                {
                    Id = Guid.NewGuid(),
                    SenderId = input.SenderId.Value,
                    ReceiverId = input.ReceiverId.Value,
                    TicketId = input.TickeId,
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
        public async Task<BaseResponse> ListStockDealAsync(Guid loginedContactId, int currentPage, int perPage)
        {
            var context = new StockDealServiceContext();
            var sql = string.Format(@"EXECUTE [GetListStockDeals] @userId = '{0}', @currentPage = {1}, @pageSize = {2}",
                loginedContactId, currentPage, perPage);

            var list = await context.ViewListStockDeals.FromSqlRaw(sql).ToListAsync();
            var paging = new PaginateDto
            {
                CurrentPage = currentPage,
                PerPage = perPage,
                TotalItems = list.FirstOrDefault() == null ? 0 : list.FirstOrDefault().TotalCount,
                Data = list
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
                .Where(e => !e.DeletedDate.HasValue)
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
