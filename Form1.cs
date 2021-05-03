using MetroFramework.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class metr : MetroFramework.Forms.MetroForm
    {
        private string directory;
        private string[] commands;
        private dynamic result;
        private dynamic sizeLogs;
        public bool NewMinute{get; set;} = false;
        public bool Changes { get; set; } = false;
        public metr()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.StyleManager = metroStyleManager1;
            metroComboBox2.Items.Add("Detection of change in assembly");
            metroComboBox2.Items.Add("The beginning of a new minute");
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                metroTextBox1.Text = openFileDialog1.FileName;
                this.directory = openFileDialog1.FileName;
                string taskText = System.IO.File.ReadAllText(openFileDialog1.FileName);
                var updates = (JContainer)JObject.Parse(taskText)["tasks"];
                this.result = updates.Descendants()
                     .OfType<JObject>()
                     .Where(x => x["command"] != null).FirstOrDefault();
                string command = this.result.command + " " + this.result.args[0] + " " + this.result.args[1] + " " + this.result.args[2];

                string[] commands = { this.directory[0].ToString() + this.directory[1].ToString(), @"cd " + directory.Substring(0, directory.Length - 19), command };
                this.commands = commands;
                File.WriteAllLines("coms.bat", this.commands);

                this.sizeLogs = new System.IO.FileInfo(directory.Substring(0, directory.Length - 19) + @"\logs.txt").Length;
                metroComboBox1.Items.Add(this.result.label);
            }
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void metroComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (metroComboBox2.SelectedItem.ToString() == "Detection of change in assembly")
            {
                this.Changes = true;
            }
            else if (metroComboBox2.SelectedItem.ToString() == "The beginning of a new minute")
            {
                this.NewMinute = true;
            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (this.directory != null)
            {
                using (StreamWriter sw = new StreamWriter(directory.Substring(0, directory.Length - 19) + @"\logs.txt", false, System.Text.Encoding.Default))
                {
                    sw.WriteLineAsync("-------Logs--------");
                }
                metroProgressSpinner1.Visible = true;
                metroProgressSpinner1.Spinning = true;
                metroLabel3.Visible = true;
                if (Changes)
                {
                    timer2.Start();
                }
                else if (NewMinute)
                {
                    timer1.Start();
                }
            }
            else
            {
                MessageBox.Show("Please, choose file!");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Process.Start("coms.bat");
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            timer2.Stop();
            metroProgressSpinner1.Spinning = false;
        }

        private void metroProgressSpinner1_Click(object sender, EventArgs e)
        {

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            fileSystemWatcher1.Path = directory.Substring(0, directory.Length - 19);
            fileSystemWatcher1.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.Attributes | NotifyFilters.DirectoryName;
            fileSystemWatcher1.Filter = "";
            fileSystemWatcher1.Changed += new FileSystemEventHandler(LogFileSystemChanges);
            fileSystemWatcher1.Created += new FileSystemEventHandler(LogFileSystemChanges);
            fileSystemWatcher1.Deleted += new FileSystemEventHandler(LogFileSystemChanges);
            fileSystemWatcher1.Renamed += new RenamedEventHandler(LogFileSystemRenaiming);
            fileSystemWatcher1.Error += new ErrorEventHandler(LogBufferError);


            var sizeLog = new System.IO.FileInfo(directory.Substring(0, directory.Length - 19) + @"\logs.txt").Length;
            if (sizeLog != this.sizeLogs)
            {
                this.sizeLogs = sizeLog;
                Process.Start("coms.bat");
            }

            fileSystemWatcher1.EnableRaisingEvents = true;
        }

        void LogBufferError(object sender, ErrorEventArgs e)
        {
            string log = string.Format("{0:G} internal buffer overflowed", DateTime.Now);
            using (StreamWriter sw = new StreamWriter(directory.Substring(0, directory.Length - 19) + @"\logs.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(log);
            }

        }
        void LogFileSystemRenaiming(object sender, RenamedEventArgs e)
        {
            string log = string.Format("{0:G} | {1} Renaimed file {2}", DateTime.Now, e.FullPath, e.OldName);
            using (StreamWriter sw = new StreamWriter(directory.Substring(0, directory.Length - 19) + @"\logs.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(log);
            }
        }
        void LogFileSystemChanges(object sender, FileSystemEventArgs e)
        {
            string log = string.Format("{0:G} | {1} | {2}", DateTime.Now, e.FullPath, e.ChangeType);
            using (StreamWriter sw = new StreamWriter(directory.Substring(0, directory.Length - 19) + @"\logs.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(log);
            }
        }
    }
}
