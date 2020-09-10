using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;

namespace CoreBot.CognitiveModels
{
    public static class LuisModelParser
    {
        public static CovidResult ParseLuisNewsRequest(this LuisResult luisResult)
        {
            var covidResult = new CovidResult();

            foreach (var entity in luisResult.Entities)
            {

                if (entity.Type == "Country")
                {
                    covidResult.Country = entity.ProcessNews();
                    covidResult.Result += "In " + covidResult.Country;
                }
                else if (entity.Type == "Active")
                {
                    covidResult.Active = entity.ProcessActiveCases();
                    covidResult.Result += "active cases are: " + covidResult.Active;
                }
                else if (entity.Type == "Death")
                {
                    covidResult.Death = entity.ProcessFailedCases();
                    covidResult.Result += " death cases are: " + covidResult.Death;
                }
            }
            return covidResult;

        }
    }

}
