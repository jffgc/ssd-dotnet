# Contratos y contratos de datos

## Contrato de persistencia: Property

El modelo persistente principal para la feature es `Property`.

```json
{
  "id": "guid",
  "title": "string",
  "description": "string",
  "address": "string",
  "price": 0.0,
  "status": "Available",
  "bedroomCount": 0,
  "bathroomCount": 0,
  "areaSquareMeters": 0.0,
  "imageUrl": "/images/properties/filename.jpg",
  "createdAt": "2026-09-22T00:00:00Z",
  "updatedAt": "2026-09-22T00:10:00Z"
}
```

## Contrato de estado

```json
{
  "value": "Available",
  "description": "La propiedad está disponible para renta"
}
```

## Contrato de seed manifest

El manifiesto central debe describir la relación entre los archivos de origen y la ruta pública final. Su esquema que se implementará durante la feature es el siguiente:

```json
{
  "sourceRoot": "support/seed-data",
  "dataFiles": [
    {
      "name": "properties.json",
      "target": "properties"
    }
  ],
  "images": [
    {
      "source": "images/1.png",
      "target": "wwwroot/images/properties/1.png",
      "publicUrl": "/images/properties/1.png"
    }
  ]
}
```

## Reglas contractuales

- `status` deberá persistirse como `string` en base de datos.
- `imageUrl` debe apuntar a la ruta pública final servida por la API.
- El origen de los datos del seed debe ser centralizado y resolverse vía manifest y no por rutas dispersas en código.
