# libraryappsearch-elasticsearch

- Download Elastic Search.
- UnZip and move the folder to your favorite location.
- Into the folder unziped enter in bin folder and then from CMD you should execute [elasticsearch.bin].
- Reset the password and assing a new, in the same location (bin folder) you should execute [elasticsearch-reset-password -u elastic].
- Now let's test the if the service is running, from CMD **[curl -H "Content-Type: application/json" -XGET --user "elastic:here-your-pwd"]**.
- If until this point you have all ok, let's load some data to test the Elastic and the consumer API.
- Search this file books-sample-data.json in the root of the repo, then use this command to load in bulk way the information **[curl -H "Content-Type: application/json" -XPOST "localhost:9200/books-index/_bulk?pretty" --data-binary @books-sample-data.json --user "elastic:here-your-pwd"]**.
- Now, we can open the solution and configure the two projects WEB and API to run in the same time.

## [Sources] 

Elastic Search
https://www.youtube.com/watch?v=ik8a0JeIERQ

Infinite Scroll / Select2
https://www.youtube.com/watch?v=R4iEfHwwU9A

