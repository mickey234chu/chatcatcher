using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Newtonsoft.Json;
using TwitchLib.Communication.Interfaces;

namespace chatcatcher
{
    public class ChatConnectionTool
    {
        public ConnectionParameters ImportParametersFromJson(string jsonFilePath)
        {
            string jsonContent = File.ReadAllText(jsonFilePath);
            ConnectionParameters parameters = JsonConvert.DeserializeObject<ConnectionParameters>(jsonContent);
            return parameters;
        }

    }
    public class TwitchTool
    {
        public void SendToTwitch(StreamWriter writer, string user, string chatname)
        {

            writer.WriteLine(string.Format(":{0}!{0}@{0}.tmi.twitch.tv PRIVMSG #{1} :{2}", user, chatname, "中文"));
            writer.Flush();

        }
    }
    public class DiscordTool
    {
        private DiscordSocketClient _client;

        public DiscordTool()
        {
            _client = new DiscordSocketClient();
            _client.Log += LogMessage;
            _client.Ready += ReadyEvent;
            _client.MessageReceived += MessageReceivedEvent;
        }
        // 编写事件处理程序的方法
        public Task LogMessage(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        public Task ReadyEvent()
        {
            Console.WriteLine("Bot已連接到 Discord");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedEvent(SocketMessage message)
        {
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Content == "!hello")
                await message.Channel.SendMessageAsync("Hello!");
        }

        public async Task StartBot(string token)
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await Task.Delay(-1);
        }
    }



    public class ConnectionParameters
    {
        public string Username { get; set; }
        public string Secret { get; set; }
        public string Chatroom { get; set; }
        public string DiscordSecret { get; set; }
    }
}
