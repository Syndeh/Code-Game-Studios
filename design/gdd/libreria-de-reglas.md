# Librería de reglas (contrato)

> **Status**: In Design
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-19 (revisión tras /design-review: NEEDS REVISION)
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
lado: el azar del juego (la tirada del Modificador y el sorteo de la posición de los
castillos) entra como un dato registrado que envía la autoridad.

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
  oculta al resolver"*; pilar 3b, probabilidades consultables). La librería promete
  exactamente eso: repeticiones exactas y el mismo resultado en las dos pantallas. **No
  promete que la autoridad elija las tiradas de forma justa:** en el MVP la autoridad es
  de confianza (regla 6), y un compromiso verificable de las tiradas, si hace falta, lo
  agrega el Servidor de partida (#28) sin cambiar Aplicar.
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

1. **Dos puertas, ninguna más.** Una partida solo cambia con **Aplicar(estado, comando)** o
   con **Resincronizar(estado, estadoAutoridad)** (regla 14). Si el comando es válido,
   Aplicar devuelve **Aceptado** con el estado nuevo y una lista de eventos; si no, devuelve
   **Rechazado** con un código de motivo y el estado queda igual. Las dos son funciones
   puras: no modifican el estado que reciben y no tocan reloj, archivos, red, registros de
   consola ni Unity.
2. **Creación.** Una partida nace con **CrearPartida(config)**. La config incluye: la
   versión de reglas; el **modo** (local u online); la tabla del Catálogo (contenido y
   huella, un resumen que identifica esa tabla exacta); las tablas de probabilidad del
   Modificador; la posición de los castillos, fija o sorteada entre k opciones; y el modo
   de timer, solo como dato informativo (la librería no mide tiempo). CrearPartida valida
   **toda** la config antes de crear nada: Catálogo no vacío y con el formato del Catálogo
   (#2); cada tabla de probabilidad según *Formulas* (`ProbabilidadesInvalidas`); castillos
   dentro del tablero y sin superponerse; k del sorteo entre 1 y 100. Cualquier otro fallo
   devuelve `ConfigInvalida`. En los dos casos no se crea partida. Con castillos fijos la
   partida nace **En curso**; con castillos sorteados nace **esperando tirada** (regla 6) y
   castillos, piezas y muros se colocan al aplicar esa Tirada. La versión, la huella del
   Catálogo y el modo no cambian en toda la partida.
3. **Comandos: una lista cerrada.** Mover(pieza, destino) · Deshacer · TerminarMovimiento ·
   AsignarAtaque(pieza, objetivo) · QuitarAtaque(pieza) · Cubrir(escudero, aliado) ·
   Reclutar(tipo) · Resolver · TurnTimeout · Tirada(número 0–99). Las reglas de cada uno
   las define su GDD (#3 a #11); agregar un comando es un cambio de contrato y sube la
   versión de reglas. Cada comando lleva su **emisor**: jugador 0, jugador 1 o la
   **autoridad**. **El emisor lo pone quien llama a Aplicar, nunca el contenido del
   mensaje:** en modo local, el cliente; en modo online, el servidor, que lo deduce de la
   conexión autenticada y descarta cualquier emisor que traiga el mensaje. Tirada y
   `TurnTimeout` solo se aceptan de la autoridad. En modo online el cliente nunca actúa como
   autoridad: aplica los comandos que le reenvía el servidor, con el emisor que puso el
   servidor.
4. **Validación y códigos.** Todo comando se valida entero antes de cambiar nada: no hay
   comandos aplicados a medias. Los rechazos son **códigos**, nunca texto; la presentación
   los traduce con Localización (#14). La validación sigue **siempre este orden** y devuelve
   el primer código que falla:

   | Orden | Código | Cuándo | Para el jugador |
   |---|---|---|---|
   | 1 | `VersionNoSoportada` | El comando o el estado traen otra versión de reglas u otra huella del Catálogo | Sí ("hay que actualizar") |
   | 2 | `PartidaTerminada` | La partida ya terminó | Sí |
   | 3 | `ComandoDesconocido` | El tipo de comando no está en la lista cerrada | Interno |
   | 4 | `SoloAutoridad` | Un jugador envía Tirada o `TurnTimeout` | Interno |
   | 5 | `EsperandoTirada` | Se espera una tirada y llega otro comando de la autoridad o de un jugador (también `TurnTimeout`) | Sí ("esperando la tirada") |
   | 6 | `TiradaNoEsperada` | Llega una Tirada y no se espera ninguna | Interno |
   | 7 | `TiradaInvalida` | Tirada con n fuera de 0–99 | Interno |
   | 8 | `NoEsTuTurno` | El emisor no es el jugador del turno | Sí |
   | 9 | `PiezaInexistente` · `NoEsTuPieza` | El ID no existe (o la pieza murió) · la pieza es del rival | Sí |
   | 10 | `FaseIncorrecta` | El comando no corresponde a la fase actual | Sí |
   | 11 | Códigos de cada regla del núcleo | `FueraDeAlcance`, `DestinoOcupado`, `SinMovimientos`, `NadaQueDeshacer`, `TipoNoReclutable`… Cada GDD del núcleo define los suyos y si los ve el jugador | Según su GDD |

   Fuera de Aplicar hay cuatro códigos más: `ConfigInvalida` y `ProbabilidadesInvalidas`
   (CrearPartida), `EstadoInvalido` (cargar un estado o Resincronizar con una huella que no
   coincide) y `VersionNoSoportada` (cargar o Resincronizar con otra versión). Los códigos
   internos no le muestran un motivo al jugador: indican un error del cliente o del
   servidor y se reportan.
5. **Determinismo y enteros.**
   - Todos los valores son enteros de 32 bits con signo y la aritmética es **verificada**:
     un desbordamiento es un error de programación que se corta, nunca un valor que da la
     vuelta. Ninguna regla se acerca a ese límite: el producto más grande del contrato es
     n × k ≤ 99 × 100 = 9.900, y las sumas de probabilidades se hacen después de validar
     cada término (≤ 10 × 100).
   - La división entera se aplica solo a valores no negativos y redondea hacia abajo.
   - Una resta que puede quedar negativa se corta en 0: el daño neto es
     `max(0, bruto − escudo)` y la vida de una pieza o un muro nunca baja de 0.
   - Cuando hay que recorrer varios elementos, el orden es fijo: piezas por ID ascendente,
     casillas por fila y después por columna. Nunca se recorre una colección sin ordenarla
     antes; un diccionario o un conjunto se usa para buscar, nunca para recorrer.
   - Prohibidos el reloj, el generador de azar, la cultura del sistema, la coma flotante,
     el `GetHashCode` de la plataforma, la reflexión, los hilos y cualquier dato de la
     plataforma. La librería vive en un ensamblado sin referencias a Unity (detalle en el
     ADR).
6. **El azar se inyecta.** La librería nunca genera azar. Cuando una regla lo necesita (la
   tirada del Modificador, el sorteo de castillos), el estado queda **esperando tirada**,
   con sus probabilidades a la vista, y solo acepta Tirada(n), con n entre 0 y 99, de la
   autoridad. La librería convierte n en un resultado con la fórmula de *Formulas*. El
   número queda en los eventos y en el registro. Una tirada nunca se pide en medio de
   Resolver: los momentos de tirada son al crear la partida (sorteo) y al empezar un turno
   (Modificador, el momento exacto lo define la Máquina de turno, #3). **La autoridad es de
   confianza:** la librería no puede probar que n se eligió sin mirar el tablero. Tirada
   lleva solo n para que el Servidor (#28) pueda agregar después un compromiso de semilla
   sin cambiar Aplicar.
7. **IDs estables.** Cada pieza recibe un ID entero al aparecer (1, 2, 3…) que nunca se
   reusa, aunque la pieza muera. Cada tramo de muro se identifica por su casilla. Los
   jugadores son 0 (azul) y 1 (rojo).
8. **Eventos.** Todo comando aceptado devuelve una lista ordenada de eventos, siempre en el
   mismo orden, con **al menos un evento** que describe su efecto (también Deshacer,
   QuitarAtaque y TerminarMovimiento). Cada evento trae lo necesario para animar, sonar o
   medir sin leer el estado: IDs, casillas, daño antes y después del escudo, muerte, muro
   roto, cara y tabla de probabilidades. Un rechazo no produce eventos. Cada evento de daño
   indica además si fue **combinado**: verdadero cuando dos o más piezas atacantes
   distintas golpearon a ese mismo objetivo en esa resolución (por ataque directo o por
   área, aunque el escudo absorba todo), falso con un solo atacante. Con ese dato Audio (#22) y Render (#18) acentúan
   el golpe combinado (pilar 2: *"un golpe combinado se siente distinto de varios golpes
   sueltos"*).
9. **Vista previa = la misma operación.** La vista previa aplica Resolver sobre una
   **copia** del estado y devuelve sus eventos. No existe un segundo cálculo del daño. La
   copia es plana (arreglos de enteros) y se hace sobre un estado de trabajo ya reservado:
   en régimen estable la vista previa no reserva memoria. Si esa resolución terminaría la
   partida, la vista previa incluye el evento de fin con el ganador y el motivo.
10. **Consultas puras.** MovimientosLegales(pieza), ObjetivosValidos(pieza),
    `EffectiveStats(estado, pieza)` (catálogo + bonos del turno + posición, como el arquero
    sobre el muro), ProbabilidadesSiguienteTirada(jugador) y `EvaluateMatchEnd(estado)`
    leen el estado sin cambiarlo.
11. **Todo a la vista y una huella.** El estado contiene toda la partida: no hay
    información oculta (lo único que no existe todavía es la próxima tirada). Se puede
    guardar y leer completo en una **forma canónica**: un flujo de bytes con los campos en
    un orden fijo, enteros de 4 bytes en little-endian y colecciones ordenadas por la regla
    5 (el orden de campos exacto lo fija el ADR). La **huella** es un resumen de ese flujo
    con un algoritmo fijo, nunca el `GetHashCode` de la plataforma. Cubre todo el estado:
    versión, huella del Catálogo, modo, fase, jugador del turno, piezas, muros, ataques
    asignados, coberturas, tirada pendiente con su tabla, pila de deshacer, contadores de
    reclutas y el próximo ID. Dos estados iguales dan la misma huella aunque se hayan
    alcanzado por caminos distintos; cambiar cualquier campo cambia la huella.
12. **Versión.** El estado lleva la versión de reglas y la huella del Catálogo. Una
    librería que no soporta esa versión rechaza la partida con `VersionNoSoportada`. Cómo
    se conservan las versiones viejas para las repeticiones lo decide el ADR.
13. **Fuera de la librería:** el tiempo (solo recibe `TurnTimeout`), los textos, el
    dibujo, el sonido, los archivos, la red, las métricas y la IA (que usa la misma API que
    un jugador). Los números de secuencia de los mensajes, la reconexión y la carrera entre
    `TurnTimeout` y los comandos del cliente son del Servidor de partida (#28). **Sin
    comando Rendirse en el MVP local; es requisito antes de lanzar el online** (agregarlo
    sube la versión de reglas).
14. **Resincronizar.** Solo en modo online y solo con un estado que manda el servidor. Si
    la versión o la huella del Catálogo no son las de la partida devuelve
    `VersionNoSoportada`; si el estado no coincide con su propia huella devuelve
    `EstadoInvalido`. Si es válido, el resultado es el estado del servidor tal cual
    (también su pila de deshacer y su tirada pendiente) y un evento `Resincronizado` con la
    huella local vieja y la nueva: la interfaz avisa al jugador que el tablero se corrigió
    y Métricas (#19) lo registra como error S1. El cliente descarta su vista previa en curso
    y los comandos que todavía no confirmó el servidor.
15. **Resolución simultánea.** Resolver calcula todo el daño sobre el estado de **antes**
    de resolver: una pieza que muere en esa resolución igual golpea, y el orden por ID solo
    fija el orden de los eventos, nunca el resultado. El daño sobre un mismo objetivo se
    suma y el escudo se resta una sola vez (la fórmula es del GDD de Resolución, #9).
16. **Fin de partida.** Después de cada resolución, `EvaluateMatchEnd` decide si hay
    ganador. Si muere un solo Rey, gana el otro jugador. **Si mueren los dos Reyes en la
    misma resolución, gana el jugador del turno**, el que resolvió, sea con Resolver o
    porque `TurnTimeout` disparó la resolución de los ataques asignados (regla de la v3,
    provisional hasta el GDD de la Máquina de turno, #3, que es su dueño).

### States and Transitions

| Desde | Hacia | Cuándo |
|---|---|---|
| (ninguno) | En curso | CrearPartida con config válida y castillos fijos: piezas y muros colocados |
| (ninguno) | En curso · esperando tirada | CrearPartida con config válida y castillos sorteados; la Tirada coloca castillos, piezas y muros |
| (ninguno) | (sin partida) | Config inválida: `ConfigInvalida` o `ProbabilidadesInvalidas` |
| En curso | En curso · esperando tirada | Empieza un turno que necesita tirada (el momento lo define la Máquina de turno, #3); nunca dentro de Resolver |
| En curso · esperando tirada | En curso | Tirada(n) válida de la autoridad. Cualquier otro comando se rechaza: `SoloAutoridad` (Tirada o `TurnTimeout` de un jugador) o `EsperandoTirada` (el resto, `TurnTimeout` de la autoridad incluido) |
| En curso | Terminada (ganador, motivo) | Después de resolver, `EvaluateMatchEnd` encuentra un ganador (regla 16). Motivo inicial: Rey muerto; la Máquina de turno puede sumar el cierre por tiempo |
| Cualquiera (online) | El estado del servidor | Resincronizar válido (regla 14) |
| Terminada | Terminada | Todo comando se rechaza con `PartidaTerminada`; es un estado final |

Las fases del turno (tirada, reclutar, mover, atacar, resolver) son de la Máquina de turno
(#3), que las define dentro de este contrato. El reloj del turno no corre mientras se
espera una tirada: la autoridad la envía en cuanto se pide.

### Interactions with Other Systems

| Sistema | Le da a la librería | Recibe de la librería | Dueño de la interfaz |
|---|---|---|---|
| 2 Catálogo | Tabla de piezas (archivo de datos) | Se fija en la config con su huella | Catálogo |
| 3 Máquina de turno | Pipeline de fases, pila de deshacer, `EvaluateMatchEnd` | Corre dentro del contrato | Librería (contrato) y #3 (fases) |
| 4–11 Reglas del núcleo | Sus comandos, códigos y eventos | Respetan las reglas 1–16 | Cada GDD |
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
| n | int | 0–99 | inyectado (Tirada de la autoridad) | Número que envía la autoridad |
| k | int | 1–10 | largo de la tabla | Cantidad de caras: siempre es el largo de la tabla, no un dato aparte (el Modificador tiene 5) |
| p_i | int | 0–100 | data file (GDD del Modificador) | Probabilidad de la cara i, en % |
| S_f | int | 0–100 | calculado | Suma acumulada hasta la cara f |
| tirada_rango | int | 100 | constante del contrato | Cantidad de valores posibles de n (0 a 99) |

**Output Range:** 1 a k. Las caras con p_i = 0 nunca salen.
**Validación de la tabla, en CrearPartida y en este orden:** la tabla tiene entre 1 y 10
caras; cada p_i está entre 0 y 100; recién entonces se suman (el total nunca pasa de 1.000)
y la suma es exactamente `tirada_rango` = 100. Si algo falla, la partida no se crea y se
devuelve `ProbabilidadesInvalidas` (no se corrige en silencio). La tabla nunca se valida al
llegar la tirada, así que una partida nunca queda esperando una tirada imposible. Un n fuera
de 0–99 se rechaza con `TiradaInvalida`.
**Example:** tabla "Pierde 3/4" 20/20/25/20/15 y n = 47: S = 20, 40, 65…; 47 < 65 → cara 3,
Llamada al servicio. Con n = 99: S_5 = 100 y 99 < 100 → cara 5, Arremetida. Con n = 0 y
p_1 = 0: S_1 = 0, 0 < 0 es falso, sigue a la primera cara con probabilidad mayor que 0.

The `opcion_por_sorteo` formula is defined as:

`opcion_por_sorteo = (n × k) / tirada_rango` (división entera hacia abajo; `tirada_rango` = 100)

**Variables:**
| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| n | int | 0–99 | inyectado (Tirada de la autoridad) | Número que envía la autoridad |
| k | int | 1–100 | config (Tablero, #4) | Cantidad de opciones equiprobables |

**Output Range:** 0 a k − 1. El producto más grande es 99 × 100 = 9.900, sin riesgo de
desborde. Si 100 no es divisible por k, algunas opciones reciben un número más y cuáles
depende de k: con k = 3 es la primera (34/33/33), con k = 7 la primera y la cuarta
(15/14/14/15/14/14/14). La probabilidad que se muestra es esa cuenta exacta sobre 100,
nunca 100/k. Con k = 3 el reparto 34/33/33 se acepta (decisión del 2026-11-19: un punto
de diferencia en un sorteo de posición no cambia la partida). Con k fuera de 1–100 la config
es inválida (`ConfigInvalida`).
**Example:** 3 pares de sectores para los castillos (4 y 6, 1 y 9, 7 y 3) y n = 50:
(50 × 3) / 100 = 1 → par "1 y 9". Con n = 0 → par 0; con n = 99 → (99 × 3) / 100 = 2 → par 2.

La huella del estado y la del Catálogo no son fórmulas de diseño: su algoritmo lo elige el
ADR de la librería.

## Edge Cases

**Quién manda cada comando**

- **If a player sends Tirada or `TurnTimeout`**: se rechaza con `SoloAutoridad`. Esos dos
  comandos solo los acepta la librería si vienen de la **autoridad**. Online, el emisor lo
  pone el servidor según la conexión (regla 3): un mensaje que dice venir de la autoridad
  pero llega por la conexión de un jugador se aplica como de ese jugador. Sin esta regla un
  cliente online podría inventarse la tirada que le conviene.
- **If a player sends a command outside their turn**: `NoEsTuTurno`, sin eventos.
- **If a command uses a rival piece**: `NoEsTuPieza`.
- **If a command uses the ID of a piece that died or never existed**: `PiezaInexistente`.
  Los IDs no se reusan, así que un ID viejo nunca apunta a otra pieza.
- **If several checks fail at once** (por ejemplo, un jugador envía Tirada(150) con la
  partida terminada): se devuelve el primer código en el orden de la regla 4, en este caso
  `PartidaTerminada`.

**Repeticiones y carreras online**

- **If the same command arrives twice** (doble clic, mensaje duplicado): la segunda copia se
  valida contra el estado nuevo y se rechaza (para una pieza que ya movió, con el código
  que defina Movimiento, #5); ningún comando se aplica dos veces. Que un mensaje viejo
  vuelva a ser válido en un turno posterior lo evita el Servidor (#28) con números de
  secuencia, no la librería.
- **If `TurnTimeout` and Resolver arrive almost at once**: gana el que la autoridad aplica
  primero; el segundo se rechaza con `FaseIncorrecta` y no tiene efecto. El registro guarda
  el orden de la autoridad, así que la repetición da lo mismo.
- **If `TurnTimeout` arrives while waiting for a roll**: `EsperandoTirada`. El reloj del
  turno no corre hasta que llega la tirada.
- **If client and server end up with different state hashes**: manda el servidor. El
  cliente aplica Resincronizar con el estado del servidor (regla 14), el jugador ve el
  aviso y la diferencia se reporta como error S1 (no debería pasar nunca).
- **If the server state sent for Resincronizar does not match its own hash**:
  `EstadoInvalido`; el cliente conserva su estado y vuelve a pedirlo (reconexión del #28).

**Deshacer y vista previa**

- **If Deshacer is sent with an empty stack**: `NadaQueDeshacer`.
- **If Deshacer is sent after TerminarMovimiento**: `FaseIncorrecta`. La pila se cierra al
  pasar a atacar y se vacía al empezar cada turno: una tirada o un reclutamiento nunca se
  deshacen (provisional hasta la Máquina de turno, #3).
- **If a preview is requested while waiting for a roll**: `EsperandoTirada`. La vista
  previa nunca puede usar ni adelantar una tirada futura.
- **If a preview is requested with no attacks assigned**: devuelve una lista de eventos
  vacía; no es un error.
- **If the previewed resolution would end the match**: la vista previa incluye el evento de
  fin con el ganador y el motivo, así el jugador sabe antes de resolver quién gana.

**Enteros y resolución**

- **If the shield absorbs all the damage**: el daño neto es 0 (nunca negativo), el evento de
  daño se emite igual y, si golpearon dos o más atacantes, es combinado.
- **If a piece dies in the same resolution in which it attacks**: igual golpea; todo se
  calcula sobre el estado de antes de resolver (regla 15).
- **If a probability table sums to 100 but some recruit faces are at 0**: es válida; esas
  caras no salen. Una tabla con todas las caras en 0 suma 0 y es inválida
  (`ProbabilidadesInvalidas`).
- **If a probability table has huge values that would overflow when added**: cada p_i se
  valida entre 0 y 100 antes de sumar, así que la tabla se rechaza con
  `ProbabilidadesInvalidas` y la suma nunca desborda.

**Versiones, datos y fin de partida**

- **If a match or replay from another rules version or Catalog hash is loaded**:
  `VersionNoSoportada`; no se reproduce nada a medias.
- **If a saved state does not match its own hash** (archivo dañado o editado):
  `EstadoInvalido`; la partida no se carga.
- **If both Kings die in the same resolution**: gana el jugador del turno, el que resolvió
  (regla 16, provisional hasta la Máquina de turno, #3). La librería garantiza que el
  resultado sea siempre el mismo.
- **If `TurnTimeout` triggers the resolution of the assigned attacks**: cuenta como
  resolución del jugador del turno; si mueren los dos Reyes, gana ese jugador.

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

- **Rango de la tirada, 0–99 (`tirada_rango` = 100, `tirada_max` = 99).** Las
  probabilidades se expresan en porcentajes enteros y las dos fórmulas usan `tirada_rango`,
  nunca un 100 suelto.
  Pasar a una granularidad más fina (por ejemplo 0–999) cambia el contrato y la versión de
  reglas.
- **Solo enteros de 32 bits con aritmética verificada y orden por ID.** Son la base del
  determinismo; no se ajustan.
- **Presupuestos de rendimiento: vista previa 5 ms y comando 1 ms (percentil 95, AC-24 y
  AC-25).** Salen del cuadro de 16,6 ms a 60 fps; cambiarlos es una decisión de UX, no de
  balance.

## Visual/Audio Requirements

No aplica: la librería no dibuja ni suena. Sus eventos (regla 8) son la fuente de todo lo
visual y lo sonoro; Render (#18), Audio (#22) y Ficha y HUD (#15) definen cómo se ve y cómo
suena cada uno.

## Game Feel

No aplica como sensación directa. La librería sostiene la fantasía con dos requisitos: sin
demora perceptible (vista previa en 5 ms o menos y comandos en 1 ms o menos, en el percentil 95, sin reservar
memoria en la vista previa; AC-24, AC-25, AC-33)
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

**Tipos de prueba.** Son **Lógica** (tests automáticos sobre la librería en .NET, sin
Unity) salvo estos: AC-6 en su parte web y AC-32 son **Integración**; AC-24 y AC-25 son
**Rendimiento** (ADVISORY en CI, BLOCKING en la máquina, el navegador y el build de
referencia que fija el ADR). AC-7 corre como chequeo estático en CI. Ninguno necesita
captura.

**Fixtures provisionales.** Los ACs que dependen de reglas de GDD todavía no escritos
(Reclutamiento, Resolución, Movimiento, Modificador, Máquina de turno) usan un Catálogo y
reglas de prueba guardados en el repositorio, y sus códigos van marcados **PROVISIONAL**:
se actualizan cuando el GDD dueño se escribe.

**Reglas centrales**

- **AC-1 (pura)** — **GIVEN** un estado S con huella H y una copia profunda C de S, **WHEN**
  se aplica un comando válido (un test por cada uno de Mover, Deshacer, QuitarAtaque y
  TerminarMovimiento), **THEN** S es igual a C campo por campo, su huella sigue siendo H, y
  el resultado trae un estado nuevo y al menos un evento.
- **AC-2 (rechazo sin cambios)** — **GIVEN** un estado S, **WHEN** se envía Mover a una
  casilla ocupada, **THEN** el resultado es Rechazado con `DestinoOcupado`, la huella
  sigue siendo H y hay 0 eventos.
- **AC-3 (creación)** — **GIVEN** una config válida con la tabla de Catálogo T y castillos
  fijos, **WHEN** se llama CrearPartida, **THEN** el estado está En curso y lleva la versión
  de reglas, el modo y la huella de T. Con una tabla de Catálogo vacía **THEN** se devuelve
  `ConfigInvalida` y no se crea ninguna partida.
- **AC-4 (lista cerrada)** — **GIVEN** una partida en curso, **WHEN** se envía un comando
  con un tipo cuyo valor numérico no está en la lista cerrada (por ejemplo 99), **THEN** se
  rechaza con `ComandoDesconocido`.
- **AC-5 (todo o nada)** — **GIVEN** un fixture con 0 casillas libres para reclutar,
  **WHEN** se envía Reclutar, **THEN** se rechaza con el código de Reclutamiento (#10;
  PROVISIONAL `SinLugarParaReclutar`), la cantidad de piezas no cambia, la huella sigue
  siendo H y hay 0 eventos.
- **AC-6 (determinismo entre plataformas)** — **GIVEN** la misma config y la misma
  secuencia de 500 comandos con sus tiradas, y un archivo dorado guardado en el repositorio
  con la huella después de cada comando y la lista de eventos, **WHEN** se reproduce en
  .NET sobre Windows y sobre Linux (en cada PR) y en un build web de Unity (cada noche),
  **THEN** las tres corridas coinciden byte a byte con el archivo dorado.
- **AC-7 (prohibiciones)** — **GIVEN** el código fuente de la librería, **WHEN** corre el
  chequeo estático de CI, **THEN** hay 0 apariciones de `float`, `double`, `decimal`,
  `DateTime`, `Stopwatch`, `System.Random`, `Guid`, `Math.`, `GetHashCode`,
  `CultureInfo`, `System.Reflection`, `System.Threading`, `UnityEngine` y de `foreach`
  sobre `Dictionary` o `HashSet`; y el ensamblado de la librería no referencia a Unity.
- **AC-8 (azar inyectado)** — **GIVEN** que se espera una tirada con la tabla
  20/20/25/20/15, **WHEN** la autoridad envía Tirada(47), **THEN** sale la cara 3, el evento
  trae n = 47 y la tabla usada, y el estado deja de esperar tirada. En el estado anterior
  (esperando), un Mover del jugador del turno se rechaza con `EsperandoTirada`.
- **AC-9 (IDs)** — **GIVEN** que muere la pieza con ID 5, **WHEN** aparece una pieza nueva
  (reclutada o colocada por el fixture), **THEN** su ID es mayor que cualquier ID asignado
  antes, y un comando con el ID 5 se rechaza con `PiezaInexistente`.
- **AC-10 (eventos)** — **GIVEN** un fixture con los atacantes A (daño 3) y B (daño 2)
  contra el objetivo T (vida 10, escudo 1), **WHEN** se envía Resolver, **THEN** hay un
  evento de daño sobre T con bruto 5, escudo 1, neto 4, vida restante 6 y combinado
  verdadero (valores PROVISIONALES hasta la Resolución, #9), y repetir la misma resolución
  desde el mismo estado da una lista idéntica.
- **AC-11 (vista previa = resolución)** — **GIVEN** ataques asignados, **WHEN** se pide la
  vista previa y después se envía Resolver, **THEN** las dos listas de eventos son idénticas
  y la vista previa no cambió la huella.
- **AC-12a (consultas puras)** — **GIVEN** un estado S, **WHEN** se llama 100 veces a cada
  una de MovimientosLegales, ObjetivosValidos, `EffectiveStats`,
  ProbabilidadesSiguienteTirada y `EvaluateMatchEnd`, **THEN** la huella no cambia y cada
  consulta devuelve siempre el mismo resultado.
- **AC-12b (estadísticas efectivas)** — **GIVEN** un fixture con un bono de turno de +2 en
  ataque y movimiento sobre una pieza (PROVISIONAL: Grito de guerra del Modificador, #11),
  **WHEN** se llama `EffectiveStats`, **THEN** devuelve los valores del catálogo +2 en
  ataque y movimiento.
- **AC-13 (guardar y cargar)** — **GIVEN** una partida a la mitad de un fixture, **WHEN** se
  guarda y se carga, **THEN** la huella es igual y los 50 comandos siguientes del fixture
  dan los mismos eventos que sin haber guardado.
- **AC-14 (versión)** — **GIVEN** un registro de una versión de reglas no soportada, o de
  la misma versión con otra huella de Catálogo, **WHEN** se carga, **THEN** se rechaza con
  `VersionNoSoportada` y no se aplica ningún comando.

**Fórmulas**

- **AC-15 (`cara_por_tirada`)** — **GIVEN** la tabla 20/20/25/20/15, **WHEN** llegan
  n = 0, 19, 20, 47 y 99, **THEN** salen las caras 1, 1, 2, 3 y 5. **GIVEN** la tabla
  0/50/0/50/0, **WHEN** llegan n = 0, 49 y 50, **THEN** salen 2, 2 y 4. En CrearPartida, las
  tablas que suman 99, que tienen un −1, que tienen 11 caras o que son
  [2147483647, 2147483647, 102] se rechazan con `ProbabilidadesInvalidas`. Con la autoridad
  esperando tirada, n = −1 y n = 100 se rechazan con `TiradaInvalida`.
- **AC-16 (`opcion_por_sorteo`)** — **GIVEN** k = 3, **WHEN** llegan n = 0, 33, 34, 66, 67 y
  99, **THEN** salen 0, 0, 1, 1, 2 y 2; recorriendo n de 0 a 99 el conteo es 34/33/33.
  Recorriendo n de 0 a 99: con k = 1 sale siempre 0; con k = 7 el conteo es
  15/14/14/15/14/14/14; con k = 100 sale n. Con k = 0 o k = 101 la config se rechaza con
  `ConfigInvalida`.

**Casos borde**

- **AC-17** — **GIVEN** un jugador, **WHEN** envía Tirada o `TurnTimeout`, **THEN** se
  rechaza con `SoloAutoridad`.
- **AC-18** — **GIVEN** un Mover válido, **WHEN** se envía dos veces seguidas, **THEN** la
  segunda copia es Rechazado (código de Movimiento, #5; PROVISIONAL `PiezaYaMovida`), la
  huella es la misma que después de la primera copia y en total hay un solo evento de
  movimiento.
- **AC-19** — **GIVEN** que se espera una tirada, **WHEN** se pide la vista previa, **THEN**
  se rechaza con `EsperandoTirada`.
- **AC-20** (PROVISIONAL hasta la Máquina de turno, #3) — **GIVEN** la pila de deshacer
  vacía, **WHEN** se envía Deshacer, **THEN** `NadaQueDeshacer`; **GIVEN** que ya se envió
  TerminarMovimiento, **WHEN** se envía Deshacer, **THEN** `FaseIncorrecta`.
- **AC-21** — **GIVEN** un estado guardado y editado a mano, **WHEN** se carga, **THEN** se
  rechaza con `EstadoInvalido`.
- **AC-22** (PROVISIONAL hasta la Máquina de turno, #3) — **GIVEN** un fixture en el que los
  ataques asignados matan a los dos Reyes, **WHEN** el jugador del turno envía Resolver,
  **THEN** la vista previa pedida antes ya trae el evento de fin con ese jugador como ganador, y
  la partida termina con ese ganador. **WHEN** en cambio la autoridad envía `TurnTimeout`
  en la fase de ataque, **THEN** el ganador es el mismo jugador del turno.

**Entre sistemas y rendimiento**

- **AC-23 (cliente y servidor)** — **GIVEN** dos instancias con la misma config, **WHEN** la
  autoridad aplica 200 comandos y los reenvía en orden al cliente, **THEN** las huellas
  coinciden después de cada comando.
- **AC-24 (vista previa rápida)** — **GIVEN** el fixture del peor tablero, guardado en el
  repositorio: 122 piezas, que son los topes de piezas vivas del concepto
  (12 + 16 + 12 + 8 + 8 + 4 = 60) más el Rey, por 2 jugadores, y 60 ataques asignados,
  **WHEN** se pide la vista previa 100 veces después de 20 corridas de calentamiento,
  **THEN** el percentil 95 es de 5 ms o menos en la máquina, el navegador y el build de
  referencia. Razón: a 60 fps un cuadro dura 16,6 ms y la vista previa se recalcula con
  cada flecha; tiene que caber en menos de un tercio del cuadro.
- **AC-25 (comando rápido)** — **GIVEN** ese mismo fixture, **WHEN** se aplica un Mover 100
  veces después de 20 de calentamiento, **THEN** el percentil 95 es de 1 ms o menos en la
  misma referencia.
- **AC-26 (Resincronizar)** — **GIVEN** un cliente en modo online cuya huella difiere de la
  del servidor, **WHEN** se aplica Resincronizar con el estado válido del servidor, **THEN**
  el estado del cliente es igual al del servidor, las huellas coinciden y hay un solo
  evento `Resincronizado` con la huella vieja y la nueva. **WHEN** el estado del servidor no
  coincide con su propia huella, **THEN** se rechaza con `EstadoInvalido` y el estado del
  cliente no cambia.

**Contrato agregado en la revisión del 2026-11-19**

- **AC-27 (combinado)** — **GIVEN** un fixture de resolución, **WHEN** dos atacantes golpean
  un mismo objetivo, **THEN** su evento de daño es combinado; con un solo atacante no lo es;
  con dos atacantes contra dos objetivos distintos ninguno lo es; con dos atacantes cuyo
  daño absorbe entero el escudo, el neto es 0 y es combinado.
- **AC-28 (config de castillos)** — **GIVEN** una config con un castillo fuera del tablero,
  o con dos castillos superpuestos, **WHEN** se llama CrearPartida, **THEN** se devuelve
  `ConfigInvalida` y no se crea partida.
- **AC-29 (orden de los rechazos)** — **WHEN** un jugador envía Tirada(150) con la partida
  terminada **THEN** `PartidaTerminada`; con la partida en curso **THEN** `SoloAutoridad`.
  **WHEN** la autoridad envía Tirada(150) sin tirada pendiente **THEN** `TiradaNoEsperada`;
  con tirada pendiente **THEN** `TiradaInvalida`. **WHEN** el jugador que no tiene el turno
  envía Mover mientras se espera tirada **THEN** `EsperandoTirada`.
- **AC-30 (huella)** — **GIVEN** dos secuencias de comandos que mueven las mismas piezas a
  las mismas casillas en distinto orden, **WHEN** llegan al comienzo del turno siguiente,
  **THEN** las dos huellas son iguales. **GIVEN** un estado, **WHEN** se cambia un solo
  campo (un test por cada campo de la regla 11), **THEN** la huella cambia. **GIVEN** 3
  estados de fixture con su huella guardada en el repositorio, **WHEN** se calcula la huella
  en .NET y en el build web, **THEN** coincide con la guardada.
- **AC-31 (QuitarAtaque y Cubrir)** — **GIVEN** la fase de movimiento, **WHEN** se envía
  QuitarAtaque o Cubrir, **THEN** se rechazan con `FaseIncorrecta` (no con
  `ComandoDesconocido`).
- **AC-32 (emisor sellado)** — **GIVEN** una partida online en el servidor (#28), **WHEN**
  por la conexión del jugador 1 llega una Tirada cuyo mensaje dice venir de la autoridad,
  **THEN** el servidor la aplica con emisor jugador 1 y se rechaza con `SoloAutoridad`.
- **AC-33 (vista previa sin memoria)** — **GIVEN** el fixture de AC-24 y 10 vistas previas
  de calentamiento, **WHEN** se piden 100 vistas previas más, **THEN** la memoria reservada
  por el hilo durante esas 100 es de 0 bytes.
- **AC-34 (Resolver no consume tiradas)** — **GIVEN** ataques asignados en un turno cuyo
  siguiente turno necesita tirada, **WHEN** se envía Resolver, **THEN** los eventos de daño
  son iguales a los de la vista previa, el estado resultante queda esperando tirada y
  ningún evento de esa resolución trae una tirada.
- **AC-35 (timeout esperando tirada)** — **GIVEN** que se espera una tirada, **WHEN** la
  autoridad envía `TurnTimeout`, **THEN** se rechaza con `EsperandoTirada` y la huella no
  cambia.
- **AC-36 (sorteo de castillos)** — **GIVEN** una config con castillos sorteados entre 3
  pares, **WHEN** se llama CrearPartida, **THEN** la partida queda esperando tirada con 0
  piezas; un Mover se rechaza con `EsperandoTirada`; **WHEN** la autoridad envía
  Tirada(50), **THEN** se colocan los castillos del par 1 con sus piezas y muros y la
  partida queda En curso.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| Estructura del código, target del ensamblado (`netstandard2.1`, C# 9), orden exacto de los campos de la forma canónica y algoritmo de la huella | ADR de la librería (`/architecture-decision`) | Antes de escribir código del MVP |
| Cómo se conservan las versiones viejas de la librería para reproducir repeticiones | ADR y Registro de partida (#26) | Antes de la Fase 2b |
| Máquina, navegador y build (release o development) de referencia para AC-24 y AC-25 | ADR de la librería | Antes de la primera prueba de rendimiento |
| Compromiso verificable de las tiradas online (semilla comprometida al crear la partida) | Servidor de partida (#28) | Antes de lanzar el online |
| Números de secuencia, reconexión y carrera entre `TurnTimeout` y comandos del cliente | Servidor de partida (#28) | Antes de lanzar el online |
| Comando Rendirse | Máquina de turno (#3) y Servidor de partida (#28) | Antes de lanzar el online |
| Regla definitiva cuando mueren los dos Reyes (hoy: gana el jugador del turno) | Máquina de turno (#3) | En su GDD |
| ¿El reclutamiento y la asignación de ataques entran en la pila de deshacer? (en la v4 solo entran los movimientos) | Máquina de turno (#3) | En su GDD |
