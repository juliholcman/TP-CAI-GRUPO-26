#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SOLUTION="$ROOT_DIR/ECommerce.sln"
REQUIRED_MAJOR="9"
SERVICES=(Products.API Users.API Orders.API Cart.API Notifications.API)
PIDS=()

if [ -x "$HOME/.dotnet/dotnet" ]; then
  export PATH="$HOME/.dotnet:$PATH"
elif ! command -v dotnet >/dev/null 2>&1 && [ -x /usr/local/share/dotnet/dotnet ]; then
  export PATH="/usr/local/share/dotnet:$PATH"
fi

usage() {
  cat <<USAGE
Uso: ./project.sh [comando]

Comandos:
  (sin comando) Compila y ejecuta las cinco APIs
  start     Compila y ejecuta las cinco APIs
  info      Muestra la version de .NET instalada
  restore   Restaura dependencias
  build     Compila los microservicios
  run       Ejecuta una API: ./project.sh run Products.API
  watch     Ejecuta una API con hot reload: ./project.sh watch Products.API
  clean     Limpia artefactos de build

Servicios disponibles:
  Products.API, Users.API, Orders.API, Cart.API, Notifications.API

Instalar .NET 9 en macOS si falta:
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel 9.0 --install-dir "\$HOME/.dotnet" --architecture arm64
USAGE
}

ensure_dotnet() {
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "No encontre dotnet en el PATH."
    echo "Instala .NET 9 con dotnet-install.sh en \$HOME/.dotnet."
    exit 1
  fi
}

ensure_dotnet_9() {
  ensure_dotnet

  local installed_versions
  installed_versions="$(dotnet --list-sdks 2>/dev/null || true)"

  if ! printf '%s\n' "$installed_versions" | grep -q "^${REQUIRED_MAJOR}\\."; then
    echo "Esta solucion requiere .NET SDK ${REQUIRED_MAJOR}.x."
    echo
    echo "SDKs instalados:"
    if [ -n "$installed_versions" ]; then
      printf '%s\n' "$installed_versions"
    else
      echo "  ninguno"
    fi
    echo
    echo "Instala .NET 9 con dotnet-install.sh en \$HOME/.dotnet."
    exit 1
  fi
}

project_path() {
  case "${1:-}" in
    Products.API|Users.API|Orders.API|Cart.API|Notifications.API)
      printf '%s/src/%s/%s.csproj\n' "$ROOT_DIR" "$1" "$1"
      ;;
    "")
      echo "Falta indicar el servicio."
      echo "Ejemplo: ./project.sh run Products.API"
      exit 1
      ;;
    *)
      echo "Servicio desconocido: $1"
      echo "Disponibles: Products.API, Users.API, Orders.API, Cart.API, Notifications.API"
      exit 1
      ;;
  esac
}

restore_solution() {
  for service in "${SERVICES[@]}"; do
    dotnet restore "$(project_path "$service")" --ignore-failed-sources /p:NuGetAudit=false
  done
}

build_solution() {
  local service

  for service in "${SERVICES[@]}"; do
    dotnet build "$(project_path "$service")" /p:NuGetAudit=false
  done
}

restore_project() {
  dotnet restore "$(project_path "$1")" --ignore-failed-sources /p:NuGetAudit=false
}

application_urls() {
  local service="$1"
  local settings="$ROOT_DIR/src/$service/Properties/launchSettings.json"

  if [ -f "$settings" ]; then
    sed -n 's/.*"applicationUrl"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$settings" | head -n 1
  fi
}

show_swagger_urls() {
  local urls="$1"
  local url

  IFS=';' read -r -a split_urls <<< "$urls"
  for url in "${split_urls[@]}"; do
    [ -n "$url" ] && echo "    Swagger: ${url%/}/swagger"
  done
}

terminate_tree() {
  local pid="$1"
  local child

  while read -r child; do
    [ -n "$child" ] && terminate_tree "$child"
  done < <(pgrep -P "$pid" 2>/dev/null || true)

  kill -TERM "$pid" 2>/dev/null || true
}

cleanup() {
  trap - INT TERM EXIT

  if [ "${#PIDS[@]}" -gt 0 ]; then
    echo
    echo "Apagando las APIs..."
    for pid in "${PIDS[@]}"; do
      terminate_tree "$pid"
    done
    wait "${PIDS[@]}" 2>/dev/null || true
    echo "Todas las APIs fueron detenidas."
  fi
}

run_all() {
  local service
  local urls
  local project

  echo "Compilando la solucion..."
  build_solution
  echo
  echo "Levantando las APIs..."

  trap cleanup INT TERM EXIT

  for service in "${SERVICES[@]}"; do
    project="$(project_path "$service")"
    urls="$(application_urls "$service")"

    if [ -n "$urls" ]; then
      ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS="$urls" \
        dotnet run --project "$project" --no-build --no-launch-profile &
    else
      ASPNETCORE_ENVIRONMENT=Development \
        dotnet run --project "$project" --no-build &
    fi

    PIDS+=("$!")
    echo "  $service iniciada (PID $!)"
    if [ -n "$urls" ]; then
      show_swagger_urls "$urls"
    else
      echo "    URLs no detectadas en launchSettings.json"
    fi
  done

  echo
  echo "Las cinco APIs son independientes y estan en ejecucion."
  echo "Presiona Ctrl+C para detenerlas."
  wait
}

command="${1:-}"

case "$command" in
  ""|start)
    ensure_dotnet_9
    run_all
    ;;
  info)
    ensure_dotnet
    dotnet --info
    ;;
  restore)
    ensure_dotnet_9
    restore_solution
    ;;
  build)
    ensure_dotnet_9
    restore_solution
    build_solution
    ;;
  run)
    ensure_dotnet_9
    restore_project "${2:-}"
    dotnet run --project "$(project_path "${2:-}")" --no-restore
    ;;
  watch)
    ensure_dotnet_9
    restore_project "${2:-}"
    dotnet watch --project "$(project_path "${2:-}")" run
    ;;
  clean)
    ensure_dotnet_9
    dotnet clean "$SOLUTION"
    ;;
  -h|--help|help)
    usage
    ;;
  *)
    echo "Comando desconocido: $command"
    echo
    usage
    exit 1
    ;;
esac
