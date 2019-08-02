using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
                try
                {
                    Dictionary<string, string> config = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText("config.json"));
                    
                    controllerurl_input.Text = config["controllerurl"];
                    username_input.Text = config["controllerusername"];
                    password_input.Text = config["controllerpassword"];
                    ftpusername_input.Text = config["ftpusername"];
                    ftppassword_input.Text = config["ftppassword"];
                    ftphost_input.Text = config["ftphost"];
                    ftpport_input.Text = config["ftpport"];
                    ftpremotePath_input.Text = config["ftpremotePath"];
                    masterFilePath_input.Text = config["masterFilePath"];
                } catch(Exception error)
                {
                   
                }
                
            }
        }
        private async void button1_Click(object sender, EventArgs e)
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
            System.IO.File.WriteAllText("config.json", JsonConvert.SerializeObject(config, Formatting.Indented));

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
                    { "name", "ftpbackuptest" },
                    { "tasktype", "registration" },
                };
            }
            else if(command == "SyncMasterFile")
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
                    { "name", "syncmasterfile3" },
                    { "tasktype", "registration" },
                };
            }
           

            Dictionary<string, object> sceneTasks = c.GetSceneTasks(scenepk);

            foreach(Dictionary<string, object> task in (List<object>)sceneTasks["objects"])
            {
                
                if((string)task["tasktype"] == "registration")
                {
                    try
                    {
                        c.DeleteSceneTask(scenepk, (string)task["pk"]);
                    }
                    catch (Exception err)
                    {
                        Console.Write("Delete Scene Task Exception e {0}", err);
                    }

                }
            }
            Dictionary<string, object> response = c.CreateSceneTask(scenepk, taskdata);
            string taskpk = (string)response["id"];
            // trigger task to execute
            Dictionary<string, object> runTaskResponse = c.RunScenetaskAsync(scenepk, taskpk);
            await Progress(scenepk, taskpk, (string)runTaskResponse["jobpk"]);
            // MessageBox.Show("Done!");
        }
        private async Task Progress(string  scenepk, string taskpk, string jobpk)
        {
            string controllerurl = controllerurl_input.Text;
            string controllerusername = username_input.Text;
            string controllerpassword = password_input.Text;

            // initialize mujincontrollerclient
            Mujin.ControllerClient c = new Mujin.ControllerClient(controllerusername, controllerpassword, controllerurl);

            int progress = 0;
            while (true)
            {
                await Task.Delay(500);
                try
                {
                    Dictionary<string, object> job = c.GetJob(jobpk);
                    richTextBox1.Text = job["status_text"].ToString();
                    progress = Math.Min(100, Math.Max(progress, (int)Math.Round(double.Parse(job["progress"].ToString()) * 100)));
                    progressBar1.Invoke(new Action(() =>
                    {
                        progressBar1.Value = progress;
                    }));
                } catch(Exception err){
                    break;
                }                
            }

            Dictionary<string, object> task = c.GetSceneTask(scenepk, taskpk);
            List<object> resultsInfoList = (List<object>)task["binpickingresults"];
            if (resultsInfoList.Count() == 0)
            {
                return;
            }
            Dictionary<string, object> resultInfo = (Dictionary<string, object>)resultsInfoList[0];
            Dictionary<string, object> result = c.Get((string)resultInfo["uri"]);

            richTextBox1.Text = JsonConvert.SerializeObject(result, Formatting.Indented);
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
        private JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };

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
