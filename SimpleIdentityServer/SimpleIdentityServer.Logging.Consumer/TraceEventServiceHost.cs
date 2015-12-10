using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.ServiceProcess;

using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Etw;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Etw.Configuration;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SimpleIdentityServer.Logging.Consumer
{
    public class TraceEventServiceHost : ServiceBase
    {
        // Capture non-transient errors from internal SLAB EventSource
        private EventListener slabNonTransientErrors;

        private TraceEventService service;
        private bool consoleMode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceEventServiceHost" /> class.
        /// </summary>
        public TraceEventServiceHost()
        {
            this.Initialize();
        }

        // For running in Console mode
        internal void Start()
        {
            this.consoleMode = true;
            this.OnStart(null);
        }

        private void Initialize()
        {
            this.AutoLog = false;
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                string configFile = "SemanticLogging.xml";
                var configuration = TraceEventServiceConfiguration.Load(configFile, monitorChanges: true);
                this.service = new TraceEventService(configuration);
                configuration.Settings.PropertyChanged += this.OnTraceEventServiceSettingsChanged;
                this.service.Start();
            }
            catch (Exception e)
            {
                // log and rethrow to notify SCM
                if (!this.consoleMode)
                {
                    this.EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
                }

                throw;
            }
        }

        private void OnTraceEventServiceSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            // We should recycle on any settings changes
            if (this.consoleMode)
            {
                Console.WriteLine("recycling");
            }

            // schedule recycle to decouple from current context
            Task.Run(() => this.RecycleService());
        }
        private void RecycleService()
        {
            try
            {
                this.OnStop();
                this.OnStart(null);
            }
            catch (Exception e)
            {
                if (this.consoleMode)
                {
                    Console.WriteLine(e.ToString());
                }
                else
                {
                    this.EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
                }
            }
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            // Short-cut if TraceEventService was not started or is already stopped.
            if (this.service == null)
            {
                return;
            }

            try
            {
                this.service.Dispose();
                this.service = null;
            }
            catch (Exception e)
            {
                // Notify in console error handling
                if (this.consoleMode)
                {
                    throw;
                }

                // Do not rethrow in Service mode so SCM can mark this service as stopped (this will allow to uninstall the service properly) 
                this.EventLog.WriteEntry(e.ToString(), EventLogEntryType.Error);
            }
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the <see cref="T:System.ServiceProcess.ServiceBase" />.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            this.OnStop();
            base.Dispose(disposing);
        }
    }
}
