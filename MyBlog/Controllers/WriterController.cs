using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBlog.IService;
using MyBlog.Model;
using MyBlog.Model.DTO;
using MyBlog.Utility._MD5;
using MyBlog.Utility.ApiReus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    [EnableCors("MyAllowSpecificOrigins")]
    public class WriterController : ControllerBase
    {
        private readonly IWriterInfoService _iWriterInfoService;

        public WriterController(IWriterInfoService IWriterInfoService)
        {
            _iWriterInfoService = IWriterInfoService;
        }
        //[Authorize]
        [HttpGet("Writer")]
        public async Task<APIResult> GetWriter()
        {
            var writer=await _iWriterInfoService.QueryAsync();
            if (writer.Count==0) return APIResultHelper.Error("没有更多作者");
            return APIResultHelper.Success(writer);
        }
        [HttpPost("Create")]
        public async Task<APIResult> Create(string name,string username,string userpwd)
        {
            if (string.IsNullOrEmpty(name)) return APIResultHelper.Error("作者名不能为空");
            WriterInfo writer = new WriterInfo
            {
                Name = name,
                UserName = username,
                UserPwd=MD5Helper.MD5Encrypt32(userpwd)
            };
            var oldwriter = await _iWriterInfoService.FindAsync(c => c.UserName == username);
            if (oldwriter != null) return APIResultHelper.Error("账号已经存在");
            bool b = await _iWriterInfoService.CaeateAsync(writer);
            if (!b) return APIResultHelper.Error("添加失败");
            return APIResultHelper.Success(b);
        }
        //[Authorize]
        [HttpDelete("Delete")]
        public async Task<APIResult> Delete(int id)
        {
            bool b = await _iWriterInfoService.DeleteAsync(id);
            if (!b) return APIResultHelper.Error("没有找到该账号");
            return APIResultHelper.Success(b);
        }
        //[Authorize]
        
        [HttpPut("Edit")]
        public async Task<APIResult> Edit(string name)
        {
            int id=Convert.ToInt32(this.User.FindFirst("Id").Value);
            var writer = await _iWriterInfoService.FindAsync(id);
            if (writer == null) return APIResultHelper.Error("没有找到该账号");
            writer.Name = name;
            bool b = await _iWriterInfoService.EditAsync(writer);
            if (!b) return APIResultHelper.Error("修改失败");
            return APIResultHelper.Success(b);
        }
        //[Authorize]
        [HttpGet("FindWriter")]
        public async Task<APIResult> FindWriter([FromServices]IMapper iMapper,int id)
        {
            var writer = await _iWriterInfoService.FindAsync(id);
            var writerDTO = iMapper.Map<WriterDTO>(writer);
            return APIResultHelper.Success(writerDTO);
        }
    }
}
