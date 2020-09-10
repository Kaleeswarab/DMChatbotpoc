using CoreBot.CognitiveModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using CoreBot.Dialogs;
using CoreBot;
using CoreBot.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.BotBuilderSamples.Dialogs;
using Microsoft.BotBuilderSamples;
using System.Net;
using System.Text;
using System.Xml;
using CoreBot.Translation;

namespace CoreBot.Dialogs
{
    public class LockDownDialog : ComponentDialog
    {
        string GlobalToken = "";
        private const string promptText = "sorry, we could not found the answer?";
        private const string repromptText = "please ask in different text?";
        private Translator _translator;
        public LockDownDialog(Translator translator) : base(nameof(LockDownDialog))
        {
            _translator = translator;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
          {
                GetLockdownNewsAsync

            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetLockdownNewsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
             var lockdownnews = (LockdownNews)stepContext.Options;
            if (lockdownnews.Country != null && lockdownnews.Lockdowndays != null)
            {
                var country = lockdownnews.Country;
                var days= lockdownnews.Lockdowndays;
                var langcode = lockdownnews.LanguageCode;
                var messageText = "In '" + country + "' Goverment announced Lock down for '" + days + " days";
                var tranlatedtext = _translator.Translation(langcode, messageText);
                var promptMessage1 = MessageFactory.Text(tranlatedtext, tranlatedtext, InputHints.ExpectingInput);
                //var promptMessage1 = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage1 }, cancellationToken);

            }
            else
            {
                    var promptMessage = MessageFactory.Text(promptText, promptText, InputHints.ExpectingInput);
                    var repromptMessage = MessageFactory.Text(repromptText, repromptText, InputHints.ExpectingInput);
                    await stepContext.PromptAsync(nameof(TextPrompt),
                      new PromptOptions
                      {
                          Prompt = promptMessage,
                          RetryPrompt = repromptMessage,
                      }, cancellationToken);
               }
            return await stepContext.EndDialogAsync(lockdownnews.Lockdowndays, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //if ((bool)stepContext.Result)
            //{
            //    var covidDetails = (CovidDetails)stepContext.Options;

            //    return await stepContext.EndDialogAsync(covidDetails, cancellationToken);
            //}
            var userName = stepContext.Context.Activity.From.Name;
            if (userName != null && stepContext.Options == null)
            {
                return await stepContext.EndDialogAsync("End LockDown", cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);


        }      
    }
}

