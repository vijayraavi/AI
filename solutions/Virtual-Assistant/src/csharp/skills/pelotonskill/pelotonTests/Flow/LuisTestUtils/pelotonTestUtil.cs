using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Solutions.Testing.Fakes;
using pelotonTests.Flow.Utterances;
using System.Collections.Generic;

namespace pelotonTests.Flow.LuisTestUtils
{
    public class pelotonTestUtil
    {
        private static Dictionary<string, IRecognizerConvert> _utterances = new Dictionary<string, IRecognizerConvert>
        {
            { SampleDialogUtterances.Trigger, CreateIntent(SampleDialogUtterances.Trigger, pelotonLU.Intent.workout) },
        };

        public static MockLuisRecognizer CreateRecognizer()
        {
            var recognizer = new MockLuisRecognizer(defaultIntent: CreateIntent(string.Empty, pelotonLU.Intent.None));
            recognizer.RegisterUtterances(_utterances);
            return recognizer;
        }

        public static pelotonLU CreateIntent(string userInput, pelotonLU.Intent intent)
        {
            var result = new pelotonLU
            {
                Text = userInput,
                Intents = new Dictionary<pelotonLU.Intent, IntentScore>()
            };

            result.Intents.Add(intent, new IntentScore() { Score = 0.9 });

            result.Entities = new pelotonLU._Entities
            {
                _instance = new pelotonLU._Entities._Instance()
            };

            return result;
        }
    }
}
