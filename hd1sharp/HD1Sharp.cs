/*
 * 
 *  Copyright(c) 2018 Jean-Michel Cohen
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
*/

using System;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using CsvHelper;
using System.Xml;
using System.Collections.Generic;
using hd1sharp.Resources;

using System.Threading;
using SerialPortLib2;

namespace hd1sharp
{

    public partial class HD1Sharp : Form
    {
        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        private const int WM_SETREDRAW = 11;

        [DllImport("user32.dll")]
        public static extern bool ShowScrollBar(System.IntPtr hWnd, int wBar, bool bShow);
        private const uint SB_HORZ = 0;
        private const uint SB_VERT = 1;

        public static readonly String[] QtDqt =
        {
            "None", "62.5", "67.0", "69.3", "71.9", "74.4", "77.0", "79.7", "82.5", "85.4", "88.5",
            "91.5", "94.8", "97.4", "100.0", "103.5", "107.2", "110.9", "114.8", "118.8", "123.0",
            "127.3", "131.8", "136.5", "141.3", "146.2", "151.4", "156.7", "159.8", "162.2", "165.5",
            "167.9", "171.3", "173.8", "179.9", "183.5", "186.2", "189.9", "192.8", "196.6", "199.5",
            "203.5", "206.5", "210.7", "218.1", "225.7", "229.1", "233.6", "241.8", "250.3", "254.1",
            "D023N", "D025N", "D026N", "D031N", "D032N", "D043N", "D047N", "D051N", "D054N", "D065N",
            "D071N", "D072N", "D073N", "D074N", "D114N", "D115N", "D116N", "D125N", "D131N", "D132N",
            "D134N", "D143N", "D152N", "D155N", "D156N", "D162N", "D165N", "D172N", "D174N", "D205N",
            "D223N", "D226N", "D243N", "D244N", "D245N", "D251N", "D261N", "D263N", "D265N", "D271N",
            "D306N", "D311N", "D315N", "D331N", "D343N", "D346N", "D351N", "D364N", "D365N", "D371N",
            "D411N", "D412N", "D413N", "D423N", "D431N", "D432N", "D445N", "D464N", "D465N", "D466N",
            "D503N", "D506N", "D516N", "D532N", "D546N", "D565N", "D606N", "D612N", "D624N", "D627N",
            "D631N", "D632N", "D654N", "D662N", "D664N", "D703N", "D712N", "D723N", "D731N", "D732N",
            "D734N", "D743N", "D754N"
        };

        public String defaultPort = "COM3";

        public ZoneManager zoneManager;
        public ContextMenu zonesPopupMenu, zonePopupMenu;
        public TreeNode zoneTreeNode;
        public TreeNode selectedNode;
        public long startTimer;

        // Declare an ArrayList to serve as the data store.
        public List<Member> members = new List<Member>();
        public List<Zone> zones = new List<Zone>();
        public List<Channel> channels = new List<Channel>();
        public List<Contact> contacts = new List<Contact>();

        // Declare a Channel object to store data for a row being edited.
        private Channel channelInEdit;
        private Contact contactInEdit;

        // Declare a variable to store the index of a row being edited. 
        // A value of -1 indicates that there is no row currently in edit. 
        private int channelRowInEdit = -1;
        private int contactRowInEdit = -1;

        // Declare a variable to indicate the commit scope. 
        // Set this value to false to use cell-level commit scope. 
        private bool rowScopeCommit = true;

        public int channelMaxRows = 3002;
        public int memberMaxRows = 3000;
        public int contactMaxRows = 100000;

        // new combo method
        delegate void SetComboBoxCellType(int iRowIndex, int iColumnIndex);
        bool bIsComboBox = false;

        // ResourceManager res_man;

        public static HD1Sharp Instance;

        public String strConfigFilePath = "";

        /*
         * https://chirp.danplanet.com/issues/5809
         */
        /*
        struct HD1COMMAND
        {
            Byte    sync;         // 0x68
            Byte    commandCode;  // 0x31
            Int16   unknown;      // 0x00 0x01
            Byte    percent;      // percent complete
            Byte    cfUnknown;    // 0xcd=read bitmap 0xcf=read memory
            Byte    length;       // max read length
            UInt16  address;      // block address
            Byte    terminator;   // 0x10
        };

        // MEM_FORMAT
        // #seekto 0x6F3000;
        struct MEM_FORMAT {
            Byte[132] unknown1;
            Byte[14] name;
            Byte[22] unknown2;
            lbcd rxfreq4;           // BCD coded
            lbcd txfreq4;           // BCD coded
            Byte[20] unknow3;
        }
        */

        public HD1Sharp()
        {
            InitializeComponent();

            Instance = this;

            /*
                        Console.WriteLine("You are speaking '{0}'", System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
                        res_man = new ResourceManager("HD1Sharp.lang_" + System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName, Assembly.GetExecutingAssembly());
                        String str = res_man.GetString("lbl_error");
            */
            createTreeView();
            createChannelGrid();
            createAddressBookGrid();
            createPriorityContactsGrid();
            initializeEncryption();

            // Double buffering can make DGV slow in remote desktop
            if (!System.Windows.Forms.SystemInformation.TerminalServerSession)
            {
                this.DoubleBuffered = true;
                foreach (Control control in this.Controls)
                {
                    control.EnableDoubleBuferring();
                }

                Type dgvType = channelGridView.GetType();
                PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(channelGridView, true, null);

                dgvType = addressbookGridView.GetType();
                pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi.SetValue(addressbookGridView, true, null);

                // tabContainer.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            }

            tabTitle.Text = tabContainer.SelectedTab.Name;
        }

        void Awake()
        {
            Instance = this;
        }

        public void setchannelGidViewRow(int rowIndex)
        {
            channelGridView.ClearSelection();
            channelGridView.Rows[rowIndex].Selected = true;
        }

        public void createTreeView()
        {
            MainTreeView.ItemHeight += 4;

            ImageList TreeImageList = new ImageList();
            TreeImageList.Images.Add(Properties.Resources.folder);
            TreeImageList.Images.Add(Properties.Resources.settings);
            TreeImageList.Images.Add(Properties.Resources.encryption);
            TreeImageList.Images.Add(Properties.Resources.information);
            MainTreeView.ImageList = TreeImageList;

            TreeNode treeNode;

            MainTreeView.Nodes.Add(treeNode = new TreeNode("HD1"));
            MainTreeView.Nodes[0].Expand();

            TreeNode tn0, tn1, tn2, tn3, tn4;
            TreeNode[] array = new TreeNode[] {
                tn0 = new TreeNode("Basic_Information"),
                tn1 = new TreeNode("Settings"),
                tn2 = new TreeNode("Key_Settings"),
                tn3 = new TreeNode("One_Key_Call"),
                tn4 = new TreeNode("Settings")
            };

            tn0.ImageIndex = 3;
            tn0.SelectedImageIndex = 3;
            tn1.ImageIndex = 1;
            tn1.SelectedImageIndex = 1;
            tn2.ImageIndex = 1;
            tn2.SelectedImageIndex = 1;
            tn3.ImageIndex = 1;
            tn3.SelectedImageIndex = 1;
            tn4.ImageIndex = 1;
            tn4.SelectedImageIndex = 1;

            treeNode.Nodes.Add(new TreeNode("Basic Settings", array));

            treeNode.Nodes.Add(new TreeNode("Channel"));
            zoneTreeNode = new TreeNode("Zone Information");
            treeNode.Nodes.Add(zoneTreeNode);

            zoneManager = new ZoneManager(Instance, MainTreeView, zoneTreeNode);

            zonesPopupMenu = new ContextMenu();
            MenuItem addZonesMenuItem = new MenuItem("Add");
            addZonesMenuItem.Click += addZoneMenuItem_Click;
            zonesPopupMenu.MenuItems.Add(addZonesMenuItem);

            zonePopupMenu = new ContextMenu();
            MenuItem deleteZoneMenuItem = new MenuItem("Delete");
            deleteZoneMenuItem.Click += deleteZoneMenuItem_Click;
            zonePopupMenu.MenuItems.Add(deleteZoneMenuItem);

            treeNode.Nodes.Add(new TreeNode("Radio"));

            array = new TreeNode[] {
                new TreeNode("Priority Contacts"),
                new TreeNode("Address Book Contacts")
            };

            treeNode.Nodes.Add(new TreeNode("Contacts", array));
            treeNode.Nodes.Add(new TreeNode("RX Groups List"));

            array = new TreeNode[] {
                tn1 = new TreeNode("Encryption")
            };

            tn1.ImageIndex = 2;
            tn1.SelectedImageIndex = 2;

            treeNode.Nodes.Add(tn1 = new TreeNode("DMR Service", array));

            Type treeType = MainTreeView.GetType();
            PropertyInfo pi = treeType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(MainTreeView, true, null);
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {            
            foreach (ToolStripItem item in fileToolStripMenuItem.DropDownItems)
            {
                if ((item.Text == "Save") || (item.Text == "Save As..."))
                    item.Enabled = strConfigFilePath != "";
            }                
        }

        private void addZoneMenuItem_Click(object sender, EventArgs e)
        {
            zoneManager.addZone();
        }

        private void changeZoneMenuItem(object sender, EventArgs e)
        {
            zoneManager.renameZone("");
        }

        private void deleteZoneMenuItem_Click(object sender, EventArgs e)
        {
            zoneManager.deleteZone(zoneTreeNode);
        }

        private void MainTreeView_MouseDown(object sender, MouseEventArgs e)
        {
            if ((selectedNode = MainTreeView.GetNodeAt(e.X, e.Y)) ==  null)
                return;

            if (e.Button == MouseButtons.Right) {
                selectedNode = null;
                return;
            }
            
            startTimer = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if ((selectedNode.Parent != null) && (selectedNode.Parent.Text != "Zone Information"))
                selectedNode = null;
        }
       
        private void MainTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label != null)
            {
                if (e.Label.Length > 0)
                {
                    if (e.Label.IndexOfAny(new char[] { '@', '.', ',', '!' }) == -1)
                    {
                        // Stop editing without canceling the label change.
                        e.Node.EndEdit(false);
                    }
                    else
                    {
                        /* Cancel the label edit action, inform the user, and 
                           place the node in edit mode again. */
                        e.CancelEdit = true;
                        MessageBox.Show("Invalid tree node label.\n" + 
                           "The invalid characters are: '@','.', ',', '!'",
                           "Node Label Edit");
                        e.Node.BeginEdit();
                    }
                }
                else
                {
                    /* Cancel the label edit action, inform the user, and 
                       place the node in edit mode again. */
                    e.CancelEdit = true;
                    MessageBox.Show("Invalid tree node label.\nThe label cannot be blank", "Node Label Edit");
                    e.Node.BeginEdit();
                }
            }
        }

        public void createChannelGrid()
        {
            /**
             * DataGridView
            */
            channelGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            channelGridView.RowHeadersVisible = false; // set it to false if not needed
            channelGridView.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            channelGridView.EnableHeadersVisualStyles = false;
            channelGridView.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#F0F0F0");
            channelGridView.Columns["ChNumber"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            channelGridView.Columns["ChNumber"].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#F0F0F0");
            channelGridView.AllowUserToAddRows = false;
            channelGridView.AllowUserToDeleteRows = false;
            channelGridView.AllowUserToOrderColumns = false;
            channelGridView.AllowUserToResizeColumns = false;
            channelGridView.AllowUserToResizeRows = false;

            // Connect the virtual-mode events to event handlers. 
            channelGridView.VirtualMode = true;
            channelGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(channelGridView_CellValueNeeded);
            channelGridView.CellValuePushed += new DataGridViewCellValueEventHandler(channelGridView_CellValuePushed);
            channelGridView.NewRowNeeded += new DataGridViewRowEventHandler(channelGridView_NewRowNeeded);
            channelGridView.RowValidated += new DataGridViewCellEventHandler(channelGridView_RowValidated);
            channelGridView.RowDirtyStateNeeded += new QuestionEventHandler(channelGridView_RowDirtyStateNeeded);
            channelGridView.CancelRowEdit += new QuestionEventHandler(channelGridView_CancelRowEdit);

            channels.Add(new Channel("VFO-A", "", "", "", "", "", "", "", "", "",
                                    "", "", "", "", "", "", "", "", "", "", 
                                    "", "", "", "", "", "", "", "", "", "", 
                                    new List<string>(), "", "", "", "", "", ""));
            channels.Add(new Channel("VFO-B", "", "", "", "", "", "", "", "", "",
                                    "", "", "", "", "", "", "", "", "", "",
                                    "", "", "", "", "", "", "", "", "", "",
                                    new List<string>(), "", "", "", "", "", ""));

            for (int i = 2; i < channelMaxRows+1; i++)
            {
                channels.Add(new Channel("" + (i-1), "", "", "", "", "", "", "", "", "",
                                    "", "", "", "", "", "", "", "", "", "",
                                    "", "", "", "", "", "", "", "", "", "",
                                    new List<string>(), "", "", "", "", "", ""));
            }

            channelGridView.RowCount = channelMaxRows;

            /**
             * DataGridView
            */
            availableGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            availableGridView.RowHeadersVisible = false; // set it to false if not needed
            availableGridView.EnableHeadersVisualStyles = false;
            availableGridView.ColumnHeadersDefaultCellStyle.BackColor = ColorTranslator.FromHtml("#F0F0F0");
            availableGridView.AllowUserToAddRows = false;
            availableGridView.AllowUserToDeleteRows = false;
            availableGridView.AllowUserToOrderColumns = false;
            availableGridView.AllowUserToResizeColumns = false;
            availableGridView.AllowUserToResizeRows = false;

            // Connect the virtual-mode events to event handlers. 
            availableGridView.VirtualMode = true;
            availableGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(availableGridView_CellValueNeeded);

            for (int i = 0; i < memberMaxRows; i++)
            {
                members.Add(new Member(i+1, 0, "", ""));
            }

            availableGridView.RowCount = memberMaxRows;
        }

        public void createAddressBookGrid()
        {
            /**
             * DataGridView
            */
            addressbookGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            addressbookGridView.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            addressbookGridView.EnableHeadersVisualStyles = false;

            // Connect the virtual-mode events to event handlers. 
            addressbookGridView.VirtualMode = true;
            addressbookGridView.CellValueNeeded += new DataGridViewCellValueEventHandler(addressbookGridView_CellValueNeeded);
            addressbookGridView.CellValuePushed += new DataGridViewCellValueEventHandler(addressbookGridView_CellValuePushed);
            addressbookGridView.NewRowNeeded += new DataGridViewRowEventHandler(addressbookGridView_NewRowNeeded);
            addressbookGridView.RowValidated += new DataGridViewCellEventHandler(addressbookGridView_RowValidated);
            addressbookGridView.RowDirtyStateNeeded += new QuestionEventHandler(addressbookGridView_RowDirtyStateNeeded);
            addressbookGridView.CancelRowEdit += new QuestionEventHandler(addressbookGridView_CancelRowEdit);
            addressbookGridView.UserDeletingRow += new DataGridViewRowCancelEventHandler(addressbookGridView_UserDeletingRow);
        }

        private void createPriorityContactsGrid()
        {
            priorityContactsGrid.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            priorityContactsGrid.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            priorityContactsGrid.EnableHeadersVisualStyles = false;            
        }

        private void addPriorityContact_Click(object sender, EventArgs e)
        {
            priorityContactsGrid.AllowUserToAddRows = true;
            DataGridViewRow row = (DataGridViewRow)priorityContactsGrid.Rows[0].Clone();
            if (priorityContactsGrid.Rows.Count == 1)
                priorityContactsGrid.Rows.Add(row);
            else
            {
                int index = priorityContactsGrid.CurrentCell.RowIndex + 1;
                priorityContactsGrid.Rows.Insert(index, row);
            }

            priorityContactsGrid.AllowUserToAddRows = false;

            // priorityContactsGrid.Rows[priorityContactsGrid.Rows.Count - 1].Selected = true;

            // if (priorityContactsGrid.Rows.Count >= 20)
            //    priorityContactsGrid.RowCount = 20;
        }

        private void deletePriorityContact_Click(object sender, EventArgs e)
        {
            if (priorityContactsGrid.SelectedRows.Count > 1)
            {
                foreach (DataGridViewRow row in priorityContactsGrid.SelectedRows)
                {
                    priorityContactsGrid.Rows.RemoveAt(row.Index);
                }
            }
            else
            {
                if (priorityContactsGrid.Rows.Count > 0)
                {
                    int index = priorityContactsGrid.CurrentCell.RowIndex;
                    priorityContactsGrid.Rows.RemoveAt(index);
                }
            }

            // if (priorityContactsGrid.Rows.Count < 20)
            //    priorityContactsGrid.RowCount = priorityContactsGrid.Rows.Count;
        }

        private void idRadioGridView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            DataGridViewCell cell = dgv[dgv.Columns[e.ColumnIndex].Name, e.RowIndex];
            DataGridViewTextBoxCell textBoxCell = dgv.CurrentCell as DataGridViewTextBoxCell;

            if (textBoxCell != null)
            {
                dgv.BeginEdit(false);
            }
        }

        void sizeDGV(DataGridView dgv)
        {
            DataGridViewElementStates states = DataGridViewElementStates.None;
            var totalHeight = dgv.Rows.GetRowsHeight(states) + dgv.ColumnHeadersHeight;
            totalHeight += dgv.Rows.Count * 4;  // a correction I need
            var totalWidth = dgv.Columns.GetColumnsWidth(states) + dgv.RowHeadersWidth;
            dgv.ClientSize = new Size(totalWidth, totalHeight);
        }

        private void WriteTag(XmlWriter xmlWriter, String tag, String value)
        {
            xmlWriter.WriteStartElement(tag);
            xmlWriter.WriteString(value);
            xmlWriter.WriteEndElement();
        }

        public void writeXmlFile(String strFile)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.NewLineOnAttributes = false;
            xmlWriterSettings.Indent = true;
            XmlWriter xmlWriter = XmlWriter.Create(strFile, xmlWriterSettings);

            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement("Configuration");

            xmlWriter.WriteStartElement("BasicInformation");
            WriteTag(xmlWriter, "SerialNo", BasicInformation.Controls.Find("SerialNo", true)[0].Text);
            WriteTag(xmlWriter, "VersionDate", BasicInformation.Controls.Find("VersionDate", true)[0].Text);
            WriteTag(xmlWriter, "FirmwareVersion", BasicInformation.Controls.Find("FirmwareVersion", true)[0].Text);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Settings");
            WriteTag(xmlWriter, "WritingPasswordSwitch", ((CheckBox)Settings.Controls.Find("WritingPasswordSwitch", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "WritingPassword", Settings.Controls.Find("WritingPassword", true)[0].Text);
            WriteTag(xmlWriter, "ReadingPasswordSwitch", ((CheckBox)Settings.Controls.Find("ReadingPasswordSwitch", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "ReadingPassword", Settings.Controls.Find("ReadingPassword", true)[0].Text);
            WriteTag(xmlWriter, "LanguageSettings", Settings.Controls.Find("LanguageSettings", true)[0].Text);
            WriteTag(xmlWriter, "SquelchLevel", Settings.Controls.Find("SquelchLevel", true)[0].Text);
            WriteTag(xmlWriter, "RelayRetransmission", Settings.Controls.Find("RelayRetransmission", true)[0].Text);
            WriteTag(xmlWriter, "BatterySaveStartupTime", Settings.Controls.Find("BatterySaveStartupTime", true)[0].Text);
            WriteTag(xmlWriter, "BatterySaveMode", Settings.Controls.Find("BatterySaveMode", true)[0].Text);
            WriteTag(xmlWriter, "PowerSavingDecodingRate", Settings.Controls.Find("PowerSavingDecodingRate", true)[0].Text);
            WriteTag(xmlWriter, "VoiceAnnunciation", Settings.Controls.Find("VoiceAnnunciation", true)[0].Text);
            WriteTag(xmlWriter, "DigitalHangUpResidence", Settings.Controls.Find("DigitalHangUpResidence", true)[0].Text);
            WriteTag(xmlWriter, "IndividualWorkResponseTime", Settings.Controls.Find("IndividualWorkResponseTime", true)[0].Text);
            WriteTag(xmlWriter, "IndividualWorkReminderTime", Settings.Controls.Find("IndividualWorkReminderTime", true)[0].Text);           
            WriteTag(xmlWriter, "ZoneSelectionA", Settings.Controls.Find("ZoneSelectionA", true)[0].Text);
            WriteTag(xmlWriter, "ZoneSelectionB", Settings.Controls.Find("ZoneSelectionB", true)[0].Text);
            WriteTag(xmlWriter, "TailEliminateMode", Settings.Controls.Find("TailEliminateMode", true)[0].Text);
            WriteTag(xmlWriter, "TailEliminate", ((CheckBox)Settings.Controls.Find("TailEliminate", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "KeyLockSwitch", ((CheckBox)Settings.Controls.Find("KeyLockSwitch", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "AutoKeyLock", ((CheckBox)Settings.Controls.Find("AutoKeyLock", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "Tones", ((CheckBox)Settings.Controls.Find("Tones", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "Roger", ((CheckBox)Settings.Controls.Find("Roger", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "Bdr", ((CheckBox)Settings.Controls.Find("Bdr", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "RadioKill", ((CheckBox)Settings.Controls.Find("RadioKill", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "RadioWakeUp", ((CheckBox)Settings.Controls.Find("RadioWakeUp", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "CallerDisplayTime", Settings.Controls.Find("CallerDisplayTime", true)[0].Text);
            WriteTag(xmlWriter, "WorkingFrequency", Settings.Controls.Find("WorkingFrequency", true)[0].Text);
            WriteTag(xmlWriter, "BackLightTime", Settings.Controls.Find("BackLightTime", true)[0].Text);
            WriteTag(xmlWriter, "PowerOnPasswordSwitch", ((CheckBox)Settings.Controls.Find("PowerOnPasswordSwitch", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "PowerOnPassword", Settings.Controls.Find("PowerOnPassword", true)[0].Text);
            WriteTag(xmlWriter, "VOXDelayTime", Settings.Controls.Find("VOXDelayTime", true)[0].Text);
            WriteTag(xmlWriter, "PressPTTCancelVOX", ((CheckBox)Settings.Controls.Find("PressPTTCancelVOX", true)[0]).Checked.ToString().ToLower());
            WriteTag(xmlWriter, "InsertHeadsetMakeVOX", ((CheckBox)Settings.Controls.Find("InsertHeadsetMakeVOX", true)[0]).Checked.ToString().ToLower());
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("KeySettings");
            WriteTag(xmlWriter, "SideKeyLongPress1", SideKeyLongPress1.Text);
            WriteTag(xmlWriter, "SideKeyShortPress1", SideKeyShortPress1.Text);
            WriteTag(xmlWriter, "SideKeyLongPress2", SideKeyLongPress2.Text);
            WriteTag(xmlWriter, "SideKeyShortPress2", SideKeyShortPress2.Text);
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("IDSettings");
            for (int i = 0; i < idRadioGridView.RowCount; i++) {
                if (idRadioGridView.Rows[i].Cells["radioId"].FormattedValue.ToString() == "")
                    break;
                xmlWriter.WriteStartElement("Radio");
                xmlWriter.WriteAttributeString("radioId", idRadioGridView.Rows[i].Cells["radioId"].FormattedValue.ToString());
                xmlWriter.WriteAttributeString("radioName", idRadioGridView.Rows[i].Cells["radioName"].FormattedValue.ToString());
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("RadioChannels");
            WriteTag(xmlWriter, "RadioDW", RadioDW.Text);
            WriteTag(xmlWriter, "WorkMode", WorkMode.Text);
            WriteTag(xmlWriter, "WorkChannel", WorkChannel.Text);
            for (int i = 0; i < radioChannelGrid.RowCount; i++)
            {
                if (radioChannelGrid.Rows[i].Cells["frequency"].FormattedValue.ToString() == "")
                    break;
                xmlWriter.WriteStartElement("Channel");
                xmlWriter.WriteAttributeString("frequency", radioChannelGrid.Rows[i].Cells["frequency"].FormattedValue.ToString());
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("PriorityContacts");
            for (int i=0; i<priorityContactsGrid.RowCount-1; i++)
            {
                xmlWriter.WriteStartElement("Contact");
                WriteTag(xmlWriter, "CallType", priorityContactsGrid.Rows[i].Cells["CallType"].FormattedValue.ToString());
                WriteTag(xmlWriter, "ContactAlias", priorityContactsGrid.Rows[i].Cells["ContactAlias"].FormattedValue.ToString());
                WriteTag(xmlWriter, "City", priorityContactsGrid.Rows[i].Cells["City"].FormattedValue.ToString());
                WriteTag(xmlWriter, "Province", priorityContactsGrid.Rows[i].Cells["Province"].FormattedValue.ToString());
                WriteTag(xmlWriter, "Country", priorityContactsGrid.Rows[i].Cells["Country"].FormattedValue.ToString());
                WriteTag(xmlWriter, "CallID", priorityContactsGrid.Rows[i].Cells["CallID"].FormattedValue.ToString());
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("Channels");
            for (int i = 0; i < channels.Count - 1; i++)
            {
                if (channels[i].ChannelAlias == "")
                    break;

                xmlWriter.WriteStartElement("Channel");
                WriteTag(xmlWriter, "Number", channels[i].ChannelNumber);
                WriteTag(xmlWriter, "Type", channels[i].ChannelType);
                WriteTag(xmlWriter, "Alias", channels[i].ChannelAlias);
                WriteTag(xmlWriter, "RXFrequency", channels[i].RxFrequency);
                WriteTag(xmlWriter, "TXFrequency", channels[i].TxFrequency);
                WriteTag(xmlWriter, "TXPower", channels[i].TxPower);
                WriteTag(xmlWriter, "Tot", channels[i].Tot);
                WriteTag(xmlWriter, "Vox", channels[i].Vox);
                WriteTag(xmlWriter, "VoxLevel", channels[i].VoxLevel);
                WriteTag(xmlWriter, "ScanAdd", channels[i].ScanAdd);
                WriteTag(xmlWriter, "WorkAlone", channels[i].ChannelWorkAlone);
                WriteTag(xmlWriter, "TalkAround", channels[i].DefaultTalkAround);
                WriteTag(xmlWriter, "Bandwidth", channels[i].Bandwidth);
                WriteTag(xmlWriter, "DecQtDqt", channels[i].DecQtDqt);
                WriteTag(xmlWriter, "EncQtDqt", channels[i].EncQtDqt);
                WriteTag(xmlWriter, "TxAuthorityA", channels[i].TxAuthorityA);
                WriteTag(xmlWriter, "Relay", channels[i].Relay);
                WriteTag(xmlWriter, "WorkMode", channels[i].WorkMode);
                WriteTag(xmlWriter, "Slot", channels[i].Slot);
                WriteTag(xmlWriter, "IdSettings", channels[i].IdSetting);
                WriteTag(xmlWriter, "ColorCode", channels[i].ColorCode);
                WriteTag(xmlWriter, "Encryption", channels[i].Encryption);
                WriteTag(xmlWriter, "EncryptionType", channels[i].EncryptionType);
                WriteTag(xmlWriter, "EncryptionKey", channels[i].EncryptionKey);
                WriteTag(xmlWriter, "Promiscuous", channels[i].Promiscuous);
                WriteTag(xmlWriter, "TxAuthorityD", channels[i].TxAuthorityD);
                WriteTag(xmlWriter, "KillCode", channels[i].KillCode);
                WriteTag(xmlWriter, "Contacts", channels[i].Contacts);
                WriteTag(xmlWriter, "RxGroupLists", channels[i].RxGroupsList);

                List<String> selectedContacts = this.channels[i].Selected;                
                for (int j=0; j<33; j++)
                {
                    WriteTag(xmlWriter, "GroupLists"+(1+j).ToString("D2"), selectedContacts[j]);
                }
                WriteTag(xmlWriter, "Gps", channels[i].Gps);
                WriteTag(xmlWriter, "SendGpsInfo", channels[i].SendGpsInfo);
                WriteTag(xmlWriter, "ReceiveGpsInfo", channels[i].ReceiveGpsInfo);
                WriteTag(xmlWriter, "GpsTimingReport", channels[i].GpsTimingReport);
                WriteTag(xmlWriter, "GpsTimingReportTxContacts", channels[i].GpsTimingReportTxContacts);

                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();


            xmlWriter.WriteEndDocument();
            xmlWriter.Close();
        }

        private void readConfiguration(String strFile)
        {
            XmlTextReader reader = new XmlTextReader(strFile);
            String name = "";
            int rowIndex = 0;
            
            Boolean readingRadio = false;
            Boolean readingChannels = false;
            Boolean readingOneChannel = false;
            Boolean readingIDSettings = false;
            Boolean readingSettings = false;
            Boolean readingKeySettings = false;
            Boolean readingPriorityContacts = false;
            Boolean readingBasicInformation = false;
            // Boolean readingZones = false;
            List<String> selectedContacts = null;

            idRadioGridView.Rows.Clear();
            priorityContactsGrid.Rows.Clear();

            radioChannelGrid.RowCount = 32;
            idRadioGridView.RowCount = 32;
            oneKeyCallGrid.RowCount = 6;
            oneKeyCallMessageGrid.RowCount = 50;

            for (int i=0; i< oneKeyCallGrid.RowCount; i++)
            {
                oneKeyCallGrid.Rows[i].Cells["okcNo"].Value = i+1;
                oneKeyCallGrid.Rows[i].Cells["okcCallMode"].Value = "None";
                oneKeyCallGrid.Rows[i].Cells["okcCallList"].Value = "None";
                enableCell(oneKeyCallGrid[oneKeyCallGrid.Columns["okcPriority"].Index, i], false);
                enableCell(oneKeyCallGrid[oneKeyCallGrid.Columns["okcAddressBook"].Index, i], false);
                oneKeyCallGrid.Rows[i].Cells["okcCallType"].Value = "None";
                oneKeyCallGrid.Rows[i].Cells["okcQuickMessage"].Value = "None";
            }

            for (int i = 0; i < oneKeyCallMessageGrid.RowCount; i++)
            {
                oneKeyCallMessageGrid.Rows[i].Cells["okcmNo"].Value = i + 1;
            }

            for (int i = 0; i < radioChannelGrid.RowCount; i++)
            {
                radioChannelGrid.Rows[i].Cells["radNo"].Value = i+1;
            }

            rowIndex = 0;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        name = reader.Name;

                        if (readingChannels)
                        {
                            if (reader.Name == "Channel")
                            {
                                selectedContacts = new List<string>();
                                readingOneChannel = true;
                            }
                            break;
                        }

                        if (reader.Name == "Channels")
                        {
                            rowIndex = 0;
                            readingChannels = true;
                            break;
                        }

                        if (readingRadio)
                        {
                            if (reader.Name == "Channel")
                            {
                                radioChannelGrid.Rows[rowIndex].Cells["radNo"].Value = rowIndex + 1;

                                // Read the attributes.
                                while (reader.MoveToNextAttribute())
                                {
                                    radioChannelGrid.Rows[rowIndex].Cells[reader.Name].Value = reader.Value;
                                }

                                rowIndex++;
                            }
                            break;
                        }

                            // Console.Write("<" + reader.Name);
                        if (readingIDSettings)
                        {
                            if (reader.Name == "Radio")
                            {
                                idRadioGridView.Rows[rowIndex].Cells["idsNo"].Value = rowIndex + 1;

                                // Read the attributes.
                                while (reader.MoveToNextAttribute())
                                {
                                    idRadioGridView.Rows[rowIndex].Cells[reader.Name].Value = reader.Value;
                                }

                                rowIndex++;
                            }
                            break;
                        }

                        if (readingPriorityContacts)
                        {
                            // for each contact add numbered row 
                            if (reader.Name == "Contact")
                            {
                                rowIndex = priorityContactsGrid.Rows.Add();
                                priorityContactsGrid.Rows[rowIndex].Cells["No"].Value = rowIndex + 1;
                            }
                            break;
                        }

                        if (reader.Name == "RadioChannels")
                        {
                            rowIndex = 0;
                            readingRadio = true;
                            break;
                        }

                        if (reader.Name == "PriorityContacts")
                        {
                            rowIndex = 0;
                            readingPriorityContacts = true;
                            break;
                        }

                        if (reader.Name == "Settings") {
                            rowIndex = 0;
                            readingSettings = true;
                            break;
                        }

                        if (reader.Name == "KeySettings")
                        {
                            rowIndex = 0;
                            readingKeySettings = true;
                            break;
                        }

                        if (reader.Name == "IDSettings")
                        {
                            rowIndex = 0;
                            readingIDSettings = true;
                            break;
                        }

                        if (reader.Name == "BasicInformation")
                        {
                            rowIndex = 0;
                            readingBasicInformation = true;
                            break;
                        }
                        
                        // Console.WriteLine(">");
                        break;

                    case XmlNodeType.Text: //Display the text in each element.
                        // Console.WriteLine(reader.Value.Trim());
                        if (readingOneChannel)
                        {
                            switch(name)
                            {
                                case "Number": channels[rowIndex].ChannelNumber = reader.Value; break;
                                case "Type": channels[rowIndex].ChannelType = reader.Value; break;
                                case "Alias": channels[rowIndex].ChannelAlias = reader.Value; break;
                                case "RXFrequency": channels[rowIndex].RxFrequency = reader.Value; break;
                                case "TXFrequency": channels[rowIndex].TxFrequency = reader.Value; break;
                                case "TXPower": channels[rowIndex].TxPower = reader.Value; break;
                                case "Tot": channels[rowIndex].Tot = reader.Value; break;
                                case "Vox": channels[rowIndex].Vox = reader.Value; break;
                                case "VoxLevel": channels[rowIndex].VoxLevel = reader.Value; break;
                                case "ScanAdd": channels[rowIndex].ScanAdd = reader.Value; break;
                                case "WorkAlone": channels[rowIndex].ChannelWorkAlone = reader.Value; break;
                                case "TalkAround": channels[rowIndex].DefaultTalkAround = reader.Value; break;
                                case "Bandwidth": channels[rowIndex].Bandwidth = reader.Value; break;
                                case "DecQtDqt": channels[rowIndex].DecQtDqt = reader.Value; break;
                                case "EncQtDqt": channels[rowIndex].EncQtDqt = reader.Value; break;
                                case "TxAuthorityA": channels[rowIndex].TxAuthorityA = reader.Value; break;
                                case "Relay": channels[rowIndex].Relay = reader.Value; break;
                                case "WorkMode": channels[rowIndex].WorkMode = reader.Value; break;
                                case "Slot": channels[rowIndex].Slot = reader.Value; break;
                                case "IdSettings": channels[rowIndex].IdSetting = reader.Value; break;
                                case "ColorCode": channels[rowIndex].ColorCode = reader.Value; break;
                                case "Encryption": channels[rowIndex].Encryption = reader.Value; break;
                                case "EncryptionType": channels[rowIndex].EncryptionType = reader.Value; break;
                                case "EncryptionKey": channels[rowIndex].EncryptionKey = reader.Value; break;
                                case "Promiscuous": channels[rowIndex].Promiscuous = reader.Value; break;
                                case "TxAuthorityD": channels[rowIndex].TxAuthorityD = reader.Value; break;
                                case "KillCode": channels[rowIndex].KillCode = reader.Value; break;
                                case "Contacts": channels[rowIndex].Contacts = reader.Value; break;
                                case "RxGroupLists":  channels[rowIndex].RxGroupsList = reader.Value; break;
                                case "Gps": channels[rowIndex].Gps = reader.Value; break;
                                case "SendGpsInfo": channels[rowIndex].SendGpsInfo = reader.Value; break;
                                case "ReceiveGpsInfo": channels[rowIndex].ReceiveGpsInfo = reader.Value; break;
                                case "GpsTimingReport": channels[rowIndex].GpsTimingReport = reader.Value; break;
                                case "GpsTimingReportTxContacts": channels[rowIndex].GpsTimingReportTxContacts = reader.Value; break;

                                default:
                                    if (name.StartsWith("GroupLists"))
                                    {
                                        selectedContacts.Add(reader.Value);
                                    }
                                    break;
                            }                            
                            break;
                        }


                        if (readingPriorityContacts)
                        {
                            priorityContactsGrid.Rows[rowIndex].Cells[name].Value = reader.Value == null ? "": reader.Value;
                            break;
                        }

                        if (readingBasicInformation)
                        {
                            switch (name)
                            {
                                // TextBoxes
                                case "SerialNo":
                                case "VersionDate":
                                case "FirmwareVersion":
                                    ((TextBox)BasicInformation.Controls.Find(name, true)[0]).Text = reader.Value;
                                    break;

                                default:
                                    break;
                            }
                        }

                        if (readingKeySettings)
                        {
                            switch (name)
                            {
                                // Comboboxes
                                case "SideKeyLongPress1":
                                case "SideKeyShortPress1":
                                case "SideKeyLongPress2":
                                case "SideKeyShortPress2":
                                    ((ComboBox)KeySettings.Controls.Find(name, true)[0]).Text = reader.Value;
                                    break;

                                default:
                                    break;
                            }
                            break;
                        }

                        if (readingRadio)
                        {
                            switch (name)
                            {
                                // ComboBoxes
                                case "RadioDW":
                                case "WorkMode":
                                case "WorkChannel":
                                    ((ComboBox)Radio.Controls.Find(name, true)[0]).Text = reader.Value;
                                    break;

                                default:
                                    break;
                            }
                        }

                        if (readingSettings)
                        {
                            switch (name)
                            {
                                // checkboxes
                                case "WritingPasswordSwitch":
                                case "ReadingPasswordSwitch":
                                case "TailEliminate":
                                case "KeyLockSwitch":
                                case "AutoKeyLock":
                                case "Tones":
                                case "Roger":
                                case "Bdr":
                                case "RadioKill":
                                case "RadioWakeUp":
                                case "PowerOnPasswordSwitch":
                                case "PressPTTCancelVOX":
                                case "InsertHeadsetMakeVOX":
                                    ((CheckBox)Settings.Controls.Find(name, true)[0]).Checked = reader.Value == "true";
                                    break;

                                // TextBoxes
                                case "WritingPassword":
                                case "ReadingPassword":
                                case "PowerOnPassword":
                                    ((TextBox)Settings.Controls.Find(name, true)[0]).Text = reader.Value;
                                    break;
                                
                                // ComboBoxes
                                case "LanguageSettings":
                                case "RelayRetransmission":
                                case "BatterySaveStartupTime":
                                case "SquelchLevel":
                                case "BatterySaveMode":
                                case "PowerSavingDecodingRate":
                                case "VoiceAnnunciation":
                                case "DigitalHangUpResidence":
                                case "IndividualWorkResponseTime":
                                case "IndividualWorkReminderTime":
                                case "ZoneSelectionA":
                                case "ZoneSelectionB":
                                case "TailEliminateMode":
                                case "WorkingFrequency":
                                case "BackLightTime":
                                case "VOXDelayTime":
                                case "CallerDisplayTime":
                                    ((ComboBox)Settings.Controls.Find(name, true)[0]).Text = reader.Value;
                                    break;

                                default:
                                    break;
                            }
                            break;
                        }
                        break;

                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name == "Channel")
                        {
                            channels[rowIndex].Selected = selectedContacts;
                            readingOneChannel = false;
                            rowIndex++;
                            break;
                        }

                        if (reader.Name == "Channels")
                        {
                            readingChannels = false;
                            readingOneChannel = false;

                            // reset the remaining channels
                            for (int i = rowIndex; i < channelMaxRows + 1; i++)
                            {
                                channels[i] = new Channel("" + (i - 1), "", "", "", "", "", "", "", "", "",
                                                    "", "", "", "", "", "", "", "", "", "",
                                                    "", "", "", "", "", "", "", "", "", "",
                                                    new List<string>(), "", "", "", "", "", "");
                            }

                            channelGridView.RowCount = channelMaxRows;
                            break;
                        }

                        if (reader.Name == "PriorityContacts")
                        {
                            readingPriorityContacts = false;
                            break;
                        }

                        if (reader.Name == "Settings")
                        {
                            readingSettings = false;
                            break;
                        }

                        if (reader.Name == "KeySettings")
                        {
                            readingKeySettings = false;
                            break;
                        }

                        if (reader.Name == "RadioChannels")
                        {
                            readingRadio = false;
                            break;
                        }

                        if (reader.Name == "BasicInformation")
                        {
                            readingBasicInformation = false;
                            break;
                        }

                        if (reader.Name == "IDSettings")
                        {
                            for (int i = rowIndex; i < idRadioGridView.RowCount; i++)
                            {
                                idRadioGridView.Rows[i].Cells["idsNo"].Value = i + 1;
                            }
                            readingIDSettings = false;
                            break;
                        }
                        // Console.WriteLine("</" + reader.Name + ">");
                        break;
                }
            }

            sizeDGV(oneKeyCallGrid);
            sizeDGV(oneKeyCallMessageGrid);
        }

        private void tabContainer_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tbc = (TabControl)sender;
            tabTitle.Text = tbc.SelectedTab.Name;
        }

        private void MainTreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if ((DateTimeOffset.Now.ToUnixTimeMilliseconds() - startTimer) > 500)
            {
                if (selectedNode != null && selectedNode.Parent != null)
                {
                    MainTreeView.SelectedNode = selectedNode;
                    MainTreeView.LabelEdit = true;
                    if (!selectedNode.IsEditing)
                    {
                        selectedNode.BeginEdit();
                    }
                }                
            }
        }

        private void MainTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null)
                return;

            switch (e.Node.Text)
            {
                case "Priority Contacts":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("PriorityContacts");
                    break;

                case "Basic Information":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("BasicInformation");
                    break;

                case "Channel":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("Channel");
                    break;

                case "Address Book Contacts":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("AddressBookContacts");
                    break;

                case "Encryption":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("Encryption");
                    break;

                case "Settings":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("Settings");
                    break;

                case "Key Settings":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("KeySettings");
                    break;

                case "One Key Call":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("OneKeyCall");
                    break;

                case "ID Settings":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("IDSettings");
                    break;

                case "Radio":
                    tabTitle.Text = e.Node.Text;
                    tabContainer.SelectTab("Radio");
                    break;

                case "Zone Information":
                    // Point where the mouse is clicked.
                    if (e.Button == MouseButtons.Right)
                        zonesPopupMenu.Show(MainTreeView, new Point(e.X, e.Y));
                    break;

                default:
                    zoneTreeNode = MainTreeView.GetNodeAt(e.X, e.Y);

                    if (zoneTreeNode.Parent.Text == "Zone Information")
                    {
                        if (e.Button == MouseButtons.Right)
                        {
                            zonePopupMenu.Show(MainTreeView, new Point(e.X, e.Y));
                        }
                        else
                        {
                            ZoneInformation.Controls.Find("ZoneAlias", true)[0].Text = zoneTreeNode.Text;
                            zoneManager.selectZone(zoneTreeNode);
                            tabTitle.Text = "Zone Information";
                            tabContainer.SelectTab("ZoneInformation");
                        }
                    }
                    break;
            }
        }

        // https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/implementing-virtual-mode-wf-datagridview-control

 
        private void availableGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            // If this is the row for new records, no values are needed.
            if (e.RowIndex == ((DataGridView)sender).RowCount - 1)
                return;

            Member memberTmp = null;

            if (e.RowIndex > memberMaxRows)
                return;

            memberTmp = (Member)members[e.RowIndex];

            // Set the cell value to paint using the Channel object retrieved.
            switch (availableGridView.Columns[e.ColumnIndex].Name)
            {
                case "Available":                    
                    e.Value = memberTmp.MemberAlias;
                    break;
            }
        }

        private void enableCell(DataGridViewCell cell, Boolean state)
        {
            if (!state)            
            {
                cell.Style.BackColor = Color.Gray;
                cell.Style.ForeColor = Color.Gray;
                cell.ReadOnly = true;
            }
        }

        private void channelGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            // If this is the row for new records, no values are needed.
            if (e.RowIndex == ((DataGridView)sender).RowCount - 1)
                return;

            Channel channelTmp = null;

            // Store a reference to the Channel object for the row being painted.
            if (e.RowIndex == channelRowInEdit)
            {
                channelTmp = channelInEdit;
            }
            else
            {
                if (e.RowIndex > channelMaxRows)
                    return;

                channelTmp = (Channel)channels[e.RowIndex];
            }

            // Set the cell value to paint using the Channel object retrieved.
            switch (channelGridView.Columns[e.ColumnIndex].Name)
            {
                case "ChNumber":
                    e.Value = channelTmp.ChannelNumber;
                    break;

                case "RXFrequency":
                    e.Value = channelTmp.RxFrequency;
                    break;

                case "TXFrequency":
                    e.Value = channelTmp.TxFrequency;
                    break;

                case "ChannelType":
                    e.Value = channelTmp.ChannelType;
                    break;

                case "DecQTDQT":
                    e.Value = channelTmp.DecQtDqt;
                    break;

                case "EncQTDQT":
                    e.Value = channelTmp.EncQtDqt;
                    break;

                case "TXPower":
                    e.Value = channelTmp.TxPower;
                    break;

                case "ScanAdd":
                    e.Value = channelTmp.ScanAdd;
                    enableCell(channelGridView[e.ColumnIndex, e.RowIndex], !channelTmp.ChannelNumber.StartsWith("VFO"));
                    break;

                case "Bandwidth":
                    e.Value = channelTmp.Bandwidth;
                    break;

                case "ChannelAlias":
                    e.Value = channelTmp.ChannelAlias;
                    break;

                case "More":
                    e.Value = channelTmp.More;
                    break;
            }
        }

        private void channelGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            Channel channelTmp = null;

            // Store a reference to the Channel object for the row being edited.
            if (e.RowIndex < this.channels.Count)
            {
                // If the user is editing a new row, create a new Channel object.
                if (this.channelInEdit == null)
                {
                    Channel currentChannel = ((Channel)this.channels[e.RowIndex]);

                    this.channelInEdit = new Channel(
                        currentChannel.ChannelNumber, 
                        currentChannel.ChannelType, 
                        currentChannel.ChannelAlias, 
                        currentChannel.RxFrequency,
                        currentChannel.TxFrequency, 
                        currentChannel.TxPower, 
                        currentChannel.Tot,
                        currentChannel.Vox,
                        currentChannel.VoxLevel,
                        currentChannel.ScanAdd,
                        currentChannel.ChannelWorkAlone,
                        currentChannel.DefaultTalkAround,
                        currentChannel.Bandwidth,
                        currentChannel.DecQtDqt,
                        currentChannel.EncQtDqt,
                        currentChannel.TxAuthorityA,
                        currentChannel.Relay,
                        currentChannel.WorkMode,
                        currentChannel.Slot,
                        currentChannel.IdSetting,
                        currentChannel.ColorCode,
                        currentChannel.Encryption,
                        currentChannel.EncryptionType,
                        currentChannel.EncryptionKey,
                        currentChannel.Promiscuous,
                        currentChannel.TxAuthorityD,
                        currentChannel.KillCode,
                        currentChannel.WakeUpCode,
                        currentChannel.Contacts,
                        currentChannel.RxGroupsList,
                        currentChannel.Selected,
                        currentChannel.Gps,
                        currentChannel.SendGpsInfo,
                        currentChannel.ReceiveGpsInfo,
                        currentChannel.GpsTimingReport,
                        currentChannel.GpsTimingReportTxContacts,
                        currentChannel.Hidden
                    );                    
                }
                channelTmp = this.channelInEdit;
                this.channelRowInEdit = e.RowIndex;
            }
            else
            {
                channelTmp = this.channelInEdit;
            }

            // Set the appropriate Channel property to the cell value entered.
            String newValue = e.Value as String;
            switch (((DataGridView)sender).Columns[e.ColumnIndex].Name)
            {
                case "ChNumber":
                    channelTmp.ChannelNumber = newValue;
                    break;

                case "RXFrequency":
                    channelTmp.RxFrequency = newValue;
                    // replicate rx value to tx if empty
                    if ((channelTmp.RxFrequency != "") && (channelTmp.TxFrequency == ""))
                        channelTmp.TxFrequency = channelTmp.RxFrequency;
                    break;

                case "TXFrequency":
                    channelTmp.TxFrequency = newValue;
                    break;

                case "ChannelType":
                    channelTmp.ChannelType = newValue;
                    break;

                case "DecQTDQT":
                    channelTmp.DecQtDqt = newValue;
                    break;

                case "EncQTDQT":
                    channelTmp.EncQtDqt = newValue;
                    break;

                case "TXPower":
                    channelTmp.TxPower = newValue;
                    break;

                case "ScanAdd":
                    channelTmp.ScanAdd = newValue;
                    break;

                case "Bandwidth":
                    channelTmp.Bandwidth = newValue;
                    break;

                case "ChannelAlias":
                    channelTmp.ChannelAlias = newValue;
                    break;

                case "More":
                    // channelTmp.More = newValue;
                    break;

            }
        }

        private void channelGridView_RowDirtyStateNeeded(object sender, QuestionEventArgs e)
        {
            if (!rowScopeCommit)
            {
                // In cell-level commit scope, indicate whether the value
                // of the current cell has been modified.
                e.Response = ((DataGridView)sender).IsCurrentCellDirty;
            }
        }

        private void channelGridView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            // Create a new Channel object when the user edits
            // the row for new records.
            this.channelInEdit = new Channel();
            this.channelRowInEdit = ((DataGridView)sender).Rows.Count - 1;
        }

        private void channelGridView_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            // Save row changes if any were made and release the edited 
            // Channel object if there is one.
            if (e.RowIndex >= this.channels.Count && e.RowIndex != dgv.Rows.Count - 1)
            {
                // Add the new Channel object to the data store.
                this.channels.Add(this.channelInEdit);
                this.channelInEdit = null;
                this.channelRowInEdit = -1;
            }
            else if (this.channelInEdit != null && e.RowIndex < this.channels.Count)
            {
                // Save the modified Channel object in the data store.
                this.channels[e.RowIndex] = this.channelInEdit;
                this.channelInEdit = null;
                this.channelRowInEdit = -1;
            }
            else if (dgv.ContainsFocus)
            {
                this.channelInEdit = null;
                this.channelRowInEdit = -1;
            }
        }

        private void channelGridView_CancelRowEdit(object sender, QuestionEventArgs e)
        {
            if (this.channelRowInEdit == ((DataGridView)sender).Rows.Count - 2 && this.channelRowInEdit == this.channels.Count)
            {
                // If the user has canceled the edit of a newly created row, 
                // replace the corresponding Channel object with a new, empty one.
                this.channelInEdit = new Channel();
            }
            else
            {
                // If the user has canceled the edit of an existing row, 
                // release the corresponding Customer object.
                this.channelInEdit = null;
                this.channelRowInEdit = -1;
            }
        }

        private void channelGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // enters edit mode on click
            if ((e.ColumnIndex >= 1) && (e.RowIndex >= 0) /*&& (e.RowIndex < channelMaxRows-1)*/)
            {
                DataGridView dgv = (DataGridView)sender;

                // dgv.Rows[e.RowIndex].Cells[i].Selected = true;
                DataGridViewCell cell = dgv[dgv.Columns[e.ColumnIndex].Name, e.RowIndex];

                switch (channelGridView.Columns[e.ColumnIndex].Name)
                {
                    case "More":
                        channelInformation_Click(sender, e);
                        break;

                    default:
                        /*
                        if (false)
                        {
                            for (int i = 1; i < e.ColumnIndex; i++)
                            {
                                if (dgv.Rows[e.RowIndex].Cells[i].Value.ToString() == "")
                                {
                                    // dgv.Rows[e.RowIndex].Cells[i].Selected = true;
                                    cell = dgv[dgv.Columns[i].Name, e.RowIndex];
                                    break;
                                }
                            }
                        }
                        */
                        dgv.CurrentCell = cell;
                        dgv.BeginEdit(true);
                        break;
                }     
            }
        }

        private void channelGridView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            DataGridViewCell cell = dgv[dgv.Columns[e.ColumnIndex].Name, e.RowIndex];
/*
            if (false)
            {
                for (int i = 1; i < e.ColumnIndex; i++)
                {
                    if (dgv.Rows[e.RowIndex].Cells[i].Value.ToString() == "")
                    {
                        // dgv.Rows[e.RowIndex].Cells[i].Selected = true;
                        cell = dgv[dgv.Columns[i].Name, e.RowIndex];
                        break;
                    }
                }
            }
*/
            switch (dgv.Columns[e.ColumnIndex].Name)
            {
                case "ScanAdd":
                case "TXPower":
                case "DecQTDQT":
                case "EncQTDQT":
                case "ChannelType":
                case "Bandwidth":
                    SetComboBoxCellType objChangeCellType = new SetComboBoxCellType(ChangeCellToComboBox);
                    dgv.BeginInvoke(objChangeCellType, e.RowIndex, e.ColumnIndex);
                    bIsComboBox = false;
                    break;

                case "More":
                    channelInformation_Click(sender, e);
                    break;

                default:
                    DataGridViewTextBoxCell textBoxCell = dgv.CurrentCell as DataGridViewTextBoxCell;
                    if (textBoxCell != null)
                    {
                        dgv.BeginEdit(false);
                    }
                    break;
            }
        }

        private void ChannelMove(int direction)
        {
            DataGridView dgv = channelGridView;

            int rowIndex = dgv.CurrentRow.Index;

            // UP
            if ((direction == -1) && (rowIndex <= 2))
                return;
            
            // DOWN
            if ((direction == 1) && (rowIndex == dgv.RowCount))
                return;

            Channel channelTo = channels[rowIndex+direction];
            Channel channelFrom = channels[rowIndex];
            Channel channelTemp;

            channelTemp = new Channel(
                channelTo.ChannelNumber,
                channelTo.ChannelType,
                channelTo.ChannelAlias,
                channelTo.RxFrequency,
                channelTo.TxFrequency,
                channelTo.TxPower,
                channelTo.Tot,
                channelTo.Vox,
                channelTo.VoxLevel,
                channelTo.ScanAdd,
                channelTo.ChannelWorkAlone,
                channelTo.DefaultTalkAround,
                channelTo.Bandwidth,
                channelTo.DecQtDqt,
                channelTo.EncQtDqt,
                channelTo.TxAuthorityA,
                channelTo.Relay,
                channelTo.WorkMode,
                channelTo.Slot,
                channelTo.IdSetting,
                channelTo.ColorCode,
                channelTo.Encryption,
                channelTo.EncryptionType,
                channelTo.EncryptionKey,
                channelTo.Promiscuous,
                channelTo.TxAuthorityD,
                channelTo.KillCode,
                channelTo.WakeUpCode,
                channelTo.Contacts,
                channelTo.RxGroupsList,
                channelTo.Selected,
                channelTo.Gps,
                channelTo.SendGpsInfo,
                channelTo.ReceiveGpsInfo,
                channelTo.GpsTimingReport,
                channelTo.GpsTimingReportTxContacts,
                channelTo.Hidden
            );

            channelTo.ChannelNumber = channelFrom.ChannelNumber;
            channelTo.ChannelType = channelFrom.ChannelType;
            channelTo.ChannelAlias = channelFrom.ChannelAlias;
            channelTo.RxFrequency = channelFrom.RxFrequency;
            channelTo.TxFrequency = channelFrom.TxFrequency;
            channelTo.TxPower = channelFrom.TxPower;
            channelTo.Tot = channelFrom.Tot;
            channelTo.Vox = channelFrom.Vox;
            channelTo.VoxLevel = channelFrom.VoxLevel;
            channelTo.ScanAdd = channelFrom.ScanAdd;
            channelTo.ChannelWorkAlone = channelFrom.ChannelWorkAlone;
            channelTo.DefaultTalkAround = channelFrom.DefaultTalkAround;
            channelTo.Bandwidth = channelFrom.Bandwidth;
            channelTo.DecQtDqt = channelFrom.DecQtDqt;
            channelTo.EncQtDqt = channelFrom.EncQtDqt;
            channelTo.TxAuthorityA = channelFrom.TxAuthorityA;
            channelTo.Relay = channelFrom.Relay;
            channelTo.WorkMode = channelFrom.WorkMode;
            channelTo.Slot = channelFrom.Slot;
            channelTo.IdSetting = channelFrom.IdSetting;
            channelTo.ColorCode = channelFrom.ColorCode;
            channelTo.Encryption = channelFrom.Encryption;
            channelTo.EncryptionType = channelFrom.EncryptionType;
            channelTo.EncryptionKey = channelFrom.EncryptionKey;
            channelTo.Promiscuous = channelFrom.Promiscuous;
            channelTo.TxAuthorityD = channelFrom.TxAuthorityD;
            channelTo.KillCode = channelFrom.KillCode;
            channelTo.WakeUpCode = channelFrom.WakeUpCode;
            channelTo.Contacts = channelFrom.Contacts;
            channelTo.RxGroupsList = channelFrom.RxGroupsList;
            channelTo.Selected = channelFrom.Selected;
            channelTo.Gps = channelFrom.Gps;
            channelTo.SendGpsInfo = channelFrom.SendGpsInfo;
            channelTo.ReceiveGpsInfo = channelFrom.ReceiveGpsInfo;
            channelTo.GpsTimingReport = channelFrom.GpsTimingReport;
            channelTo.GpsTimingReportTxContacts = channelFrom.GpsTimingReportTxContacts;
            channelTo.Hidden = channelFrom.Hidden;
           
            channelFrom.ChannelNumber = channelTemp.ChannelNumber;
            channelFrom.ChannelType = channelTemp.ChannelType;
            channelFrom.ChannelAlias = channelTemp.ChannelAlias;
            channelFrom.RxFrequency = channelTemp.RxFrequency;
            channelFrom.TxFrequency = channelTemp.TxFrequency;
            channelFrom.TxPower = channelTemp.TxPower;
            channelFrom.Tot = channelTemp.Tot;
            channelFrom.Vox = channelTemp.Vox;
            channelFrom.VoxLevel = channelTemp.VoxLevel;
            channelFrom.ScanAdd = channelTemp.ScanAdd;
            channelFrom.ChannelWorkAlone = channelTemp.ChannelWorkAlone;
            channelFrom.DefaultTalkAround = channelTemp.DefaultTalkAround;
            channelFrom.Bandwidth = channelTemp.Bandwidth;
            channelFrom.DecQtDqt = channelTemp.DecQtDqt;
            channelFrom.EncQtDqt = channelTemp.EncQtDqt;
            channelFrom.TxAuthorityA = channelTemp.TxAuthorityA;
            channelFrom.Relay = channelTemp.Relay;
            channelFrom.WorkMode = channelTemp.WorkMode;
            channelFrom.Slot = channelTemp.Slot;
            channelFrom.IdSetting = channelTemp.IdSetting;
            channelFrom.ColorCode = channelTemp.ColorCode;
            channelFrom.Encryption = channelTemp.Encryption;
            channelFrom.EncryptionType = channelTemp.EncryptionType;
            channelFrom.EncryptionKey = channelTemp.EncryptionKey;
            channelFrom.Promiscuous = channelTemp.Promiscuous;
            channelFrom.TxAuthorityD = channelTemp.TxAuthorityD;
            channelFrom.KillCode = channelTemp.KillCode;
            channelFrom.WakeUpCode = channelTemp.WakeUpCode;
            channelFrom.Contacts = channelTemp.Contacts;
            channelFrom.RxGroupsList = channelTemp.RxGroupsList;
            channelFrom.Selected = channelTemp.Selected;
            channelFrom.Gps = channelTemp.Gps;
            channelFrom.SendGpsInfo = channelTemp.SendGpsInfo;
            channelFrom.ReceiveGpsInfo = channelTemp.ReceiveGpsInfo;
            channelFrom.GpsTimingReport = channelTemp.GpsTimingReport;
            channelFrom.GpsTimingReportTxContacts = channelTemp.GpsTimingReportTxContacts;
            channelFrom.Hidden = channelTemp.Hidden;

            channelGridView.Update();
            channelGridView.Refresh();

            dgv.CurrentCell = dgv.Rows[rowIndex + direction].Cells[dgv.CurrentCell.ColumnIndex];
            dgv.Rows[rowIndex + direction].Selected = true;
        }

        private void ChannelUp_Click(object sender, EventArgs e)
        {
            ChannelMove(-1);
        }

        private void ChannelDown_Click(object sender, EventArgs e)
        {
            ChannelMove(1);
        }

        /**
         * work around to limit dropdown to 8 lines
         */
        private void channelGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            switch (dgv.Columns[dgv.CurrentCell.ColumnIndex].Name)
            {
                case "ScanAdd":
                case "TXPower":
                case "DecQTDQT":
                case "EncQTDQT":
                case "ChannelType":
                case "Bandwidth":
                    ComboBox cb = e.Control as ComboBox;
                    if (cb != null)
                    {
                        cb.IntegralHeight = false;
                        cb.MaxDropDownItems = 8;
                    }
                    break;

                default:
                    break;
            }
        }

        private void ChangeCellToComboBox(int iRowIndex, int iColumnIndex)
        {
            if (bIsComboBox == false)
            {
                DataGridViewComboBoxCell dgComboCell = new DataGridViewComboBoxCell();
                dgComboCell.DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing;
                dgComboCell.DisplayMember = "Text";
                dgComboCell.ValueMember = "Value";

                switch (channelGridView.Columns[iColumnIndex].Name)
                {
                    case "DecQTDQT":
                    case "EncQTDQT":
                        for (int i = 0; i < QtDqt.Length; i++)
                            dgComboCell.Items.Add(new { Text = QtDqt[i], Value = QtDqt[i] });
                        break;

                    case "Bandwidth":
                        dgComboCell.Items.Add(new { Text = "12.5K", Value = "12.5K" });
                        dgComboCell.Items.Add(new { Text = "25K", Value = "25K" });
                        break;

                    case "ChannelType":
                        dgComboCell.Items.Add(new { Text = "Digital CH", Value = "Digital CH" });
                        dgComboCell.Items.Add(new { Text = "Analog CH", Value = "Analog CH" });
                        break;

                    case "TXPower":
                        dgComboCell.Items.Add(new { Text = "High", Value = "High" });
                        dgComboCell.Items.Add(new { Text = "Mid", Value = "Mid" });
                        dgComboCell.Items.Add(new { Text = "Low", Value = "Low" });
                        break;

                    case "ScanAdd":
                        dgComboCell.Items.Add(new { Text = "Yes", Value = "Yes" });
                        dgComboCell.Items.Add(new { Text = "No", Value = "No" });
                        break;

                    default:
                        break;
                }

                if (channelGridView.CurrentCell.Value != null)
                    dgComboCell.Value = channelGridView.CurrentCell.Value;

                channelGridView.Rows[iRowIndex].Cells[channelGridView.CurrentCell.ColumnIndex] = dgComboCell;
                bIsComboBox = true;
            }
        }

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, System.Windows.Forms.Keys keyData)
        {
            int icolumn = channelGridView.CurrentCell.ColumnIndex;
            int irow = channelGridView.CurrentCell.RowIndex;

            if (keyData == Keys.Enter)
            {
                if (icolumn == channelGridView.Columns.Count - 1)
                {
                    channelGridView.Rows.Add();
                    channelGridView.CurrentCell = channelGridView[0, irow + 1];
                }
                else
                {
                    channelGridView.CurrentCell = channelGridView[icolumn + 1, irow];
                }
                return true;
            }
            else
                return base.ProcessCmdKey(ref msg, keyData);
        }

        public bool IsNumeric(string input)
        {
            int test;
            return int.TryParse(input, out test);
        }

        public static List<string> ReadSelected(String rawRecord)
        {
            List<string> result = new List<string>();
            String value;

            using (StreamReader reader = new StreamReader(new MemoryStream(System.Text.Encoding.ASCII.GetBytes(rawRecord))))
            {
                var csv = new CsvReader(reader);
                csv.Configuration.Encoding = Encoding.UTF8;
                csv.Configuration.HasHeaderRecord = false;
                csv.Configuration.IgnoreQuotes = true;
                csv.Configuration.Delimiter = ",";
                csv.Configuration.BadDataFound = null;

                if (csv.Read()) {
                    for (int i = 30; csv.TryGetField<string>(i, out value) && i<63; i++)
                    {
                        result.Add(value);
                    }
                }
            }
            return result;
        }

        private void ChannelImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = ".";
            openFileDialog1.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Open Channel File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {                   
                    // String filename = @"../../files/channels.csv";
                    String filename = openFileDialog1.FileName;
                    StringBuilder builder = new StringBuilder();

                    using (StreamReader reader = File.OpenText(filename))
                    {
                        SendMessage(channelGridView.Handle, WM_SETREDRAW, false, 0);

                        channelGridView.Rows.Clear();
                        channelGridView.Refresh();
                        channels.Clear();

                        availableGridView.Rows.Clear();
                        availableGridView.Refresh();
                        members.Clear();

                        var csv = new CsvReader(reader);
                        csv.Configuration.Encoding = Encoding.UTF8;
                        // csv.Configuration.TrimHeaders = true;
                        csv.Configuration.HasHeaderRecord = true;
                        csv.Configuration.IgnoreQuotes = true;
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.BadDataFound = null;

                        int rowNumber = 0;

                        while (csv.Read())
                        {
                            String str = csv.GetField<String>(0);

                            switch (str)
                            {
                                case "No#":
                                case "No.":
                                    continue;

                                case "VFO-A":
                                case "VFO-B":
                                    break;

                                default:
                                    rowNumber++;
                                    str = "" + rowNumber;
                                    break;
                            }

                            String channestrlType = csv.GetField<String>(1);
                            String rawRecord = csv.Context.RawRecord;
                            String memberType;
                            String memberAlias;

                            channels.Add(
                                new Channel(
                                str,
                                memberType = csv.GetField<String>(1),
                                memberAlias = csv.GetField<String>(2),
                                csv.GetField<String>(3),
                                csv.GetField<String>(4),
                                csv.GetField<String>(5),
                                csv.GetField<String>(6),
                                csv.GetField<String>(7),
                                csv.GetField<String>(8),
                                str.StartsWith("VFO") ? "No" : csv.GetField<String>(9),
                                csv.GetField<String>(10),
                                csv.GetField<String>(11),
                                csv.GetField<String>(12),
                                csv.GetField<String>(13),
                                csv.GetField<String>(14),
                                csv.GetField<String>(15),
                                csv.GetField<String>(16),
                                csv.GetField<String>(17),
                                csv.GetField<String>(18),
                                csv.GetField<String>(19),
                                csv.GetField<String>(20),
                                csv.GetField<String>(21),
                                csv.GetField<String>(22),
                                csv.GetField<String>(23),
                                csv.GetField<String>(24),
                                csv.GetField<String>(25),
                                csv.GetField<String>(26),
                                csv.GetField<String>(27),
                                csv.GetField<String>(28),
                                csv.GetField<String>(29),
                                ReadSelected(rawRecord),
                                csv.GetField<String>(63),
                                csv.GetField<String>(64),
                                csv.GetField<String>(65),
                                csv.GetField<String>(66),
                                csv.GetField<String>(67),
                                rawRecord)
                            );

                            if (IsNumeric(str))
                                members.Add(new Member(Int32.Parse(str), 0, memberType, memberAlias));
                        }

                        channelGridView.RowCount = 3002;
                        availableGridView.RowCount = 3000;

                        SendMessage(channelGridView.Handle, WM_SETREDRAW, true, 0);
                        channelGridView.Refresh();
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void HD1Sharp_Load(object sender, EventArgs e)
        {
            // readConfiguration(@"../../files/configuration.xml");

            if (Properties.Settings.Default.IsMaximized)
                WindowState = FormWindowState.Maximized;
            else 
            {
                Rectangle rect = new Rectangle(Properties.Settings.Default.WindowLocation, Properties.Settings.Default.WindowSize);
                // in case of multi screens
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.WorkingArea.IntersectsWith(rect)) {
                        this.StartPosition = FormStartPosition.Manual;
                        this.Location = Properties.Settings.Default.WindowLocation;
                        this.Size = Properties.Settings.Default.WindowSize;
                        WindowState = FormWindowState.Normal;
                        break;
                    }
                }
            }
        }

        private void HD1Sharp_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.WindowLocation = this.Location;
            Properties.Settings.Default.WindowSize = this.Size;
            Properties.Settings.Default.Save();
        }
 
        private void aboutMenuItem_Click(object sender, System.EventArgs e)
        {
            AboutBox a = new AboutBox();
            a.StartPosition = FormStartPosition.CenterParent;
            a.ShowDialog(this);
        }

        private void SizeLastColumn(ListView lv)
        {
            lv.Columns[lv.Columns.Count - 1].Width = lv.Width - SystemInformation.VerticalScrollBarWidth;
        }

        private String getNumericPart(String str)
        {
            StringBuilder sb = new StringBuilder();

            foreach(char c in str)
            {
                if (c < '0' || c > '9')
                    break;

                sb.Append(c);
            }

            return sb.ToString();
        }

        public void writeChannelInformations(ChannelInformation c, int rowIndex)
        {
            Channel currentChannel = (Channel)this.channels[rowIndex];

            // currentChannel.ChannelNumber =
            currentChannel.ChannelType = c.ciChannelType.Text;
            currentChannel.ChannelAlias = c.ciChannelAlias.Text;
            currentChannel.RxFrequency = c.ciRxFrequency.Text;
            currentChannel.TxFrequency = c.ciTxFrequency.Text;
            currentChannel.TxPower = c.ciTxPower.Text;
            currentChannel.Tot = c.ciTimeOutTimer.Text;
            currentChannel.Vox = c.ciVox.Text;
            currentChannel.VoxLevel = c.ciVoxLevel.Text;
            currentChannel.ScanAdd = c.ciStep.Text;
            currentChannel.ChannelWorkAlone = c.ciWorkAlone.Text;
            currentChannel.DefaultTalkAround = c.ciTalkAround.Text;
            currentChannel.Bandwidth = c.ciBandwidth.Text;
            currentChannel.DecQtDqt = c.ciDec.Text;
            currentChannel.EncQtDqt = c.ciEnc.Text;
            currentChannel.TxAuthorityA = c.ciTxAuthorityA.Text;
            currentChannel.Relay = c.ciRelay.Text;
            currentChannel.WorkMode = c.ciWorkMode.Text;
            currentChannel.Slot = c.ciSlot.Text;
            currentChannel.IdSetting = c.ciIDSetting.Text;
            currentChannel.ColorCode = c.ciColorCode.Text;
            currentChannel.Encryption = c.ciEncryption.Text;
            currentChannel.EncryptionType = c.ciEncryptionType.Text;
            currentChannel.EncryptionKey = c.ciEncryptionKey.Text;
            currentChannel.Promiscuous = c.ciPromiscuous.Text;
            currentChannel.TxAuthorityD = c.ciTxAuthorityD.Text;
            currentChannel.KillCode = c.ciKillCode.Text;
            currentChannel.WakeUpCode = c.ciWakeUpCode.Text;
            currentChannel.Contacts = c.ciContacts.Text;
            currentChannel.RxGroupsList = c.ciRXGroupList.Text;

            currentChannel.Selected.Clear();
            for(int i=0; i< c.ciSelectedMembers.Items.Count; i++) {
                // if (c.ciSelectedMembers.Items[i].Text == "")
                //    break;
                currentChannel.Selected.Add(c.ciSelectedMembers.Items[i].Text);
            }            

            // currentChannel.Selected;
            currentChannel.Gps = c.ciGPS.Text;
            currentChannel.SendGpsInfo = c.ciSendGPSInfo.Text;
            currentChannel.ReceiveGpsInfo = c.ciReceiveGPSInfo.Text;
            currentChannel.GpsTimingReport = c.ciGPSTiming.Text;
            currentChannel.GpsTimingReportTxContacts = c.ciGPSContacts.Text;

            if (channelGridView.Rows[rowIndex].Displayed)
                channelGridView.InvalidateRow(rowIndex);
        }

        public void readChannelInformations(ChannelInformation c, int rowIndex)
        {
            Channel channel = channels[rowIndex];
            String rawRecord = channel.Hidden;

            c.ciSelectedMembers.Items.Clear();

            channelGridView.Refresh();

            // Channel Name
            c.channelName.Text = channel.ChannelNumber;
            // Channel Type
            c.ciChannelType.Text = channel.ChannelType;
            c.ciInputContactNo.Visible = (channel.ChannelType == "Digital CH");

            // Tx Power
            c.ciTxPower.Text = channel.TxPower;
            // Timeout Timer
            String s = getNumericPart(channel.Tot);
            c.ciTimeOutTimer.Text = s == "0" ? "Endless": s;
            // VOX Level
            c.ciVoxLevel.Text = channel.VoxLevel;
            // VOX
            c.ciVox.Checked = channel.Vox == "Yes";
            // Channel Alias
            c.ciChannelAlias.Text = channel.ChannelAlias;
            // RxFrequency
            c.ciRxFrequency.Text = channel.RxFrequency;
            // TxFrequency
            c.ciTxFrequency.Text = channel.TxFrequency;
            // Step
            c.ciStep.Text = channel.ScanAdd;
            // Bandwidth
            c.ciBandwidth.Text = channel.Bandwidth;
            // Dec
            c.ciDec.Text = channel.DecQtDqt;
            // Enc
            c.ciEnc.Text = channel.EncQtDqt;
            // TX Authority Analogic
            c.ciTxAuthorityA.Text = channel.TxAuthorityA;
            // Relay
            c.ciRelay.Checked = channel.Relay == "Yes";
            // Talk Around
            c.ciTalkAround.Checked = channel.DefaultTalkAround == "Yes";
            // Work Alone
            c.ciWorkAlone.Checked = channel.ChannelWorkAlone == "Yes";

            // Work Mode
            c.ciWorkMode.Text = channel.WorkMode;
            // Slot
            c.ciSlot.Text = channel.Slot;
            // ID Setting
            c.ciIDSetting.Text = channel.IdSetting;
            // Color Code
            c.ciColorCode.Text = channel.ColorCode;
            // TX Authority Digital
            c.ciTxAuthorityD.Text = channel.TxAuthorityD;
            // Kill Code
            c.ciKillCode.Text = channel.KillCode;
            // WakeUp Code
            c.ciWakeUpCode.Text = channel.WakeUpCode;
            // RX Group List
            c.ciRXGroupList.Items.Clear();
            c.ciRXGroupList.IntegralHeight = false;
            c.ciRXGroupList.MaxDropDownItems = 8;
            for (int i = 0; i < 1; i++)
            {
                ComboboxItem item = new ComboboxItem();
                item.Text = "Custom";
                item.Value = i;
                c.ciRXGroupList.Items.Add(item);
            }
            c.ciRXGroupList.Text = channel.RxGroupsList;
            // Encryption
            c.ciEncryption.Checked = (channel.Encryption == "Yes");
            // Encryption Type
            c.ciEncryptionType.Text = channel.EncryptionType;
            // Encryption Key
            c.ciEncryptionKey.Text = channel.EncryptionKey;
            // Promiscuous
            c.ciPromiscuous.Checked = (channel.Promiscuous == "Yes");
            // Contacts
            c.ciContacts.Text = channel.Contacts;
            c.ciInputContactNoTitle.Visible = (c.ciContacts.Text == "Address Book");

            // GPS
            c.ciGPS.Checked = (channel.Gps == "Yes");
            // Send GPS Info
            c.ciSendGPSInfo.Checked = (channel.SendGpsInfo == "Yes");
            // Receive GPS Info
            c.ciReceiveGPSInfo.Checked = (channel.ReceiveGpsInfo == "Yes");
            // GPS Timing Report
            c.ciGPSTiming.Text = channel.GpsTimingReport;
            // GPS Timing Report TX Contacts,
            c.ciGPSContacts.Text = channel.GpsTimingReportTxContacts;

            // Selected Members
            try
            {
                for (int i = 0; i < 33; i++)
                {
                    String str = channel.Selected[i];
                    ListViewItem lvi = new ListViewItem(str);
                    c.ciSelectedMembers.Items.Add(lvi);
                }
            }
            catch(Exception)
            {

            }
        }

        private void channelInformation_Click(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            if (dgv.CurrentCell.RowIndex >= channelMaxRows-1)
                return;

            ChannelInformation c = new ChannelInformation(sender, e);
            c.StartPosition = FormStartPosition.CenterParent;
            SizeLastColumn(c.ciSelectedMembers);
            SizeLastColumn(c.ciPriorityContacts);

            dgv.Rows[e.RowIndex].Selected = true;

            // Dec & Enc
            c.ciDec.IntegralHeight = false;
            c.ciDec.MaxDropDownItems = 8;
            c.ciEnc.IntegralHeight = false;
            c.ciEnc.MaxDropDownItems = 8;

            for (int i = 0; i < QtDqt.Length; i++)
            {
                ComboboxItem item = new ComboboxItem();
                item.Text = QtDqt[i];
                item.Value = i;
                c.ciDec.Items.Add(item);
                c.ciEnc.Items.Add(item);
            }

            // Timeout Timer
            c.ciTimeOutTimer.IntegralHeight = false;
            c.ciTimeOutTimer.MaxDropDownItems = 8;
            for (int i = 0; i < 615; i += 15)
            {
                ComboboxItem item = new ComboboxItem();
                item.Text = (i == 0) ? "Endless":""+i;
                item.Value = i;
                c.ciTimeOutTimer.Items.Add(item);
            }

            // GPS Timing Report
            c.ciGPSTiming.IntegralHeight = false;
            c.ciGPSTiming.MaxDropDownItems = 8;
            for (int i = 20; i < 1010; i += 10)
            {
                ComboboxItem item = new ComboboxItem();
                item.Text = (i == 20) ? "OFF" : "" + i;
                item.Value = i;
                c.ciGPSTiming.Items.Add(item);
            }

            readChannelInformations(c, c.rowIndex = dgv.CurrentCell.RowIndex);

            // trick, set one column
            ListView l = c.Controls.Find("ciPriorityContacts", true)[0] as ListView;
            if (l != null)
            {
                l.Clear();

                // ShowScrollBar(l.Handle, (int)SB_VERT, true);

                ColumnHeader header1;
                header1 = new ColumnHeader();

                // Set the text, alignment and width for each column header.
                header1.Text = "Available";
                header1.TextAlign = HorizontalAlignment.Center;
                header1.Width = 100;
                l.AllowColumnReorder = false;

                // Add the headers to the ListView control.
                l.Columns.Add(header1);
                l.Columns[0].Width = l.Width - 4;

                // priorityContactsGrid contains all the data
                // c.ciSelectedMembers a subset
                for (int i=0; i<priorityContactsGrid.RowCount; i++)
                {
                    // exit if no more
                    if (priorityContactsGrid.Rows[i].Cells["CallType"].Value == null)
                        break;

                    // select only Group Call type
                    if (priorityContactsGrid.Rows[i].Cells["CallType"].Value.ToString() == "Group Call")
                    {
                        String str = priorityContactsGrid.Rows[i].Cells["ContactAlias"].Value.ToString();
                        Boolean found = false;

                        // don't add items if they are already in c.ciSelectedMembers
                        for (int j = 0; j < c.ciSelectedMembers.Items.Count; j++)
                        {
                            if (c.ciSelectedMembers.Items[j].Text.Length == 0)
                                break;

                            if (c.ciSelectedMembers.Items[j].Text.StartsWith("Priority Contacts: ")) { 
                                if (c.ciSelectedMembers.Items[j].Text.Substring(19) == str)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }

                        if (!found)
                        {
                            ListViewItem lvi = new ListViewItem(str);
                            l.Items.Add(lvi);
                        }
                    }
                }

                if (l.Items.Count > 0)
                {
                    l.Items[0].Selected = true;
                    // hack to activate scrollbar
                    l.Select();
                    c.Select();
                }
            }

            c.ShowDialog(this);

            writeChannelInformations(c, c.rowIndex);
        }

        private void saveMenuItem_Click(object sender, EventArgs e)
        {
            writeXmlFile(strConfigFilePath);
        }

        private void saveAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.FileName = Path.GetFileName(strConfigFilePath);
            saveFileDialog1.OverwritePrompt = true;
            saveFileDialog1.InitialDirectory = ".";
            saveFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    writeXmlFile(saveFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file to disk. Original error: " + ex.Message);
                }
            }
        }

        private void openMenuItem_Click(object sender, System.EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = ".";
            openFileDialog1.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "Open Configuration File";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    readConfiguration(openFileDialog1.FileName);
                    strConfigFilePath = openFileDialog1.FileName;

                    /* 
                        if channel tab, refresh the grid after loading configuration
                    */
                    if (tabContainer.SelectedTab.Name == "Channel")
                    {
                        channelGridView.Update();
                        channelGridView.Refresh();
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }
        private void openPowerOnLogo_Click(object sender, System.EventArgs e)
        {
            PowerOnLogo p = new PowerOnLogo();
            p.StartPosition = FormStartPosition.CenterParent;
            p.Port = defaultPort;
            p.ShowDialog(this);
        }

        private void openPort_Click(object sender, System.EventArgs e)
        {
            Port p = new Port();
            p.StartPosition = FormStartPosition.CenterParent;
            p.portsList.Items.Clear();

            string[] ports = SerialPort.GetPortNames();

            // Display each port name to the console.
            foreach (string port in ports)
            {
                p.portsList.Items.Add(port);
            }

            if (p.portsList.Items.Count > 0)
                p.portsList.SelectedIndex = 0;

            if (p.ShowDialog(this) == DialogResult.OK)
            {
                if (p.portsList.SelectedIndex != -1)
                    defaultPort = p.portsList.Items[p.portsList.SelectedIndex].ToString();
            }
        }

        private void ExportAddressBook_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.InitialDirectory = ".";
            saveFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.Title = "Save Configuration File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder builder = new StringBuilder();

                    using (StreamWriter writer = File.CreateText(saveFileDialog.FileName))
                    {
                        SendMessage(channelGridView.Handle, WM_SETREDRAW, false, 0);

                        var csv = new CsvWriter(writer);
                        csv.Configuration.HasHeaderRecord = false;
                        csv.Configuration.Delimiter = ",";

                        foreach (Contact contact in contacts) {
                            csv.WriteField<String>(contact.CallType);       // Call Type
                            csv.WriteField<String>(contact.ContactAlias);   // Contact Alias
                            csv.WriteField<String>(contact.City);           // City
                            csv.WriteField<String>(contact.Province);       // Province
                            csv.WriteField<String>(contact.Country);        // Country
                            csv.WriteField<String>(contact.CallID);         // Call ID

                            csv.NextRecord();
                        }

                        SendMessage(channelGridView.Handle, WM_SETREDRAW, true, 0);

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void ImportAddressBook_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            int rowNumber = 0;

            openFileDialog.InitialDirectory = ".";
            openFileDialog.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {                    
                    StringBuilder builder = new StringBuilder();
                    Boolean capitalize = ((CheckBox)AddressBookContacts.Controls.Find("abCapitalize", true)[0]).Checked;

                    using (StreamReader reader = File.OpenText(openFileDialog.FileName))
                    {                     
                        SendMessage(channelGridView.Handle, WM_SETREDRAW, false, 0);

                        addressbookGridView.Rows.Clear();
                        contacts.Clear();

                        var csv = new CsvReader(reader);
                        csv.Configuration.Encoding = Encoding.UTF8;
                        csv.Configuration.HasHeaderRecord = false;
                        csv.Configuration.IgnoreQuotes = true;
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.BadDataFound = null;

                        addressbookGridView.Refresh();
                        
                        while (csv.Read())
                        {
                            rowNumber++;

                            // Private Call,VE3THW Wayne,Toronto,Ontario,Canada,1023001,                           
                            if (capitalize) {
                                contacts.Add(
                                    new Contact(
                                    "" + rowNumber,                     // CH n°
                                    csv.GetField<String>(0),            // Call Type
                                    csv.GetField<String>(1).ToUpper(),  // Contact Alias
                                    csv.GetField<String>(2).ToUpper(),  // City
                                    csv.GetField<String>(3).ToUpper(),  // Province
                                    csv.GetField<String>(4).ToUpper(),  // Country
                                    csv.GetField<String>(5)             // Call ID                                
                                    )
                                );
                            }
                            else
                            {
                                contacts.Add(
                                    new Contact(
                                    "" + rowNumber,               // CH n°
                                    csv.GetField<String>(0),    // Call Type
                                    csv.GetField<String>(1),    // Contact Alias
                                    csv.GetField<String>(2),    // City
                                    csv.GetField<String>(3),    // Province
                                    csv.GetField<String>(4),    // Country
                                    csv.GetField<String>(5)     // Call ID                                
                                    )
                                );
                            }
                        }

                        addressbookGridView.RowCount = rowNumber;

                        SendMessage(channelGridView.Handle, WM_SETREDRAW, true, 0);
                        addressbookGridView.Refresh();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void addressbookGridView_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
        {
            // If this is the row for new records, no values are needed.
            if (e.RowIndex == ((DataGridView)sender).RowCount - 1)
                return;

            Contact contactTmp = null;

            // Store a reference to the Contact object for the row being painted.
            if (e.RowIndex == contactRowInEdit)
            {
                contactTmp = contactInEdit;
            }
            else
            {
                if (e.RowIndex > contactMaxRows)
                    return;

                contactTmp = (Contact)contacts[e.RowIndex];
            }
           
            // Set the cell value to paint using the Channel object retrieved.
            switch (addressbookGridView.Columns[e.ColumnIndex].Name)
            {
                case "ctNumber":
                    e.Value = contactTmp.Number;
                    break;

                case "ctCallType":
                    e.Value = contactTmp.CallType;
                    break;

                case "ctContactAlias":
                    e.Value = contactTmp.ContactAlias;
                    break;

                case "ctCity":
                    e.Value = contactTmp.City;
                    break;

                case "ctProvince":
                    e.Value = contactTmp.Province;
                    break;

                case "ctCountry":
                    e.Value = contactTmp.Country;
                    break;

                case "ctCallID":
                    e.Value = contactTmp.CallID;
                    break;
            }
        }

        private void addressbookGridView_CellValuePushed(object sender, DataGridViewCellValueEventArgs e)
        {
            Contact contactTmp = null;

            // Store a reference to the Channel object for the row being edited.
            if (e.RowIndex < this.contacts.Count)
            {
                // If the user is editing a new row, create a new Channel object.
                if (this.contactInEdit == null)
                {
                    this.contactInEdit = new Contact(
                        ((Contact)this.contacts[e.RowIndex]).Number,
                        ((Contact)this.contacts[e.RowIndex]).CallType,
                        ((Contact)this.contacts[e.RowIndex]).ContactAlias,
                        ((Contact)this.contacts[e.RowIndex]).City,
                        ((Contact)this.contacts[e.RowIndex]).Province,
                        ((Contact)this.contacts[e.RowIndex]).Country,
                        ((Contact)this.contacts[e.RowIndex]).CallID
                   );
                }
                contactTmp = this.contactInEdit;
                this.contactRowInEdit = e.RowIndex;
            }
            else
            {
                contactTmp = this.contactInEdit;
            }

            // Set the appropriate Channel property to the cell value entered.
            String newValue = e.Value as String;
            switch (((DataGridView)sender).Columns[e.ColumnIndex].Name)
            {
                case "ctCallTyper":
                    contactTmp.CallType = newValue;
                    break;

                case "ctContactAlias":
                    contactTmp.ContactAlias = newValue;
                    break;

                case "ctCity":
                    contactTmp.City = newValue;
                    break;

                case "ctProvince":
                    contactTmp.Province = newValue;
                    break;

                case "ctCountry":
                    contactTmp.Country = newValue;
                    break;

                case "ctCallID":
                    contactTmp.CallID = newValue;
                    break;
            }
        }

        private void addressbookGridView_RowDirtyStateNeeded(object sender, QuestionEventArgs e)
        {
            if (!rowScopeCommit)
            {
                // In cell-level commit scope, indicate whether the value
                // of the current cell has been modified.
                e.Response = ((DataGridView)sender).IsCurrentCellDirty;
            }
        }

        private void addressbookGridView_NewRowNeeded(object sender, DataGridViewRowEventArgs e)
        {
            // Create a new Channel object when the user edits
            // the row for new records.
            this.contactInEdit = new Contact();
            this.contactRowInEdit = ((DataGridView)sender).Rows.Count - 1;
        }

        private void addressbookGridView_RowValidated(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;

            // Save row changes if any were made and release the edited 
            // Contact object if there is one.
            if (e.RowIndex >= this.channels.Count && e.RowIndex != dgv.Rows.Count - 1)
            {
                // Add the new Channel object to the data store.
                this.contacts.Add(this.contactInEdit);
                this.contactInEdit = null;
                this.contactRowInEdit = -1;
            }
            else if (this.contactInEdit != null && e.RowIndex < this.contacts.Count)
            {
                // Save the modified Contact object in the data store.
                this.contacts[e.RowIndex] = this.contactInEdit;
                this.contactInEdit = null;
                this.contactRowInEdit = -1;
            }
            else if (dgv.ContainsFocus)
            {
                this.contactInEdit = null;
                this.contactRowInEdit = -1;
            }
        }

        private void addressbookGridView_CancelRowEdit(object sender, QuestionEventArgs e)
        {
            if (this.contactRowInEdit == ((DataGridView)sender).Rows.Count - 2 && this.contactRowInEdit == this.contacts.Count)
            {
                // If the user has canceled the edit of a newly created row, 
                // replace the corresponding Channel object with a new, empty one.
                this.contactInEdit = new Contact();
            }
            else
            {
                // If the user has canceled the edit of an existing row, 
                // release the corresponding Customer object.
                this.contactInEdit = null;
                this.contactRowInEdit = -1;
            }
        }

        private void addressbookGridView_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (e.Row.Index < this.contacts.Count)
            {
                // If the user has deleted an existing row, remove the 
                // corresponding Contact object from the data store.
                this.contacts.RemoveAt(e.Row.Index);
            }

            if (e.Row.Index == this.contactRowInEdit)
            {
                // If the user has deleted a newly created row, release
                // the corresponding Contact object. 
                this.contactRowInEdit = -1;
                this.contactInEdit = null;
            }
        }

        private void addressbookGridView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            DataGridViewCell cell = dgv[dgv.Columns[e.ColumnIndex].Name, e.RowIndex];
            DataGridViewTextBoxCell textBoxCell = dgv.CurrentCell as DataGridViewTextBoxCell;

            if (textBoxCell != null)
            {
                dgv.BeginEdit(false);
            }
        }
/*
        private void addressbookGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (addressbookGridView.SortOrder.ToString() == "Ascending") // Check if sorting is Ascending
            {
                addressbookGridView.Sort(addressbookGridView.Columns[addressbookGridView.SortedColumn.Name], System.ComponentModel.ListSortDirection.Descending);
            }
            else
            {
                addressbookGridView.Sort(addressbookGridView.Columns[addressbookGridView.SortedColumn.Name], System.ComponentModel.ListSortDirection.Ascending);
            }
        }
*/
        private void initializeEncryption()
        {
            encryptGridView1.Rows.Add("1", "1000");
            encryptGridView1.Rows.Add("2", "2000");
            encryptGridView1.Rows.Add("3", "3000");
            encryptGridView1.Rows.Add("4", "4000");
            encryptGridView1.Rows.Add("5", "5000");
            encryptGridView1.Rows.Add("6", "6000");
            encryptGridView1.Rows.Add("7", "7000");
            encryptGridView1.Rows.Add("8", "8000");
            encryptGridView1.Rows.Add("9", "9000");
            encryptGridView1.Rows.Add("10", "1100");
            encryptGridView1.Rows.Add("11", "1200");
            encryptGridView1.Rows.Add("12", "1300");
            encryptGridView1.Rows.Add("13", "1400");
            encryptGridView1.Rows.Add("14", "1500");
            encryptGridView1.Rows.Add("15", "1600");
            encryptGridView1.Rows.Add("16", "1700");

            encryptGridView2.Rows.Add("1", "10000000000000000000000000000000");
            encryptGridView2.Rows.Add("2", "20000000000000000000000000000000");
            encryptGridView2.Rows.Add("3", "30000000000000000000000000000000");
            encryptGridView2.Rows.Add("4", "40000000000000000000000000000000");
            encryptGridView2.Rows.Add("5", "50000000000000000000000000000000");
            encryptGridView2.Rows.Add("6", "60000000000000000000000000000000");
            encryptGridView2.Rows.Add("7", "70000000000000000000000000000000");
            encryptGridView2.Rows.Add("8", "80000000000000000000000000000000");
            encryptGridView2.Rows.Add("9", "90000000000000000000000000000000");
            encryptGridView2.Rows.Add("10", "11000000000000000000000000000000");
            encryptGridView2.Rows.Add("11", "12000000000000000000000000000000");
            encryptGridView2.Rows.Add("12", "13000000000000000000000000000000");
            encryptGridView2.Rows.Add("13", "14000000000000000000000000000000");
            encryptGridView2.Rows.Add("14", "15000000000000000000000000000000");
            encryptGridView2.Rows.Add("15", "16000000000000000000000000000000");
            encryptGridView2.Rows.Add("16", "17000000000000000000000000000000");
        }

        private void GridViewEditMode_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = (DataGridView)sender;
            DataGridViewCell cell = dgv[dgv.Columns[e.ColumnIndex].Name, e.RowIndex];
            DataGridViewTextBoxCell textBoxCell = dgv.CurrentCell as DataGridViewTextBoxCell;

            if (textBoxCell != null)
            {
                dgv.BeginEdit(false);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {

        }


        private void button7_Click(object sender, EventArgs e)
        {

        }
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public static class Extensions
    {
        public static void EnableDoubleBuferring(this Control control)
        {
            var property = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            property.SetValue(control, true, null);
        }
    }
}
