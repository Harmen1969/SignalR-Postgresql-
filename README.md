# SignalR-Postgresql
Real time push notification using SignalR  and PostgreSQL

An implementation of real time notification using SignalR and the PostgreSQL listen/notify feature.

When changes (insert/delete/update) are made in a PostgreSQL database table, the changed content is pushed to the webpage.

# Npgsql
The Npgsql NET data provider for PostgreSQL is not included in this repo.
To install <b>Npgsql</b>, run the following command in the Package Manager Console: Install-Package Npgsql

# Database Objects
CREATE TABLE tickets
(
    TICKETID serial primary key
    CREATED date NOT NULL DEFAULT CURRENT_DATE,
    DATEASSIGNED date,
    TITLE varchar(50),
    DESCRIPTION text,
    PRIORITY varchar(20),
    ASSIGNEDTO varchar(100),
    TICKETSTATUS character varchar(20)
)

 CREATE OR REPLACE FUNCTION ticketf() RETURNS TRIGGER AS $$
    BEGIN
    IF TG_OP = 'INSERT' then
    PERFORM pg_notify('notifytickets', format('INSERT %s %s', NEW.TICKETID, NEW.CREATED));
    ELSIF TG_OP = 'UPDATE' then
    PERFORM pg_notify('notifytickets', format('UPDATE %s %s', OLD.TICKETID, OLD.CREATED));
    ELSIF TG_OP = 'DELETE' then
    PERFORM pg_notify('notifytickets', format('DELETE %s %s', OLD.TICKETID, OLD.CREATED));
    END IF;
    RETURN NULL;
    END;
  

CREATE TRIGGER any_after_ticket
    AFTER INSERT OR DELETE OR UPDATE 
    ON tickets
    FOR EACH ROW
    EXECUTE PROCEDURE ticketf();
