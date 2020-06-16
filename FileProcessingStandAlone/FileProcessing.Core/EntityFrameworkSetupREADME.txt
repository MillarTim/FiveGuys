***this should produce a db schema based on the models setup in the FileContext class***

If you want to change the db schema it may be easiest to drop the entire schema.  To do so, you can run the EF commands
from the Package Manager Console.  

Make sure the Default Project is set to CSS.Connector.FileProcessing.Core(EF Project)

helpful commands

drop-database -- drops the database - CAREFUL
remove-migration -- removes the files in the Migrations folder
add-migration initial -- adds new migration with name initial
update-database -- will apply the latest migration to the database
script-migration -- scripts out the create tables scripts to a new SQL file

The UsersTable.sql was a result of the script-migration command.