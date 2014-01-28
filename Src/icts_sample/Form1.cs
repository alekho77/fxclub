using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using FxComApiTrader;
using Microsoft.Win32; // For read/write registry

namespace sample
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class Form1 : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxSchema;
        private System.Windows.Forms.TextBox textBoxLogin;
        private System.Windows.Forms.TextBox textBoxPassword;
        private System.Windows.Forms.Button buttonLogon;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.Button buttonLogout;

        FxComApiTrader.FxTraderApiClass API = new FxTraderApiClass();
        private Label labelConnectionStatus;

        public Form1()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public void API_OnError(FxComApiTrader.IFxError error)
        {
            Log("OnError: " + error.Message);
        }

        delegate void delegChangeStatus(string text, Color color);
        void ChangeStatus(string text, Color color)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBoxLog.InvokeRequired)
            {
                delegChangeStatus d = new delegChangeStatus(ChangeStatus);
                this.Invoke(d, new object[] {text, color});
            }
            else
            {
                labelConnectionStatus.Text      = text;
                labelConnectionStatus.BackColor = color;
            }
        }

        public void API_OnStatusChange(FxConnectionStatus status)
        {
            switch (status)
            {
                case FxConnectionStatus.cs_LoadingData:
                {
                    ChangeStatus("LoadingData...", Color.DarkGoldenrod);
                    break;
                }
                case FxConnectionStatus.cs_Offline:
                {
                    ChangeStatus("Offline", Color.Gray);
                    break;
                }
                case FxConnectionStatus.cs_Online:
                {
                    ChangeStatus("Online...", Color.Green);
                    break;
                }
                case FxConnectionStatus.cs_WaitingForConnection:
                {
                    ChangeStatus("WaitingForConnection...", Color.Magenta);
                    break;
                }
            }

            Log("OnStatusChange: " + status.ToString());
        }

        public void API_OnMarginCallMessage(IFxMarginCallMessage message)
        {
            Log(String.Format("OnMarginCallMessage: ID: {0}, Number: {1}, Level: {2}, Time: {3}", message.AccountId, message.MagrinNumber, message.MarginLevel, message.MarginTime));
        }

        public void API_OnAccountMessage(IFxAccount account, FxMessageType messageType)
        {
            Log(String.Format("OnAccountMessage({0}) ID: {1} Type: {2} Owner: {3} Balance: {4}", messageType.ToString(), account.AccountId, account.AccountType, account.MoneyOwner, account.Balance));
        }

        public void API_OnOrderMessage(IFxOrder order, FxMessageType messageType)
        {
            Log(String.Format("OnOrderMessage({0}) ID: {1} Type: {2} Condition: {3} Initiator: {4}", messageType.ToString(), order.OrderId, order.OrderType, order.Condition, order.Initiator));
        }

        public void API_OnPairMessage(IFxPair pair, FxMessageType messageType)
        {
            Log(String.Format("OnPairMessage({0}): ID: {1} Buy: {2} Sell: {3}", messageType.ToString(), pair.Name, pair.Low, pair.High));
        }

        public void API_OnRulesChange()
        {
            Log("OnRulesChange");
        }

        public void API_OnTextMessage(IFxTextMessage message)
        {
            Log(String.Format("OnTextMessage: <{0}>: {1}", message.Sender, message.Text));
        }

        public void API_OnTradeMessage(IFxTrade trade, FxMessageType messageType)
        {
            Log(String.Format("OnTradeMessage({0}) ID: {1} PairId: {2} OpenTime: {3}",
               messageType.ToString(),
               trade.TradeId,
               trade.PairId,
               trade.OpenTime.ToString("HH:mm:ss")
               ));
        }


        delegate void delegLog(string text);
        void Log(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBoxLog.InvokeRequired)
            {
                BeginInvoke(new delegLog(Log), new object[] { text });
            }
            else
            {
                this.textBoxLog.Text += text + "\r\n";
                textBoxLog.Select(textBoxLog.Text.Length, 0);
                textBoxLog.ScrollToCaret();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSchema = new System.Windows.Forms.TextBox();
            this.textBoxLogin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonLogon = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.buttonLogout = new System.Windows.Forms.Button();
            this.labelConnectionStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Schema:";
            // 
            // textBoxSchema
            // 
            this.textBoxSchema.Location = new System.Drawing.Point(65, 12);
            this.textBoxSchema.Name = "textBoxSchema";
            this.textBoxSchema.Size = new System.Drawing.Size(100, 20);
            this.textBoxSchema.TabIndex = 1;
            this.textBoxSchema.Text = "demo.20";
            // 
            // textBoxLogin
            // 
            this.textBoxLogin.Location = new System.Drawing.Point(217, 12);
            this.textBoxLogin.Name = "textBoxLogin";
            this.textBoxLogin.Size = new System.Drawing.Size(100, 20);
            this.textBoxLogin.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(173, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Login:";
            // 
            // textBoxPassword
            // 
            this.textBoxPassword.Location = new System.Drawing.Point(389, 12);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new System.Drawing.Size(100, 20);
            this.textBoxPassword.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(325, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Password:";
            // 
            // buttonLogon
            // 
            this.buttonLogon.Location = new System.Drawing.Point(8, 40);
            this.buttonLogon.Name = "buttonLogon";
            this.buttonLogon.Size = new System.Drawing.Size(75, 23);
            this.buttonLogon.TabIndex = 6;
            this.buttonLogon.Text = "Logon";
            this.buttonLogon.Click += new System.EventHandler(this.buttonLogon_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(8, 72);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(683, 318);
            this.textBoxLog.TabIndex = 7;
            // 
            // buttonLogout
            // 
            this.buttonLogout.Location = new System.Drawing.Point(88, 40);
            this.buttonLogout.Name = "buttonLogout";
            this.buttonLogout.Size = new System.Drawing.Size(75, 23);
            this.buttonLogout.TabIndex = 8;
            this.buttonLogout.Text = "Logoff";
            this.buttonLogout.Click += new System.EventHandler(this.buttonLogout_Click);
            // 
            // labelConnectionStatus
            // 
            this.labelConnectionStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelConnectionStatus.BackColor = System.Drawing.Color.Gray;
            this.labelConnectionStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelConnectionStatus.Location = new System.Drawing.Point(500, 13);
            this.labelConnectionStatus.Name = "labelConnectionStatus";
            this.labelConnectionStatus.Size = new System.Drawing.Size(191, 50);
            this.labelConnectionStatus.TabIndex = 10;
            this.labelConnectionStatus.Text = "Offline";
            this.labelConnectionStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(703, 400);
            this.Controls.Add(this.labelConnectionStatus);
            this.Controls.Add(this.buttonLogout);
            this.Controls.Add(this.textBoxLog);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxLogin);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxSchema);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonLogon);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ICTS Trader API Test";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new Form1());
        }

        private void buttonLogon_Click(object sender, System.EventArgs e)
        {
            // Saving to registry schema/login/password
            RegistryKey key = Registry.CurrentUser.CreateSubKey("ActForex");
            RegistryKey key01 = key.CreateSubKey("FXCOMAPITrader");
            key01.SetValue("Schema", textBoxSchema.Text);
            key01.SetValue("Login", textBoxLogin.Text);
            key01.SetValue("Password", textBoxPassword.Text);

            IFxError error = API.Logon(textBoxSchema.Text, textBoxLogin.Text, textBoxPassword.Text, "en_US", "879hozin");

            if (error != null)
            {
                MessageBox.Show(error.Message, "Error");
                return;
            }

            Log("======= Pairs =======");
            int iCount = API.Pairs.Count;
            Log("Count: " + iCount.ToString());
            for (int i = 0; i < iCount; i++)
            {
                IFxPair pair = API.Pairs.get_Pair(i);
                Log(String.Format("ID: {0} Name: {1} BuyRate: {2} SellRate: {3}",
                   pair.PairId,
                   pair.Name,
                   pair.BuyRate,
                   pair.SellRate
                   ));
            }

            Log("======= Accounts =======");
            iCount = API.Accounts.Count;
            Log("Count: " + iCount.ToString());
            for (int i = 0; i < iCount; i++)
            {
                IFxAccount account = API.Accounts.get_Account(i);
                Log(String.Format(
                   "ID: {0} Type: {1} Active: {2} Balance: {3} CustId: {4}",
                   account.AccountId,
                   account.AccountType,
                   account.Active,
                   account.Balance,
                   account.CustId
                   ));
            }
            
            Log("======= Orders =======");
            iCount = API.Orders.Count;
            Log("Count: " + iCount.ToString());
            for (int i = 0; i < iCount; i++)
            {
                IFxOrder order = API.Orders.get_Order(i);
                Log(String.Format(
                   "ID: {0} PairID: {1} Time: {2}",
                   order.OrderId,
                   order.PairId,
                   order.Time.ToString("HH:mm:ss")
                   ));
            }

            ShowTrades();
        }

        void ShowTrades()
        {
            Log("======= Trades =======");
            int iCount = API.Trades.Count;
            Log("Count: " + iCount.ToString());
            for (int i = 0; i < iCount; i++)
            {
                IFxTrade trade = API.Trades.get_Trade(i);
                Log(String.Format(
                   "ID: {0} PairId: {1} OpenTime: {2}",
                   trade.TradeId,
                   trade.PairId,
                   trade.OpenTime.ToString("HH:mm:ss")
                   ));
            }
        }

        private void buttonLogout_Click(object sender, System.EventArgs e)
        {
            IFxError error = API.Logoff();
            if (error != null)
            {
                MessageBox.Show(error.Message, "Error");
                return;
            }
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            // Reading from registry schema/login/password
            RegistryKey key = Registry.CurrentUser.OpenSubKey("ActForex");
            if (key == null)
                return;
            RegistryKey key01 = key.OpenSubKey("FXCOMAPITrader");
            if (key01 == null)
                return;
            textBoxSchema.Text   = (string)key01.GetValue("Schema"  );
            textBoxLogin.Text    = (string)key01.GetValue("Login"   );
            textBoxPassword.Text = (string)key01.GetValue("Password");

            API.OnStatusChange      += new IFxTraderApiEvents_OnStatusChangeEventHandler     (API_OnStatusChange     );
            API.OnMarginCallMessage += new IFxTraderApiEvents_OnMarginCallMessageEventHandler(API_OnMarginCallMessage);
            API.OnOrderMessage      += new IFxTraderApiEvents_OnOrderMessageEventHandler     (API_OnOrderMessage     );
            API.OnAccountMessage    += new IFxTraderApiEvents_OnAccountMessageEventHandler   (API_OnAccountMessage   );
            API.OnRulesChange       += new IFxTraderApiEvents_OnRulesChangeEventHandler      (API_OnRulesChange      );
            API.OnTextMessage       += new IFxTraderApiEvents_OnTextMessageEventHandler      (API_OnTextMessage      );
            API.OnTradeMessage      += new IFxTraderApiEvents_OnTradeMessageEventHandler     (API_OnTradeMessage     );
            API.OnError             += new IFxTraderApiEvents_OnErrorEventHandler            (API_OnError            );
            API.OnPairMessage       += new IFxTraderApiEvents_OnPairMessageEventHandler      (API_OnPairMessage      );
        }
    }
}
