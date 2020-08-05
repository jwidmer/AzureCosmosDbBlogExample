using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogWebApp.Models
{
    public class BlogPostComment
    {

        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get
            {
                return CommentId;
            }
        }

        [JsonProperty(PropertyName = "commentId")]
        public string CommentId { get; set; }


        [JsonProperty(PropertyName = "type")]
        public string Type
        {
            get
            {
                return "comment";
            }
        }

        [JsonProperty(PropertyName = "postId")]
        public string PostId { get; set; }



        [JsonProperty(PropertyName = "userId")]
        public string CommentAuthorId { get; set; }

        [JsonProperty(PropertyName = "userUsername")]
        public string CommentAuthorUsername { get; set; }


        [JsonProperty(PropertyName = "content")]
        public string CommentContent { get; set; }


        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime CommentDateCreated { get; set; }



        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
