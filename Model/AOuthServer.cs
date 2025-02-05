using JohnUtilities.Classes;
using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    public class OAuthServer
    {
        private HttpListener _listener;
        private readonly string _redirectUri = "http://localhost:5000/auth/patreon/callback"; // Local server address
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public async Task<string> StartServerAsync()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(_redirectUri + "/");

                _listener.Start();

                // Start listening for the OAuth callback in a background task
                var tokenTask = ListenForCallback(_cancellationTokenSource.Token);

                // Wait for the callback to be received and token to be processed
                string token = await tokenTask; // Await the task that processes the token

                // Return the token once it's received
                return token;
            }
            catch (Exception ex)
            {
                Logging.WriteLogLine("Error: " + ex.Message);
                return "";
            }
        }

        private async Task<string> ListenForCallback(CancellationToken cancellationToken)
        {
            try
            {
                // Handle incoming requests
                while (!cancellationToken.IsCancellationRequested)
                {
                    var context = await _listener.GetContextAsync();

                    // Process the request and extract the token from the query
                    if (context.Request.Url.AbsolutePath.Equals("/auth/patreon/callback"))
                    {
                        // Get the authorization code or token from the query parameters
                        var queryParams = context.Request.QueryString;
                        string authorizationCode = queryParams["token"];

                        // Here, you would exchange the code for a token if needed
                        // Or use the token directly if it's in the query string
                        string token = authorizationCode;  // For this example, we assume the code is the token

                        // Send response back to the browser
                        string responseHtml = "<html><body><h1>Authentication Complete!</h1></body></html>";
                        byte[] buffer = Encoding.UTF8.GetBytes(responseHtml);
                        context.Response.ContentLength64 = buffer.Length;
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                        context.Response.OutputStream.Close();

                        // Return the token to the calling method
                        return token;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle error (optional)
                Logging.WriteLogLine("Error: " + ex.Message);
            }

            return null;
        }
    }
}
