using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace SchwartzTest
{

    public class FactResource
    {
        public FactResource(string language)
        {
            this.Language = language;
            this.Facts = new Dictionary<string, List<string>>();
        }

        public string Language { get; set; }
        public string SkillName { get; set; }
        public Dictionary<string, List<string>> Facts { get; set; }
        public string GetFactMessage { get; set; }
        public string HelpMessage { get; set; }
        public string HelpReprompt { get; set; }
        public string StopMessage { get; set; }
    }

    public class Function
    {

        public List<FactResource> GetResources()
        {
            List<FactResource> resources = new List<FactResource>();
            FactResource enUSResource = new FactResource("en-US");
            enUSResource.SkillName = "Schwartz Family Facts";
            enUSResource.GetFactMessage = "Here's your Schwartz family fact: ";
            enUSResource.HelpMessage = "You can say tell me a Schwartz fact, or, you can say exit... What can I help you with?";
            enUSResource.HelpReprompt = "What can I help you with?";
            enUSResource.StopMessage = "Goodbye!";
            Dictionary<string, List<string>> facts = new Dictionary<string, List<string>>();
            facts.Add("maya", new List<string>()
            {
                "Maya is 8 years old.",
                "Maya's best friend is Tahlia",
                "Maya is a great reader - she can read up to 20 books per day",
                "Maya Maya bo baya banana fana fo faya, mee moe Maya - Maya",
                "Maya's birthday is August 29 2009 at 5:04 am",
                "Maya is beautiful",
                "Maya is a jokester "
            });
            facts.Add("lily", new List<string>()
            {
                "Lily is 5 years old.",
                "Lily's best friend is Liya",
                "Lily is a beautiful dancer.  She can dance hip hop an ballet.",
                "Lily likes to wear her hair in a bun",
                "Lily's birthday is April 30 2012",
                "Lily is friendly and funny!"
            });
            facts.Add("carlos", new List<string>()
            {
                "Carlos is 2 years old",
                "Carlos loves Cars 3.  He loves to play cars and race on the track.",
                "Carlos's favorite character is Miss Fritter",
                "Carlos's favorite song is Mika Moka",
                "Carlos is a grump grump",
                "Carlos likes to do interactive performances of Cars 3",
                "Carlos Carlos bo Barlos banana fana fo farlos, me my mo Marlos, Carlos",
                "Carlos's birthday is February 14 a.ka. valintines day"
            });
            facts.Add("leslie", new List<string>()
            {
               "Leslie is cray cray",
               "Leslie is pretty"
            });
            facts.Add("mommy", new List<string>()
            {
               "Mommy is nice"
            });
            facts.Add("daddy", new List<string>()
            {
               "Daddy is the best!"
            });
            facts.Add("Cici", new List<string>()
            {
               "Cici cici bo bici banana fana fo fici, mee my mo Mici, Cici",
               "Cici is a grandma!"
            });
            facts.Add("tahlia", new List<string>()
            {
                "Tahlia is awesome!",
                "Tahlia's teacher is Mrs. Wheelock",
                "Tahlia's mommy is named Cici",
                "Tahlia is a girl scout",
                "Tahlia is nice"
            });
            facts.Add("grandparents", new List<string>()
            {
                "Michael's parents are Kenny and Lucy also known as Pop-pop and Cici",
                "Leslie's parents are David and Amelia, also known as Pawpaw and Nana"

            });
            enUSResource.Facts = facts;
            resources.Add(enUSResource);
            return resources;
        }

        public string emitNewFact(ILambdaContext context, FactResource resource, String person, bool withPreface)
        {
            Random r = new Random();
            var log = context.Logger;
            int value = r.Next(resource.Facts.Count);
            if (person != null && person.Length > 0)
            {
                person = person.ToLower();
                log.LogLine($"emitting a fact for" + person);
                log.LogLine(resource.Facts.Keys.ToString());
                return resource.Facts[person][r.Next(resource.Facts[person].Count)];
            }
            int numberOfItems = resource.Facts.ElementAt(value).Value.Count;
            if (withPreface)
                return resource.GetFactMessage +
                       resource.Facts.ElementAt(value).Value[r.Next(numberOfItems)];
            return resource.Facts.ElementAt(value).Value[r.Next(numberOfItems)];
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse();
            response.Response = new ResponseBody();
            response.Response.ShouldEndSession = false;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;

            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Schwartz Family Facts");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(context, resource, "", true);
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;
                var person = "";
                if (intentRequest.Intent.Slots.ContainsKey("name"))
                {
                    if (intentRequest.Intent.Slots["name"] != null)
                    {
                        log.LogLine($"trying to get person");
                        log.LogLine("name: " + intentRequest.Intent.Slots["name"].Name);
                        log.LogLine("value: " + intentRequest.Intent.Slots["name"].Value);
                        person = intentRequest.Intent.Slots["name"].Value;
                        log.LogLine(person);
                    }
                }
                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        log.LogLine($"AMAZON.CancelIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.StopIntent":
                        log.LogLine($"AMAZON.StopIntent: send StopMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.StopMessage;
                        response.Response.ShouldEndSession = true;
                        break;
                    case "AMAZON.HelpIntent":
                        log.LogLine($"AMAZON.HelpIntent: send HelpMessage");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpMessage;
                        break;
                    case "GetFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(context, resource, person, false);
                        break;
                    case "GetNewFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(context, resource, person, false);
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }
            }
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            return response;
        }
    }
}
