# Setup script for development environment
# Run this after cloning the repository

Write-Host "Setting up development environment..." -ForegroundColor Cyan

# Configure git to use project hooks
Write-Host "Configuring git hooks..." -ForegroundColor Yellow
git config core.hooksPath .githooks

# Restore dotnet tools (including CSharpier)
Write-Host "Restoring dotnet tools..." -ForegroundColor Yellow
dotnet tool restore

Write-Host ""
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "The following tools are now available:"
Write-Host "  - CSharpier: dotnet csharpier format ."
Write-Host ""
Write-Host "Pre-commit hook is configured to check C# formatting."
