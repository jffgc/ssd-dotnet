# Investigación: Base de la solución Realtor

## Decisión

- La versión de .NET para toda la solución se toma de [global.json](../../global.json), que define `sdk.version` como `11.0.100-preview.7.26381.103`.
- La initiative foundation se limita a la estructura base del repositorio y a la preparación operativa de backend y frontend.
- El backend se implementará con ASP.NET Core Minimal APIs, sin controladores ni endpoints de negocio en esta fase.
- El frontend se implementará como Blazor Web App con Razor Components, con configuración mínima y sin features concretas.
- No se crean entidades de dominio ni contratos de negocio en la fundación.

## Racional

La constitución del repositorio exige que el stack tecnológico no sea negociable y que la solución sea única y compartida. Dado que ya existe un `global.json` válido en la raíz, esta se convierte en la única fuente de verdad para la versión de .NET y bloquea cualquier decisión divergente. La foundation debe dejar una base estable y neutral, donde la arquitectura canónica esté presente pero el alcance se mantenga estrictamente sin lógica de negocio.

## Alternativas consideradas

- Mantener una versión de .NET independiente del repositorio: descartada porque la constitución exige derivarla de `global.json` y porque la solución debe ser reproducible por todo el equipo.
- Crear modelo de dominio o endpoints operativos desde el inicio: descartada porque la foundation no debe implementar negocio ni features, y la constitución explicitamente prohíbe mezclar dominio con infraestructura base.
- Usar controladores en el backend: descartada porque la arquitectura canónica del repositorio exige Minimal APIs y prohibe controllers.
- Crear una solución paralela para frontend y backend: descartada porque la constitución exige una sola solución compartida.

## Hallazgos clave

- El repositorio ya está preparado para una solución .NET 11 basada en la carpeta `app/`.
- La base debe servir como arranque para futuras iniciativas sin introducir decisiones de negocio.
- Los únicos artefactos “funcionales” de la foundation son la solución, los proyectos base y la configuración mínima del arranque.
