using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.Net.NetworkInformation;
using GHIElectronics.NETMF.Net;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using Idiot.DataTypes;
using System.Collections;
using System.Threading;
using System.IO;
using System.Resources;
using Idiot.Properties;
using System.Text;

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

        private string username;
        private string hubId;
        private string projectName;
        private Credentials credentials;

        /// <summary>
        /// Initialize connection
        /// </summary>
        public DataPointService(Credentials credentials, string projectName, string hubId, int intervalMilliseconds)
        {
            // Configure Internet connection
            byte[] mac = { 0x00, 0x26, 0x1C, 0x7B, 0x29, 0xE8 };
            string hostname = "idiotsdk";
            WIZnet_W5100.Enable(SPI.SPI_module.SPI1, (Cpu.Pin)FEZ_Pin.Digital.Di10, (Cpu.Pin)FEZ_Pin.Digital.Di7, true);
            Dhcp.EnableDhcp(mac, hostname);
            Dhcp.RenewDhcpLease();

            // Set interval between requests
            this.RequestInterval = intervalMilliseconds;

            this.username = credentials.Username;
            this.credentials = credentials;
            this.projectName = projectName;
            this.hubId = hubId;

            // TODO: Display info on screen module
        }

        /// <summary>
        /// Send a data point to the server
        /// </summary>
        /// <returns>True if the request was successful</returns>
        private bool sendDataPoint()
        {
            if (this.dataPoints.Count > 0)
            {
                // There are data points waiting to be sent
                IDataPoint dataPoint = (IDataPoint)this.dataPoints.Dequeue();    

                string requestUriString = UrlBuilder.Join(Resources.GetString(Resources.StringResources.DevelopmentAppUrl), "api", this.username, this.projectName, "hubs", this.hubId, "datapoints");
                
                Stream stream = null;
                HttpWebResponse response = null;
                HttpWebRequest request = null;

                // Create request
                using (request = (HttpWebRequest)HttpWebRequest.Create(requestUriString))
                {
                    request.Method = "POST";
                    request.ContentType = "application/json";
                    request.Headers.Add("Authorization", this.credentials.AuthorizationHeader);

                    // Encode request body
                    byte[] buffer = Encoding.UTF8.GetBytes(dataPoint.toJson());
                    request.ContentLength = buffer.Length;

                    // Get stream for writing to request body
                    stream = request.GetRequestStream();
                    stream.Write(buffer, 0, buffer.Length);

                    // Get response
                    using (response = (HttpWebResponse)request.GetResponse())
                    {
                        Debug.Print(response.StatusCode.ToString());

                        if (response.StatusCode == HttpStatusCode.Created)
                        {
                            // The server repsonded with 201 Created
                            // TODO: Do something with the response
                            return true;
                        }

                        // The server did not respond with the expected status
                        // TODO: Identify problem
                        return false;
                    }
                }
            }
            return true;
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
