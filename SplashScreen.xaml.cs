using System;
using System.Windows;
using System.IO;
using System.Net;
using System.Windows.Threading;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO.Compression;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CanaryLauncherUpdate
{
	public partial class SplashScreen : Window
	{
		static readonly HttpClient httpClient = new HttpClient();
		DispatcherTimer timer = new DispatcherTimer();
		string clientName = "client.exe";
		string urlClient = "https://github.com/dudantas/CanaryLauncherUpdate/releases/download/download-files/client.zip";
		string urlPackage = "https://github.com/dudantas/CanaryLauncherUpdate/releases/download/download-files/package.json";
		string newVersion = "";
		string path = AppDomain.CurrentDomain.BaseDirectory.ToString();
		
		// This will pull the version of the "package.json" file from a user-defined url.
		private async Task<string> GetPackageVersionFromUrl(string url)
		{
			using (HttpClient client = new HttpClient())
			{
				string json = await client.GetStringAsync(url);
				var data = JsonConvert.DeserializeObject<dynamic>(json);
				return data.version.ToString();
			}
		}

		static string GetClientVersion(string path)
		{
			string json = path + "/package.json";
			StreamReader stream = new StreamReader(json);
			dynamic jsonString = stream.ReadToEnd();
			dynamic versionclient = JsonConvert.DeserializeObject(jsonString);
			foreach (string version in versionclient)
			{
				return version;
			}

			return "";
		}

		private void StartClient()
		{
			Process.Start(path + "/bin/" + clientName);
			this.Close();
		}

		public SplashScreen()
		{
			InitializeComponent();
			timer.Tick += new EventHandler(timer_SplashScreen);
			timer.Interval = new TimeSpan(0, 0, 5);
			timer.Start();
		}

		public async void timer_SplashScreen(object? sender, EventArgs e)
		{
			string newVersion = await GetPackageVersionFromUrl(urlPackage);
			if (newVersion == null)
			{
				this.Close();
			}

			// Start the client if the versions are the same
			if (File.Exists(path + "/package.json")) {
				string actualVersion = GetClientVersion(path);
				if (newVersion == actualVersion) {
					StartClient();
				}
			}

			var requestClient = new HttpRequestMessage(HttpMethod.Post, urlClient);
			var response = await httpClient.SendAsync(requestClient);
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				this.Close();
			}

			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			MainWindow mainWindow = new MainWindow();
			this.Close();
			mainWindow.Show();
			timer.Stop();
		}
	}
}
