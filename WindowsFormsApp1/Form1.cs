using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
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
        private int seconds;
        private Process process = new Process();
        
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
                this.directory = openFileDialog1.FileName.Substring(0, openFileDialog1.FileName.Length - 19);
                metroLabel5.Text = this.directory;
                metroLabel5.Visible = true;

                string taskText = System.IO.File.ReadAllText(openFileDialog1.FileName);
                var updates = (JContainer)JObject.Parse(taskText)["tasks"];
                this.result = updates.Descendants()
                     .OfType<JObject>()
                     .Where(x => x["command"] != null).FirstOrDefault();
                string command = this.result.command + " " + this.result.args[0] + " " + this.result.args[1] + " " + this.result.args[2];

                string[] commands = { this.directory[0].ToString() + this.directory[1].ToString(), @"cd " + this.directory, command,"TYPE " + this.directory + @"\logs.txt", "pause" };
                this.commands = commands;
                File.WriteAllLines("coms.bat", this.commands);

                this.process.StartInfo.FileName = "coms.bat";
                using (StreamWriter sw = new StreamWriter(this.directory + @"\logs.txt", false, System.Text.Encoding.Default))
                {
                    sw.WriteLineAsync("-------Logs--------");
                }

                //metroComboBox1.Items.Add(this.result.label);
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
                this.NewMinute = false;
            }
            else if (metroComboBox2.SelectedItem.ToString() == "The beginning of a new minute")
            {
                this.NewMinute = true;
                this.Changes = false;

            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.FileName != null)
            {
                metroProgressSpinner1.Visible = true;
                metroProgressSpinner1.Spinning = true;
                metroLabel3.Visible = true;
                metroLabel3.Text = "Task in progress";
                if (Changes)
                {
                    fileSystemWatcher1.Path = this.directory;
                    fileSystemWatcher1.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    fileSystemWatcher1.Filter = "";
                    fileSystemWatcher1.Changed += new FileSystemEventHandler(LogFileSystemChanges);
                    fileSystemWatcher1.Created += new FileSystemEventHandler(LogFileSystemChanges);
                    fileSystemWatcher1.Deleted += new FileSystemEventHandler(LogFileSystemChanges);
                    fileSystemWatcher1.Renamed += new RenamedEventHandler(LogFileSystemRenaiming);
                    fileSystemWatcher1.Error += new ErrorEventHandler(LogBufferError);
                    fileSystemWatcher1.EnableRaisingEvents = true;
                }
                else if (NewMinute)
                {
                    this.seconds = DateTime.Now.Second;
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
            this.seconds++;
            if (this.seconds == 60)
            {
                Process.Start("coms.bat");
                string log = "New minute - " + DateTime.Now;
                using (StreamWriter sw = new StreamWriter(this.directory + @"\logs.txt", true, System.Text.Encoding.Default))
                {
                    sw.WriteLineAsync(log);
                }
                this.seconds = 0;
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            this.process.Close();
            metroProgressSpinner1.Spinning = false;
            metroProgressSpinner1.Visible = false;
            metroLabel3.Text = "Task is not active";
        }


        void LogBufferError(object sender, ErrorEventArgs e)
        {
            string log = string.Format("{0:G} internal buffer overflowed", DateTime.Now);
            using (StreamWriter sw = new StreamWriter(this.directory + @"\logs.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(log);
            }

            this.process.Start();
        }
        void LogFileSystemRenaiming(object sender, RenamedEventArgs e)
        {
            string log = string.Format("{0:G} | {1} Renaimed file {2}", DateTime.Now, e.FullPath, e.OldName);
            using (StreamWriter sw = new StreamWriter(this.directory + @"\logs.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(log);
            }

            this.process.Start();
        }
        void LogFileSystemChanges(object sender, FileSystemEventArgs e)
        {
            string log = string.Format("{0:G} | {1} | {2}", DateTime.Now, e.FullPath, e.ChangeType);
            using (StreamWriter sw = new StreamWriter(this.directory + @"\logs.txt", true, System.Text.Encoding.Default))
            {
                sw.WriteLineAsync(log);
            }

            this.process.Start();
        }
    }
}
