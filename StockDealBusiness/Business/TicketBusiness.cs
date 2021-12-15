using Microsoft.EntityFrameworkCore;
using StockDealDal.Dto;
using StockDealDal.Dto.Ticket;
using StockDealDal.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealBusiness.Business
{
    public class TicketBusiness : BaseBusiness
    {
        /// <summary>
        /// tạo tin bán cổ phiếu
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="loginContactName"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateSaleTicketAsync(CreateSaleTicketDto saleTicketDto, Guid loginContactId, string loginContactName)
        {
            var _context = new StockDealServiceContext();
            var ticket = _context.Add(new SaleTicket
            {
                Id = Guid.NewGuid(),
                FullName = loginContactName,
                CreatedBy = loginContactId,
                Status = 1,
                ExpDate = DateTime.Now.AddDays(180),
                Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}"
            });

            ticket.CurrentValues.SetValues(saleTicketDto);

            await _context.SaveChangesAsync();

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
            var _context = new StockDealServiceContext();
            var ticket = _context.Add(new BuyTicket
            {
                Id = Guid.NewGuid(),
                FullName = loginContactName,
                CreatedBy = loginContactId,
                Status = 1,
                ExpDate = DateTime.Now.AddDays(180),
                Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}"
            });

            ticket.CurrentValues.SetValues(buyTicketDto);

            await _context.SaveChangesAsync();

            return SuccessResponse(data: ticket.Entity.Id);
        }



        /// <summary>
        /// Cập nhật tin bán cổ phiếu
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <returns></returns>
        public async Task<BaseResponse> UpdateSaleTicketAsync(UpdateSaleTicketDto saleTicketDto, Guid loginContactId)
        {
            var _context = new StockDealServiceContext();
            var ticket = await _context.SaleTickets.FindAsync(saleTicketDto.Id);

            if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

            ticket.ModifiedBy = loginContactId;
            ticket.ModifiedDate = DateTime.Now;

            var ticketDb = _context.Update(ticket);
            ticketDb.CurrentValues.SetValues(saleTicketDto);

            await _context.SaveChangesAsync();

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
            var _context = new StockDealServiceContext();
            var ticket = await _context.BuyTickets.FindAsync(buyTicketDto.Id);

            if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

            ticket.ModifiedBy = loginContactId;
            ticket.ModifiedDate = DateTime.Now;

            var ticketDb = _context.Update(ticket);
            ticketDb.CurrentValues.SetValues(buyTicketDto);

            await _context.SaveChangesAsync();

            return SuccessResponse(data: buyTicketDto.Id);
        }



        /// <summary>
        /// lấy chi tiết tin bằng id
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <returns></returns>
        public async Task<BaseResponse> GetTicketAsync(Guid ticketId)
        {
            var _context = new StockDealServiceContext();
            var ticket = await _context.Tickets
                .Where(e => !e.DeletedDate.HasValue)
                .Where(e => !e.ExpDate.HasValue || e.ExpDate.Value >= DateTime.Now)
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
            var _context = new StockDealServiceContext();
            var ticket = await _context.Tickets
                .Where(e => !e.DeletedDate.HasValue)
                .Where(e => e.CreatedBy == loginContactId)
                .Where(e => e.Id == ticketId)
                .FirstOrDefaultAsync();

            if (ticket == null) return NotFoundResponse();

            ticket.DeletedDate = DateTime.Now;
            ticket.DeletedBy = loginContactId;

            await _context.SaveChangesAsync();

            return SuccessResponse(data: ticketId);
        }



        public async Task<BaseResponse> ListTicketAsync(ListTicketDto listTicketDto, Guid loginContactId)
        {
            var _context = new StockDealServiceContext();

            var query = _context.Tickets.Where(e => !e.DeletedDate.HasValue);

            if (listTicketDto.TicketType.HasValue)
            {
                if (listTicketDto.TicketType.Value == (int)TicketType.Sale)
                {
                    query.Where(e => e is SaleTicket);
                }
                else if (listTicketDto.TicketType.Value == (int)TicketType.Buy)
                {
                    query.Where(e => e is BuyTicket);
                }
            }


            if (listTicketDto.Status.HasValue)
            {
                query.Where(e => e.Status.Equals(listTicketDto.Status.Value));
            }


            if (listTicketDto.IsUser)
            {
                query.Where(e => e.CreatedBy.Value.Equals(loginContactId));
            }

            PaginateDto paginate = new();
            paginate.CurrentPage = listTicketDto.CurrentPage;
            paginate.PerPage = listTicketDto.PerPage;
            paginate.TotalItems = await query.CountAsync();
            paginate.Data = await query.AsNoTracking().ToListAsync();

            return SuccessResponse(data: paginate);
        }
    }
}
