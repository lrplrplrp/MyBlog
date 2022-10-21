using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyBlog.IRepositiry;
using MyBlog.IService;
using MyBlog.Repository;
using MyBlog.Service;
using MyBlog.Utility.AutoMapper;
using SqlSugar.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog
{
    public class Startup
    {
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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MyBlog", Version = "v1" });

                #region JWT受鉴权组件
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Description = "直接在下框中输入Bearer {token}（注意两者之间是一个空格）",
                    Name = "Authorization",
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
          {
            new OpenApiSecurityScheme
            {
              Reference=new OpenApiReference
              {
                Type=ReferenceType.SecurityScheme,
                Id="Bearer"
              }
            },
            new string[] {}
          }
        });
                #endregion

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
            // 10秒入门
            #region 依赖注入
            services.AddCustomIOC();
            #endregion

            #region JWT鉴权
            services.AddCustomJWT();
            #endregion

            #region AutoMapper
            services.AddAutoMapper(typeof(CustomAutoMapperProFile));
            #endregion

            #region CORS
            services.AddCors(options =>
            {
                options.AddPolicy("MyAllowSpecificOrigins",

                    builder => builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    
                    .WithMethods("GET", "POST", "HEAD", "PUT", "DELETE", "OPTIONS")

                    );

            });
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyBlog v1"));
            }

            app.UseRouting();

            //添加到管道中
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("MyAllowSpecificOrigins");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    public static class ICOExtend
    {
        public static IServiceCollection AddCustomIOC(this IServiceCollection service)
        {
            service.AddScoped<IBlogNewsRepository, BlogNewsRepository>();
            service.AddScoped<IBlogNewsService, BlogNewsService>();
            service.AddScoped<ITypeInfoRepository, TypeInfoRepository>();
            service.AddScoped<ITypeInfoService, TypeInfoService>();
            service.AddScoped<IWriterInfoRepository, WriterInfoRepository>();
            service.AddScoped<IWriterInfoService, WriterInfoService>();
            return service;

        }
        public static IServiceCollection AddCustomJWT(this IServiceCollection services)
        {
            #region JWT鉴权
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SDMC-CJAS1-SAD-DFSFA-SADHJVF-VF")),
                    ValidateIssuer = true,
                    ValidIssuer = "http://localhost:6060",
                    ValidateAudience = true,
                    ValidAudience = "http://localhost:5000",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(60)
                };
            });
            return services;
            #endregion
        }
    }
}
