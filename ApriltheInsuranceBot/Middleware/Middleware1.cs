using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApriltheInsuranceBot.Middleware
{
    public class Middleware1 : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.Message) {
                await turnContext.SendActivityAsync("Before - Middleware 1");
                await next(cancellationToken);
                await turnContext.SendActivityAsync("After - Middleware 1");
            }
            else
            {
                await next(cancellationToken);
            }
        }
    }
}
