using System;
using System.Windows;
using System.IO;
using System.Net;
using System.Windows.Threading;
using System.Net.Http;

namespace CanaryLauncherUpdate
{
	public partial class SplashScreen : Window
	{
		static readonly HttpClient httpClient = new HttpClient();
		DispatcherTimer timer = new DispatcherTimer();
		string urlClient = "https://github.com/lucasgiovannibr/clientlauncherupdate/archive/refs/heads/main.zip";
		string urlVersion = "https://raw.githubusercontent.com/lucasgiovannibr/clientlauncherupdate/main/version.txt";
		string currentVersion = "";
		string path = AppDomain.CurrentDomain.BaseDirectory.ToString();

		public SplashScreen()
		{
			InitializeComponent();
			timer.Tick += new EventHandler(timer_SplashScreen);
			timer.Interval = new TimeSpan(0, 0, 5);
			timer.Start();
		}

		public async void timer_SplashScreen(object? sender, EventArgs e)
		{
			var requestCurrentVersion = new HttpRequestMessage(HttpMethod.Post, urlVersion);
			var responseCurrentVersion = await httpClient.SendAsync(requestCurrentVersion);
			currentVersion = await responseCurrentVersion.Content.ReadAsStringAsync();
			if (currentVersion == null)
			{
				this.Close();
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
