# SignalR-Postgresql
Real time push notification using SignalR  and PostgreSQL

An implementation of real time notification using SignalR and the PostgreSQL listen/notify feature.

When changes (insert/delete/update) are made in a PostgreSQL database table, the changed content is pushed to the webpage.

# Npgsql
The Npgsql NET data provider for PostgreSQL is not included in this repo.
You can download a copy from here: https://github.com/npgsql/npgsql.git or
execute <b>Install-Package Npgsql</b> in the Package Manager Console
