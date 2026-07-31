.PHONY: restore build test demo stop-demo fault docs architecture check clean

restore:
	dotnet restore

build:
	dotnet build src/MotionControl.OperatorConsole --configuration Release
	dotnet build src/MotionControl.Hmi.Wpf --configuration Release

test:
	dotnet test tests/MotionControl.Domain.Tests --configuration Release
	dotnet test tests/MotionControl.Application.Tests --configuration Release
	dotnet test tests/MotionControl.IntegrationTests --configuration Release

demo:
	dotnet run --project src/MotionControl.OperatorConsole -- normal

stop-demo:
	dotnet run --project src/MotionControl.OperatorConsole -- operator-stop

fault:
	dotnet run --project src/MotionControl.OperatorConsole -- probe-timeout

docs:
	python scripts/check_docs.py

architecture:
	python scripts/check_architecture.py

check:
	./scripts/check.sh

clean:
	dotnet clean
	rm -rf .runtime TestResults
