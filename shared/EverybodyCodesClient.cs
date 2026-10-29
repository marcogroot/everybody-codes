using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

public sealed class EverybodyCodesClient
{
	readonly Lazy<HttpClient> lazyClient = new(CreateHttpClient);
	readonly int eventYear;
	int? seed;

	HttpClient client => lazyClient.Value;

	public EverybodyCodesClient(int eventYear = 2026)
	{
		this.eventYear = eventYear;
	}

	static HttpClient CreateHttpClient()
	{
		var cookies = new CookieContainer();
		cookies.Add(new Uri("https://everybody.codes"), new Cookie("everybody-codes", LoadApiKey(), "/", ".everybody.codes"));
		return new HttpClient(new HttpClientHandler { CookieContainer = cookies });
	}

	public async Task<string> GetInputAsync(int quest, int part)
	{
		var cacheFile = CacheFile(part);
		if (File.Exists(cacheFile)) return File.ReadAllText(cacheFile);

		await DownloadUnlockedPartsAsync(quest);

		return File.Exists(cacheFile)
			? File.ReadAllText(cacheFile)
			: throw new InvalidOperationException($"Quest {quest} part {part} is still locked");
	}

	public async Task SubmitAsync(int quest, int part, object answer)
	{
		var answerText = answer.ToString()!;
		var request = new StringContent(new JsonObject { ["answer"] = answerText }.ToJsonString(), Encoding.UTF8, "application/json");
		using var response = await client.PostAsync($"https://api.everybody.codes/event/{eventYear}/quest/{quest}/part/{part}/answer", request);
		var body = await response.Content.ReadAsStringAsync();

		JsonElement root;
		try
		{
			root = JsonDocument.Parse(body).RootElement;
		}
		catch (JsonException)
		{
			Console.WriteLine($"Not accepted, answer was {answerText} ({(int)response.StatusCode}): {body}");
			return;
		}

		var verdict = root.TryGetProperty("correct", out var correct)
			? correct.GetBoolean() ? "Correct" : "Incorrect"
			: "Not accepted";
		Console.WriteLine($"{verdict}, answer was {answerText}");

		if (verdict == "Correct") return;

		foreach (var field in root.EnumerateObject())
		{
			if (field.Name == "correct") continue;
			Console.WriteLine($"  {field.Name}: {field.Value}");
		}
	}

	async Task DownloadUnlockedPartsAsync(int quest)
	{
		seed ??= await GetSeedAsync();

		using var input = JsonDocument.Parse(await client.GetStringAsync($"https://everybody.codes/assets/{eventYear}/{quest}/input/{seed}.json"));
		using var keys = JsonDocument.Parse(await client.GetStringAsync($"https://api.everybody.codes/event/{eventYear}/quest/{quest}"));

		Directory.CreateDirectory("input");
		for (var part = 1; part <= 3; part++)
		{
			if (!keys.RootElement.TryGetProperty($"key{part}", out var key)) continue;

			var text = Decrypt(input.RootElement.GetProperty($"{part}").GetString()!, key.GetString()!);
			File.WriteAllText(CacheFile(part), text);
		}
	}

	static string CacheFile(int part) => Path.Combine("input", $"part{part}.txt");

	async Task<int> GetSeedAsync()
	{
		using var me = JsonDocument.Parse(await client.GetStringAsync("https://api.everybody.codes/user/me"));
		return me.RootElement.GetProperty("seed").GetInt32();
	}

	static string Decrypt(string hex, string key)
	{
		using var aes = Aes.Create();
		aes.Key = Encoding.UTF8.GetBytes(key);
		var iv = Encoding.UTF8.GetBytes(key[..16]);
		return Encoding.UTF8.GetString(aes.DecryptCbc(Convert.FromHexString(hex), iv));
	}

	static string LoadApiKey()
	{
		for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
		{
			var envFile = Path.Combine(dir.FullName, ".env");
			if (!File.Exists(envFile)) continue;

			foreach (var line in File.ReadAllLines(envFile))
			{
				var trimmed = line.Trim();
				if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;

				var idx = trimmed.IndexOf('=');
				if (idx < 0) continue;

				if (trimmed[..idx].Trim() == "API_KEY")
					return trimmed[(idx + 1)..].Trim().Trim('"');
			}
		}

		return Environment.GetEnvironmentVariable("API_KEY")
			?? throw new InvalidOperationException("API_KEY not found in any .env or the environment");
	}
}
