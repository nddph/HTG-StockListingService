using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockDealBusiness.Business;
using StockDealDal.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace StockDealService.Controllers
{
    public class ChatHub : Hub
    {
        private readonly ChatHubBusiness _chatHubBusiness;
        private readonly StockDealCoreBusiness _stockDealCoreBusiness;
        private readonly ILogger _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _chatHubBusiness = new();
            _stockDealCoreBusiness = new();
            _logger = logger;
        }



        public async Task SendMessage([Required] Guid groupId, [Required] CreateStockDetailDto input)
        {
            try
            {
                var userId = Guid.Empty;

                var stockDetail = await _stockDealCoreBusiness.CreateStockDealDetailAsync(groupId, userId, input);
                if (stockDetail?.StatusCode == 200)
                {
                    await Clients.Group(groupId.ToString()).SendAsync(groupId.ToString(), JsonConvert.SerializeObject(stockDetail));
                }
            } catch(Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }



        public override async Task<Task> OnConnectedAsync()
        {
            try
            {
                var userId = Guid.Empty;

                var rooms = (await _chatHubBusiness.ListStockDealAsync(userId)).Select(e => e.Id);

                foreach (var room in rooms)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, room.ToString());
                }

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
                var userId = Guid.Empty;

                var rooms = (await _chatHubBusiness.ListStockDealAsync(userId)).Select(e => e.Id);

                foreach (var room in rooms)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.ToString());
                }

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
