using System.Text;
using GHIElectronics.NETMF.Net;

namespace Idiot.Net
{
    public class Credentials
    {
        private string password;

        public Credentials(string username, string password)
        {
            this.Username = username;
            this.password = password;

            this.AuthorizationHeader = this.toAuthorizationHeader();
        }

        /// <summary>
        /// Basic authorization header value to be used for server authentication
        /// </summary>
        public string AuthorizationHeader { get; private set; }

        private string toAuthorizationHeader()
        {
            return "Basic " + ConvertBase64.ToBase64String(Encoding.UTF8.GetBytes(this.Username + ":" + this.password));
        }

        public string Username { get; set; }
    }
}
