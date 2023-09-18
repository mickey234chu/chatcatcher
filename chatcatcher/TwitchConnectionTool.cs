using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace chatcatcher
{
    public class TwitchConnectionTool
    {
        public TwitchConnectionParameters ImportParametersFromJson(string jsonFilePath)
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            TwitchConnectionParameters parameters = JsonConvert.DeserializeObject<TwitchConnectionParameters>(jsonContent);
            return parameters;
        }
    }
    public class TwitchConnectionParameters
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public string Chatroom { get; set; }
    }
}
