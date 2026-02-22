#!/usr/bin/env powershell

# Watchdog Test Script - Demonstrates all key features

param(
    [switch]$TestBasic,
    [switch]$TestWithNotepad,
    [switch]$TestWithArgs,
    [switch]$All
)

function Test-WatchdogExists {
    $watchdogPath = "f:\AMSM\bin\Debug\net6.0\Watchdog.exe"
    if (-not (Test-Path $watchdogPath)) {
        Write-Host "❌ Watchdog.exe not found at: $watchdogPath" -ForegroundColor Red
        Write-Host "Please build the project first: dotnet build" -ForegroundColor Yellow
        return $false
    }
    return $true
}

function Show-Header {
    param([string]$Title)
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║ $($Title.PadRight(45)) ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
}

function Show-Instructions {
    Write-Host "Instructions:" -ForegroundColor Green
    Write-Host ""
}

function Test-Basic {
    Show-Header "Test 1: Help/Usage Message"
    
    Write-Host "Command: Watchdog.exe (no arguments)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Expected Output: Usage instructions should be displayed" -ForegroundColor Cyan
    Write-Host ""
    
    & "f:\AMSM\bin\Debug\net6.0\Watchdog.exe"
    
    Write-Host ""
    Write-Host "✅ Test completed. Press Enter to continue..." -ForegroundColor Green
    Read-Host
}

function Test-WithNotepad {
    Show-Header "Test 2: Monitor Notepad (Interactive Test)"
    
    Show-Instructions
    Write-Host "1. Run: Watchdog.exe notepad.exe" -ForegroundColor Cyan
    Write-Host "2. Notepad will open with watchdog monitoring it" -ForegroundColor Cyan
    Write-Host "3. Close Notepad - it should automatically restart" -ForegroundColor Cyan
    Write-Host "4. Close Notepad again - it should restart again" -ForegroundColor Cyan
    Write-Host "5. Press Ctrl+C in the watchdog window to stop" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Starting watchdog..." -ForegroundColor Green
    Write-Host ""
    
    & "f:\AMSM\bin\Debug\net6.0\Watchdog.exe" notepad.exe
}

function Test-WithArguments {
    Show-Header "Test 3: Monitor Calculator"
    
    Show-Instructions
    Write-Host "1. Run: Watchdog.exe calc.exe" -ForegroundColor Cyan
    Write-Host "2. Calculator will open with watchdog monitoring it" -ForegroundColor Cyan
    Write-Host "3. Close Calculator - it should automatically restart" -ForegroundColor Cyan
    Write-Host "4. Press Ctrl+C in watchdog to stop" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Starting watchdog..." -ForegroundColor Green
    Write-Host ""
    
    & "f:\AMSM\bin\Debug\net6.0\Watchdog.exe" "C:\Windows\System32\calc.exe"
}

function Show-MainMenu {
    Write-Host ""
    Write-Host "╔════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║    Watchdog Application - Test Suite          ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Select a test to run:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1) Show Usage/Help Message" -ForegroundColor Green
    Write-Host "  2) Test with Notepad (Interactive)" -ForegroundColor Green
    Write-Host "  3) Test with Calculator (Interactive)" -ForegroundColor Green
    Write-Host "  4) Run All Tests" -ForegroundColor Green
    Write-Host "  Q) Quit" -ForegroundColor Green
    Write-Host ""
}

# Main execution

if (-not (Test-WatchdogExists)) {
    exit 1
}

if ($All) {
    Test-Basic
    Start-Sleep -Seconds 2
    Test-WithNotepad
    Test-WithArguments
} elseif ($TestBasic) {
    Test-Basic
} elseif ($TestWithNotepad) {
    Test-WithNotepad
} elseif ($TestWithArgs) {
    Test-WithArguments
} else {
    # Interactive menu
    do {
        Show-MainMenu
        $choice = Read-Host "Enter your choice (1-4 or Q)"
        
        switch ($choice.ToUpper()) {
            "1" { Test-Basic }
            "2" { Test-WithNotepad }
            "3" { Test-WithArguments }
            "4" { 
                Test-Basic
                Start-Sleep -Seconds 2
                Test-WithNotepad
                Test-WithArguments
            }
            "Q" { 
                Write-Host "Goodbye!" -ForegroundColor Green
                exit 0
            }
            default {
                Write-Host "Invalid choice. Please try again." -ForegroundColor Red
            }
        }
    } while ($true)
}
