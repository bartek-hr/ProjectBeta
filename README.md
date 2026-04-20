# Restoring NuGet Packages

When you clone or pull this repository, all required NuGet packages (such as Microsoft.EntityFrameworkCore.Sqlite) are listed in the .csproj files. Run `dotnet restore` or simply build the project (`dotnet build`) to automatically download and install all dependencies. No manual package installation is needed.

# ProjectBeta

A C#/.NET console application for cinema reservation management, using SQLite and Entity Framework Core.

## Database Usage

This project uses SQLite with the connection string in `appsettings.json`.
Relative SQLite paths are resolved to the project directory, so local development, IDE runs, `dotnet run`, and EF Core commands all use `ProjectBeta/app.db`.
If you change the connection string to an absolute path, the app will use that exact database file unchanged.

If you already have duplicate database files from older runs, compare the existing copies first and keep the canonical file at `ProjectBeta/app.db`.
After confirming which one contains the data you want, you can manually remove a stray repo-root `app.db`.

**Note:** Do not add comments inside `appsettings.json` (JSON does not support comments).

### Entity Framework Core Migrations

To create or manage migrations, you need the Entity Framework Core CLI tools installed on your machine. These are not included in the .csproj file.

Install the EF Core CLI tools globally (only once per machine):

```
dotnet tool install --global dotnet-ef
```
Note: The project must reference the correct EF Core packages in the .csproj, but the CLI tool is a separate global install.

### Team Workflow: Adding and Applying Migrations and Local Database File
- Whenever you make changes to your models (add, remove, or modify properties/tables), always create a new migration before pushing your changes:
  
  ```sh
  dotnet ef migrations add <MigrationName>
  ```
  Replace `<MigrationName>` with a descriptive name for your change.

- To apply migrations and update the database (while preserving existing data), run:
  
  ```sh
  dotnet ef database update
  ```
  This updates the existing database to match the latest schema.

- If you delete your local database file (e.g., `app.db`), the next time you run the app or apply migrations, a new database will be created from scratch using all existing migrations. The schema will match the latest migration, but any previous data will be lost.

- Always keep your migration files in source control. This allows any developer to recreate the database structure from scratch by running the migrations, even if the database file is missing or deleted.

### Downgrading (Rolling Back) Migrations

If you need to revert your database schema to a previous state (for example, if you switch to a branch that does not have the latest migrations):

1. Find the name of the migration you want to downgrade to. You can list all migrations with:
   
   ```sh
   dotnet ef migrations list
   ```

2. Run the following command to downgrade your database to a specific migration:
   
   ```sh
   dotnet ef database update <MigrationName>
   ```
   Replace `<MigrationName>` with the name of the migration you want to revert to. The database will be rolled back to match the schema at that migration. Any tables or columns added in later migrations will be removed.

3. If your database is ahead of your code (for example, you have a Movies table from a migration that does not exist in your current branch), you may get errors until you downgrade or recreate the database.

4. If you want to reset everything, you can delete the database file and run `dotnet ef database update` to recreate it from the migrations present in your branch.
