using System;
using System.IO;
using System.Threading;
using System.Net;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        static HttpListener serviceListener;
        private Thread serviceListenerThread;
        delegate void StringArgReturningVoidDelegate(SelectedParcelInformation information);

        public Form1()
        {
            //NOTE - Browser control runs in IE7 mode by default.
            //https://blogs.msdn.microsoft.com/patricka/2015/01/12/controlling-webbrowser-control-compatibility/ 
            //Set the browser control to run in IE11 as opposed to IE7 mode - https://stackoverflow.com/questions/18333459/c-sharp-webbrowser-ajax-call

            setBrowserFeatureControl();

            InitializeComponent();
            this.ActiveControl = webBrowser1;

            //Create an HTTP listener to accept POST request for incoming data. The HTTP listener is started when the application is started.
            serviceListener = new HttpListener();
            serviceListener.Prefixes.Add("http://localhost:8989/netlistener/");
            serviceListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            serviceListener.Start();

            //Start the HTTP listener on an alternate thread to the application. In case there are issues with the application.
            this.serviceListenerThread = new Thread(new ParameterizedThreadStart(startServiceListener));
            serviceListenerThread.Start();

            textBoxChosenID.Text = "HTTP Service Listener Active!";
        }

        private void startServiceListener(object s)
        {
            while (true)
            {
                processRequest();
            }
        }

        private void processRequest()
        {
            var retrieveRequest = serviceListener.BeginGetContext(processRequestCallback, serviceListener);
            retrieveRequest.AsyncWaitHandle.WaitOne();
        }

        private void processRequestCallback(IAsyncResult requestResults)
        {
            HttpListenerContext serviceListenerContext = serviceListener.EndGetContext(requestResults);
            Thread.Sleep(1000);

            var readIncomingData = new StreamReader(serviceListenerContext.Request.InputStream, serviceListenerContext.Request.ContentEncoding).ReadToEnd();
            string decodedIncomingData = System.Web.HttpUtility.UrlDecode(readIncomingData);

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            SelectedParcelInformation parsedIncomingData = jsSerializer.Deserialize<SelectedParcelInformation>(decodedIncomingData);

            //Append HTTP headers to handle COR's issues when working with external URL's
            HttpListenerResponse serviceListenerResponse = serviceListenerContext.Response;
            serviceListenerResponse.AppendHeader("Access-Control-Allow-Origin", "*"); 
            serviceListenerResponse.AppendHeader("Access-Control-Allow-Methods", "*");
            serviceListenerResponse.AppendHeader("Access-Control-Allow-Headers", "*");

            serviceListenerResponse.StatusCode = 200;
            serviceListenerResponse.StatusDescription = "OK";

            string responseString = string.Format("Receieved - {0}", decodedIncomingData);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            serviceListenerResponse.ContentLength64 = buffer.Length;

            Stream outputStream = serviceListenerResponse.OutputStream;
            outputStream.Write(buffer, 0, buffer.Length);
            outputStream.Close();

            updateLabel(parsedIncomingData);
            serviceListenerResponse.Close();
        }

        private void updateLabel(SelectedParcelInformation parsedIncomingData)
        {

            if (this.textBoxChosenID.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(updateLabel);
                this.Invoke(d, new object[] { parsedIncomingData });
            }
            else
            {
                this.textBoxChosenID.Text = "Information Received - " + DateTime.Now.ToString() + "\r\n\r\nParcel Type -- \r\n" + parsedIncomingData.selectedParcelInformation[0].parcelType + "\r\nParcel ID -- " + parsedIncomingData.selectedParcelInformation[0].parcelID;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Uri myuri = new Uri(textBox1.Text);
            Uri myuri = new Uri(comboBox1.Text);
            webBrowser1.Url = myuri;
        }

        private void setBrowserFeatureControl()
        {
            //NOTE - Available Feature Controls - http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx
            var fileName = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            if (String.Compare(fileName, "devenv.exe", true) == 0 || String.Compare(fileName, "XDesProc.exe", true) == 0)
                return;

            setBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, 11000);
        }

        private void setBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, Microsoft.Win32.RegistryValueKind.DWord);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            Environment.Exit(0);
        }
    }
}

public class SelectedParcelInformation
{
    public System.Collections.Generic.List<ParcelInformation> selectedParcelInformation { get; set; }
}

public class ParcelInformation
{
    public string parcelID { get; set; }
    public string parcelType { get; set; }
}