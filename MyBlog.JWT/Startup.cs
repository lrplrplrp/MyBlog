using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MyBlog.IRepositiry;
using MyBlog.IService;
using MyBlog.Repository;
using MyBlog.Service;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.JWT
{
    public class Startup
    {
        private string MyAllowSpecificOrigins = "MyAllowSpecificOrigins";//这里也是跨域的东西

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyBlog.JWT", Version = "v1" });
            });

            #region sqlsugar
            SugarIocServices.AddSqlSugar(new IocConfig()
            {
                //ConfigId="db01"  多租户用到
                ConnectionString = this.Configuration["SqlConn"],
                DbType = IocDbType.MySql,
                IsAutoCloseConnection = true//自动释放
            }); //多个库就传List<IocConfig>

            //配置参数
            SugarIocServices.ConfigurationSugar(db =>
            {
                db.Aop.OnLogExecuting = (sql, p) =>
                {
                    Console.WriteLine(sql);
                };
                //设置更多连接参数
                //db.CurrentConnectionConfig.XXXX=XXXX
                //db.CurrentConnectionConfig.MoreSettings=new ConnMoreSettings(){}
                //二级缓存设置
                //db.CurrentConnectionConfig.ConfigureExternalServices = new ConfigureExternalServices()
                //{
                // DataInfoCacheService = myCache //配置我们创建的缓存类
                //}
                //读写分离设置
                //laveConnectionConfigs = new List<SlaveConnectionConfig>(){...}

                /*多租户注意*/
                //单库是db.CurrentConnectionConfig 
                //多租户需要db.GetConnection(configId).CurrentConnectionConfig 
            });
            #endregion

            #region CORS
            services.AddCors(options =>
            {
                options.AddPolicy(MyAllowSpecificOrigins,

                    builder => builder.AllowAnyOrigin()

                    .AllowAnyHeader()

                    .WithMethods("GET", "POST", "HEAD", "PUT", "DELETE", "OPTIONS")

                    );

            });
            #endregion

            services.AddScoped<IWriterInfoRepository, WriterInfoRepository>();
            services.AddScoped<IWriterInfoService, WriterInfoService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyBlog.JWT v1"));
            }


            app.UseRouting();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthorization();



            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
