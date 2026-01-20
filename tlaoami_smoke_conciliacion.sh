#!/usr/bin/env bash
set -euo pipefail

# tlaoami_smoke_conciliacion.sh
# Usage: BASE_URL=http://localhost:3000 ./tlaoami_smoke_conciliacion.sh /path/to/estado_cuenta.csv

BASE_URL="${BASE_URL:-http://localhost:3000}"
CSV_PATH="${1:-}"

if [[ -z "${CSV_PATH}" ]]; then
  echo "Usage: BASE_URL=<url> $0 <ruta_csv>" >&2
  exit 1
fi
if [[ ! -f "${CSV_PATH}" ]]; then
  echo "[ERROR] CSV no encontrado: ${CSV_PATH}" >&2
  exit 1
fi

ok_count=0
fail_count=0
start_ts=$(date +%s)

log() {
  echo "[$(date '+%Y-%m-%d %H:%M:%S')] $*"
}

preview_json() {
  local body_file="$1"
  # Compact preview to 300 chars
  if command -v jq >/dev/null 2>&1; then
    jq -c . <"${body_file}" | head -c 300 || true
  else
    head -c 300 "${body_file}" || true
  fi
}

is_json_content_type() {
  local ct="$1"
  local ct_lower
  ct_lower=$(echo "${ct}" | tr '[:upper:]' '[:lower:]')
  [[ "${ct_lower}" == application/json* || "${ct_lower}" == application/problem+json* ]]
}

# request METHOD URL [extra curl args...]
request() {
  local method="$1"; shift
  local url="$1"; shift
  local tmp_h
  local tmp_b
  tmp_h=$(mktemp)
  tmp_b=$(mktemp)

  # shellcheck disable=SC2068
  if ! curl -sS -D "${tmp_h}" -o "${tmp_b}" -X "${method}" "$url" -H 'Accept: application/json' -H 'User-Agent: tlaoami-smoke' "$@"; then
    log "HTTP (curl error) ${method} ${url}"
    rm -f "${tmp_h}" "${tmp_b}"
    return 1
  fi

  LAST_STATUS=$(awk 'toupper($1) ~ /^HTTP\// {code=$2} END{print code+0}' "${tmp_h}")
  LAST_CTYPE=$(awk -F': ' 'tolower($1)=="content-type"{print $2}' "${tmp_h}" | tr -d '\r' | tail -n1)
  LAST_BODY_FILE="${tmp_b}"
  LAST_HDR_FILE="${tmp_h}"

  log "HTTP ${LAST_STATUS} ${method} ${url}"
  preview_json "${LAST_BODY_FILE}"; echo

  return 0
}

assert_json_body() {
  local ok=0
  if ! is_json_content_type "${LAST_CTYPE}"; then
    log "[ERROR] Content-Type no JSON: '${LAST_CTYPE}'"
    ok=1
  fi
  if ! jq -e . <"${LAST_BODY_FILE}" >/dev/null 2>&1; then
    log "[ERROR] Respuesta no es JSON válido"
    ok=1
  fi
  if [[ ${ok} -ne 0 ]]; then
    return 1
  fi
  return 0
}

run_test() {
  local name="$1"; shift
  log "--- TEST: ${name} ---"
  set +e
  "$@"
  local rc=$?
  set -e
  if [[ $rc -eq 0 ]]; then
    ok_count=$((ok_count+1))
    log "OK: ${name}"
    return 0
  else
    fail_count=$((fail_count+1))
    log "FAIL: ${name}"
    return 1
  fi
}

# 0) Ping
step_ping() {
  request GET "${BASE_URL}/swagger/v1/swagger.json" || return 1
  assert_json_body || {
    # Try health as fallback if swagger failed JSON
    request GET "${BASE_URL}/api/v1/health" || return 1
    assert_json_body || return 1
  }
}

# 1) Importar CSV
step_importar() {
  request POST "${BASE_URL}/api/v1/conciliacion/importar-estado-cuenta" -F "archivoCsv=@${CSV_PATH};type=text/csv" || return 1
  assert_json_body || return 1
  # Optional sanity: check object with expected-ish fields
  if ! jq -e 'type=="object"' <"${LAST_BODY_FILE}" >/dev/null 2>&1; then
    log "[ERROR] Importación no regresó objeto JSON"
    return 1
  fi
}

# 2) Listar movimientos
MOV_ID=""
step_listar_movs() {
  request GET "${BASE_URL}/api/v1/conciliacion/movimientos?estado=1&tipo=1&page=1&pageSize=20" || return 1
  assert_json_body || return 1
  if ! jq -e 'type=="array"' <"${LAST_BODY_FILE}" >/dev/null 2>&1; then
    log "[ERROR] Movimientos no regresó arreglo JSON"
    return 1
  fi
  MOV_ID=$(jq -r '.[0]?.id // empty' <"${LAST_BODY_FILE}")
  if [[ -n "${MOV_ID}" ]]; then
    log "Primer movimiento id: ${MOV_ID}"
  else
    log "Sin movimientos disponibles para pruebas dependientes"
  fi
}

# 3) Sugerencias (si hay movimiento)
step_sugerencias() {
  if [[ -z "${MOV_ID}" ]]; then
    log "Skip sugerencias (no hay movimiento)"
    return 0
  fi
  request GET "${BASE_URL}/api/v1/conciliacion/${MOV_ID}/sugerencias" || return 1
  assert_json_body || return 1
  jq -e 'type=="array"' <"${LAST_BODY_FILE}" >/dev/null 2>&1 || {
    log "[ERROR] Sugerencias no regresó arreglo"
    return 1
  }
}

# 4) Conciliar + idempotencia
step_conciliar() {
  if [[ -z "${MOV_ID}" ]]; then
    log "Skip conciliar (no hay movimiento)"
    return 0
  fi
  request POST "${BASE_URL}/api/v1/conciliacion/conciliar" -H 'Content-Type: application/json' -d "{\"movimientoBancarioId\":\"${MOV_ID}\"}" || return 1
  assert_json_body || return 1
  # Reintento idempotente
  request POST "${BASE_URL}/api/v1/conciliacion/conciliar" -H 'Content-Type: application/json' -d "{\"movimientoBancarioId\":\"${MOV_ID}\"}" || return 1
  assert_json_body || return 1
}

# 5) Revertir (con motivo extra ignorado por el backend)
step_revertir() {
  if [[ -z "${MOV_ID}" ]]; then
    log "Skip revertir (no hay movimiento)"
    return 0
  fi
  request POST "${BASE_URL}/api/v1/conciliacion/revertir" -H 'Content-Type: application/json' -d "{\"movimientoBancarioId\":\"${MOV_ID}\",\"motivo\":\"smoke-test\"}" || return 1
  assert_json_body || return 1
}

# 6) Negative tests
step_negativos() {
  # Ruta inexistente → 404 JSON
  request GET "${BASE_URL}/__ruta_inexistente__" || return 1
  if [[ "${LAST_STATUS}" -ne 404 ]]; then
    log "[ERROR] Se esperaba 404 en ruta inexistente"
    return 1
  fi
  assert_json_body || return 1

  # Conciliar con Guid.Empty → 4xx ProblemDetails JSON
  request POST "${BASE_URL}/api/v1/conciliacion/conciliar" -H 'Content-Type: application/json' --data '{"movimientoBancarioId":"00000000-0000-0000-0000-000000000000"}' || true
  if [[ "${LAST_STATUS}" -lt 400 ]]; then
    log "[ERROR] Se esperaba status 4xx al conciliar con Guid.Empty"
    return 1
  fi
  assert_json_body || return 1
  # Preferentemente application/problem+json
  local ct_lower
  ct_lower=$(echo "${LAST_CTYPE}" | tr '[:upper:]' '[:lower:]')
  if ! [[ "${ct_lower}" == application/problem+json* ]]; then
    log "[WARN] Content-Type no es application/problem+json: ${LAST_CTYPE}"
  fi
}

log "Base URL: ${BASE_URL}"
run_test "0) Ping API" step_ping
run_test "1) Importar CSV" step_importar
run_test "2) Listar movimientos" step_listar_movs
run_test "3) Sugerencias" step_sugerencias
run_test "4) Conciliar idempotente" step_conciliar
run_test "5) Revertir" step_revertir
run_test "6) Negative tests" step_negativos

end_ts=$(date +%s)
elapsed=$((end_ts-start_ts))

log "================ SUMMARY ================"
log "OK:   ${ok_count}"
log "FAIL: ${fail_count}"
log "Time: ${elapsed}s"
if [[ ${fail_count} -eq 0 ]]; then
  log "=== ✅ OK ==="
  exit 0
else
  log "=== ❌ FAIL ==="
  exit 1
fi
