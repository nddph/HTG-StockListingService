using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockDealBusiness.Business;
using StockDealBusiness.EventBus;
using StockDealDal.Dto;
using StockDealDal.Dto.EventBus;
using StockDealDal.Dto.StockDeal;
using StockDealDal.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockDealService.Controllers
{
    [Authorize]
    public class StockDealHub : Hub
    {
        private readonly static ConcurrentDictionary<Guid, Guid> _userOnlineDeal = new();

        private readonly StockDealHubBusiness _chatHubBusiness;
        private readonly StockDealCoreBusiness _stockDealCoreBusiness;
        private readonly ILogger _logger;



        private Guid GetStockDealId()
        {
            return Guid.Parse(Context.GetHttpContext().Request.Query["stockDealId"]);
        }



        private string LoginedContactFullName
        {
            get
            {
                return Context.User?.Claims?.FirstOrDefault(claim => claim.Type == "fullName")?.Value;
            }
        }



        private Guid LoginedContactId
        {
            get
            {
                var id = Guid.Empty;
                if (Context.User.Identity.IsAuthenticated)
                {
                    _ = Guid.TryParse(Context.User.Claims.FirstOrDefault(claim => claim.Type == "contactId")?.Value, out id);
                }
                return id;
            }
        }



        public StockDealHub(ILogger<StockDealHub> logger)
        {
            _chatHubBusiness = new();
            _stockDealCoreBusiness = new();
            _logger = logger;
        }



        [HubMethodName("DeleteStockDealDetail")]
        public async Task<BaseResponse> DeleteStockDealDetail([Required] Guid? stockDealDeailId)
        {
            try
            {
                var response = await _stockDealCoreBusiness.DeleteStockDetailAsync(stockDealDeailId.Value, LoginedContactId);

                if (response.StatusCode != 200) return response;

                var groupId = GetStockDealId();
                await Clients.Group(groupId.ToString()).SendAsync("DeleteStockDealDetail", stockDealDeailId);

                return new BaseResponse();

            } catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new BaseResponse() { StatusCode = 400, Message = e.Message };
            }
        }



        [HubMethodName("CreateStockDealDetail")]
        public async Task<BaseResponse> CreateStockDealDetail([Required] CreateStockDetailDto input)
        {
            try
            {
                var groupId = GetStockDealId();
                var userId = LoginedContactId;

                input.SenderName = LoginedContactFullName;

                // tạo tin nhắn
                var stockDetail = await _stockDealCoreBusiness.CreateStockDealDetailAsync(groupId, userId, input);
                if (stockDetail?.StatusCode != 200) return stockDetail;

                // đánh dấu người gửi đã đọc tin nhắn
                await _stockDealCoreBusiness.ReadStockDealDetailAsync(groupId, userId);

                // gửi tin nhắn
                var data = await _chatHubBusiness.GetStockDetailAsync((Guid)stockDetail.Data);
                await Clients.Group(groupId.ToString()).SendAsync("CreateStockDealDetail", JsonConvert.SerializeObject(data));

                #region kiểm tra người nhận offline để đẩy thông báo
                SendDealNofifyDto sendDealNofify = null;

                var group = await _chatHubBusiness.GetStockDealAsync(groupId);

                Guid groupIdReceiverOnline;

                if (userId == group.SenderId)
                {
                    _userOnlineDeal.TryGetValue(group.ReceiverId, out groupIdReceiverOnline);

                    if (groupIdReceiverOnline != groupId)
                    {
                        sendDealNofify = new()
                        {
                            SenderId = group.SenderId,
                            SenderName = group.SenderName,
                            ReceiverId = group.ReceiverId,
                            ReceiverName = group.ReceiverName,
                            StockCodes = group.Ticket?.Code,
                            StockDealId = group.Id
                        };
                    }

                } else if (userId == group.ReceiverId)
                {
                    _userOnlineDeal.TryGetValue(group.SenderId, out groupIdReceiverOnline);

                    if (groupIdReceiverOnline != groupId)
                    {
                        sendDealNofify = new()
                        {
                            SenderId = group.ReceiverId,
                            SenderName = group.ReceiverName,
                            ReceiverId = group.SenderId,
                            ReceiverName = group.SenderName,
                            StockCodes = group.Ticket.Code,
                            StockDealId = group.Id
                        };
                    }
                }

                if (sendDealNofify != null) await CallEventBus.SendDealNofify(sendDealNofify);
                #endregion

                return new BaseResponse();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new BaseResponse() { StatusCode = 400, Message = e.Message };
            }
        }



        public override async Task<Task> OnConnectedAsync()
        {
            try
            {

                var userId = LoginedContactId;

                var stockDealId = GetStockDealId();

                // kiểm tra tồn tại stockdeal
                var response = await _stockDealCoreBusiness.GetStockDealAsync(stockDealId);
                if (response.StatusCode != 200)
                {
                    _logger.LogError($"not found stockdeal {stockDealId}");
                    Context.Abort();
                    return Task.CompletedTask;
                }

                // kiểm tra người dùng có trong stockdeal
                var room = response.Data as StockDeal;
                if (!(room.SenderId.Equals(userId) || room.ReceiverId.Equals(userId)))
                {
                    _logger.LogError($"user {userId} not in stockdeal {stockDealId}");
                    Context.Abort();
                    return Task.CompletedTask;
                }

                _userOnlineDeal.TryAdd(userId, stockDealId);

                await Groups.AddToGroupAsync(Context.ConnectionId, stockDealId.ToString());

                return base.OnConnectedAsync();

            } catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
            
        }



        public override async Task<Task> OnDisconnectedAsync(Exception exception)
        {

            try
            {
                var userId = LoginedContactId;

                var stockDealId = GetStockDealId();

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, stockDealId.ToString());

                _userOnlineDeal.TryRemove(userId, out _);

                return base.OnDisconnectedAsync(exception);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                throw;
            }
        }
    }
}
