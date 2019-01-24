using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.Bot
{
    public class AprilBot : IBot
    {
        private readonly BotAccessor _accessor;
        public AprilBot(BotAccessor accessor)
        {
            _accessor = accessor ?? throw new System.ArgumentNullException(nameof(accessor)); ;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if(turnContext.Activity.Type == ActivityTypes.Message)
            {
                var botState = await _accessor.GetStateAsync(turnContext);

                string msg = turnContext.Activity.Text;

                if (msg != "Display")
                {
                    botState.DriversLicenceNumber = msg;
                    await turnContext.SendActivityAsync($"You DL Number is {botState.DriversLicenceNumber}");
                    await _accessor.SetStateAsync(turnContext, botState);
                }
                else
                {
                    await turnContext.SendActivityAsync($"Stored DL Number is {botState.DriversLicenceNumber}");
                }
            }
        }

        

    }
}
