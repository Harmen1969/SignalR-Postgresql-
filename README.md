# SignalR-Postgresql
Real time push notification using SignalR  and PostgreSQL

An implementation of real time notification using SignalR and the PostgreSQL listen/notify feature.

When changes (insert/delete/update) are made in a PostgreSQL database table, the changed content is pushed to the webpage.

# Npgsql
The Npgsql NET data provider for PostgreSQL is not included in this repo.
To install <b>Npgsql</b>, run the following command in the Package Manager Console: Install-Package Npgsql

# Database Objects
CREATE TABLE tickets</br>
(</br>
    TICKETID serial primary key</br>
    CREATED date NOT NULL DEFAULT CURRENT_DATE,</br>
    DATEASSIGNED date,</br>
    TITLE varchar(50),</br>
    DESCRIPTION text,</br>
    PRIORITY varchar(20),</br>
    ASSIGNEDTO varchar(100),</br>
    TICKETSTATUS character varchar(20)</br>
)</br>

 CREATE OR REPLACE FUNCTION ticketf() RETURNS TRIGGER AS $$</br>
    BEGIN</br>
    IF TG_OP = 'INSERT' then</br>
    PERFORM pg_notify('notifytickets', format('INSERT %s %s', NEW.TICKETID, NEW.CREATED));</br>
    ELSIF TG_OP = 'UPDATE' then</br>
    PERFORM pg_notify('notifytickets', format('UPDATE %s %s', OLD.TICKETID, OLD.CREATED));</br>
    ELSIF TG_OP = 'DELETE' then</br>
    PERFORM pg_notify('notifytickets', format('DELETE %s %s', OLD.TICKETID, OLD.CREATED));</br>
    END IF;</br>
    RETURN NULL;</br>
    END;</br>
    $$ LANGUAGE plpgsql;</br>
  

CREATE TRIGGER any_after_ticket</br>
    AFTER INSERT OR DELETE OR UPDATE</br> 
    ON tickets</br>
    FOR EACH ROW</br>
    EXECUTE PROCEDURE ticketf();</br>
