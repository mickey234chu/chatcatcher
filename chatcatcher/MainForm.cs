using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using TwitchLib.Api;
using TwitchLib.Api.Core;
using TwitchLib.Api.Core.Enums;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

using static System.Formats.Asn1.AsnWriter;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using TwitchLib.PubSub.Models.Responses.Messages.AutomodCaughtMessage;
using System.Reflection.Metadata;
using TwitchLib.PubSub.Models.Responses;
using Discord;
using Discord.WebSocket;

namespace chatcatcher
{
    public partial class MainForm : Form
    {
        private const string Host = "irc.chat.twitch.tv";
        private const int Port = 6667;

        private TcpClient client;
        private StreamReader reader;
        public StreamWriter writer;
        public TwitchTool twitchTool;
        private DiscordTool discordTool;
        private Task chatTask;
        private Boolean isConnected = false;
        private String user;
        private String secret;
        private String discordserver;
        private String discordchannel;
        private String discordsecret;
        private String oathtoken;
        private String chatname;
        private String path;
        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {


            // 設置 TableLayoutPanel 控件的行和列樣式
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));


            // 將多行文本框控件添加到 TableLayoutPanel 控件中
            tableLayoutPanel1.Controls.Add(btnOpenFile, 0, 0);
            tableLayoutPanel1.Controls.Add(txtChat, 1, 0);
            tableLayoutPanel1.Controls.Add(btn, 2, 0);
            btnOpenFile.Dock = DockStyle.Fill;
            txtChat.Dock = DockStyle.Fill;
            btn.Dock = DockStyle.Fill;
        }

        private async void btn_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                if (user == null || chatname == null)
                {
                    AppendText("請確定已載入有效的ID和密碼，按下「選擇連接參數而載入ID和密碼」" + Environment.NewLine);

                }
                else
                {

                    try
                    {
                        //Twitch 聊天室連線
                        AppendText("嘗試進行連接" + Environment.NewLine);
                        //取得Token
                        oathtoken = await Oath2Authorize();
                        if (oathtoken == null)
                        {
                            AppendText("oathtoken 取得失敗" + Environment.NewLine);
                        }
                        else
                        {
                            
                              AppendText("token:" + oathtoken + Environment.NewLine);
                            
                        }
                        //實例化使用工具
                        twitchTool = new TwitchTool();
                        // 開啟聊天室抓取任務
                        // 初始化Twitch聊天室連接
                        AppendText("初始化聊天室連接" + Environment.NewLine);
                        Connect(user, secret, oathtoken, chatname);
                        chatTask = Task.Run(ReadChatMessages);
                        //完成Twitch聊天室的連線
                        /////////////////////////////////////////////////

                        // 創建 DiscordSocketClient 
                        discordTool = new DiscordTool(this, discordserver, discordchannel, user, chatname); 
                        //連接Discord Bot
                        isConnected = await discordTool.StartBot(discordsecret);
                        //根據結果進行Window上的修改
                        if (isConnected)
                        {
                            btn.Text = "結束連結";
                        }
                        else
                        {
                            CloseTwitch();
                        }

                        
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                
            }
            else
            {
                CloseTwitch();
                discordTool.StopBot();
                isConnected = false;
                btn.Text = "開始連結";
            }


        }
        private void CloseTwitch()
        {
            AppendText("停止連接" + Environment.NewLine);

            // 停止聊天室抓取任務
            chatTask = null;

            // 關閉聊天室連接
            writer.WriteLine("QUIT");
            writer.Flush();
            client.Close();
        }
        private async Task<string> Oath2Authorize()
        {

            string clientId = user;
            string redirectUri = "http://localhost:8080";
            // 以空格分隔的Twitch權限列表
            string scope = "channel_read channel:read:subscriptions chat:read chat:edit";
            string authorizationUrl = $"https://id.twitch.tv/oauth2/authorize?client_id={clientId}&redirect_uri={redirectUri}&response_type=code&scope={scope}";
            AppendText("authorizationUrl: " + authorizationUrl+ Environment.NewLine);
            // 創建建本地 HTTP server
            var listener = new HttpListener();
            listener.Prefixes.Add(redirectUri + "/");
            listener.Start();
            //打開瀏覽器進行授權
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = authorizationUrl,
                UseShellExecute = true
            });

            // 接收来自 Twitch 的回調請求
            var context = await listener.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            // 提取授權碼
            string authorizationCode = HttpUtility.ParseQueryString(request.Url.Query)["code"];

            // 關閉本地 HTTP server
            listener.Stop();
            listener.Close();
            
            if (authorizationCode != null)
            {
                AppendText("Oath2.0授權結果: 成功" + Environment.NewLine);
                AppendText("已完成Oath2.0授權，開始進行存取權杖" + Environment.NewLine);
                string token = await GetTwitchAccessToken(authorizationCode);
                return token;
            }
            else
            {
                AppendText("Oath2.0授權發生錯誤: " + Environment.NewLine);
            }
            
            return null;
            
        }
        private async Task<string> GetTwitchAccessToken(string OathCode)
        {
            string clientId = user;
            string clientSecret = secret;
            string redirectUri = "http://localhost:8080";
            string authorizationCode = OathCode;
            var httpClient = new HttpClient();

            var requestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            });

            var response = await httpClient.PostAsync("https://id.twitch.tv/oauth2/token", requestBody);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // 解析存取權杖
                AppendText("存取權杖結果: 成功" + Environment.NewLine);
                String token = JObject.Parse(responseContent)["access_token"]?.ToString();
                return token;
            }
            else
            {
                AppendText("存取權杖發生錯誤: " + responseContent);
                return null;
            }
        }
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;

                // 將選定的文件路徑傳遞給方法或保存到適當的參數中
                // ...
                path = selectedFilePath;

                // 在此處可以執行其他操作，如更新 UI 或執行其他邏輯
                // ...
                // 創建 TwitchConnectionTool 實例
                ChatConnectionTool connectionTool = new ChatConnectionTool();

                // 引入 Twitch 連接參數
                ConnectionParameters parameters = connectionTool.ImportParametersFromJson(path);
                ConnectVarSetting(parameters);
                AppendText("目前套用之參數文件路徑:" + path + Environment.NewLine);
                AppendText("目前套用內容:" + Environment.NewLine);
                AppendText("你想連接的聊天室為:" + parameters.Chatroom + Environment.NewLine);
            }
        }
        private void ConnectVarSetting(ConnectionParameters parameters)
        {
            // 更新當前要串的資料
            user = parameters.Username;
            secret = parameters.Secret;
            chatname = parameters.Chatroom;
            discordserver = parameters.DiscordServerID;
            discordchannel = parameters.DiscordChannelID;
            discordsecret = parameters.DiscordSecret;
 

        }
        //無需要secret
        public void Connect(string username,string secret, string accessToken, string channel)
        {
            client = new TcpClient(Host, Port);
            //reader = new StreamReader(client.GetStream(), Encoding.GetEncoding("iso-8859-1"));
            //writer = new StreamWriter(client.GetStream(), Encoding.GetEncoding("iso-8859-1"));
            reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            writer = new StreamWriter(client.GetStream(), Encoding.UTF8);
            // 發送身份驗證信息
            writer.WriteLine();
            writer.WriteLine("PASS oauth:" + accessToken);
            writer.WriteLine("NICK " + username.ToLower());
            writer.WriteLine("JOIN #" + channel.ToLower());
            writer.Flush();
           
        }
        public string UTF8TOISO (string message) 
        {
            Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = utf8.GetBytes(message);
            byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            string msg = iso.GetString(isoBytes);
            return msg;
        }
        private async Task ReadChatMessages()
        {
            while (chatTask != null)
            {
                try
                {
                    string message = await reader.ReadLineAsync();
                    // 分解Message
                    if (message != null)
                    {
                        //一般打印
                        if (message.Contains("PRIVMSG"))
                        {
                            ProcessChatMessage(message);
                            
                        }
                        else
                        {
                            AppendText(message + Environment.NewLine);
                        }

                    }
                    
                }
                catch (IOException e)
                {
                    //關掉就一定會報錯，無視就好
                    //AppendText(e.Message + Environment.NewLine);
                    break;
                }
            }
        }
        private void ProcessChatMessage(string message)
        {
            // 分解message
            string[] parts = message.Split(' ');
            if (parts.Length >= 4 && parts[1] == "PRIVMSG")
            {
                string username = parts[0].Substring(1, parts[0].IndexOf("!") - 1);
                string content = string.Join(" ", parts.Skip(3).ToArray()).Substring(1);
                // 在文本框中顯示聊天室内容
                AppendText(username + ":" + content + Environment.NewLine);
                // 將處理後的訊息傳遞給 Discord Bot
                if (discordTool._isConnected)
                {
                    discordTool.SendMessageToDiscord(username, content);
                }
                
            }
            else
            {
                AppendText(message + Environment.NewLine);
            }
        }
        public void AppendText(string text)
        {
            if (txtChat.InvokeRequired)
            {
                txtChat.Invoke(new Action<string>(AppendText), text);
            }
            else
            {
                txtChat.AppendText(text);
            }
        }


    }
}