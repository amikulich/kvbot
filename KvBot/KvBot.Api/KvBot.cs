using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KvBot.Api.BotServices;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace KvBot.Api
{
    public class KvBot : IBot
    {
        private readonly IKkvService _kkvService;
        private readonly KvBotAccessors _accessors;
        private readonly ILogger _logger;

        private const string KvCommand = "kv ";

        public KvBot(ConversationState conversationState, 
            IKkvService kkvService,
            ILoggerFactory loggerFactory)
        {
            if (conversationState == null)
            {
                throw new System.ArgumentNullException(nameof(conversationState));
            }

            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _kkvService = kkvService;

            _accessors = new KvBotAccessors(conversationState)
            {
                KkvBuildingState = conversationState.CreateProperty<KkvBuildingState>(KvBotAccessors.KkvBuildingStateName),
            };

            _logger = loggerFactory.CreateLogger<KvBot>();
            _logger.LogTrace("Turn start.");
        }

        public KkvBuildingState KkvBuildingState { get; set; }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    await ProcessMessage(turnContext);
                    break;
                case ActivityTypes.ContactRelationUpdate:
                    var reply = turnContext.Activity.CreateReply("Hi! I have something to keep your mind sharp. ");

                    reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                    reply.Attachments = new List<Attachment>()
                    {
                        BuildCard("Play Math Warm Up", "", "", "", "").ToAttachment(),
                    };

                    await turnContext.SendActivityAsync(reply, cancellationToken);
                    break;
                default:
                    //await turnContext.SendActivityAsync($"{turnContext.Activity.Type} event detected");
                    break;
            }
        }

        private async Task ProcessMessage(ITurnContext turnContext)
        {
            if (_kkvService.TryFindByKey(turnContext.Activity.Text, turnContext.Activity.From.Id, out string message))
            {
                await turnContext.SendActivityAsync(message);
                return;
            }

            if (turnContext.Activity.Text.StartsWith(KvCommand, StringComparison.OrdinalIgnoreCase))
            {
                var state = await _accessors.KkvBuildingState.GetAsync(turnContext, () => new KkvBuildingState());
                if (_kkvService.TryParse(turnContext.Activity.Text.Substring(KvCommand.Length), out (KkvScope scope, string value, string[] keys) kkv,
                    out (KkvParseResult code, string message) operationResult))
                {
                    if (operationResult.code == KkvParseResult.KeysAndValue)
                    {
                        state.SetScope(kkv.scope);
                        state.SetKeys(kkv.keys);
                        state.SetValue(kkv.value);
                    }
                    else if (operationResult.code == KkvParseResult.KeysOnly)
                    {
                        state.SetKeys(kkv.keys);
                    }
                    else if (operationResult.code == KkvParseResult.ValueOnly)
                    {
                        state.SetValue(kkv.value);
                    }
                }

                if (state.IsCommitable())
                {
                    await _kkvService.SaveAsync(state.Scope == KkvScope.Private ? turnContext.Activity.From.Id : null, 
                        state.Value, 
                        state.Keys.ToList());

                    await _accessors.KkvBuildingState.DeleteAsync(turnContext);
                    await _accessors.ConversationState.ClearStateAsync(turnContext);

                    await turnContext.SendActivityAsync("Saved");
                    return;
                }
            
                if (!state.IsTransient())
                {
                    await _accessors.KkvBuildingState.SetAsync(turnContext, state);
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);
                    return;
                }
            }

            await turnContext.SendActivityAsync("Sorry, I don't understand. Consider typing help");
        }

        private static ThumbnailCard BuildCard(string title, string imageUrl, string description, string playCommand, string rulesCommand)
        {
            var playAction = new CardAction("imBack", "Play", image: null, value: playCommand);
            var rulesAction = new CardAction("imBack", "Rules", image: null, value: rulesCommand);

            return new ThumbnailCard(title,
                subtitle: null,
                text: description,
                images: new List<CardImage>()
                {
                    new CardImage(imageUrl)
                },
                buttons: new List<CardAction>()
                {
                    playAction,
                    rulesAction
                });
        }
    }
}
