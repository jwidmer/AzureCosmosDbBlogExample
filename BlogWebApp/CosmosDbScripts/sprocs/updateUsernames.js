function updateUsernames(userId, newUsername) {

    var collection = getContext().getCollection();

    collection.queryDocuments(
        collection.getSelfLink(),
        `SELECT * FROM p WHERE p.userId = '${userId}'`,
        { setEnableCrossPartitionQuery: true },
        function (err, results) {
            if (err) throw err;

            for (var i in results) {
                var doc = results[i];
                doc.userUsername = newUsername;

                collection.upsertDocument(
                    collection.getSelfLink(),
                    doc);
            }
        });
}