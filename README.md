# Windows Forms DigitalPersona One Touch Demo

## Estructura del proyecto
- `Models/`  
  - `AppData.cs`: Datos compartidos y templates de huellas
  - `Cliente.cs`: Modelo de cliente
- `Forms/`  
  - `EnrollmentForm.cs`: Formulario de enrollamiento (agrega el control EnrollmentControl desde la toolbox)
  - `VerificationForm.cs`: Formulario de verificación (agrega el control VerificationControl desde la toolbox)
- `Services/`  
  - (puedes agregar lógica de base de datos aquí)

## Pasos para usar el SDK DigitalPersona
1. **Agrega manualmente las referencias a los DLLs del SDK** en tu proyecto:
   - `DPFPGuiNET.dll`
   - `DPFPShrNET.dll`
   - `DPFPVerNET.dll`
   - `DPFPDevNET.dll`
   - `DPFPEngNET.dll`
2. Abre cada formulario (`EnrollmentForm`, `VerificationForm`) en el diseñador de Visual Studio.
3. Arrastra el control `EnrollmentControl` o `VerificationControl` desde la toolbox al formulario si quieres UI visual.
4. Conecta los eventos como en los ejemplos de código.
5. Compila y ejecuta en Windows (no compatible con MAUI, WPF puro ni multiplataforma).

## Notas
- El SDK maneja automáticamente la detección del lector y la captura de huellas.
- Puedes extender la lógica de base de datos en la carpeta `Services`.
- Si tienes dudas, consulta la documentación oficial del SDK DigitalPersona One Touch.
