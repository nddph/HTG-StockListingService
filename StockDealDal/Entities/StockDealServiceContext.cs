using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuyTicket>().Property(e => e.StockCode).HasConversion(
                e => JsonConvert.SerializeObject(e),
                e => JsonConvert.DeserializeObject<List<string>>(e));
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.LogTo(Console.WriteLine);
                optionsBuilder.UseSqlServer(GetSessionByName("StockDealConn", "ConnectionStrings"));
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



        //[DbFunction("ListTickets")]
        //public IQueryable<Ticket> ListTickets(List<string> stockCode, int? ticketType = null, int? status = null,
        //    int? quantityFrom = null, int? quantityTo = null,
        //    decimal? priceFrom = null, decimal? priceTo = null)
        //=> FromExpression(() => ListTickets(stockCode, ticketType, status,
        //    quantityFrom, quantityTo, priceFrom, priceTo));
    }
}
