using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreBot.Dialogs
{
    public class AppDialog :  ComponentDialog
    {
        private readonly AppLuisRecognizer _appLuisRecognizer;
        protected readonly ILogger Logger;

        // Dependency injection uses this constructor to instantiate MainDialog
        public AppDialog(AppLuisRecognizer luisRecognizer, NewsDialog newsDialog, ILogger<AppDialog> logger)
            : base(nameof(AppDialog))
        {
            _appLuisRecognizer = luisRecognizer;
            Logger = logger;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(newsDialog);
        }
    }
}
