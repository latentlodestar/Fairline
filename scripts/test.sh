#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(dirname "$SCRIPT_DIR")"

echo "=== Running .NET tests ==="
dotnet test "$ROOT_DIR/Fairline.sln" --verbosity normal

echo ""
echo "=== Running frontend tests ==="
cd "$ROOT_DIR/src/Fairline.Web"
npm test
