using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebTest.JsonConverters;
using WebTest.Services;

namespace WebTest
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers(setup =>
            {
                setup.ReturnHttpNotAcceptable = true;
            })
               .AddXmlDataContractSerializerFormatters()
           .AddJsonOptions(config =>
           {
               config.JsonSerializerOptions.WriteIndented = true;
               config.JsonSerializerOptions.Converters.Add(new DateTimeConverter());// 日期格式转换器
               config.JsonSerializerOptions.Converters.Add(new IntStrConverter());// 字符串数字隐式转换器
               config.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());// 枚举转换器
                                                                                          //config.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
               config.JsonSerializerOptions.PropertyNamingPolicy = null;
           });

            services.AddScoped<IJobService, JobService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseFileServer();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
