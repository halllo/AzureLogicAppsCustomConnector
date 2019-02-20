using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace CreateTeamsTeam
{
	class Program
	{
		static void Main(string[] args)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(System.IO.Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);

			var configuration = builder.Build();

			Do(configuration).Wait();
			Console.ReadLine();
		}

		static void Log(string text, ConsoleColor? color = null)
		{
			var colorBefore = Console.ForegroundColor;
			if (color.HasValue)
			{
				Console.ForegroundColor = color.Value;
			}
			Console.WriteLine(text);
			Console.ForegroundColor = colorBefore;
		}
















		static async Task Do(IConfiguration config)
		{
			var accessToken = await AccessToken(config);
			Log(accessToken, ConsoleColor.Green);

			var graphClient = GraphClient(accessToken);
			var allUsers = await Users(graphClient);
			var requestedOwners = config.GetSection("NewGroupOwners").GetChildren().Select(c => c.Value).ToArray();
			var requestedMembers = config.GetSection("NewGroupMembers").GetChildren().Select(c => c.Value).ToArray();

			var groupName = "Manuels Unified Group " + DateTime.Now.ToString();
			var newGroup = await AddGroup(graphClient,
				name: groupName,
				description: groupName + " (automatically created, just for experimenting)",
				owners: requestedOwners.Select(owner => User(allUsers, owner)),
				members: requestedMembers.Select(member => User(allUsers, member))
			);

			Log($"Created new group \"{newGroup.DisplayName}\" (id:{newGroup.Id})", ConsoleColor.Magenta);
		}








		static async Task<string> AccessToken(IConfiguration config)
		{
			var client = new ConfidentialClientApplication(
				clientId: config["ClientID"],
				authority: $"https://login.microsoftonline.com/{config["TenantID"]}/",
				redirectUri: config["ClientRedirectUri"],
				clientCredential: new ClientCredential(config["ClientSecret"]),
				userTokenCache: null,
				appTokenCache: null);

			try
			{
				var result = await client.AcquireTokenForClientAsync(new[] { "https://graph.microsoft.com/.default" });
				return result.AccessToken;
			}
			catch (Exception e)
			{
				Log(e.ToString(), ConsoleColor.Red);
				throw;
			}
		}

		static IGraphServiceClient GraphClient(string accessToken)
		{
			var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) =>
			{
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", accessToken);
				return Task.FromResult(0);
			}));
			return graphClient;
		}

		static async Task<List<User>> Users(IGraphServiceClient graphClient)
		{
			var usersResult = new List<User>();
			var users = await graphClient.Users.Request().Select("id,displayName,mail,userPrincipalName,userType").GetAsync();
			while (users.Count > 0)
			{
				foreach (var u in users)
				{
					usersResult.Add(u);
				}

				if (users.NextPageRequest != null)
				{
					users = await users.NextPageRequest.GetAsync();
				}
				else
				{
					break;
				}
			}

			return usersResult;
		}

		static User User(IEnumerable<User> users, string userPrincipalName)
		{
			return users.SingleOrDefault(u => string.Equals(u.UserPrincipalName, userPrincipalName, StringComparison.InvariantCultureIgnoreCase));
		}

		static async Task<Group> AddGroup(IGraphServiceClient graphClient, string name, string description, IEnumerable<User> owners, IEnumerable<User> members)
		{

			var group = new GroupExtended
			{
				GroupTypes = new List<string> { "Unified" },
				DisplayName = name,
				Description = description,
				MailNickname = string.Concat(name.Except(new[] { ' ', '/', ':' })),
				MailEnabled = true,
				SecurityEnabled = false,
				Visibility = "Private",
				OwnersODataBind = owners.NullIfEmpty()?.Select(u => string.Format("https://graph.microsoft.com/v1.0/users/{0}", u.Id)).ToArray(),
				MembersODataBind = members.NullIfEmpty()?.Select(u => string.Format("https://graph.microsoft.com/v1.0/users/{0}", u.Id)).ToArray(),
			};

			var newGroup = await graphClient.Groups.Request().AddAsync(group);
			return newGroup;
		}

	}


	public class GroupExtended : Group
	{
		[JsonProperty("owners@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
		public string[] OwnersODataBind { get; set; }
		[JsonProperty("members@odata.bind", NullValueHandling = NullValueHandling.Ignore)]
		public string[] MembersODataBind { get; set; }
	}




	public static class EnumerableExtensions
	{
		public static IEnumerable<T> NeverNull<T>(this IEnumerable<T> elements)
		{
			if (elements != null)
			{
				return elements;
			}
			else
			{
				return Enumerable.Empty<T>();
			}
		}

		public static IEnumerable<T> NullIfEmpty<T>(this IEnumerable<T> elements)
		{
			if (elements != null && elements.Any())
			{
				return elements;
			}
			else
			{
				return null;
			}
		}
	}
}
