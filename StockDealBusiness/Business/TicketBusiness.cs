using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
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
            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            var context = new StockDealServiceContext();
            var ticket = context.Add(new SaleTicket
            {
                Id = Guid.NewGuid(),
                FullName = loginContactName,
                CreatedBy = loginContactId,
                Status = 1,
                ExpDate = DateTime.Now.AddDays(180),
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
                ExpDate = DateTime.Now.AddDays(180),
                Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}",
                Email = stockHolderInfo.WorkingEmail,
                EmployeeCode = stockHolderInfo.EmployeeCode,
                Phone = stockHolderInfo.Phone,
                StockCodes = string.Join("|", buyTicketDto.StockCode)
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
            var context = new StockDealServiceContext();
            var ticket = await context.SaleTickets.FindAsync(saleTicketDto.Id);

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
            var ticket = await context.BuyTickets.FindAsync(buyTicketDto.Id);

            if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

            ticket.StockCodes = string.Join("|", buyTicketDto.StockCode);
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



        public async Task<BaseResponse> ListTicketAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            var context = new StockDealServiceContext();

            var query = context.Tickets
                .Where(e => !e.DeletedDate.HasValue)
                .Where(e => e.ExpDate >= DateTime.Now);

            if (listTicketDto.TicketType.HasValue)
            {
                if (listTicketDto.TicketType.Value == (int)TicketType.Sale)
                {
                    query = query.Where(e => e is SaleTicket);
                }
                else if (listTicketDto.TicketType.Value == (int)TicketType.Buy)
                {
                    query = query.Where(e => e is BuyTicket);
                }
            }


            if (listTicketDto.Status.HasValue)
            {
                query = query.Where(e => e.Status.Equals(listTicketDto.Status.Value));
            }

            if (listTicketDto.IsUser)
            {
                query = query.Where(e => e.CreatedBy.Value.Equals(loginContactId));
            }


            if (listTicketDto.PriceFrom.HasValue)
            {
                query = query.Where(e => e is SaleTicket && (e as SaleTicket).PriceFrom.HasValue && (e as SaleTicket).PriceFrom >= listTicketDto.PriceFrom);
            }
            

            if (listTicketDto.PriceTo.HasValue)
            {
                query = query.Where(e => e is SaleTicket && (e as SaleTicket).PriceFrom.HasValue && (e as SaleTicket).PriceFrom <= listTicketDto.PriceTo);
            }
            

            if (listTicketDto.QuantityFrom.HasValue)
            {
                query = query.Where(e => e is SaleTicket && (e as SaleTicket).Quantity >= listTicketDto.QuantityFrom);
            }
            

            if (listTicketDto.QuantityTo.HasValue)
            {
                query = query.Where(e => e is SaleTicket && (e as SaleTicket).Quantity <= listTicketDto.QuantityTo);
            }

            if (listTicketDto.StockCode != null && listTicketDto.StockCode.Count > 0)
            {
                query = query.Where(e => !(e is SaleTicket) || listTicketDto.StockCode.Contains((e as SaleTicket).StockCode));
            }


            List<Ticket> listTicket = await query.AsNoTracking().ToListAsync();


            PaginateDto paginate = new();
            paginate.CurrentPage = listTicketDto.CurrentPage;
            paginate.PerPage = listTicketDto.PerPage;
            paginate.TotalItems = listTicket.Count;

            if (listTicketDto.byNewer)
            {
                listTicket = listTicket.OrderByDescending(e => e.CreatedDate)
                    .Skip((listTicketDto.CurrentPage - 1) * listTicketDto.PerPage)
                    .Take(listTicketDto.PerPage).ToList();
            }

            paginate.Data = listTicket;

            return SuccessResponse(data: paginate);
        }

    }
}
