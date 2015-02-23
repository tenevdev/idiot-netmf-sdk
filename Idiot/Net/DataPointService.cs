using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.Net.NetworkInformation;
using GHIElectronics.NETMF.Net;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using Idiot.DataTypes;
using System.Collections;
using System.Threading;

namespace Idiot.Net
{
    /// <summary>
    /// Send data points to the server using a background thread
    /// </summary>
    public class DataPointService
    {
        /// <summary>
        /// A collection for storing data points
        /// </summary>
        private Queue dataPoints = new Queue();

        /// <summary>
        /// The thread to use when sending requests and waiting for a response
        /// </summary>
        private Thread communicationThread = null;

        /// <summary>
        /// Initialize connection
        /// </summary>
        public DataPointService(int intervalMilliseconds)
        {
            // Configure Internet connection
            byte[] mac = { 0x00, 0x26, 0x1C, 0x7B, 0x29, 0xE8 };
            string hostname = "idiotsdk";
            WIZnet_W5100.Enable(SPI.SPI_module.SPI1, (Cpu.Pin)FEZ_Pin.Digital.Di10, (Cpu.Pin)FEZ_Pin.Digital.Di7, true);
            Dhcp.EnableDhcp(mac, hostname);
            Dhcp.RenewDhcpLease();

            // Set interval between requests
            this.RequestInterval = intervalMilliseconds;

            // TODO: Display info on screen module
        }

        /// <summary>
        /// Send a data point to the server
        /// </summary>
        /// <returns>True if the request was successful</returns>
        private bool sendDataPoint()
        {
            // TODO : Implement request-response
            return false;
        }

        /// <summary>
        /// Add a data point to be sent
        /// </summary>
        /// <param name="dataPoint">The data point to be sent</param>
        public void PushDataPoint(IDataPoint dataPoint)
        {
            this.dataPoints.Enqueue(dataPoint);
        }

        /// <summary>
        /// Open a thread for sending data points that have been pushed.
        /// You can keep pushing data points after the service has been started.
        /// </summary>
        public void Start()
        {
            if (!this.IsStarted)
            {
                this.communicationThread = new Thread(new ThreadStart(() =>
                {
                    this.IsStarted = true;

                    // Send data points while the request is successful and
                    // the communication thread is started
                    while (this.sendDataPoint() && this.IsStarted)
                    {
                        // Wait for the request interval before sending another request
                        Thread.Sleep(this.RequestInterval);
                    }

                    this.Stop();
                }));
            }

            // Do nothing if the thread had already started
        }

        /// <summary>
        /// Completely close the thread communicating with the server
        /// </summary>
        public void Stop()
        {
            this.communicationThread.Abort();
            this.IsStarted = false;
        }

        /// <summary>
        /// Suspend the thread communicating with the server
        /// </summary>
        public void Suspend() 
        {
            this.communicationThread.Suspend();
            this.IsStarted = false;
        }


        /// <summary>
        /// Resume the thread communicating with the server
        /// </summary>
        public void Resume() 
        {
            this.communicationThread.Resume();
            this.Start();
        }

        /// <summary>
        /// The period in milliseconds between two consecutive requests
        /// </summary>
        public int RequestInterval { get; set; }

        /// <summary>
        /// True if the service is currently sending data points
        /// </summary>
        public bool IsStarted { get; set; }
    }
}
