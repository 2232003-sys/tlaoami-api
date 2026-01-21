# ENTREGA - Colegiaturas mensuales automáticas (MVP primaria)

## Cambios clave
- Facturas en estado BORRADOR por defecto; soporte Periodo (YYYY-MM), ConceptoCobroId, TipoDocumento (Factura/Recibo), ReciboFolio/ReciboEmitidoAtUtc.
- Nuevas entidades: ReglaColegiatura, BecaAlumno (1 activa por alumno+ciclo), ReglaRecargo (porcentaje), FacturaLinea.
- Idempotencia: UNQ (AlumnoId, Periodo, ConceptoCobroId) en facturas no canceladas; recargo se aplica 1 vez por factura.
- Servicios y controladores nuevos: ReglasColegiatura, Becas, RecargosReglas, Colegiaturas (generar, aplicar-recargos), emitir-recibo en Facturas.

## Endpoints
- POST /api/v1/Colegiaturas/generar (borra/emitir opcional)
- POST /api/v1/Colegiaturas/aplicar-recargos
- POST /api/v1/Facturas/{id}/emitir-recibo (idempotente)
- CRUD /api/v1/ReglasColegiatura, /api/v1/Becas, /api/v1/RecargosReglas

## Migraciones
- 20260121004835_AddColegiaturasMensuales (aplicada)

## Comandos ejecutados
- dotnet ef migrations add AddColegiaturasMensuales --project src/Tlaoami.Infrastructure --startup-project src/Tlaoami.API
- dotnet ef database update --project src/Tlaoami.Infrastructure --startup-project src/Tlaoami.API
- dotnet test

## Smoke manual
- Ver pasos en docs/SMOKE_COLEGIATURAS_MENSUAL.md (<=8 pasos: conceptos, regla, beca, generar, recargos, recibo).
