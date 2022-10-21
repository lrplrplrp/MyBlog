using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Utility.ApiReus
{
    public class APIResultHelper
    {
        public static APIResult Success(dynamic data)
        {
            return new APIResult
            {
                Code = 200,
                Data = data,
                Msg = "操作成功",
                Total=0
            };
        }

        public static APIResult Success(dynamic data,RefAsync<int> total)
        {
            return new APIResult
            {
                Code = 200,
                Data = data,
                Msg = "操作成功",
                Total = total
            };
        }

        public static APIResult Error(string msg)
        {
            return new APIResult
            {
                Code = 500,
                Data = null,
                Msg=msg,
                Total=0
            };
        }
    }
}
