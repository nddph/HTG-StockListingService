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
        public async Task<BaseResponse> CreateStockDealAsync(Guid loginContact, CreateStockDetailDto input)
        {
            var _context = new StockDealServiceContext();

            if (await _context.StockDeals.AnyAsync(e =>
                ((e.SenderId == loginContact && e.ReceiverId == input.ReceiverId)
                || (e.SenderId == input.ReceiverId && e.ReceiverId == loginContact))
                && e.TickeId == input.TickeId)) return BadRequestResponse("StockDeal_Exits");

            var stockDeal = new StockDeal
            {
                Id = Guid.NewGuid(),
                SenderId = loginContact,
                ReceiverId = input.ReceiverId.Value,
                TickeId = input.TickeId,
                CreatedDate = DateTime.Now
            };

            _context.Add(stockDeal);
            await _context.SaveChangesAsync();

            return SuccessResponse(stockDeal);
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
                paging.Data = await list.Skip( (paging.CurPage + (curPage.HasValue ? -1 : 0) ) * perPage).Take(perPage).ToListAsync();
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
    }
}
