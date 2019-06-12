using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Newtonsoft.Json;

namespace EchoBot.Dialogs
{
    public class RegistrationDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public RegistrationDialog(UserState userState) : base(nameof(RegistrationDialog))
        {
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            var waterFallSteps = new WaterfallStep[]
            {
                GenderStepAsync,
                UserNameAsync,
                KnownAsAsync,
                NameConfirmStepAsync,
                DateOfBirthAsync,
                CityAsync,
                CountryAsync,
                PasswordAsync,
                SummaryStepAsync
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterFallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> GenderStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("Please enter your gender."),
                    Choices = ChoiceFactory.ToChoices(new List<string> { "Male", "Female" }),
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> UserNameAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["gender"] = ((FoundChoice)stepContext.Result).Value;

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your username.") }, cancellationToken);
        }

        private static async Task<DialogTurnResult> KnownAsAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["username"] = ((string)stepContext.Result);

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("What are you KnownAs?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> NameConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["knownas"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your DateOfBirth (MM/DD/YYYY).") }, cancellationToken);
        }

        private async Task<DialogTurnResult> DateOfBirthAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["dateofbirth"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your City.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> CityAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["city"] = (string)stepContext.Result;

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your Country.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> CountryAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["country"] = (string)stepContext.Result;

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your Password.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> PasswordAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var msg = !string.IsNullOrWhiteSpace((string)stepContext.Result) ?
             "I have got your password." : "Please re-enter the password";

             stepContext.Values["password"] = (string)stepContext.Result;

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["password"] = (string)stepContext.Result;

            var msg = !string.IsNullOrWhiteSpace(stepContext.Values["password"].ToString()) ?
             "I have got your password." : "Please re-enter the password";

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                userProfile.UserName = (string)stepContext.Values["username"];
                userProfile.KnownAs = (string)stepContext.Values["knownas"];
                userProfile.DateOfBirth = (string)stepContext.Values["dateofbirth"];
                userProfile.City = (string)stepContext.Values["city"];
                userProfile.Country = (string)stepContext.Values["country"];
                userProfile.Password = (string)stepContext.Values["password"];
                userProfile.Gender = (string)stepContext.Values["gender"];

                var msg = $"I have your name as {userProfile.UserName}, DateOfBirth as {userProfile.DateOfBirth}, city as {userProfile.City}, country as {userProfile.Country}";

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please wait while registering!!!"), cancellationToken);
                var result = await RegisterUser(userProfile, stepContext, cancellationToken);

                if (result != null)
                {
                    if(!string.IsNullOrWhiteSpace(result.UserName))
                    {
                        await stepContext.Context
                            .SendActivityAsync(
                            MessageFactory.Text($"You can now login as UserName: {result.UserName} with Password: {userProfile.Password}"), cancellationToken);
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be kept."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<UserProfile> RegisterUser(UserProfile userProfile, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string apiResponse = string.Empty;
            try
            {
                using (var client = new HttpClient())
                {
                    var requestUri = new Uri("https://connecthubonline.com/api/auth/register");
                    var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

                    var requestBody = JsonConvert.SerializeObject(userProfile);

                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    var result = await client.SendAsync(request);

                    apiResponse = result.Content.ReadAsStringAsync().Result;

                    // Attempt to deserialise the reponse to the desired type, otherwise throw an expetion with the response from the api.
                    if (apiResponse != "")
                        return JsonConvert.DeserializeObject<UserProfile>(apiResponse);
                }
            }
            catch (Exception)
            {
                var message = !string.IsNullOrWhiteSpace(apiResponse) ? apiResponse : "unknown";
                await stepContext.Context
                    .SendActivityAsync(MessageFactory
                    .Text($"Unable to register please try again ({message})"), cancellationToken);
            }

            return null;
        }
    }
}
