using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StockDealDal.Dto.StockDeal;
using StockDealDal.Dto.Ticket;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockDealDal.Entities
{
    public class StockDealServiceContext : DbContext
    {

        public DbSet<StockDeal> StockDeals { get; set; }
        public DbSet<StockDealDetail> StockDealDetails { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<SaleTicket> SaleTickets { get; set; }
        public DbSet<BuyTicket> BuyTickets { get; set; }
        public DbSet<BuyTicketDetail> BuyTicketDetails { get; set; }


        [NotMapped]
        public DbSet<ViewTickets> ViewTickets { get; set;}
        public DbSet<ViewListStockDeals> ViewListStockDeals { get; set;}



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            { 
                optionsBuilder.UseSqlServer(GetSessionByName("StockDealConn", "ConnectionStrings"));

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    optionsBuilder.EnableSensitiveDataLogging();
                    optionsBuilder.LogTo(Console.WriteLine);
                    optionsBuilder.EnableDetailedErrors();
                }
            }
        }

        private static IConfiguration _configuration = null;
        public static string GetSessionByName(string key, string sessionName)
        {
            if (_configuration == null)
            {
                var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                _configuration = builder.Build();
            }
            return _configuration.GetSection(sessionName).GetValue<string>(key);
        }
        public static void ResetStaticConfig()
        {
            _configuration = null;
            GetSessionByName("", "");
        }



    }
}
