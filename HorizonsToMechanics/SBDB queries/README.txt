This is the original commit message from 2023-09-05: "Here is more info from the small object database, but I don't see how to query Horizons in such a way. GM is mass expressed as a product of the mass “M” and gravitational constant “G” (km3/s2)"

As of 2024-02-24, this is what I think I remember:
 - This directory contains some queries (the filenames) and results (the file contents) from NASA's "Small-Body Database Lookup". I didn't end up using this.
 - The advantage of this approach was that the body data was all JSON-formatted, unlike the Horizons ephemeris header, where the body's physical properties were just a long string with seemingly arbitrary formatting.
 - I ended up using the Horizons ephemeris header anyway, I think because SBDB only had data for very few bodies, or maybe some of the data was huge binaries?
