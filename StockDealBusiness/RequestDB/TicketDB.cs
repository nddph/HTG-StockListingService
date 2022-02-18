using Microsoft.EntityFrameworkCore;
using StockDealCommon;
using StockDealDal.Dto.Ticket;
using StockDealDal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.RequestDB
{
    public class TicketDB
    {
        public string GetListTicketQuery(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            return string.Format(@"EXECUTE [GetListTickets] @ticketType = {0},
                        @stockCodes = N'{1}', @status = {2}, @ownerId = '{3}', @byUserType = {4}, @priceFrom = {5}, @priceTo = {6},
                        @quantityFrom = {7}, @quantityTo = {8}, @byNewer = {9},
                        @expTicketStatus = {10}, @delTicketStatus = {11},
                        @currentPage = {12}, @pageSize = {13}, @quantityStatus = {14}, @searchText = N'{15}', @orderByPriceType = {16},
                        @stockTypeIds = N'{17}', @isPaging = {18}, @isHidden = {19}, @ticketId = {20}",
                        listTicketDto.TicketType,
                        string.Join(",", listTicketDto.StockCodes),
                        listTicketDto.Status,
                        loginContactId,
                        listTicketDto.ByUserType,
                        listTicketDto.PriceFrom, listTicketDto.PriceTo,
                        listTicketDto.QuantityFrom, listTicketDto.QuantityTo,
                        listTicketDto.ByNewer ? 1 : 0,
                        listTicketDto.ExpTicketStatus,
                        listTicketDto.DelTicketStatus,
                        listTicketDto.CurrentPage,
                        listTicketDto.PerPage,
                        listTicketDto.QuantityStatus,
                        (listTicketDto.SearchText ?? "").Trim(),
                        listTicketDto.OrderByPriceType,
                        string.Join(",", listTicketDto.StockTypeIds),
                        listTicketDto.IsPaging ? 1 : 0,
                        listTicketDto.IsHidden.GetValueOrDefault(false),
                        listTicketDto.TicketId.HasValue ? $"'{listTicketDto.TicketId.GetValueOrDefault()}'" : "null"
                        );
        }



        /// <summary>
        /// lấy danh sách tin mua
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        public static async Task<List<ViewSaleTickets>> ListSaleTicketAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            var ticketDb = new TicketDB();
            var context = new StockDealServiceContext();

            var sql = ticketDb.GetListTicketQuery(listTicketDto, loginContactId);

            return await context.ViewSaleTickets.FromSqlRaw(sql).AsNoTracking().ToListAsync();
        }



        /// <summary>
        /// Lấy danh sách tin bán
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="isBuyTicket"></param>
        /// <returns></returns>
        public static async Task<List<ViewBuyTickets>> ListBuyTicketAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            var ticketDb = new TicketDB();
            var context = new StockDealServiceContext();

            var sql = ticketDb.GetListTicketQuery(listTicketDto, loginContactId);

            return await context.ViewBuyTickets.FromSqlRaw(sql).AsNoTracking().ToListAsync();
        }



        /// <summary>
        /// Xóa tin mua bán
        /// nếu isAll bằng true thì xóa tất cả, ngược lại thì xóa đổi với các ticket có id trong listTicketId
        /// </summary>
        /// <param name="isAll"></param>
        /// <param name="userId"></param>
        /// <param name="listTicketId"></param>
        /// <returns></returns>
        public static async Task DeleteTickets(bool isAll, Guid userId, List<Guid> listTicketId)
        {
            if (listTicketId == null) listTicketId = new();

            var context = new StockDealServiceContext();

            var query = string.Format("exec DeleteTickets @isAll = {0}, @userId = '{1}', @listTicketId='{2}'",
                isAll, userId, string.Join("|", listTicketId));

            await context.Database.ExecuteSqlRawAsync(query);
        }



        /// <summary>
        /// cập nhật trạng thái ticket theo status
        /// nếu isAll bằng true thì cập nhật tất cả, ngược lại thì cập nhật đổi với các ticket có id trong listTicketId
        /// </summary>
        /// <param name="isAll"></param>
        /// <param name="status"></param>
        /// <param name="userId"></param>
        /// <param name="listTicketId"></param>
        /// <returns></returns>
        public static async Task UpdateTicketStatusAsync (bool isAll, int status, Guid userId, List<Guid> listTicketId)
        {
            if (listTicketId == null) listTicketId = new();

            var context = new StockDealServiceContext();

            var query = string.Format("exec UpdateTicketStatus @isAll = {0}, @status = {1}, @userId = '{2}', @listTicketId='{3}'",
                isAll, status, userId, string.Join("|", listTicketId) );

            await context.Database.ExecuteSqlRawAsync(query);
        }
    }
}
