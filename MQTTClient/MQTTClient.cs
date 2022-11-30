using Microsoft.VisualBasic.FileIO;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MQTTClient
{
    public class MQTTConnectModel
    {
        public string ClientId;
        public string Url;
        public int? Port;
        public string Username;
        public string Password;

        public bool CleanSession = true;

        /// <summary>
        /// //心跳间隔
        /// </summary>
        public TimeSpan KeepAlivePeriod = TimeSpan.FromSeconds(10);

        /// <summary>
        /// 保活发送间隔
        /// </summary>
        public TimeSpan KeepAliveSendInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 通信超时
        /// </summary>
        public TimeSpan CommunicationTimeout = TimeSpan.FromSeconds(30);
    }

    internal class MQTTClient
    {
        /// <summary>
        /// 客户端对象
        /// </summary>
        public IMqttClient _client;

        MainWindow mainWindow;
        MqttClientOptions _option;

        public MQTTClient(MQTTConnectModel model)
        {
            mainWindow = Application.Current.MainWindow as MainWindow;

            var options = new MqttClientOptionsBuilder()
                        .WithTcpServer(model.Url, Convert.ToInt32(model.Port))
                        .WithCredentials(model.Username, model.Password)
                        .WithClientId(model.ClientId)
                        .WithCleanSession(model.CleanSession)
                        .WithKeepAlivePeriod(model.KeepAlivePeriod)
                        .WithTimeout(model.CommunicationTimeout)
                        .Build();
            _option = options;
            _client = new MQTTnet.MqttFactory().CreateMqttClient();
            _client.ConnectedAsync += Client_ConnectedAsync;
            _client.DisconnectedAsync += Client_DisConnectedAsync;
            _client.ApplicationMessageReceivedAsync += _client_ApplicationMessageReceivedAsync;
            
        }

        private Task _client_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
        {
            Task task = new Task(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    string msg =
                "收到来自客户端"
                    + e.ClientId
                    + "，主题为"
                    + e.ApplicationMessage.Topic
                    + "的消息："
                    + Encoding.UTF8.GetString(e.ApplicationMessage.Payload)
                    + " ,QoS:"
                    + e.ApplicationMessage.QualityOfServiceLevel
                    + " ,Retain:"
                    + e.ApplicationMessage.Retain
                    + "\r\n";

                    mainWindow.RecieveTextBox.Text = msg + mainWindow.RecieveTextBox.Text;
                }));
            });
            task.Start();
            return task;
        }

        private Task Client_DisConnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            Task task = new Task(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    string msg = $"客户端{_client.Options.ClientId} 断开连接 \r\n";
                    mainWindow.RecieveTextBox.Text = msg + mainWindow.RecieveTextBox.Text;
                    mainWindow.ConnectState.Content = "连接";
                }));
            });
            task.Start();
            return task;
        }

        private Task Client_ConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            Task task = new Task(() =>
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    string msg = $"客户端{_client.Options.ClientId} 连接成功 \r\n";
                    mainWindow.RecieveTextBox.Text = msg + mainWindow.RecieveTextBox.Text;
                    mainWindow.ConnectState.Content = "断开";
                }));
            });
            task.Start();
            return task;
        }

        public void PublicMsg(string topicName, string msg)
        {
            _client.PublishStringAsync(topicName, msg);
        }

        public void Subscribe(string topicName)
        {
            var res =_client.SubscribeAsync(topicName);
        }

        public void UnSubscribe(string topicName)
        {
            var res = _client.UnsubscribeAsync(topicName);
        }

        public bool IsConnected()
        {
            return _client.IsConnected;
        }

        public void Connect()
        {
            _client.ConnectAsync(_option, CancellationToken.None);
        }

        public void DisConnect()
        {
            MqttClientDisconnectOptions mqttClientDisconnectOptions = new MqttClientDisconnectOptions();
            _client.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
        }
    }
}
