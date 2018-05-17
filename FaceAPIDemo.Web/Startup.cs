using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ProjectOxford.Face;

namespace FaceAPIDemo.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // 添加FaceServiceClient用来调用Azure Face API
            // 第二个参数为Face API的终结点
            services.AddTransient(_ => new FaceServiceClient("Your Cognitive Service Key", "https://eastasia.api.cognitive.microsoft.com/face/v1.0/"));
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
