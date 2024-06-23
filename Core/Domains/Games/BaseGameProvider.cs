using Autofac;
using Core.Domains.Games.Entities;
using Core.Ecosystem.Entities;
using Core.Interfaces;
using Core.Interfaces.Data;
using Core.Services;
using Core.Utilities;
using System.Net.Http.Json;

namespace Core.Domains.Games
{
    /// <summary>
    /// Provides basic contract to add a game to the network
    /// </summary>
    public abstract class BaseGameProvider : BaseService, INamed
    {

        public const string ProfileIgn = "Ign";
        public const string ProfileScreenshot = "SS";
        public const string ProfileUserid = "UID";
        public const string ProfileLevel = "LEVEL";

        protected BaseGameProvider(ILifetimeScope scope, ContextNames name = ContextNames.Game) : base(scope, name)
        {
        }

        public abstract string Name { get; }

        public abstract string Description { get; }
        public abstract string GameLogoUrl { get; }
        public abstract bool IsSquadSupported { get; }
        public abstract List<string> GameGenre { get; }
        public abstract string[] Tags { get; set; }
        public abstract Dictionary<string, string> ImageUrls { get; }
        public abstract List<string> ProfileDataRequired { get; }
        public abstract Task<Player> VerifyPlayerProfile(Player player);
        public abstract Task<Dictionary<string, string>> GetGameRelatedPlayerInformation(Player player);


        protected List<float> Cluster(List<float> input, int clusters, bool takeMax = true)
        {
            var output = new List<float>(); var margin = 0;
            do
            {
                output = new();
                margin++;
                foreach (var i in input)
                {
                    if (output.Any(o => o > i - margin && o < i + margin) == false)
                        output.Add(i);
                    else
                    {
                        var cluster = output.FirstOrDefault(o => o > i - margin && o < i + margin);
                        if (takeMax && i > cluster) { output.Remove(cluster); output.Add(i); }
                        else if (takeMax == false && i < cluster) { output.Remove(cluster); output.Add(i); }
                    }
                } 
            } while (output.Count > clusters);

            return output.OrderBy(o => o).ToList();
        }
        protected List<float> ClusterWithinMargin(List<float> input, int margin = 10)
        {
            var output = new List<float>();
            foreach (var i in input)
            {
                if (output.Any(o => o > i - margin && o < i + margin) == false)
                    output.Add(i + margin);
            }


            return output.OrderBy(o => o).ToList();
        }

        protected int ParseInt(string text)
        {
            if (int.TryParse(text.Split(" ").FirstOrDefault(), out int pos))
                return pos;
            return 0;
        }

        protected string SanitizeNumberParsingErrors(string text)
        {
            if (text.Contains(":unselected:"))
                ;
            text = text.Replace("\n:unselected:", "");
            switch (text.ToUpper())
            {
                case "O":
                    return "0";
                case "L":
                    return "1";
                case "I":
                    return "0";
                case "U":
                    return "0";
                case "DO":
                    return "8";
                case "Z":
                    return "2";
                case "D":
                    return "0";
                case "|":
                    return "1";
            }

            return text;
        }


        
    }

    public class NotRequiredException : Exception
    {

    }

}
