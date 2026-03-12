# Análisis de Crash - Aplicación Pollos - Escaneo de Códigos

## 📋 Resumen Ejecutivo
La aplicación se crasheaba al escanear códigos de barras debido a **múltiples problemas**: falta de solicitud de permisos de cámara en tiempo de ejecución, validaciones insuficientes de códigos y manejo pobre de excepciones.

---

## 🔴 Problemas Identificados

### **PROBLEMA 1: Falta de Permisos de Cámara en Tiempo de Ejecución (CRÍTICO)**
**Causa:** Android 6.0+ (API 23+) requiere solicitar permisos en tiempo de ejecución, no solo declararlos en `AndroidManifest.xml`.

**Síntoma:** Excepción de Java cuando se intenta acceder a la cámara:
```
java.lang.SecurityException: Permission denied: camera access
```

**Ubicación:** `AgregarCajaView.xaml.cs` - `OnAppearing()`

**Solución Implementada:**
```csharp
private async Task RequestCameraPermission()
{
    var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
    if (status != PermissionStatus.Granted)
    {
        status = await Permissions.RequestAsync<Permissions.Camera>();
    }
}
```

---

### **PROBLEMA 2: Validación Insuficiente de Longitud de Subcadenas**
**Causa:** El código intenta extraer subcadenas sin validar si la posición + longitud excede el tamaño del string.

**Ejemplo problemático:**
```csharp
// Si codigo_barras.Length < 33, esto causa ArgumentOutOfRangeException
cajaParaLista.numero_lote = int.Parse(codigo_barras.Substring(23, 10));
```

**Síntoma:** `System.ArgumentOutOfRangeException` en el método `Agregar()`

**Solución Implementada:**
- Método auxiliar `TryParseSubstring()` que valida antes de extraer
- Validación de rango segura con try-catch explícito

```csharp
private bool TryParseSubstring(string source, int startIndex, int length, out string result)
{
    result = null;
    if (source == null || startIndex + length > source.Length)
        return false;
    try
    {
        result = source.Substring(startIndex, length);
        return true;
    }
    catch { return false; }
}
```

---

### **PROBLEMA 3: KeyNotFoundException en Dictionary**
**Causa:** No se valida si la clave GTIN existe en el diccionario de categorías antes de acceder.

**Ubicación:** `CajasViewModel.cs` línea 115
```csharp
cajaParaLista.rango_peso = categorias[codigo_barras.Substring(2, 4)]; // ⚠️ Sin validación
```

**Síntoma:** `KeyNotFoundException` si el GTIN no existe en `categorias`

**Solución Implementada:**
```csharp
if (!categorias.ContainsKey(gtin))
{
    ListaErrores.Add($"ERROR BCR_04: GTIN '{gtin}' no encontrado en categorías.");
    return;
}
```

---

### **PROBLEMA 4: Parse Inseguro de Valores Numéricos**
**Causa:** `int.Parse()` y `decimal.Parse()` lanzan excepciones si el formato no es válido.

**Ubicación:** `CajasViewModel.cs` líneas 112-114, 118-119

**Solución Implementada:**
```csharp
if (!int.TryParse(lote_str, out var numero_lote) ||
    !decimal.TryParse(peso_str, out var peso_valor))
{
    ListaErrores.Add("ERROR BCR_03: No se pudieron parsear los valores numéricos.");
    return;
}
```

---

### **PROBLEMA 5: DisplayAlert sin await en Handler**
**Causa:** En el handler de escaneo, `DisplayAlert()` se llama sin `await`.

**Ubicación:** `AgregarCajaView.xaml.cs` línea 48

**Solución Implementada:**
```csharp
await DisplayAlert("Error", "Código de barras muy corto...", "Aceptar");
```

---

## ✅ Cambios Realizados

### **Archivo: `AgregarCajaView.xaml.cs`**

1. **Agregado método de solicitud de permisos:**
   ```csharp
   protected override async void OnAppearing()
   {
       base.OnAppearing();
       await RequestCameraPermission();  // ← NUEVO
       await Task.Delay(100);
       txtCodigo.Focus();
   }

   private async Task RequestCameraPermission()  // ← NUEVO
   {
       try
       {
           var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
           if (status != PermissionStatus.Granted)
           {
               status = await Permissions.RequestAsync<Permissions.Camera>();
           }
           if (status != PermissionStatus.Granted)
           {
               await DisplayAlert("Permisos", 
                   "Se requiere permiso de cámara para escanear códigos de barras", 
                   "Aceptar");
           }
       }
       catch (Exception ex)
       {
           await DisplayAlert("Error", $"Error solicitando permiso de cámara: {ex.Message}", "Aceptar");
       }
   }
   ```

2. **Mejorada validación en `barcodeReader_BarcodesDetected()`:**
   - Validación de longitud mínima (20 caracteres) y máxima (50)
   - `DisplayAlert` ahora usa `await`
   - Mejor manejo de errores

### **Archivo: `CajasViewModel.cs`**

1. **Agregado método auxiliar seguro:**
   ```csharp
   private bool TryParseSubstring(string source, int startIndex, int length, out string result)
   {
       result = null;
       if (source == null || startIndex + length > source.Length)
           return false;
       try
       {
           result = source.Substring(startIndex, length);
           return true;
       }
       catch { return false; }
   }
   ```

2. **Refactorizado método `Agregar()`:**
   - Validación de subcadenas seguras con `TryParseSubstring()`
   - Validación con `TryParse()` para conversiones numéricas
   - Validación de claves en diccionario con `ContainsKey()`
   - Mensajes de error específicos para cada caso
   - Actualización de `ListaErrores` con `PropertyChanged`

---

## 🧪 Casos Manejados Ahora

| Escenario | Antes | Después |
|-----------|-------|---------|
| Permiso de cámara no otorgado | ❌ Crash | ✅ Alerta y solicitud |
| Código muy corto | ❌ Crash | ✅ Validación y alerta |
| Código con formato inválido | ❌ Crash | ✅ Error agregado a lista |
| GTIN no existe en categorías | ❌ Crash | ✅ Error específico |
| Conversión numérica falla | ❌ Crash | ✅ Manejo controlado |
| Subcadena fuera de rango | ❌ Crash | ✅ Validación previa |

---

## 📝 Recomendaciones Adicionales

1. **Agregar manejo de recurso de cámara:**
   - Considerar desactivar el escaneo cuando la página no es visible
   - Implementar `OnDisappearing()` para liberar recursos

2. **Mejorar validación de códigos:**
   - Considerar expresiones regulares para validar formato
   - Añadir checksum validation para códigos de barras estándar

3. **Logging mejorado:**
   - Registrar errores de escaneo para análisis posterior
   - Usar un sistema de logging en lugar de solo `ListaErrores`

4. **Tests unitarios:**
   - Crear tests para `ValidarCodigoBarras()`
   - Crear tests para `TryParseSubstring()`
   - Crear tests para `Agregar()` con diferentes formatos de código

---

## ✨ Estado Final
✅ **Compilación exitosa**
✅ **Todos los problemas identificados resueltos**
✅ **Código más robusto y mantenible**
