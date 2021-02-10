# AzureCosmosDbBlogExample

## About the code
This repository contains sample code for the article [How to model and partition data on Azure Cosmos DB using a real-world example](https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-model-partition-example).  The code included in this sample is intended to further illustrate the concepts from the article.



## Prerequisites
1. **Visual Studio 2019** - You can download Visual Studio 2019 Community Edition [here](https://visualstudio.microsoft.com/downloads/).
1. **Azure Cosmos DB Emulator** - More information on how to install and use the Azure Cosmos DB emulator can be found [here](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator).
1. **Azure Storage Emulator** - More information on how to install and use Azure Storage Emulator can be found [here](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator).


## Running this sample
1. Clone this repository.
1. From Visual Studio, open the **AzureCosmosDbBlogExample.sln** file from the root directory.
1. In Visual Studio Build menu, select Build Solution (or Press F6).
1. Start Azure Storage Emulator.
1. Start Azure Cosmos DB Emulator.
1. Change the Azure Cosmos Database Name (Optional).

	*The Azure Cosmos DB Database Name defaults to **MyBlog**.  If you want to change this you will need to update the name in **both** the BlogFunctionApp (local.settings.json) and the BlogWebApp (appsettings.json).*

1. Set both projects BlogFunctionApp and BlogWebApp to startup when the solution runs.

	*Only the BlogWebApp project will start when you run the solution.  To also get the Azure Function App BlogFunctionApp to start, right-click on the Solution and choose Set as Startup Project and then set both projects to start.*

1. You can now run and debug the application locally by pressing F5 in Visual Studio.
1. You can register as any username to create a new user (this sample does not require a password for logins).

	*To login as the Blog Administrator, register and login as the username **jsmith**.  The Admin username can be changed in the BlogWebApp appsettings.json file.*

## GitHub Codespaces 
This repository can be run within a GitHub Codespace (Preview).

**From Visual Studio 2019 Preview**

1. Open Visual Studio 2019 Preview.
1. From the home screen click *Connect to a codespace*.
1. Enter the repository url and start your codespace.
1. Open the Terminal (View > Terminal).
1. Run the following to install the prerequisites:
	*devinit init*
	Note: I had to run this command twice.  The first time it takes about 5 minutes while SQL Server is downloaded and installed and then it fails.  The second time it will run successfully but quickly since SQL Server will already be installed.
1. From within the Terminal window, change your directory to *C:\Program Files\Azure Cosmos DB Emulator*.
1. Run *.\Microsoft.Azure.Cosmos.Emulator.exe start*

	`PS C:\Program Files\Azure Cosmos DB Emulator> .\Microsoft.Azure.Cosmos.Emulator.exe start`
1. Now hit F5 to run the application and it will start in your browser.





## More information

- [How to model and partition data on Azure Cosmos DB using a real-world example](https://docs.microsoft.com/en-us/azure/cosmos-db/how-to-model-partition-example)
- [Azure Cosmos DB Documentation](https://docs.microsoft.com/azure/cosmos-db/index)
- [Azure Cosmos DB .NET SDK for SQL API](https://docs.microsoft.com/azure/cosmos-db/sql-api-sdk-dotnet)
- [Azure Cosmos DB .NET SDK Reference Documentation](https://docs.microsoft.com/dotnet/api/overview/azure/cosmosdb?view=azure-dotnet)
