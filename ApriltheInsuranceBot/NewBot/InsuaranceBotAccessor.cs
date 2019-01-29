using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.NewBot
{
    public class InsuaranceBotAccessor
    {
        public ConversationState ConversationState { get; }

        public IStatePropertyAccessor<InsuaranceState> InsuaranceState { get; set; }

        public static string InsuaranceStateName = "InsuaranceStateKey";

        public InsuaranceBotAccessor(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public async Task SetStateAsync(ITurnContext turnContext, InsuaranceState insState)
        {
            await InsuaranceState.SetAsync(turnContext, insState);
            await ConversationState.SaveChangesAsync(turnContext);
        }
        public async Task<InsuaranceState> GetStateAsync(ITurnContext turnContext)
        {
            return await InsuaranceState.GetAsync(turnContext, () => new InsuaranceState());
        }
    }
}
