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

            //using var _context = new StockDealServiceContext();

            //var group = await _context.StockDeals.FindAsync(groupId);
            //if (group == null) return;
            //if (!(userId == group.SenderId || userId == group.ReceiverId)) return;

            await Clients.Group(groupId.ToString()).SendAsync(groupId.ToString(), userId, message);

            //StockDealDetail stockDealDetail = new()
            //{
            //    Id = Guid.NewGuid(),
            //    SenderId = userId,
            //    Message = message,
            //    StockDetailId = groupId,
            //    CreatedDate = DateTime.Now
            //};
            //_context.Add(stockDealDetail);
            //await _context.SaveChangesAsync();
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
