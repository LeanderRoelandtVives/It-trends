# DineAI - OpenAI API Key Setup Script
# Run this script in PowerShell from the RestaurantAi.Mvc directory

Write-Host "???  DineAI - OpenAI API Key Setup" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the correct directory
if (-not (Test-Path "RestaurantAi.Mvc.csproj")) {
    Write-Host "? Error: Please run this script from the RestaurantAi.Mvc directory!" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Navigate to the correct directory first:" -ForegroundColor Yellow
    Write-Host "cd C:\Users\seppe\Downloads\It-trends\Application\RestaurantAi\RestaurantAi.Mvc" -ForegroundColor White
    exit 1
}

Write-Host "? Correct directory detected" -ForegroundColor Green
Write-Host ""

# Step 1: Initialize User Secrets
Write-Host "?? Step 1: Initializing User Secrets..." -ForegroundColor Yellow
try {
    $output = dotnet user-secrets init 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? User Secrets initialized successfully" -ForegroundColor Green
    } else {
        Write-Host "??  User Secrets already initialized" -ForegroundColor Cyan
    }
} catch {
    Write-Host "? Failed to initialize User Secrets: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Prompt for API Key
Write-Host "?? Step 2: Enter your OpenAI API Key" -ForegroundColor Yellow
Write-Host ""
Write-Host "Don't have one yet? Get it here:" -ForegroundColor White
Write-Host "?? https://platform.openai.com/api-keys" -ForegroundColor Cyan
Write-Host ""
Write-Host "Your key should look like: sk-proj-..." -ForegroundColor Gray
Write-Host ""

$apiKey = Read-Host "Paste your OpenAI API Key here"

if ([string]::IsNullOrWhiteSpace($apiKey)) {
    Write-Host "? No API key provided. Setup cancelled." -ForegroundColor Red
    exit 1
}

# Basic validation
if (-not ($apiKey.StartsWith("sk-"))) {
    Write-Host "??  Warning: API key doesn't start with 'sk-'. Are you sure this is correct?" -ForegroundColor Yellow
    $confirm = Read-Host "Continue anyway? (y/n)"
    if ($confirm -ne "y") {
        Write-Host "Setup cancelled." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# Step 3: Set the API Key
Write-Host "?? Step 3: Saving API Key to User Secrets..." -ForegroundColor Yellow
try {
    dotnet user-secrets set "OpenAI:ApiKey" $apiKey
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? API Key saved successfully!" -ForegroundColor Green
    } else {
        Write-Host "? Failed to save API key" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "? Error saving API key: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 4: Verify configuration
Write-Host "?? Step 4: Verifying configuration..." -ForegroundColor Yellow
try {
    $secrets = dotnet user-secrets list 2>&1
    if ($secrets -match "OpenAI:ApiKey") {
        Write-Host "? Configuration verified!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Your secrets:" -ForegroundColor Cyan
        Write-Host $secrets -ForegroundColor White
    } else {
        Write-Host "??  Warning: Could not verify API key in secrets" -ForegroundColor Yellow
    }
} catch {
    Write-Host "??  Warning: Could not verify configuration: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "? Setup Complete!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Start your application (F5 in Visual Studio)" -ForegroundColor White
Write-Host "2. Navigate to the Concierge page" -ForegroundColor White
Write-Host "3. Test the AI chat functionality" -ForegroundColor White
Write-Host ""
Write-Host "?? For more info, see OPENAI_SETUP.md" -ForegroundColor Cyan
Write-Host ""
Write-Host "?? Tip: Your API key is stored securely in:" -ForegroundColor Yellow
Write-Host "   %APPDATA%\Microsoft\UserSecrets\<user-secrets-id>\secrets.json" -ForegroundColor Gray
Write-Host ""
Write-Host "?? Happy coding!" -ForegroundColor Magenta
Write-Host ""

# Optional: Open the app
$openApp = Read-Host "Would you like to build and run the app now? (y/n)"
if ($openApp -eq "y") {
    Write-Host ""
    Write-Host "?? Building application..." -ForegroundColor Yellow
    dotnet build
    if ($LASTEXITCODE -eq 0) {
        Write-Host "? Build successful!" -ForegroundColor Green
        Write-Host ""
        Write-Host "?? Starting application..." -ForegroundColor Yellow
        dotnet run
    } else {
        Write-Host "? Build failed. Please check the errors above." -ForegroundColor Red
    }
}
