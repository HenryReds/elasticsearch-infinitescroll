# libraryappsearch-elasticsearch

Download Elastic Search.

UnZip and move the folder to your favorite location.

Into the folder unziped enter in bin folder and then from CMD you should execute [elasticsearch.bin].

Reset the password and assing a new, in the same location (bin folder) you should execute [elasticsearch-reset-password -u elastic].

Now let's test the if the service is running, from CMD [curl -H "Content-Type: application/json" -XGET --user "elastic:here-your-pwd"].
