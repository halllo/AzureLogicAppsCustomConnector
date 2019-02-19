using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace CustomLogicAppsConnector.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ValuesController : ControllerBase
	{
		public static List<string> values = new List<string>(new string[] { "value1", "value2" });

		// GET api/values
		[HttpGet]
		public ActionResult<IEnumerable<string>> GetsAllTheValues()
		{
			return values;
		}

		//// GET api/values/5
		//[HttpGet("{id}")]
		//public ActionResult<string> Get(int id)
		//{
		//	return values[id];
		//}

		// POST api/values
		[HttpPost]
		public void AddsANewValue([FromBody] string value)
		{
			values.Add(value);
		}

		//// PUT api/values/5
		//[HttpPut("{id}")]
		//public void Put(int id, [FromBody] string value)
		//{
		//	values[id] = value;
		//}

		//// DELETE api/values/5
		//[HttpDelete("{id}")]
		//public void Delete(int id)
		//{
		//	values.RemoveAt(id);
		//}
	}
}
