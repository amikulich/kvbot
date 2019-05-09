// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;

namespace KvBot.Api
{
    public class KvBotAccessors
    {
        public KvBotAccessors(ConversationState conversationState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
        }

        public static string KkvBuildingStateName { get; } = $"{nameof(KvBotAccessors)}.KkvBuildingState";

        public IStatePropertyAccessor<KkvBuildingState> KkvBuildingState { get; set; }

        public ConversationState ConversationState { get; }
    }
}
