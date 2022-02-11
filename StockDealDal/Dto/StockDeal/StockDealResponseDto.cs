﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Dto.StockDeal
{
    public class StockDealResponseDto
    {

        public StockDealResponseDto() { }
        public StockDealResponseDto(ViewListStockDeals viewListStockDeal)
        {
            Id = viewListStockDeal.Id;
            SenderId = viewListStockDeal.SenderId;
            ReceiverId = viewListStockDeal.ReceiverId;
            SenderName = viewListStockDeal.SenderName;
            ReceiverName = viewListStockDeal.ReceiverName;
            ReceiverType = viewListStockDeal.ReceiverType;
            LastUpdate = viewListStockDeal.LastUpdate;
            CountUnread = viewListStockDeal.CountUnread;
            Ticket = new()
            {
                Id = viewListStockDeal.TicketId,
                Code = viewListStockDeal.TicketCode,
                TicketType = viewListStockDeal.TicketType,
                Title = viewListStockDeal.TicketTitle,
                IsNegotiate = viewListStockDeal.TicketIsNegotiate,
                PriceFrom = viewListStockDeal.TicketPrice,
                Quantity = viewListStockDeal.TicketQuantity,
                StockTypeName = viewListStockDeal.TicketStockTypeName,
                StockCode = viewListStockDeal.TicketStockCode,
                StockCodes = viewListStockDeal.TicketStockCodes,
                DeletedDate = viewListStockDeal.TicketDeletedDate
            };
            LastDealDetail = new()
            {
                LastStockDetailId = viewListStockDeal.LastStockDetailId,
                Description = viewListStockDeal.Description,
                Quantity = viewListStockDeal.Quantity,
                TotalPrice = viewListStockDeal.TotalPrice,
                UnitPrice = viewListStockDeal.UnitPrice,
                IsDeletedDealDetail = viewListStockDeal.IsDeletedDealDetail,
                IsOnwerLastDealDetail = viewListStockDeal.IsOnwerLastDealDetail,
                StockDetailType = viewListStockDeal.StockDetailType
            };
        }

        // deal
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public int? ReceiverType { get; set; }

        public DateTime? LastUpdate { get; set; }
        public int CountUnread { get; set; }

        public TicketStockDealResponseDto Ticket { get; set; }
        public DealDetailStockDealResponseDto LastDealDetail { get; set; }

    }

    public class DealDetailStockDealResponseDto
    {
        // deal detail
        public Guid? LastStockDetailId { get; set; }
        public int? StockDetailType { get; set; }
        public bool? IsOnwerLastDealDetail { get; set; }

        /// <summary>
        /// 1 deal gửi, 2 deal nhận
        /// </summary>
        public string Description { get; set; }
        public int? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public bool? IsDeletedDealDetail { get; set; }
    }

    public class TicketStockDealResponseDto
    {
        public Guid? Id { get; set; }

        public string Code { get; set; }

        public string Title { get; set; }

        public int? TicketType { get; set; }


        // sale ticket

        public string StockCode { get; set; }

        public string StockTypeName { get; set; }

        public bool? IsNegotiate { get; set; }

        public decimal? PriceFrom { get; set; }

        public int? Quantity { get; set; }

        // buy ticket
        public string StockCodes { get; set; }

        public DateTime? DeletedDate { get; set; }
    }
}
