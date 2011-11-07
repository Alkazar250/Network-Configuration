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
    public partial class Form2 : Form
    {
        int index = 0;
        Confs configurations=new Confs();
        public Form2(string adapterName, string adapterIndex)
        {
            InitializeComponent();
            Adapter.Text += adapterName;
            index = Int16.Parse(adapterIndex);
            Deserialize();
            Initialize();
        }
        public void Initialize()
        {
            foreach (Config c in configurations.confs)
                comboBox1.AutoCompleteCustomSource.Add(c.name);
            comboBox1.DataSource = comboBox1.AutoCompleteCustomSource;
        }
        public void ChangeIP(int ind)
        {
            try
            {
                ManagementObject classInstance =
                   new ManagementObject("root\\CIMV2",
                   "Win32_NetworkAdapterConfiguration.Index='"+ind+"'",
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

            }
            catch (ManagementException err)
            {
                MessageBox.Show("An error occurred while trying to execute the WMI method: " + err.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ChangeIP(index);
            ChangeGW(index);
            ChangeDNS(index);
            this.Close();
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
            configurations = (Confs) s.Deserialize(r);
            r.Close();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            Config cf = new Config();
            cf.name = comboBox1.Text;
            cf.ip = textBox1.Text;
            cf.mask = textBox2.Text;
            cf.gw = textBox4.Text;
            cf.dns = textBox3.Lines;
            configurations.confs.Add(cf);
            Serialize();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Config c = new Config();
            foreach (Config cf in configurations.confs)
                if (cf.name == comboBox1.Text) c = cf;
            textBox1.Text = c.ip;
            textBox2.Text = c.mask;
            textBox4.Text = c.gw;
            textBox3.Lines = c.dns;
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
        [XmlAttribute] public string name;
        [XmlElement] public string ip;
        [XmlElement] public string mask;
        [XmlElement] public string gw;
        [XmlElement] public string[] dns;
    }
}
