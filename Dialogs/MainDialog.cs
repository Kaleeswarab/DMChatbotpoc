// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;//
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using CoreBot.Dialogs;
using CoreBot.CognitiveModels;
using CoreBot;
using CoreBot.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using CoreBot.Translation;

namespace Microsoft.BotBuilderSamples.Dialogs
{
    public class MainDialog : LogoutDialog
    {
        string LanuageCode = "";
         private readonly FlightBookingRecognizer _luisRecognizer;
        protected readonly ILogger Logger;
        public IConfiguration _configuration;
        public Translator _translator;

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(IConfiguration configuration, FlightBookingRecognizer luisRecognizer, NewsDialog newsDialog, ILogger<MainDialog> logger, SportsDialog sportsDialog, LockDownDialog lockDownDialog, GenericDialog genericDialog, Translator translator)
            : base(nameof(MainDialog), configuration["ConnectionName"])
        {
            _configuration = configuration;

          _luisRecognizer = luisRecognizer;
            Logger = logger;
            _translator = translator;
            //AddDialog(new OAuthPrompt(
            //   nameof(OAuthPrompt),
            //   new OAuthPromptSettings
            //   {
            //       ConnectionName = ConnectionName,
            //       Text = "Please Sign In to the Azure AD",
            //       Title = "Sign In",
            //       Timeout = 300000, // User has 5 minutes to login (1000 * 60 * 5)
            //    }));
            //AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(newsDialog);
            AddDialog(sportsDialog);
            AddDialog(lockDownDialog);
            AddDialog(genericDialog);
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
           {
                //PromptStepAsync,
                 //LoginStepAsync,
                //  IntroStepAsync,
                 ActStepAsync,
                 FinalStepAsync
           }));

              // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> PromptStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.BeginDialogAsync(nameof(OAuthPrompt), null, cancellationToken);
        }

        private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             // Get the token from the previous step. Note that we could also have gotten the
            // token directly from the prompt itself. There is an example of this in the next method.
            var tokenResponse = (TokenResponse)stepContext.Result;
            if (tokenResponse != null)
            {
                var userName = stepContext.Context.Activity.From.Name;
                if(stepContext.Options == null)
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Welcome - {userName }, You are now logged in; For sign out, type logout"), cancellationToken);
                if (!_luisRecognizer.IsConfigured)
                {
                    await stepContext.Context.SendActivityAsync(
                        MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                    return await stepContext.NextAsync(null, cancellationToken);
                }
                var messageText = stepContext.Options?.ToString() ?? "How can I Help you";
                var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            var messageText = stepContext.Options?.ToString() ?? "Get latest news here";
            var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                // LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
                return await stepContext.BeginDialogAsync(nameof(NewsDialog), new CovidResult(), cancellationToken);
            }
            LanuageCode = _translator.DetectLanguageCode(stepContext.Context.Activity.Text);
            var translatedText = _translator.TranslateToEnglish(LanuageCode, stepContext.Context.Activity.Text);
            stepContext.Context.Activity.Text = translatedText;
            var luisResult = await _luisRecognizer.RecognizeAsync<LuisIntentEntity>(stepContext.Context, cancellationToken);
           
            switch (luisResult.TopIntent().intent)
            {
                case LuisIntentEntity.Intent.Generic:
                    var genericDetails = new GenericResult();
                    if (luisResult != null && luisResult.Entities != null)
                    {
                        if (luisResult.Entities.General != null)
                        {

                            genericDetails.General = GetGenericResponse(luisResult.Entities.General);
                            genericDetails.LanguageCode = LanuageCode;
                        }
                        else
                        {
                            GetText("Generic", luisResult.Text);
                        }
                    }
                    return await stepContext.BeginDialogAsync(nameof(GenericDialog), genericDetails, cancellationToken);

                case LuisIntentEntity.Intent.CovidNews:
                    //Log file 
                    var covidDetails = new CovidResult();
                    covidDetails.LanguageCode = LanuageCode;
                    if (luisResult != null && luisResult.Entities != null)
                    {
                        if (luisResult.Entities.General != null)
                        {
                            covidDetails.General = GetGenericResponse(luisResult.Entities.General);
                        }
                        else if (luisResult.Entities.Country != null)
                        {
                            // Get destination and origin from the composite entities arrays.
                            if (luisResult.Entities.Active != null && luisResult.Entities.Active.Length > 0)
                                covidDetails.Active = luisResult.Entities.Active.FirstOrDefault()[0].ToString();
                            if (luisResult.Entities.Country != null && luisResult.Entities.Country.Length > 0)
                                covidDetails.Country = luisResult.Entities.Country.FirstOrDefault()[0].ToString();
                            if (luisResult.Entities.Death != null && luisResult.Entities.Death.Length > 0)
                                covidDetails.Death = luisResult.Entities.Death.FirstOrDefault()[0].ToString();
                            if (luisResult.Entities.Recovered != null && luisResult.Entities.Recovered.Length > 0)
                                covidDetails.Recovered = luisResult.Entities.Recovered.FirstOrDefault()[0].ToString();
                      
                        }

                        else
                        {
                            GetText("CovidNews", luisResult.Text);
                        }
                    }

                    return await stepContext.BeginDialogAsync(nameof(NewsDialog), covidDetails, cancellationToken);

                case LuisIntentEntity.Intent.SportsNews:

                    //Log file 
                    if (luisResult.Entities.CricketNews == null)
                    {
                        GetText("CricketNews", luisResult.Text);
                    }

                    var sportsNews = new SportsNews();
                    if (luisResult.Entities.CricketNews != null)
                        sportsNews.News = luisResult.Entities.CricketNews.FirstOrDefault()[0].ToString();
                    sportsNews.LanguageCode = LanuageCode;
                    return await stepContext.BeginDialogAsync(nameof(SportsDialog), sportsNews, cancellationToken);


                case LuisIntentEntity.Intent.Lockdown:
                    //Log file 
                    if (luisResult.Entities.Lockdowndays == null)
                    {
                        GetText("Lockdown", luisResult.Text);
                    }

                    var lockdownnews = new LockdownNews();
                    if (luisResult.Entities.Lockdowndays != null && luisResult.Entities.Lockdowndays != null)
                    {

                        if (luisResult.Entities.Country != null && luisResult.Entities.Country.Length > 0)
                            lockdownnews.Country = luisResult.Entities.Country.FirstOrDefault()[0].ToString();
                        if (luisResult.Entities.Lockdowndays != null && luisResult.Entities.Lockdowndays.Length > 0)
                            lockdownnews.Lockdowndays = luisResult.Entities.Lockdowndays.FirstOrDefault()[0].ToString();

                    }
                    return await stepContext.BeginDialogAsync(nameof(LockDownDialog), lockdownnews, cancellationToken);

                default:
                    GetText("None", luisResult.Text);
                    // Catch all for unhandled intents
                    var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
                    var trnslatedtxt =_translator.Translation(LanuageCode, didntUnderstandMessageText);
                    var didntUnderstandMessage = MessageFactory.Text(trnslatedtxt, trnslatedtxt, InputHints.IgnoringInput);
                    await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
                    break;
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Restart the main dialog with a different message the second time around
            var promptMessage = "Do you want to know more information?";
            var trnslatedtxt = _translator.Translation(LanuageCode, promptMessage);
            return await stepContext.ReplaceDialogAsync(InitialDialogId, trnslatedtxt, cancellationToken);
        }

        private string GetGenericResponse(string[][] General)
        {
            string result = string.Empty;

            if (General != null && General.Length > 0)
            {

                for (int i = 0; i < General.FirstOrDefault().Count(); i++)
                {
                    if (string.IsNullOrEmpty(result))
                    {
                        result = General.FirstOrDefault()[i].ToString();
                    }
                    else
                    {
                        result += Environment.NewLine + General.FirstOrDefault()[i].ToString();
                    }
                }

            }
            return result;
        }

             
        private void GetText(string intent, string question)
        {
            var fileName = Environment.CurrentDirectory + @"\chatbotlog.txt";

             try
            {

                if (!(File.Exists(fileName)))
                {
                    // Create a new file     
                    using (StreamWriter sw = File.CreateText(fileName))
                    {
                        sw.WriteLine(intent + " : " + question + "  " + DateTime.Now.ToString() + " ", new[] { DateTime.Now.ToString() });

                    }
                }
                else//Append to excisting file 
                {

                    File.AppendAllText(fileName, intent + " : " + question + "  " + DateTime.Now.ToString() + " " + Environment.NewLine);

                }


            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.ToString());
            }
        }

     

       

       
       

        
    }
}
