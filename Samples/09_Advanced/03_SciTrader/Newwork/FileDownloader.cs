using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;

namespace SciTrader.Network
{
	public class FileDownloader
	{
		private readonly HttpClient _httpClient;
		private readonly string _serverUrl;
		private readonly string _saveDirectory;

		public FileDownloader(string serverUrl, string saveDirectory)
		{
			_httpClient = new HttpClient();
			_serverUrl = serverUrl.TrimEnd('/');
			_saveDirectory = saveDirectory;

			if (!Directory.Exists(_saveDirectory))
				Directory.CreateDirectory(_saveDirectory);
		}

		// Get the file list from the server
		public async Task<List<string>> GetFileListAsync()
		{
			var fileList = new List<string>();
			try
			{
				string url = $"{_serverUrl}/list";
				string responseHtml = await _httpClient.GetStringAsync(url);

				// Extract file names using regex
				var matches = Regex.Matches(responseHtml, @"<a href='/download/(.*?)'>(.*?)</a>");
				foreach (Match match in matches)
				{
					if (match.Groups.Count > 1)
					{
						fileList.Add(match.Groups[1].Value);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting file list: {ex.Message}");
			}
			return fileList;
		}

		// Download a single file
		public async Task<bool> DownloadFileAsync(string fileName)
		{
			try
			{
				string fileUrl = $"{_serverUrl}/download/{fileName}";
				string savePath = Path.Combine(_saveDirectory, fileName);

				Console.WriteLine($"Downloading: {fileName}...");

				byte[] fileBytes = await _httpClient.GetByteArrayAsync(fileUrl);
				await File.WriteAllBytesAsync(savePath, fileBytes);

				Console.WriteLine($"Download completed: {fileName}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to download {fileName}: {ex.Message}");
				return false;
			}
		}

		// Download all files from the server
		public async Task DownloadAllFilesAsync(Action onComplete)
		{
			var fileList = await GetFileListAsync();
			if (fileList.Count == 0)
			{
				Console.WriteLine("No files available for download.");
				return;
			}

			Console.WriteLine($"Found {fileList.Count} files. Starting download...");

			foreach (var file in fileList)
			{
				await DownloadFileAsync(file);
			}

			Console.WriteLine("All files downloaded successfully.");

			onComplete?.Invoke();
		}
	}
}
