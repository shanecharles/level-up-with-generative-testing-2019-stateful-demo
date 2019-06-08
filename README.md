# Stateful Testing with FsCheck

## Directory List

- api: ASPNET Core application as our system under test
- sql: Script to generate the database required for the API
- tests: Contains the stateful_testing code

## Setup and Running

- Create the database using the script found in `sql/`
- Change the connection string in the `appsettings.Development.json`
- Run the api project
- In the `tests` directory, execute `build.cmd` (Windows) or `build.sh` (Not Windows) 
  - pulls in required packages
- Open the `tests/stateful_tests/test.fsx` and load it into F# Interactive (FSI)
  - might have to change the `baseUrl` to match your system
  - `Check.One` is the line that starts this stateful testing
