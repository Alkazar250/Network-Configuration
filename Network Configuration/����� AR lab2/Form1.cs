using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;

namespace AR_lab2
{

    public partial class Form1 : Form
    {
        bool firstTime = true;
        public Form1()
        {
            InitializeComponent();
            Start();
        }
        public void Start()
        {
            int count = -1;
            treeView1.Nodes.Clear();
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=true");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    count++;
                    treeView1.Nodes.Add("Network Card № " + queryObj["Index"]);

                    //Console.WriteLine("Caption: {0}", queryObj["Caption"]);
                    treeView1.Nodes[count].Nodes.Add("Name: " + queryObj["Caption"]);
                    treeView1.Nodes[count].Nodes.Add("Description: " + queryObj["Description"]);
                    treeView1.Nodes[count].Nodes.Add("MACAddress: " + queryObj["MACAddress"]);

                    if (queryObj["IPAddress"] == null)
                        treeView1.Nodes[count].Nodes.Add("IPAddress: " + queryObj["IPAddress"]);
                    else
                    {
                        String[] arrIPAddress = (String[])(queryObj["IPAddress"]);
                        foreach (String arrValue in arrIPAddress)
                        {
                            treeView1.Nodes[count].Nodes.Add("IPAddress: " + arrValue);
                        }
                    }

                    if (queryObj["IPSubnet"] == null)
                        treeView1.Nodes[count].Nodes.Add("IPSubnet: " + queryObj["IPSubnet"]);
                    else
                    {
                        String[] arrIPSubnet = (String[])(queryObj["IPSubnet"]);
                        foreach (String arrValue in arrIPSubnet)
                        {
                            treeView1.Nodes[count].Nodes.Add("IPSubnet: " + arrValue);
                        }
                    }

                    if (queryObj["DefaultIPGateway"] == null)
                        treeView1.Nodes[count].Nodes.Add("DefaultIPGateway: " + queryObj["DefaultIPGateway"]);
                    else
                    {
                        String[] arrDefaultIPGateway = (String[])(queryObj["DefaultIPGateway"]);
                        foreach (String arrValue in arrDefaultIPGateway)
                        {
                            treeView1.Nodes[count].Nodes.Add("DefaultIPGateway: " + arrValue);
                        }
                    }

                    if (queryObj["DNSServerSearchOrder"] == null)
                        treeView1.Nodes[count].Nodes.Add("DNSServerSearchOrder: " + queryObj["DNSServerSearchOrder"]);
                    else
                    {
                        String[] arrDNSServerSearchOrder = (String[])(queryObj["DNSServerSearchOrder"]);
                        foreach (String arrValue in arrDNSServerSearchOrder)
                        {
                            treeView1.Nodes[count].Nodes.Add("DNSServerSearchOrder: " + arrValue);
                        }
                    }
                }
            }
            catch (ManagementException e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }
            treeView1.ExpandAll();
            firstTime = true;
        }
        
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (treeView1.SelectedNode.Parent == null && firstTime == false)
            {
                new Form2(treeView1.SelectedNode.Nodes[1].Text.Split(':')[1],
                    treeView1.SelectedNode.Text.Split('№')[1]).ShowDialog();
                Start();
            }
            else firstTime = false;
            
        }
    }
}

    
