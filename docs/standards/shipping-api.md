# Shipping Vendor API

## Overview

- **Vendor:** Acme Shipping Co.
- **Base URL:** https://api.acmeshipping.com/v1
- **Protocol:** REST / JSON

---

## Authentication

- **Method:** API Key
- **Header:** `X-Api-Key: {key}`
- **Credentials stored in:** environment variable `SHIPPING_API_KEY`

---

## Estimate Shipping Cost

### Request

`POST /estimates`

```json
{
  "origin": {
    "postalCode": "string",
    "country": "string"
  },
  "destination": {
    "postalCode": "string",
    "country": "string"
  },
  "package": {
    "weightLbs": 0.0
  }
}
```

### Response (200 OK)

```json
{
  "estimatedCost": 0.00,
  "currency": "USD"
}
```

### Error Responses

| Status | Meaning |
|--------|---------|
| 400 | Bad request — missing or invalid fields |
| 401 | Authentication failure |
| 500 | Vendor server error |

Error body:
```json
{
  "error": "string",
  "message": "string"
}
```
