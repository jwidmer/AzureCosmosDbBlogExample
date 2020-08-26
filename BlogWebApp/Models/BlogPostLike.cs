using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.Models
{
    public class BlogPostLike
    {

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get
            {
                return LikeId;
            }
        }

        [JsonProperty(PropertyName = "likeId")]
        public string LikeId { get; set; }


        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get
            {
                return "like";
            }
        }

        [JsonProperty(PropertyName = "postId")]
        public string PostId { get; set; }



        [JsonProperty(PropertyName = "userId")]
        public string LikeAuthorId { get; set; }

        [JsonProperty(PropertyName = "userUsername")]
        public string LikeAuthorUsername { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime LikeDateCreated { get; set; }



        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
