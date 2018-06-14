# Cycle Shop Bot 

A demo [Azure Bot Framework](https://dev.botframework.com/) sales bot for a fictitious cycling shop.  Say "Hi" and it will guide you on some of the things it can help with.

Highlights:

- An Azure Function-based backend (compiled C# function)
- [Bot Builder C# SDK](https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-overview?view=azure-bot-service-3.0) (v3.15.2.2 - current production/stable release as of this writing)
- LUIS language understanding model for:
    - Triggering different intents/dialogs within the bot
    - Maintaining domain lists for product offerings & regions using LUIS List Entities
- Many bot features:
    - Multiple bot Dialogs and dialog transfer (Call, Fail, etc.)
    - Adaptive Cards and other card support
    - Suggested Actions
    - Choice prompts
    - Postback card actions
    - Example of inspecting current channel and altering behavior per channel
- Calling into Node.js functions hosted side by side with C# functions
- A demo mode to use/demo the bot without needing the SAP instance

## Sample Conversation UI

Via the [Bot Framework Emulator v3](https://github.com/Microsoft/BotFramework-Emulator)

![](/data/images/CycleShopBot.gif)

## Setup

1. Create a bot in the [Azure portal](https://portal.azure.com) using the Functions Bot template.
2. Go to the [LUIS dashboard](https://www.luis.ai/applications) and find the new LUIS application that was created as a part of the bot template.  Import the json model found in the /data folder.  Train and publish the model.
3. Add an [application setting](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-overview?view=azure-bot-service-3.0#app-service-settings) named `MockData` and set its value to `true`.
3. [Download the new bot source](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-build-download-source-code?view=azure-bot-service-3.0) from the portal.  
  
    **NOTE**: You may need to do this from the Functions UI in Azure (Build tab -> "Open this bot in Azure Functions" -> "Download app content", making sure to check the box to include the app settings file).  

4. Grab the `local.settings.json` file from the downloaded content and add/include this into the Cycle Shop solution root.  Verify it contains the newly added `MockData` setting and add it if needed.
5. Build the solution and [Publish your Cycle Shop Bot solution to Azure](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-build-download-source-code?view=azure-bot-service-3.0#publish-c-bot-source-code-to-azure), or [Set up continuous deployment](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-build-continuous-deployment?view=azure-bot-service-3.0) via your source code repo.
6. Once the Function(s) have been deployed in Azure, you may need to [review and change the messaging endpoint](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-settings?view=azure-bot-service-3.0) for your bot.  Your messaging endpoint can be found in the Azure Functions UI via the "Get function URL" link when you've selected the `CycleShopBot` Function.
7. Test your bot via the [Test in Web Chat feature](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-test-webchat?view=azure-bot-service-3.0).
8. [Enable any Channels](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-channels?view=azure-bot-service-3.0) you want the bot to be available on.

Having trouble?  Log an issue here and I'll try to help.

## Debugging

To learn how to debug Azure Bot Service bots, please visit [https://aka.ms/bf-docs-azure-debug](https://aka.ms/bf-docs-azure-debug).

To debug locally, you'll need to [configure your dev environment](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs) and [run/debug the Azure function(s) locally](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local).  Then you can use the locally running bot by [connecting the Bot Framework Emulator to the locally running bot Function](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-debug-emulator?view=azure-bot-service-3.0).

# About

## Contributors

- [Nate Rickard](https://github.com/NateRickard)
- [Steve Hall](https://github.com/srhallx)

## License

Licensed under the MIT License (MIT). See [LICENSE](/LICENSE) for details.