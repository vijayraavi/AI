using Luis;
using Microsoft.Bot.Builder.Dialogs;

namespace peloton
{
    public class SkillConversationState : DialogState
    {
        public SkillConversationState()
        {
        }

        public string Token { get; internal set; }
        public pelotonLU LuisResult { get; set; }

        public void Clear()
        {
        }
    }
}