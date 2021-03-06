// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using CoreBot;
using CoreBot.Dialogs;
using CoreBot.Translation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.BotBuilderSamples.Bots;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.BotBuilderSamples
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDistributedMemoryCache();

            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromSeconds(3600);
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.IsEssential = true;
            //    options.Cookie.Name = "conversionId";
            //});
             services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

         //   services.AddSingleton<ConfigurationCredentialProvider>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            // Register LUIS recognizer
            services.AddSingleton<FlightBookingRecognizer>();
            // services.AddSingleton<AppLuisRecognizer>();
          services.AddSingleton<Translator>();


            // Register the BookingDialog.
            //services.AddSingleton<BookingDialog>();
            services.AddSingleton<NewsDialog>();
            services.AddSingleton<SportsDialog>();
            services.AddSingleton<LockDownDialog>();
            services.AddSingleton<GenericDialog>();

            // The MainDialog that will be run by the bot.
            services.AddSingleton<MainDialog>();
           // services.AddSingleton<AppDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
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
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseWebSockets();
            app.UseMvc();
           // app.UseSession();
        }
       
    }
}
