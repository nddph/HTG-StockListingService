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
                        @currentPage = {12}, @pageSize = {13}, @quantityStatus = {14}, @searchText = N'{15}', 
                        @stockTypeIds = N'{16}', @isPaging = {17}, @isHidden = {18}, @ticketId = {19}, @sortIndex = {20}, @sortDirection = {21},
                        @stockHolderName = N'{22}', @createdDateFrom = {23}, @createdDateTo = {24}, @modifiedDateFrom = {25}, @modifiedDateTo = {26}",
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
                        string.Join(",", listTicketDto.StockTypeIds),
                        listTicketDto.IsPaging ? 1 : 0,
                        listTicketDto.IsHidden.GetValueOrDefault(false),
                        listTicketDto.TicketId.HasValue ? $"'{listTicketDto.TicketId.GetValueOrDefault()}'" : "null",
                        listTicketDto.SortIndex,
                        listTicketDto.SortDirection,
                        (listTicketDto.StockHolderName ?? "").Trim(),
                        Helper.FormatRequestDate(listTicketDto.CreatedDateFrom.GetValueOrDefault()),
                        Helper.FormatRequestDate(listTicketDto.CreatedDateTo.GetValueOrDefault()),
                        Helper.FormatRequestDate(listTicketDto.ModifiedDateFrom.GetValueOrDefault()),
                        Helper.FormatRequestDate(listTicketDto.ModifiedDateTo.GetValueOrDefault())
                        );
        }


        /// <summary>
        /// Lấy danh sách tin mua bán
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        public static async Task<List<ViewTickets>> ListTicketAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            var ticketDb = new TicketDB();
            var context = new StockDealServiceContext();

            var sql = ticketDb.GetListTicketQuery(listTicketDto, loginContactId);

            return await context.ViewTickets.FromSqlRaw(sql).AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Xóa tin mua bán
        /// nếu isAll bằng true thì xóa tất cả, ngược lại thì xóa đổi với các ticket có id trong listTicketId
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="listTicketId"></param>
        /// <returns></returns>
        public static async Task DeleteTickets(Guid userId, List<Guid> listTicketId)
        {
            if (listTicketId == null) listTicketId = new();

            var context = new StockDealServiceContext();

            var query = string.Format("exec DeleteTickets @userId = '{0}', @listTicketId='{1}'",
                            userId, string.Join("|", listTicketId));

            await context.Database.ExecuteSqlRawAsync(query);
        }

    }
}
