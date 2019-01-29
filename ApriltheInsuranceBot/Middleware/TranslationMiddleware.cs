using ApriltheInsuranceBot.Constants;
using ApriltheInsuranceBot.NewBot;
using ApriltheInsuranceBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.Middleware
{
    public class TranslationMiddleware : IMiddleware
    {
        private readonly MicrosoftTranslator _translator;
        private readonly InsuranceBotAccessor _accessor;
        private DialogSet _dialogs;
        public TranslationMiddleware(MicrosoftTranslator translator, InsuranceBotAccessor accessor)
        {
            _translator = translator ?? throw new ArgumentNullException(nameof(translator));
            _accessor = accessor ?? throw new System.ArgumentNullException(nameof(accessor));

            _dialogs = new DialogSet(_accessor.ConversationDialogState);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }
            
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
                if (dialogContext.ActiveDialog == null)
                {
                    var model = await _translator.TranslateAsync(turnContext.Activity.Text, SupportedLanguages.DefaultLanguage, cancellationToken);

                    var botState = await _accessor.GetStateAsync(turnContext);
                    botState.UserLanguage = model.DetectedLanguage;
                    await _accessor.SetStateAsync(turnContext, botState);

                    turnContext.Activity.Text = model.Text;

                    turnContext.OnSendActivities(HandleBotResponses);
                    turnContext.OnUpdateActivity(HandleBotResponse);
                }
                else
                {
                    dialogContext.Context.OnSendActivities(HandleBotResponses);
                    dialogContext.Context.OnUpdateActivity(HandleBotResponse);
                }
            }

            await next(cancellationToken).ConfigureAwait(false);

        }

        private async Task<ResourceResponse> HandleBotResponse(ITurnContext turnContext, Activity activity, Func<Task<ResourceResponse>> next)
        {
            // Translate messages sent to the user to user language
            if (activity.Type == ActivityTypes.Message)
            {
                var botState = await _accessor.GetStateAsync(turnContext);
                if (botState.UserLanguage != SupportedLanguages.DefaultLanguage)
                {
                    await TranslateMessageActivityAsync(activity.AsMessageActivity(), botState.UserLanguage);
                }
            }

            return await next();
        }

        private async Task<ResourceResponse[]> HandleBotResponses(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var botState = await _accessor.GetStateAsync(turnContext);
            // Translate messages sent to the user to user language
            if (botState.UserLanguage != SupportedLanguages.DefaultLanguage)
            {
                List<Task> tasks = new List<Task>();
                foreach (Activity currentActivity in activities.Where(a => a.Type == ActivityTypes.Message))
                {
                    tasks.Add(TranslateMessageActivityAsync(currentActivity.AsMessageActivity(), botState.UserLanguage));
                }

                if (tasks.Any())
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }

            return await next();
        }

        private async Task TranslateMessageActivityAsync(IMessageActivity activity, string targetLocale, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (activity.Type == ActivityTypes.Message)
            {
                activity.Text = (await _translator.TranslateAsync(activity.Text, targetLocale)).Text;
            }
        }
    }
}
