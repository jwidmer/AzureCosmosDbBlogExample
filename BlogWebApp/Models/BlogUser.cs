using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.Models
{
    public class BlogUser
    {

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get
            {
                return UserId;
            }
        }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; } = string.Empty;


        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get
            {
                return "user";
            }
        }


        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; } = string.Empty;


        [JsonProperty(PropertyName = "action")]
        public string Action { get; set; } = string.Empty;


        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
