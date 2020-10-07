function truncateFeed() {
    const maxDocs = 5;
    var context = getContext();
    var collection = context.getCollection();

    collection.queryDocuments(
        collection.getSelfLink(),
        "SELECT VALUE COUNT(1) FROM f",
        function (err, results) {
            if (err) throw err;

            processCountResults(results);
        });

    function processCountResults(results) {
        // + 1 because the query didn't count the newly inserted doc
        if ((results[0] + 1) > maxDocs) {
            var docsToRemove = results[0] + 1 - maxDocs;
            collection.queryDocuments(
                collection.getSelfLink(),
                `SELECT TOP ${docsToRemove} * FROM f ORDER BY f.dateCreated`,
                function (err, results) {
                    if (err) throw err;

                    processDocsToRemove(results, 0);
                });
        }
    }

    function processDocsToRemove(results, index) {
        var doc = results[index];
        if (doc) {
            collection.deleteDocument(
                doc._self,
                function (err) {
                    if (err) throw err;

                    processDocsToRemove(results, index + 1);
                });
        }
    }
}