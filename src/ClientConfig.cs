using System;
using System.Windows;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LauncherConfig
{
	public class ClientConfig
	{
		public string clientVersion { get; set; }
		public string launcherVersion { get; set; }
		public bool replaceFolders { get; set; }
		public ReplaceFolderName[] replaceFolderName { get; set; }
		public string clientFolder { get; set; }
		public string newClientUrl { get; set; }
		public string newConfigUrl { get; set; }
		public string clientExecutable { get; set; }

		public static ClientConfig loadFromFile(string url)
		{
			using (HttpClient client = new HttpClient())
			{
				Task<string> jsonTask = client.GetStringAsync(url);
				string jsonString = jsonTask.Result;
				return JsonConvert.DeserializeObject<ClientConfig>(jsonString);
			}
		}
	}

	public class ReplaceFolderName
	{
		public string name { get; set; }
	}
}
