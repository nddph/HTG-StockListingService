using Microsoft.EntityFrameworkCore;
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
        /// tạo stock deal
        /// </summary>
        /// <param name="loginContact"></param>
        /// <param name="receiverId"></param>
        /// <param name="tickeId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateStockDealAsync(CreateStockDealDto input)
        {
            var _context = new StockDealServiceContext();

            var _transaction = await _context.Database.BeginTransactionAsync();

            var stockDeal = await _context.StockDeals.FirstOrDefaultAsync(e =>
                ((e.SenderId == input.SenderId && e.ReceiverId == input.ReceiverId)
                || (e.SenderId == input.ReceiverId && e.ReceiverId == input.SenderId))
                && e.TickeId == input.TickeId);

            if (stockDeal == null)
            {
                stockDeal = new StockDeal
                {
                    Id = Guid.NewGuid(),
                    SenderId = input.SenderId.Value,
                    ReceiverId = input.ReceiverId.Value,
                    TickeId = input.TickeId,
                    SenderName = input.SenderName,
                    ReceiverName = input.ReceiverName
                };
                _context.StockDeals.Add(stockDeal);
                await _context.SaveChangesAsync();
            }

            await CreateStockDealDetailAsync(stockDeal.Id, input.StockDetail);

            await _transaction.CommitAsync();

            return SuccessResponse(stockDeal.Id);
        }



        /// <summary>
        /// Danh sách stock deal
        /// </summary>
        /// <param name="loginedContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ListStockDealAsync(Guid loginedContactId, bool isPaging, int curPage, int perPage)
        {
            var _context = new StockDealServiceContext();
            var listStockDeal = _context.StockDeals
                .Where(e => e.SenderId == loginedContactId || e.ReceiverId == loginedContactId);

            listStockDeal = listStockDeal.OrderByDescending(e => e.CreatedDate);

            var paging = new PaginateDto();
            if (isPaging)
            {
                paging.CurPage = curPage;
                paging.PerPage = perPage;
                paging.TotalItems = await listStockDeal.CountAsync();
                paging.Data = await listStockDeal.Skip((curPage - 1) * perPage).Take(perPage).ToListAsync();
            } else
            {
                paging.CurPage = 1;
                paging.TotalItems = await listStockDeal.CountAsync();
                paging.PerPage = paging.TotalItems;
                paging.Data = await listStockDeal.ToListAsync();
            }

            return SuccessResponse(paging);
        }



        /// <summary>
        /// Danh sách StockDealDetail
        /// </summary>
        /// <param name="stockDetailId"></param>
        /// <param name="loginedContactId"></param>
        /// <param name="isPaging"></param>
        /// <param name="curPage"></param>
        /// <param name="perPage"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ListStockDealDetailAsync(Guid stockDetailId, Guid loginedContactId, bool isPaging, int? curPage = null, int perPage = 20)
        {
            var _context = new StockDealServiceContext();
            var stockDeal = _context.StockDeals
                .Where(e => e.Id == stockDetailId)
                .Where(e => e.SenderId == loginedContactId || e.ReceiverId == loginedContactId);

            if (await stockDeal.FirstOrDefaultAsync() == null) return NotFoundResponse();

            var list = _context.StockDealDetails
                .Where(e => e.StockDetailId == stockDetailId)
                .OrderBy(e => e.CreatedDate);

            var paging = new PaginateDto();
            if (isPaging)
            {
                paging.TotalItems = await list.CountAsync();
                paging.PerPage = perPage;
                paging.CurPage = curPage ?? paging.TotalPages;
                paging.Data = await list.Skip( (paging.CurPage + (paging.CurPage > 0 ? -1 : 0) ) * perPage).Take(perPage).ToListAsync();
            }
            else
            {
                paging.CurPage = 1;
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
        public async Task<BaseResponse> CreateStockDealDetailAsync(Guid stockDealId, CreateStockDetailDto input)
        {
            var _context = new StockDealServiceContext();

            var stockDetail = new StockDealDetail()
            {
                Id = Guid.NewGuid(),
                StockDetailId = stockDealId
            };

            var stockDetailDb = _context.Add(stockDetail);
            stockDetailDb.CurrentValues.SetValues(input);

            await _context.SaveChangesAsync();

            return SuccessResponse(data: stockDetail.Id);
        }

    }
}
