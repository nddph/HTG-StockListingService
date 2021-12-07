using Microsoft.AspNetCore.SignalR;
using StockDealBusiness.Business;
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

        public ChatHub()
        {
            _chatHubBusiness = new();
        }



        public async Task SendMessage([Required] Guid groupId, [Required] Guid userId, [Required] string message)
        {
            if (await _chatHubBusiness.CreateStockDetail(groupId, userId, message))
            {
                await Clients.Group(groupId.ToString()).SendAsync(groupId.ToString(), userId, message);
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
