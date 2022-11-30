using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MQTTServer
{
    public class TopicInfo
    {
        public string ClientId { get; set; }
        public string TopicName { get; set; }
        public string SubscribeTime { get; set; }
    }

    internal class ServerVM : ViewModelBase
    {
        private MqttServer MQTTServer;
        public delegate void EventCallBack(string msg);
        private EventCallBack _eventCallBack;

        public CommandBase StrartCmd { get; set; }

        private ObservableCollection<TopicInfo> _topicList = new ObservableCollection<TopicInfo>();
        public ObservableCollection<TopicInfo> TopicList
        {
            get { return _topicList; }
            set
            {
                _topicList = value;
                OnPropertyChanged(() => TopicList);
            }
        }

        private string _startStr = "启动";
        public string StartStr
        {
            get { return _startStr; }
            set
            {
                _startStr = value;
                OnPropertyChanged(() => StartStr);
            }
        }

        private string msg = "";
        public string Msg
        {
            get { return msg; }
            set
            {
                msg = value;
                OnPropertyChanged(() => Msg);
            }
        }

        public ServerVM()
        {
            _eventCallBack = new EventCallBack(CallBackMsg);
            MQTTServer = new MqttServer(_eventCallBack);
            StrartCmd = new CommandBase(StrartExcute);
            
        }

        private void StrartExcute(object obj)
        {
            if (MQTTServer.IsStared())
            {
                MQTTServer.StopServer();
                StartStr = "启动";
            }
            else
            {
                MQTTServer.StartServer();
                StartStr = "断开";
            }
        }

        private void CallBackMsg(string msg)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Msg = msg + "\r\n" + Msg;
            }
                )); 
        }
    }
}
