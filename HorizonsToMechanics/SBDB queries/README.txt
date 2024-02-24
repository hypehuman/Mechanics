2023-09-05: Here is more info from the small object database, but I don't see how to query Horizons in such a way. GM is mass expressed as a product of the mass “M” and gravitational constant “G” (km3/s2)

2024-02-24: This is what I think I remember:
 - This directory contains some query results from NASA's "Small-Body Database Lookup" (SBDB). I didn't end up using this.
 - The advantage of this approach was that the body data was all JSON-formatted, unlike the Horizons ephemeris header, where the body's physical properties were just a long string with seemingly arbitrary formatting.
 - I ended up using the Horizons ephemeris header anyway, I think because SBDB only had data for very few bodies, or maybe some of the data was huge binaries?

Here are the URLs I pasted into my browser to get the respective json results:
    result_count.json
        https://ssd-api.jpl.nasa.gov/sbdb_query.api?info=count
    result_GM.json
        https://ssd-api.jpl.nasa.gov/sbdb_query.api?fields=spkid,full_name,GM
    result_GM_DF.json
        https://ssd-api.jpl.nasa.gov/sbdb_query.api?fields=spkid,full_name,GM&sb-cdata="GM|DF"

2024-03-09: I repeated the result_GM query and there appears to be new data, suggesting that SBDB is still actively maintained.
