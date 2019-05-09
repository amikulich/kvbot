using KvBot.Api.BotServices;
using KvBot.Api.DbConfiguration;
using KvBot.DataAccess;
using KvBot.DataAccess.Contract;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KvBot
{
    public class Startup
    {
        private ILoggerFactory _loggerFactory;

        public Startup(IHostingEnvironment env)
        {
            env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureDataAccess(services);

            services.AddScoped<IKkvService, KkvService>();

            IStorage dataStore = new MemoryStorage();

            // Create and add conversation state.
            var conversationState = new ConversationState(dataStore);
            services.AddSingleton(conversationState);

            services.AddBot<Api.KvBot>(options =>
           {
               var appId = Configuration.GetSection("MicrosoftAppId").Value;
               var appPassword = Configuration.GetSection("MicrosoftAppPassword").Value;
               options.CredentialProvider = new SimpleCredentialProvider(appId, appPassword);

               ILogger logger = _loggerFactory.CreateLogger<Api.KvBot>();

               options.OnTurnError = async (context, exception) =>
               {
                   logger.LogError($"Exception caught : {exception}");
                   await context.SendActivityAsync("Sorry, it looks like something went wrong.");
               };
           });
        }

        private void ConfigureDataAccess(IServiceCollection services)
        {
            var mongo = Configuration.GetSection("MongoConfiguration").Get<MongoConfiguration>();
            var connectionString = $"mongodb://{mongo.User}:{mongo.Password}@{mongo.Endpoint}/{mongo.Database}";

            services.AddSingleton<IDbContext, DbContext>((s) => new DbContext(connectionString, mongo.Database));

            services.AddScoped<IKkvFindCommand, KkvFindCommand>();
            services.AddScoped<IKkvWriteCommand, KkvWriteCommand>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}
