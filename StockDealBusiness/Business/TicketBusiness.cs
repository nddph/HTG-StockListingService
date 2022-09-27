using Microsoft.EntityFrameworkCore;
using StockDealBusiness.EventBus;
using StockDealBusiness.RequestDB;
using StockDealCommon;
using StockDealDal.Dto;
using StockDealDal.Dto.EventBus;
using StockDealDal.Dto.StockDeal;
using StockDealDal.Dto.Ticket;
using StockDealDal.Entities;
using System;
using System.Collections;
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
        public async Task<ViewTickets> GetTicketAsync(Guid ticketId, TicketType ticketType, Guid loginContactId)
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

            return (await TicketDB.ListTicketAsync(ticketSearchCriteria, loginContactId)).FirstOrDefault();
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
            var listBuyTicket = await TicketDB.ListTicketAsync(listTicketDto, loginContactId);

            listTicketDto.TicketType = (int)TicketType.Sale;
            var listSaleTicket = await TicketDB.ListTicketAsync(listTicketDto, loginContactId);

            return SuccessResponse(new CountTicketResponseDto 
            { 
                CountBuyTicket = listBuyTicket.Select(e => e.TotalCount).FirstOrDefault(),
                CountSaleTicket = listSaleTicket.Select(e => e.TotalCount).FirstOrDefault()
            });
        }

        /// <summary>
        /// tạo tin mua bán cổ phiếu
        /// </summary>
        /// <param name="ticketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public async Task<BaseResponse> CreateTicketAsync(CreateTicketDto ticketDto, Guid loginContactId, TicketType ticketType)
        {
            var context = new StockDealServiceContext();

            //kiểm tra xem quá số lượng cho phép đăng tin 1 ngày hay không
            var today = DateTime.Now.Date;
            var totalTicketDaily = await context.Tickets.Where(x => x.CreatedDate.HasValue && x.CreatedDate.Value.Date == today && x.CreatedBy == loginContactId && x.DeletedDate == null).CountAsync();
            var minTicketDaily = await SystemSettingDB.GetMinTicketDaily();
            if (minTicketDaily != null && totalTicketDaily >= minTicketDaily && minTicketDaily > 0)
            {
                return BadRequestResponse("ticket_ERR_MIN_TICKET_DAILY", minTicketDaily);
            }

            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            if (ticketType == TicketType.Buy)
            {
                // cho phép người dùng không phải nhân viên đăng bài
                if (await SystemSettingDB.AllowCreateBuyTicketAsync() == false)
                {
                    // nếu không phải nhân viên thì không cho đăng
                    if (stockHolderInfo.Status == 0) return BadRequestResponse("user_ERR_ONLY_STAFF_CREATE_BUY_TICKET");
                }
            }

            if (ticketType == TicketType.Sale)
            {
                var stockLimit = await CallEventBus.GetStockHolderLimitAsync(loginContactId, ticketDto.StockId.Value, ticketDto.StockTypeId.Value);
                if (ticketDto.Quantity.Value > stockLimit) return BadRequestResponse($"quantity_ERR_INVALID_VALUE");
            }

            var stockInfo = await CallEventBus.GetStockDetailById(ticketDto.StockId.Value);
            if (stockInfo == null) return BadRequestResponse($"stockId_ERR_INVALID_VALUE");

            var stockTypeInfo = await CallEventBus.GetStockTypeDetailOrDefault(ticketDto.StockTypeId);
            if (stockTypeInfo == null) return BadRequestResponse($"stockTypeId_ERR_INVALID_VALUE");

            //kiểm tra hạn mức giao dịch tin bán
            var stockPolicyList = await CallEventBus.GetStockPolicyList(ticketDto.StockId.GetValueOrDefault(), ticketDto.StockTypeId.GetValueOrDefault());
            if (stockPolicyList != null && stockPolicyList.Count > 0)
            {
                var stockPolicy = stockPolicyList.OrderBy(x => x.EffectDate).FirstOrDefault();
                if (ticketDto.Quantity.Value < stockPolicy.MinSaleTrans)
                {
                    return BadRequestResponse("quantity_ERR_LOW_THAN_POLICY", stockPolicy.MinSaleTrans);
                }
            }

            //kiểm tra số lượng CP có hợp lệ hay không
            if (!ticketDto.IsNegotiate)
            {
                var systemSetting = await SystemSettingDB.GetTransactionMultiple();
                if (systemSetting != null && ticketDto.Quantity % systemSetting != 0)
                {
                    return BadRequestResponse("quantity_ERR_TRANS_MULTIPLES");
                }
            }

            Guid ticketId;

            if (ticketType == TicketType.Sale)
            {
                var ticket = context.Add(new SaleTicket
                {
                    Id = Guid.NewGuid(),
                    FullName = stockHolderInfo.FullName,
                    CreatedBy = loginContactId,
                    CreatedDate = DateTime.Now,
                    Status = 1,
                    ExpDate = DateTime.Now.AddDays(await SystemSettingDB.GetTicketExpDateAsync()).Date,
                    Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}",
                    Email = stockHolderInfo.WorkingEmail,
                    EmployeeCode = stockHolderInfo.EmployeeCode,
                    Phone = stockHolderInfo.Phone,
                    StockCode = stockInfo.StockCode,
                    StockTypeName = stockTypeInfo.Name
                });

                ticket.CurrentValues.SetValues(ticketDto);

                ticketId = ticket.Entity.Id;
            }
            else
            {
                var ticket = context.Add(new BuyTicket
                {
                    Id = Guid.NewGuid(),
                    FullName = stockHolderInfo.FullName,
                    CreatedBy = loginContactId,
                    CreatedDate = DateTime.Now,
                    Status = 1,
                    ExpDate = DateTime.Now.AddDays(await SystemSettingDB.GetTicketExpDateAsync()).Date,
                    Code = $"TD{DateTime.Now:yyyyMMddHHmmssfff}",
                    Email = stockHolderInfo.WorkingEmail,
                    EmployeeCode = stockHolderInfo.EmployeeCode,
                    Phone = stockHolderInfo.Phone,
                    StockCodes = string.Join(",", stockInfo.StockCode)
                });

                ticket.CurrentValues.SetValues(ticketDto);

                ticketId = ticket.Entity.Id;

                var buyTicketDetail = new BuyTicketDetail()
                {
                    Id = Guid.NewGuid(),
                    BuyTicketId = ticket.Entity.Id,
                    StockId = stockInfo.Id,
                    StockCode = stockInfo.StockCode,
                    StockTypeId = stockTypeInfo.Id,
                    StockTypeName = stockTypeInfo.Name,
                    PriceFrom = ticketDto.PriceFrom,
                    PriceTo = ticketDto.PriceTo,
                    Quantity = ticketDto.Quantity,
                    IsNegotiate = ticketDto.IsNegotiate,
                    CreatedBy = loginContactId,
                    CreatedDate = DateTime.Now
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
                TicketType = ticketType == TicketType.Sale ? (int)TicketType.Buy : (int)TicketType.Sale,
                DelTicketStatus = 1,
                ExpTicketStatus = 1,
                QuantityStatus = 2,
                StockTypeIds = new List<string>() { Convert.ToString(stockTypeInfo.Id) },
                StockCodes = new List<string>() { stockInfo.StockCode },
                Status = 1
            };
            var listUserRes = await ListTicketsAsync(ticketSearch, loginContactId);
            if (listUserRes.StatusCode == 200 && listUserRes.Data != null)
            {
                var listUser = (listUserRes.Data as PaginateDto).Data as List<ViewTickets>;

                SuggestTicketDto suggestTicketDto = new()
                {
                    UserId = loginContactId,
                    TicketId = ticketId,
                    ListReceiverUser = listUser.Select(e => e.CreatedBy.Value).Distinct().ToList(),
                    TicketType = ticketType == TicketType.Sale ? (int)TicketType.Sale : (int)TicketType.Buy,
                    StockCodes = stockInfo.StockCode,
                    Quantity = ticketDto.Quantity,
                    Price = ticketDto.PriceFrom,
                    IsNegotiate = ticketDto.IsNegotiate,
                    Title = ticketDto.Title
                };
                await CallEventBus.NotificationSuggestTicketAsync(suggestTicketDto, false);
            }
            #endregion

            return SuccessResponse(data: ticketId);
        }

        /// <summary>
        /// Cập nhật tin mua bán cổ phiếu
        /// </summary>
        /// <param name="ticketDto"></param>
        /// <param name="loginContactId"></param>
        /// <param name="ticketType"></param>
        /// <returns></returns>
        public async Task<BaseResponse> UpdateTicketAsync(UpdateTicketDto ticketDto, Guid loginContactId, TicketType ticketType)
        {
            var stockHolderInfo = await CallEventBus.GetStockHolderDetail(loginContactId);
            if (stockHolderInfo == null) return BadRequestResponse();

            if (ticketType == TicketType.Sale)
            {
                var stockLimit = await CallEventBus.GetStockHolderLimitAsync(loginContactId, ticketDto.StockId.Value, ticketDto.StockTypeId.Value);
                if (ticketDto.Quantity.Value > stockLimit) return BadRequestResponse($"quantity_ERR_INVALID_VALUE");
            }

            var stockInfo = await CallEventBus.GetStockDetailById(ticketDto.StockId.Value);
            if (stockInfo == null) return BadRequestResponse($"stockId_ERR_INVALID_VALUE");

            var stockTypeInfo = await CallEventBus.GetStockTypeDetailOrDefault(ticketDto.StockTypeId);
            if (stockTypeInfo == null) return BadRequestResponse($"stockTypeId_ERR_INVALID_VALUE");

            //kiểm tra hạn mức giao dịch tin bán
            var stockPolicyList = await CallEventBus.GetStockPolicyList(ticketDto.StockId.GetValueOrDefault(), ticketDto.StockTypeId.GetValueOrDefault());
            if (stockPolicyList != null && stockPolicyList.Count > 0)
            {
                var stockPolicy = stockPolicyList.OrderBy(x => x.EffectDate).FirstOrDefault();
                if (ticketDto.Quantity.Value < stockPolicy.MinSaleTrans)
                {
                    return BadRequestResponse("quantity_ERR_LOW_THAN_POLICY", stockPolicy.MinSaleTrans);
                }
            }

            //kiểm tra số lượng CP có hợp lệ hay không
            if (!ticketDto.IsNegotiate)
            {
                var systemSetting = await SystemSettingDB.GetTransactionMultiple();
                if (systemSetting != null && ticketDto.Quantity % systemSetting != 0)
                {
                    return BadRequestResponse("quantity_ERR_TRANS_MULTIPLES");
                }
            }

            var context = new StockDealServiceContext();

            if (ticketType == TicketType.Sale)
            {
                var ticket = await context.SaleTickets.Where(e => e.Id == ticketDto.Id && e.CreatedBy == loginContactId).FirstOrDefaultAsync();

                if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

                ticket.ModifiedBy = loginContactId;
                ticket.ModifiedDate = DateTime.Now;
                ticket.StockCode = stockInfo.StockCode;
                ticket.StockTypeName = stockTypeInfo.Name;

                var ticketDb = context.Update(ticket);
                ticketDb.CurrentValues.SetValues(ticketDto);
            }
            else
            {
                var ticket = await context.BuyTickets.Where(e => e.Id == ticketDto.Id && e.CreatedBy == loginContactId).Include(X => X.BuyTicketDetail).FirstOrDefaultAsync();

                if (ticket == null || ticket.DeletedDate.HasValue) return NotFoundResponse();

                ticket.StockCodes = string.Join(",", stockInfo.StockCode);
                ticket.ModifiedBy = loginContactId;
                ticket.ModifiedDate = DateTime.Now;

                ticket.BuyTicketDetail.StockId = stockInfo.Id;
                ticket.BuyTicketDetail.StockCode = stockInfo.StockCode;
                ticket.BuyTicketDetail.StockTypeId = stockTypeInfo.Id;
                ticket.BuyTicketDetail.StockTypeName = stockTypeInfo.Name;
                ticket.BuyTicketDetail.PriceFrom = ticketDto.PriceFrom;
                ticket.BuyTicketDetail.PriceTo = ticketDto.PriceTo;
                ticket.BuyTicketDetail.Quantity = ticketDto.Quantity;
                ticket.BuyTicketDetail.IsNegotiate = ticketDto.IsNegotiate;
                ticket.BuyTicketDetail.ModifiedBy = loginContactId;
                ticket.BuyTicketDetail.ModifiedDate = DateTime.Now;

                var ticketDb = context.Update(ticket);
                ticketDb.CurrentValues.SetValues(ticketDto);
            }

            await context.SaveChangesAsync();

            return SuccessResponse(data: ticketDto.Id);
        }

        /// <summary>
        /// lấy chi tiết tin bằng id
        /// </summary>
        /// <param name="buyTicketDto"></param>
        /// <returns></returns>
        public async Task<BaseResponse> GetTicketAsync(Guid ticketId, Guid loginContactId)
        {
            var ticket = await GetTicketAsync(ticketId, TicketType.Buy, loginContactId);

            if (ticket == null)
            {
                ticket = await GetTicketAsync(ticketId, TicketType.Sale, loginContactId);
            }

            if (ticket == null) return NotFoundResponse();

            //ktra nếu ticket đã bị ẩn thì chỉ có người đăng tin mới đc phép xem
            if (loginContactId == Guid.Empty || loginContactId != ticket.CreatedBy)
            {
                if (ticket.Status != 1 || ticket.DeletedDate != null ||
                      (ticket.IsExpTicket.HasValue && ticket.IsExpTicket.Value))
                {
                    return BadRequestResponse("ticket_ERR_HIDDEN");
                }
            }

            //lấy thông tin các deal theo tin tức này
            var stockDealSearch = new StockDealSearchCriteria()
            {
                TicketId = ticketId,
                LoginedContactId = loginContactId,
                CurrentPage = 1,
                PerPage = 1000
            };
            var list = await StockDealDB.ListStockDealAsync(stockDealSearch);
            var listResult = list.Select(e => new StockDealResponseDto(e)).ToList();
            ticket.StockDeals = listResult.Where(x => x.DealDetailNotByUser.LastStockDetailId != null).OrderByDescending(x => x.DealDetailNotByUser.Quantity).ToList();
            return SuccessResponse(ticket);
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

            var tickets = await TicketDB.ListTicketAsync(listTicketDto, loginContactId);

            if (tickets.Count == 0) return NotFoundResponse();

            PaginateDto pagination = new()
            {
                CurrentPage = listTicketDto.IsPaging ? listTicketDto.CurrentPage : 1,
                TotalItems = tickets.FirstOrDefault().TotalCount,
                Data = tickets
            };

            pagination.PerPage = listTicketDto.IsPaging ? listTicketDto.PerPage : pagination.TotalItems;

            return SuccessResponse(pagination);
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
