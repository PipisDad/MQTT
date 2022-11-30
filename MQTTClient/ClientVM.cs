using Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MQTTClient
{
    public class ClientVM : ViewModelBase
    {
        MQTTClient MQTTClient;

        #region 命令定义
        public CommandBase AddTopicCmd { get; set; }
        public CommandBase RemoveTopicCmd { get; set; }
        public CommandBase SendMsgCmd { get; set; }
        public CommandBase ConnectCmd { get; set; }

        #endregion

        #region 属性

        private ObservableCollection<string> _topicList = new ObservableCollection<string>();
        public ObservableCollection<string> TopicList
        {
            get { return _topicList; }
            set
            {
                _topicList = value;
                OnPropertyChanged(() => TopicList);
            }
        }
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; OnPropertyChanged(() => SelectedIndex); }
        }

        
        private string _connetStr = "连接";
        public string ConnetStr
        {
            get { return _connetStr; }
            set { _connetStr = value; OnPropertyChanged(() => ConnetStr); }
        }

        private string _newTopic = "";
        public string NewTopic
        {
            get { return _newTopic; }
            set { _newTopic = value; OnPropertyChanged(() => NewTopic); }
        }
        

        private string _sendTopic = "";
        public string SendTopic
        {
            get { return _sendTopic; }
            set { _sendTopic = value; OnPropertyChanged(() => SendTopic); }
        }

        private string _sendText = "";
        public string SendText
        {
            get { return _sendText; }
            set { _sendText = value; OnPropertyChanged(() => SendText); }
        }
        private string _revieceText = "";
        public string RevieceText
        {
            get { return _revieceText; }
            set { _revieceText = value; OnPropertyChanged(() => RevieceText); }
        }

        #endregion
        public ClientVM()
        {
            AddTopicCmd = new CommandBase(AddTopicExcute);
            RemoveTopicCmd = new CommandBase(RemoveTopicExcute);
            SendMsgCmd = new CommandBase(SendMsgExcute);
            ConnectCmd = new CommandBase(ConnectExcute);

            MQTTConnectModel mQTTConnectModel = new MQTTConnectModel();
            mQTTConnectModel.ClientId = Guid.NewGuid().ToString();
            mQTTConnectModel.Url = "127.0.0.1";
            mQTTConnectModel.Port = 12345;
            MQTTClient = new MQTTClient(mQTTConnectModel);
        }

        #region 命令方法
        private void AddTopicExcute(object obj)
        {
            //TopicList
            if (!string.IsNullOrEmpty(NewTopic))
            {
                if(TopicList.Count(t=>t == NewTopic) == 0)
                {
                    TopicList.Add(NewTopic);
                    MQTTClient.Subscribe(NewTopic);
                    return;
                }
            }
            MessageBox.Show("添加失败");
        }

        private void RemoveTopicExcute(object obj)
        {
            if(SelectedIndex<0)
            {
                MessageBox.Show("未选中对象");
                return;
            }
            TopicList.RemoveAt(SelectedIndex);
            MQTTClient.UnSubscribe(NewTopic);
        }

        private void SendMsgExcute(object obj)
        {
            MQTTClient.PublicMsg(SendTopic, SendText);
        }

        private void ConnectExcute(object obj)
        {
            if(!MQTTClient.IsConnected())
            {
                MQTTClient.Connect();
                ConnetStr = "断开";
            }
            else
            {
                MQTTClient.DisConnect();
                ConnetStr = "连接";
            }
        }
        #endregion
    }
}
