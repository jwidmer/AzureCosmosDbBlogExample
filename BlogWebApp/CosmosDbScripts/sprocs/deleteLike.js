function deleteLike(postId, userId) {
    var collection = getContext().getCollection();

    var likeQuery = {
        query: "SELECT * FROM p WHERE p.postId=@postId AND p.type ='like' AND p.userId = @userId",
        parameters: [{ name: "@postId", value: postId }, { name: "@userId", value: userId }]
    };

    collection.queryDocuments(collection.getSelfLink(), likeQuery, function (err, feed, responseOptions) {
        if (err) throw err;

        if (feed.length >= 1) {
            collection.deleteDocument(feed[0]._self, {}, function (err, responseOptions) {
                if (err) throw err;

                collection.readDocument(
                    `${collection.getAltLink()}/docs/${postId}`,
                    function (err, post) {
                        if (err) throw err;

                        if (post.likeCount > 0) {
                            post.likeCount--;
                        }
                        else {
                            post.likeCount = 0;
                        }
                        collection.replaceDocument(
                            post._self,
                            post,
                            function (err) {
                                if (err) throw err;
                            }
                        );
                    })
            });
        }
    });

}