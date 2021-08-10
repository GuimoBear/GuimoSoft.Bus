using GuimoSoft.Bus.Kafka;
using GuimoSoft.Bus.Examples.Handlers.HelloMessage;
using GuimoSoft.Bus.Examples.Messages;
using GuimoSoft.Bus.Examples.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace GuimoSoft.Bus.Examples
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            isDevelopment = env.IsDevelopment();
        }

        private readonly bool isDevelopment;
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (isDevelopment)
                EnvFile.CarregarVariaveis();

            var wrapper = services
                .AddKafkaProducer(config =>
                {
                    config
                        .Produce().FromType<HelloMessage>().ToEndpoint(HelloMessage.TOPIC_NAME)
                        .ToServer(options =>
                        {
                            options.BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_HOSTS");
                            options.Acks = Confluent.Kafka.Acks.All;
                        });
                })
                .AddKafkaConsumer(config => 
                {
                    config
                        .FromServer(options =>
                        {
                            options.BootstrapServers = Environment.GetEnvironmentVariable("KAFKA_HOSTS");
                            options.GroupId = Environment.GetEnvironmentVariable("KAFKA_CONSUMER_GROUP_ID");
                        })
                        .Consume().OfType<HelloMessage>().WithMiddleware<HelloMessageMiddleware>().FromEndpoint(HelloMessage.TOPIC_NAME);
                });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GuimoSoft.Bus.Examples", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GuimoSoft.Bus.Examples v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
