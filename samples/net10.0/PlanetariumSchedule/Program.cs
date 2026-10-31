if (args.Length == 2 && args[0] == "--unignored-reload") await ProviderChecks.UnignoredChild(args[1]);
else await ProviderChecks.Run();
