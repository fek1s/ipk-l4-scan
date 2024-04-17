# Define default target
.DEFAULT_GOAL := all

# Define phony targets
.PHONY: all clean

all:
	@echo "Building C# projects..."
	dotnet build
    
    

clean:
	@echo "Cleaning up..."
