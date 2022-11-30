using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Diagnostics;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using static MQTTServer.ServerVM;

namespace MQTTServer
{
    internal class MqttServer
    {
        private MQTTnet.Server.MqttServer _server;
        EventCallBack _eventCallBack;

        public MqttServer(EventCallBack eventCallBack)
        {
            var mqttFactory = new MqttFactory();
            var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder()
                .WithDefaultEndpoint()
                .WithDefaultEndpointBoundIPAddress(IPAddress.Parse("127.0.0.1"))
                .WithDefaultEndpointPort(12345)
                .Build();
            _server = mqttFactory.CreateMqttServer(mqttServerOptions);

            _server.StartedAsync += _server_StartedAsync;
            _server.StoppedAsync += _server_StoppedAsync;
            _server.ClientAcknowledgedPublishPacketAsync += _server_ClientAcknowledgedPublishPacketAsync;
            _server.ClientConnectedAsync += _server_ClientConnectedAsync;
            _server.ClientDisconnectedAsync += _server_ClientDisconnectedAsync;
            _server.InterceptingPublishAsync += _server_InterceptingPublishAsync;
            _server.ClientSubscribedTopicAsync += _server_ClientSubscribedTopicAsync;
            _server.InterceptingSubscriptionAsync += _server_InterceptingSubscriptionAsync;
            _eventCallBack = eventCallBack;
        }



        private Task _server_StartedAsync(EventArgs arg)
        {
            _eventCallBack("已启动");
            return null;
        }

        private Task _server_StoppedAsync(EventArgs arg)
        {
            _eventCallBack("已断开");
            return null;
        }

        private Task _server_InterceptingSubscriptionAsync(InterceptingSubscriptionEventArgs arg)
        {
            string id = arg.ClientId;
            string topic = arg.TopicFilter.Topic;

            string msg = $"客户端取消：{id} 订阅了:{topic}";
            _eventCallBack(msg);
            return null;
        }

        private Task _server_ClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs arg)
        {
            string id = arg.ClientId;
            string topic = arg.TopicFilter.Topic;
            string msg = $"客户端：{id} 订阅了:{topic}";
            _eventCallBack(msg);
            return null;
        }

        private Task _server_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
        {
            string id = arg.ClientId;
            string topic = arg.ApplicationMessage.Topic;
            string payLoad = arg.ApplicationMessage.Payload.ToString();
            string msg = $"客户端：{id} 主题为:{topic}发布了:{payLoad}";
            _eventCallBack(msg);
            return null;
        }

        private Task _server_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
        {
            string id = arg.ClientId;
            string msg = $"客户端：{id} 断开连接";
            _eventCallBack(msg);
            return null;
        }

        private Task _server_ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            string id = arg.ClientId;
            string msg = $"客户端：{id} 连接成功";
            return null;
        }

        private Task _server_ClientAcknowledgedPublishPacketAsync(ClientAcknowledgedPublishPacketEventArgs arg)
        {
            string id = arg.ClientId;
            return null;
        }


        public async Task<MQTTnet.Server.MqttServer> StartServer()
        {
            //await _server.StartAsync();
            _server.StartAsync();
            return _server;
        }

        public async Task<MQTTnet.Server.MqttServer> StopServer()
        {
            await _server.StopAsync();
            return _server;
        }

        public async Task StopClientByClientId(string id)
        {
            var affectedClient = (await _server.GetClientsAsync()).FirstOrDefault(c => c.Id == id);
            if (affectedClient != null)
            {
                await affectedClient.DisconnectAsync();
            }
        }

        public async Task PublicMsg(string topic, string msg)
        {
            var message = new MqttApplicationMessageBuilder().WithTopic(topic).WithPayload(msg).Build();
            await _server.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message)
                    {
                       // SenderClientId = "SenderClientId"
                    });
        }

        
        public bool IsStared()
        {
            return _server.IsStarted;
        }

    }
}
