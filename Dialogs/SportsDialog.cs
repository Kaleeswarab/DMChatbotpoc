using CoreBot.Models;
using CoreBot.Translation;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;


namespace CoreBot.Dialogs
{
    public class SportsDialog : ComponentDialog
    {
        string GlobalToken = "";
        private const string promptText = "sorry, we could not found the answer?";
        private const string repromptText = "please ask in different text?";
        private Translator _translator;
        public SportsDialog(Translator translator) :  base(nameof(SportsDialog))
        {
            _translator = translator;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
          {
                GetSportsNewsAsync

            }));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetSportsNewsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var sportsNews = (SportsNews)stepContext.Options;
            var languagecode = sportsNews.LanguageCode;
            if (sportsNews != null)
            {
                var messageText = sportsNews.News;
             
               // 
                var tranlatedtext = _translator.Translation(languagecode, messageText);
                var promptMessage1 = MessageFactory.Text(tranlatedtext, tranlatedtext, InputHints.ExpectingInput);
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage1 }, cancellationToken);

            }
            else
            {
                var translatedtext = _translator.Translation(languagecode, promptText);
                var promptMessage = MessageFactory.Text(translatedtext, translatedtext, InputHints.ExpectingInput);
                await stepContext.PromptAsync(nameof(TextPrompt),
                  new PromptOptions
                  {
                      Prompt = promptMessage,
                  }, cancellationToken);

            }
            return await stepContext.EndDialogAsync(sportsNews.News, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userName = stepContext.Context.Activity.From.Name;
            if (userName != null && stepContext.Options == null)
            {
                return await stepContext.EndDialogAsync("End SportsDialog", cancellationToken);
            }
            return await stepContext.EndDialogAsync(null, cancellationToken);


        }

    }
}
