using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Xml.Serialization;
using System.Collections;
using System.IO;

namespace AR_lab2
{

    public partial class Form1 : Form
    {
        int index = 0;
        Confs configurations = new Confs();
        public Form1()
        {
            InitializeComponent();
            Populate();
            Deserialize();
            Initialize();
        }
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);

        public void Populate()
        {
            listBox1.Items.Clear();
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=true");

                foreach (ManagementObject queryObj in searcher.Get())
                    listBox1.Items.Add(queryObj["Index"]+". "+queryObj["Caption"].ToString().Substring(11));
            }
            catch (ManagementException e)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message);
            }
        }
        public void Initialize()
        {
            foreach (Config c in configurations.confs)
                toolStripComboBox1.Items.Add(c.name);
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            index = Int16.Parse(listBox1.SelectedItem.ToString().Split('.')[0]);
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index='"+
                    index+"'");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Adapter.Text = "Network Adapter: " + queryObj["Description"];

                    textBox5.Text = queryObj["MACAddress"].ToString();

                    if (queryObj["DHCPEnabled"].ToString() == "True")
                        dhcp.Checked = true;
                    else
                        dhcp.Checked = false;

                    textBox1.Text = ((string[])queryObj["IPAddress"])[0];
                    textBox2.Text = ((string[])queryObj["IPSubnet"])[0];

                    if (queryObj["DefaultIPGateway"] == null)
                        textBox4.Text = "Unable to detect";
                    else  textBox4.Text = ((string[])queryObj["DefaultIPGateway"])[0];

                    if (queryObj["DNSServerSearchOrder"] == null)
                       textBox3.Text = "Unable to detect";
                    else
                    {
                        textBox3.Text = "";
                        String[] arrDNSServerSearchOrder = (String[])(queryObj["DNSServerSearchOrder"]);
                        foreach (String arrValue in arrDNSServerSearchOrder)
                        {
                            textBox3.Text += arrValue +"\r\n";
                        }
                    }
                }
            }
            catch (ManagementException ex)
            {
                MessageBox.Show("An error occurred while querying for WMI data: " + ex.Message);
            }
        }
        public void EnableDhcp(int ind)
        {
            try
            {
                ManagementObject classInstance =
                   new ManagementObject("root\\CIMV2",
                   "Win32_NetworkAdapterConfiguration.Index='" + ind + "'",
                   null);

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams =
                    classInstance.InvokeMethod("EnableDHCP", null, null);
            }
            catch (ManagementException err)
            {
                MessageBox.Show("An error occurred while trying to execute the WMI method: " + err.Message);
            }
        }
        public void ChangeIP(int ind)
        {
            try
            {
                ManagementObject classInstance =
                   new ManagementObject("root\\CIMV2",
                   "Win32_NetworkAdapterConfiguration.Index='" + ind + "'",
                   null);

                // Obtain in-parameters for the method
                ManagementBaseObject inParams =
                    classInstance.GetMethodParameters("EnableStatic");

                // Add the input parameters.
                inParams["IPAddress"] = textBox1.Lines;
                inParams["SubnetMask"] = textBox2.Lines;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams =
                    classInstance.InvokeMethod("EnableStatic", inParams, null);
            }
            catch (ManagementException err)
            {
                MessageBox.Show("An error occurred while trying to execute the WMI method: " + err.Message);
            }
        }
        public void ChangeGW(int ind)
        {
            try
            {
                ManagementObject classInstance =
                    new ManagementObject("root\\CIMV2",
                    "Win32_NetworkAdapterConfiguration.Index='" + ind + "'",
                    null);

                // Obtain in-parameters for the method
                ManagementBaseObject inParams =
                    classInstance.GetMethodParameters("SetGateways");

                // Add the input parameters.
                inParams["DefaultIPGateway"] = textBox4.Lines;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams =
                    classInstance.InvokeMethod("SetGateways", inParams, null);
            }
            catch (ManagementException err)
            {
                MessageBox.Show("An error occurred while trying to execute the WMI method: " + err.Message);
            }
        }
        public void ChangeDNS(int ind)
        {
            try
            {
                ManagementObject classInstance =
                    new ManagementObject("root\\CIMV2",
                    "Win32_NetworkAdapterConfiguration.Index='" + ind + "'",
                    null);

                // Obtain in-parameters for the method
                ManagementBaseObject inParams =
                    classInstance.GetMethodParameters("SetDNSServerSearchOrder");

                // Add the input parameters.
                inParams["DNSServerSearchOrder"] = textBox3.Lines;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams =
                    classInstance.InvokeMethod("SetDNSServerSearchOrder", inParams, null);

                classInstance.Dispose();
            }
            catch (ManagementException err)
            {
                MessageBox.Show("An error occurred while trying to execute the WMI method: " + err.Message);
            }
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (dhcp.Checked == true)
                EnableDhcp(index);
            else
            {
                ChangeIP(index);
                ChangeGW(index);
                ChangeDNS(index);
            }
        }
        public void Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(Confs));
            TextWriter w = new StreamWriter("confs.xml");
            s.Serialize(w, configurations);
            w.Flush();
            w.Close();
        }
        public void Deserialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(Confs));
            TextReader r = new StreamReader("confs.xml");
            configurations = (Confs)s.Deserialize(r);
            r.Close();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Config cf = new Config();
            cf.name = toolStripComboBox1.Text;
            cf.ip = textBox1.Text;
            cf.mask = textBox2.Text;
            cf.gw = textBox4.Text;
            cf.dns = textBox3.Lines;
            configurations.confs.Add(cf);
            Serialize();

            Deserialize();
            Initialize();
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config c = new Config();
            foreach (Config cf in configurations.confs)
                if (cf.name == toolStripComboBox1.Text) c = cf;
            textBox1.Text = c.ip;
            textBox2.Text = c.mask;
            textBox4.Text = c.gw;
            textBox3.Lines = c.dns;
            if (c.dhcp == true) dhcp.Checked = true;
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            this.notifyIcon1.Visible = false;
            //this.BringToFront();
            Populate();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //this.Visible = false;
                this.ShowInTaskbar = false;
                this.notifyIcon1.Visible = true;
                SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
            }
        }

        private void dhcp_CheckedChanged(object sender, EventArgs e)
        {
            if(dhcp.Checked==true)
            {
                textBox1.ReadOnly = true;
                textBox2.ReadOnly = true;
                textBox3.ReadOnly = true;
                textBox4.ReadOnly = true;
            }
            else
            {
                textBox1.ReadOnly = false;
                textBox2.ReadOnly = false;
                textBox3.ReadOnly = false;
                textBox4.ReadOnly = false;
            }
        }

    }
    [XmlRoot]
    public class Confs
    {
        [XmlArray]
        public List<Config> confs;
        public Confs()
        {
            confs = new List<Config>();
        }
    }
    public class Config
    {
        [XmlAttribute]
        public string name;
        [XmlElement]
        public bool dhcp;
        [XmlElement]
        public string ip;
        [XmlElement]
        public string mask;
        [XmlElement]
        public string gw;
        [XmlElement]
        public string[] dns;
    }
}

    
