restore:
	dotnet restore src/Xs.Cli.Main/Xs.Cli.Main.csproj

build:
	dotnet build src/Xs.Cli.Main/Xs.Cli.Main.csproj

clean:
	@find . -type d -name bin | xargs rm -rf
	@find . -type d -name obj | xargs rm -rf
	@find . -type f -name coverage.json | xargs rm -rf

.PHONY: $(MAKECMDGOALS)