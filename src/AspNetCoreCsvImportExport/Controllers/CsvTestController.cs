using System.Collections.Generic;
using System.Net;
using AspNetCoreCsvImportExport.Model;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreCsvImportExport.Controllers
{
    [Route("api/[controller]")]
    public class CsvTestController : Controller
    {
        // GET api/csvtest
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LocalizationRecord>), (int)HttpStatusCode.OK)]
        public IActionResult Get()
        {
            return Ok(DummyDataSimple());
        }

        [HttpGet]
        [Route("data.csv")]
        [Produces("text/csv")]
        public IActionResult GetDataAsCsv()
        {
            return Ok(DummyDataSimple());
        }

        [HttpGet]
        [Route("datacomplex.csv")]
        [Produces("text/csv")]
        public IActionResult GetComplexDataAsCsv()
        {
            return Ok(DummyDataComplex());
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

        private static IEnumerable<LocalizationRecord> DummyDataSimple()
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
                    Text = "test2 öäüéàè text de-CH",
                    LocalizationCulture = "de-CH",
                    ResourceKey = "test"

                }
            };

            return model;
        }
        private static IEnumerable<LocalizationRecord> DummyDataComplex()
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
                    Text = "test2 öäüéàè text de-CH",
                    LocalizationCulture = "de-CH",
                    ResourceKey = "test"

                },
                new LocalizationRecord
                {
                    Id = 3,
                    Key = "test3",
                    Text = "test2 öäüéàè text de-CH, it-CH, en-US",
                    LocalizationCulture = "de-CH",
                    ResourceKey = "test"

                }
            };

            return model;
        }

    }
}
