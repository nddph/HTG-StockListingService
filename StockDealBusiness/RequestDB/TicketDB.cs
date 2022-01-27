using Microsoft.EntityFrameworkCore;
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
