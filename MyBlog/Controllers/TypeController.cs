using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBlog.IService;
using MyBlog.Model;
using MyBlog.Utility.ApiReus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    [EnableCors("MyAllowSpecificOrigins")]
    public class TypeController : ControllerBase
    {
        private readonly ITypeInfoService _iTypeInfoService;
        public TypeController(ITypeInfoService iTypeInfoService)
        {
            this._iTypeInfoService = iTypeInfoService;
        }
        [HttpGet("Types")]
        public async Task<APIResult> GetType()
        {
            var types=await _iTypeInfoService.QueryAsync();
            //默认返回一个无数据的集合对象，但不为空所以不能判断为空
            if (types.Count==0) return APIResultHelper.Error("没有文章类型");
            return APIResultHelper.Success(types);
        }
        [HttpPost("Create")]
        public async Task<APIResult> Create(string name)
        {
            //数据验证
            if (string.IsNullOrEmpty(name)) return APIResultHelper.Error("类型名不能为空");
            TypeInfo type = new TypeInfo
            {
                Name = name
            };
            bool b=await _iTypeInfoService.CaeateAsync(type);
            if (!b) return APIResultHelper.Error("添加失败");
            return APIResultHelper.Success(b);
        }
        [HttpDelete("Delete")]
        public async Task<APIResult> Delete(int id)
        {
            bool b = await _iTypeInfoService.DeleteAsync(id);
            if (!b) return APIResultHelper.Error("没有找到该文章类型");
            return APIResultHelper.Success(b);
        }

        [HttpPut("Edit")]
        public async Task<APIResult> Edit(int id,string name)
        {
            var type = await _iTypeInfoService.FindAsync(id);
            if (type == null) return APIResultHelper.Error("没有找到该文章类型");
            type.Name = name;
            bool b=await _iTypeInfoService.EditAsync(type);
            if (!b) return APIResultHelper.Error("修改失败");
            return APIResultHelper.Success(b);
        }
    }


}
