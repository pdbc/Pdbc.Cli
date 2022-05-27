# Pdbc.Cli

Cli for class generation (roslyn)

## Cli

aercli.exe -e Route -p Routes -a Store

## Getting started

Generate the solution structure with the template

CI/CD changes
- setup the libraries 
- run the pipeline

### What is generated

Generate an entity in the solution
 - add the properites to the entity
 - implement the test data builder (to generate default entities)
 - create the database + setup user
 - adjust the appSettings.json   
   - verify the connectionstrings 
 - run the migrator
 - verify the repository tests


DTO 
 - 
 - add the properties to the dto objects
 - implement the test data builder (to generate default entities)

Services

Controller

## Next steps

1. Adapt the domain models
2. Adjust the entity framework configuration settings (include the validation constants)
3. Generate the InitialDatabase (dotnet ef migrations add InitialSetup --project Locations.Data)
4. Run the database migrator
   5. setup the database sd-(ApplicationName) and make srv_integrationtests / development dbowner
5. Run the UnitTests
6. Adjust the Domain TestDataBuilder
7. Run the Data IntegrationTests

