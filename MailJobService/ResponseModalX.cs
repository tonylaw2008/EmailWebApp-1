using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MailTaskJobService
{
   
     
    public partial class ResponseModalX
    {
        public ResponseModalX()
        {
            _meta = new MetaModalX { Success = true, Message = "OK", ErrorCode = (int)SendMailResult.SUCCESS };
            _data = new { };
        }
        private MetaModalX _meta;
        [JsonProperty("meta")]
        public MetaModalX meta
        {
            get
            {
                return _meta;
            }
            set
            {
                _meta = value;
            }
        }
        private Object _data;
        [JsonProperty("data")]
        public Object data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }

        public static implicit operator Task<object>(ResponseModalX v)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// R代表目标实体   T代表数据源实体
        /// </summary>
        /// <typeparam name="R"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static R Mapping<R, T>(T model)
        {
            R result = Activator.CreateInstance<R>();
            foreach (PropertyInfo info in typeof(R).GetProperties())
            {
                PropertyInfo pro = typeof(T).GetProperty(info.Name);
                if (pro != null)
                    info.SetValue(result, pro.GetValue(model));
            }
            return result;
        }
         
        /// <summary>
        /// 用于异步线程生成文件 主线程等待 Used for asynchronous thread to generate files, main thread waiting
        /// </summary>
        /// <param name="pathFileName">检测的目标文件 Detected target file</param>
        /// <param name="hasCreatedMinuteSpan">创建超过30分钟的文件重新创建 必须是正整数 Files created more than 30 minutes ago must be a positive integer</param>
        /// <param name="maxWaitSecond">创建线程 耗时最长的秒数,超过则主线程不等待,视为失败处理Creating a thread takes the longest number of seconds. If it exceeds the number of seconds, the main thread will not wait and it will be treated as a failure.</param>
        /// <param name="isFreshFile">文件是否新鲜的,超过30分钟为旧文件 Whether the file is fresh, more than 30 minutes is the old file</param>
        public static void FileCreateWait(string pathFileName,int hasCreatedMinuteSpan, int maxWaitSecond,ref bool isFreshFile)
        {
                hasCreatedMinuteSpan = Math.Abs(hasCreatedMinuteSpan);

                maxWaitSecond = maxWaitSecond * 1000;  //millsecond = second*1000

                bool IsExist = false;
                bool IsWait = true; 
                int i = 0;
                while (IsWait && !IsExist)
                {
                    IsExist = System.IO.File.Exists(pathFileName);
                    if (IsExist)
                    { 
                        FileInfo fileInfo = new FileInfo(pathFileName);
                        double distanceMins = DateTime.Now.Subtract(fileInfo.LastWriteTime).TotalMinutes;
                        if (distanceMins > hasCreatedMinuteSpan)
                        {
                            Thread.Sleep(500);
                            IsWait = true; 
                            isFreshFile = false;
                        }
                        else
                        {
                            isFreshFile = true;
                            IsWait = false;
                        }
                    }
                    else
                    {
                        isFreshFile = false;
                        IsWait = true;
                        Thread.Sleep(1000);
                    }
               
                    if (i * 1000 > maxWaitSecond)  //force to stop
                    {
                        IsExist = true;
                        IsWait = false; 
                    }
                    i++;
                } //while
        }
    }
    public partial class MetaModalX
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = true;

        [JsonProperty("message")]
        public string Message { get; set; } = "OK";

        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; } = (int)GeneralResult.SUCCESS;
    }

}
