using EamilWebApp.Models;
using log4net;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EamilWebApp
{
    public class PubBusiness:IPubBusiness
    {
        private readonly IList<AuthenticUserModel> authenticUserModelList;
        public PubBusiness(IList<AuthenticUserModel> _authenticUserModelList)
        {
            authenticUserModelList = _authenticUserModelList;
        }

        // 授權認證方法，驗證 mainComId 和 authHeader 是否有效
        public bool IsValidAuth(string mainComId, string authHeader)
        {
            // 1. 參數非空驗證
            if (string.IsNullOrEmpty(mainComId) || string.IsNullOrEmpty(authHeader))
                return false;

            // 2. 根據 mainComId 尋找對應的認證用戶
            var user = authenticUserModelList?.FirstOrDefault(u => u.MainComId == mainComId);
            if (user == null)
                return false;

            // 3. 取得期望的認證方法（如 "Bearer"），預設為 "Bearer"
            string expectedMethod = user.AuthenticMethods ?? "Bearer";

            // 4. 檢查 authHeader 是否以期望的方法開頭（不區分大小寫）
            if (!authHeader.StartsWith(expectedMethod, StringComparison.OrdinalIgnoreCase))
                return false;

            // 5. 提取 token 部分（去掉方法前綴和空格）
            string token = authHeader.Substring(expectedMethod.Length).Trim();

            // 6. 與儲存的 AuthenticationCode 比較  必須 加上 前綴Bearer
            return token == user.AuthenticationCode;
        }
    }
    public interface IPubBusiness
    {
        bool IsValidAuth(string mainComId, string authHeader);
    }
}
