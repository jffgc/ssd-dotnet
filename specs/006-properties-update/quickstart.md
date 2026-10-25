# Guía rápida de validación: actualización de propiedades

## Prerrequisitos

- .NET 11 SDK instalado.
- PostgreSQL configurado y accesible para la app backend.
- Proyecto backend en `app/backend/src/RealtorApi` y solución `app/Realtor.sln` presentes.
- La feature 006 debe ejecutarse sobre el nivel actual de dominio y persistencia ya validado por las specs 001-005.

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

**Resultado esperado**: pruebas del slice de actualización y validación del archivo sin regresiones del dominio previo.

### 3. Arranque de la aplicación

```bash
dotnet run --project app/backend/src/RealtorApi/RealtorApi.csproj
```

**Resultado esperado**:
- La API arranca con el host configurado.
- El endpoint `PUT /api/properties/{id}` queda registrado por el auto-registro de slices.
- La carpeta `wwwroot/assets/properties` queda disponible para almacenamiento de imágenes.

### 4. Validación manual del flujo principal

#### Caso A: actualización sin imagen

```bash
curl -X PUT "http://localhost:5000/api/properties/{id}" \
  -F "Title=Casa actualizada" \
  -F "Description=Descripcion actualizada" \
  -F "Address=Calle Real 456" \
  -F "Price=300000" \
  -F "Status=Available" \
  -F "BedroomCount=4" \
  -F "BathroomCount=2" \
  -F "AreaSquareMeters=150"
```

**Resultado esperado**: `200 OK` o equivalente y el valor de `imageUrl` conserva la imagen previa.

#### Caso B: actualización con imagen válida

```bash
curl -X PUT "http://localhost:5000/api/properties/{id}" \
  -F "Title=Casa con nueva imagen" \
  -F "Description=Propiedad actualizada con imagen nueva" \
  -F "Address=Avenida Siempre Viva 742" \
  -F "Price=340000" \
  -F "Status=Available" \
  -F "BedroomCount=4" \
  -F "BathroomCount=3" \
  -F "AreaSquareMeters=180" \
  -F "Image=@./sample-property.jpg;type=image/jpeg"
```

**Resultado esperado**: actualización exitosa y un `imageUrl` público relativo o absoluto consumible por clientes.

#### Caso C: id inexistente o archivo inválido

- `PUT /api/properties/{id-inexistente}`: respuesta `404` y sin cambios.
- Archivo vacío, formato no soportado o tamaño > 5MB: respuesta `415` o `413` según la regla de negocio y sin cambios parciales.

## Resultado de éxito

La feature queda validada cuando la API actualiza correctamente propiedades existentes, conserva `imageUrl` cuando no se envía archivo, reemplaza la imagen con un archivo válido y responde con `ProblemDetails` estandarizados en errores funcionales.
