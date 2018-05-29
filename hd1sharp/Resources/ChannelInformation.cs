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
using System.Windows.Forms;

namespace hd1sharp.Resources
{
    public partial class ChannelInformation : Form
    {
        public static String PRIORITYCONTACTS = "Priority Contacts: ";
        public String selectedContacts = "";
        public int rowIndex; 

        public ChannelInformation(object sender, DataGridViewCellEventArgs e)
        {
            InitializeComponent();
            rowIndex = e.RowIndex;

            // Double buffering can make DGV slow in remote desktop
            if (!System.Windows.Forms.SystemInformation.TerminalServerSession)
            {
                this.DoubleBuffered = true;
                foreach (Control control in this.Controls)
                {
                    control.EnableDoubleBuferring();
                }
            }
            // ciSelectedMembers.Columns[0].Width = 0;
        }

        private void FirstChannel_Click(object sender, EventArgs e)
        {
            rowIndex = 0;
            HD1Sharp.Instance.readChannelInformations(this, rowIndex);
        }

        private void LastChannel_Click(object sender, EventArgs e)
        {
            rowIndex = HD1Sharp.Instance.channels.Count - 1;
            HD1Sharp.Instance.readChannelInformations(this, rowIndex);
        }

        private void NextChannel_Click(object sender, EventArgs e)
        {
            if (rowIndex < (HD1Sharp.Instance.channels.Count - 1))
                HD1Sharp.Instance.readChannelInformations(this, ++rowIndex);
        }

        private void PreviousChannel_Click(object sender, EventArgs e)
        {
            if (rowIndex > 0)
                HD1Sharp.Instance.readChannelInformations(this, --rowIndex);
        }

        private void list_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (var sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;

                using (var headerFont = new Font("Segoe UI Semibold", 9, FontStyle.Bold))
                {
                    e.Graphics.FillRectangle(Brushes.LightGray, e.Bounds);
                    int y = e.Bounds.Height - headerFont.Height / 2;
                    RectangleF r = e.Bounds;
                    r.Y = (e.Bounds.Height - headerFont.Height) / 2;
                    r.Height = headerFont.Height;
                    e.Graphics.DrawString(e.Header.Text, headerFont, Brushes.Black, r, sf);
                }
            }
        }

        private void list_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if ((e.State & ListViewItemStates.Selected) != 0)
            {
                // Draw the background and focus rectangle for a selected item.
                e.Graphics.FillRectangle(Brushes.Blue, e.Bounds);
                e.DrawFocusRectangle();               
            }
            else
            {                
                e.Graphics.FillRectangle(Brushes.White, e.Bounds);                
            }

            // Draw the item text for views other than the Details view.
            if (((ListView)sender).View == View.Details)
            {
                // e.DrawText();
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Near;

                    using (var textFont = new Font("Segoe UI Semibold", 9, FontStyle.Regular))
                    {
                        if ((e.State & ListViewItemStates.Selected) != 0)
                            e.Graphics.DrawString(e.Item.Text, textFont, Brushes.White, e.Bounds, sf);
                        else
                            e.Graphics.DrawString(e.Item.Text, textFont, Brushes.Black, e.Bounds, sf);
                    }
                }
            }
        }

        private void list_MouseUp(object sender, MouseEventArgs e)
        {
            ListViewItem clickedItem = ((ListView)sender).GetItemAt(5, e.Y);
            if (clickedItem != null)
            {
                clickedItem.Selected = true;
                clickedItem.Focused = true;
            }
        }

        private void ciAddPriorityContact_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in ciPriorityContacts.SelectedItems)
            {              
                for (int i = 0; i < ciSelectedMembers.Items.Count; i++) {
                    if (ciSelectedMembers.Items[i].Text.Length == 0)
                    {
                        ciSelectedMembers.Items[i].Text = PRIORITYCONTACTS + item.SubItems[0].Text;
                        ciPriorityContacts.Items.Remove(item);
                        break;
                    }
                }
            }
        }

        private void ciRemovePriorityContact_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in ciSelectedMembers.SelectedItems)
            {
                if (item.Text.Length == 0)
                    break;

                if (item.Text.StartsWith(PRIORITYCONTACTS))
                {
                    ListViewItem lvi = new ListViewItem(item.Text.Substring(19));                    
                    ciPriorityContacts.Items.Add(lvi);
                    ciSelectedMembers.Items.Remove(item);
                }
            }
        }

        private void ChannelInformation_FormClosing(object sender, FormClosingEventArgs e)
        {
            selectedContacts = "";

            foreach (ListViewItem item in ciSelectedMembers.Items)
            {
                
                selectedContacts += item.Text + ",";
            }
        }

        private void channelName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    if (channelName.Text.ToLower() == "vfo-a")
                    {
                        rowIndex = 0;
                    }
                    else
                    if (channelName.Text.ToLower() == "vfo-b")
                        rowIndex = 1;
                    else
                    {
                        int temp = Int32.Parse(channelName.Text);

                        if ((temp < 1) || (temp > (HD1Sharp.Instance.channels.Count)))
                        {
                            channelName.Text = ""+(rowIndex-1);
                            channelName.Focus();
                            channelName.SelectionStart = channelName.Text.Length;
                            return;
                        }

                        rowIndex = temp+1;
                    }

                    HD1Sharp.Instance.readChannelInformations(this, rowIndex);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
