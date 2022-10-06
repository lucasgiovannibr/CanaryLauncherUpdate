using System;
using System.Windows;
using System.IO;
using System.Net;
using System.Windows.Threading;

namespace CanaryLauncherUpdate
{
	/// <summary>
	/// Lógica interna para SplashScreen.xaml
	/// </summary>
	public partial class SplashScreen : Window
	{
		readonly WebClient webClient = new();
		readonly string urlClient = "https://github.com/lucasgiovannibr/clientlauncherupdate/archive/refs/heads/main.zip";
		readonly string urlVersion = "https://raw.githubusercontent.com/lucasgiovannibr/clientlauncherupdate/main/version.txt";
		string currentVersion = "";
		readonly DispatcherTimer timer = new();

		public SplashScreen()
		{
			InitializeComponent();
			timer.Tick += new EventHandler(TimerSplashScreen);
			timer.Interval = new TimeSpan(0, 0, 5); // 5 seconds
			timer.Start();
		}

		public void TimerSplashScreen(object Source, EventArgs e)
		{
			// Check current version
			currentVersion = webClient.DownloadString(urlVersion);
			if (currentVersion == null)
			{
				this.Close();
			}

			// Check client download
			HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(urlClient);
			HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
			if (response.StatusCode == HttpStatusCode.NotFound)
			{
				this.Close();
			}

			if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient"))
			{
				Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/CanaryClient");
			}
			MainWindow mainWindow = new();
			this.Close();
			mainWindow.Show();
			timer.Stop();
		}
	}
}
