#!/usr/bin/env bash
set -euo pipefail

# Simple ERP smoke test for API (DEV). Requires API running (default localhost:3000 or custom host via API_BASE).
# Steps: ping -> create ciclo -> create grupo -> create alumno -> assign alumno -> fetch grupo actual -> list alumnos.

API_BASE=${API_BASE:-http://localhost:3000}
HEADER_JSON=("-H" "Content-Type: application/json")

log() { printf "\n== %s ==\n" "$1"; }

# 0) ping swagger
log "Ping swagger"
curl -s -o /dev/null -w "HTTP %{http_code}\n" "$API_BASE/swagger/index.html"

# 1) create ciclo
log "Crear ciclo"
CICLO_PAYLOAD='{"nombre":"2025-2026","fechaInicio":"2025-08-15","fechaFin":"2026-07-15"}'
CICLO_ID=$(curl -s -X POST "$API_BASE/api/v1/ciclos" "${HEADER_JSON[@]}" -d "$CICLO_PAYLOAD" | jq -r '.id')

echo "Ciclo ID: $CICLO_ID"

# 2) create grupo
log "Crear grupo"
GRUPO_PAYLOAD=$(jq -n --arg cid "$CICLO_ID" '{nombre:"2A", grado:2, turno:"Matutino", cicloEscolarId:$cid}')
GRUPO_ID=$(curl -s -X POST "$API_BASE/api/v1/grupos" "${HEADER_JSON[@]}" -d "$GRUPO_PAYLOAD" | jq -r '.id')

echo "Grupo ID: $GRUPO_ID"

# 3) create alumno
log "Crear alumno"
ALUMNO_PAYLOAD='{"matricula":"A-SMOKE-001","nombre":"Juan","apellido":"Perez","email":"juan.smoke@demo.com"}'
ALUMNO_RESP=$(curl -s -X POST "$API_BASE/api/v1/alumnos" "${HEADER_JSON[@]}" -d "$ALUMNO_PAYLOAD")
ALUMNO_ID=$(echo "$ALUMNO_RESP" | jq -r '.id')

echo "Alumno ID: $ALUMNO_ID"

# 4) assign alumno to grupo
log "Asignar alumno a grupo"
ASIG_PAYLOAD=$(jq -n --arg aid "$ALUMNO_ID" --arg gid "$GRUPO_ID" '{alumnoId:$aid, grupoId:$gid, fechaInicio:"2026-01-01"}')
curl -s -X POST "$API_BASE/api/v1/asignaciones/alumno-grupo" "${HEADER_JSON[@]}" -d "$ASIG_PAYLOAD"

echo "Asignación lista"

# 5) get current group
log "Grupo actual del alumno"
curl -s "$API_BASE/api/v1/alumnos/$ALUMNO_ID/grupo-actual"

echo

# 6) list alumnos
log "Listar alumnos"
curl -s "$API_BASE/api/v1/alumnos"

echo
