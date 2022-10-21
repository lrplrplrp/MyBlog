using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBlog.IService;
using MyBlog.Model;
using MyBlog.Model.DTO;
using MyBlog.Utility.ApiReus;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    [EnableCors("MyAllowSpecificOrigins")]
    public class BlogNewsController : ControllerBase
    {
        private readonly IBlogNewsService _iBlogNewsService;
        public BlogNewsController(IBlogNewsService iBlogNewsService)
        {
            this._iBlogNewsService = iBlogNewsService;
        }
        [Authorize]
        [HttpGet("BlogNews")]
        public async Task<ActionResult<APIResult>> GetBlogNews()
        {
            var data = await _iBlogNewsService.QueryAsync(c => c.WriterId == Convert.ToInt32(this.User.FindFirst("Id").Value));
            if (data.Count == 0) return APIResultHelper.Error("没有更多的文章");
            return APIResultHelper.Success(data);
        }
        //[Authorize]
        [HttpPost("Create")]
        public async Task<ActionResult<APIResult>> Create(string title, string content, int typeid)
        {
            //数据验证
            BlogNews blogNews = new BlogNews
            {
                BrowseCount = 0,
                Content = content,
                LikeCount = 0,
                Time = DateTime.Now,
                Title = title,
                TypeId = typeid,
                WriterId = Convert.ToInt32(this.User.FindFirst("Id").Value)
            };
            bool b = await _iBlogNewsService.CaeateAsync(blogNews);
            if (!b) return APIResultHelper.Error("添加失败，服务器发生错误");
            return APIResultHelper.Success(blogNews);
        }
        //[Authorize]
        [HttpDelete("Delete")]
        public async Task<ActionResult<APIResult>> Delete(int id)
        {
            bool b = await _iBlogNewsService.DeleteAsync(id);
            if (!b) return APIResultHelper.Error("删除失败");
            return APIResultHelper.Success(b);
        }
       // [Authorize]
        [HttpPut("Edit")]
        public async Task<ActionResult<APIResult>> Edit(int id, string title, string content, int typeid)
        {
            var blogNews = await _iBlogNewsService.FindAsync(id);
            if (blogNews == null) return APIResultHelper.Error("没有找到文章");
            blogNews.Title = title;
            blogNews.Content = content;
            blogNews.TypeId = typeid;
            bool b = await _iBlogNewsService.EditAsync(blogNews);
            if (!b) return APIResultHelper.Error("修改失败");
            return APIResultHelper.Success(b);
        }
        [HttpGet("BlogNewsPage")]
        public async Task<APIResult> GetBlogNews([FromServices] IMapper iMapper, int page, int size)
        {
            RefAsync<int> total = 0;
            var blogNews = await _iBlogNewsService.QueryAsync(page, size, total);

            try
            {
                var blgnewsDTO = iMapper.Map<List<BlogNewsDTO>>(blogNews);
                return APIResultHelper.Success(blgnewsDTO,total);
            }
            catch (Exception)
            {

                return APIResultHelper.Error("AutoMapper映射错误");
            }
        }
        [HttpGet("BlogNewsId")]
        public async Task<ActionResult<APIResult>> GetBlogNews(int id)
        {
            var data = await _iBlogNewsService.QueryAsync(c => c.Id == id);
            if (data.Count == 0) return APIResultHelper.Error("未找到文章");
            return APIResultHelper.Success(data);
        }
        [HttpOptions("Options")]
        public HttpResponseMessage OptionsUser()
        {
            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.OK;
            return response;
        }
        
    }
}
