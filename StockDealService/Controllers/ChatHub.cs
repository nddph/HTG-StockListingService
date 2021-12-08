using Microsoft.AspNetCore.SignalR;
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

        public ChatHub()
        {
            _chatHubBusiness = new();
            _stockDealCoreBusiness = new();
        }



        public async Task SendMessage([Required] Guid groupId, [Required] CreateStockDetailDto input)
        {
            if ((await _stockDealCoreBusiness.CreateStockDealDetailAsync(groupId, input))?.StatusCode == 200)
            {
                await Clients.Group(groupId.ToString()).SendAsync(groupId.ToString(), input);
            }
        }



        public override async Task<Task> OnConnectedAsync()
        {
            
            var userId = Guid.Empty;

            var rooms = (await _chatHubBusiness.ListStockDealAsync(userId)).Select(e => e.Id);

            foreach (var room in rooms)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, room.ToString());
            }

            return base.OnConnectedAsync();
        }



        public override async Task<Task> OnDisconnectedAsync(Exception exception)
        {

            var userId = Guid.Empty;

            var rooms = (await _chatHubBusiness.ListStockDealAsync(userId)).Select(e => e.Id);

            foreach (var room in rooms)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.ToString());
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
