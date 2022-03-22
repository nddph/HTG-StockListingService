using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemSettingSharing.Entities;

namespace StockDealBusiness.RequestDB
{
    public class SystemSettingDB
    {
        private async Task<SysSetting> GetValueBuyKeyAsync(string key)
        {
            var syscontext = new SystemSettingContext();
            var res = await syscontext.SystemSettings.Where(e => e.Key == key).FirstOrDefaultAsync();
            return res;
        }



        /// <summary>
        /// số ngày tin đăng đc hiển thị từ systemsetting
        /// </summary>
        /// <returns></returns>
        public static async Task<int> GetTicketExpDateAsync()
        {
            var systemSettingDB = new SystemSettingDB();
            var res = await systemSettingDB.GetValueBuyKeyAsync("Ticket.ExpDate");
            if (res == null) return 0;
            if (int.TryParse(res.Value, out int expDate)) return expDate;
            return 0;
        }



        /// <summary>
        /// cho phép người dùng không phải nhân viên đăng tin mua
        /// 1: có
        /// 0: không
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> AllowCreateBuyTicketAsync()
        {
            var systemSettingDB = new SystemSettingDB();
            var res = await systemSettingDB.GetValueBuyKeyAsync("AllowCreateBuyTicket");
            if (res == null) return false;
            if (int.TryParse(res.Value, out int isAllow)) return isAllow == 1;
            return false;
        }
    }
}
