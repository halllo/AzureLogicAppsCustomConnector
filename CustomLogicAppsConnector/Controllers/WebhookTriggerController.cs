namespace CustomLogicAppsConnector.Controllers
{
	//[Route("api/webhooktrigger")]
	//[ApiController]
	//public class WebhookTriggerController : ControllerBase
	//{
	//	public static List<string> subscriptions = new List<string>();


	//	[HttpPost, Route("subscribe")]
	//	public ActionResult Subscribe([FromBody] string callbackUrl)
	//	{
	//		subscriptions.Add(callbackUrl);

	//		return Accepted();
	//	}


	//	[HttpGet, Route("trigger")]
	//	public async Task<ActionResult> Get()
	//	{
	//		using (var client = new HttpClient())
	//		{
	//			foreach (string callbackUrl in subscriptions)
	//			{
	//				await client.PostAsync(callbackUrl, @"{""trigger"":""fired""}", new JsonMediaTypeFormatter(), "application/json");
	//			}
	//		}

	//		return Accepted(new { SubscriptionsFired = subscriptions.Count });
	//	}


	//	[HttpPost, Route("unsubscribe")]
	//	public ActionResult Unsubscribe([FromBody] string callbackUrl)
	//	{
	//		subscriptions.Remove(callbackUrl);
	//		return Accepted();
	//	}
	//}
}
