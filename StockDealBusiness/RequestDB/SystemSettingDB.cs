using NewsCommon.Utilities;
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

            var res = await systemSettingBusiness.GetSystemSettingItem(Constants.TicketExpDate, 0);

            if (res == null) return 0;
            if (int.TryParse(res.DisplayValue, out int expDate)) return expDate;
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

            var res = await systemSettingBusiness.GetSystemSettingItem(Constants.AllowCreateBuyTicket, 0);

            if (res == null) return false;
            if (int.TryParse(res.DisplayValue, out int isAllow)) return isAllow == 1;
            return false;
        }

        /// <summary>
        /// lấy bội số cho phép khi giao dịch
        /// </summary>
        /// <returns></returns>
        public static async Task<int?> GetTransactionMultiple()
        {
            SystemSettingBusiness systemSettingBusiness = new();

            var systemSetting = await systemSettingBusiness.GetSystemSettingItem(Constants.TransactionMultiple, 1);

            if (systemSetting != null && !string.IsNullOrWhiteSpace(systemSetting.DisplayValue))
            {
                return Int32.Parse(systemSetting.DisplayValue);
            }

            return null;
        }

        /// <summary>
        /// số tin rao vặt tối đa được đăng mỗi ngày của 1 người
        /// </summary>
        /// <returns></returns>
        public static async Task<int?> GetMinTicketDaily()
        {
            SystemSettingBusiness systemSettingBusiness = new();

            var systemSetting = await systemSettingBusiness.GetSystemSettingItem(Constants.MinTicketDaily, 1);

            if (systemSetting != null && !string.IsNullOrWhiteSpace(systemSetting.DisplayValue))
            {
                return Int32.Parse(systemSetting.DisplayValue);
            }

            return null;
        }

        /// <summary>
        /// ds user bị chặn bán
        /// </summary>
        /// <returns></returns>
        public static async Task<List<string>> GetUserForbiddenSale()
        {
            SystemSettingBusiness systemSettingBusiness = new();

            var list = new List<string>();

            var systemSetting = await systemSettingBusiness.GetSystemSettingItem(Constants.ForbiddenSale, 0);

            if (systemSetting != null && !string.IsNullOrWhiteSpace(systemSetting.DisplayValue))
            {
                list = systemSetting.DisplayValue.Split(";").ToList();
            }

            return list;
        }

    }
}
