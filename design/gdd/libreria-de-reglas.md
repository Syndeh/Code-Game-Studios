# Librería de reglas (contrato)

> **Status**: In Design
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-19
> **Last Verified**: 2026-11-19
> **Implements Pillar**: 1 Planificar y cosechar (y la base del duelo justo para el online)
> **Creative Director Review (CD-GDD-ALIGN)**: REVISED 2026-11-19 — regla 8 marca el golpe combinado (pilar 2); AC-26 cubre el desacuerdo entre cliente y servidor

## Summary

La Librería de reglas es el núcleo, sin gráficos ni Unity, que decide qué pasa en cada
partida: recibe un estado y un comando y devuelve el estado siguiente con sus eventos, o un
rechazo con su motivo, siempre con el mismo resultado en cualquier máquina. Existe para que
"lo que ves es lo que pasa" se cumpla: la vista previa, la resolución, las repeticiones y
el servidor online usan exactamente las mismas reglas.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `None`

## Overview

La **Librería de reglas** es el núcleo que decide qué pasa en una partida de rolchess.
Recibe el estado de una partida y un comando de un jugador (mover una pieza, deshacer,
asignar un ataque, elegir un recluta, resolver, `TurnTimeout`) y devuelve el estado
siguiente, o un código de rechazo que dice por qué el comando no es válido. No dibuja, no
mide el tiempo, no guarda archivos y no conoce a Unity: el mismo código corre en el cliente
local, en el servidor online, en los tests y en el reproductor de repeticiones.

Su promesa central es el **determinismo**: el mismo estado inicial y la misma secuencia de
comandos dan siempre el mismo resultado, en cualquier máquina. Para cumplirla usa solo
números enteros, resuelve todo en un orden fijo por ID de pieza y no toma azar de ningún
lado: el único azar del juego, el resultado del Modificador, entra como un dato registrado.

Los demás sistemas del núcleo (Máquina de turno, Tablero, Castillo, Movimiento, Ataques,
Cobertura, Resolución, Reclutamiento, Modificador y Registro de partida) son reglas escritas
dentro de este contrato. Este GDD fija el contrato, no cada regla.

Sin la librería, la vista previa podría mostrar un resultado y la resolución dar otro, cada
plataforma tendría su propia copia de las reglas y un servidor no podría validar una partida
online ni reproducirla.

*Alternativas descartadas:* reglas repartidas en los componentes de Unity (no corren en el
servidor ni en los tests sin el motor); reglas duplicadas en C# y en JavaScript para Cloud
Functions (dos implementaciones terminan divergiendo; riesgo alto marcado en el concepto).

## Player Fantasy

**"Lo que ves es lo que pasa."** La vista previa marca MUERE sobre el Rey rival; apretás
Resolver y cae exactamente eso, ni un punto más ni uno menos. El jugador nunca ve a la
librería, pero siente su promesa en cada resolución: el tablero cumple lo que mostró. Es la
base del pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus
frutos"*, y de su prueba de diseño, *"El daño es fijo y se ve antes de resolver"*.

De esa promesa salen otras dos sensaciones:

- **"El tablero no hace trampa."** Después de perder una partida online pareja, la
  repetición reproduce cada movimiento y cada tirada tal como pasaron, igual en las dos
  pantallas: se perdió por una jugada, no por el sistema (anti-pilar *"NOT información
  oculta al resolver"*; pilar 3b, probabilidades consultables).
- **"Pensé mejor que el otro y el tablero lo muestra"** (fantasía central del concepto).
  Durante el turno el jugador prueba órdenes de movimiento con el deshacer y cada vez el
  tablero responde igual y al instante; si un movimiento no vale, el juego dice por qué.

**Qué le exige al resto del diseño:**

- La vista previa y la resolución usan la misma función de la librería; una diferencia
  entre las dos es un error de severidad máxima (S1).
- Las repeticiones son exactas y la tirada del Modificador queda registrada.
- Deshacer y vista previa no tienen demora perceptible.
- Todo rechazo de un comando tiene un motivo que la interfaz puede mostrar.

**Lo que el jugador nunca debería sentir:** "el juego me mostró una cosa e hizo otra", "en
la pantalla del otro pasó distinto", "perdí por algo que no se veía".

## Detailed Design

### Core Rules

1. **Una sola puerta.** Una partida solo cambia con la operación **Aplicar(estado,
   comando)**. Si el comando es válido devuelve **Aceptado** con el estado nuevo y una lista
   de eventos; si no, devuelve **Rechazado** con un código de motivo y el estado queda
   igual. Aplicar es una función pura: no modifica el estado que recibe y no toca reloj,
   archivos, red, registros de consola ni Unity.
2. **Creación.** Una partida nace con **CrearPartida(config)**. La config incluye: la
   versión de reglas; la tabla del Catálogo (contenido y huella, un resumen que identifica
   esa tabla exacta); la posición de los castillos, fija o sorteada; y el modo de timer,
   solo como dato informativo (la librería no mide tiempo). Con una config inválida no se
   crea partida y se devuelve `ConfigInvalida`. El sorteo de castillos, si lo hay, entra
   como una tirada (regla 6).
3. **Comandos: una lista cerrada.** Mover(pieza, destino) · Deshacer · TerminarMovimiento ·
   AsignarAtaque(pieza, objetivo) · QuitarAtaque(pieza) · Cubrir(escudero, aliado) ·
   Reclutar(tipo) · Resolver · TurnTimeout · Tirada(número 0–99). Cada comando lleva su
   emisor: jugador 0, jugador 1 o la **autoridad** (el cliente local o el servidor); Tirada
   y TurnTimeout solo se aceptan de la autoridad (ver *Edge Cases*). Las reglas de cada uno las define su GDD (#3 a #11); agregar un
   comando es un cambio de contrato y sube la versión de reglas.
4. **Validación antes de aplicar.** Todo comando se valida entero antes de cambiar nada:
   no hay comandos aplicados a medias. Los rechazos son **códigos**, nunca texto; la
   presentación los traduce con Localización (#14). Ejemplos: `ComandoDesconocido`, `NoEsTuTurno`,
   `FaseIncorrecta`, `PiezaInexistente`, `FueraDeAlcance`, `DestinoOcupado`,
   `SinMovimientos`, `NadaQueDeshacer`, `TipoNoReclutable`, `TiradaNoEsperada`,
   `PartidaTerminada`, `VersionNoSoportada`. Cada GDD del núcleo agrega los suyos.
5. **Determinismo.** Las reglas usan **solo enteros**; la división entera se aplica solo a
   valores no negativos y redondea hacia abajo. Cuando hay que recorrer varios elementos,
   el orden es fijo: piezas por ID ascendente, casillas por fila y después por columna.
   Nunca se recorre una colección sin ordenarla antes. Prohibidos el reloj, el generador de
   azar, la cultura del sistema y cualquier dato de la plataforma.
6. **El azar se inyecta.** La librería nunca genera azar. Cuando una regla lo necesita (la
   tirada del Modificador, el sorteo de castillos), el estado queda **esperando tirada**,
   con sus probabilidades a la vista, y solo acepta Tirada(n), con n entre 0 y 99, enviado
   por quien manda (el cliente en partida local, el servidor online). La librería convierte
   n en un resultado con la fórmula de *Formulas*. El número queda en los eventos y en el
   registro.
7. **IDs estables.** Cada pieza recibe un ID entero al aparecer (1, 2, 3…) que nunca se
   reusa, aunque la pieza muera. Cada tramo de muro se identifica por su casilla. Los
   jugadores son 0 (azul) y 1 (rojo).
8. **Eventos.** Todo comando aceptado devuelve una lista ordenada de eventos, siempre en el
   mismo orden. Cada evento trae lo necesario para animar, sonar o medir sin leer el
   estado: IDs, casillas, daño antes y después del escudo, muerte, muro roto, cara y tabla
   de probabilidades. Un rechazo no produce eventos. Cada evento de daño indica además si
   fue **combinado** (dos o más atacantes golpearon a ese mismo objetivo en la
   resolución); con ese dato Audio (#22) y Render (#18) acentúan el golpe combinado
   (pilar 2: *"un golpe combinado se siente distinto de varios golpes sueltos"*).
9. **Vista previa = la misma operación.** La vista previa aplica Resolver sobre una
   **copia** del estado y devuelve sus eventos. No existe un segundo cálculo del daño.
10. **Consultas puras.** MovimientosLegales(pieza), ObjetivosValidos(pieza),
    `EffectiveStats(estado, pieza)` (catálogo + bonos del turno + posición, como el arquero
    sobre el muro), ProbabilidadesSiguienteTirada(jugador) y `EvaluateMatchEnd(estado)`
    leen el estado sin cambiarlo.
11. **Todo a la vista.** El estado contiene toda la partida: no hay información oculta (lo
    único que no existe todavía es la próxima tirada). Se puede guardar y leer completo, y
    tiene una huella para comparar el del cliente con el del servidor.
12. **Versión.** El estado lleva la versión de reglas y la huella del Catálogo. Una
    librería que no soporta esa versión rechaza la partida con `VersionNoSoportada`. Cómo
    se conservan las versiones viejas para las repeticiones lo decide el ADR.
13. **Fuera de la librería:** el tiempo (solo recibe `TurnTimeout`), los textos, el
    dibujo, el sonido, los archivos, la red, las métricas y la IA (que usa la misma API que
    un jugador). **Sin comando Rendirse** por ahora (pregunta abierta para la Fase 2).

### States and Transitions

| Desde | Hacia | Cuándo |
|---|---|---|
| (ninguno) | En curso | CrearPartida con config válida: piezas y muros colocados |
| (ninguno) | (sin partida) | Config inválida: `ConfigInvalida` |
| En curso | En curso · esperando tirada | Una regla necesita azar (el momento lo define la Máquina de turno, #3) |
| En curso · esperando tirada | En curso | Tirada(n) válida; cualquier otro comando se rechaza con `TiradaNoEsperada` o `FaseIncorrecta` |
| En curso | Terminada (ganador, motivo) | Después de resolver, `EvaluateMatchEnd` encuentra un ganador. Motivo inicial: Rey muerto; la Máquina de turno puede sumar el cierre por tiempo |
| Terminada | Terminada | Todo comando se rechaza con `PartidaTerminada`; es un estado final |

Las fases del turno (tirada, reclutar, mover, atacar, resolver) son de la Máquina de turno
(#3), que las define dentro de este contrato.

### Interactions with Other Systems

| Sistema | Le da a la librería | Recibe de la librería | Dueño de la interfaz |
|---|---|---|---|
| 2 Catálogo | Tabla de piezas (archivo de datos) | Se fija en la config con su huella | Catálogo |
| 3 Máquina de turno | Pipeline de fases, pila de deshacer, `EvaluateMatchEnd` | Corre dentro del contrato | Librería (contrato) y #3 (fases) |
| 4–11 Reglas del núcleo | Sus comandos, códigos y eventos | Respetan las reglas 1–12 | Cada GDD |
| 12 Timer | Comando `TurnTimeout` | Nada | Timer |
| 13 Entrada | Traduce clics en comandos | Consultas para resaltar casillas; códigos de rechazo | Entrada |
| 14 Localización | Textos de códigos y eventos | Códigos | Localización |
| 15–18, 22 Presentación | Nada | Estado, eventos y vista previa | Cada GDD |
| 19 Métricas | Nada | Eventos | Métricas |
| 25 Rival IA | Comandos por la misma API | Estado público y consultas | Rival IA |
| 26 Registro | Config, comandos y tiradas guardados | Reproduce aplicando los mismos comandos | Registro |
| 28 Servidor | Es la autoridad de Tirada y `TurnTimeout` | Corre la misma librería; compara huellas con el cliente | Servidor |

Ninguno de estos GDD existe todavía: las interfaces son contrato provisional hasta que cada
uno se escriba.

## Formulas

La librería casi no tiene números de juego propios: daño, alcance y probabilidades son de
otros GDD. Sus dos fórmulas convierten una tirada inyectada (regla 6) en un resultado. No
tienen ciclos de retroalimentación: la librería no contiene balance.

The `cara_por_tirada` formula is defined as:

`cara_por_tirada = la primera cara f (de 1 a k) tal que n < S_f, donde S_f = p_1 + p_2 + … + p_f`

**Variables:**
| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| n | int | 0–99 | calculado (Tirada inyectada) | Número que envía quien manda |
| k | int | 1–10 | constante | Cantidad de caras (el Modificador tiene 5) |
| p_i | int | 0–100 | data file (GDD del Modificador) | Probabilidad de la cara i, en % |
| S_f | int | 0–100 | calculado | Suma acumulada hasta la cara f |

**Output Range:** 1 a k. Las caras con p_i = 0 nunca salen. Si las p_i no suman exactamente
100 o alguna es negativa, la tabla se rechaza con `ProbabilidadesInvalidas` (no se corrige
en silencio). Un n fuera de 0–99 se rechaza con `TiradaInvalida`.
**Example:** tabla "Pierde 3/4" 20/20/25/20/15 y n = 47: S = 20, 40, 65…; 47 < 65 → cara 3,
Llamada al servicio. Con n = 99: S_5 = 100 y 99 < 100 → cara 5, Arremetida. Con n = 0 y
p_1 = 0: S_1 = 0, 0 < 0 es falso, sigue a la primera cara con probabilidad mayor que 0.

The `opcion_por_sorteo` formula is defined as:

`opcion_por_sorteo = (n × k) / 100` (división entera hacia abajo)

**Variables:**
| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| n | int | 0–99 | calculado (Tirada inyectada) | Número que envía quien manda |
| k | int | 1–100 | constante | Cantidad de opciones equiprobables |

**Output Range:** 0 a k − 1. Si 100 no es divisible por k, las primeras opciones reciben un
número más. Con k = 3 el reparto es 34/33/33; se acepta (decisión del 2026-11-19: un punto
de diferencia en un sorteo de posición no cambia la partida). Con k fuera de 1–100 la config
es inválida (`ConfigInvalida`).
**Example:** 3 pares de sectores para los castillos (4 y 6, 1 y 9, 7 y 3) y n = 50:
(50 × 3) / 100 = 1 → par "1 y 9". Con n = 0 → par 0; con n = 99 → (99 × 3) / 100 = 2 → par 2.

La huella del estado y la del Catálogo no son fórmulas de diseño: su algoritmo lo elige el
ADR de la librería.

## Edge Cases

**Quién manda cada comando**

- **If a player sends Tirada or `TurnTimeout`**: se rechaza con `SoloAutoridad`. Esos dos
  comandos solo los acepta la librería si vienen de la **autoridad** (el cliente local o el
  servidor), un tercer emisor distinto de los jugadores 0 y 1. Sin esta regla un cliente
  online podría inventarse la tirada que le conviene.
- **If a player sends a command outside their turn**: `NoEsTuTurno`, sin eventos.
- **If a command uses a rival piece**: `NoEsTuPieza`.
- **If a command uses the ID of a piece that died or never existed**: `PiezaInexistente`.
  Los IDs no se reusan, así que un ID viejo nunca apunta a otra pieza.

**Repeticiones y carreras online**

- **If the same command arrives twice** (doble clic, mensaje duplicado): la segunda copia se
  valida contra el estado nuevo y se rechaza (por ejemplo, `PiezaYaMovida`); ningún comando
  se aplica dos veces.
- **If `TurnTimeout` and Resolver arrive almost at once**: gana el que la autoridad aplica
  primero; el segundo se rechaza con `FaseIncorrecta` y no tiene efecto. El registro guarda
  el orden de la autoridad, así que la repetición da lo mismo.
- **If client and server end up with different state hashes**: manda el servidor. El
  cliente descarta su estado y adopta el del servidor, y la diferencia se reporta como
  error S1 (no debería pasar nunca).

**Deshacer y vista previa**

- **If Deshacer is sent with an empty stack**: `NadaQueDeshacer`.
- **If Deshacer is sent after TerminarMovimiento**: `FaseIncorrecta`. La pila se cierra al
  pasar a atacar y se vacía al empezar cada turno: una tirada o un reclutamiento nunca se
  deshacen.
- **If a preview is requested while waiting for a roll**: `FaseIncorrecta`. La vista previa
  nunca puede usar ni adelantar una tirada futura.
- **If a preview is requested with no attacks assigned**: devuelve una lista de eventos
  vacía; no es un error.

**Versiones, datos y fin de partida**

- **If a match or replay from another rules version or Catalog hash is loaded**:
  `VersionNoSoportada`; no se reproduce nada a medias.
- **If a saved state does not match its own hash** (archivo dañado o editado):
  `EstadoInvalido`; la partida no se carga.
- **If a probability table sums to 100 but every recruit face is at 0**: es válida; esas
  caras no salen.
- **If both Kings die in the same resolution**: gana el jugador que resolvió (regla de la
  v3, provisional hasta el GDD de la Máquina de turno, #3, que es su dueño). La librería
  garantiza que el resultado sea siempre el mismo.

## Dependencies

**Depende de: ninguno.** La librería es la base de todo el núcleo y no necesita a ningún
otro sistema para existir. La tabla del Catálogo (#2) entra como dato de la config: la
librería la recibe, la valida y la fija con su huella; el formato de esa tabla lo define
el GDD del Catálogo.

**Dependen de ella:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| 3 Máquina de turno | Dura | Escribe sus fases dentro del contrato; pila de deshacer; `EvaluateMatchEnd` |
| 4 Tablero y despliegue | Dura | CrearPartida, IDs estables, sorteo de castillos (`opcion_por_sorteo`) |
| 5–11 Reglas del núcleo | Dura | Escriben sus reglas, comandos, códigos y eventos dentro del contrato |
| 25 Rival IA | Dura | Juega con la misma API y las consultas |
| 26 Registro de partida | Dura | Guarda config, comandos y tiradas; reproduce con Aplicar |
| 28 Servidor de partida | Dura | Corre la misma librería; es la autoridad; compara huellas |
| 13, 15–19, 22, 24 Cliente | Blanda | Consultas, eventos y vista previa; funcionan con cualquier implementación que cumpla el contrato |

**Bidireccionalidad:** ninguno de esos GDD existe todavía. Cada uno, al escribirse, tiene
que nombrar a la Librería de reglas en su sección Dependencies.

## Tuning Knobs

**La librería no tiene perillas de balance propias.** Todo valor que un ajuste de balance
tocaría vive en el GDD que lo define; la librería solo lo recibe como dato:

| Valor | Dónde se ajusta | Nota |
|---|---|---|
| Estadísticas de las piezas | Catálogo (#2), archivo de datos | Cada partida fija la tabla que usó con su huella |
| Probabilidades p_i de cada tabla | Modificador (#11), archivo de datos | La librería solo exige que sumen 100 (`cara_por_tirada`) |
| Opciones del sorteo de castillos | Tablero (#4) | Hoy son 3 pares (`opcion_por_sorteo`, k = 3) |

**Constantes del contrato (no son perillas):**

- **Rango de la tirada, 0–99.** Las probabilidades se expresan en porcentajes enteros.
  Pasar a una granularidad más fina (por ejemplo 0–999) cambia el contrato y la versión de
  reglas.
- **Solo enteros y orden por ID.** Son la base del determinismo; no se ajustan.

## Visual/Audio Requirements

No aplica: la librería no dibuja ni suena. Sus eventos (regla 8) son la fuente de todo lo
visual y lo sonoro; Render (#18), Audio (#22) y Ficha y HUD (#15) definen cómo se ve y cómo
suena cada uno.

## Game Feel

No aplica como sensación directa. La librería sostiene la fantasía con dos requisitos: sin
demora perceptible (vista previa en 5 ms o menos y comandos en 1 ms o menos; AC-24, AC-25)
y resultados idénticos entre la vista previa y la resolución (AC-11).

## UI Requirements

La librería no tiene interfaz. Le exige a la interfaz:

- Todo código de rechazo tiene un texto traducido que dice por qué; no hay rechazos
  silenciosos (Entrada #13, Localización #14).
- Las probabilidades de la próxima tirada se pueden consultar desde el estado (Interfaz del
  Modificador #17; pilar 3b).
- La vista previa muestra exactamente los eventos que devuelve la librería, sin calcular
  nada por su cuenta (Vista previa #16).

## Cross-References

| Esta GDD referencia | En el sistema | Qué |
|---|---|---|
| Estadísticas de las piezas | Catálogo (#2) | Formato de la tabla de datos; huella por partida |
| Fases del turno, deshacer, fin de partida, dos Reyes muertos | Máquina de turno (#3) | Dueño de las fases y de `EvaluateMatchEnd` |
| Sorteo de castillos | Tablero (#4) | Usa `opcion_por_sorteo` con k = 3 |
| Probabilidades p_i | Modificador (#11) | Usa `cara_por_tirada`; las tablas suman 100 |
| Comando `TurnTimeout` | Timer (#12) | Lo emite la autoridad |
| Registro y repeticiones | Registro de partida (#26) | Guarda config, comandos y tiradas |
| Autoridad online y huellas | Servidor de partida (#28) | Aplica tiradas y timeouts; compara huellas |

Ninguno de esos GDD existe todavía: estas referencias se verifican cuando se escriban.

## Acceptance Criteria

Todos son de tipo **Lógica**: tests automáticos sobre la librería, sin Unity. AC-6, AC-24 y
AC-25 también corren en un build web de Unity (integración). Ninguno necesita captura.

**Reglas centrales**

- **AC-1 (pura)** — **GIVEN** un estado S con huella H, **WHEN** se aplica un comando
  válido, **THEN** S conserva la huella H y el resultado trae un estado nuevo y al menos
  un evento.
- **AC-2 (rechazo sin cambios)** — **GIVEN** un estado S, **WHEN** se envía Mover a una
  casilla ocupada, **THEN** el resultado es Rechazado con `DestinoOcupado`, la huella
  sigue siendo H y hay 0 eventos.
- **AC-3 (creación)** — **GIVEN** una config válida con la tabla de Catálogo T, **WHEN** se
  llama CrearPartida, **THEN** el estado lleva la versión de reglas y la huella de T. Con
  una tabla vacía **THEN** se devuelve `ConfigInvalida` y no se crea ninguna partida.
- **AC-4 (lista cerrada)** — **GIVEN** una partida en curso, **WHEN** se envía un tipo de
  comando que no está en la lista, **THEN** se rechaza con `ComandoDesconocido`.
- **AC-5 (todo o nada)** — **GIVEN** que no hay lugar libre para reclutar, **WHEN** se envía
  Reclutar, **THEN** se rechaza, la cantidad de piezas no cambia y la huella sigue siendo H.
- **AC-6 (determinismo entre plataformas)** — **GIVEN** la misma config y la misma secuencia
  de 500 comandos con sus tiradas, **WHEN** se reproduce dos veces en .NET y una en un
  build web de Unity, **THEN** las tres huellas finales y las tres listas de eventos son
  idénticas.
- **AC-7 (prohibiciones)** — **GIVEN** el código fuente de la librería, **WHEN** se buscan
  `float`, `double`, `decimal`, `DateTime`, `System.Random` y `UnityEngine`, **THEN** hay 0
  apariciones.
- **AC-8 (azar inyectado)** — **GIVEN** que se espera una tirada con la tabla
  20/20/25/20/15, **WHEN** la autoridad envía Tirada(47), **THEN** sale la cara 3 y el
  evento trae n = 47 y la tabla usada; en ese mismo estado un Mover se rechaza con
  `TiradaNoEsperada`.
- **AC-9 (IDs)** — **GIVEN** que muere la pieza con ID 5, **WHEN** se recluta una pieza
  nueva, **THEN** su ID es mayor que cualquier ID asignado antes, y un comando con el ID 5
  se rechaza con `PiezaInexistente`.
- **AC-10 (eventos)** — **GIVEN** dos atacantes contra el mismo objetivo, **WHEN** se envía
  Resolver, **THEN** cada evento de daño trae el ID del objetivo, el daño bruto, el escudo,
  el daño neto y la vida restante; repetir la misma resolución desde el mismo estado da una
  lista idéntica.
- **AC-11 (vista previa = resolución)** — **GIVEN** ataques asignados, **WHEN** se pide la
  vista previa y después se envía Resolver, **THEN** las dos listas de eventos son idénticas
  y la vista previa no cambió la huella.
- **AC-12 (consultas puras)** — **GIVEN** un estado S, **WHEN** se llama 100 veces a cada
  consulta, **THEN** la huella no cambia y los resultados son siempre iguales; con Grito de
  guerra activo, `EffectiveStats` devuelve el catálogo +2 en ataque y movimiento.
- **AC-13 (guardar y cargar)** — **GIVEN** una partida a la mitad, **WHEN** se guarda y se
  carga, **THEN** la huella es igual y los 50 comandos siguientes dan los mismos eventos que
  sin haber guardado.
- **AC-14 (versión)** — **GIVEN** un registro de una versión no soportada, **WHEN** se
  carga, **THEN** se rechaza con `VersionNoSoportada` y no se aplica ningún comando.

**Fórmulas**

- **AC-15 (`cara_por_tirada`)** — **GIVEN** la tabla 20/20/25/20/15, **WHEN** llegan
  n = 0, 19, 20, 47 y 99, **THEN** salen las caras 1, 1, 2, 3 y 5. **GIVEN** la tabla
  0/50/0/50/0, **WHEN** llegan n = 0, 49 y 50, **THEN** salen 2, 2 y 4. Una tabla que suma
  99 se rechaza con `ProbabilidadesInvalidas`; n = −1 y n = 100 con `TiradaInvalida`.
- **AC-16 (`opcion_por_sorteo`, k = 3)** — **GIVEN** k = 3, **WHEN** llegan n = 0, 33, 34,
  66, 67 y 99, **THEN** salen 0, 0, 1, 1, 2 y 2; recorriendo n de 0 a 99 el conteo es
  34/33/33. Con k = 0 la config se rechaza con `ConfigInvalida`.

**Casos borde**

- **AC-17** — **GIVEN** un jugador, **WHEN** envía Tirada o `TurnTimeout`, **THEN** se
  rechaza con `SoloAutoridad`.
- **AC-18** — **GIVEN** un Mover válido, **WHEN** se envía dos veces, **THEN** la segunda
  copia se rechaza con `PiezaYaMovida` y hay un solo evento de movimiento.
- **AC-19** — **GIVEN** que se espera una tirada, **WHEN** se pide la vista previa, **THEN**
  se rechaza con `FaseIncorrecta`.
- **AC-20** — **GIVEN** la pila de deshacer vacía, **WHEN** se envía Deshacer, **THEN**
  `NadaQueDeshacer`; **GIVEN** que ya se envió TerminarMovimiento, **WHEN** se envía
  Deshacer, **THEN** `FaseIncorrecta`.
- **AC-21** — **GIVEN** un estado guardado y editado a mano, **WHEN** se carga, **THEN** se
  rechaza con `EstadoInvalido`.
- **AC-22** — **GIVEN** que los dos Reyes mueren en la misma resolución, **WHEN** termina la
  resolución, **THEN** gana el jugador que resolvió.

**Entre sistemas y rendimiento**

- **AC-23 (cliente y servidor)** — **GIVEN** dos instancias con la misma config, **WHEN** la
  autoridad aplica 200 comandos y los reenvía en orden al cliente, **THEN** las huellas
  coinciden después de cada comando.
- **AC-24 (vista previa rápida)** — **GIVEN** el peor tablero posible (122 piezas, todos los
  topes llenos) y 60 ataques asignados, **WHEN** se pide la vista previa, **THEN** tarda
  5 ms o menos de promedio en 100 corridas, en el navegador de escritorio de referencia.
  Razón: a 60 fps un cuadro dura 16,6 ms y la vista previa se recalcula con cada flecha;
  tiene que caber en menos de un tercio del cuadro.
- **AC-25 (comando rápido)** — **GIVEN** ese mismo tablero, **WHEN** se aplica un Mover,
  **THEN** tarda 1 ms o menos de promedio en 100 corridas.
- **AC-26 (desacuerdo cliente y servidor)** — **GIVEN** un cliente cuya huella difiere de la
  del servidor, **WHEN** llega el estado del servidor, **THEN** el cliente lo adopta, su
  huella pasa a ser igual a la del servidor y se emite un evento de error S1 para el
  registro.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| Estructura del código, target del ensamblado (`netstandard2.1`, C# 9) y algoritmo de las huellas | ADR de la librería (`/architecture-decision`) | Antes de escribir código del MVP |
| Cómo se conservan las versiones viejas de la librería para reproducir repeticiones | ADR y Registro de partida (#26) | Antes de la Fase 2b |
| Navegador de escritorio de referencia para AC-24 y AC-25 | ADR de la librería | Antes de la primera prueba de rendimiento |
| Comando Rendirse | Máquina de turno (#3) y Servidor de partida (#28) | Fase 2 |
| Regla definitiva cuando mueren los dos Reyes | Máquina de turno (#3) | En su GDD |
| ¿El reclutamiento y la asignación de ataques entran en la pila de deshacer? (en la v4 solo entran los movimientos) | Máquina de turno (#3) | En su GDD |
