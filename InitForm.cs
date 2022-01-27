using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinAMBurner;

namespace WinAMBurnerInit
{
    public partial class InitForm : Form
    {
        private const double version = 1.1;
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

            Text += " Version " + version;

            new Field(ltype: typeof(PictureBox), lplaceh: Place.Twoh, lplacev: Place.One).draw(this, true);
            new Field(ltype: typeof(Label), ltext: "AM Burner", width: Field.DefaultWidthAMBurner, font: Field.DefaultFontLarge, lplacev: Place.One).draw(this, true);

            (progressBar = new Field(ltype: typeof(ProgressBar), width: Field.DefaultWidthAMBurner, lplaceh: Place.Fiveh, lplacev: Place.One).draw(this, true) as ProgressBar)
                .Visible = false;
            connect = new Field(ltype: typeof(Button), ltext: "Connect", width: Field.DefaultWidthAMBurner, autosize: false, color: Color.Red, eventHandler: connect_Click, lplaceh: Place.Fourh, lplacev: Place.Seven).draw(this, true) as Button;
            new Field(ltype: typeof(Button), ltext: "Read AM", width: Field.DefaultWidthAMBurner, autosize: false, eventHandler: read_Click, lplaceh: Place.Fiveh, lplacev: Place.Two).draw(this, true);
            //new Field(ltype: typeof(CheckBox), ltext: "Erase Chip", width: Field.DefaultWidthAMBurner, autosize: false, eventHandler: erase_Click, lplaceh: Place.Fiveh, lplacev: Place.Three).draw(this, true);
            new Field(ltype: typeof(Button), ltext: "Burn AM", width: Field.DefaultWidthAMBurner, autosize: false, eventHandler: burn_Click, lplaceh: Place.Fiveh, lplacev: Place.Three).draw(this, true);
            new Field(ltype: typeof(Button), ltext: "Dump AM", width: Field.DefaultWidthAMBurner, autosize: false, eventHandler: dump_Click, lplaceh: Place.Fiveh, lplacev: Place.Four).draw(this, true);
            new Field(ltype: typeof(Button), ltext: "Test AM", width: Field.DefaultWidthAMBurner, autosize: false, eventHandler: test_Click, lplaceh: Place.Fiveh, lplacev: Place.Seven).draw(this, true);

            new Field(ltype: typeof(Label), ltext: "AM Serial #:", width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Two).draw(this, true);
            lsnum = new Field(ltype: typeof(Label), width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Two).draw(this, true) as Label;
            (snum = new Field(type: typeof(RichTextBox), dflt: "AM Serial #", width: Field.DefaultWidthAMBurner, autosize: false, placeh: Place.Fourh, placev: Place.Two)).draw(this, false);

            new Field(ltype: typeof(Label), ltext: "Maximum Pulses:", width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Three).draw(this, true);
            lmaxi = new Field(ltype: typeof(Label), width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Three).draw(this, true) as Label;
            (maxi = new Field(type: typeof(RichTextBox), dflt: "Maximum Pulses", width: Field.DefaultWidthAMBurner, autosize: false, placeh: Place.Fourh, placev: Place.Three)).draw(this, false);

            new Field(ltype: typeof(Label), ltext: "Current AM:", width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Four).draw(this, true);
            lfactor = new Field(ltype: typeof(Label), width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Four).draw(this, true) as Label;
            (factor = new Field(type: typeof(RichTextBox), dflt: "Current AM", width: Field.DefaultWidthAMBurner, autosize: false, placeh: Place.Fourh, placev: Place.Four)).draw(this, false);

            new Field(ltype: typeof(Label), ltext: "CU Id:", width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Five).draw(this, true);
            laptxid = new Field(ltype: typeof(Label), width: Field.DefaultWidthAPTXID, height: Field.DefaultHeightLarge, autosize: false, lplacev: Place.Five).draw(this, true) as Label;
            //laptxid = new Field(ltype: typeof(Label), width: 500, autosize: false, lplacev: Place.Five).draw(this, true) as Label;
            (lkeep = new Field(ltype: typeof(RadioButton), ltext: "Keep CU Id", width: Field.DefaultWidthAMBurner, autosize: false, eventHandler: radio_CheckedChanged, lplaceh: Place.Fiveh, lplacev: Place.Five).draw(this, true) as RadioButton)
                .Checked = true;
            lerase = new Field(ltype: typeof(RadioButton), ltext: "Erase CU Id", width: Field.DefaultWidthAMBurner, autosize: false, eventHandler: radio_CheckedChanged, lplaceh: Place.Fiveh, lplacev: Place.Six).draw(this, true) as RadioButton;

            new Field(ltype: typeof(Label), ltext: "Date:", width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Twoh, lplacev: Place.Seven).draw(this, true);
            ldate = new Field(ltype: typeof(Label), width: Field.DefaultWidthAMBurner, autosize: false, lplaceh: Place.Threeh, lplacev: Place.Seven).draw(this, true) as Label;

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

                if (await am.AMCmd(Cmd.ID) == ErrCode.OK)
                {
                    await am.AMCmd(Cmd.READ_01);
                    await am.AMCmd(Cmd.READ_03_FF, blockNum: "FF");
                    assign();
                }
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
            //if ((am != null) && (am.SNum != Am.ERROR) &&
            //    (snum.checkValid()) && (maxi.checkValid()) && (factor.checkValid()))
            //{
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

            if (await am.AMCmd(Cmd.ID) == ErrCode.OK)
            {
                List<string> cmds = new List<string>();
                cmds.Add("scan,3,0#");
                cmds.Add("scan,3,0#");
                cmds.Add("scan,3,0#");
                for (int i = 0; i < 10; i++)
                    cmds.Add("NOP#");
                cmds.Add("nuke,3#");
                if (await am.AMCmd(Cmd.GENERAL, request: cmds, reply: "Nuked chip") != ErrCode.EFIND)
                {
                    await am.AMCmd(Cmd.WRITE_00);
                    await am.AMCmd(Cmd.READ_00);
                    await am.AMCmd(Cmd.WRITE_01);
                    await am.AMCmd(Cmd.READ_01);
                    await am.AMCmd(Cmd.WRITE_03_FF, "FF");
                    await am.AMCmd(Cmd.READ_03_FF, "FF");
                    assign();
                }
            }
            else
                await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
                //await FormMain.notify("Error", "Error, to burn AM properly: \n   1. Connect AM \n   2. Read AM \n   3. Fill the form", "OK");
            progressBar.Visible = false;
            FormMain.Enables(this, true);
            //}
            //else
            //    await FormMain.notify("Error", "Error, to burn AM properly: \n   1. Connect AM \n   2. Read AM \n   3. Fill the form", "OK");
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

        private async void dump_Click(object sender, EventArgs e)
        {
            if (am != null)
            {
                FormMain.Enables(this, false);
                progressBar.Minimum = 0;
                progressBar.Value = progressBar.Minimum;
                progressBar.Maximum = 600;
                am.progress = progressBar;
                progressBar.Visible = true;

                if (await am.AMCmd(Cmd.ID) == ErrCode.OK)
                {
                    List<string> cmds = new List<string>();
                    for (int i = 0; i < 500; i++)
                        cmds.Add("NOP#");
                    cmds.Add("dump#");
                    if (await am.AMCmd(Cmd.GENERAL, request: cmds, reply: "cs[dump] Done!") != ErrCode.EFIND)
                    //if(true)
                    {
                        string dataRdStr = File.ReadAllText(LogFile.logFileName);
                        string[] dataSplit = dataRdStr.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                        string[] temp = null;
                        string[] dump_txt = dataSplit;
                        do
                        {
                            temp = dump_txt.SkipWhile(d => !d.Contains("dump#")).Skip(1).ToArray();
                            if (temp.Count() != 0)
                            {
                                dump_txt = temp;
                            }
                        }
                        while (temp.Count() != 0);

                        //List<string> rows = dump_txt.SkipWhile(row => !row.Contains("cs[3] address: 0x0 translated as 0x0-0x0-0x0")).ToList();
                        //rows = rows.Where(row => !row.Contains("cs[3] address:")).TakeWhile(row => !row.Contains("cs[dump] Done!")).ToList();
                        //List<List<int>> nums = rows.Select(row => row.Split(' ')
                        //    .Select<String, int>(snum => 
                        //        int.Parse(snum.Skip(2).ToArray(), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture)).ToList()).ToList();

                        List<byte> dump_int =
                        dump_txt.SkipWhile(row => !row.Contains("cs[3] address: 0x0 translated as 0x0-0x0-0x0"))
                            .TakeWhile(row => !row.Contains("cs[dump] Done!"))
                            .Where(row => !row.Contains("cs[3] address:"))
                            .Select(row =>
                            {
                                List<byte> nums = row.Split(' ')
                                    .Select<String, byte>(snum =>
                                    {
                                        byte i = 0xFF;
                                        byte.TryParse(snum.Skip(2).ToArray(), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out i);
                                        return i;
                                    }).ToList();
                                return nums;
                            })
                            .Aggregate(new List<byte>(), (r, m) => { r.AddRange(m); return r; });
                        await File.WriteAllBytesAsync("AmBurnerDump" +
                            new string(DateTime.Now.ToString()
                            .Select(c => 
                            { 
                                if ((c == '/') || (c == '\\') || (c == ':') || (c == ' ')) 
                                    c = '_'; 
                                return c; 
                            }).ToArray()) +
                            ".bin", dump_int.ToArray());
                        await FormMain.notify("Dump AM", "Dump AM Done", "OK");
                    }
                    else
                        await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
                }
                else
                    await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
                progressBar.Value = progressBar.Maximum;
                progressBar.Visible = false;
                FormMain.Enables(this, true);
            }
            else
                await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
        }

        private async void test_Click(object sender, EventArgs e)
        {
            if (am != null)
            {
                if (await FormMain.notify("Test AM", "AM contents will be permanently deleted, are you soure you want to proceed?", "Yes", "No"))
                {
                    FormMain.Enables(this, false);
                    progressBar.Minimum = 0;
                    progressBar.Value = progressBar.Minimum;
                    progressBar.Maximum = 600;
                    am.progress = progressBar;
                    progressBar.Visible = true;
                    if (await am.AMCmd(Cmd.ID) == ErrCode.OK)
                    {
                        List<string> cmds = new List<string>();
                        for (int i = 0; i < 500; i++)
                            cmds.Add("NOP#");
                        cmds.Add("test,3#");
                        cmds.Add("debug#");
                        if (await am.AMCmd(Cmd.GENERAL, request: cmds, reply: "test ... done err:") == ErrCode.OK)
                            await FormMain.notify("Test AM", string.Format("Test AM Done, errors: {0}", am.PrmGeneral), "OK");
                        else
                            await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
                    }
                    else
                        await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
                    progressBar.Value = progressBar.Maximum;
                    progressBar.Visible = false;
                    FormMain.Enables(this, true);
                }
            }
            else
                await FormMain.notify("AM not connected", "AM not found make sure the AM is connected\nto the tablet by using a USB cable", "OK");
        }

        private void InitForm_Load(object sender, EventArgs e)
        {

        }
    }
}
