using Microsoft.Bot.Builder;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Bot.Builder.AI.Luis;

namespace CoreBot.CognitiveModels
{
    public class LuisIntentEntity : IRecognizerConvert
    {
       

        public string Text;
        public string AlteredText;

            public enum Intent
        {
            CovidNews,
            Cancel,
            SportsNews,
            Lockdown,
            Generic,
            None
        };

        public class _Entities
        {
            // Built-in entities
            public DateTimeSpec[] datetime;

            // Lists
            public string[][] Active;

            //Lists
            public string[][] Country;

            //Lists
            public string[][] Death;

            //Lists
            public string[][] Recovered;

            //String
            public string[][] General;

            //string
            public string[][] CricketNews;
            public string[][] Lockdowndays;

            // Instance
            public class _Instance
            {
                public InstanceData[] datetime;
                public InstanceData[] Active;
                public InstanceData[] Country;
                public InstanceData[] Death;
                public InstanceData[] CricketNews;
                public InstanceData[] Lockdowndays;
                public InstanceData[] General;
                public InstanceData[] Recovered;
            }
            [JsonProperty("$instance")]
            public _Instance _instance;
        }

        public _Entities Entities;

        [JsonExtensionData(ReadData = true, WriteData = true)]
        public IDictionary<string, object> Properties { get; set; }
        public Dictionary<Intent, IntentScore> Intents;

        public void Convert(dynamic result)
        {
            var app = JsonConvert.DeserializeObject<LuisIntentEntity>(JsonConvert.SerializeObject(result, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
            Text = app.Text;
            AlteredText = app.AlteredText;
            Intents = app.Intents;
            Entities = app.Entities;
            Properties = app.Properties;
        }

        public (Intent intent, double score) TopIntent()
        {
            Intent maxIntent = Intent.None;
            var max = 0.0;
            foreach (var entry in Intents)
            {
                if (entry.Value.Score > max)
                {
                    maxIntent = entry.Key;
                    max = entry.Value.Score.Value;
                }
            }
            return (maxIntent, max);
        }
    }
}
