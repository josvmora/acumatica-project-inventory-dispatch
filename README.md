# Despacho final — V1 (2026-10-07)

Versión de fuentes para instalar y validar en Acumatica. La carpeta `../Dispacht`
permanece como versión estable. Esta carpeta es una versión alternativa completa,
con los mismos nombres de clases y pantallas: reemplazar los archivos del proyecto
de personalización, no publicar ambas definiciones a la vez.

## Comportamiento

- Details admite PO Normal (`POOrderType.RegularOrder`) y SO `TR`, con el mismo
  proyecto de la cabecera. Se conserva el requisito de estado Completed y la
  exclusión de órdenes ya incluidas en otro despacho de la versión estable.
- Al agregar una orden, sus líneas de inventario stock se copian a la tabla real
  `JEINDispachtMaterial`. Servicios, cargos y artículos no stock se omiten.
- PO: warehouse de `POLine.SiteID`. SO TR: warehouse de
  `SOOrder.DestinationSiteID`, nunca el warehouse de salida Mainwarehouse.
- Location se resuelve en `INLocation` por SiteID y ProjectID. Debe existir una
  sola ubicación activa válida para salida (`SalesValid`). Si falta o hay más de
  una, la operación falla con un mensaje; no elige la primera ni una ubicación
  general. ProjectID y TaskID se leen por cache para incluir extensiones del DAC.
- Se conserva Qty como cantidad ordenada, para compatibilidad de reportes.
  `DispatchQty` es una cantidad independiente, editable, inicialmente cero.
  Warehouse, Location, artículo, UOM y referencias de origen son de solo lectura.
- Disponibilidad: `min(INLocationStatus.QtyAvail, QtyOnHand)`, por artículo,
  subartículo, bodega y ubicación. Se presenta en la UOM de la línea. La validación
  suma las cantidades del despacho en unidad base y comprueba al editar, guardar
  y crear Issues. Es una comprobación de disponibilidad, no una reserva.
- Se toma TaskID de la ubicación cuando existe; en otro caso, de la línea origen.
  CostCodeID proviene de la línea origen. Las validaciones contables y de proyecto
  del graph estándar siguen aplicando.

## Flujo

1. Crear/editar en On Hold, agregar órdenes y escribir Dispatch Qty.
2. Guardar y ejecutar Remove Hold.
3. Despachar y liberar pide confirmación: la operación crea un Inventory Issue
   por warehouse con cantidades positivas y Reason Code `PROJECTISSUE`, y luego
   libera todos los Issues en segundo plano. La liberación mueve inventario y
   afecta el costo real del proyecto. La creación de Issues y sus vínculos se
   guarda en una transacción; la liberación es posterior y no se puede revertir
   automáticamente si ya se liberó alguno.
4. La pestaña Issues muestra número con enlace, warehouse, razón, estado, Hold,
   Released, fecha, total y fecha de creación.
5. Si un Issue requiere asignaciones de lote/serie u otros datos, la liberación
   falla con el error de Acumatica; completar esos datos y ejecutar de nuevo
   Despachar y liberar procesa los Issues que aún no se hayan liberado.
6. Cuando todos se liberan, el despacho pasa automáticamente a Completed. El
   cierre verifica que artículo/subartículo/bodega/ubicación, cantidades en unidad
   base, proyecto y Reason Code todavía coincidan con el despacho.
7. Para cancelar pruebas antes de liberar, ejecutar Undo Dispatch. Solo procede
   si todos los Issues vinculados existen y siguen en Hold y sin liberar; elimina
   los Issues y vínculos en una transacción y devuelve el despacho a Hold.

Crear Issues bloquea edición, eliminación, cancelación y reapertura del despacho.
Una segunda ejecución no crea otro lote. La clave única por compañía/despacho/
warehouse evita duplicados ante dos ejecuciones concurrentes; si una falla,
la transacción de creación completa se revierte. Una falla durante la liberación
puede dejar algunos Issues ya liberados; volver a ejecutar Despachar y liberar
procesa los que falten. No se regeneran automáticamente Issues eliminados
manualmente. No cambiar ni eliminar sus líneas de negocio: el cierre detectará
diferencias. Esta versión no implementa reversos ni despachos parciales sucesivos
contra una misma orden.

## Instalación

1. Ejecutar `sql/JEDispacht.sql` (base estable, repetible) y luego
   `sql/JEDispachtInventory.sql` (las dos nuevas tablas, repetible).
2. Reemplazar los archivos existentes por los de `code/`. Agregar también
   `JEDispachtEntryInventory.cs` (segunda parte del mismo graph) y
   `JEDispachtIssue.cs` y `JEDispachtINReleaseProcessExt.cs` (cierre autom?tico
   desde INReleaseProcess). Son nueve archivos de c?digo. No conservar la definición con PXProjection de
   JEINDispachtMaterial junto con la nueva definición persistente.
3. Reemplazar `screens/JE401003.aspx`; se incluyen los demás archivos de pantalla
   sin cambios para conservar una copia completa. Se mantienen JE401003 y JE401004.
4. Tener configurado el Reason Code `PROJECTISSUE` para Inventory Issues, con sus
   cuentas y subcuentas válidas. El código lo usa, no lo crea ni cambia contabilidad.
5. Validar y publicar en una instancia de pruebas de la misma versión del cliente.
6. Actualizar el esquema del reporte JE401005 / je401003.rpx: Qty sigue siendo
   cantidad ordenada; para cantidad realmente despachada usar DispatchQty. El
   filtro por DispachtNbr sigue siendo necesario. El RPX no está en el repositorio.

Las órdenes agregadas desde esta versión generan materiales al instante. Los
despachos antiguos no reciben una copia automática de datos actuales. Para uno
que deba continuar en este flujo: Put on Hold, revisar las órdenes y ejecutar
Load Missing Materials. Solo agrega filas faltantes y conserva cantidades ya
editadas. No reconstruye un histórico: copia los datos actuales del origen.
No ejecutar sobre despachos antiguos que se deban conservar solo como históricos.

## Validación pendiente en la instancia

No hay DLL de Acumatica ni proyecto de compilación en este repositorio. Las
comprobaciones locales de sintaxis y estructura no sustituyen compilar/publicar
contra la versión instalada. Validar especialmente los DAC de inventario,
extensiones ProjectID/TaskID de INLocation, permisos y configuración contable.

Repetir las comprobaciones locales con `python verification/run_checks.py`
(requiere el SDK .NET 9.0.300 instalado en este equipo). El chequeo SHA256 usa
`stable-hashes.json` para comprobar que las fuentes estables no fueron modificadas.

- PO con dos bodegas; TR cuya bodega origen sea distinta del destino: confirmar
  que Materials y el Issue usan las bodegas indicadas arriba.
- Ubicación inexistente, inactiva, no válida para salida y dos ubicaciones del
  proyecto: deben bloquear; ubicación de otro proyecto nunca debe elegirse.
- Guardar/reabrir un despacho nuevo: materiales, claves y cantidades persisten.
  Quitar una orden en Hold elimina solo sus materiales.
- Mismo artículo y ubicación en dos órdenes: cantidades individuales válidas cuya
  suma exceda disponible deben fallar. Repetir con UOM distintas y subartículos.
- Cero, negativo, cantidad exacta disponible y exceso; reducir inventario entre
  edición y Despachar para comprobar la nueva validación.
- Error en el segundo warehouse: no debe quedar ningún Issue ni vínculo parcial.
  Reintento y dos sesiones creando el mismo despacho: no deben duplicar Issues.
- Crear Issues y comprobar Hold, PROJECTISSUE, proyecto, tarea, costo, warehouse,
  ubicación y cantidades. Los Issues deben permanecer sin liberar.
- Liberación manual, regreso al despacho y Complete. Sin liberar, con Issue
  eliminado o con cantidad/ubicación modificada, Complete debe rechazarlo.
- Probar artículos con lote/serie: completar asignaciones en el Issue; no hay
  selección automática de lotes/series en esta versión.

La disponibilidad puede cambiar entre creación y liberación manual. El Issue
estándar es responsable de validar el movimiento al liberarlo. Esta versión usa
saldo por ubicación; no añade selección de capas de inventario específicas de
proyecto. Si se usa Project-Specific Inventory con capas separadas, validar ese
modo en la instancia antes de usarlo.

Referencia oficial para las distintas medidas de disponibilidad:
[Inventory Allocation Details](https://github.com/Acumatica/Acumatica-AI-Resources/blob/2026R1/Documentation/UserGuide/IN_40_20_00.md).
El campo mostrado aquí es la disponibilidad conservadora descrita arriba, no el
campo estándar denominado Available for Issue.

## Actualizaci?n de flujo (2026-10-07)

Para una instalaci?n que ya ten?a la versi?n anterior, reemplazar
`JEDispacht.cs`, `JEDispachtEntry.cs` y `JEDispachtEntryInventory.cs`; agregar
`JEDispachtINReleaseProcessExt.cs` y publicar. La pantalla conserva Issues al final.
El estado D cabe en la columna Status existente: no requiere cambios de esquema.
La extensi?n debe compilarse y probarse contra el INReleaseProcess de la versi?n
instalada; aqu? solo se verific? sintaxis, no se ejecut? una liberaci?n real.

Los despachos creados con el flujo anterior, con Issues pero todav?a en Open,
no se migran autom?ticamente. El nuevo ciclo se aplica al pulsar Despachar
en documentos sin Issues. Revisar los documentos de prueba anteriores por
separado antes de adoptar esta versi?n.
