using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using fastJSON;

// This is the code for your desktop app.
// Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.

namespace RegistrationBackup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (System.IO.File.Exists("config.json"))
            {
                Dictionary<string, object> config = (Dictionary<string, object>)JSON.Parse(System.IO.File.ReadAllText("config.json"));

                controllerurl_input.Text = config["controllerurl"].ToString();
                username_input.Text = config["controllerusername"].ToString();
                password_input.Text = config["controllerpassword"].ToString();
                ftpusername_input.Text = config["ftpusername"].ToString();
                ftppassword_input.Text = config["ftppassword"].ToString();
                ftphost_input.Text = config["ftphost"].ToString();
                ftpport_input.Text = config["ftpport"].ToString();
                ftpremotePath_input.Text = config["ftpremotePath"].ToString();
                masterFilePath_input.Text = config["masterFilePath"].ToString();
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            Button button1 = (Button)sender;
            button1.Enabled = false;
            richTextBox1.Text = "Starting backup task ...";
            try
            {

                string command = "Backup";
                if (radioButton2_syncmasterfile.Checked)
                {
                    command = "SyncMasterFile";
                }
                string controllerurl = controllerurl_input.Text;
                string controllerusername = username_input.Text;
                string controllerpassword = password_input.Text;
                string ftpusername = ftpusername_input.Text;
                string ftppassword = ftppassword_input.Text;
                string ftphost = ftphost_input.Text;
                string ftpport = ftpport_input.Text;
                string ftpremotePath = ftpremotePath_input.Text;
                string masterFilePath = masterFilePath_input.Text;

                Dictionary<string, string> config = new Dictionary<string, string>() {
                    { "controllerurl", controllerurl},
                    { "controllerusername", controllerusername},
                    { "controllerpassword", controllerpassword},
                    { "ftpusername", ftpusername},
                    { "ftppassword", ftppassword},
                    { "ftphost", ftphost},
                    { "ftpport", ftpport},
                    { "ftpremotePath", ftpremotePath},
                    { "masterFilePath", masterFilePath}
                };
                System.IO.File.WriteAllText("config.json", JSON.ToNiceJSON(config));

                // initialize mujincontrollerclient
                Mujin.ControllerClient c = new Mujin.ControllerClient(controllerusername, controllerpassword, controllerurl);

                // get scene pk
                string scenepk = c.GetCurrentSceneURI().Substring("mujin:/".Length);

                // create a new task
                Dictionary<string, object> taskdata = new Dictionary<string, object>();
                if (command == "Backup")
                {
                    taskdata = new Dictionary<string, object>()
                    {
                        {
                            "taskparameters", new Dictionary<string, object>() {
                                { "fileStorageInfo", new Dictionary<string, object>() {
                                    { "type", "ftp" },
                                    { "username", ftpusername},
                                    { "password", ftppassword },
                                    { "host", ftphost },
                                    { "port", ftpport },
                                    { "remotePath", ftpremotePath},
                                }
                                },
                                {"command", "Backup" }
                            }
                        },
                        { "name", String.Format("registration-backup-{0}", DateTime.Now.ToString(@"yyyyMMdd-hhmmss")) },
                        { "tasktype", "registration" },
                    };
                }
                else if (command == "SyncMasterFile")
                {
                    taskdata = new Dictionary<string, object>()
                    {
                        {
                            "taskparameters", new Dictionary<string, object>() {
                                { "fileStorageInfo", new Dictionary<string, object>() {
                                    { "type", "ftp" },
                                    { "username", ftpusername},
                                    { "password", ftppassword },
                                    { "host", ftphost },
                                    { "port", ftpport },
                                    { "remotePath", ftpremotePath},
                                }
                                },
                                {"command", "SyncMasterFile" },
                                {"remoteMasterFilePath", masterFilePath }
                            }
                        },
                        { "name", String.Format("registration-syncmasterfile-{0}", DateTime.Now.ToString(@"yyyyMMdd-hhmmss")) },
                        { "tasktype", "registration" },
                    };
                }

                // delete previous task
                Dictionary<string, object> sceneTasks = c.GetSceneTasks(scenepk);
                foreach (Dictionary<string, object> task in (List<object>)sceneTasks["objects"])
                {

                    if ((string)task["tasktype"] == "registration")
                    {
                        try
                        {
                            c.DeleteSceneTask(scenepk, (string)task["pk"]);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Faield to delete previous task: {0}", ex);
                        }

                    }
                }
                Dictionary<string, object> response = c.CreateSceneTask(scenepk, taskdata);
                string taskpk = (string)response["id"];
                // trigger task to execute
                Dictionary<string, object> runTaskResponse = c.RunScenetaskAsync(scenepk, taskpk);
                await Progress(scenepk, taskpk, (string)runTaskResponse["jobpk"]);
                MessageBox.Show(button1, "Done!");
            }
            catch (Exception ex)
            {
                richTextBox1.Text = ex.ToString();
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        private async Task Progress(string scenepk, string taskpk, string jobpk)
        {
            string controllerurl = controllerurl_input.Text;
            string controllerusername = username_input.Text;
            string controllerpassword = password_input.Text;

            // initialize mujincontrollerclient
            Mujin.ControllerClient c = new Mujin.ControllerClient(controllerusername, controllerpassword, controllerurl);

            int progress = 0;
            bool jobSeen = false;
            DateTime startTime = DateTime.Now;
            while (true)
            {
                await Task.Delay(200);

                Dictionary<string, object> job = null;
                Dictionary<string, object> jobs = c.GetJobs();
                foreach (Dictionary<string, object> j in (List<object>)jobs["objects"])
                {
                    if (j["pk"].ToString() == jobpk)
                    {
                        job = j;
                        break;
                    }
                }

                if (job == null)
                {
                    // job has been seen before, so must be done now
                    if (jobSeen)
                    {
                        break;
                    }

                    // perhaps job finished too quickly
                    if (DateTime.Now - startTime > TimeSpan.FromSeconds(2))
                    {
                        break;
                    }

                    // wait a bit more
                    continue;
                }

                jobSeen = true;
                richTextBox1.Text = job["status_text"].ToString();

                double progressRaw = double.Parse(job["progress"].ToString());
                if (progressRaw < 0)
                {
                    progress = 100;
                    progressBar1.Invoke(new Action(() => {
                        progressBar1.Value = progress;
                    }));
                    break;
                }
                else
                {
                    progress = Math.Min(100, Math.Max(progress, (int)Math.Round(progressRaw * 100)));
                    progressBar1.Invoke(new Action(() => {
                        progressBar1.Value = progress;
                    }));
                }
            }

            int objectCount = 0;
            Dictionary<string, object> task = c.GetSceneTask(scenepk, taskpk);
            List<object> resultsInfoList = (List<object>)task["binpickingresults"];
            if (resultsInfoList.Count > 0)
            {
                Dictionary<string, object> resultInfo = (Dictionary<string, object>)resultsInfoList[0];
                Dictionary<string, object> result = c.GetJsonMessage(Mujin.HttpMethod.GET, (string)resultInfo["uri"]);
                Console.WriteLine("{0}", JSON.ToNiceJSON(result));

                if (result.ContainsKey("output"))
                {
                    Dictionary<string, object> output = (Dictionary<string, object>)result["output"];
                    if (output.ContainsKey("objects"))
                    {
                        List<object> objects = (List<object>)output["objects"];
                        objectCount = objects.Count;
                    }
                }
                
            }
            richTextBox1.Text = String.Format("Backed up {0} objects.", objectCount);
            progressBar1.Invoke(new Action(() =>
            {
                progressBar1.Value = 100;
            }));
        }

        private void Label2_Click(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void ProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void Ftpusername_TextChanged(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Label4_Click(object sender, EventArgs e)
        {

        }

        private void OpenFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Label7_Click(object sender, EventArgs e)
        {

        }

        private void Password_input_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
