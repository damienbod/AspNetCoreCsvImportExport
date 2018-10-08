# Import and Export CSV in ASP.NET Core
## Where to get it
You can download [this package](https://www.nuget.org/packages/WebApiContrib.Core.Formatter.Csv/) on NuGet.

## About
This article shows how to import and export csv data in an ASP.NET Core application. The InputFormatter and the OutputFormatter classes are used to convert the csv data to the C# model classes. 

<strong>2018-10-08: </strong> Updated to .NET Core 2.1.5, adding compression

<strong>2018-09-14: </strong> Updated to .NET Core 2.1.4

<strong>2018-07-09: </strong> Updated to .NET Core 2.1.1

<strong>2018-05-31: </strong> Support for ignore, Updated to .NET Core 2.1

<strong>2018-05-13: </strong> Updated to .NET Core 2.1 rc

<strong>2017-10-29: </strong> Support for CSV encoding

<strong>2017-08-23: </strong> Updated to ASP.NET Core 2.0

<strong>2017-02-12: </strong> Updated to VS2017 msbuild

<strong>2016-11-25:</strong> Updated ASP.NET Core 1.1

<strong>2016-06-29:</strong> Updated to ASP.NET Core RTM

The LocalizationRecord class is used as the model class to import and export to and from csv data.

```csharp
using System;

namespace AspNetCoreCsvImportExport.Model
{
    public class LocalizationRecord
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public string Text { get; set; }
        public string LocalizationCulture { get; set; }
        public string ResourceKey { get; set; }
    }
}
```

The MVC Controller CsvTestController  makes it possible to import and export the data. The Get method exports the data using the Accept header in the HTTP Request. Per default, Json will be returned. If the Accept Header is set to 'text/csv', the data will be returned as csv. The GetDataAsCsv method always returns csv data because the Produces attribute is used to force this. This makes it easy to download the csv data in a browser. 

The Import method uses the Content-Type HTTP Request header to decide how to handle the request body. If the 'text/csv' is defined, the custom csv input formatter will be used.

```csharp
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

```

The csv input formatter implements the InputFormatter class. This checks if the context ModelType property is a type of IList and if so, converts the csv data to a List of Objects of type T using reflection. This is implemented in the read stream method. The implementation is very basic and will not work if you have more complex structures in your model class.

 
```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace AspNetCoreCsvImportExport.Formatters
{
    /// <summary>
    /// ContentType: text/csv
    /// </summary>
    public class CsvInputFormatter : InputFormatter
    {
        private readonly CsvFormatterOptions _options;

        public CsvInputFormatter(CsvFormatterOptions csvFormatterOptions)
        {
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));

            if (csvFormatterOptions == null)
            {
                throw new ArgumentNullException(nameof(csvFormatterOptions));
            }

            _options = csvFormatterOptions;
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var type = context.ModelType;
            var request = context.HttpContext.Request;
            MediaTypeHeaderValue requestContentType = null;
            MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType);


            var result = ReadStream(type, request.Body);
            return InputFormatterResult.SuccessAsync(result);
        }

        public override bool CanRead(InputFormatterContext context)
        {
            var type = context.ModelType;
            if (type == null)
                throw new ArgumentNullException("type");

            return IsTypeOfIEnumerable(type);
        }

        private bool IsTypeOfIEnumerable(Type type)
        {

            foreach (Type interfaceType in type.GetInterfaces())
            {

                if (interfaceType == typeof(IList))
                    return true;
            }

            return false;
        }

        private object ReadStream(Type type, Stream stream)
        {
            Type itemType;
            var typeIsArray = false;
            IList list;
            if (type.GetGenericArguments().Length > 0)
            {
                itemType = type.GetGenericArguments()[0];
                list = (IList)Activator.CreateInstance(type);
            }
            else
            {
                typeIsArray = true;
                itemType = type.GetElementType();

                var listType = typeof(List<>);
                var constructedListType = listType.MakeGenericType(itemType);

                list = (IList)Activator.CreateInstance(constructedListType);
            }

            var reader = new StreamReader(stream, Encoding.GetEncoding(_options.Encoding));

            bool skipFirstLine = _options.UseSingleLineHeaderInCsv;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(_options.CsvDelimiter.ToCharArray());
                if(skipFirstLine)
                {
                    skipFirstLine = false;
                }
                else
                {
                    var itemTypeInGeneric = list.GetType().GetTypeInfo().GenericTypeArguments[0];
                    var item = Activator.CreateInstance(itemTypeInGeneric);
                    var properties = item.GetType().GetProperties();
                    for (int i = 0;i<values.Length; i++)
                    {
                        properties[i].SetValue(item, Convert.ChangeType(values[i], properties[i].PropertyType), null);
                    }

                    list.Add(item);
                }

            }

            if(typeIsArray)
            {
                Array array = Array.CreateInstance(itemType, list.Count);

                for(int t = 0; t < list.Count; t++)
                {
                    array.SetValue(list[t], t);
                }
                return array;
            }
            
            return list;
        }
    }
}
```

The csv output formatter is implemented using the code from <a href="http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format">Tugberk Ugurlu's blog</a> with some small changes. Thanks for this. This formatter uses ';' to separate the properties and a new line for each object. The headers are added tot he first line.


```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.ComponentModel.DataAnnotations;

namespace AspNetCoreCsvImportExport.Formatters
{
    /// <summary>
    /// Original code taken from
    /// http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format
    /// Adapted for ASP.NET Core and uses ; instead of , for delimiters
    /// </summary>
    public class CsvOutputFormatter : OutputFormatter
    {
        private readonly CsvFormatterOptions _options;

        public string ContentType { get; private set; }

        public CsvOutputFormatter(CsvFormatterOptions csvFormatterOptions)
        {
            ContentType = "text/csv";
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));
            _options = csvFormatterOptions ?? throw new ArgumentNullException(nameof(csvFormatterOptions));
        }

        protected override bool CanWriteType(Type type)
        {

            if (type == null)
                throw new ArgumentNullException("type");

            return IsTypeOfIEnumerable(type);
        }

        private bool IsTypeOfIEnumerable(Type type)
        {

            foreach (Type interfaceType in type.GetInterfaces())
            {

                if (interfaceType == typeof(IList))
                    return true;
            }

            return false;
        }

        public async override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            Type type = context.Object.GetType();
            Type itemType;

            if (type.GetGenericArguments().Length > 0)
            {
                itemType = type.GetGenericArguments()[0];
            }
            else
            {
                itemType = type.GetElementType();
            }

            var streamWriter = new StreamWriter(response.Body, Encoding.GetEncoding(_options.Encoding));

            if (_options.UseSingleLineHeaderInCsv)
            {
                await streamWriter.WriteLineAsync(
                    string.Join(
                        _options.CsvDelimiter, itemType.GetProperties().Select(x => x.GetCustomAttribute<DisplayAttribute>(false)?.Name ?? x.Name)
                    )
                );
            }

            foreach (var obj in (IEnumerable<object>)context.Object)
            {

                var vals = obj.GetType().GetProperties().Select(
                    pi => new
                    {
                        Value = pi.GetValue(obj, null)
                    }
                );

                string valueLine = string.Empty;

                foreach (var val in vals)
                {
                    if (val.Value != null)
                    {

                        var _val = val.Value.ToString();

                        //Check if the value contans a comma and place it in quotes if so
                        if (_val.Contains(","))
                            _val = string.Concat("\"", _val, "\"");

                        //Replace any \r or \n special characters from a new line with a space
                        if (_val.Contains("\r"))
                            _val = _val.Replace("\r", " ");
                        if (_val.Contains("\n"))
                            _val = _val.Replace("\n", " ");

                        valueLine = string.Concat(valueLine, _val, _options.CsvDelimiter);

                    }
                    else
                    {
                        valueLine = string.Concat(valueLine, string.Empty, _options.CsvDelimiter);
                    }
                }

                await streamWriter.WriteLineAsync(valueLine.TrimEnd(_options.CsvDelimiter.ToCharArray()));
            }

            await streamWriter.FlushAsync();
        }
    }
}

```

The custom formatters need to be added to the MVC middleware, so that it knows how to handle media types 'text/csv'. 


```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(options =>
    {
        options.InputFormatters.Add(new CsvInputFormatter());
        options.OutputFormatters.Add(new CsvOutputFormatter());
        options.FormatterMappings.SetMediaTypeMappingForFormat("csv", MediaTypeHeaderValue.Parse("text/csv"));
    });
}
```

When the data.csv link is requested, a csv type response is returned to the client, which can be saved. This data contains the header texts and the value of each property in each object. This can then be opened in excel.

http://localhost:10336/api/csvtest/data.csv

```csharp
Id;Key;Text;LocalizationCulture;ResourceKey
1;test;test text;en-US;test
2;test;test2 text de-CH;de-CH;test
```

This data can then be used to upload the csv data to the server which is then converted back to a C# object. I use fiddler, postman or curl can also be used, or any HTTP Client where you can set the header Content-Type.

```csharp

 http://localhost:10336/api/csvtest/import 

 User-Agent: Fiddler 
 Content-Type: text/csv 
 Host: localhost:10336 
 Content-Length: 110 


 Id;Key;Text;LocalizationCulture;ResourceKey 
 1;test;test text;en-US;test 
 2;test;test2 text de-CH;de-CH;test 

```

The following image shows that the data is imported correctly.


<img src="https://damienbod.files.wordpress.com/2016/06/importexportcsv.png" alt="importExportCsv" width="598" height="558" class="alignnone size-full wp-image-6742" />

<strong>Notes</strong>

The implementation of the InputFormatter and the OutputFormatter classes are specific for a list of simple classes with only properties. If you require or use more complex classes, these implementations need to be changed.

<strong>Links</strong>

http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format

https://damienbod.com/2015/06/03/asp-net-5-mvc-6-custom-protobuf-formatters/

http://www.strathweb.com/2014/11/formatters-asp-net-mvc-6/

https://wildermuth.com/2016/03/16/Content_Negotiation_in_ASP_NET_Core
