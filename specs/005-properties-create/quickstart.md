# Guía rápida de validación: creación de propiedades

## Prerrequisitos

- .NET 11 SDK instalado.
- PostgreSQL configurado y accesible para la app backend.
- Proyecto backend en `app/backend/src/RealtorApi` y solución `app/Realtor.sln` presentes.
- El repositorio ya incorpora el dominio de propiedades y la infraestructura de persistencia previa.

## Validación funcional

### 1. Construcción del backend

```bash
dotnet restore app/Realtor.sln
dotnet build app/Realtor.sln
```

**Resultado esperado**: compilación correcta del backend y de la solución.

### 2. Ejecución de pruebas relevantes

```bash
dotnet test app/backend/tests/RealtorApiTests/RealtorApiTests.csproj
```

**Resultado esperado**: pruebas de endpoint y validación del upload, sin regresiones del dominio previo.

### 3. Arranque de la aplicación

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj
```

**Resultado esperado**:
- La API arranca con el host configurado.
- El endpoint `POST /api/properties` queda registrado por el auto-registro de slices.
- La carpeta `wwwroot/assets/properties` queda disponible para almacenamiento de imágenes.

### 4. Validación manual del flujo principal

#### Caso A: creación sin imagen

```bash
curl -X POST "http://localhost:5000/api/properties" \
  -F "Title=Casa de prueba" \
  -F "Description=Propiedad de ejemplo" \
  -F "Address=Calle Falsa 123" \
  -F "Price=250000" \
  -F "Status=Available" \
  -F "BedroomCount=3" \
  -F "BathroomCount=2" \
  -F "AreaSquareMeters=120"
```

**Resultado esperado**: `201 Created` y `imageUrl` en `null`.

#### Caso B: creación con imagen válida

```bash
curl -X POST "http://localhost:5000/api/properties" \
  -F "Title=Casa con imagen" \
  -F "Description=Propiedad con imagen" \
  -F "Address=Avenida Siempre Viva 742" \
  -F "Price=340000" \
  -F "Status=Available" \
  -F "BedroomCount=4" \
  -F "BathroomCount=3" \
  -F "AreaSquareMeters=180" \
  -F "Image=@./sample-property.jpg;type=image/jpeg"
```

**Resultado esperado**: `201 Created` y un `imageUrl` público relativo o absoluto consumible por clientes.

#### Caso C: archivo no permitido o demasiado grande

- Formato no soportado: `png`, `jpg`, `jpeg` únicamente.
- Tamaño > 5MB: rechazo con `ProblemDetails` y sin persistencia parcial.

## Resultado de éxito

La feature queda validada cuando la app responde correctamente a los escenarios principales, la imagen se guarda en la ruta pública configurada y la propiedad se persiste con el valor `ImageUrl` esperado.
