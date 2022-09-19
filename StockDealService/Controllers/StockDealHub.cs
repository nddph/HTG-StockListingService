using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StockDealBusiness.Business;
using StockDealBusiness.EventBus;
using StockDealCommon;
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

        private readonly StockDealHubBusiness _stockDealHubBusiness;
        private readonly StockDealCoreBusiness _stockDealCoreBusiness;
        private readonly ILogger _logger;

        private Guid GetStockDealId()
        {
            Context.GetHttpContext().Request.Query.TryGetValue("stockDealId", out StringValues stockDealIds);

            _ = Guid.TryParse(stockDealIds.FirstOrDefault(), out Guid stockDealId);

            return stockDealId;
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
            _stockDealHubBusiness = new();
            _stockDealCoreBusiness = new();
            _logger = logger;
        }

        [HubMethodName(ConstStockDealHub.DeleteStockDealDetail)]
        public async Task<BaseResponse> DeleteStockDealDetail([Required] Guid? stockDealDeailId)
        {
            try
            {
                var response = await _stockDealCoreBusiness.DeleteStockDetailAsync(stockDealDeailId.Value, LoginedContactId);

                if (response.StatusCode != 200) return response;

                var groupId = GetStockDealId();
                await Clients.Group(groupId.ToString()).SendAsync(ConstStockDealHub.DeleteStockDealDetail, stockDealDeailId);

                return new BaseResponse();

            } catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new BaseResponse() { StatusCode = 400, Message = e.Message };
            }
        }

        [HubMethodName(ConstStockDealHub.CreateStockDealDetail)]
        public async Task<BaseResponse> CreateStockDealDetail([Required] CreateStockDetailDto input)
        {
            try
            {
                #region validate input
                var contextValidate = new ValidationContext(input, serviceProvider: null, items: null);
                var validationResults = new List<ValidationResult>();

                bool isValid = Validator.TryValidateObject(input, contextValidate, validationResults, true);

                if (!isValid)
                {
                    var invalid = validationResults.FirstOrDefault();
                    var key = validationResults.FirstOrDefault().MemberNames.FirstOrDefault();
                    return new BaseResponse
                    {
                        StatusCode = 400,
                        Message = $"{key.Substring(0, 1).ToLower()}{key.Substring(1)}_{invalid.ErrorMessage}"
                    };
                }
                #endregion


                var groupId = GetStockDealId();
                var userId = LoginedContactId;

                input.SenderName = LoginedContactFullName;

                // tạo tin nhắn
                var stockDetail = await _stockDealCoreBusiness.CreateStockDealDetailAsync(groupId, userId, input);
                if (stockDetail?.StatusCode != 200) return stockDetail;

                // đánh dấu người gửi đã đọc tin nhắn
                await _stockDealCoreBusiness.ReadStockDealDetailAsync(groupId, userId);

                // gửi tin nhắn
                var data = await _stockDealHubBusiness.GetStockDetailAsync(Guid.Parse(stockDetail.Data.ToString()));

                if (data.Type == (int)TypeStockDealDetail.WaitingForResponse)
                {
                    await Clients.Caller.SendAsync(ConstStockDealHub.CreateStockDealDetail, 
                        JsonConvert.SerializeObject(data, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }));
                } else
                {
                    await Clients.Group(groupId.ToString()).SendAsync(ConstStockDealHub.CreateStockDealDetail, JsonConvert.SerializeObject(data, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }));
                }

                // Gửi thông báo hoặc đánh dấu người nhận đã đọc tin nhắn
                await SendDealNotificationAsync(LoginedContactId, groupId, data);

                return new BaseResponse() { Data = data };
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return new BaseResponse() { StatusCode = 400, Message = e.Message };
            }
        }

        /// <summary>
        /// Gửi thông báo thương lượng
        /// </summary>
        /// <param name="loginContactId"></param>
        /// <param name="groupId"></param>
        /// <param name="stockDealDetail"></param>
        /// <returns></returns>
        private async Task<BaseResponse> SendDealNotificationAsync(Guid loginContactId, Guid groupId, StockDealDetail stockDealDetail)
        {
            var stockDealRes = await _stockDealCoreBusiness.GetStockDealAsync(groupId, loginContactId);
            if (stockDealRes.StatusCode != 200) return stockDealRes;

            var stockDeal = stockDealRes.Data as StockDealResponseDto;

            Guid senderId = stockDeal.SenderId;
            Guid receiverId = stockDeal.ReceiverId;
            string senderName = stockDeal.SenderName;
            string receiverName = stockDeal.ReceiverName;

            if (loginContactId != stockDeal.SenderId)
            {
                senderId = stockDeal.ReceiverId;
                receiverId = stockDeal.SenderId;
                senderName = stockDeal.ReceiverName;
                receiverName = stockDeal.SenderName;
            }

            _userOnlineDeal.TryGetValue(receiverId, out Guid groupIdReceiverOnline);

            // nếu người nhận online thì đánh dấu người nhận đã đọc tin nhắn, ngược lại thì gửi thông báo
            if (groupIdReceiverOnline == groupId)
            {
                await _stockDealCoreBusiness.ReadStockDealDetailAsync(groupId, receiverId);
                return new BaseResponse();
            }

            // gửi thông báo
            // chỉ gửi thông báo deal, ko gửi thông báo thương lượng và chờ phản hồi

            var stockCodes = "";
            if (stockDeal.Ticket != null) stockCodes = stockDeal.Ticket.StockCode;

            var sendDealNofifyDto = new SendDealNofifyDto()
            {
                SenderId = senderId,
                SenderName = senderName,
                ReceiverId = receiverId,
                ReceiverName = receiverName,
                StockCodes = stockCodes,
                StockDealId = groupId
            };

            if (stockDealDetail.Type == (int?)TypeStockDealDetail.DealDetail)
            {
                await CallEventBus.SendDealNofify(sendDealNofifyDto, false);
            }

            return new BaseResponse();
        }

        public override async Task<Task> OnConnectedAsync()
        {
            try
            {

                var userId = LoginedContactId;

                var stockDealId = GetStockDealId();

                if (userId == Guid.Empty || stockDealId == Guid.Empty)
                {
                    _logger.LogError($"ConnectionId: {Context.ConnectionId} | user: {userId} or stockDealId: {stockDealId} is empty");
                    Context.Abort();
                    return Task.CompletedTask;
                }

                // kiểm tra tồn tại stockdeal
                var room = await _stockDealHubBusiness.GetStockDealAsync(stockDealId);
                if (room == null)
                {
                    _logger.LogError($"ConnectionId: {Context.ConnectionId} | not found stockDealId: {stockDealId} for user: {userId}");
                    Context.Abort();
                    return Task.CompletedTask;
                }

                // kiểm tra người dùng có trong stockdeal
                if (!(room.SenderId.Equals(userId) || room.ReceiverId.Equals(userId)))
                {
                    _logger.LogError($"ConnectionId: {Context.ConnectionId} | user: {userId} not in stockDealId: {stockDealId}");
                    Context.Abort();
                    return Task.CompletedTask;
                }

                //_logger.LogInformation($"ConnectionId: {Context.ConnectionId} | user: {userId} connected stockDealId: {stockDealId}");

                _userOnlineDeal.AddOrUpdate(userId, stockDealId, (oldkey, oldvalue) => stockDealId);

                await Groups.AddToGroupAsync(Context.ConnectionId, stockDealId.ToString());

                // đánh dấu đã đọc tin nhắn
                await _stockDealCoreBusiness.ReadStockDealDetailAsync(stockDealId, LoginedContactId);  


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

                //_logger.LogInformation($"ConnectionId: {Context.ConnectionId} | user: {userId} disconnected stockDealId: {stockDealId}");

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
