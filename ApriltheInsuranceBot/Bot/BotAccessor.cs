using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.Bot
{
    public class BotAccessor
    {
        public ConversationState ConversationState { get; }

        public IStatePropertyAccessor<BotState> BotState { get; set; }

        public static string BotStateName = "BotStateKey";

        public BotAccessor(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public async Task SetStateAsync(ITurnContext turnContext, BotState botState)
        {
            await BotState.SetAsync(turnContext, botState);
            await ConversationState.SaveChangesAsync(turnContext);
        }
        public async Task<BotState> GetStateAsync(ITurnContext turnContext)
        {
            return await BotState.GetAsync(turnContext, () => new BotState());
        }

    }
}
