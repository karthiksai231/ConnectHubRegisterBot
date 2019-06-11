using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConnectHubRegisterBot.Extensions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ConnectHubRegisterBot.Bots
{
    public class ConnectHubBot<T> : ActivityHandler where T : Dialog
    {
        private readonly ConversationState _conversationState;
        private readonly UserState _userState;
        private readonly Dialog _dialog;
        private readonly ILogger _logger;
        public ConnectHubBot(ConversationState conversationState, UserState userState, T dialog, ILogger<ConnectHubBot<T>> logger)
        {
            _logger = logger;
            _dialog = dialog;
            _userState = userState;
            _conversationState = conversationState;

        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Running dialog with Message Activity.");

            // Run the Dialog with the new message Activity.
            await _dialog.Run(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome to ConnectHub!"), cancellationToken);
                }
            }
        }
    }
}