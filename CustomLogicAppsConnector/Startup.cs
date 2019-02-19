using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });

				c.OperationFilter<SwaggerDefaultValues>();
				c.CustomSchemaIds(x => x.FullName);

			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();

			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			});

			app.UseMvc();
		}

		internal class SwaggerDefaultValues : IOperationFilter
		{
			public void Apply(Operation operation, OperationFilterContext context)
			{
				if (operation.Parameters == null)
				{
					return;
				}

				// REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
				// REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
				foreach (var parameter in operation.Parameters.OfType<NonBodyParameter>())
				{
					var description = context.ApiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
					var routeInfo = description.RouteInfo;

					if (parameter.Description == null)
					{
						parameter.Description = description.ModelMetadata?.Description;
					}

					if (routeInfo == null)
					{
						continue;
					}

					if (parameter.Default == null)
					{
						parameter.Default = routeInfo.DefaultValue;
					}

					parameter.Required |= !routeInfo.IsOptional;
				}
			}
		}
	}
}
