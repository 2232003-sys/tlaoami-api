# 🧪 Smoke: Colegiaturas Mensuales (<=8 pasos)

Prep rápido
```bash
API="http://localhost:3000"
BASE_URL="$API/api/v1"
TOKEN="<token-preautenticado>"   # usa el token que ya tengas o exporta el de tu entorno
```

1) Crear conceptos COLEGIATURA y RECARGO (solo si no existen)
```bash
curl -X POST "$BASE_URL/ConceptosCobro" -H "$AUTH" -H "Content-Type: application/json" \
  -d '{"clave":"COLEGIATURA","nombre":"Colegiatura mensual","periodicidad":"Mensual","requiereCFDI":true}'
curl -X POST "$BASE_URL/ConceptosCobro" -H "$AUTH" -H "Content-Type: application/json" \
  -d '{"clave":"RECARGO","nombre":"Recargo por mora","periodicidad":null,"requiereCFDI":true}'
```

2) Crear ReglaColegiatura (grupo + monto + día vencimiento)
```bash
COLEGIATURA_ID="<id-concepto-colegiatura>"
curl -X POST "$BASE_URL/ReglasColegiatura" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"cicloId\":\"$CICLO_ID\",\"grupoId\":\"$GRUPO_ID\",\"conceptoCobroId\":\"$COLEGIATURA_ID\",\"montoBase\":5500,\"diaVencimiento\":5}"
```

3) Crear BecaAlumno para 1 alumno (opcional)
```bash
curl -X POST "$BASE_URL/Becas" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"alumnoId\":\"$ALUMNO_ID\",\"cicloId\":\"$CICLO_ID\",\"tipo\":0,\"valor\":0.10}"
```

4) Generar periodo 2026-02 (dryRun)
```bash
curl -X POST "$BASE_URL/Colegiaturas/generar" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"cicloId\":\"$CICLO_ID\",\"grupoId\":\"$GRUPO_ID\",\"periodo\":\"2026-02\",\"dryRun\":true}"
# Esperado: creadas > 0, sin escritura
```

5) Generar periodo 2026-02 (persistir, borrador por default)
```bash
curl -X POST "$BASE_URL/Colegiaturas/generar" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"cicloId\":\"$CICLO_ID\",\"grupoId\":\"$GRUPO_ID\",\"periodo\":\"2026-02\",\"dryRun\":false,\"emitir\":false}"
# Esperado: facturas en BORRADOR con una línea de colegiatura (Monto ajustado por beca)
```

6) Repetir generar mismo periodo
```bash
curl -X POST "$BASE_URL/Colegiaturas/generar" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"cicloId\":\"$CICLO_ID\",\"grupoId\":\"$GRUPO_ID\",\"periodo\":\"2026-02\",\"dryRun\":false}"
# Esperado: omitidasPorExistir > 0 (idempotente)
```

7) Aplicar recargos (usar periodo vencido, e.g. 2026-01)
```bash
RECARGO_ID="<id-concepto-recargo>"
# Crear regla recargo si falta
curl -X POST "$BASE_URL/RecargosReglas" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"cicloId\":\"$CICLO_ID\",\"conceptoCobroId\":\"$RECARGO_ID\",\"diasGracia\":0,\"porcentaje\":0.10}"
# Generar colegiaturas 2026-01 (ya vencidas al 20 Jan 2026) si no existen
curl -X POST "$BASE_URL/Colegiaturas/generar" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"cicloId\":\"$CICLO_ID\",\"grupoId\":\"$GRUPO_ID\",\"periodo\":\"2026-01\",\"dryRun\":false}"
# Aplicar recargos
curl -X POST "$BASE_URL/Colegiaturas/aplicar-recargos" -H "$AUTH" -H "Content-Type: application/json" \
  -d "{\"cicloId\":\"$CICLO_ID\",\"periodo\":\"2026-01\",\"dryRun\":false}"
# Esperado: recargo aplicado 1 vez por factura con saldo>0.01; repetición omite (idempotente)
```

8) Emitir recibo interno (sin CFDI) para una factura
```bash
FACTURA_ID="<factura-generada>"  # obtén con GET $BASE_URL/Facturas?desde=2026-02-01&hasta=2026-02-05
curl -X POST "$BASE_URL/Facturas/$FACTURA_ID/emitir-recibo" -H "$AUTH"
# Esperado: TipoDocumento=Recibo, ReciboFolio asignado; segundo llamado es no-op (200)
```
