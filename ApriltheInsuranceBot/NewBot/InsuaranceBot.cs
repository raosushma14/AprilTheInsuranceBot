using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI;
using Microsoft.Bot.Builder.Dialogs;
using ApriltheInsuranceBot.Services;
using Microsoft.Bot.Builder.Dialogs.Choices;
using SMSGlobal;

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
        private readonly VinDecoder _vinDecoder;

        public InsuaranceBot(BotServices services, InsuranceBotAccessor accessor, VinDecoder vinDecoder)
        {
            _accessor = accessor ?? throw new System.ArgumentNullException(nameof(accessor));
            _services = services ?? throw new System.ArgumentNullException(nameof(services));
            _vinDecoder = vinDecoder ?? throw new System.ArgumentNullException(nameof(vinDecoder));

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
                AgeFetchStepAsync,
                LicenseFetchAsync,
                VINFetchAsync,
                ConfirmVehicleInfoAsync,
                ProcessChoiceActionAsync,
                SendUserQuoteAsync,
       
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            _dialogs.Add(new WaterfallDialog("insuranceQuoteDialogs", waterfallSteps));
            _dialogs.Add(new TextPrompt("textPrompt"));
            _dialogs.Add(new NumberPrompt<int>("numberPrompt"));
            _dialogs.Add(new ConfirmPrompt("confirmPrompt"));
            _dialogs.Add(new ChoicePrompt("choicePrompt"));


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

            return await stepContext.PromptAsync("textPrompt", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your Driver's Licence Number Eg: ")
            }, cancellationToken);
           // return await stepContext.EndDialogAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> LicenseFetchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string licenseNumber = (string)stepContext.Result;
            var botState = await _accessor.GetStateAsync(stepContext.Context);
            botState.InsuranceQuoteForm.LicenseNumber = licenseNumber;
            await stepContext.Context.SendActivityAsync($" , Thanks for entering your Driver's License Number.");
            //await stepContext.Context.SendActivityAsync($" {botState.InsuranceQuoteForm.Name}, Thanks for entering your Driver's License Number.");
            await stepContext.Context.SendActivityAsync($"  Your Driver's License Number is set to {botState.InsuranceQuoteForm.LicenseNumber}");

            await _accessor.SetStateAsync(stepContext.Context, botState);
            return await stepContext.PromptAsync("textPrompt", new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your Vehicle Identification Number(VIN) Number Eg: ")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> VINFetchAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string vin = ((string)stepContext.Result).Trim();
            var botState = await _accessor.GetStateAsync(stepContext.Context);
            botState.InsuranceQuoteForm.VIN = vin;
            await _accessor.SetStateAsync(stepContext.Context, botState);
            
            await stepContext.Context.SendActivityAsync($"Great, your VIN is recorded as **{botState.InsuranceQuoteForm.VIN}**");

            var vehicle = await _vinDecoder.DecodeVINAsync(vin);
            await stepContext.Context.SendActivityAsync($"Here's what I found for the specified VIN\n" +
                $"**Make:** {vehicle.Make}\n**Model:** {vehicle.Model}\n**Year:** {vehicle.Year}");

            return await stepContext.PromptAsync("confirmPrompt", new PromptOptions {
                Prompt = MessageFactory.Text("Do you confirm whether this information is correct?")
            }, cancellationToken);

            //return await stepContext.EndDialogAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmVehicleInfoAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            bool yes = (bool) stepContext.Result;
            if (yes)
            {
                //put quote

                return await stepContext.PromptAsync("choicePrompt", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please select how you want to get the quote"),
                    RetryPrompt = MessageFactory.Text("Sorry, please choose from the options."),
                    Choices = ChoiceFactory.ToChoices(choices),
                }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync("Sorry, I was not able to track the Vehicle details. Please try again next time");
                return await stepContext.EndDialogAsync(cancellationToken);
            }

           
        }

        private List<string> choices = new List<string> { "Message", "Email", "Nevermind" };

        private async Task<DialogTurnResult> ProcessChoiceActionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var choice = (FoundChoice) stepContext.Result;

            if (choice.Value == choices[0])
            {
                var botState = await _accessor.GetStateAsync(stepContext.Context);
                botState.UserChoice = choices[0];
                await _accessor.SetStateAsync(stepContext.Context, botState);
                return await stepContext.PromptAsync("textPrompt", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Can you please enter your phone number?")
                }, cancellationToken);
            }
            else if(choice.Value == choices[1])
            {
                var botState = await _accessor.GetStateAsync(stepContext.Context);
                botState.UserChoice = choices[1];
                await _accessor.SetStateAsync(stepContext.Context, botState);
                return await stepContext.PromptAsync("textPrompt", new PromptOptions
                {
                    Prompt = MessageFactory.Text("Can you please enter your email id?")
                }, cancellationToken);
            }
            else if(choice.Value == choices[2])
            {
                await stepContext.Context.SendActivityAsync("Sure, Thank you for your time. Have a great rest of the day");
                return await stepContext.EndDialogAsync(cancellationToken);
            }
            //else { }

            return await stepContext.EndDialogAsync(cancellationToken);
        }

        private async Task<DialogTurnResult> SendUserQuoteAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var botState = await _accessor.GetStateAsync(stepContext.Context);
            if(botState.UserChoice == "Message")
            {
                var phoneNumber = ((string)stepContext.Result).Trim();
                await SMSService.SendMessageAsync($"1{phoneNumber}", "Thank you for choosing BBB Insurance. Please contact the number below with you quote number.\n" +
                    "Toll-Free: 1 800 123 9999");
            }
            else if(botState.UserChoice == "Email")
            {

            }
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
                        case Intent_FAQs:
                            await turnContext.SendActivityAsync("Sure, I can help with your questions. \n Ask me your doubts..");
                            break;
                        case Intent_cover:
                            await turnContext.SendActivityAsync("Auto insurance covers you, your car and others involved in a vehicular accident.");
                            break;
                        case Intent_change:
                            await turnContext.SendActivityAsync("Yes, but you will be charged a cancellation fee. If you wait until the end of your policy, then you won’t be charged.");
                            break;
                        case Intent_deductible:
                            await turnContext.SendActivityAsync("An insurance deductible is the amount of money you pay after an accident before your insurance company pays for the remaining amount.");
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
        public const string Intent_FAQs = "FAQS";
        public const string Intent_cover = "insuranceCover";
        public const string Intent_deductible = "insuranceDeductible";
        public const string Intent_change = "ChangeCoverage";


    }
}
