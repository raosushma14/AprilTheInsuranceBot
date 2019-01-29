using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI;

namespace ApriltheInsuranceBot.NewBot
{
    public class InsuaranceBot : IBot
    {
        private readonly InsuaranceBotAccessor _accessor;

        public static readonly string LuisKey = "luis";

        private const string WelcomeText = "This bot will introduce you to natural language processing with LUIS. Type an utterance to get started";

        /// <summary>
        /// Services configured from the ".bot" file.
        /// </summary>
        private readonly BotServices _services;
        public InsuaranceBot(BotServices services)
        {
            //_accessor = accessor ?? throw new System.ArgumentNullException(nameof(accessor)); ;
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            if (!_services.LuisServices.ContainsKey(LuisKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a LUIS service named '{LuisKey}'.");
            }
        }
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                // Check LUIS model
                var recognizerResult = await _services.LuisServices[LuisKey].RecognizeAsync(turnContext, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();

                switch (topIntent.Value.intent)
                {
                    case Intent_None:
                        await turnContext.SendActivityAsync("Sorry, I wasn't quite able to get that. Can you try re-phrasing your sentence, please?");
                        break;
                    case Intent_Greeting:
                        await turnContext.SendActivityAsync("Hi, I am Juliet, a new recuit to BBB Auto Insurance Company. " +
                            "I am virtual assistant who's always ready to help you with your insurance needs");
                        await turnContext.SendActivityAsync("How can I help you today?");
                        
                        break;
                    case Intent_Capabilities:
                        await turnContext.SendActivityAsync("I can help you get an insurance quote for your car.\nTry saying,\n*I need an insurance quote*");
                        break;
                    case Intent_InsuranceQuote:
                        await turnContext.SendActivityAsync("Sure, I can help with getting an insurance quote");
                        break;
                    default:
                        await turnContext.SendActivityAsync("I am unable to understand that. I am very sorry for not being able to assit you on this.");
                        break;
                }
                
            }
            
        }

        public const string Intent_InsuranceQuote = "InsuranceQuote";
        public const string Intent_Greeting = "Greeting";
        public const string Intent_Capabilities = "Capabilities";
        public const string Intent_None = "None";


    }
}
