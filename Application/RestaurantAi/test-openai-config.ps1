# DineAI - OpenAI Configuration Test Script
# Tests if your OpenAI API key is properly configured

Write-Host "?? DineAI - OpenAI Configuration Test" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the correct directory
if (-not (Test-Path "RestaurantAi.Mvc.csproj")) {
    Write-Host "? Error: Please run this script from the RestaurantAi.Mvc directory!" -ForegroundColor Red
    exit 1
}

Write-Host "? Correct directory" -ForegroundColor Green
Write-Host ""

# Test 1: Check User Secrets
Write-Host "Test 1: Checking User Secrets..." -ForegroundColor Yellow
try {
    $secrets = dotnet user-secrets list 2>&1
    if ($secrets -match "OpenAI:ApiKey") {
        Write-Host "? PASS: API Key found in User Secrets" -ForegroundColor Green
        
        # Check if it's not empty
        if ($secrets -match "OpenAI:ApiKey = sk-") {
            Write-Host "? PASS: API Key appears valid (starts with 'sk-')" -ForegroundColor Green
        } else {
            Write-Host "??  WARNING: API Key doesn't start with 'sk-'" -ForegroundColor Yellow
        }
    } else {
        Write-Host "? FAIL: No API Key found in User Secrets" -ForegroundColor Red
        Write-Host ""
        Write-Host "Run the setup script first:" -ForegroundColor Yellow
        Write-Host ".\setup-openai.ps1" -ForegroundColor White
        exit 1
    }
} catch {
    Write-Host "? FAIL: Could not read User Secrets: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Check appsettings files
Write-Host "Test 2: Checking appsettings.json..." -ForegroundColor Yellow
$appsettings = Get-Content "appsettings.json" | ConvertFrom-Json
if ($appsettings.OpenAI.Model) {
    Write-Host "? PASS: OpenAI configuration found" -ForegroundColor Green
    Write-Host "   Model: $($appsettings.OpenAI.Model)" -ForegroundColor Gray
    Write-Host "   Endpoint: $($appsettings.OpenAI.Endpoint)" -ForegroundColor Gray
} else {
    Write-Host "??  WARNING: OpenAI configuration not found in appsettings.json" -ForegroundColor Yellow
}

Write-Host ""

# Test 3: Check if API key is in appsettings (security check)
Write-Host "Test 3: Security Check..." -ForegroundColor Yellow
$appsettingsContent = Get-Content "appsettings.json" -Raw
$appsettingsDevContent = Get-Content "appsettings.Development.json" -Raw

$foundInAppSettings = $false
if ($appsettingsContent -match "sk-[a-zA-Z0-9-]+") {
    Write-Host "? SECURITY RISK: API Key found in appsettings.json!" -ForegroundColor Red
    Write-Host "   Remove it immediately! Use User Secrets instead." -ForegroundColor Red
    $foundInAppSettings = $true
}

if ($appsettingsDevContent -match "sk-[a-zA-Z0-9-]+") {
    Write-Host "? SECURITY RISK: API Key found in appsettings.Development.json!" -ForegroundColor Red
    Write-Host "   Remove it immediately! Use User Secrets instead." -ForegroundColor Red
    $foundInAppSettings = $true
}

if (-not $foundInAppSettings) {
    Write-Host "? PASS: No API keys found in appsettings files (secure!)" -ForegroundColor Green
}

Write-Host ""

# Test 4: Check .gitignore
Write-Host "Test 4: Checking .gitignore..." -ForegroundColor Yellow
if (Test-Path "..\.gitignore") {
    $gitignore = Get-Content "..\.gitignore" -Raw
    if ($gitignore -match "appsettings.*\.json" -or $gitignore -match "secrets") {
        Write-Host "? PASS: .gitignore protects sensitive files" -ForegroundColor Green
    } else {
        Write-Host "??  WARNING: .gitignore might not protect appsettings files" -ForegroundColor Yellow
    }
} else {
    Write-Host "??  WARNING: No .gitignore found" -ForegroundColor Yellow
}

Write-Host ""

# Test 5: Check if AIService exists
Write-Host "Test 5: Checking AIService..." -ForegroundColor Yellow
if (Test-Path "Services\AIService.cs") {
    Write-Host "? PASS: AIService.cs found" -ForegroundColor Green
} else {
    Write-Host "??  WARNING: AIService.cs not found" -ForegroundColor Yellow
}

Write-Host ""

# Test 6: Try to build the project
Write-Host "Test 6: Building project..." -ForegroundColor Yellow
$buildOutput = dotnet build --no-restore 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "? PASS: Project builds successfully" -ForegroundColor Green
} else {
    Write-Host "? FAIL: Build errors detected" -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor Gray
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "?? Test Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Summary
$allPassed = $true

if ($secrets -notmatch "OpenAI:ApiKey") {
    Write-Host "? API Key not configured" -ForegroundColor Red
    $allPassed = $false
}

if ($foundInAppSettings) {
    Write-Host "??  Security issues detected" -ForegroundColor Yellow
    $allPassed = $false
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed" -ForegroundColor Red
    $allPassed = $false
}

Write-Host ""

if ($allPassed) {
    Write-Host "? All tests passed! Your OpenAI configuration is ready." -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now:" -ForegroundColor Yellow
    Write-Host "1. Start the application (F5 or 'dotnet run')" -ForegroundColor White
    Write-Host "2. Navigate to the Concierge page" -ForegroundColor White
    Write-Host "3. Test the AI chat" -ForegroundColor White
} else {
    Write-Host "??  Some issues detected. Review the output above." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Need help? Check OPENAI_SETUP.md" -ForegroundColor Cyan
}

Write-Host ""
