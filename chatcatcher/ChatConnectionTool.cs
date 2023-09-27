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
using System.Diagnostics;

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
    // TODO:把它改成輸出別人的訊息而不是只輸出"中文"
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
        private MainForm _mainForm;
        private string _serverID;
        private string _chatID;
        public bool _isConnected;
        public DiscordTool(MainForm mainForm,string serverID, string channelID)
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                // 其他配置项...
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            }) ;
            _mainForm = mainForm;
            _serverID = serverID;
            _chatID = channelID;
            _client.Log += LogMessage;
            _client.Ready += ReadyEvent;
            _client.MessageReceived += MessageReceivedEvent;
            
            _isConnected = false;
        }
        // 编写事件处理程序的方法
        public Task LogMessage(LogMessage message)
        {
            _mainForm.AppendText("LOG: " + message.ToString() + Environment.NewLine);
            return Task.CompletedTask;
        }

        public Task ReadyEvent()
        {
            _mainForm.AppendText("Bot已連接到 Discord" + Environment.NewLine);
            Trace.WriteLine("Bot已連接到 Discord");
            // 伺服ID
            var guild = _client.GetGuild(ulong.Parse(_serverID)); // 更換為您的伺服ID

            // 取得聊天室（频道）对象
            var channel = guild.GetTextChannel(ulong.Parse(_chatID)); // 更換為您的聊天室ID

            if (channel != null)
            {
                channel.SendMessageAsync("Hello, channel!"); // 更換為您要發送的消息内容
            }
            _isConnected = true;
            return Task.CompletedTask;
        }

        private async Task MessageReceivedEvent(SocketMessage message)
        {
            _mainForm.AppendText("Discord 訊息: " + message.Content.ToString() + Environment.NewLine);
            Trace.WriteLine("Received Discord Message:" + message.ToString());
            if (message.Content == null|| message.Content.Length < 1)
            {
               
                Trace.WriteLine("ERROR!NO Message, ID :" + _client.Guilds.ToString());
            }
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            if (message.Content == "!hello")
                await message.Channel.SendMessageAsync("Hello!");
        }

        public async Task<bool> StartBot(string token)
        {
            try
            {
               
                await _client.LoginAsync(TokenType.Bot, token);
                await _client.StartAsync();
                //_client.GetChannel(ulong.Parse("1004975659409485917"));
                _isConnected = true;
            }
            catch (Exception ex) 
            {
                _mainForm.AppendText("連接失敗:" + Environment.NewLine);
                _isConnected = false;
            }
            
            return _isConnected;
        }
        public async Task StopBot()
        {
            try
            {
                await _client.StopAsync();
                await _client.LogoutAsync();
                _isConnected = false;
            }
            catch (Exception ex)
            {
                _mainForm.AppendText("中斷 Discord 連接时發生錯誤：" + ex.Message);
            }
        }
        public async Task SendMessageToDiscord(string username, string content)
        {
            // 伺服ID
            var guild = _client.GetGuild(ulong.Parse(_serverID));

            // 取得聊天室（频道）对象
            var channel = guild.GetTextChannel(ulong.Parse(_chatID));

            if (channel != null)
            {
                string message = $"Twitch用戶{username}:{content}";
                await channel.SendMessageAsync(message);
            }
        }

    }

    

    public class ConnectionParameters
    {
        public string Username { get; set; }
        public string Secret { get; set; }
        public string Chatroom { get; set; }

        public string DiscordServerID { get; set; }
        public string DiscordChannelID { get; set; }
        public string DiscordSecret { get; set; }
    }
}
