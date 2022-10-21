using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace MyBlog.Model
{
    public class BlogNews:BaseId
    {
        [SugarColumn(ColumnDataType ="nvarchar(30)")]
        public string Title { get; set; }
        [SugarColumn(ColumnDataType ="text")]
        public string Content { get; set; }
        public DateTime Time { get; set; }
        
        public int BrowseCount { get; set; }
        public int LikeCount { get; set; }
        
        //导航查询
        public int WriterId { get; set; }
        public int TypeId { get; set; }

        //IsIgnore =true意思为不映射到数据库，可以进行连表查询
        [SugarColumn(IsIgnore =true)]
        public TypeInfo TypeInfo { get; set; }
        [SugarColumn(IsIgnore = true)]
        public WriterInfo WriterInfo { get; set; }
    }
}
