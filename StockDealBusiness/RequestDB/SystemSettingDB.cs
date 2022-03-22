using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemSettingSharing.SystemSettingBusiness;

namespace StockDealBusiness.RequestDB
{
    public class SystemSettingDB
    {

        /// <summary>
        /// số ngày tin đăng đc hiển thị từ systemsetting
        /// </summary>
        /// <returns></returns>
        public static async Task<int> GetTicketExpDateAsync()
        {
            SystemSettingBusiness systemSettingBusiness = new();

            var res = await systemSettingBusiness.GetSystemSettingItem("Ticket.ExpDate", 0);

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
            SystemSettingBusiness systemSettingBusiness = new();

            var res = await systemSettingBusiness.GetSystemSettingItem("AllowCreateBuyTicket", 0);

            if (res == null) return false;
            if (int.TryParse(res.Value, out int isAllow)) return isAllow == 1;
            return false;
        }
    }
}
