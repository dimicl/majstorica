Param(
    [string]$ComposeFilePath = "$PSScriptRoot\docker-compose.yml"
)

Write-Host "== Majstorica DB setup (Mongo + Redis + Neo4j + RabbitMQ) =="

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker nije instaliran ili nije u PATH-u."
    exit 1
}

if (-not (Test-Path $ComposeFilePath)) {
    Write-Error "Nije pronađen docker-compose.yml na lokaciji: $ComposeFilePath"
    exit 1
}

$roles = @{
    "Mongo"    = @("majstorica-mongo", "mongo-lab", "mongo");
    "Redis"    = @("redis-stack", "majstorica-redis", "redis-server", "redis");
    "Neo4j"    = @("majstorica-neo4j", "neo4j", "neo4j-test");
    "RabbitMQ" = @("majstorica-rabbitmq", "rabbitmq");
}

Write-Host "Pokušavam da startujem postojeće kontejnere (bez povlačenja novih slika)..."

$anyExisting = $false

foreach ($role in $roles.Keys) {
    $startedForRole = $false
    foreach ($name in $roles[$role]) {
        $exists = docker ps -a --format "{{.Names}}" | Where-Object { $_ -eq $name }
        if ($exists) {
            $anyExisting = $true
            $startedForRole = $true
            Write-Host "[$role] startujem kontejner: $name"
            docker start $name | Out-Null
            break
        }
    }
    if (-not $startedForRole) {
        Write-Host "[$role] nije pronađen postojeći kontejner po poznatim imenima."
    }
}

if (-not $anyExisting) {
    Write-Host "Nema nijednog postojećeg kontejnera, pokrećem docker compose (prvo kreiranje može da povuče slike)..."
    Write-Host "Komanda: docker compose -f `"$ComposeFilePath`" up -d"
    docker compose -f $ComposeFilePath up -d

    if ($LASTEXITCODE -eq 0) {
        Write-Host "MongoDB, Redis, Neo4j i RabbitMQ su podignuti (kreirani po prvi put)."
    } else {
        Write-Error "Došlo je do greške pri pokretanju docker compose."
        exit $LASTEXITCODE
    }
} else {
    Write-Host "Postojeći kontejneri su startovani (gde su pronađeni)."
}

