using Microsoft.SPOT;
using GHIElectronics.NETMF.Net.NetworkInformation;
using GHIElectronics.NETMF.Net;
using GHIElectronics.NETMF.FEZ;
using Microsoft.SPOT.Hardware;
using Idiot.DataTypes;
using System.Collections;
using System.Threading;
using System.IO;
using Idiot.Properties;
using System.Text;
using IndianaJones.NETMF.Json;
using GHIElectronics.NETMF.Net.Sockets;
using System;
using GHIElectronics.NETMF.Hardware;

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
        private bool isDevelopment = false;
        private Serializer serializer = new Serializer();

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

            // Set current time
            NTPTime("pool.ntp.org");

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
                DataPoint dataPoint = (DataPoint)this.dataPoints.Dequeue();

                string baseUrl = Resources.GetString(Resources.StringResources.WebAppUrl);

                if (this.isDevelopment)
                {
                    // Use development url in requests
                    baseUrl = Resources.GetString(Resources.StringResources.DevelopmentAppUrl);
                }

                string requestUriString = UrlBuilder.Join(baseUrl, "api", this.username, this.projectName, "hubs", this.hubId, "datapoints");
                
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
                    string json = this.serializer.Serialize(dataPoint);
                    byte[] buffer = Encoding.UTF8.GetBytes(json);
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
        public void PushDataPoint(DataPoint dataPoint)
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

                }));

                this.communicationThread.Start();
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

        public void ConfigureForDevelopment(string baseUrl) 
        {
            this.isDevelopment = true;
        }

        /// <summary>
        /// The period in milliseconds between two consecutive requests
        /// </summary>
        public int RequestInterval { get; set; }

        /// <summary>
        /// True if the service is currently sending data points
        /// </summary>
        public bool IsStarted { get; set; }

        /// <summary>
        /// Try to update both system and RTC time using the NTP protocol
        /// </summary>
        /// <param name="TimeServer">Time server to use, ex: pool.ntp.org</param>
        /// <param name="GmtOffset">GMT offset in minutes, ex: -240</param>
        /// <returns>Returns true if successful</returns>
        public static bool NTPTime(string TimeServer, int GmtOffset = 0)
        {
            Socket s = null;
            try
            {
                EndPoint rep = new IPEndPoint(Dns.GetHostEntry(TimeServer).AddressList[0], 123);
                s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                byte[] ntpData = new byte[48];
                Array.Clear(ntpData, 0, 48);
                ntpData[0] = 0x1B; // Set protocol version
                s.SendTo(ntpData, rep); // Send Request   
                if (s.Poll(30 * 1000 * 1000, SelectMode.SelectRead)) // Waiting an answer for 30s, if nothing: timeout
                {
                    s.ReceiveFrom(ntpData, ref rep); // Receive Time
                    byte offsetTransmitTime = 40;
                    ulong intpart = 0;
                    ulong fractpart = 0;
                    for (int i = 0; i <= 3; i++) intpart = (intpart << 8) | ntpData[offsetTransmitTime + i];
                    for (int i = 4; i <= 7; i++) fractpart = (fractpart << 8) | ntpData[offsetTransmitTime + i];
                    ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);
                    s.Close();
                    DateTime dateTime = new DateTime(1900, 1, 1) + TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);
                    Utility.SetLocalTime(dateTime.AddMinutes(GmtOffset));
                    RealTimeClock.SetTime(DateTime.Now);
                    return true;
                }
                s.Close();
            }
            catch
            {
                try { s.Close(); }
                catch { }
            }
            return false;
        }
    }

}
