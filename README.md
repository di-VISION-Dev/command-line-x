# command-line-x
![Tool](https://img.shields.io/badge/.Net-8-lightblue) [<img src="https://img.shields.io/github/v/release/di-VISION-Dev/command-line-x" title="Latest">](../../releases/latest)

[System.CommandLine](https://github.com/dotnet/command-line-api) is a really handy .Net library for parsing command line arguments passed to an application. Unfortunately some useful features available in its beta stage were removed from the library's release candidates. CommandLineX brings back some of them: **hosting extensions** and **arguments model binding** including DI.

## Getting Started
### Creating Console App
1. Open terminal in the directory where your project usually are located.
1. Execute
   ```sh
   dotnet new console -n MyConsoleApp --no-restore
   ```
1. Navigate to the project directory.
1. Open `MyConsoleApp.csproj` in XML editor of your choice.
1. Change project SDK by replacing `<Project Sdk="Microsoft.NET.Sdk">` with `<Project Sdk="Microsoft.NET.Sdk.Worker">` (needed for hosting integration).

### Installing The Library
In the directory of your project execute
```sh
dotnet add package diVISION.CommandLineX
```

### Integrating The Library In Your Application
1. Create `MyFirstAction.cs` (command action model) in your project directory:
   ```cs
    using diVISION.CommandLineX;

    namespace MyConsoleApp
    {
        public class MyFirstAction(ILogger<MyFirstAction> logger): ICommandAction
        {
            // injected by the host via DI
            private readonly ILogger<MyFirstAction> _logger = logger;
        
            // "do-amount" argument
            public IEnumerable<int> DoAmount { get; set; } = [0];
            // "--directory" option
            public DirectoryInfo Directory { get; set; } = new (".");
        
            public int Invoke(CommandActionContext context)
            {
                return InvokeAsync(context).Result;
            }

            public Task<int> InvokeAsync(CommandActionContext context, CancellationToken cancellationToken = default)
            {
                _logger.LogDebug("Starting work");
                Console.WriteLine($"Doing it {string.Join(" then ", DoAmount)} times on {Directory.FullName}");
                return Task.FromResult(DoAmount.FirstOrDefault());
            }
        
        }
    }
   ```
1. Modify `Program.cs` (`Main` method resp. depending on whether `--use-program-main` option used with `dotnet new`):
   ```cs
    using diVISION.CommandLineX.Hosting;
    using System.CommandLine;

    var rootCmd = new RootCommand("Running commands");

    var builder = Host.CreateDefaultBuilder(args);
    builder
        .ConfigureAppConfiguration((config) =>
        {
            config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
        })
        .ConfigureDefaults(null)
        .UseConsoleLifetime()
        // next 2 calls initalize CommandLine hosting
        .UseCommandLine(rootCmd)
        .UseCommandWithAction<MyFirstAction>(rootCmd, new("myFirst", "Doing things")
        {
            new Argument<IEnumerable<int>>("do-amount")
            {
                Arity = new(1, 2)
            },
            new Option<DirectoryInfo>("-d", ["--directory"])
            {
                Description = "Some directory to use",
                DefaultValueFactory = (_) => new DirectoryInfo(".")
            }
        });

    using var host = builder.Build();

    return await host.RunCommandLineAsync(args);
   ```
### Running Your App
Execute one of the following in your project directory
```sh
dotnet run -- myFirst 42
```
```sh
dotnet run -- myFirst 42 43
```
```sh
dotnet run -- myFirst 42 43 -d someOtherDirectory
```
You can also request help on commands like
```sh
dotnet run -- -?
```
```sh
dotnet run -- myFirst -?
```

## Building
For building the application .NET SDK 8.x is required (recommended: Visual Studio or Visual Studio Code).

After cloning the repository you can either open the solution `CommandLineX.sln` in your IDE and hit "Build" or open the terminal in the solution directory and execute
```sh
dotnet build
```

## Contributing
All contributions to development and error fixing are welcome. Please always use `develop` branch for forks and pull requests, `main` is reserved for stable releases and critical vulnarability fixes only. Please note: all code changes should meet minimal code coverage requirements to be merged into `main` or `develop`.