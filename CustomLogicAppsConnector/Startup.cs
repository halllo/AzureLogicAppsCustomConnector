using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CustomLogicAppsConnector
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddHttpContextAccessor();

			services.AddSwaggerGen(c =>
			{
				c.DocumentFilter<HostDocumentFilter>();
				c.SwaggerDoc("v1", new Info { Title = "MyValues", Version = "v1" });
			});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}
			app.UseHttpsRedirection();

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyValues V1");
			});

			app.UseMvc();
		}

		public class HostDocumentFilter : IDocumentFilter
		{
			private readonly IHttpContextAccessor httpContextAccessor;

			public HostDocumentFilter(IHttpContextAccessor httpContextAccessor)
			{
				this.httpContextAccessor = httpContextAccessor;
			}

			public void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context)
			{
				var request = httpContextAccessor.HttpContext.Request;
				swaggerDoc.Host = request.Host.ToString();
				swaggerDoc.Schemes = new List<string>() { request.Scheme };
			}
		}
	}
}
