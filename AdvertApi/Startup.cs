using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvertApi.HealthChecks;
using AdvertApi.Services;
using Amazon.ServiceDiscovery;
using Amazon.ServiceDiscovery.Model;
using Amazon.Util;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace AdvertApi
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
            services.AddAutoMapper();
            services.AddTransient<IAdvertStorageService, DynamoDbAdvertStorage>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddHealthChecks()
                .AddCheck<StorageHealthCheck>("example_health_check");

            services.AddCors(options =>
            {
                options.AddPolicy("AllOrigin", policy => { policy.WithOrigins("*").AllowAnyHeader(); });
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info
                {
                    Title = "Web Advertisement Apis",
                    Version = "version 1",
                    Contact = new Contact
                    {
                        Name = "Panos Anastasiadis",
                        Email = "panospd@domain.com"
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async Task Configure(IApplicationBuilder app, IHostingEnvironment env)
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Advert Api");
            });
            
            app.UseCors();

            await RegisterToCloudMap();

            app.UseMvc();
            app.UseHealthChecks("/health");
        }

        private async Task RegisterToCloudMap()
        {
            const string serviceId = "srv-3bnclkdyesopssh5";
            var instanceId = EC2InstanceMetadata.InstanceId;

            if (!string.IsNullOrEmpty(instanceId))
            {
                var ipv4 = EC2InstanceMetadata.PrivateIpAddress;

                var client = new AmazonServiceDiscoveryClient();

                await client.RegisterInstanceAsync(new RegisterInstanceRequest
                {
                    InstanceId = instanceId,
                    ServiceId = serviceId,
                    Attributes = new Dictionary<string, string>
                    {
                        {"AWS_INSTANCE_IPV4",ipv4 },
                        {"AWS_INSTANCE_PORT","80" }
                    }
                });
            }
        }
    }
}
