using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
using StockDealBusiness.RequestDB;
using StockDealCommon;
using StockDealDal.Dto;
using StockDealDal.Dto.EventBus;
using StockDealDal.Dto.Ticket;
using StockDealDal.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemSettingSharing.Entities;

namespace StockDealBusiness.Business
{
    public class TicketBusiness : BaseBusiness
    {
        public async Task<object> GetTicketAsync(Guid ticketId, TicketType ticketType, Guid loginContactId)
        {
            var ticketSearchCriteria = new TicketSearchCriteria()
            {
                TicketId = ticketId,
                DelTicketStatus = -1,
                IsPaging = true,
                CurrentPage = 1,
                PerPage = 1,
                ByNewer = true,
                ByUserType = -1,
                TicketType = (int)ticketType,
                ExpTicketStatus = -1,
                IsHidden = false,
                Status = -1,
                QuantityStatus = -1
            };

            if (ticketType == TicketType.Buy) return (await TicketDB.ListBuyTicketAsync(ticketSearchCriteria, loginContactId)).FirstOrDefault();
            return (await TicketDB.ListSaleTicketAsync(ticketSearchCriteria, loginContactId)).FirstOrDefault();

        }



        /// <summary>
        /// đếm số lượng ticket
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CountTicketAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            listTicketDto.IsPaging = true;
            listTicketDto.CurrentPage = 1;
            listTicketDto.PerPage = 1;

            listTicketDto.TicketType = (int)TicketType.Buy;
            var listBuyTicket = await TicketDB.ListBuyTicketAsync(listTicketDto, loginContactId);

            listTicketDto.TicketType = (int)TicketType.Sale;
            var listSaleTicket = await TicketDB.ListSaleTicketAsync(listTicketDto, loginContactId);

            return SuccessResponse(new CountTicketResponseDto 
            { 
                CountBuyTicket = listBuyTicket.Select(e => e.TotalCount).FirstOrDefault(),
                CountSaleTicket = listSaleTicket.Select(e => e.TotalCount).FirstOrDefault()
            });
        }


        /// <summary>
        /// tạo tin bán cổ phiếu
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="loginContactName"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateSaleTicketAsync(CreateSaleTicketDto saleTicketDto, Guid loginContactId)
        {
            var context = new StockDealServiceContext();

            //kiểm tra xem quá số lượng cho phép đăng tin 1 ngày hay không
            var today = DateTime.Now.Date;
            var totalTicketDaily = await context.Tickets.Where(x => x.CreatedDate.HasValue && x.CreatedDate.Value.Date == today && x.DeletedDate == null).CountAsync();
            var minTicketDaily = await SystemSettingDB.GetMinTicketDaily();
            if (minTicketDaily != null && totalTicketDaily == minTicketDaily && minTicketDaily > 0)
            {
                return BadRequestResponse("ticket_ERR_MIN_TICKET_DAILY");
            }

            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            var stockLimit = await CallEventBus.GetStockHolderLimitAsync(loginContactId, saleTicketDto.StockId.Value, saleTicketDto.StockTypeId.Value);
            if (saleTicketDto.Quantity.Value > stockLimit) return BadRequestResponse($"quantity_ERR_INVALID_VALUE");

            var stockInfo = await CallEventBus.GetStockDetailById(saleTicketDto.StockId.Value);
            if (stockInfo == null) return BadRequestResponse($"stockId_ERR_INVALID_VALUE");
            else saleTicketDto.StockCode = stockInfo.StockCode;

            var stockTypeInfo = await CallEventBus.GetStockTypeDetailOrDefault(saleTicketDto.StockTypeId);
            if (stockTypeInfo == null) return BadRequestResponse($"stockTypeId_ERR_INVALID_VALUE");
            else saleTicketDto.StockTypeName = stockTypeInfo.Name;

            //kiểm tra số lượng CP có hợp lệ hay không
            if(!saleTicketDto.IsNegotiate)
            {
                var systemSetting = await SystemSettingDB.GetTransactionMultiple();
                if (systemSetting != null && saleTicketDto.Quantity % systemSetting != 0)
                {
                    return BadRequestResponse("quantity_ERR_TRANS_MULTIPLES");
                }
            }

            var ticket = context.Add(new SaleTicket
            {
                Id = Guid.NewGuid(),
                FullName = stockHolderInfo.FullName,
                CreatedBy = loginContactId,
                Status = 1,
                ExpDate = DateTime.Now.AddDays(await SystemSettingDB.GetTicketExpDateAsync()).Date,
                Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}",
                Email = stockHolderInfo.WorkingEmail,
                EmployeeCode = stockHolderInfo.EmployeeCode,
                Phone = stockHolderInfo.Phone
            });

            ticket.CurrentValues.SetValues(saleTicketDto);

            await context.SaveChangesAsync();

            #region gửi thông báo có tin đăng liên quan
            TicketSearchCriteria ticketSearch = new()
            {
                IsPaging = false,
                ByUserType = 2,
                ByNewer = true,
                TicketType = (int)TicketType.Buy,
                DelTicketStatus = 1,
                ExpTicketStatus = 1,
                QuantityStatus = 2,
                StockCodes = new List<string>() { saleTicketDto.StockCode },
                Status = 1
            };
            var listUserRes = await ListTicketsAsync(ticketSearch, loginContactId);
            if (listUserRes.StatusCode != 200) return listUserRes;
            var listUser = (listUserRes.Data as PaginateDto).Data as List<ViewBuyTickets>;

            SuggestTicketDto suggestTicketDto = new()
            {
                UserId = loginContactId,
                TicketId = ticket.Entity.Id,
                ListReceiverUser = listUser.Select(e => e.CreatedBy.Value).Distinct().ToList(),
                TicketType = (int)TicketType.Sale,
                StockCodes = ticket.Entity.StockCode,
                Quantity = ticket.Entity.Quantity,
                Price = ticket.Entity.PriceFrom,
                IsNegotiate = ticket.Entity.IsNegotiate,
                Title = ticket.Entity.Title
            };
            await CallEventBus.NotificationSuggestTicketAsync(suggestTicketDto, false);
            #endregion

            return SuccessResponse(data: ticket.Entity.Id);
        }



        /// <summary>
        /// Tạo tin mua cổ phiếu
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="loginContactName"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateBuyTicketAsync(CreateBuyTicketDto buyTicketDto, Guid loginContactId)
        {
            var context = new StockDealServiceContext();

            //kiểm tra xem quá số lượng cho phép đăng tin 1 ngày hay không
            var today = DateTime.Now.Date;
            var totalTicketDaily = await context.Tickets.Where(x => x.CreatedDate.HasValue && x.CreatedDate.Value.Date == today && x.DeletedDate == null).CountAsync();
            var minTicketDaily = await SystemSettingDB.GetMinTicketDaily();
            if (minTicketDaily != null && totalTicketDaily == minTicketDaily && minTicketDaily > 0)
            {
                return BadRequestResponse("ticket_ERR_MIN_TICKET_DAILY");
            }

            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            // cho phép người dùng không phải nhân viên đăng bài
            if (await SystemSettingDB.AllowCreateBuyTicketAsync() == false)
            {
                // nếu không phải nhân viên thì không cho đăng
                if (stockHolderInfo.Status == 0) return BadRequestResponse("user_ERR_ONLY_STAFF_CREATE_BUY_TICKET");
            }

            //kiểm tra số lượng CP có hợp lệ hay không
            if(buyTicketDto.Quantity != null)
            {
                var systemSetting = await SystemSettingDB.GetTransactionMultiple();
                if (systemSetting != null && buyTicketDto.Quantity % systemSetting != 0)
                {
                    return BadRequestResponse("quantity_ERR_TRANS_MULTIPLES");
                }
            }

            var ticket = context.Add(new BuyTicket
            {
                Id = Guid.NewGuid(),
                FullName = stockHolderInfo.FullName,
                CreatedBy = loginContactId,
                Status = 1,
                ExpDate = DateTime.Now.AddDays(await SystemSettingDB.GetTicketExpDateAsync()).Date,
                Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}",
                Email = stockHolderInfo.WorkingEmail,
                EmployeeCode = stockHolderInfo.EmployeeCode,
                Phone = stockHolderInfo.Phone,
                StockCodes = string.Join(",", buyTicketDto.StockCode)
            });

            ticket.CurrentValues.SetValues(buyTicketDto);

            //danh sách các mã CP đang có trong hệ thống
            var stocks = await CallEventBus.GetStockList(true);

            foreach(var item in buyTicketDto.StockCode)
            {
                var stock = stocks.Count > 0 ? stocks.FirstOrDefault(x => x.StockCode == item) : null;
                if(stock == null)
                {
                    return BadRequestResponse("stockCode_ERR_INVALID_VALUE", item);
                }
                var buyTicketDetail = new BuyTicketDetail()
                {
                    Id = Guid.NewGuid(),
                    BuyTicketId = ticket.Entity.Id,
                    StockId = stock.Id,
                    StockCode = stock.StockCode,
                    PriceFrom = buyTicketDto.PriceFrom,
                    PriceTo = buyTicketDto.PriceTo,
                    Quantity = buyTicketDto.Quantity,
                    IsNegotiate = buyTicketDto.IsNegotiate,
                    CreatedBy = loginContactId
                };
                context.Add(buyTicketDetail);
            }

            await context.SaveChangesAsync();

            #region gửi thông báo có tin đăng liên quan
            TicketSearchCriteria ticketSearch = new()
            {
                IsPaging = false,
                ByUserType = 2,
                ByNewer = true,
                TicketType = (int)TicketType.Sale,
                DelTicketStatus = 1,
                ExpTicketStatus = 1,
                QuantityStatus = 2,
                StockCodes = buyTicketDto.StockCode,
                Status = 1
            };
            var listUserRes = await ListTicketsAsync(ticketSearch, loginContactId);
            if (listUserRes.StatusCode != 200) return listUserRes;
            var listUser = (listUserRes.Data as PaginateDto).Data as List<ViewSaleTickets>;

            SuggestTicketDto suggestTicketDto = new()
            {
                UserId = loginContactId,
                TicketId = ticket.Entity.Id,
                ListReceiverUser = listUser.Select(e => e.CreatedBy.Value).Distinct().ToList(),
                TicketType = (int)TicketType.Buy,
                StockCodes = ticket.Entity.StockCodes,
                Title = ticket.Entity.Title
            };
            await CallEventBus.NotificationSuggestTicketAsync(suggestTicketDto, false);
            #endregion

            return SuccessResponse(data: ticket.Entity.Id);
        }



        /// <summary>
        /// Cập nhật tin bán cổ phiếu
        /// </summary>
        /// <param name="saleTicketDto"></param>
        /// <returns></returns>
        public async Task<BaseResponse> UpdateSaleTicketAsync(UpdateSaleTicketDto saleTicketDto, Guid loginContactId)
        {
            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            var stockLimit = await CallEventBus.GetStockHolderLimitAsync(loginContactId, saleTicketDto.StockId.Value, saleTicketDto.StockTypeId.Value);
            if (saleTicketDto.Quantity.Value > stockLimit) return BadRequestResponse($"quantity_ERR_INVALID_VALUE");

            var stockInfo = await CallEventBus.GetStockDetailById(saleTicketDto.StockId.Value);
            if (stockInfo == null) return BadRequestResponse($"stockId_ERR_INVALID_VALUE");
            else saleTicketDto.StockCode = stockInfo.StockCode;

            var stockTypeInfo = await CallEventBus.GetStockTypeDetailOrDefault(saleTicketDto.StockTypeId);
            if (stockTypeInfo == null) return BadRequestResponse($"stockTypeId_ERR_INVALID_VALUE");
            else saleTicketDto.StockTypeName = stockTypeInfo.Name;

            //kiểm tra số lượng CP có hợp lệ hay không
            if (!saleTicketDto.IsNegotiate)
            {
                var systemSetting = await SystemSettingDB.GetTransactionMultiple();
                if (systemSetting != null && saleTicketDto.Quantity % systemSetting != 0)
                {
                    return BadRequestResponse("quantity_ERR_TRANS_MULTIPLES");
                }
            }

            var context = new StockDealServiceContext();
            var ticket = await context.SaleTickets
                .Where(e => e.CreatedBy == loginContactId)
                .Where(e => e.Id == saleTicketDto.Id.Value)
                .FirstOrDefaultAsync();

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
            //kiểm tra số lượng CP có hợp lệ hay không
            if (buyTicketDto.Quantity != null)
            {
                var systemSetting = await SystemSettingDB.GetTransactionMultiple();
                if (systemSetting != null && buyTicketDto.Quantity % systemSetting != 0)
                {
                    return BadRequestResponse("quantity_ERR_TRANS_MULTIPLES");
                }
            }

            var context = new StockDealServiceContext();
            var ticket = await context.BuyTickets
                .Where(e => e.CreatedBy == loginContactId)
                .Where(e => e.Id == buyTicketDto.Id.Value)
                .Include(x => x.BuyTicketDetails)
                .FirstOrDefaultAsync();

            if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

            ticket.StockCodes = string.Join(",", buyTicketDto.StockCode);
            ticket.ModifiedBy = loginContactId;
            ticket.ModifiedDate = DateTime.Now;

            //cập nhật danh sách mã CP

            //danh sách các mã CP đang có trong hệ thống
            var stocks = await CallEventBus.GetStockList(true);

            //các mã CP được xóa đi
            var stockCodeDeleted = ticket.BuyTicketDetails.Where(x => !buyTicketDto.StockCode.Contains(x.StockCode)).ToList();
            context.RemoveRange(stockCodeDeleted);

            //các mã CP được cập nhật hoặc thêm mới
            var stockCodeUpdate = ticket.BuyTicketDetails.Where(x => buyTicketDto.StockCode.Contains(x.StockCode)).ToList();

            foreach (var item in buyTicketDto.StockCode)
            {
                var stock = stocks.Count > 0 ? stocks.FirstOrDefault(x => x.StockCode == item) : null;
                if (stock == null)
                {
                    return BadRequestResponse("stockCode_ERR_INVALID_VALUE", item);
                }

                var detailDB = stockCodeUpdate.Count > 0 ? stockCodeUpdate.FirstOrDefault(x => x.StockCode == item) : null;

                if(detailDB == null)
                {
                    var buyTicketDetail = new BuyTicketDetail()
                    {
                        Id = Guid.NewGuid(),
                        BuyTicketId = ticket.Id,
                        StockId = stock.Id,
                        StockCode = stock.StockCode,
                        PriceFrom = buyTicketDto.PriceFrom,
                        PriceTo = buyTicketDto.PriceTo,
                        Quantity = buyTicketDto.Quantity,
                        IsNegotiate = buyTicketDto.IsNegotiate,
                        CreatedBy = loginContactId
                    };
                    context.Add(buyTicketDetail);
                }
                else
                {
                    detailDB.PriceFrom = buyTicketDto.PriceFrom;
                    detailDB.PriceTo = buyTicketDto.PriceTo;
                    detailDB.Quantity = buyTicketDto.Quantity;
                    detailDB.IsNegotiate = buyTicketDto.IsNegotiate;
                    detailDB.ModifiedBy = loginContactId;
                    detailDB.ModifiedDate = DateTime.Now;

                    context.Update(detailDB);
                }
            }

            var ticketDb = context.Update(ticket);
            ticketDb.CurrentValues.SetValues(buyTicketDto);

            await context.SaveChangesAsync();

            return SuccessResponse(buyTicketDto.Id);
        }



        /// <summary>
        /// lấy chi tiết tin bằng id
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <returns></returns>
        public async Task<BaseResponse> GetTicketAsync(Guid ticketId, Guid loginContactId)
        {

            var ticket = await GetTicketAsync(ticketId, TicketType.Buy, loginContactId);

            if (ticket == null) ticket = await GetTicketAsync(ticketId, TicketType.Sale, loginContactId);

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
                //.Where(e => e.Status == 1)
                .Where(e => e.CreatedBy == loginContactId)
                .Where(e => e.Id == ticketId)
                .FirstOrDefaultAsync();

            if (ticket == null) return NotFoundResponse();

            ticket.Status = 0;
            ticket.DeletedDate = DateTime.Now;
            ticket.DeletedBy = loginContactId;

            await context.SaveChangesAsync();

            return SuccessResponse(data: ticketId);
        }



        /// <summary>
        /// Danh sách tin mua bán
        /// </summary>
        /// <param name="listTicketDto"></param>
        /// <param name="loginContactId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> ListTicketsAsync(TicketSearchCriteria listTicketDto, Guid loginContactId)
        {
            if (listTicketDto.CreatedDateFrom != null || listTicketDto.CreatedDateTo != null)
            {
                if (listTicketDto.CreatedDateFrom == null && listTicketDto.CreatedDateTo != null)
                {
                    return BadRequestResponse("createdDateFrom_ERR_REQUIRED");
                }
                if (listTicketDto.CreatedDateFrom != null && listTicketDto.CreatedDateTo == null)
                {
                    return BadRequestResponse("createdDateTo_ERR_REQUIRED");
                }
            }

            if (listTicketDto.ModifiedDateFrom != null || listTicketDto.ModifiedDateTo != null)
            {
                if (listTicketDto.ModifiedDateFrom == null && listTicketDto.ModifiedDateTo != null)
                {
                    return BadRequestResponse("modifiedDateFrom_ERR_REQUIRED");
                }
                if (listTicketDto.ModifiedDateFrom != null && listTicketDto.ModifiedDateTo == null)
                {
                    return BadRequestResponse("modifiedDateTo_ERR_REQUIRED");
                }
            }

            PaginateDto paginate = new();

            if (listTicketDto.TicketType == (int)TicketType.Buy)
            {
                var query = await TicketDB.ListBuyTicketAsync(listTicketDto, loginContactId);

                paginate.TotalItems = query.Select(e => e.TotalCount).FirstOrDefault();
                paginate.Data = query;
            }
            else
            {
                var query = await TicketDB.ListSaleTicketAsync(listTicketDto, loginContactId);

                paginate.TotalItems = query.Select(e => e.TotalCount).FirstOrDefault();
                paginate.Data = query;
            }

            paginate.CurrentPage = listTicketDto.IsPaging ? listTicketDto.CurrentPage : 1;
            paginate.PerPage = listTicketDto.IsPaging ? listTicketDto.PerPage : paginate.TotalItems;


            return SuccessResponse(data: paginate);
        }


        /// <summary>
        /// Xóa nhiều tin mua bán hoặc tất cả bằng storeproduce
        /// </summary>
        /// <param name="deleteTicketsDto"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<BaseResponse> UpdateTicketStatusAsync(UpdateTicketsStatusDto updateTicketsDto, Guid userId)
        {
            if (!updateTicketsDto.IsDelete && updateTicketsDto.Status != (int)TicketStatus.Show && updateTicketsDto.Status != (int)TicketStatus.HideAdmin)
            {
                return BadRequestResponse("status_ERR_INVALID_VALUE");
            }

            if (!updateTicketsDto.IsDelete && updateTicketsDto.Status == (int)TicketStatus.HideAdmin && string.IsNullOrWhiteSpace(updateTicketsDto.Reason))
            {
                return BadRequestResponse("reason_ERR_REQUIRED");
            }

            var context = new StockDealServiceContext();
            var adminHiddenTickets = new List<AdminHiddenTicketDto>();

            var tickets = await context.Tickets.Where(x => updateTicketsDto.ticketIds.Contains(x.Id) && x.DeletedDate == null).ToListAsync();

            if (tickets.Count != updateTicketsDto.ticketIds.Count)
            {
                var idNotFound = updateTicketsDto.ticketIds.Where(x => !tickets.Select(x => x.Id).Contains(x)).ToList();
                return BadRequestResponse("ticketId_ERR_DATA_NOT_FOUND", idNotFound);
            }

            if (updateTicketsDto.IsDelete)
            {
                await TicketDB.DeleteTickets(userId, updateTicketsDto.ticketIds);
            }
            else
            {
                if (updateTicketsDto.Status == (int)TicketStatus.Show)
                {
                    updateTicketsDto.Reason = "";
                };

                //lấy id không tìm thấy được thông tin ra
                foreach (var ticket in tickets)
                {
                    //muốn hiển thị tin tức đã bị ẩn mà không phải do phía admin ẩn
                    if (updateTicketsDto.Status == 1 && ticket.Status != 2)
                    {
                        return BadRequestResponse("ticketId_ERR_STATUS_NOT_CHANGE", ticket.Id);
                    }
                    ticket.ModifiedDate = DateTime.Now;
                    ticket.ModifiedBy = userId;
                    ticket.Status = updateTicketsDto.Status;
                    ticket.Reason = updateTicketsDto.Reason;

                    var adminHiddenTicket = adminHiddenTickets.FirstOrDefault(x => x.UserId == ticket.CreatedBy);
                    if (adminHiddenTicket != null)
                    {
                        adminHiddenTicket.TicketId.Add(ticket.Id);
                    }
                    else
                    {
                        if (ticket.CreatedBy.HasValue)
                        {
                            adminHiddenTicket = new AdminHiddenTicketDto()
                            {
                                UserId = ticket.CreatedBy.Value
                            };
                            adminHiddenTicket.TicketId.Add(ticket.Id);
                            adminHiddenTickets.Add(adminHiddenTicket);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();

            #region gửi thông báo có tin đăng liên quan
            if (!updateTicketsDto.IsDelete)
            {
                await CallEventBus.NotificationAdminHiddenTicketAsync(adminHiddenTickets, false);
            }
            #endregion

            return SuccessResponse();
        }

    }
}
