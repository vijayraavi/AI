using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Solutions.Extensions;
using Microsoft.Bot.Solutions.Skills;
using peloton.Dialogs.Sample.Resources;
using peloton.Dialogs.Shared;
using peloton.ServiceClients;

namespace peloton.Dialogs.Sample
{
    public class SampleDialog : SkillTemplateDialog
    {
        public SampleDialog(
            SkillConfigurationBase services,
            IStatePropertyAccessor<SkillConversationState> conversationStateAccessor,
            IStatePropertyAccessor<SkillUserState> userStateAccessor,
            IServiceManager serviceManager,
            IBotTelemetryClient telemetryClient)
            : base(nameof(SampleDialog), services, conversationStateAccessor, userStateAccessor, serviceManager, telemetryClient)
        {
            var sample = new WaterfallStep[]
            {        
                PrintMessage,
                End,
            };

            AddDialog(new WaterfallDialog(nameof(SampleDialog), sample));
            AddDialog(new TextPrompt(DialogIds.MessagePrompt));
        }

        private async Task<DialogTurnResult> PromptForMessage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var prompt = stepContext.Context.Activity.CreateReply(SampleResponses.MessagePrompt);
            return await stepContext.PromptAsync(DialogIds.MessagePrompt, new PromptOptions { Prompt = prompt });
        }

        private async Task<DialogTurnResult> PrintMessage(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //var tokens = new StringDictionary
            //{
            //    { "Message", stepContext.Result.ToString() },
            //};

            //var response = stepContext.Context.Activity.CreateReply(SampleResponses.MessageResponse, ResponseBuilder, tokens);

            var state = await ConversationStateAccessor.GetAsync(stepContext.Context, () => new SkillConversationState());

            switch (state.LuisResult.TopIntent().intent)
            {
                case pelotonLU.Intent.workout:

                    string action = null;
                    string type = null;

                    if (state.LuisResult.Entities.Action != null)
                    {
                        action = state.LuisResult.Entities.Action[0][0].ToString();
                    }

                    if (state.LuisResult.Entities.workout != null)
                    {
                        type = state.LuisResult.Entities.workout[0][0].ToString();
                    }

                    await stepContext.Context.SendActivityAsync($"workout: {action ?? "no action"}, type={type ?? "no type"}", speak: $"{action} {type}");

                    if (action != null && type != null)
                    {
                        await SendActionToDevice(stepContext, action, type);
                    }
                    break;
                case pelotonLU.Intent.Speed:

                    string speed = null;

                    if (state.LuisResult.Entities.Setting != null)
                    {
                        speed = state.LuisResult.Entities.Setting[0][0].ToString();
                                                 
                        await stepContext.Context.SendActivityAsync($"{speed} speed");

                        await SendActionToDevice(stepContext, "Speed", speed);
                    }
                    else if (state.LuisResult.Entities.dimension != null)
                    {
                        var dimension = state.LuisResult.Entities.dimension[0];
                        await stepContext.Context.SendActivityAsync($"Setting speed to {dimension.Number} {dimension.Units}");

                        await SendActionToDevice(stepContext, "Speed", $"{dimension.Number} {dimension.Units}");
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"No setting change found, I would normally prompt with options at this point.");
                    }

                    break;
                case pelotonLU.Intent.bootcamp:
                    string bootcampAction = null;

                    if (state.LuisResult.Entities.Action != null)
                    {
                        bootcampAction = state.LuisResult.Entities.Action[0][0].ToString();
    
                        await stepContext.Context.SendActivityAsync($"workout: {bootcampAction} Bootcamp", speak: $"{bootcampAction} Bootcamp");
                        await SendActionToDevice(stepContext, bootcampAction, "Bootcamp");
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"Starting Bootcamp guide.");
                    }

                    break;
                case pelotonLU.Intent.ChangeChannel:
                    await stepContext.Context.SendActivityAsync("Changing Channel");
                    break;
                case pelotonLU.Intent.Incline:
                    string setting  = null;

                    if (state.LuisResult.Entities.Setting != null)
                    {
                        setting = state.LuisResult.Entities.Setting[0][0].ToString();

                        await stepContext.Context.SendActivityAsync($"{setting} incline");

                        await SendActionToDevice(stepContext, "Incline", setting);
                    }
                    else
                    {
                        await stepContext.Context.SendActivityAsync($"No incline change found, I would normally prompt with options at this point.");
                    }
                    break;
                case pelotonLU.Intent.ShowGuide:
                    await stepContext.Context.SendActivityAsync("Show Guide");
                    await SendActionToDevice(stepContext, "ShowGuide", string.Empty);
                    break;
                case pelotonLU.Intent.WatchTV:
                    await stepContext.Context.SendActivityAsync("Watch TV");
                    await SendActionToDevice(stepContext, "WatchTV", string.Empty);
                    break;
            }

            return await stepContext.NextAsync();
        }

        private async Task SendActionToDevice(WaterfallStepContext sc, string name, string value)
        {            
            var actionEvent = sc.Context.Activity.CreateReply();
            actionEvent.Type = ActivityTypes.Event;
            actionEvent.Name = $"DEVICE.{name}";
            actionEvent.Value = value;

            await sc.Context.SendActivityAsync(actionEvent);
        }

        private Task<DialogTurnResult> End(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return stepContext.EndDialogAsync();
        }

        private class DialogIds
        {
            public const string MessagePrompt = "messagePrompt";
        }
    }
}
