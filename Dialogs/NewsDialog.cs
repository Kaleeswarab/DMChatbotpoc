
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using CoreBot.Translation;

namespace CoreBot.Dialogs
{
    public class NewsDialog : ComponentDialog
    {
        public IConfiguration configuration;
        string SubscriptionKey = "";
        //SubscriptionKey=configuration["SubsricptionKey"];
        string GlobalToken = "";

        private const string promptText = "sorry, we could not found the answer?";
        private const string repromptText = "please ask in different text?";
        private Translator _translator;
        public NewsDialog(Translator translator) : base(nameof(NewsDialog))
        {
            _translator = translator;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                GetCovidNewsAsync           
              }));
          

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        
        private async Task<DialogTurnResult> GetCovidNewsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var coviDetails = (CovidResult)stepContext.Options;
            var Lncode = coviDetails.LanguageCode.ToString();

            if (coviDetails.Country != null)
            {
                var country = coviDetails.Country;
                int active = 0, death = 0, recovered = 0;
                int.TryParse(coviDetails.Active, out active);
                int.TryParse(coviDetails.Recovered, out recovered);
                int.TryParse(coviDetails.Death, out death);
                int total = active + death + recovered;

                var messageText = "In " + country + "Total Cases: " + total + Environment.NewLine + "Active Count: " + active + Environment.NewLine + "Recovered Count: " + recovered + Environment.NewLine + "Death Count: " + death;
                var translatedtext = "";
                translatedtext = _translator.Translation(Lncode, messageText);
                var promptMessage1 = MessageFactory.Text(translatedtext, translatedtext, InputHints.ExpectingInput);
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage1 }, cancellationToken);
            }
            else if (coviDetails.General != null)
            {
                var general = coviDetails.General;                
                var translatedtext = "";               
                translatedtext = _translator.Translation(Lncode, general);
                var promptMessage1 = MessageFactory.Text(translatedtext, translatedtext, InputHints.ExpectingInput);
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage1 }, cancellationToken);
            }
            else
            {
                var translatedtext = _translator.Translation(Lncode, promptText);
                var promptMessage = MessageFactory.Text(translatedtext, translatedtext, InputHints.ExpectingInput);
                    await stepContext.PromptAsync(nameof(TextPrompt),
                      new PromptOptions
                      {
                          Prompt = promptMessage,
                      }, cancellationToken);
               
            }
           
           return await stepContext.EndDialogAsync(coviDetails.Country, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            
            var userName = stepContext.Context.Activity.From.Name;
            if(userName != null && stepContext.Options == null)
            {
                return await stepContext.EndDialogAsync("End News Dialog", cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);


        }

    }
}
