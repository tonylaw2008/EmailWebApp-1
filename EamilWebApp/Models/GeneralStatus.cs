using System;
using System.ComponentModel;
using System.Reflection;
namespace EamilWebApp
{
    /// <summary>
    /// 通用的狀態枚舉，表示某個實體的狀態是啟用還是停用。
    /// </summary>
    public enum GeneralStatus
    {  
        ACTIVE = 1, 
        INACTIVE = 0,
    }

    /// <summary>
    /// 通用的返回結果枚舉，表示某個操作的結果是成功還是失敗。
    /// </summary>
    public enum GeneralResult
    {
        SUCCESS = 1,
        FAIL = 0,
        EXCEPTION = -1,
    }

    /// <summary>
    /// 發送EMAIL的結果狀態，成功或失敗。
    /// </summary>
    public enum SendMailResult
    {
        SUCCESS = 1,
        FAIL = 0,
    }
}
