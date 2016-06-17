using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace AspNetCoreCsvImportExport.Formatters
{
    /// <summary>
    /// Original code taken from
    /// http://www.tugberkugurlu.com/archive/creating-custom-csvmediatypeformatter-in-asp-net-web-api-for-comma-separated-values-csv-format
    /// Adapted for ASP.NET Core and uses ; instead of , for delimiters
    /// </summary>
    public class CsvOutputFormatter :  OutputFormatter
    {
        public string ContentType { get; private set; }

        public CsvOutputFormatter()
        {
            ContentType = "text/csv";
            SupportedMediaTypes.Add(Microsoft.Net.Http.Headers.MediaTypeHeaderValue.Parse("text/csv"));

            //SupportedEncodings.Add(Encoding.GetEncoding("utf-8"));
        }

        protected override bool CanWriteType(Type type)
        {

            if (type == null)
                throw new ArgumentNullException("type");

            return isTypeOfIEnumerable(type);
        }

        private bool isTypeOfIEnumerable(Type type)
        {

            foreach (Type interfaceType in type.GetInterfaces())
            {

                if (interfaceType == typeof(IEnumerable))
                    return true;
            }

            return false;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            Type type = context.Object.GetType();
            writeStream(type, context.Object, response.Body);
            return Task.FromResult(response);
        }

        private void writeStream(Type type, object value, Stream stream)
        {
            Type itemType = type.GetGenericArguments()[0];

            StringWriter _stringWriter = new StringWriter();

            _stringWriter.WriteLine(
                string.Join<string>(
                    ";", itemType.GetProperties().Select(x => x.Name)
                )
            );

            foreach (var obj in (IEnumerable<object>)value)
            {

                var vals = obj.GetType().GetProperties().Select(
                    pi => new {
                        Value = pi.GetValue(obj, null)
                    }
                );

                string _valueLine = string.Empty;

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

                        _valueLine = string.Concat(_valueLine, _val, ";");

                    }
                    else
                    {

                        _valueLine = string.Concat(string.Empty, ";");
                    }
                }

                _stringWriter.WriteLine(_valueLine.TrimEnd(';'));
            }

            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(_stringWriter.ToString());
            streamWriter.Flush();
        }
    }
}
