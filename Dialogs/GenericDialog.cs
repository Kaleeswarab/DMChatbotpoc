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
    public class GenericDialog :ComponentDialog
    {
        private const string promptText = "sorry, we could not found the answer?";
        private const string repromptText = "please ask in different text?";
        private Translator _translator;
        public GenericDialog(Translator translator) : base(nameof(GenericDialog))
        {
            _translator = translator;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
                {
                GetGenericResultAsync
                  }));


            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> GetGenericResultAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
           var result = (GenericResult)stepContext.Options;
            var Lncode = result.LanguageCode.ToString();

            if (result.General != null)
            {
                var general = result.General;
                var promptMessage1 = MessageFactory.Text(general, general, InputHints.ExpectingInput);
                var translatedtext = "";
                
                translatedtext = _translator.Translation(Lncode, promptMessage1.ToString());
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
            return await stepContext.EndDialogAsync(result.General, cancellationToken);
        }

    }
}
