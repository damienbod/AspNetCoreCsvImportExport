using System.Collections.Generic;
using AspNetCoreCsvImportExport.Model;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreCsvImportExport.Controllers
{
    [Route("api/[controller]")]
    public class CsvTestController : Controller
    {
        // GET api/csvtest
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(DummyData());
        }

        [HttpGet]
        [Route("data.csv")]
        [Produces("text/csv")]
        public IActionResult GetDataAsCsv()
        {
            return Ok( DummyData());
        }

        private static IEnumerable<LocalizationRecord> DummyData()
        {
            var model = new List<LocalizationRecord>
            {
                new LocalizationRecord
                {
                    Id = 1,
                    Key = "test",
                    Text = "test text",
                    LocalizationCulture = "en-US",
                    ResourceKey = "test"

                },
                new LocalizationRecord
                {
                    Id = 2,
                    Key = "test",
                    Text = "test2 text de-CH",
                    LocalizationCulture = "de-CH",
                    ResourceKey = "test"

                }
            };

            return model;
        }

        // GET api/csvtest/5
        [HttpGet("{id}")]
        public LocalizationRecord Get(int id)
        {
            return new LocalizationRecord
            {
                Id = id,
                Key = "test",
                Text = "test text",
                LocalizationCulture = "en-US",
                ResourceKey = "test"
            };
        }

        // POST api/csvtest
        [HttpPost]
        public IActionResult Post([FromBody]LocalizationRecord value)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                LocalizationRecord data = value;
                return Created("http://localhost:5000/api/csvtest", data);
            }
        }

        // POST api/csvtest/import
        [HttpPost]
        [Route("import")]
        public IActionResult Import([FromBody]List<LocalizationRecord> value)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                List<LocalizationRecord> data = value;
                return Ok();
            }
        }

    }
}
