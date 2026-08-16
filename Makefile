.PHONY: all help build-debug build-release test-unit clean run-debug run-release

# Configuration
PROJECT_DIR := src/Crai.Desktop
BUILD_DIR := $(PROJECT_DIR)/bin/Debug/net10.0

# Help
help:
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  build-debug      Build in Debug mode"
	@echo "  build-release    Build in Release mode"
	@echo "  test-unit        Run unit tests"
	@echo "  clean            Clean build artifacts"
	@echo "  run-debug        Run in Debug mode"
	@echo "  run-release      Run in Release mode"

# Build
build-debug:
	@dotnet build $(PROJECT_DIR) -c Debug

build-release:
	@dotnet build $(PROJECT_DIR) -c Release

# Test
test-unit:
	@dotnet test tests/Crai.Application.Tests

# Clean
clean:
	@dotnet clean $(PROJECT_DIR) -c Debug
	@dotnet clean $(PROJECT_DIR) -c Release

# Run
run-debug:
	@dotnet run -p $(PROJECT_DIR) -c Debug

run-release:
	@dotnet run -p $(PROJECT_DIR) -c Release
