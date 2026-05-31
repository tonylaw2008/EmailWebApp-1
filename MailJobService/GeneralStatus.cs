using System;
using System.ComponentModel;
using System.Reflection;
namespace MailTaskJobService
{
    /// <summary>
    /// MailTaskServiceSetting TestMode
    /// </summary>
    public enum ServiceSettingMode
    {
        ONLINE = 1,
        TEST_MODE = 0,
    }

    /// <summary>
    /// 通用的狀態枚舉，表示某個實體的狀態是啟用還是停用。
    /// </summary>
    public enum GeneralStatus
    {  
        ACTIVE = 1, 
        INACTIVE = 0,
    }

    public enum StatusCode
    {
        NOT_SET = -1,
        ACTIVE = 1,
        INACTIVE = 0,
        OCCUPIED = 2,
    }

    /// <summary>
    /// 通用的返回結果枚舉，表示某個操作的結果是成功還是失敗。
    /// </summary>
    public enum GeneralResult
    {
        SUCCESS = 1,
        FAIL = 0,
        NO_EMAILTASK_TO_EXECUTE = 13 
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
