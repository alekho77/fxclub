using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace fxdataloader
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            textBoxLog.Clear();
            textBoxLog.AppendText(LoaderContext.APIVersion + "\r\n");
            LoaderContext.Context.OnFxErrorEvent += new FxErrorEventHandler(OnFxError);
            LoaderContext.Context.OnFxStatusChangeEvent += new FxStatusChangeEventHandler(OnFxStatusChange);
            LoaderContext.Context.OnFxPairMessageEvent += new FxPairMessageEventHandler(OnFxPairMessage);
            OnFxStatusChange(LoaderContext.APIStatus);
        }

        private void OnFxError(string msg)
        {
            if (textBoxLog.InvokeRequired)
            {
                BeginInvoke(new FxErrorEventHandler(OnFxError), new object[] { msg });
            }
            else
            {
                textBoxLog.AppendText("ERROR " + msg);
            }
        }

        private void OnFxStatusChange(FxStatus status)
        {
            toolStripStatusLabelStatus.Text = status.ToString();
            textBoxSchema.Enabled = (status == FxStatus.Offline);
            textBoxLogin.Enabled = (status == FxStatus.Offline);
            textBoxPassword.Enabled = (status == FxStatus.Offline);
            textBoxLanguage.Enabled = (status == FxStatus.Offline);
            buttonLogon.Enabled = true;
            buttonLogon.Text = (status == FxStatus.Offline) ? "Logon" : "Logout";
        }

        private void OnFxPairMessage(FxInstrument Inst, string operation)
        {
            if (textBoxLog.InvokeRequired)
            {
                BeginInvoke(new FxPairMessageEventHandler(OnFxPairMessage), new object[] { Inst, operation });
            }
            else
            {
                textBoxLog.AppendText("Pair " + Inst.name + " " + operation + ": buy=" + Inst.buy.ToString() + ", sell=" + Inst.sell.ToString() + "\r\n");
                if (Inst.name == "EURUSD")
                {
                    label2.Text = Inst.buy.ToString();
                }
            }
        }

        private void buttonLogon_Click(object sender, EventArgs e)
        {
            buttonLogon.Enabled = false;
            try
            {
                if (LoaderContext.APIStatus == FxStatus.Offline)
                {
                    LoaderContext.Logon(textBoxSchema.Text, textBoxLogin.Text, textBoxPassword.Text, textBoxLanguage.Text);
                    textBoxLog.AppendText("==== Logon: " + textBoxLogin.Text + " ====\r\n");
                    DateTime srvtime = LoaderContext.FxServerTime;
                    textBoxLog.AppendText("Server Time: " + srvtime.ToLongDateString() + " " + srvtime.ToLongTimeString() + "\r\n");
                    textBoxLog.AppendText("\r\n");
                    textBoxLog.AppendText("Chart intervals for candle charts:\r\n");
                    foreach (KeyValuePair<string, int> interval in LoaderContext.FxChartIntervals)
                    {
                        if (interval.Value < 60)
                        {
                            textBoxLog.AppendText(interval.Key + " = " + interval.Value.ToString() + " m\r\n");
                        } 
                        else
                        {
                            if (interval.Value < 24*60)
                            {
                                textBoxLog.AppendText(interval.Key + " = " + (interval.Value / 60).ToString() + " h\r\n");
                            } 
                            else
                            {
                                textBoxLog.AppendText(interval.Key + " = " + (interval.Value / (24*60)).ToString() + " d\r\n");
                            }
                        }
                    }
                    textBoxLog.AppendText("\r\n");
                    textBoxLog.AppendText("Pair's group's list:\r\n");
                    foreach (KeyValuePair<string, string> group in LoaderContext.FxPairGroups)
                    {
                        textBoxLog.AppendText(group.Key + ": " + group.Value + "\r\n");
                    }
                }
                else
                {
                    LoaderContext.Logout();
                    textBoxLog.AppendText("==== Logout: " + textBoxLogin.Text + " ====\r\n");
                }
            }
            catch (FxErrorException ex)
            {
                textBoxLog.AppendText("ERROR " + ex.Message + "\r\n");
            }
        }

        private void buttonGetPairs_Click(object sender, EventArgs e)
        {
            textBoxLog.AppendText("\r\n");
            textBoxLog.AppendText("List of available pairs (instruments):\r\n");
            foreach (FxInstrument inst in LoaderContext.FxPairs)
            {
                textBoxLog.AppendText(inst.id + ": " + inst.name + " (" + inst.type + ") - buy=" + inst.buy.ToString() + ", sell=" + inst.sell.ToString() + "\r\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBoxLog.AppendText("\r\n");
            textBoxLog.AppendText("Candle list:\r\n");
            foreach (Candle candle in LoaderContext.Test())
            {
                textBoxLog.AppendText(candle.time.ToString() + ": [" + candle.open.ToString() + "; " + candle.high.ToString() + "; " + candle.low.ToString() + "; " + candle.close.ToString() + "]\r\n");
            }
        }
    }
}
