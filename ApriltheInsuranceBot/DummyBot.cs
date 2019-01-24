using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot
{
    public class DummyBot : IBot
    {
        private readonly ApriltheInsuranceBotAccessors _accessor;

        public DummyBot(ApriltheInsuranceBotAccessors accessor)
        {
            _accessor = accessor ?? throw new System.ArgumentNullException(nameof(accessor)); ;
        }

        public async Task OnTurnAsync(ITurnContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (context.Activity.Type == ActivityTypes.Message) {

                var ctrState = await _accessor.CounterState.GetAsync(context, () => new CounterState());

                ctrState.TurnCount++;

                string text = context.Activity.Text;
                string reply = $"[{ctrState.TurnCount}]The length of {text} is {text.Length}";
                await context.SendActivityAsync(reply);

                await _accessor.CounterState.SetAsync(context, ctrState);
                await _accessor.ConversationState.SaveChangesAsync(context);
            }
            else
            {
                await context.SendActivityAsync($"Activity.Type: {context.Activity.Type}");
            }
        }
    }
}
