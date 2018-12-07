using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Threading;

namespace A10_AutoTestTool
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        /*****************************************变量声明*******************************************/
        enum Command
        {
            CMD_BATTERY_TEST = 0x11,
            CMD_LOCK_TEST = 0x12,
            CMD_TEMP_TEST = 0x13,
            CMD_POWER_DOWN_TEST = 0x14,
            CMD_SET_PCB = 0x15,
            CMD_GET_FW = 0x16,
            TestMode = 0x99//测试请求或结束
        }; 

        enum TEST_MODE
        {
            TEST_MODE_START = 0x00,
            TEST_MODE_STOP
        };

        struct GetResult
        {
            public int testMode;
            public int testModeAllow;
            public int key;
            public int[] keyValue;
            public int tapCard;
            public string cardNum;
            public int lcd;
            public int _2G;
            public int _2gCSQ;
            public string _2G_Iccid;
            public int trumpet;
            public int relay;
            public int measurementChip;
            public int[] getPower;
            public int SetCID;
            public int SetPcbCode;
            public int SetRegisterCode;
            public string MainBoardCode;
            public string InterfaceBoardCode;
            public int BLE;
            public string FwVersion;
            public UInt32 UsedTime_interface;
            public UInt32 UsedTime_main;
            public UInt32 UsedTime_Charger;
        };

        struct CountDownTime
        {
            public int testMode;
            public int PowerSource;
            public int SetCID;
            public int SetPcbCode;
            public int BLE;
            public int Temp;
            public int Lock;
            public int PowerDown;
            public int Battery;

        };

        enum MB_TEST_ITEM
        {
            MB_TEST_ITEM_PCBA = 0x00,
            MB_TEST_ITEM_POWER = 0x01,
            MB_TEST_ITEM_BLUETOOTH = 0x02,
            MB_TEST_ITEM_TEMP = 0x03,
            MB_TEST_ITEM_LOCK = 0x04,
            MB_TEST_ITEM_POWER_DOWN = 0x05,
            MB_TEST_ITEM_BATTERY = 0x06,
            MB_TEST_ITEM_STOP_TEST = 0x07
        };


        Dictionary<string, object> TestSettingInfo = new Dictionary<string, object>
        {
            {"ChargerModel","A10" },
            {"CountDown",30 },
            {"TempLowerLimit",20 },
            {"TempUpperLimit",60 },

        };

        GetResult GetResultObj = new GetResult
        {
            testMode = -1,
            testModeAllow = -1,
            key = -1,
            keyValue = new int[12],
            tapCard = -1,
            lcd = -1,
            _2G = -1,
            _2gCSQ = -1,
            _2G_Iccid = "",
            trumpet = -1,
            relay = -1,
            SetCID = -1,
            measurementChip = -1,
            SetPcbCode = -1,
            BLE = -1,
            SetRegisterCode = -1,
            cardNum = "",
            getPower = new int[12],
            FwVersion = "",
            UsedTime_interface = 0,
            UsedTime_main = 0,
            UsedTime_Charger = 0,
            MainBoardCode = "",
            InterfaceBoardCode = ""
        };

        CountDownTime countDownTime_MB = new CountDownTime
        {
            testMode = 0,
            PowerSource = 0,
            SetCID = 0,
            SetPcbCode = 0,
            BLE = 0,
            Temp = 0,
            Battery = 0,
            PowerDown = 0,
            Lock = 0,
        };

        public static string reportPath = @".\智能报表";
        public static List<byte> arraybuffer = new List<byte> { };

        bool MsgDebug = true;
        static byte sequence = 0;
        public static bool MBTestingFlag = false;
        Thread MBTestThread;

        static int MBTabSelectIndex;
        static int PreMBTabSelectIndex = 0;
        static int TestMeunSelectIndex;
        static int PCBATestSelectIndex;
        public UInt32 ItemTestTime = 0;

        Dictionary<string, string> MBTestResultDir = new Dictionary<string, string>();
        Dictionary<string, string> SBTestResultDir = new Dictionary<string, string>();
        Dictionary<string, string> ChargerTestResultDir = new Dictionary<string, string>();
        /*******************************************************************************************/
        private void MainForm_Load(object sender, EventArgs e)
        {
            skinTabControl_Menu.SelectTab(skinTabPage_Config);
            skinTabPage_Config.Text = "用户:" + Server.PresentAccount;
            if (Server.PresentAccount == "Admin")
            {
                skinButton_AccountSetting.Visible = true;
            }
            else
            {
                skinButton_AccountSetting.Visible = false;
            }

            TestSettingInfo = Server.ReadConfig(Server.testConfigFile, TestSettingInfo);

            comboBox_ChargerModel.SelectedItem = TestSettingInfo["ChargerModel"];
            comboBox_ChargerModel.SelectedIndex = 0;
            numericUpDownTestWaittime.Value = Convert.ToDecimal(TestSettingInfo["CountDown"]);
            numericUpDown_TempLowerLimit.Value = Convert.ToDecimal(TestSettingInfo["TempLowerLimit"]);
            numericUpDown_TempUpperLimit.Value = Convert.ToDecimal(TestSettingInfo["TempUpperLimit"]);

            timer1.Enabled = true;
            timer1.Start();

            try
            {
                if (Directory.Exists(reportPath) == false)
                {
                    Directory.CreateDirectory(reportPath);
                }

                //添加串口项目  
                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {//获取有多少个COM口  
                    skinComboBox_SerialPortNum.Items.Add(s);
                }
                if (skinComboBox_SerialPortNum.Items.Count > 0)
                {
                    skinComboBox_SerialPortNum.SelectedIndex = 0;
                    skinComboBox_BandRate.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Server.DealBackUpData(Server.backupMysqlCmdFile);
        }

        //主窗口退出时销毁线程
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (MBTestThread != null)
                {
                    if (MBTestThread.IsAlive)
                    {
                        MBTestThread.Abort();
                    }
                }

                this.Dispose();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Application.Exit();
            }
        }

        //日志打印
        public void LOG(String text)
        {
            try
            {
                this.textBoxDebug.Invoke(
                    new MethodInvoker(delegate
                    {
                        this.textBoxDebug.AppendText(text + "\r\n");
                    }
                 )
                );
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        
        //清除日志
        private void skinButton_PCBA_CLEAR_LOG_Click(object sender, EventArgs e)
        {
            textBoxDebug.Text = "";
        }
     

        //获取当前tab控件的页
        private TabPage getPresentTabPage(TabControl tabControl)
        {
            TabPage tabPage = null;
            try
            {
                tabControl.Invoke(
                new MethodInvoker(delegate
                {
                    tabPage = tabControl.SelectedTab;
                }));
            }
            catch (Exception ex)
            {

                LOG(ex.Message);
            }
            return tabPage;
        }

        //更新table控件的索引
        private void updateTableSelectedIndex(TabControl tabControl, int index)
        {
            try
            {
                tabControl.Invoke(
                new MethodInvoker(delegate {

                    tabControl.SelectedIndex = index;
                }
              )
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void skinComboBox_SerialPortNum_DropDown(object sender, EventArgs e)
        {
            try
            {
                skinComboBox_SerialPortNum.Items.Clear();

                foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
                {//获取多少串口
                    skinComboBox_SerialPortNum.Items.Add(s);
                }

                if (skinComboBox_SerialPortNum.Items.Count > 0)
                {
                    skinComboBox_SerialPortNum.SelectedIndex = 0;
                    skinComboBox_SerialPortNum.SelectedIndex = 0;
                }
                else
                {
                    skinComboBox_SerialPortNum.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示");
            }
        }

        //波特率变化
        private void skinComboBox_BandRate_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.BaudRate = int.Parse(skinComboBox_BandRate.SelectedItem.ToString());
        }

        //串口控制按钮点击事件监听
        private void skinButton_SerialCtrl_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialPort1.IsOpen)
                {
                    serialPort1.BaudRate = int.Parse(skinComboBox_BandRate.SelectedItem.ToString());
                    serialPort1.PortName = skinComboBox_SerialPortNum.SelectedItem.ToString();
                    serialPort1.Open();
                    if (serialPort1.IsOpen)
                    {
                        skinButton_SerialCtrl.Text = "关闭串口";
                    }
                }
                else
                {
                    serialPort1.Close();

                    if (!serialPort1.IsOpen)
                    {
                        skinButton_SerialCtrl.Text = "打开串口";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        //串口接收
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int n = serialPort1.BytesToRead;
            byte[] buf = new byte[n];
            serialPort1.Read(buf, 0, n);
            arraybuffer.AddRange(buf);
            TestDataHandle(arraybuffer.ToArray());
        }

        //串口发送
        private bool SendSerialData(byte[] data)
        {
            bool ret = false;

            try
            {
                if (serialPort1 != null)
                {
                    if (MsgDebug)
                    {
                        string send = "";
                        for (int j = 0; j < data.Length; j++)
                        {
                            send += data[j].ToString("X2") + " ";
                        }
                        LOG("Send: " + send);
                    }

                    serialPort1.Write(data, 0, data.Length);
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                LOG(ex.Message);
                ret = false;
            }
            return ret;
        }

        //定时器
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MBTestingFlag)//测试倒计时
            {
                countDownTime_MB.PowerSource = ItemCountDown(countDownTime_MB.PowerSource, skinLabel_MB_PowerTimeCountDown, skinTabControl_MB, skinTabPage_MB_Power);
                countDownTime_MB.BLE = ItemCountDown(countDownTime_MB.BLE, skinLabel_MB_BT_TIME, skinTabControl_MB, skinTabPage_MB_BT);
                countDownTime_MB.Temp = ItemCountDown(countDownTime_MB.Temp, skinLabel_MB_TEMP_TIME, skinTabControl_MB, skinTabPage_MB_Temp);
                countDownTime_MB.Lock = ItemCountDown(countDownTime_MB.Lock, skinLabel_MB_Lock_Time, skinTabControl_MB, skinTabPage_MB_Lock);
                countDownTime_MB.PowerDown = ItemCountDown(countDownTime_MB.PowerDown, skinLabel_MB_PowerDown_Time, skinTabControl_MB, skinTabPage_MB_PowerDown);
                countDownTime_MB.Battery = ItemCountDown(countDownTime_MB.Battery, skinLabel_MB_Battery_Time, skinTabControl_MB, skinTabPage_MB_Battery);
            }
        }

        //倒计时显示
        public int ItemCountDown(int time, Label label, TabControl tabControl, TabPage tabPage)
        {
            if (time > 0)
            {
                time--;
                updateControlText(label, time.ToString("D2"));
                if (time == 0)
                {
                    if (tabControl.SelectedTab == tabPage)
                    {
                        tabControl.SelectedIndex++;
                    }

                }
            }
            return time;
        }

        
        public static Dictionary<string, string> ModifyResultData(Dictionary<string, string> inputDic)
        {
            string resKey = "", resValue = "";
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            foreach (var item in inputDic)
            {
                dictionary.Add(item.Key, item.Value);
            }

            foreach (var item in dictionary)
            {
                if (item.Value == "" && item.Key != "测试结果")
                {
                    inputDic[item.Key] = "未测试";
                }
                else if (item.Value == "不通过" && item.Value != "无")
                {
                    inputDic["测试结果"] = "不通过";
                }

                if (inputDic[item.Key] == "未测试")
                {
                    inputDic["测试结果"] = "不通过";
                }
            }
            if (inputDic["测试结果"] == "")
            {
                inputDic["测试结果"] = "通过";
            }

            foreach (var item in inputDic)
            {
                resKey += item.Key + " :\r\n";
                resValue += item.Value + "\r\n";
            }

            return inputDic;
        }

        //发送数据组包
        private static byte[] MakeSendArray(byte cmd, byte[] data)
        {
            UInt16 length;
            List<byte> list = new List<byte> { };
            byte[] srtDes = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            list.Add(0xAA);
            list.Add(0x55);

            list.AddRange(srtDes);
            byte ver = 0x01;
            sequence++;

            if (data != null)
            {
                length = (UInt16)(1 + 1 + 1 + data.Length + 1);
            }
            else
            {
                length = 2;
            }

            list.Add((byte)(length));
            list.Add((byte)(length >> 8));
            list.Add(ver);
            list.Add(sequence);
            list.Add(cmd);
            if (data != null)
            {
                list.AddRange(data);
            }

            list.Add(Server.caculatedCRC(list.ToArray(), list.Count));

            return list.ToArray();
        }

        //接收数据处理
        private void TestDataHandle(byte[] data)
        {
            int length = 0;

            try
            {
                if (data.Length > 17)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] == 0xAA && data[i + 1] == 0x55)
                        {
                            length = (data[i + 12]) + (data[i + 13] << 8) + 14;

                            if (data.Length >= (length+i))
                            {
                                byte checkSum = data[length + i - 1];
                                byte[] validFrame = new byte[length];
                                Array.Copy(data, i, validFrame, 0, validFrame.Length);
                                byte calcCRC = Server.caculatedCRC(validFrame, validFrame.Length - 1);

                                if (MsgDebug)
                                {
                                    string receive = "";
                                    for (int j = 0; j < validFrame.Length; j++)
                                    {
                                        receive += validFrame[j].ToString("X2") + " ";
                                    }
                                    LOG("Receive: " + receive);
                                }

                                if (calcCRC == checkSum)
                                {
                                    arraybuffer.Clear();
                                    byte cmd = validFrame[16];
                                    switch (cmd)
                                    {
                                        case (byte)Command.TestMode://测试模式请求
                                            MessageTestModeHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_TEMP_TEST://温度测试请求
                                            MessageTestTempHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_BATTERY_TEST://电池检测请求
                                            MessageTestBatteryHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_LOCK_TEST://柜锁测试请求
                                            MessageTestLockHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_POWER_DOWN_TEST://掉电测试请求
                                            MessageTestPowerDownHandle(validFrame);
                                            break;

                                        case (byte)Command.CMD_SET_PCB:
                                            GetResultObj.SetPcbCode = validFrame[18];
                                            if (validFrame[17] == 0x00)
                                            {
                                                if (GetResultObj.SetPcbCode == 0x00)
                                                {

                                                    LOG("主板编码设置成功");
                                                }
                                                else
                                                {
                                                    LOG("主板编码设置失败");
                                                }
                                            }
                                            break;

                                        case (byte)Command.CMD_GET_FW:// 
                                            if (validFrame[17] == 0x00)
                                            {
                                                int fwVer = (int)((validFrame[18] << 8) | (validFrame[19]));
                                                int subver = validFrame[20];
                                                GetResultObj.FwVersion = fwVer + "." + subver;
                                            }
                                           
                                            LOG("validFrame[17]:" + validFrame[17].ToString("D2"));
                                            LOG("validFrame[18]:" + validFrame[18].ToString("D2"));
                                            LOG("validFrame[19]:" + validFrame[19].ToString("D2"));
                                            LOG("validFrame[20]:" + validFrame[20].ToString("D2"));
                                            if (MBTestingFlag)
                                            {
                                                MBTestResultDir["软件版本"] = GetResultObj.FwVersion;
                                            }
                                            LOG("软件版本:" + GetResultObj.FwVersion);
                                            break;
                                        
                                        default:
                                            break;
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //设置pcb编号请求消息处理
        private void MessageSetPcbCodeHandle(byte[] pkt)
        {
            GetResultObj.SetPcbCode = pkt[18];
            if (pkt[17] == 0x00)
            {
                if (GetResultObj.SetPcbCode == 0x00)
                {
                    LOG("主板编码设置成功");
                }
                else
                {
                    LOG("主板编码设置失败");
                }
            }
            else if (pkt[17] == 0x01)
            {
                if (GetResultObj.SetPcbCode == 0x00)
                {
                    LOG("副板编码设置成功");
                }
                else
                {
                    LOG("副板编码设置失败");
                }
            }
        }

        //蓝牙测试消息处理
        private void MessageBtTestHandle(byte[] pkt)
        {
            GetResultObj.BLE = pkt[17];
            if (MBTestingFlag)
            {
                switch (GetResultObj.BLE)
                {
                    case 0x01:
                        MBTestResultDir["蓝牙"] = "通过";
                        updateControlText(skinLabel_MB_BT_RESULT, "测试通过", Color.Green);
                        updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        break;
                    case 0x00:
                    case 0x02:
                        if (TestMeunSelectIndex == 0 && PCBATestSelectIndex == 6)
                        {
                            updateControlText(skinLabel_MB_BT_RESULT, "测试不通过", Color.Red);
                            MBTestResultDir["蓝牙"] = "不通过";
                        }
                        break;
                }
            }
        }

        //获取软件版本消息处理
        private void MessageGetFwHandle(byte[] pkt)
        {
            if (pkt[17] == 0x00)
            {
                int fwVer = (int)((pkt[18] << 8) | (pkt[19]));
                int subver = pkt[20];
                GetResultObj.FwVersion = fwVer + "." + subver;
            }
            else if (pkt[17] == 0x01)
            {
                GetResultObj.FwVersion = pkt[18].ToString("D3");
            }

            if (MBTestingFlag)
            {
                MBTestResultDir["软件版本"] = GetResultObj.FwVersion;
            }
            LOG("软件版本:" + GetResultObj.FwVersion);
        }



        //发送测试请求
        private void SendTestModeReq(byte mode)
        {
            byte[] data = { mode };
            //int wait = 0, n = 0;
            GetResultObj.testMode = -1;
            GetResultObj.testModeAllow = -1;

            SendSerialData(MakeSendArray((byte)Command.TestMode, data));
        }

        //发送温度测试请求
        private void SendTempTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_TEMP_TEST, null));
        }

        //发送柜锁测试请求
        private void SendLockTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_LOCK_TEST, null));
        }

        //发送电池测试请求
        private void SendBatteryTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_BATTERY_TEST, null));
        }

        //发送掉电测试请求
        private void SendPowerDownTestReq()
        {
            SendSerialData(MakeSendArray((byte)Command.CMD_POWER_DOWN_TEST, null));
        }

        //获取软件版本请求
        private void SendGetFwVersionReq(byte operate)
        {
            byte[] data = { operate };
            GetResultObj.FwVersion = "";
            SendSerialData(MakeSendArray((byte)Command.CMD_GET_FW, data));
            int waittime = 0, n = 0;
            while (GetResultObj.FwVersion == "")
            {
                Thread.Sleep(300);
                waittime++;
                if (waittime > 10)
                {
                    n++;
                    waittime = 0;
                    SendSerialData(MakeSendArray((byte)Command.CMD_GET_FW, data));
                }
                if (n > 3)
                {
                    break;
                }
            }
            if (n > 3)
            {
                if (MessageBox.Show("获取PCB软件版本失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendGetFwVersionReq(operate);
                }
            }
        }

        //设置PCB编号请求
        private void SendSetPcbCodeReq(byte type, string code)
        {
            List<byte> data = new List<byte>();
            data.Add(type);
            string str = Server.fillString(code, 16, '0', 0);
            data.AddRange(Server.stringToBCD(str));
            GetResultObj.SetPcbCode = -1;
            SendSerialData(MakeSendArray((byte)Command.CMD_SET_PCB, data.ToArray()));
            int wait = 0, n = 0;
            while (GetResultObj.SetPcbCode == -1)
            {
                Thread.Sleep(300);
                if (wait++ > 10)
                {
                    wait = 0;
                    n++;
                    SendSerialData(MakeSendArray((byte)Command.CMD_SET_PCB, data.ToArray()));
                }
                if (n > 3)
                {
                    break;
                }
            }

            if (n > 3)
            {
                if (MessageBox.Show("PCB编号设置失败！\r\n是否重试", "提示", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning) == DialogResult.Retry)
                {
                    SendSetPcbCodeReq(type, code);
                }
            }
        }

        //获取当前时间戳
        public static UInt32 GetCurrentTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToUInt32(ts.TotalSeconds);
        }

        //柜锁测试命令消息处理
        private void MessageTestLockHandle(byte[] pkt)
        {
            string sLock = null;
            string sLock1 = null;
            byte lockStatus;
            bool lockFlag = true;
            byte[] bit = new byte[8];

            if (pkt[17] == 0x0)//开锁成功
            {
                lockStatus = pkt[18];

                //bit=0开锁成功， bit=1开锁失败
                for (int i=0; i<8; i++)
                {
                    bit[i] = (byte)((lockStatus >> i) & 0x1);
                    if (bit[i] == 1)
                    {
                        lockFlag = false;
                    }
                }

                if (lockFlag == true)
                {
                    MBTestResultDir["柜锁"] = "通过";
                    updateControlText(skinLabel_MB_Lock_Result, "通过", Color.Green);
                    updateControlText(skinLabel_MB_Lock_Result1, "");
                    updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                }
                else
                {
                    sLock += "测试不通过，" + '\n' +
                             "柜锁1：" + bit[0].ToString() + '\n' +
                             "柜锁3：" + bit[2].ToString() + '\n' +
                             "柜锁5：" + bit[4].ToString() + '\n' +
                             "柜锁7：" + bit[6].ToString();
                    sLock1 += '\n' + "柜锁2：" + bit[1].ToString() + '\n' +
                             "柜锁4：" + bit[3].ToString() + '\n' +
                              "柜锁6：" + bit[5].ToString() + '\n' +
                              "柜锁8：" + bit[7].ToString();
                    MBTestResultDir["柜锁"] = "不通过";
                    updateControlText(skinLabel_MB_Lock_Result, sLock, Color.Red);
                    updateControlText(skinLabel_MB_Lock_Result1, sLock1, Color.Red);
                    updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                }
            }
        }

        //掉电测试命令消息处理
        private void MessageTestPowerDownHandle(byte[] pkt)
        {
            if (pkt[17] == 0x0)//掉电成功
            {
                MBTestResultDir["掉电"] = "通过";
                updateControlText(skinLabel_MB_PowerDown_Result, "通过", Color.Green);
                updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
            }
            else//掉电失败
            {
                MBTestResultDir["掉电"] = "不通过";
                updateControlText(skinLabel_MB_PowerDown_Result, "不通过", Color.Red);
                updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
            }

        }

        //电池测试命令消息处理
        private void MessageTestBatteryHandle(byte[] pkt)
        {
            if (pkt[17] == 0x0)//成功
            {
                MBTestResultDir["电池"] = "通过";
                updateControlText(skinLabel_MB_Battery_Result, "通过", Color.Green);
                updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
            }
            else
            {
                MBTestResultDir["电池"] = "不通过";
                updateControlText(skinLabel_MB_Battery_Result, "不通过", Color.Red);
                updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
            }
        }

        //温度测试命令消息处理
        private void MessageTestTempHandle(byte[] pkt)
        {
            string sTemp=null;
            string sTemp1 = null;
            bool testFlag = true;
            int  tempLowerLimit= Convert.ToInt32(TestSettingInfo["TempLowerLimit"]);
            int tempUpperLimit = Convert.ToInt32(TestSettingInfo["TempUpperLimit"]);

            //sTemp += '第';
            if (pkt[17] == 0x0)//成功
            {
                for (byte i = 0; i < 8; i++)
                {
                    if ((pkt[18 + i] >= tempLowerLimit) && (pkt[18 + i] <= tempUpperLimit))
                    {

                    }
                    else
                    {
                        //sTemp += i.ToString();
                        testFlag = false;
                    }
                }

                if (testFlag == true)
                {
                    MBTestResultDir["温度"] = "通过";
                    updateControlText(skinLabel_MB_Temp_Result, "通过", Color.Green);
                    updateControlText(skinLabel_MB_Temp_Result1, "");
                    updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                }
                else
                {
                    sTemp += "测试不通过，" + '\n' +
                             "温度1：" + pkt[18].ToString() + '\n' +
                             "温度3：" + pkt[20].ToString() + '\n' +
                             "温度5：" + pkt[22].ToString() + '\n' +
                             "温度7：" + pkt[24].ToString();
                    sTemp1 += '\n' + "温度2：" + pkt[19].ToString() + '\n' +
                             "温度4：" + pkt[21].ToString() + '\n' +
                              "温度6：" + pkt[23].ToString() + '\n' +
                              "温度8：" + pkt[25].ToString();
                    MBTestResultDir["温度"] = "不通过";
                    updateControlText(skinLabel_MB_Temp_Result, sTemp, Color.Red);
                    updateControlText(skinLabel_MB_Temp_Result1, sTemp1, Color.Red);
                    updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                }
            }
            else
            {
                LOG("温度检测失败.\n");
            }
        }

        //测试模式命令消息处理
        private void MessageTestModeHandle(byte[] pkt)
        {
            GetResultObj.testMode = pkt[17];
            GetResultObj.testModeAllow = pkt[18];

            MBTestingFlag = false;

            if (TestMeunSelectIndex == 0)
            { //PCBA测试
                if (PCBATestSelectIndex == 0)
                { //主板测试
                    if (GetResultObj.testMode == 0x00)//开始测试ack
                    {
                        if (GetResultObj.testModeAllow == 0x00)//成功
                        {
                            LOG("主板请求开始测试成功.");
                            MBTestingFlag = true;
                            MBTabSelectIndex = 1;
                            updateTableSelectedIndex(skinTabControl_MB, MBTabSelectIndex);

                            DateTime now = DateTime.Now;
                            MBTestResultDir.Clear();
                            MBTestResultDir.Add("PCB编号", textBox_MB_QRCode.Text.Trim());
                            MBTestResultDir.Add("测试员", Server.PresentAccount);
                            MBTestResultDir.Add("软件版本", "");
                            MBTestResultDir.Add("测试结果", "");
                            MBTestResultDir.Add("电源", "");
                            MBTestResultDir.Add("蓝牙", "");
                            MBTestResultDir.Add("温度", "");
                            MBTestResultDir.Add("柜锁", "");
                            MBTestResultDir.Add("掉电", "");
                            MBTestResultDir.Add("电池", "");
                            MBTestResultDir.Add("测试时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            MBTestResultDir.Add("测试用时", "0");

                            GetResultObj.UsedTime_main = GetCurrentTimeStamp();

                            if (MBTestThread != null)
                            {
                                MBTestThread.Abort();
                                MBTestThread = null;
                            }
                            MBTestThread = new Thread(MainBoardTestProcess);
                            MBTestThread.Start();
                        }
                        else
                        {
                            LOG("主板请求开始测试失败.");
                            MBTestingFlag = false;
                        }
                    }
                    else
                    {//结束测试ack
                        LOG("主板请求结束测试成功.");
                        MBTestingFlag = false;
                    }
                }
            }
        }



        

        int countdownTime;
        //主板测试线程
        private void MainBoardTestProcess()
        {
            bool selectIndexUpgradeFlag = false;
            countdownTime = Convert.ToInt32(TestSettingInfo["CountDown"]);
            while (MBTestingFlag == true)
            {
                if (PreMBTabSelectIndex != MBTabSelectIndex)
                {
                    PreMBTabSelectIndex = MBTabSelectIndex;
                    selectIndexUpgradeFlag = true;
                }

                switch (MBTabSelectIndex)
                {
                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_PCBA:
                        LOG("扫描主板二维码.");
                        break;
                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_POWER://电源检测
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.PowerSource = countdownTime;
                            MBTestResultDir["电源"] = "";
                            updateControlText(skinLabel_MB_POWER_RESULT, "");
                            LOG("检测电源是否正常.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("检测电源超时.");
                            MBTestResultDir["电源"] = "不通过";
                            updateControlText(skinLabel_MB_POWER_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;
              
                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_BLUETOOTH://蓝牙检测
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.BLE = countdownTime;
                            MBTestResultDir["蓝牙"] = "";
                            updateControlText(skinLabel_MB_BT_RESULT, "");
                            LOG("蓝牙测试.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("蓝牙测试时间超时.");
                            MBTestResultDir["蓝牙"] = "不通过";
                            updateControlText(skinLabel_MB_BT_RESULT, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;

                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_TEMP://温度检测
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.Temp = countdownTime;
                            MBTestResultDir["温度"] = "";
                            updateControlText(skinLabel_MB_Temp_Result, "");
                            updateControlText(skinLabel_MB_Temp_Result1, "");
                            SendTempTestReq();
                            LOG("温度测试.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("温度测试时间超时.");
                            MBTestResultDir["温度"] = "不通过";
                            updateControlText(skinLabel_MB_Temp_Result, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;

                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_LOCK://柜锁检测
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.Lock = countdownTime;
                            MBTestResultDir["柜锁"] = "";
                            updateControlText(skinLabel_MB_Lock_Result, "");
                            updateControlText(skinLabel_MB_Lock_Result1, "");
                            SendLockTestReq();
                            LOG("柜锁测试.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("柜锁测试时间超时.");
                            MBTestResultDir["柜锁"] = "不通过";
                            updateControlText(skinLabel_MB_Lock_Result, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;

                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_POWER_DOWN://掉电检测
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.PowerDown = countdownTime;
                            MBTestResultDir["掉电"] = "";
                            updateControlText(skinLabel_MB_PowerDown_Result, "");
                            //SendPowerDownTestReq();
                            LOG("掉电测试.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("掉电测试时间超时.");
                            MBTestResultDir["掉电"] = "不通过";
                            updateControlText(skinLabel_MB_PowerDown_Result, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;

                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_BATTERY://电池检测
                        if (selectIndexUpgradeFlag == true)
                        {
                            selectIndexUpgradeFlag = false;
                            ItemTestTime = GetCurrentTimeStamp();
                            countDownTime_MB.Battery = countdownTime;
                            MBTestResultDir["电池"] = "";
                            updateControlText(skinLabel_MB_Battery_Result, "");
                            SendBatteryTestReq();
                            LOG("电池检测.");
                        }
                        if ((GetCurrentTimeStamp() - ItemTestTime) >= 30)//超时
                        {
                            LOG("电池检测时间超时.");
                            MBTestResultDir["电池"] = "不通过";
                            updateControlText(skinLabel_MB_Battery_Result, "不通过", Color.Red);
                            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
                        }
                        break;

                    case (byte)MB_TEST_ITEM.MB_TEST_ITEM_STOP_TEST://结束测试

                        LOG("获取软件版本号。");
                        SendGetFwVersionReq(0x00);
                        Thread.Sleep(100);
                       // SendSetPcbCodeReq(0x00, textBox_MB_QRCode.Text.Trim());
                       // Thread.Sleep(100);
                        GetResultObj.UsedTime_main = GetCurrentTimeStamp() - GetResultObj.UsedTime_main;
                        MBTestResultDir["测试用时"] = (GetResultObj.UsedTime_main / 60) + "分 " + ((GetResultObj.UsedTime_main) % 60) + "秒";
                        MBTestResultDir = ModifyResultData(MBTestResultDir);
                        LOG("结束测试\r\n用时:" + MBTestResultDir["测试用时"]);

                        ShowMainboardResult();
                        SendTestModeReq(0x01);

                        //写入excel表
                        Server.WriteReport(TestSettingInfo["ChargerModel"] + "_PCBA_主板.xlsx", TestSettingInfo["ChargerModel"] + "_PCBA_主板", MBTestResultDir);
                        /*
                        string mysqlCmd = Server.MainboardTestMysqlCommand(
                            TestSettingInfo["ChargerModel"].ToString(),
                            MBTestResultDir["PCB编号"],
                            MBTestResultDir["测试员"],
                            MBTestResultDir["软件版本"],
                            MBTestResultDir["测试结果"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["电源"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["LCD"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["喇叭"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["按键"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["EEPROM"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["蓝牙"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["2G"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["信号值"],
                            MBTestResultDir["继电器"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["CP电压"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["接地检测"] == "通过" ? "Pass" : "Fail",
                            MBTestResultDir["测试时间"],
                            GetResultObj.UsedTime_main
                            );

                        if (Server.SendMysqlCommand(mysqlCmd, true) == true)
                        {
                            LOG("主板测试记录添加数据库成功");
                            Server.DealBackUpData(Server.backupMysqlCmdFile);
                        }*/
                        
                        updateControlText(textBox_MB_QRCode, "");
                        MBTestingFlag = false;
                        break;
                    default:
                        break;
                }
                Thread.Sleep(200);
            }
        }
        public Color decideColor(string text)
        {

            switch (text)
            {
                case "通过":
                    return Color.Green;

                case "不通过":
                    return Color.Red;

                default:
                    return Color.Black;

            }
        }
        private void ShowMainboardResult()
        {
            updateControlText(MB_PCB_RESULT_VAL, MBTestResultDir["PCB编号"], Color.Black);
            updateControlText(MB_TESTOR_RESULT_VAL, MBTestResultDir["测试员"], Color.Black);
            updateControlText(MB_FW_RESULT_VAL, MBTestResultDir["软件版本"], Color.Black);
            updateControlText(MB_ALL_RESULT_VAL, MBTestResultDir["测试结果"], decideColor(MBTestResultDir["测试结果"]));
            updateControlText(MB_POWER_RESULT_VAL, MBTestResultDir["电源"], decideColor(MBTestResultDir["电源"]));
            updateControlText(MB_BT_RESULT_VAL, MBTestResultDir["蓝牙"], decideColor(MBTestResultDir["蓝牙"]));
            updateControlText(MB_TEMP_RESULT_VAL, MBTestResultDir["温度"], decideColor(MBTestResultDir["温度"]));
            updateControlText(MB_POWER_DOWN_RESULT_VAL, MBTestResultDir["掉电"], decideColor(MBTestResultDir["掉电"]));
            updateControlText(MB_LOCK_RESULT_VAL, MBTestResultDir["柜锁"], decideColor(MBTestResultDir["柜锁"]));
            updateControlText(MB_BATTERY_RESULT_VAL, MBTestResultDir["电池"], decideColor(MBTestResultDir["电池"]));
            updateControlText(MB_TEST_USED_TIME_VAL, MBTestResultDir["测试用时"], Color.Black);
            updateControlText(MB_TEST_START_TIME, MBTestResultDir["测试时间"], Color.Black);
        }

        private void skinButton_SaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                TestSettingInfo["ChargerModel"] = comboBox_ChargerModel.SelectedItem;
                TestSettingInfo["CountDown"] = numericUpDownTestWaittime.Value;
                TestSettingInfo["TempLowerLimit"] = numericUpDown_TempLowerLimit.Value;
                TestSettingInfo["TempUpperLimit"] = numericUpDown_TempUpperLimit.Value;
                Server.WriteConfig(Server.testConfigFile, TestSettingInfo);
                MessageBox.Show("保存成功", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }


        //更新控件的文字内容
        private void updateControlText(Control control, string text)
        {
            try
            {
                control.Invoke(
                new MethodInvoker(delegate {
                    control.Text = text;
                }
              )
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //更新控件的文字内容及颜色
        private void updateControlText(Control control, string text, Color color)
        {
            try
            {
                control.Invoke(
                new MethodInvoker(delegate {

                    control.Text = text;
                    control.ForeColor = color;
                }
              )
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void updateControlColor(Control control, Color color)
        {
            try
            {
                control.Invoke(
                new MethodInvoker(delegate {
                    control.BackColor = color;
                }
              )
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void textBox_MB_QRCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                skinButton_MB_Confirm_Click(sender, e);
            }
        }

        private void skinButton_MB_Confirm_Click(object sender, EventArgs e)
        {
            if (textBox_MB_QRCode.Text == "")
            {
                MessageBox.Show("PCB编码不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox_MB_QRCode.Text = "";
                TestSettingInfo["ChargerModel"] = comboBox_ChargerModel.SelectedItem;
                return;
            } 

            MBTestingFlag = true;
            LOG("主板请求开始测试.");
            SendTestModeReq((byte)TEST_MODE.TEST_MODE_START);
        }

        private void skinTabControl_Menu_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if ((getPresentTabPage(skinTabControl_Menu) == skinTabPage_PCBA))
            {
                if (serialPort1.IsOpen == false)
                {
                    MessageBox.Show("请先打开串口", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
        }

        //主板测试项索引更新监听
        private void skinTabControl_MB_SelectedIndexChanged(object sender, EventArgs e)
        {
            MBTabSelectIndex = skinTabControl_MB.SelectedIndex;
            
            if (skinTabControl_MB.SelectedIndex == 0)
            {
                MBTestingFlag = false;
                textBox_MB_QRCode.Focus();
            }
        }

        //菜单测试项索引更新监听
        private void skinTabControl_Menu_SelectedIndexChanged(object sender, EventArgs e)
        {
            MBTestingFlag = false;
            TestMeunSelectIndex = skinTabControl_Menu.SelectedIndex;
            switch (skinTabControl_Menu.SelectedIndex)
            {
                case 0://PCBA测试
                    skinTabControl_MB.SelectedIndex = 0;
                    textBox_MB_QRCode.Focus();
                    updateControlText(textBox_MB_QRCode, "");
                    break;
                case 1://测试设置
                    skinComboBox_SerialPortNum.Focus();
                    break;
                default:
                    break;

            }
        }

        private void skinButton_MB_Power_Success_Click(object sender, EventArgs e)
        {
            LOG("主板检测电源成功.");
            MBTestResultDir["电源"] = "通过";
            updateControlText(skinLabel_MB_POWER_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_Power_Fail_Click(object sender, EventArgs e)
        {
            LOG("主板检测电源失败.");
            MBTestResultDir["电源"] = "不通过";
            updateControlText(skinLabel_MB_POWER_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_Power_Over_Click(object sender, EventArgs e)
        {
            LOG("跳过主板检测电源.");
            //MBTestResultDir["电源"] = "跳过";
            updateControlText(skinLabel_MB_POWER_RESULT, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_BT_SKIP_Click(object sender, EventArgs e)
        {
            LOG("跳过主板蓝牙测试.");
            updateControlText(skinLabel_MB_BT_RESULT, "跳过", Color.Red);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_BT_RETEST_Click(object sender, EventArgs e)
        {
            ItemTestTime = GetCurrentTimeStamp();
            countDownTime_MB.BLE = countdownTime;
            MBTestResultDir["蓝牙"] = "";
            updateControlText(skinLabel_MB_BT_RESULT, "");
            LOG("蓝牙重新测试.");
        }

        private void skinButton_MB_BT_SUCCESS_Click(object sender, EventArgs e)
        {
            LOG("主板检测蓝牙成功.");
            MBTestResultDir["蓝牙"] = "通过";
            updateControlText(skinLabel_MB_BT_RESULT, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_BT_FAIL_Click(object sender, EventArgs e)
        {
            LOG("主板检测蓝牙失败.");
            MBTestResultDir["蓝牙"] = "不通过";
            updateControlText(skinLabel_MB_BT_RESULT, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);

        }

        private void skinButton_MB_Temp_Skip_Click(object sender, EventArgs e)
        {
            
            LOG("跳过主板温度检测.");
            //MBTestResultDir["温度"] = "不通过";
            updateControlText(skinLabel_MB_Temp_Result, "跳过", Color.Green);
            updateControlText(skinLabel_MB_Temp_Result1, "");
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_Temp_rTest_Click(object sender, EventArgs e)
        {
            ItemTestTime = GetCurrentTimeStamp();
            countDownTime_MB.Temp = countdownTime;
            MBTestResultDir["温度"] = "";
            updateControlText(skinLabel_MB_Temp_Result, "");
            updateControlText(skinLabel_MB_Temp_Result1, "");
            SendTempTestReq();
            LOG("温度重新测试.");
        }

        private void skinButton_MB_Lock_Skip_Click(object sender, EventArgs e)
        {
            LOG("跳过主板柜锁检测.");
            updateControlText(skinLabel_MB_Lock_Result, "跳过", Color.Green);
            updateControlText(skinLabel_MB_Lock_Result1, "");
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);

        }

        private void skinButton_MB_Lock_rTest_Click(object sender, EventArgs e)
        {
            ItemTestTime = GetCurrentTimeStamp();
            countDownTime_MB.Lock = countdownTime;
            MBTestResultDir["柜锁"] = "";
            updateControlText(skinLabel_MB_Lock_Result, "");
            updateControlText(skinLabel_MB_Lock_Result1, "");
            SendLockTestReq();
            LOG("主板柜锁重新测试.");

        }

        private void skinButton_MB_PowerDown_Success_Click(object sender, EventArgs e)
        {
            LOG("掉电检测成功.");
            MBTestResultDir["掉电"] = "通过";
            updateControlText(skinLabel_MB_PowerDown_Result, "通过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);

        }

        private void skinButton_MB_PowerDown_Fail_Click(object sender, EventArgs e)
        {
            LOG("掉电检测失败.");
            MBTestResultDir["掉电"] = "不通过";
            updateControlText(skinLabel_MB_PowerDown_Result, "不通过", Color.Red);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);

        }

        private void skinButton_MB_PowerDown_Skip_Click(object sender, EventArgs e)
        {
            LOG("跳过掉电检测.");
            updateControlText(skinLabel_MB_PowerDown_Result, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_PowerDown_rTest_Click(object sender, EventArgs e)
        {
            countDownTime_MB.PowerDown = countdownTime;
            MBTestResultDir["掉电"] = "";
            updateControlText(skinLabel_MB_PowerDown_Result, "");
            SendPowerDownTestReq();
            LOG("掉电测试.");
        }

        private void skinButton_MB_Battery_Skip_Click(object sender, EventArgs e)
        {
            LOG("跳过电池检测.");
            updateControlText(skinLabel_MB_Battery_Result, "跳过", Color.Green);
            updateTableSelectedIndex(skinTabControl_MB, ++MBTabSelectIndex);
        }

        private void skinButton_MB_Battery_rTest_Click(object sender, EventArgs e)
        {
            countDownTime_MB.Battery = countdownTime;
            MBTestResultDir["电池"] = "";
            updateControlText(skinLabel_MB_Battery_Result, "");
            SendBatteryTestReq();
            LOG("电池重新检测.");

        }

        private void skinButton_MB_Power_rTest_Click(object sender, EventArgs e)
        {

        }
    }
}



















