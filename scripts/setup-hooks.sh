#!/bin/bash
# Setup script for development environment
# Run this after cloning the repository

echo "Setting up development environment..."

# Configure git to use project hooks
echo "Configuring git hooks..."
git config core.hooksPath .githooks

# Restore dotnet tools (including CSharpier)
echo "Restoring dotnet tools..."
dotnet tool restore

echo ""
echo "Setup complete!"
echo ""
echo "The following tools are now available:"
echo "  - CSharpier: dotnet csharpier format ."
echo ""
echo "Pre-commit hook is configured to check C# formatting."
