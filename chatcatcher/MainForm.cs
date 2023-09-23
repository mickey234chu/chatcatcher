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

namespace chatcatcher
{
    public partial class MainForm : Form
    {
        private const string Host = "irc.chat.twitch.tv";
        private const int Port = 6667;

        private TcpClient client;
        private StreamReader reader;
        private StreamWriter writer;
        private Task chatTask;
        private Boolean isConnected = false;
        private String user;
        private String secret;
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
                if (user.Length < 1 || chatname.Length < 1)
                {
                    AppendText("請確定已載入有效的ID和密碼，按下「選擇連接參數而載入ID和密碼」" + Environment.NewLine);

                }
                else
                {

                    try
                    {
                        AppendText("嘗試進行連接" + Environment.NewLine);
                        //取得Token
                        oathtoken = await GetTwitchAccessToken();
                        if (oathtoken == null)
                        {
                            AppendText("oathtoken 取得失敗" + Environment.NewLine);
                        }
                        else
                        {
                            
                              AppendText("oathtoken:" + oathtoken + Environment.NewLine);
                            
                        }
                        
                        // 開啟聊天室抓取任務
                        // 初始化聊天室连接
                        /*
                        Connect(user, secret, oathtoken, chatname);
                        chatTask = Task.Run(ReadChatMessages);
                        isConnected = true;
                        btn.Text = "結束連結";*/
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
                
            }
            else
            {
                AppendText("停止連接" + Environment.NewLine);

                // 停止聊天室抓取任務
                chatTask = null;

                // 關閉聊天室連接
                writer.WriteLine("QUIT");
                writer.Flush();
                client.Close();
                isConnected = false;
                btn.Text = "開始連結";
            }


        }
        private async Task<string> GetTwitchAccessToken()
        {
            string clientId = user;
            string clientSecret = secret;
            string redirectUri = "http://localhost";
            string authorizationCode = "";
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
                String token = JObject.Parse(responseContent)["access_token"]?.ToString();
                return token;
            }
            else
            {
                AppendText("發生錯誤: " + responseContent);
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
                TwitchConnectionTool connectionTool = new TwitchConnectionTool();

                // 引入 Twitch 連接參數
                TwitchConnectionParameters parameters = connectionTool.ImportParametersFromJson(path);
                ConnectToTwitch(parameters);
                AppendText("目前套用之參數文件路徑:" + path + Environment.NewLine);
                AppendText("目前套用內容:" + Environment.NewLine);
                AppendText("你想連接的聊天室為:" + parameters.Chatroom + Environment.NewLine);
            }
        }
        private void ConnectToTwitch(TwitchConnectionParameters parameters)
        {
            // 更新當前要串的資料
            user = parameters.Username;
            secret = parameters.Secret;
            chatname = parameters.Chatroom;


        }
        public void Connect(string username,string secret, string accessToken, string channel)
        {
            client = new TcpClient(Host, Port);
            reader = new StreamReader(client.GetStream(), Encoding.GetEncoding("iso-8859-1"));
            writer = new StreamWriter(client.GetStream(), Encoding.GetEncoding("iso-8859-1"));

            // 發送身份驗證信息
            writer.WriteLine("PASS " + secret);
            writer.WriteLine("NICK " + username.ToLower());
            writer.WriteLine("JOIN #" + channel.ToLower());
            writer.Flush();
        }
        
        private async Task ReadChatMessages()
        {
            while (chatTask != null)
            {
                try
                {
                    string message = await reader.ReadLineAsync();

                    // 在文本框中顯示聊天室内容
                    AppendText(message + Environment.NewLine);
                }
                catch (IOException)
                {
                    // 處理錯誤或失敗
                    break;
                }
            }
        }

        private void AppendText(string text)
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