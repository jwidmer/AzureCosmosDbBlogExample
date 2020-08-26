function createLike(postId, like) {
    var collection = getContext().getCollection();

    collection.readDocument(
        `${collection.getAltLink()}/docs/${postId}`,
        function (err, post) {
            if (err) throw err;

            post.likeCount++;
            collection.replaceDocument(
                post._self,
                post,
                function (err) {
                    if (err) throw err;

                    //like.postId = postId;
                    collection.createDocument(
                        collection.getSelfLink(),
                        like
                    );
                }
            );
        })
}