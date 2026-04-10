#!/usr/bin/env bash
# =============================================================================
# Lothal Agent Aliases
# =============================================================================
# Usage: source this file in your shell profile or manually before a session.
#
#   source .antigravity/aliases.sh
#
# Then use:
#   ag-junior   "Fix the null checks in BasketService.cs"
#   ag-mid      "Add a RemoveItem endpoint to BasketController"
#   ag-senior   "Create a new Coupon microservice"
# =============================================================================

# Resolve the project root relative to this file's location so aliases work
# regardless of which directory the shell is in when the file is sourced.
_LOTHAL_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

ag-junior() {
  antigravity --instructions "${_LOTHAL_ROOT}/.antigravity/agents/junior.md" "$@"
}

ag-mid() {
  antigravity --instructions "${_LOTHAL_ROOT}/.antigravity/agents/mid.md" "$@"
}

ag-senior() {
  antigravity --instructions "${_LOTHAL_ROOT}/.antigravity/agents/senior.md" "$@"
}

echo "✅  Lothal agent aliases loaded: ag-junior | ag-mid | ag-senior"
