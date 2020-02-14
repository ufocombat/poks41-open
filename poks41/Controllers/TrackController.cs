using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using poks41.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace poks41.Controllers
{
    [Route("api/[controller]")]
    public class TrackController : Controller
    {
        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get(Storage s)
        {
            var tracks = await s.GetList<TrackClass>("tracks");
            return new OkObjectResult(JsonConvert.SerializeObject(tracks));
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrackClass track, Storage s)
        {
            track.PartitionKey = track.PartitionKey ?? "unknown";
            track.RowKey = track.RowKey ?? Guid.NewGuid().ToString();

            await s.InsertIn("tracks", track);

            return new OkObjectResult(JsonConvert.SerializeObject(track));
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
