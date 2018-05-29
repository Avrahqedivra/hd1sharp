using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hd1sharp
{
    public class ZoneManager
    {
        private TreeView mainTreeView;
        public ContextMenu zonesPopupMenu, zonePopupMenu;
        public TreeNode zoneTreeNode;
        public TreeNode selectedNode;
        public long startTimer;
        public HD1Sharp hd1sharp;

        public List<Zone> zones = new List<Zone>();

        public ZoneManager(HD1Sharp hd1sharp, TreeView mainTreeView, TreeNode zoneTreeNode)
        {
            this.hd1sharp = hd1sharp;
            this.zoneTreeNode = zoneTreeNode;
            this.mainTreeView = mainTreeView;
        }

        public void addZone()
        {
            String zoneAlias = "Zone" + zoneTreeNode.GetNodeCount(true);
            zoneTreeNode.Nodes.Add(new TreeNode(zoneAlias));
            zones.Add(new Zone("Priority Contact - ", zoneAlias));
        }

        public void deleteZone(TreeNode selectedNode)
        {
            if (selectedNode != null)
            {
                String zoneAlias = selectedNode.Text;
                foreach(Zone z in zones)
                {
                    if (z.Name == zoneAlias)
                    {
                        zones.Remove(z);
                        zoneTreeNode.Nodes.Remove(selectedNode);
                        break;
                    }
                }
            }
        }

        public void renameZone(String newName)
        {
            Point point = mainTreeView.PointToClient(Cursor.Position);
            TreeNode selectedNode = mainTreeView.GetNodeAt(point.X, point.Y);

            if (selectedNode != null)
            {
                foreach (Zone zone in zones)
                {
                    if (zone.Name == selectedNode.Name)
                    {                      
                        break;
                    }
                }
            }
        }

        private void initZoneGrid(String zoneTitle)
        {
            for (int i = 2, j = 0; i < hd1sharp.channelMaxRows - 1 && j < hd1sharp.memberMaxRows; i++)
            {
                if (hd1sharp.channels[i].ChannelType == "Digital CH")
                {
                    hd1sharp.members[j].MemberNumber = Int32.Parse(hd1sharp.channels[i].ChannelNumber);
                    hd1sharp.members[j].MemberChannel = i;
                    hd1sharp.members[j].MemberAlias = hd1sharp.channels[i].ChannelAlias;
                    hd1sharp.members[j++].MemberType = hd1sharp.channels[i].ChannelType;
                }
            }
        }

        public void selectZone(TreeNode zoneTreeNode)
        {
            if (zoneTreeNode != null) {
                initZoneGrid(zoneTreeNode.Text);
            }
        }
    }
}
