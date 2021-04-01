using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CommandLine;
using fastJSON;

namespace registrationbackup
{
    class Program
    {
        public class Options
        {
            [Option("consoleEncoding", Required = false, Default = "shift_jis", HelpText = "Console output encoding")]
            public string ConsoleEncoding { get; set; }

            [Option("controllerUrl", Required = true, HelpText = "URL to controller, e.g. http://controller123")]
            public string ControllerUrl { get; set; }

            [Option("controllerUsername", Required = false, Default = "mujin", HelpText = "Username to authenticate with controller")]
            public string ControllerUsername { get; set; }

            [Option("controllerPassword", Required = false, Default = "mujin", HelpText = "Password to authenticate with controller")]
            public string ControllerPassword { get; set; }

            [Option("scenePrimaryKey", Required = false, Default = "", HelpText = "If not supplied, will automatically determine from server")]
            public string ScenePrimaryKey { get; set; }

            [Option("ftpHost", Required = true, HelpText = "Hostname or IP address of FTP server")]
            public string FtpHost { get; set; }

            [Option("ftpPort", Required = false, Default = 21, HelpText = "Port number of FTP server")]
            public int FtpPort { get; set; }

            [Option("ftpUsername", Required = false, Default = "anonymous", HelpText = "Username to authenticate with FTP server")]
            public string FtpUsername { get; set; }

            [Option("ftpPassword", Required = false, Default = "", HelpText = "Password to authenticate with FTP server")]
            public string FtpPassword { get; set; }

            [Option("ftpPath", Required = false, Default = "/", HelpText = "Path on FTP server")]
            public string FtpPath { get; set; }

            [Option("backup", Required = false, Default = false, HelpText = "Backup registration object to FTP")]
            public bool Backup { get; set; }

            [Option("syncMasterFile", Required = false, Default = "", HelpText = "If supplied, will sync this master file on FTP, e.g. /somewhere/masterfile.txt")]
            public string MasterFilePath { get; set; }

            [Option("outputFilename", Required = false, Default = "", HelpText = "If supplied, will output to file specified, otherwise file will be named after task name")]
            public string OutputFilename { get; set; }
        }

        private static string taskType = "registration";

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(Run);
        }

        private static void Run(Options options)
        {
            Console.OutputEncoding = Encoding.GetEncoding(options.ConsoleEncoding);

            string command = "";
            if (options.Backup)
            {
                if (command.Length > 0)
                {
                    throw new ArgumentException("Cannot specify both --backup and --syncMasterFile at the same time.");
                }
                command = "Backup";
            }
            if (options.MasterFilePath.Length > 0)
            {
                if (command.Length > 0)
                {
                    throw new ArgumentException("Cannot specify both --backup and --syncMasterFile at the same time.");
                }
                command = "SyncMasterFile";
            }
            if (command.Length == 0)
            {
                throw new ArgumentException("Have to specify either --backup or --syncMasterFile in command line.");
            }

            string taskName = String.Format("registration-{0}-{1}", command.ToLower(), DateTime.Now.ToString(@"yyyyMMdd-HHmmss"));
            Console.WriteLine("Using task name: {0}", taskName);

            if (options.OutputFilename.Length == 0)
            {
                options.OutputFilename = String.Format("{0}.json", taskName);
            }

            Mujin.ControllerClient controllerClient = new Mujin.ControllerClient(options.ControllerUsername, options.ControllerPassword, options.ControllerUrl);

            // cancel previous running jobs
            CancelPreviousJobs(controllerClient, options);

            // get scenepk
            if (options.ScenePrimaryKey.Length == 0)
            {
                options.ScenePrimaryKey = controllerClient.GetCurrentSceneURI().Substring("mujin:/".Length);
                Console.WriteLine("Using current scene: {0}", options.ScenePrimaryKey);
            }

            // delete previous task
            DeletePreviousTasks(controllerClient, options);

            // create new task
            string taskPrimaryKey = CreateTask(controllerClient, options, command, taskName);
            Console.WriteLine("Task created: {0}", taskPrimaryKey);

            // trigger task to execute
            string jobPrimaryKey = RunTaskAsync(controllerClient, options, taskPrimaryKey);
            Console.WriteLine("Task started: {0}", jobPrimaryKey);

            // wait for job
            WaitForJob(controllerClient, options, jobPrimaryKey);
            Console.WriteLine("Task finished");

            // wait until result is written
            Dictionary<string, object> result = WaitForTaskResult(controllerClient, options, taskPrimaryKey);
            Console.WriteLine("Result fetched");

            // write to file
            System.IO.File.WriteAllText(options.OutputFilename, JSON.ToNiceJSON(result));
            Console.WriteLine("Output written to file: {0}", options.OutputFilename);
        }

        private static Dictionary<string, object> PrepareTaskData(Options options, string command, string taskName)
        {
            Dictionary<string, object> fileStorageInfo = new Dictionary<string, object>()
            {
                { "type", "ftp" },
                { "username", options.FtpUsername },
                { "password", options.FtpPassword },
                { "host", options.FtpHost },
                { "port", options.FtpPort },
                { "remotePath", options.FtpPath },
            };
            Dictionary<string, object> taskparameters = new Dictionary<string, object>()
            {
                { "command", command },
                { "fileStorageInfo", fileStorageInfo },
            };
            if (options.MasterFilePath.Length > 0)
            {
                taskparameters["remoteMasterFilePath"] = options.MasterFilePath;
            }
            return new Dictionary<string, object>()
            {
                { "tasktype", taskType },
                { "name", taskName },
                { "taskparameters", taskparameters },
            };
        }

        private static void CancelPreviousJobs(Mujin.ControllerClient controllerClient, Options options)
        {
            Dictionary<string, object> jobs = controllerClient.GetJobs();
            foreach (Dictionary<string, object> job in (List<object>)jobs["objects"])
            {
                if (job["description"].ToString().Contains("/registration-"))
                {
                    controllerClient.DeleteJob(job["pk"].ToString());
                    Console.WriteLine("Canceled previous job: {0}", job["pk"].ToString());
                }
            }
        }

        private static void DeletePreviousTasks(Mujin.ControllerClient controllerClient, Options options)
        {
            Dictionary<string, object> sceneTasks = controllerClient.GetSceneTasks(options.ScenePrimaryKey);
            foreach (Dictionary<string, object> sceneTask in (List<object>)sceneTasks["objects"])
            {
                if ((string)sceneTask["tasktype"] == taskType)
                {
                    try
                    {
                        controllerClient.DeleteSceneTask(options.ScenePrimaryKey, sceneTask["pk"].ToString());
                        Console.WriteLine("Deleted previous task: {0}", sceneTask["pk"].ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Faield to delete previous task: {0}: {0}", sceneTask["pk"].ToString(), ex);
                    }
                }
            }
        }

        private static string CreateTask(Mujin.ControllerClient controllerClient, Options options, string command, string taskName)
        {
            Dictionary<string, object> createSceneTaskResponse = controllerClient.CreateSceneTask(options.ScenePrimaryKey, PrepareTaskData(options, command, taskName));
            return createSceneTaskResponse["id"].ToString();
        }

        private static string RunTaskAsync(Mujin.ControllerClient controllerClient, Options options, string taskPrimaryKey)
        {
            Dictionary<string, object> runSceneTaskResponse = controllerClient.RunScenetaskAsync(options.ScenePrimaryKey, taskPrimaryKey);
            return runSceneTaskResponse["jobpk"].ToString();
        }

        private static Dictionary<string, object> LookupJob(Mujin.ControllerClient controllerClient, Options options, string jobPrimaryKey)
        {
            Dictionary<string, object> jobs = controllerClient.GetJobs();
            foreach (Dictionary<string, object> job in (List<object>)jobs["objects"])
            {
                if (job["pk"].ToString() == jobPrimaryKey)
                {
                    return job;
                }
            }
            return null;
        }

        private static void WaitForJob(Mujin.ControllerClient controllerClient, Options options, string jobPrimaryKey)
        {
            string jobStatus = "";
            double jobProgress = 0.0;
            bool jobSeen = false;
            DateTime startTime = DateTime.Now;
            while (true)
            {
                Dictionary<string, object> job = LookupJob(controllerClient, options, jobPrimaryKey);
                if (job == null)
                {
                    // job has been seen before, so must be done now
                    if (jobSeen)
                    {
                        return;
                    }

                    // perhaps job finished too quickly
                    if (DateTime.Now - startTime > TimeSpan.FromSeconds(2))
                    {
                        return;
                    }

                    // wait a bit more
                    Thread.Sleep(200);
                    continue;
                }

                // we have now seen this job at least once
                jobSeen = true;
                double newProgress = double.Parse(job["progress"].ToString());
                string newStatus = job["status_text"].ToString();
                newProgress = Math.Min(1.0, Math.Max(jobProgress, newProgress));

                if (newProgress != jobProgress || newStatus != jobStatus)
                {
                    jobProgress = newProgress;
                    jobStatus = newStatus;
                    Console.WriteLine("Progress {0}%: {1}", Math.Round(jobProgress * 100.0), jobStatus);
                }

                string status = job["status"].ToString();
                switch (status)
                {
                    case "succeeded":
                        return;
                    case "lost":
                    case "preempted":
                        throw new Exception(String.Format("Job has stopped unexpectedly: {0}", status));
                }

                Thread.Sleep(200);
            }
        }

        private static Dictionary<string, object> GetTaskResult(Mujin.ControllerClient controllerClient, Options options, string taskPrimaryKey)
        {
            Dictionary<string, object> task = controllerClient.GetSceneTask(options.ScenePrimaryKey, taskPrimaryKey);
            List<object> results = (List<object>)task["binpickingresults"];
            if (results.Count == 0)
            {
                return null;
            }
            Dictionary<string, object> result = (Dictionary<string, object>)results[0];
            return (Dictionary<string, object>)controllerClient.GetJsonMessage(Mujin.HttpMethod.GET, (string)result["uri"]);
        }

        private static Dictionary<string, object> WaitForTaskResult(Mujin.ControllerClient controllerClient, Options options, string taskPrimaryKey)
        {
            DateTime startTime = DateTime.Now;
            while (true)
            {
                Dictionary<string, object> result = GetTaskResult(controllerClient, options, taskPrimaryKey);
                if (result != null)
                {
                    return result;
                }

                if (DateTime.Now - startTime > TimeSpan.FromSeconds(5))
                {
                    throw new Exception("Timed out waiting for task result");
                }

                Thread.Sleep(200);
            }
        }
    }
}
