using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace AspNetCoreCsvImportExport.Formatters
{
    /// <summary>
    /// ContentType: text/csv
    /// </summary>
    public class CsvInputFormatter : InputFormatter
    {
        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var type = context.ModelType;
            var request = context.HttpContext.Request;
            MediaTypeHeaderValue requestContentType = null;
            MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType);


            var result = readStream(type, request.Body);
            return InputFormatterResult.SuccessAsync(result);
        }

        public override bool CanRead(InputFormatterContext context)
        {
            var type = context.ModelType;
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

        private object readStream(Type type, Stream stream)
        {
            var list = Activator.CreateInstance(type);

            var reader = new StreamReader(stream);

            bool skipFirstLine = true;
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');
                if(skipFirstLine)
                {
                    skipFirstLine = false;
                }
                else
                {
                    // Start reading items!
                }

            }

            return list;
        }
    }
}
