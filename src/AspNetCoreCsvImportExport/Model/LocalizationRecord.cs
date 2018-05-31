using System;
using Newtonsoft.Json;

namespace AspNetCoreCsvImportExport.Model
{
    public class LocalizationRecord
    {
        [JsonIgnore]
        public long? Id { get; set; }

        [JsonProperty(PropertyName = "CustomKeyName")]
        public string Key { get; set; }

        public string Text { get; set; }

        public string LocalizationCulture { get; set; }
        
        public string ResourceKey { get; set; }
    }
}
