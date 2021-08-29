using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using DynamoDB.Common;
using MediatR;
using BWF.Api.Services.Store;
using BWF.Api.Host.Middleware;
using BWF.Api.Services;

namespace BWF.Api.Host
{
    public class Startup
    {
        public const string AWSDynamoDBServiceURL = "AWS:DynamoDB:ServiceUrl";

        protected const string BadWordsTable = "ApiSettings:WordsTable";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            ConfigureSwagger(services);
            ConfigureDynamo(services);
            ConfigureEngine(services);
            services.AddHealthChecks();
        }

        private void ConfigureEngine(IServiceCollection services)
        {
            services.AddSingleton<ICheckEngine, CheckEngine>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseErrorHandlerMiddleware();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
            UseSwaggerMiddleware(app);
        }
        protected virtual void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Version = "v1",
                    Title = "Bad Words Finder API",
                });

                // set the comments path for the Swagger JSON and UI
                foreach (string xmlDocument in System.IO.Directory.EnumerateFiles(AppContext.BaseDirectory, "*.xml"))
                {
                    options.IncludeXmlComments(xmlDocument, includeControllerXmlComments: true);
                }
            });
        }
        protected virtual void UseSwaggerMiddleware(IApplicationBuilder app)
        {
            app.UseSwagger();
            // enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("swagger/v1/swagger.json", "Band Words Finder API v1");
                options.RoutePrefix = string.Empty;
            });
        }
        protected virtual void ConfigureDynamo(IServiceCollection services)
        {
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            var serviceURL = Configuration.GetValue<string>(AWSDynamoDBServiceURL);
            if (!string.IsNullOrWhiteSpace(serviceURL))
            {
                services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient("local", "local", new AmazonDynamoDBConfig() { ServiceURL = serviceURL }));
            }
            else
            {
                services.AddAWSService<IAmazonDynamoDB>();
            }

            services.AddTransient<IDynamoDBContext, DynamoDBContext>();
            services.AddSingleton<IDynamoDBTableDefinition, TableWithSortDataDefinition>();
            var provider = services.BuildServiceProvider();
            new DynamoDBTableInitializer(
                provider.GetService<IAmazonDynamoDB>(),
                Configuration.GetValue<string>(BadWordsTable),
                provider.GetService<ILogger<DynamoDBTableInitializer>>(),
                provider.GetService<IDynamoDBTableDefinition>()
                ).Init().Wait();

            services.AddSingleton<IBadWordsRepository>
              (_ => new Services.DynamoDB.WordsRepository(_.GetService<IDynamoDBContext>(), Configuration.GetValue<string>(BadWordsTable)));
        }

    }
}