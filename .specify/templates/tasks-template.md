---

description: "Plantilla de tareas para implementar funcionalidades"
---

# Tareas: [NOMBRE DE LA FUNCIONALIDAD]

**Entrada**: Documentos de diseño de `/specs/[###-nombre-de-funcionalidad]/`

**Prerrequisitos**: plan.md (obligatorio), spec.md (obligatorio para historias de usuario), research.md, data-model.md, contracts/

**Pruebas**: Los ejemplos siguientes incluyen tareas de pruebas. Las pruebas son OPCIONALES; inclúyelas solo si la especificación de la funcionalidad las solicita explícitamente.

**Organización**: Las tareas se agrupan por historia de usuario para permitir su implementación y prueba independientes.

## Formato: `[ID] [P?] [Historia] Descripción`

- **[P]**: Puede ejecutarse en paralelo (archivos distintos, sin dependencias)
- **[Historia]**: Historia de usuario a la que pertenece la tarea (p. ej., US1, US2, US3)
- Incluye rutas de archivo exactas en las descripciones

## Convenciones de rutas

- **Proyecto único**: `src/`, `tests/` en la raíz del repositorio
- **Aplicación web**: `backend/src/`, `frontend/src/`
- **Aplicación móvil**: `api/src/`, `ios/src/` o `android/src/`
- Las rutas siguientes suponen un proyecto único; ajústalas según la estructura de plan.md

<!--
  ============================================================================
  IMPORTANTE: Las tareas siguientes son solo EJEMPLOS para ilustrar el formato.

  El comando /speckit.tasks DEBE reemplazarlas por tareas reales basadas en:
  - Historias de usuario de spec.md (con prioridades P1, P2, P3...)
  - Requisitos de la funcionalidad en plan.md
  - Entidades de data-model.md
  - Endpoints de contracts/

  Las tareas DEBEN organizarse por historia de usuario para que cada historia pueda:
  - Implementarse de forma independiente
  - Probarse de forma independiente
  - Entregarse como un incremento del MVP

  NO conserves estas tareas de ejemplo en el archivo tasks.md generado.
  ============================================================================
-->

## Fase 1: Preparación (infraestructura compartida)

**Propósito**: Inicialización del proyecto y estructura básica

- [ ] T001 Crear la estructura del proyecto según el plan de implementación
- [ ] T002 Inicializar el proyecto de [lenguaje] con dependencias de [framework]
- [ ] T003 [P] Configurar herramientas de análisis estático y formato

---

## Fase 2: Fundamentos (prerrequisitos bloqueantes)

**Propósito**: Infraestructura base que DEBE completarse antes de implementar CUALQUIER historia de usuario

**⚠️ CRÍTICO**: No se puede empezar ninguna historia de usuario hasta completar esta fase

Ejemplos de tareas fundacionales (ajústalos al proyecto):

- [ ] T004 Configurar el esquema de base de datos y el sistema de migraciones
- [ ] T005 [P] Implementar el sistema de autenticación y autorización
- [ ] T006 [P] Configurar el enrutamiento de la API y la estructura de middleware
- [ ] T007 Crear modelos o entidades base de los que dependan todas las historias
- [ ] T008 Configurar la gestión de errores y la infraestructura de logging
- [ ] T009 Configurar la administración de la configuración por entorno

**Punto de control**: Fundamentos listos; ya puede comenzar en paralelo la implementación de las historias de usuario

---

## Fase 3: Historia de usuario 1 - [Título] (Prioridad: P1) 🎯 MVP

**Objetivo**: [Descripción breve de lo que entrega esta historia]

**Prueba independiente**: [Cómo verificar que esta historia funciona por sí sola]

### Pruebas para la historia de usuario 1 (OPCIONAL, solo si se solicitaron pruebas) ⚠️

> **NOTA: Escribe estas pruebas PRIMERO y comprueba que FALLEN antes de implementar.**

- [ ] T010 [P] [US1] Prueba de contrato para [endpoint] en tests/contract/test_[name].py
- [ ] T011 [P] [US1] Prueba de integración para [recorrido de usuario] en tests/integration/test_[name].py

### Implementación de la historia de usuario 1

- [ ] T012 [P] [US1] Crear el modelo [Entidad1] en src/models/[entity1].py
- [ ] T013 [P] [US1] Crear el modelo [Entidad2] en src/models/[entity2].py
- [ ] T014 [US1] Implementar [Servicio] en src/services/[service].py (depende de T012 y T013)
- [ ] T015 [US1] Implementar [endpoint/funcionalidad] en src/[ubicación]/[archivo].py
- [ ] T016 [US1] Añadir validación y gestión de errores
- [ ] T017 [US1] Añadir logging para las operaciones de la historia de usuario 1

**Punto de control**: En este punto, la historia de usuario 1 debe ser completamente funcional y probarse de forma independiente

---

## Fase 4: Historia de usuario 2 - [Título] (Prioridad: P2)

**Objetivo**: [Descripción breve de lo que entrega esta historia]

**Prueba independiente**: [Cómo verificar que esta historia funciona por sí sola]

### Pruebas para la historia de usuario 2 (OPCIONAL, solo si se solicitaron pruebas) ⚠️

- [ ] T018 [P] [US2] Prueba de contrato para [endpoint] en tests/contract/test_[name].py
- [ ] T019 [P] [US2] Prueba de integración para [recorrido de usuario] en tests/integration/test_[name].py

### Implementación de la historia de usuario 2

- [ ] T020 [P] [US2] Crear el modelo [Entidad] en src/models/[entity].py
- [ ] T021 [US2] Implementar [Servicio] en src/services/[service].py
- [ ] T022 [US2] Implementar [endpoint/funcionalidad] en src/[ubicación]/[archivo].py
- [ ] T023 [US2] Integrar con los componentes de la historia de usuario 1 (si hace falta)

**Punto de control**: En este punto, las historias de usuario 1 y 2 deben funcionar de forma independiente

---

## Fase 5: Historia de usuario 3 - [Título] (Prioridad: P3)

**Objetivo**: [Descripción breve de lo que entrega esta historia]

**Prueba independiente**: [Cómo verificar que esta historia funciona por sí sola]

### Pruebas para la historia de usuario 3 (OPCIONAL, solo si se solicitaron pruebas) ⚠️

- [ ] T024 [P] [US3] Prueba de contrato para [endpoint] en tests/contract/test_[name].py
- [ ] T025 [P] [US3] Prueba de integración para [recorrido de usuario] en tests/integration/test_[name].py

### Implementación de la historia de usuario 3

- [ ] T026 [P] [US3] Crear el modelo [Entidad] en src/models/[entity].py
- [ ] T027 [US3] Implementar [Servicio] en src/services/[service].py
- [ ] T028 [US3] Implementar [endpoint/funcionalidad] en src/[ubicación]/[archivo].py

**Punto de control**: Todas las historias de usuario deben ser funcionales de forma independiente

---

[Añade las fases de historias de usuario que hagan falta siguiendo este mismo patrón]

---

## Fase N: Pulido y aspectos transversales

**Propósito**: Mejoras que afectan a varias historias de usuario

- [ ] TXXX [P] Actualizar la documentación en docs/
- [ ] TXXX Limpiar y refactorizar el código
- [ ] TXXX Optimizar el rendimiento en todas las historias
- [ ] TXXX [P] Añadir pruebas unitarias adicionales (si se solicitaron) en tests/unit/
- [ ] TXXX Reforzar la seguridad
- [ ] TXXX Ejecutar la validación de quickstart.md
- [ ] TXXX Regenerar y validar contrato OpenAPI v1: ejecutar support/scripts/generate-openapi-v1.ps1 y versionar app/backend/src/RealtorApi/wwwroot/openapi/v1.json (obligatorio si la spec crea o modifica endpoints)

Nota: Esta tarea es obligatoria en toda spec que toque el backend. El cierre de la spec exige que `dotnet test` pase en verde; incluye `OpenApiDriftTests` como red de seguridad anti-drift.

---

## Dependencias y orden de ejecución

### Dependencias entre fases

- **Preparación (fase 1)**: Sin dependencias; puede empezar de inmediato
- **Fundamentos (fase 2)**: Depende de completar la preparación; BLOQUEA todas las historias de usuario
- **Historias de usuario (fase 3+)**: Todas dependen de completar la fase de fundamentos
  - Después pueden avanzar en paralelo si hay capacidad
  - O secuencialmente por prioridad (P1 → P2 → P3)
- **Pulido (fase final)**: Depende de completar todas las historias de usuario previstas

### Dependencias entre historias de usuario

- **Historia de usuario 1 (P1)**: Puede empezar después de Fundamentos (fase 2); no depende de otras historias
- **Historia de usuario 2 (P2)**: Puede empezar después de Fundamentos (fase 2); puede integrarse con US1, pero debe probarse de forma independiente
- **Historia de usuario 3 (P3)**: Puede empezar después de Fundamentos (fase 2); puede integrarse con US1 o US2, pero debe probarse de forma independiente

### Dentro de cada historia de usuario

- Las pruebas (si se incluyen) DEBEN escribirse y FALLAR antes de implementar
- Modelos antes que servicios
- Servicios antes que endpoints
- Implementación principal antes que integración
- Completar una historia antes de pasar a la siguiente prioridad

### Oportunidades de paralelismo

- Todas las tareas de preparación marcadas con [P] pueden ejecutarse en paralelo
- Todas las tareas fundacionales marcadas con [P] pueden ejecutarse en paralelo dentro de la fase 2
- Al terminar Fundamentos, todas las historias pueden comenzar en paralelo si hay capacidad
- Todas las pruebas de una historia marcadas con [P] pueden ejecutarse en paralelo
- Los modelos de una historia marcados con [P] pueden crearse en paralelo
- Distintas personas pueden trabajar en paralelo en historias diferentes

---

## Ejemplo de paralelismo: historia de usuario 1

```bash
# Ejecuta en conjunto todas las pruebas de la historia de usuario 1 (si se solicitaron):
Tarea: "Prueba de contrato para [endpoint] en tests/contract/test_[name].py"
Tarea: "Prueba de integración para [recorrido de usuario] en tests/integration/test_[name].py"

# Crea en conjunto todos los modelos de la historia de usuario 1:
Tarea: "Crear el modelo [Entidad1] en src/models/[entity1].py"
Tarea: "Crear el modelo [Entidad2] en src/models/[entity2].py"
```

---

## Estrategia de implementación

### Primero el MVP (solo historia de usuario 1)

1. Completar la fase 1: Preparación
2. Completar la fase 2: Fundamentos (CRÍTICO; bloquea todas las historias)
3. Completar la fase 3: Historia de usuario 1
4. **DETENERSE Y VALIDAR**: Probar la historia de usuario 1 de forma independiente
5. Desplegar o hacer una demostración si está listo

### Entrega incremental

1. Completar Preparación y Fundamentos → fundamentos listos
2. Añadir Historia de usuario 1 → probar de forma independiente → desplegar o demostrar (¡MVP!)
3. Añadir Historia de usuario 2 → probar de forma independiente → desplegar o demostrar
4. Añadir Historia de usuario 3 → probar de forma independiente → desplegar o demostrar
5. Cada historia aporta valor sin romper las anteriores

### Estrategia para un equipo en paralelo

Con varias personas desarrolladoras:

1. El equipo completa conjuntamente Preparación y Fundamentos
2. Al terminar Fundamentos:
  - Persona A: Historia de usuario 1
  - Persona B: Historia de usuario 2
  - Persona C: Historia de usuario 3
3. Las historias se completan e integran de forma independiente

---

## Notas

- Las tareas con [P] afectan archivos distintos y no tienen dependencias
- La etiqueta [Historia] vincula la tarea con una historia de usuario para su trazabilidad
- Cada historia de usuario debe poder completarse y probarse de forma independiente
- Comprueba que las pruebas fallen antes de implementar
- Crea un commit después de cada tarea o grupo lógico
- Detente en cada punto de control para validar la historia de forma independiente
- Evita tareas imprecisas, conflictos sobre el mismo archivo y dependencias entre historias que impidan su independencia
