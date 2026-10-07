<#
 Recrea la base de datos desde cero con el modelo actual (value objects incluidos).
 Úsalo UNA vez después de bajar estos cambios. Requiere: PostgreSQL encendido y la herramienta `dotnet ef`.
 Uso (desde la raíz del repositorio):   .\scripts\reset-db.ps1
 ATENCIÓN: borra todos los datos de la base de desarrollo.
#>
$ErrorActionPreference = "Stop"
$persistence = "src/Infrastructure/FH.Customers.Persistence"
$startup     = "src/Presentation/FH.Customers.Api"

Write-Host "1/4 Borrando base de datos..." -ForegroundColor Cyan
dotnet ef database drop --force --project $persistence --startup-project $startup

Write-Host "2/4 Borrando migraciones anteriores..." -ForegroundColor Cyan
if (Test-Path "$persistence/Migrations") { Remove-Item "$persistence/Migrations" -Recurse -Force }

Write-Host "3/4 Generando migración inicial..." -ForegroundColor Cyan
dotnet ef migrations add InitialCreate --project $persistence --startup-project $startup

Write-Host "4/4 Creando tablas..." -ForegroundColor Cyan
dotnet ef database update --project $persistence --startup-project $startup

Write-Host "Listo. Ahora corre la API y luego .\scripts\smoke-test.ps1" -ForegroundColor Green
