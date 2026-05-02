#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SOLUTION="$ROOT_DIR/ECommerce.sln"
REQUIRED_MAJOR="9"
SERVICES=(Products.API Users.API Orders.API Cart.API Notifications.API)

if [ -x "$HOME/.dotnet/dotnet" ]; then
  export PATH="$HOME/.dotnet:$PATH"
elif ! command -v dotnet >/dev/null 2>&1 && [ -x /usr/local/share/dotnet/dotnet ]; then
  export PATH="/usr/local/share/dotnet:$PATH"
fi

usage() {
  cat <<USAGE
Uso: ./scripts/project.sh <comando>

Comandos:
  info      Muestra la version de .NET instalada
  restore   Restaura dependencias
  build     Compila los microservicios
  run       Ejecuta una API: ./scripts/project.sh run Products.API
  watch     Ejecuta una API con hot reload: ./scripts/project.sh watch Products.API
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
      echo "Ejemplo: ./scripts/project.sh run Products.API"
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
  for service in "${SERVICES[@]}"; do
    dotnet build "$(project_path "$service")" --no-restore /p:NuGetAudit=false
  done
}

restore_project() {
  dotnet restore "$(project_path "$1")" --ignore-failed-sources /p:NuGetAudit=false
}

command="${1:-}"

case "$command" in
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
  ""|-h|--help|help)
    usage
    ;;
  *)
    echo "Comando desconocido: $command"
    echo
    usage
    exit 1
    ;;
esac
