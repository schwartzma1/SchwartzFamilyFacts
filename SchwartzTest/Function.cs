using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                "Maya is 9 years old.",
                "Maya is a great reader - she can read up to 20 books per day",
                "Maya Maya bo baya banana fana fo faya, mee moe Maya - Maya",
                "Maya's birthday is August 29 2009 at 5:04 am",
                "Maya is beautiful",
                "Maya is a jokester ",
                "Maya is taking piano lessons",
                "Maya is in the 4th grade at Herod elementary",
                "Maya is a soccer player she has played soccer for several years."
            });
            facts.Add("coco", new List<string>()
            {
                "Coco likes to bite people",
                "Coco is a fast racer - he likes to race in the back yard",
                "Coco loves to sleep in his cage",
                "Coco is a Maltipoo and weighs 6 pounds"
            });
           facts.Add("lily", new List<string>()
            {
                "Lily is 6 years old.",
                "Lily's best friend is Liya",
                "Lily is a beautiful dancer.  She can dance hip hop an ballet.",
                "Lily likes to wear her hair in a bun",
                "Lily's birthday is April 30 2012",
                "Lily is friendly and funny!",
                "Lily is in 2nd grade at Herod elementary",
                "Lily has an ice cream cone bike!  I wish I had one of those!"
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
                "Carlos's birthday is February 14 also known as valentines day",
                "Carlos is a cookie monster.  He eats all the cookies and all the M&Ms"
            });
            facts.Add("leslie", new List<string>()
            {
               "Leslie is cray cray",
               "Leslie is pretty",
               "Leslie has written 2 books.  They are called Fuego and Nightbloom and Cenote",
            });
            facts.Add("mommy", new List<string>()
            {
               "Mommy is nice",
            });
            facts.Add("daddy", new List<string>()
            {
               "Daddy is the best!",
               "Daddy's birthday is January 19, 1979",
               "Daddy is from Cleveland Ohio",
            });
            facts.Add("cici", new List<string>()
            {
               "Cici cici bo bici banana fana fo fici, mee my mo Mici, Cici",
               "Cici is a grandma!"
            });
            facts.Add("poppop", new List<string>()
            {
               "Pop pop is a grandpa!",
               "Pop pop's birthday is January second"
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

        public string emitBirthdayFact(ILambdaContext context, String person)
        {
            var log = context.Logger;
            if (person != null && person.Length > 0)
            {
                // trim the plural form back to singular form
                int indexOfSingleQuote = person.IndexOf('\'');
                if (indexOfSingleQuote > 0)
                    person = person.Substring(0, indexOfSingleQuote);
                person = person.ToLower();
                log.LogLine($"emitting birthday fact for " + person);
                DateTime today = DateTime.Today;
                DateTime birthday = DateTime.Today;
                if (person == "maya")
                {
                    birthday = new DateTime(DateTime.Today.Year, 8, 29);
                }
                else if (person == "lily")
                {
                    birthday = new DateTime(DateTime.Today.Year, 4, 30);
                }
                else if (person == "carlos")
                {
                    birthday = new DateTime(DateTime.Today.Year, 2, 14);
                }
                else return "sorry I don't know who that is!";

                DateTime next = new DateTime(today.Year, birthday.Month, birthday.Day);
                if (next < today)
                    next = next.AddYears(1);

                int numDays = (next - today).Days;
                String suffix = "";
                if (numDays == 0)
                {
                    suffix = "It is " + person + "'s birthday!  Happy birthday to you!";
                }
                else if (numDays < 10)
                    suffix = "It is getting close to " + person + "'s birthday!  You better be ready!";
               
                return person + "'s birthday is in " + numDays + " days.  " + suffix;
            }
            return "invalid person was received";
        }


        public string emitNewFact(ILambdaContext context, FactResource resource, String person, Dictionary<string, List<int>> previousFactsEmitted, bool withPreface, ref String personUsed, ref int factIndex)
        {
            Random r = new Random();
            var log = context.Logger;
            int nextRandom;
            int value = r.Next(resource.Facts.Count);
            personUsed = resource.Facts.Keys.ElementAt(value);
            int numberOfItems = resource.Facts.ElementAt(value).Value.Count;
            if (person != null && person.Length > 0)
            {
                personUsed = person;
                person = person.ToLower();
                log.LogLine($"emitting a fact for" + person);
                if (resource.Facts.ContainsKey(person))
                {
                    log.LogLine(resource.Facts.Keys.ToString());
                    nextRandom = r.Next(resource.Facts[person].Count);
                    if (previousFactsEmitted.ContainsKey(person))
                    {
                        List<int> factsAlreadyUsed = previousFactsEmitted[person];
                        int iter = 0;
                        if (!factsAlreadyUsed.Contains(nextRandom))
                        {
                            log.LogLine($"This was a unique fact - using it!");
                        }
                        while (factsAlreadyUsed.Contains(nextRandom))
                        { 
                            log.LogLine($"Getting another fact because that one was already used.");
                            iter++;
                            nextRandom = r.Next(resource.Facts[person].Count);
                            if (iter >= 7)
                                break;
                        }
                        
                    }
                    factIndex = nextRandom;
                    return resource.Facts[person][nextRandom];
                }
                else
                {
                    return "Sorry I don't know who " + person + " is, but here is a fact." +
                            resource.Facts.ElementAt(value).Value[r.Next(numberOfItems)];
                }
            }

            nextRandom = r.Next(numberOfItems);
            factIndex = nextRandom;
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
            Reprompt reprompt = new Reprompt();
            reprompt.OutputSpeech = new PlainTextOutputSpeech();
            ((PlainTextOutputSpeech)reprompt.OutputSpeech).Text = "How about another one?";
            response.Response.Reprompt = reprompt;
            IOutputSpeech innerResponse = null;
            var log = context.Logger;
            int factIndex = -1;
            string personUsed = "";
            var allResources = GetResources();
            var resource = allResources.FirstOrDefault();

            Dictionary<string, List<int>> factsEmitted = new Dictionary<string, List<int>>();
            if (input.Session?.Attributes?.ContainsKey("FactsEmitted") == true)
            {
                log.LogLine($"Getting the string");
                JObject s = (JObject)input.Session.Attributes["FactsEmitted"];
                log.LogLine($"Got the string");
                factsEmitted = JsonConvert.DeserializeObject< Dictionary<string, List<int>>>(s.ToString());
            }

            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                log.LogLine($"Default LaunchRequest made: 'Alexa, open Schwartz Family Facts");
                innerResponse = new PlainTextOutputSpeech();
                (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(context, resource, "", factsEmitted, true, ref personUsed, ref factIndex);
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
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(context, resource, person, factsEmitted, false, ref personUsed, ref factIndex);
                        break;
                    case "GetNewFactIntent":
                        log.LogLine($"GetFactIntent sent: send new fact");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitNewFact(context, resource, person, factsEmitted, false, ref personUsed, ref factIndex);
                        break;
                    case "BirthdayIntent":
                        log.LogLine($"BirthdayIntent sent: send new birthday");
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = emitBirthdayFact(context, person);
                        break;
                    default:
                        log.LogLine($"Unknown intent: " + intentRequest.Intent.Name);
                        innerResponse = new PlainTextOutputSpeech();
                        (innerResponse as PlainTextOutputSpeech).Text = resource.HelpReprompt;
                        break;
                }
            }
            else if (input.GetRequestType() == typeof(SessionEndedRequest))
            {
                response.Response.ShouldEndSession = true;
                ((PlainTextOutputSpeech)reprompt.OutputSpeech).Text = "May the Schwartz be with you!";
            }
            if (!string.IsNullOrEmpty(personUsed))
            {
                if (factsEmitted.ContainsKey(personUsed))
                {
                    factsEmitted[personUsed].Add(factIndex);
                }
                else
                {
                    factsEmitted.Add(personUsed, new List<int>() { factIndex });
                }
            }
            response.SessionAttributes = new Dictionary<string, object>();
            response.SessionAttributes.Add("FactsEmitted", factsEmitted);
            response.Response.OutputSpeech = innerResponse;
            response.Version = "1.0";
            return response;
        }
    }
}
