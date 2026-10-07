<#
 Prueba de humo de TODO el microservicio (Customers, Subscriptions, Beneficiaries, Audit).
 Uso (con la API corriendo):   .\scripts\smoke-test.ps1
 Otra URL:                     .\scripts\smoke-test.ps1 -BaseUrl https://localhost:7035
 Cada corrida usa documentos únicos, se puede repetir cuantas veces quieras.
#>
param([string]$BaseUrl = "https://localhost:7035")

$script:results = @()
$run = Get-Random -Minimum 100000 -Maximum 999999
$tmpBody = Join-Path $env:TEMP "fh-smoke-body.json"
$tmpOut  = Join-Path $env:TEMP "fh-smoke-out.txt"

# Usa curl.exe (incluido en Windows 10/11): -k acepta el certificado de desarrollo de https://localhost
function Call([string]$Name, [string]$Method, [string]$Path, $Body, [int]$Expected) {
    $args = @("-k", "-s", "-o", $tmpOut, "-w", "%{http_code}", "-X", $Method, "$BaseUrl$Path")
    if ($null -ne $Body) {
        $json = $Body | ConvertTo-Json -Depth 5
        [System.IO.File]::WriteAllText($tmpBody, $json, (New-Object System.Text.UTF8Encoding($false)))
        $args += @("-H", "Content-Type: application/json", "--data-binary", "@$tmpBody")
    }
    $code = 0; $text = ""
    try {
        $out = & curl.exe @args 2>&1
        $code = [int]("$out".Trim())
        if (Test-Path $tmpOut) { $text = [System.IO.File]::ReadAllText($tmpOut, [System.Text.Encoding]::UTF8) }
    } catch { $code = 0; $text = $_.Exception.Message }
    $ok = ($code -eq $Expected)
    $script:results += [pscustomobject]@{ Prueba = $Name; Esperado = $Expected; Recibido = $code; Resultado = $(if ($ok) { "OK" } else { "FALLA" }) }
    if (-not $ok) {
        $detalle = if ($code -eq 0) { "no hubo conexión: ¿la API está corriendo en $BaseUrl ?" } else { $text }
        Write-Host "  [FALLA] $Name -> esperaba $Expected, recibió $code : $detalle" -ForegroundColor Red
    }
    if ($text -and $text.TrimStart().StartsWith("{")) { return ($text | ConvertFrom-Json) }
    if ($text -and $text.TrimStart().StartsWith("[")) { return ,($text | ConvertFrom-Json) }
    return $null
}

$doc = "SMK$run"

# ---------- Salud ----------
Call "Salud" GET "/health" $null 200 | Out-Null

# ---------- Customers ----------
$cust = Call "Crear cliente" POST "/api/v1/customers" @{ customerType = "Individual"; name = "Cliente Prueba $run"; identificationType = "CC"; identificationNumber = $doc; email = "prueba$run@example.com"; phone = "+573001112233"; address = "Calle 1" } 201
Call "Crear cliente duplicado (409)" POST "/api/v1/customers" @{ customerType = "Individual"; name = "Otro"; identificationType = "CC"; identificationNumber = $doc } 409 | Out-Null
Call "Crear cliente con correo inválido (400, FluentValidation)" POST "/api/v1/customers" @{ customerType = "Individual"; name = "Ana"; identificationType = "CC"; identificationNumber = "E$run"; email = "no-es-correo" } 400 | Out-Null
Call "Crear cliente sin nombre (400)" POST "/api/v1/customers" @{ customerType = "Individual"; name = ""; identificationType = "CC"; identificationNumber = "X$run" } 400 | Out-Null
if (-not $cust) { Write-Host "No se pudo crear el cliente: se detiene la prueba." -ForegroundColor Red; $script:results | Format-Table -AutoSize; exit 1 }
$cid = $cust.id
Call "Listar clientes" GET "/api/v1/customers" $null 200 | Out-Null
Call "Cliente por id" GET "/api/v1/customers/$cid" $null 200 | Out-Null
Call "Cliente por documento" GET "/api/v1/customers/by-identification?type=CC&number=$doc" $null 200 | Out-Null
Call "Mismo número con otro tipo de documento es otro cliente (201)" POST "/api/v1/customers" @{ customerType = "Corporate"; name = "Empresa $run"; identificationType = "NIT"; identificationNumber = $doc } 201 | Out-Null
Call "Cliente por documento en minúsculas (se normaliza)" GET "/api/v1/customers/by-identification?type=cc&number=$($doc.ToLower())" $null 200 | Out-Null
Call "Cliente inexistente (404)" GET "/api/v1/customers/11111111-2222-3333-4444-555555555555" $null 404 | Out-Null
Call "Actualizar cliente" PUT "/api/v1/customers/$cid" @{ name = "Cliente Prueba Editado"; email = "nuevo$run@example.com"; phone = "+573009998877"; address = "Calle 2" } 200 | Out-Null
Call "Suspender cliente" PATCH "/api/v1/customers/$cid/status" @{ status = "Suspended" } 204 | Out-Null
Call "Reactivar cliente" PATCH "/api/v1/customers/$cid/status" @{ status = "Active" } 204 | Out-Null

# ---------- Subscriptions ----------
$sub = Call "Crear suscripción" POST "/api/v1/subscriptions" @{ customerId = $cid; externalPlanId = "99999999-9999-9999-9999-999999999999"; maxBeneficiaries = 4; startDate = (Get-Date).ToString("yyyy-MM-dd") } 201
Call "Suscripción de cliente inexistente (404)" POST "/api/v1/subscriptions" @{ customerId = "11111111-2222-3333-4444-555555555555"; externalPlanId = "99999999-9999-9999-9999-999999999999"; maxBeneficiaries = 4; startDate = (Get-Date).ToString("yyyy-MM-dd") } 404 | Out-Null
if (-not $sub) { Write-Host "No se pudo crear la suscripción: se detiene la prueba." -ForegroundColor Red; $script:results | Format-Table -AutoSize; exit 1 }
$sid = $sub.id
Call "Suscripción por id" GET "/api/v1/subscriptions/$sid" $null 200 | Out-Null
Call "Capacidad de suscripción" GET "/api/v1/subscriptions/$sid/capacity" $null 200 | Out-Null
Call "Suscripciones del cliente" GET "/api/v1/customers/$cid/subscriptions" $null 200 | Out-Null
Call "Cambiar cupo máximo" PATCH "/api/v1/subscriptions/$sid/max-beneficiaries" @{ maxBeneficiaries = 5 } 204 | Out-Null

# ---------- Beneficiaries ----------
$benBody = @{ subjectType = "Human"; firstName = "Laura"; birthDate = "1990-05-10"; beneficiaryType = "Associated"; relationshipType = "Spouse"; lastName = "Gómez"; identificationType = "CC"; identificationNumber = "B$run"; email = "laura$run@example.com"; phone = "+573105559876" }
$ben = Call "Agregar beneficiario" POST "/api/v1/subscriptions/$sid/beneficiaries" $benBody 201
Call "Agregar beneficiario duplicado (409)" POST "/api/v1/subscriptions/$sid/beneficiaries" $benBody 409 | Out-Null
if ($ben) {
    $mid = $ben.memberId
    Call "Listar beneficiarios" GET "/api/v1/subscriptions/$sid/beneficiaries" $null 200 | Out-Null
    Call "Actualizar beneficiario (PUT)" PUT "/api/v1/subscriptions/$sid/beneficiaries/$mid" @{ firstName = "Laura Sofía"; birthDate = "1990-05-10"; lastName = "Gómez Pérez"; identificationType = "CC"; identificationNumber = "B$run"; email = "laura.nueva$run@example.com"; phone = "+573105550000" } 200 | Out-Null
    # ---------- Audit ----------
    Call "Auditoría de la suscripción" GET "/api/v1/subscriptions/$sid/audit-log" $null 200 | Out-Null
    Call "Auditoría del miembro" GET "/api/v1/members/$mid/audit-log" $null 200 | Out-Null
    Call "Retirar beneficiario" DELETE "/api/v1/subscriptions/$sid/beneficiaries/$mid" $null 204 | Out-Null
    Call "Retirar de nuevo el mismo beneficiario (409)" DELETE "/api/v1/subscriptions/$sid/beneficiaries/$mid" $null 409 | Out-Null
}
Call "Mascota con relación distinta a Pet (400)" POST "/api/v1/subscriptions/$sid/beneficiaries" @{ subjectType = "Pet"; firstName = "Firulais"; birthDate = "2020-01-01"; beneficiaryType = "Associated"; relationshipType = "Spouse" } 400 | Out-Null
Call "Agregar mascota (201)" POST "/api/v1/subscriptions/$sid/beneficiaries" @{ subjectType = "Pet"; firstName = "Firulais"; birthDate = "2020-01-01"; beneficiaryType = "Associated"; relationshipType = "Pet" } 201 | Out-Null
Call "Suscripción inexistente en beneficiarios (404)" GET "/api/v1/subscriptions/11111111-2222-3333-4444-555555555555/beneficiaries" $null 404 | Out-Null

# ---------- Validaciones (FluentValidation) ----------
Call "Suscripción con cupo 0 (400)" POST "/api/v1/subscriptions" @{ customerId = $cid; externalPlanId = "99999999-9999-9999-9999-999999999999"; maxBeneficiaries = 0; startDate = (Get-Date).ToString("yyyy-MM-dd") } 400 | Out-Null
Call "Beneficiario con fecha de nacimiento futura (400)" POST "/api/v1/subscriptions/$sid/beneficiaries" @{ subjectType = "Human"; firstName = "Futuro"; birthDate = (Get-Date).AddDays(10).ToString("yyyy-MM-dd"); beneficiaryType = "Associated"; relationshipType = "Spouse" } 400 | Out-Null
Call "Beneficiario con tipo de documento sin número (400)" POST "/api/v1/subscriptions/$sid/beneficiaries" @{ subjectType = "Human"; firstName = "Pedro"; birthDate = "1990-01-01"; beneficiaryType = "Associated"; relationshipType = "Spouse"; identificationType = "CC" } 400 | Out-Null

# ---------- Cierre ----------
Call "Cancelar suscripción" PATCH "/api/v1/subscriptions/$sid/cancel" $null 204 | Out-Null

Write-Host ""
$script:results | Format-Table -AutoSize
$fails = ($script:results | Where-Object { $_.Resultado -eq "FALLA" }).Count
if ($fails -eq 0) { Write-Host "TODO OK: $($script:results.Count) pruebas pasaron." -ForegroundColor Green; exit 0 }
else { Write-Host "$fails de $($script:results.Count) pruebas fallaron." -ForegroundColor Red; exit 1 }
