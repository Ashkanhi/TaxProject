using NPSTools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceProcess;


namespace NPSServiceWatcher
{
    public partial class Service1 : ServiceBase
    {
        private Timer _SynceTimer;
        private int SyncTimerPeriod = 10;
        private static string configFile = AppDomain.CurrentDomain.BaseDirectory + "Conf.ini";
        ServiceController sc;
        ClsWatcherConfigDTO config;
        int CountOfStop;
        int CountOfStartPending;
        public Service1()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            try
            {
                CountOfStop = 0;
                CountOfStartPending = 0;
                   config = ClsTools.DeSerializeObject<ClsWatcherConfigDTO>(configFile);
                SyncTimerPeriod = config.TimerCheckInSecond * 1000;
                _SynceTimer = new Timer(SynceTimerCallBack, null, SyncTimerPeriod, Timeout.Infinite);
                sc = new ServiceController(config.ServiceNameToWatch);

            }
            catch (Exception e)
            {


            }

        }

        [Obsolete]
        private void SynceTimerCallBack(object state)
        {
            try
            {
                sc = new ServiceController(config.ServiceNameToWatch);

                CheckMode(sc.Status.ToString());
            }
            catch
            {
                //ignored
            }
            finally
            {
                _SynceTimer.Change(SyncTimerPeriod, Timeout.Infinite);
            }

        }

        [Obsolete]
        private void CheckMode(string status)
        {
            DateTime dt12 = new DateTime(DateTime.Now.Year, DateTime.Now.Month
                , DateTime.Now.Day,12,0,0);
          
            ClsTools.WriteWathcerLog(status);
            if (status == "Running")
            {
                ClsTools.WriteWathcerLog("Running Hooora...");

            };
            if (status == "Stopped")
            {
                CountOfStop++;
                if (CountOfStop >= 3)
                {
                    ClsTools.WriteWathcerLog("Stopped Service - Run " + config.ExcuteFilePath);
                    RunExFile(config.ExcuteFilePath);
                    CountOfStop = 0;
                }
               

            };
            if (status == "StartPending")
            {
                  CountOfStartPending++;
                if (CountOfStartPending >= 5)
                {
                    ClsTools.WriteWathcerLog("StartPending Service - Run " + config.ExcuteFilePath);
                    RunExFile(config.ExcuteFilePath);
                    CountOfStartPending = 0;
                }
              
            };
            if (status == "StopPending")
            {

            };
            if (status == "Paused")
            {

            };
            _SynceTimer.Change(SyncTimerPeriod, Timeout.Infinite);
        }
        public static void RunExFile(string path)
        {
            
            Process pinfo = new Process();
            pinfo.StartInfo = new ProcessStartInfo();
            pinfo.StartInfo.CreateNoWindow = true;
            pinfo.StartInfo.FileName = path;
            pinfo.StartInfo.ErrorDialog = false;
            pinfo.StartInfo.UseShellExecute = false;
            pinfo.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pinfo.Start();
            ClsTools.WriteWathcerLog("Success Run " + path);

        }
        protected override void OnStop()
        {
        }
    }
}
