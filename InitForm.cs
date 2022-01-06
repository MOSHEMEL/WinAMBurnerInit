using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAMBurner;

namespace WinAMBurnerInit
{
    public partial class InitForm : Form
    {
        private Am am;
        private Button connect;
        private ProgressBar progressBar;
        private Label lsnum;
        private Label lmaxi;
        private Label lfactor;
        private Label ldate;
        private RichTextBox date;
        private bool checks;
        private Field snum;
        private Field maxi;
        private Field factor;
        private Label laptxid;
        private RadioButton lerase;
        private RadioButton lkeep;

        public InitForm()
        {
            InitializeComponent();

            new Field(ltype: typeof(PictureBox), lplaceh: Place.Twoh, lplacev: Place.One).draw(this, true);
            new Field(ltype: typeof(Label), ltext: "AM Burner", font: Field.DefaultFontLarge, lplacev: Place.One).draw(this, true);

            (progressBar = new Field(ltype: typeof(ProgressBar), width: Field.DefaultWidthEntity, height: Field.DefaultHeightSmall, lplaceh: Place.Fiveh, lplacev: Place.Seven).draw(this, true) as ProgressBar)
                .Visible = false;
            connect = new Field(ltype: typeof(Button), ltext: "Connect", width: Field.DefaultWidthEntity, color: Color.Red, eventHandler: connect_Click, lplacev: Place.Seven).draw(this, true) as Button;
            new Field(ltype: typeof(Button), ltext: "Read", width: Field.DefaultWidthEntity, eventHandler: read_Click, lplaceh: Place.Fiveh, lplacev: Place.Two).draw(this, true);
            //new Field(ltype: typeof(CheckBox), ltext: "Erase Chip", width: Field.DefaultWidthEntity, autosize: false, eventHandler: erase_Click, lplaceh: Place.Fiveh, lplacev: Place.Three).draw(this, true);
            new Field(ltype: typeof(Button), ltext: "Burn", width: Field.DefaultWidthEntity, eventHandler: burn_Click, lplaceh: Place.Fiveh, lplacev: Place.Three).draw(this, true);
            new Field(ltype: typeof(Button), ltext: "Dump Memory", width: Field.DefaultWidthEntity, eventHandler: dump_Click, lplaceh: Place.Fiveh, lplacev: Place.Four).draw(this, true);

            new Field(ltype: typeof(Label), ltext: "AM Serial #:", width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Two).draw(this, true);
            lsnum = new Field(ltype: typeof(Label), width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Two).draw(this, true) as Label;
            (snum = new Field(type: typeof(RichTextBox), dflt: "AM Serial #", width: Field.DefaultWidthEntity, autosize: false, placeh: Place.Fourh, placev: Place.Two)).draw(this, false);

            new Field(ltype: typeof(Label), ltext: "Maximum Pulses:", width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Three).draw(this, true);
            lmaxi = new Field(ltype: typeof(Label), width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Three).draw(this, true) as Label;
            (maxi = new Field(type: typeof(RichTextBox), dflt: "Maximum Pulses", width: Field.DefaultWidthEntity, autosize: false, placeh: Place.Fourh, placev: Place.Three)).draw(this, false);

            new Field(ltype: typeof(Label), ltext: "Current AM:", width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Four).draw(this, true);
            lfactor = new Field(ltype: typeof(Label), width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Four).draw(this, true) as Label;
            (factor = new Field(type: typeof(RichTextBox), dflt: "Current AM", width: Field.DefaultWidthEntity, autosize: false, placeh: Place.Fourh, placev: Place.Four)).draw(this, false);

            new Field(ltype: typeof(Label), ltext: "CU Id:", width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Five).draw(this, true);
            laptxid = new Field(ltype: typeof(Label), width: Field.DefaultWidthAPTXID, autosize: false, lplacev: Place.Five).draw(this, true) as Label;
            (lkeep = new Field(ltype: typeof(RadioButton), ltext: "Keep CU Id", width: Field.DefaultWidthEntity, autosize: false, eventHandler: radio_CheckedChanged, lplaceh: Place.Fiveh, lplacev: Place.Five).draw(this, true) as RadioButton)
                .Checked = true;
            lerase = new Field(ltype: typeof(RadioButton), ltext: "Erase CU Id", width: Field.DefaultWidthEntity, autosize: false, eventHandler: radio_CheckedChanged, lplaceh: Place.Fiveh, lplacev: Place.Six).draw(this, true) as RadioButton;

            new Field(ltype: typeof(Label), ltext: "Date:", width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Six).draw(this, true);
            ldate = new Field(ltype: typeof(Label), width: Field.DefaultWidthEntity, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Six).draw(this, true) as Label;

            connectAm();
        }

        private async void connect_Click(object sender, EventArgs e)
        {
            await connectAm();
        }

        private async Task connectAm()
        {
            am = new Am(FormMain.progressBar_Callback);
            FormMain.Enables(this, false);
            progressBar.Minimum = 0;
            progressBar.Value = progressBar.Minimum;
            progressBar.Maximum = 3;
            am.progress = progressBar;
            progressBar.Visible = true;

            connect.Text = "Connecting..";
            if (await am.AMCheckConnect() == ErrCode.OK)
            {
                connect.Text = "Disconnect";
                connect.Click -= connect_Click;
                connect.Click += disconnect_Click;
                connect.ForeColor = Color.Green;
                progressBar.Value = progressBar.Maximum;
            }
            else
            {
                await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
                connect.Text = "Connect";
            }
            progressBar.Visible = false;
            FormMain.Enables(this, true);
        }

        private void disconnect_Click(object sender, EventArgs e)
        {
            am = null;
            connect.Text = "Connect";
            connect.Click -= disconnect_Click;
            connect.Click += connect_Click;
            connect.ForeColor = Color.Red;
        }

        private async void read_Click(object sender, EventArgs e)
        {
            if (am != null)
            {
                FormMain.Enables(this, false);
                progressBar.Minimum = 0;
                progressBar.Value = progressBar.Minimum;
                progressBar.Maximum = 27;
                am.progress = progressBar;
                progressBar.Visible = true;

                if (await am.AMCmd(Cmd.READALL) != ErrCode.ECONNECT)
                    assign();
                else
                    await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
                progressBar.Value = progressBar.Maximum;
                progressBar.Visible = false;
                FormMain.Enables(this, true);
            }
            else
                await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
        }

        private void assign()
        {
            lsnum.Text = am.SNum.ToString();
            lfactor.Text = am.Factor.ToString();
            lmaxi.Text = am.Maxi.ToString();
            ldate.Text = am.Date.ToString();
            laptxid.Text = am.AptxId.Aggregate("", (r, m) => r += m.ToString("X") + " ");
        }

        private async void burn_Click(object sender, EventArgs e)
        {
            if ((am != null) && (am.SNum != Am.ERROR) &&
                (snum.checkValid()) && (maxi.checkValid()) && (factor.checkValid()))
            {
                am.SNum = (uint)Field.stringToInt(snum.val);
                am.Factor = (uint)Field.stringToInt(factor.val);
                am.Maxi = (uint)Field.stringToInt(maxi.val);
                if (lerase.Checked)
                    am.AptxId = new uint[] { Am.ERROR, Am.ERROR, Am.ERROR };
                FormMain.Enables(this, false);
                progressBar.Minimum = 0;
                progressBar.Value = progressBar.Minimum;
                progressBar.Maximum = 93;
                am.progress = progressBar;
                progressBar.Visible = true;

                if (await am.AMCmd(Cmd.INIT) != ErrCode.ECONNECT)
                    assign();
                else
                    await FormMain.notify("Error", "Error, to burn AM properly: \n   1. Connect AM \n   2. Read AM \n   3. Fill the form", "OK");
                progressBar.Visible = false;
                FormMain.Enables(this, true);
            }
            else
                await FormMain.notify("Error", "Error, to burn AM properly: \n   1. Connect AM \n   2. Read AM \n   3. Fill the form", "OK");
        }

        private void radio_CheckedChanged(object sender, EventArgs e)
        {
            if ((lkeep != null) && (lerase != null))
            {
                if (lerase.Checked)
                {
                    lkeep.Checked = false;
                }
                if (lkeep.Checked)
                {
                    lerase.Checked = false;
                }
            }
        }

        private void dump_Click(object sender, EventArgs e)
        {
        }

        private void InitForm_Load(object sender, EventArgs e)
        {

        }
    }
}
