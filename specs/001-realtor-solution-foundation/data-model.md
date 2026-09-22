# Modelo de datos de la foundation

## Visión general

La iniciativa foundation no define entidades de negocio ni persistencia operativa. El propósito de este documento es dejar explícito que el modelo de dominio aún no existe y que la base del proyecto debe mantenerse neutra hasta que una iniciativa posterior apruebe requirements de negocio.

## Entidades y estado actual

### 1. Solución principal
- Nombre: `Realtor`
- Propósito: contener los proyectos backend y frontend de la aplicación.
- Estado: activa en la fase de foundation.

### 2. Proyecto backend
- Nombre: `RealtorApi`
- Propósito: servicio web base con ASP.NET Core Minimal APIs.
- Estado: inicial, sin endpoints de negocio ni modelos de dominio.

### 3. Proyecto frontend
- Nombre: `RealtorWeb`
- Propósito: aplicación de presentación base con Blazor Web App.
- Estado: inicial, sin componentes funcionales ni lógica de negocio.

### 4. Proyectos de prueba
- Nombre: `RealtorApiTests` y `RealtorWeb`
- Propósito: garantizar compilación y estabilidad a partir de la estructura base.
- Estado: configurados sin pruebas de dominio.

## Reglas de validación

- No se crean entidades de dominio en esta iniciativa.
- No se agregan modelos de persistencia ni migraciones.
- No se introducen requests, responses o DTOs de negocio.
- La estructura debe conservarse como infraestructura base únicamente.

## Consecuencias para el futuro

Cuando haya una spec aprobada con negocio, se definirá el modelo de dominio correspondiente en la iniciativa apropiada. Esta foundation sirve únicamente como arranque técnico y organizativo de la solución.
