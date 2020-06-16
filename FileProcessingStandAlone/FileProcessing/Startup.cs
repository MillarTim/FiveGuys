using CSS.Cloud.Framework;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

using System.Collections.Generic;
using System.Linq;


namespace CSS.Connector.FileProcessing
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
			  .SetBasePath(env.ContentRootPath)
			  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			  .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
			  .AddEnvironmentVariables();
			Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
					builder => builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials()
					.WithExposedHeaders("WWW-Authenticate")
					);
			});

			services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
			.AddIdentityServerAuthentication(options =>
			{
				options.Authority = ServiceFabricManager.GetConfigParameter("Config", "FileProcessingConfig", "AuthorityURL");
                options.SupportedTokens = SupportedTokens.Both;
                options.RequireHttpsMetadata = false;
                options.ApiName = ServiceFabricManager.GetConfigParameter("Config", "FileProcessingConfig", "apiname");
                options.ApiSecret = ServiceFabricManager.GetConfigParameter("Config", "FileProcessingConfig", "apisecret");
            });

            services.AddMvc();
               

/*
			services.AddAuthorization(options =>
			{
				options.AddPolicy("<def_scr_dsc>", policy => policy.RequireClaim("<def_scr_dsc>"));
			});
*/
			services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
			services.AddResponseCompression();

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Info { Title = "FileDefinition Processing API", Version = "v1" });
				c.AddSecurityDefinition("Bearer", new ApiKeyScheme() { In = "header", Description = "Please enter Bearer Token", Name = "Authorization", Type = "apiKey" });
				c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "Bearer", Enumerable.Empty<string>() }, });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();
			app.UseResponseCompression();
			app.UseCors("CorsPolicy");
			app.UseAuthentication();
			app.UsePathBase("/FileProcessingService");
            app.UseLoggingMiddleware();
            app.UseMvc();
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/FileProcessingService/swagger/v1/swagger.json", "My API V1");
			});
		}
	}
}
