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
        private readonly ISimpleResolver _simpleResolver;
        private readonly IKkvService _kkvService;
        private readonly KvBotAccessors _accessors;
        private readonly ILogger _logger;

        public KvBot(ConversationState conversationState, 
            ISimpleResolver simpleResolver,
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

            _simpleResolver = simpleResolver;
            _kkvService = kkvService;

            _accessors = new KvBotAccessors(conversationState)
            {
                KvvBuildingState = conversationState.CreateProperty<KkvBuildingState>(KvBotAccessors.KvvBuildingStateName),
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
            if (_simpleResolver.TryResolve(turnContext.Activity, out string message))
            {
                await turnContext.SendActivityAsync(message);
                return;
            }

            if (turnContext.Activity.Text.StartsWith("kv ", StringComparison.OrdinalIgnoreCase))
            {
                await _accessors.ConversationState.LoadAsync(turnContext);
                var state = await _accessors.KvvBuildingState.GetAsync(turnContext, () => new KkvBuildingState());
                if (_kkvService.TryParse(turnContext.Activity.Text.Substring("kv ".Length), out (string scope, string value, string[] keys) kkv,
                    out (KkvParserCodes code, string message) operationResult))
                {
                    if (operationResult.code == KkvParserCodes.KeysAndValue)
                    {
                        state.SetKeys(kkv.keys);
                        state.SetValue(kkv.value);
                    }
                    else if (operationResult.code == KkvParserCodes.KeysOnly)
                    {
                        state.SetKeys(kkv.keys);
                    }
                    else if (operationResult.code == KkvParserCodes.ValueOnly)
                    {
                        state.SetValue(kkv.value);
                    }
                }

                if (state.IsCommitable())
                {
                    await _kkvService.SaveAsync("private", state.Value, state.Keys.ToList());

                    await _accessors.KvvBuildingState.DeleteAsync(turnContext);
                    await _accessors.ConversationState.ClearStateAsync(turnContext);

                    await turnContext.SendActivityAsync(message ?? "Saved");
                    return;
                }
            
                if (!state.IsTransient())
                {
                    await _accessors.KvvBuildingState.SetAsync(turnContext, state);
                    await _accessors.ConversationState.SaveChangesAsync(turnContext);
                    return;
                }
            }

            await turnContext.SendActivityAsync(message ?? "I don't understand");
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
