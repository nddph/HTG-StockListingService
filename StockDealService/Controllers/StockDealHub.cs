﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockDealBusiness.Business;
using StockDealDal.Dto;
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
        private readonly static ConcurrentDictionary<Guid, string> _connections = new();

        private readonly StockDealHubBusiness _chatHubBusiness;
        private readonly StockDealCoreBusiness _stockDealCoreBusiness;
        private readonly ILogger _logger;



        protected string LoginedContactFullName
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
                    Guid.TryParse(Context.User.Claims.FirstOrDefault(claim => claim.Type == "contactId")?.Value, out id);
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



        public async Task SendMessage([Required] Guid groupId, [Required] CreateStockDetailDto input)
        {
            try
            {
                var userId = LoginedContactId;

                input.SenderName = LoginedContactFullName;

                var stockDetail = await _stockDealCoreBusiness.CreateStockDealDetailAsync(groupId, userId, input);
                if (stockDetail?.StatusCode != 200) return;
                
                var data = await _chatHubBusiness.GetStockDetailAsync((Guid)stockDetail.Data);
                await Clients.Group(groupId.ToString()).SendAsync(groupId.ToString(), JsonConvert.SerializeObject(data));

                var group = await _chatHubBusiness.GetStockDealAsync(groupId);
                if (userId == group.SenderId)
                {
                    if (!_connections.ContainsKey(group.ReceiverId))
                    {
                        Console.WriteLine("send notification");
                    }
                } else
                {
                    if (!_connections.ContainsKey(group.SenderId))
                    {
                        Console.WriteLine("send notification");
                    }
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

                var userId = LoginedContactId;

                var stockDealId = Guid.Parse(Context.GetHttpContext().Request.Query["stockDealId"]);

                var response = await _stockDealCoreBusiness.GetStockDealAsync(stockDealId);

                if (response.StatusCode != 200) throw new Exception("DealNotFound");

                var room = response.Data as StockDeal;

                if (!(room.SenderId.Equals(userId) || room.ReceiverId.Equals(userId))) throw new Exception("UserNotInDeal");

                _connections.TryAdd(userId, Context.ConnectionId);

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

                var stockDealId = Guid.Parse(Context.GetHttpContext().Request.Query["stockDealId"]);

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, stockDealId.ToString());

                _connections.TryRemove(userId, out _);

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