using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI;
using Microsoft.Bot.Builder.Dialogs;

namespace ApriltheInsuranceBot.NewBot
{
    public class InsuaranceBot : IBot
    {
        private readonly InsuranceBotAccessor _accessor;

        public static readonly string LuisKey = "luis";

        private const string WelcomeText = "This bot will introduce you to natural language processing with LUIS. Type an utterance to get started";

        private DialogSet _dialogs;
        /// <summary>
        /// Services configured from the ".bot" file.
        /// </summary>
        private readonly BotServices _services;
        public InsuaranceBot(BotServices services, InsuranceBotAccessor accessor)
        {
            _accessor = accessor ?? throw new System.ArgumentNullException(nameof(accessor));
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            if (!_services.LuisServices.ContainsKey(LuisKey))
            {
                throw new System.ArgumentException($"Invalid configuration. Please check your '.bot' file for a LUIS service named '{LuisKey}'.");
            }

            // The DialogSet needs a DialogState accessor, it will call it when it has a turn context.
            _dialogs = new DialogSet(accessor.ConversationDialogState);

            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                NameStepAsync,
                NameConfirmStepAsync,
                AgeFetchStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("insuranceQuoteDialogs", waterfallSteps));
            _dialogs.Add(new TextPrompt("textPrompt"));
            _dialogs.Add(new NumberPrompt<int>("numberPrompt"));
            _dialogs.Add(new ConfirmPrompt("confirmPrompt"));

        }
        
        private async Task<DialogTurnResult> NameStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync("textPrompt", new PromptOptions {
                Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }
        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string name = (string) stepContext.Result;
            await stepContext.Context.SendActivityAsync($"Hi {name}, Thanks for entering your name.");

            var botState = await _accessor.GetStateAsync(stepContext.Context);
            botState.InsuranceQuoteForm.Name = name;
            await _accessor.SetStateAsync(stepContext.Context, botState);

            //return await stepContext.EndDialogAsync(cancellationToken);

            return await stepContext.PromptAsync("numberPrompt", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your age [Example: 23]")
            }, cancellationToken);
        }
        private async Task<DialogTurnResult> AgeFetchStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            int age = (int) stepContext.Result;
            var botState = await _accessor.GetStateAsync(stepContext.Context);

            botState.InsuranceQuoteForm.Age = age;

            await stepContext.Context.SendActivityAsync($"{botState.InsuranceQuoteForm.Name}, your age is set to {age} years.");
            await _accessor.SetStateAsync(stepContext.Context, botState);

            await stepContext.Context.SendActivityAsync("Thanks");

            return await stepContext.EndDialogAsync(cancellationToken);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                var results = await dialogContext.ContinueDialogAsync(cancellationToken);
                
                if (results.Status == DialogTurnStatus.Empty)
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
                            await dialogContext.BeginDialogAsync("insuranceQuoteDialogs", null, cancellationToken);
                            break;
                        default:
                            await turnContext.SendActivityAsync("I am unable to understand that. I am very sorry for not being able to assit you on this.");
                            break;
                    }
                }
                
            }
            await _accessor.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        public const string Intent_InsuranceQuote = "InsuranceQuote";
        public const string Intent_Greeting = "Greeting";
        public const string Intent_Capabilities = "Capabilities";
        public const string Intent_None = "None";


    }
}
