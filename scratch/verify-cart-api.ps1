# Integration Test for Cart API and Products API Integration

$ErrorActionPreference = "Stop"

$productsProc = $null
$cartProc = $null

$scratchDir = "C:\Users\beraj\source\repos\juliholcman\TP-CAI-GRUPO-26\scratch"

try {
    # 1. Start Products.API with output redirection
    Write-Host "Starting Products.API..." -ForegroundColor Cyan
    $productsProc = Start-Process dotnet -ArgumentList "run --project src/Products.API/Products.API.csproj --no-build --urls http://localhost:61009" -PassThru -WindowStyle Hidden -RedirectStandardOutput "$scratchDir\products-stdout.log" -RedirectStandardError "$scratchDir\products-stderr.log"

    # 2. Start Cart.API with output redirection
    Write-Host "Starting Cart.API..." -ForegroundColor Cyan
    $cartProc = Start-Process dotnet -ArgumentList "run --project src/Cart.API/Cart.API.csproj --no-build --urls http://localhost:61017" -PassThru -WindowStyle Hidden -RedirectStandardOutput "$scratchDir\cart-stdout.log" -RedirectStandardError "$scratchDir\cart-stderr.log"

    # 3. Wait for endpoints to be ready
    Write-Host "Waiting 8 seconds for services to warm up..." -ForegroundColor Yellow
    Start-Sleep -Seconds 8

    # Base URLs
    $cartUrl = "http://localhost:61017/api/cart"
    $userId = [Guid]::NewGuid().ToString()
    $validProductId = "b69b109d-9c5c-4f68-9942-a0ba2f4710b1" # Lenovo IdeaPad (Stock 12)
    $nonExistentProductId = [Guid]::NewGuid().ToString()

    Write-Host "`n--- Running Tests ---" -ForegroundColor Green

    # Test 1: GET cart when it doesn't exist -> Expect 404 (CRT-001)
    Write-Host "Test 1: GET inactive cart..." -ForegroundColor Cyan
    try {
        Invoke-RestMethod -Uri "$cartUrl/$userId" -Method Get
        Write-Error "Expected 404 but request succeeded."
    } catch {
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $rawBody = $reader.ReadToEnd()
            $body = $rawBody | ConvertFrom-Json
            if ($statusCode -eq 404 -and $body.errorCode -eq "CRT-001") {
                Write-Host "SUCCESS: Returned 404 and errorCode CRT-001 ($($body.errorMessage))" -ForegroundColor Green
            } else {
                Write-Error "FAILED: Expected 404 and CRT-001, got status $statusCode and body: $rawBody"
            }
        } else {
            Write-Error "FAILED: Connection error: $_"
        }
    }

    # Test 2: POST add item with quantity 0 -> Expect 400 (CRT-004)
    Write-Host "`nTest 2: POST item with quantity 0..." -ForegroundColor Cyan
    try {
        $body = @{ productId = $validProductId; cantidad = 0 } | ConvertTo-Json
        Invoke-RestMethod -Uri "$cartUrl/$userId/items" -Method Post -Body $body -ContentType "application/json"
        Write-Error "Expected 400 but request succeeded."
    } catch {
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $rawBody = $reader.ReadToEnd()
            $body = $rawBody | ConvertFrom-Json
            if ($statusCode -eq 400 -and $body.errorCode -eq "CRT-004") {
                Write-Host "SUCCESS: Returned 400 and errorCode CRT-004 ($($body.errorMessage))" -ForegroundColor Green
            } else {
                Write-Error "FAILED: Expected 400 and CRT-004, got status $statusCode and body: $rawBody"
            }
        } else {
            Write-Error "FAILED: Connection error: $_"
        }
    }

    # Test 3: POST add non-existent product -> Expect 404 (CRT-002)
    Write-Host "`nTest 3: POST non-existent product..." -ForegroundColor Cyan
    try {
        $body = @{ productId = $nonExistentProductId; cantidad = 2 } | ConvertTo-Json
        Invoke-RestMethod -Uri "$cartUrl/$userId/items" -Method Post -Body $body -ContentType "application/json"
        Write-Error "Expected 404 but request succeeded."
    } catch {
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $rawBody = $reader.ReadToEnd()
            $body = $rawBody | ConvertFrom-Json
            if ($statusCode -eq 404 -and $body.errorCode -eq "CRT-002") {
                Write-Host "SUCCESS: Returned 404 and errorCode CRT-002 ($($body.errorMessage))" -ForegroundColor Green
            } else {
                Write-Error "FAILED: Expected 404 and CRT-002, got status $statusCode and body: $rawBody"
            }
        } else {
            Write-Error "FAILED: Connection error: $_"
        }
    }

    # Test 4: POST add valid product -> Expect 200
    Write-Host "`nTest 4: POST valid product..." -ForegroundColor Cyan
    $body = @{ productId = $validProductId; cantidad = 5 } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "$cartUrl/$userId/items" -Method Post -Body $body -ContentType "application/json"
    if ($response.userId -eq $userId -and $response.items.Count -eq 1 -and $response.items[0].cantidad -eq 5) {
        Write-Host "SUCCESS: Product added, quantity is 5." -ForegroundColor Green
    } else {
        Write-Error "FAILED: Cart response is invalid: $($response | ConvertTo-Json)"
    }

    # Test 5: POST add quantity exceeding stock (total will be 5 + 10 = 15 > 12) -> Expect 422 (CRT-003)
    Write-Host "`nTest 5: POST quantity exceeding stock..." -ForegroundColor Cyan
    try {
        $body = @{ productId = $validProductId; cantidad = 10 } | ConvertTo-Json
        Invoke-RestMethod -Uri "$cartUrl/$userId/items" -Method Post -Body $body -ContentType "application/json"
        Write-Error "Expected 422 but request succeeded."
    } catch {
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode.value__
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $rawBody = $reader.ReadToEnd()
            $body = $rawBody | ConvertFrom-Json
            if ($statusCode -eq 422 -and $body.errorCode -eq "CRT-003") {
                Write-Host "SUCCESS: Returned 422 and errorCode CRT-003 ($($body.errorMessage))" -ForegroundColor Green
            } else {
                Write-Error "FAILED: Expected 422 and CRT-003, got status $statusCode and body: $rawBody"
            }
        } else {
            Write-Error "FAILED: Connection error: $_"
        }
    }

    # Test 6: PUT update item quantity to valid amount (8 <= 12) -> Expect 200
    Write-Host "`nTest 6: PUT update quantity to 8..." -ForegroundColor Cyan
    $body = @{ cantidad = 8 } | ConvertTo-Json
    $response = Invoke-RestMethod -Uri "$cartUrl/$userId/items/$validProductId" -Method Put -Body $body -ContentType "application/json"
    if ($response.items[0].cantidad -eq 8) {
        Write-Host "SUCCESS: Quantity updated to 8." -ForegroundColor Green
    } else {
        Write-Error "FAILED: Expected quantity 8, got $($response.items[0].cantidad)"
    }

    # Test 7: GET cart -> Expect 200 with correct total
    Write-Host "`nTest 7: GET active cart..." -ForegroundColor Cyan
    $response = Invoke-RestMethod -Uri "$cartUrl/$userId" -Method Get
    if ($response.userId -eq $userId -and $response.total -gt 0) {
        Write-Host "SUCCESS: Cart retrieved, total is $($response.total)" -ForegroundColor Green
    } else {
        Write-Error "FAILED: GET response invalid: $($response | ConvertTo-Json)"
    }

    # Test 8: DELETE remove item -> Expect 204
    Write-Host "`nTest 8: DELETE remove item..." -ForegroundColor Cyan
    $response = Invoke-WebRequest -Uri "$cartUrl/$userId/items/$validProductId" -Method Delete
    if ($response.StatusCode -eq 204) {
        Write-Host "SUCCESS: Item removed (204 No Content)" -ForegroundColor Green
    } else {
        Write-Error "FAILED: Expected 204, got $($response.StatusCode)"
    }

    # Test 9: DELETE empty cart -> Expect 204
    Write-Host "`nTest 9: DELETE clear/empty cart..." -ForegroundColor Cyan
    # Let's add an item back first so we have an active cart to empty
    $body = @{ productId = $validProductId; cantidad = 2 } | ConvertTo-Json
    $null = Invoke-RestMethod -Uri "$cartUrl/$userId/items" -Method Post -Body $body -ContentType "application/json"
    
    $response = Invoke-WebRequest -Uri "$cartUrl/$userId" -Method Delete
    if ($response.StatusCode -eq 204) {
        Write-Host "SUCCESS: Cart cleared (204 No Content)" -ForegroundColor Green
    } else {
        Write-Error "FAILED: Expected 204, got $($response.StatusCode)"
    }

    Write-Host "`nAll Integration Tests Passed Successfully!" -ForegroundColor Green

} finally {
    Write-Host "`nCleaning up processes..." -ForegroundColor Yellow
    if ($productsProc) {
        Stop-Process -Id $productsProc.Id -Force -ErrorAction SilentlyContinue
    }
    if ($cartProc) {
        Stop-Process -Id $cartProc.Id -Force -ErrorAction SilentlyContinue
    }
}
