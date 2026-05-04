using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.Models
{
    public class BlogUniqueUsername
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; } = string.Empty;


        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get
            {
                return "unique_username";
            }
        }


        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; } = string.Empty;


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
