# Librería de reglas (contrato)

> **Status**: In Design
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-19 (segunda revisión tras la re-revisión de /design-review: NEEDS REVISION)
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
  promete que la autoridad elija las tiradas ni el momento de los timeouts de forma
  justa:** en el MVP la autoridad es de confianza (regla 6), y un compromiso verificable de
  las tiradas, si hace falta, lo agrega el Servidor de partida (#28) sin cambiar Aplicar.
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
   **Rechazado** con un código de motivo y el estado queda igual. Hay un tercer resultado,
   **ErrorInterno**, solo para un error de programación (regla 5): el estado recibido queda
   intacto y el error se reporta como S1. Las dos son funciones puras: no modifican el
   estado que reciben y no tocan reloj, archivos, red, registros de consola ni Unity.
   "Pura" quiere decir que no modifica la entrada, no que reserve memoria nueva: quien
   llama puede pasar un estado de destino y un buffer de eventos ya reservados, que nunca
   son el mismo objeto que el estado de origen (la firma exacta la fija el ADR).
2. **Creación.** Una partida nace con **CrearPartida(config)**. La config incluye: la
   versión de reglas; el **id de partida** (un entero que pone quien crea la partida); el
   **modo** (local u online); la tabla del Catálogo (contenido y huella, un resumen que
   identifica esa tabla exacta); las tablas de probabilidad del Modificador, con el papel
   de cada cara (reclutar, Grito de guerra, Arremetida u otra); la posición de los
   castillos, fija o sorteada entre k opciones; y el modo de timer, solo como dato
   informativo (la librería no mide tiempo). CrearPartida valida **toda** la config antes
   de crear nada: Catálogo no vacío y con el formato del Catálogo (#2); cada tabla de
   probabilidad según *Formulas* (`ProbabilidadesInvalidas`); k del sorteo entre 1 y 100; y
   los castillos dentro del tablero y sin superponerse **en cada una de las k opciones**,
   no solo en la que salga, así una Tirada nunca coloca castillos inválidos. Si fallan las
   tablas y algo más, gana `ProbabilidadesInvalidas`; cualquier otro fallo devuelve
   `ConfigInvalida`. En los dos casos no se crea partida. Con castillos fijos la partida
   nace **En curso**; con castillos sorteados nace **esperando tirada** (regla 6) y
   castillos, piezas y muros se colocan al aplicar esa Tirada. La versión, el id de
   partida, la huella del Catálogo y el modo no cambian en toda la partida.
3. **Comandos: una lista cerrada.** Mover(pieza, destino) · Deshacer · TerminarMovimiento ·
   AsignarAtaque(pieza, objetivo) · QuitarAtaque(pieza) · Cubrir(escudero, aliado) ·
   Reclutar(tipo) · Resolver · Rendirse · TurnTimeout(turno, etapa) · Tirada(número 0–99).
   Las reglas de cada uno las define su GDD (#3 a #11); agregar un comando es un cambio de
   contrato y sube la versión de reglas. Cada comando lleva su **emisor**: jugador 0,
   jugador 1 o la **autoridad**. **El emisor es un parámetro de Aplicar que pone quien
   llama, nunca un campo del mensaje:** en modo local, el cliente; en modo online, el
   servidor, que lo deduce de la conexión autenticada y descarta cualquier emisor que
   traiga el mensaje. **Comandos de autoridad:** Tirada y `TurnTimeout`, y la autoridad no
   envía ningún otro. **Comandos de jugador:** todos los demás. En modo online el cliente
   nunca actúa como autoridad: los comandos de autoridad solo los aplica tal como se los
   reenvía el servidor.
   - `TurnTimeout(turno, etapa)` nombra el turno (`numeroTurno`, regla 11) y la etapa del
     timer (mover o atacar) que vencieron. Un timeout que no coincide con el turno y la
     etapa actuales es viejo y se rechaza con `TimeoutObsoleto`, sin efecto.
   - `Rendirse` lo puede enviar cualquiera de los dos jugadores en cualquier momento de una
     partida en curso, también fuera de su turno y mientras se espera una tirada: termina
     la partida, gana el otro jugador y el motivo es Rendición. La interfaz del MVP local no
     tiene que ofrecerlo; el online no se lanza sin él.
4. **Validación y códigos.** Todo comando se valida entero antes de cambiar nada: no hay
   comandos aplicados a medias. Los rechazos son **códigos**, nunca texto; la presentación
   los traduce con Localización (#14). La validación sigue **siempre este orden** y devuelve
   el primer código que falla. Las filas 6 a 8 solo se aplican a comandos de autoridad y las
   filas 9 a 12, solo a comandos de jugador; `Rendirse` válido pasa la fila 4 y se acepta:

   | Orden | Código | Cuándo | Para el jugador |
   |---|---|---|---|
   | 1 | `VersionNoSoportada` | El comando o el estado traen otra versión de reglas u otra huella del Catálogo | Sí ("hay que actualizar") |
   | 2 | `PartidaTerminada` | La partida ya terminó | Sí |
   | 3 | `ComandoDesconocido` | El tipo de comando no está en la lista cerrada | Interno |
   | 4 | `SoloAutoridad` · `SoloJugador` | Un jugador envía un comando de autoridad · la autoridad envía un comando de jugador | Interno |
   | 5 | `EsperandoTirada` | Se espera una tirada y llega cualquier comando que no sea Tirada ni `Rendirse` (también `TurnTimeout`) | Sí ("esperando la tirada") |
   | 6 | `TiradaNoEsperada` | Llega una Tirada y no se espera ninguna | Interno |
   | 7 | `TiradaInvalida` | Tirada con n fuera de 0–99 | Interno |
   | 8 | `TimeoutObsoleto` | `TurnTimeout` con un turno o una etapa que no son los actuales | Interno (se descarta sin aviso) |
   | 9 | `NoEsTuTurno` | El emisor no es el jugador del turno | Sí |
   | 10 | `PiezaInexistente` · `NoEsTuPieza` | El ID no existe (o la pieza murió) · la pieza es del rival | Sí |
   | 11 | `FaseIncorrecta` | El comando no corresponde a la fase actual | Sí |
   | 12 | Códigos de cada regla del núcleo | `FueraDeAlcance`, `DestinoOcupado`, `SinMovimientos`, `NadaQueDeshacer`, `TipoNoReclutable`… Cada GDD del núcleo define los suyos y si los ve el jugador | Según su GDD |

   Fuera de Aplicar hay más códigos: `ConfigInvalida` y `ProbabilidadesInvalidas`
   (CrearPartida); `EstadoInvalido` (cargar un estado o Resincronizar con una huella que no
   coincide); `VersionNoSoportada` (cargar o Resincronizar con otra versión); y
   `SoloOnline`, `OtraPartida`, `EstadoViejo` y `PartidaTerminada` (Resincronizar, regla
   14). Los códigos internos no le muestran un motivo al jugador: indican un error del
   cliente o del servidor y se reportan.
5. **Determinismo y enteros.**
   - Todos los valores son enteros de 32 bits con signo y la aritmética es **verificada**:
     un desbordamiento es un error de programación, nunca un valor que da la vuelta. Aplicar
     lo devuelve como **ErrorInterno** (regla 1), con el estado de entrada intacto, en
     todas las plataformas por igual: la verificación está escrita en el código de la
     librería, no depende de una opción de compilación (cómo, lo fija el ADR). Ninguna
     regla se acerca a ese límite: el producto más grande del contrato es
     n × k ≤ 99 × 100 = 9.900, y las sumas de probabilidades se hacen después de validar
     cada término (≤ 10 × 100).
   - La división entera se aplica solo a valores no negativos y redondea hacia abajo.
   - Una resta que puede quedar negativa se pone en 0: el daño neto es
     `max(0, bruto − escudo)`, donde **bruto es la suma de todo el daño sobre ese objetivo
     en la resolución** (regla 15), y la vida de una pieza o un muro nunca baja de 0.
   - Cuando hay que recorrer varios elementos, el orden es fijo: piezas por ID ascendente,
     casillas por fila y después por columna. Nunca se recorre una colección sin ordenarla
     antes; un diccionario o un conjunto se usa para buscar, nunca para recorrer. Todo
     ordenamiento usa un **orden total**: si la clave puede repetirse (daño, vida, tipo),
     el desempate final es el ID o la casilla, así el resultado no depende de que el
     algoritmo de ordenamiento sea estable.
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
   (Modificador, el momento exacto lo define la Máquina de turno, #3).
   - **Tabla efectiva del Modificador.** La librería calcula la tabla al entrar en
     "esperando tirada" y la guarda en el estado: elige la tabla base según la regla del
     Modificador (#11) y pone en 0 las caras de reclutar que no pueden salir (muro roto o
     llamadas agotadas), repartiendo su parte con `reparto_liberado` (*Formulas*). La
     tabla efectiva se valida igual que en CrearPartida; si no pasa, es ErrorInterno, que
     la fórmula hace inalcanzable. Lo que el jugador consulta
     (`ProbabilidadesSiguienteTirada`) es esa misma tabla.
   - El estado cuenta las tiradas aplicadas (`contadorTirada`).
   - **La autoridad es de confianza:** la librería no puede probar que n se eligió sin
     mirar el tablero. Tirada lleva solo n para que el Servidor (#28) pueda agregar después
     un compromiso de semilla atado a `contadorTirada` sin cambiar Aplicar.
7. **IDs estables.** Cada pieza recibe un ID entero al aparecer (1, 2, 3…) que nunca se
   reusa, aunque la pieza muera. Cuando aparecen varias piezas a la vez, los IDs se
   asignan en un orden fijo: en el despliegue inicial, primero las del jugador 0 y después
   las del jugador 1; dentro de cada jugador, por el orden de tipos del Catálogo y después
   por casilla (fila, columna). En un reclutamiento de varias piezas, por casilla de
   aparición (fila, columna). Cada tramo de muro se identifica por su casilla. Los jugadores
   son 0 (azul) y 1 (rojo).
8. **Eventos.** Todo comando aceptado devuelve una lista ordenada de eventos, siempre en el
   mismo orden, con **al menos un evento** que describe su efecto (también Deshacer,
   QuitarAtaque y TerminarMovimiento). Cada evento trae lo necesario para animar, sonar o
   medir sin leer el estado: IDs, casillas, daño antes y después del escudo, muerte, muro
   roto, cara y tabla de probabilidades. El evento de daño trae además la vida que
   realmente perdió el objetivo (con 30 de daño neto y 5 de vida, pierde 5). Resolver sin
   ataques asignados emite igual el evento `TurnoTerminado`. Un rechazo no produce
   eventos. Cada evento de daño indica además si fue **combinado**: verdadero cuando dos o
   más piezas distintas **del rival del dueño del objetivo** lo golpearon en esa
   resolución (por ataque directo o por área, aunque el escudo absorba todo), falso con un
   solo atacante rival. El fuego amigo nunca cuenta para el combinado (decisión del
   2026-11-19: no se celebra un golpe contra uno mismo). El daño que un escudero recibe por
   cubrir a un aliado cuenta como golpe al escudero. Con ese dato Audio (#22) y Render (#18)
   acentúan el golpe combinado (pilar 2: *"un golpe combinado se siente distinto de varios
   golpes sueltos"*).
9. **Vista previa = la misma operación.** La vista previa aplica Resolver sobre una
   **copia** del estado y devuelve **exactamente** lo que devolvería Resolver: la misma
   lista de eventos (también `TurnoTerminado` sin ataques, y el evento de fin con ganador y
   motivo si la partida terminaría) o el mismo código de rechazo (por ejemplo
   `EsperandoTirada`, `NoEsTuTurno`, `FaseIncorrecta` o `PartidaTerminada`). No existe un
   segundo cálculo del daño. La copia es plana (arreglos de enteros) y se hace sobre un
   estado de trabajo y un buffer de eventos ya reservados, de capacidad fija y nunca
   compartidos entre partidas: en régimen estable la vista previa no reserva memoria.
10. **Consultas puras.** MovimientosLegales(pieza), ObjetivosValidos(pieza),
    `EffectiveStats(estado, pieza)` (catálogo + bonos del turno + posición, como el arquero
    sobre el muro), ProbabilidadesSiguienteTirada(jugador) y `EvaluateMatchEnd(estado)`
    leen el estado sin cambiarlo.
11. **Todo a la vista y una huella.** El estado contiene toda la partida: no hay
    información oculta (lo único que no existe todavía es la próxima tirada). Se puede
    guardar y leer completo en una **forma canónica**: un flujo de bytes con los campos en
    un orden fijo, enteros de 4 bytes en little-endian escritos byte a byte (nunca con el
    orden de bytes de la plataforma), **la cantidad de elementos antes de cada colección** y
    **un byte de presencia antes de cada campo opcional**, así dos estados distintos nunca
    dan el mismo flujo. Las colecciones van ordenadas por la regla 5 (el orden de campos
    exacto lo fija el ADR). La **huella** es un resumen de ese flujo con un algoritmo fijo,
    nunca el `GetHashCode` de la plataforma. Cubre **todos los campos del estado, sin
    excepción**: lo que no esté en la forma canónica no puede estar en el estado. Hoy eso
    incluye versión, id de partida, huella del Catálogo, modo, tablas base del Modificador,
    `numeroTurno`, fase y etapa, jugador del turno, cara del Modificador de este turno y sus
    bonos (Grito de guerra), piezas con sus movimientos usados, muros, par de castillos
    sorteado, ataques asignados, coberturas, tirada pendiente (de qué tipo, para qué
    jugador y con su tabla efectiva), pila de deshacer, contadores de reclutas,
    `contadorTirada`, `contadorComandos` (comandos aceptados), próximo ID, y ganador y
    motivo de fin. Un test de exhaustividad falla si se agrega un campo al estado sin
    agregarlo a la forma canónica (AC-45). Dos estados con los mismos campos dan la misma
    huella aunque se hayan alcanzado por caminos distintos; cambiar cualquier campo cambia
    la huella. Aplicar no calcula la huella: es una consulta aparte, `Huella(estado)`, y
    quien la usa decide cada cuánto la pide.
12. **Versión.** El estado lleva la versión de reglas y la huella del Catálogo. Una
    librería que no soporta esa versión rechaza la partida con `VersionNoSoportada`. Cómo
    se conservan las versiones viejas para las repeticiones lo decide el ADR.
13. **Fuera de la librería:** el tiempo (solo recibe `TurnTimeout`), los textos, el
    dibujo, el sonido, los archivos, la red, las métricas y la IA (que usa la misma API que
    un jugador). Los números de secuencia de los mensajes, la reconexión, la vida de la
    autoridad (qué pasa si deja de responder mientras se espera una tirada) y el modelo de
    red del cliente son del Servidor de partida (#28). **El contrato es neutral respecto
    del modelo de red:** sirve tanto si el cliente solo aplica lo que el servidor confirma
    como si predice sus propios comandos. Como Aplicar es pura, deshacer una predicción
    rechazada es volver a aplicar los comandos pendientes sobre el último estado
    confirmado, nunca Resincronizar. Para eso el estado expone `numeroTurno`,
    `contadorComandos` y `contadorTirada` (regla 11).
14. **Resincronizar(estado, estadoAutoridad, motivo).** Corrige un cliente con el estado
    que manda el servidor. El motivo es **Reconexion** (el cliente vuelve después de
    perder la conexión) o **Divergencia** (las huellas no coincidieron). Valida en este
    orden:
    1. Modo local → `SoloOnline`.
    2. Otra versión o huella del Catálogo → `VersionNoSoportada`.
    3. El estado no coincide con su propia huella → `EstadoInvalido`.
    4. Otro id de partida → `OtraPartida`.
    5. El estado local ya está Terminada → `PartidaTerminada`.
    6. El `contadorComandos` del servidor es menor que el del estado local confirmado →
       `EstadoViejo` (un estado viejo, aunque sea válido, nunca hace retroceder la
       partida).

    Hacia un estado Terminado sí se puede: el final del servidor manda. Si es válido, el
    resultado es el estado del servidor tal cual (también su pila de deshacer y su tirada
    pendiente) y un evento `Resincronizado` con el motivo, la huella local vieja y la nueva.
    Con Divergencia, la interfaz avisa al jugador que el tablero se corrigió y Métricas
    (#19) lo registra como error S1. Con Reconexion no hay aviso ni S1. Una carrera
    esperable, como un comando del jugador que llega después del `TurnTimeout`, nunca
    termina en Resincronizar: el servidor lo rechaza con su código (`NoEsTuTurno`,
    `EsperandoTirada`, `TimeoutObsoleto`…) y el cliente vuelve a su último estado
    confirmado.
15. **Resolución simultánea.** Resolver calcula todo el daño sobre el estado de **antes**
    de resolver: una pieza que muere en esa resolución igual golpea, y el orden por ID solo
    fija el orden de los eventos, nunca el resultado. El daño sobre un mismo objetivo se
    suma (bruto) y el escudo se resta una sola vez (la fórmula es del GDD de Resolución,
    #9).
16. **Fin de partida.** Después de cada resolución, `EvaluateMatchEnd` decide si hay
    ganador. Si muere un solo Rey, gana el otro jugador (motivo **ReyMuerto**). **Si mueren
    los dos Reyes en la misma resolución, gana el jugador del turno**, el que resolvió, sea
    con Resolver o porque `TurnTimeout` disparó la resolución de los ataques asignados
    (motivo **DobleRey**; regla de la v3, provisional hasta el GDD de la Máquina de turno,
    #3, que es su dueño). `Rendirse` termina la partida con motivo **Rendicion**. El estado
    reserva desde la versión 1 los motivos **CierrePorTiempo** y **Tablas**, y un ganador
    vacío para las tablas, aunque ninguna regla los use todavía: la política del tope de
    25 minutos (pilar 4) es de la Máquina de turno (#3) y del Timer (#12), y usarla no va a
    cambiar el formato del estado.
17. **Qué hace `TurnTimeout`** (provisional hasta la Máquina de turno, #3).
    - **Etapa mover:** si hay un Reclutar obligatorio pendiente, la librería recluta el
      primer tipo disponible en el orden del Catálogo (decisión del 2026-11-19: el jugador
      no pierde el recurso). Después cierra la pila de deshacer y pasa a atacar, igual que
      TerminarMovimiento.
    - **Etapa atacar:** resuelve los ataques asignados, como Resolver. Las coberturas ya
      asignadas siguen valiendo. Sin ataques, el turno termina con `TurnoTerminado`.
    - **Fin automático del turno:** Aplicar nunca adelanta la fase por su cuenta mientras
      el jugador pueda usar Deshacer. Una fase sin ninguna acción posible al empezar se
      salta (por ejemplo, la de mover con Distracción).

### States and Transitions

| Desde | Hacia | Cuándo |
|---|---|---|
| (ninguno) | En curso | CrearPartida con config válida y castillos fijos: piezas y muros colocados |
| (ninguno) | En curso · esperando tirada | CrearPartida con config válida y castillos sorteados; la Tirada coloca castillos, piezas y muros |
| (ninguno) | (sin partida) | Config inválida: `ConfigInvalida` o `ProbabilidadesInvalidas` |
| En curso | En curso · esperando tirada | Empieza un turno que necesita tirada (el momento lo define la Máquina de turno, #3); nunca dentro de Resolver |
| En curso · esperando tirada | En curso | Tirada(n) válida de la autoridad. Cualquier otro comando se rechaza: `SoloAutoridad` (Tirada o `TurnTimeout` de un jugador) o `EsperandoTirada` (el resto, `TurnTimeout` de la autoridad incluido), salvo `Rendirse` |
| En curso (también esperando tirada) | Terminada (ganador, motivo) | Después de resolver, `EvaluateMatchEnd` encuentra un ganador (regla 16: ReyMuerto o DobleRey), o un jugador envía `Rendirse` (Rendicion). CierrePorTiempo y Tablas quedan reservados para la Máquina de turno |
| En curso o esperando tirada (online) | El estado del servidor, también Terminada | Resincronizar válido (regla 14) |
| Terminada | Terminada | Todo comando y todo Resincronizar se rechazan con `PartidaTerminada`; es un estado final |

Las fases del turno (tirada, reclutar, mover, atacar, resolver) son de la Máquina de turno
(#3), que las define dentro de este contrato. El reloj del turno no corre mientras se
espera una tirada: la autoridad la envía en cuanto se pide.

### Interactions with Other Systems

| Sistema | Le da a la librería | Recibe de la librería | Dueño de la interfaz |
|---|---|---|---|
| 2 Catálogo | Tabla de piezas (archivo de datos) | Se fija en la config con su huella | Catálogo |
| 3 Máquina de turno | Pipeline de fases, pila de deshacer, `EvaluateMatchEnd` | Corre dentro del contrato | Librería (contrato) y #3 (fases) |
| 4–11 Reglas del núcleo | Sus comandos, códigos y eventos | Respetan las reglas 1–17 | Cada GDD |
| 12 Timer | Comando `TurnTimeout(turno, etapa)` | `numeroTurno` y etapa actuales | Timer |
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
otros GDD. Dos de sus fórmulas convierten una tirada inyectada (regla 6) en un resultado; la
tercera arma la tabla efectiva del Modificador. No tienen ciclos de retroalimentación: la
librería no contiene balance.

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
devuelve `ProbabilidadesInvalidas` (no se corrige en silencio). La tabla efectiva que arma
`reparto_liberado` se valida al entrar en "esperando tirada", nunca al llegar la tirada, así
que una partida nunca queda esperando una tirada imposible (una tabla que sumara 99 dejaría
n = 99 sin cara). Un n fuera de 0–99 se rechaza con `TiradaInvalida`.
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

The `reparto_liberado` formula is defined as:

`L = Σ p_i de las caras de reclutar que no pueden salir` · esas caras pasan a 0 ·
`p_Grito' = p_Grito + L / 2` (división entera hacia abajo) · `p_Arremetida' = p_Arremetida + (L − L / 2)`

**Variables:**
| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| p_i | int | 0–100 | tabla base elegida (Modificador, #11) | Probabilidad de cada cara antes del reparto |
| L | int | 0–100 | calculado | Suma de las caras de reclutar liberadas: por muro roto, las dos; por llamadas agotadas, la que se agotó |
| p_Grito', p_Arremetida' | int | 0–100 | calculado | Caras que reciben lo liberado; cuáles son lo dice el papel de cada cara en la config (regla 2) |

**Output Range:** una tabla de las mismas k caras que suma exactamente `tirada_rango`, porque
`L / 2 + (L − L / 2) = L`. Se reparte **una sola vez sobre el total liberado**, no cara por
cara (decisión del 2026-11-19: un solo redondeo, y el punto impar siempre va a Arremetida).
Con L = 0 la tabla no cambia. Si la base tiene Grito y Arremetida en 0, igual reciben L y la
suma sigue en 100. La tabla resultante se valida como en CrearPartida (regla 6).
**Example:** tabla "Pierde con 1/2" 15/20/25/25/15 (Distracción, Grito, Llamada, Cuerno,
Arremetida) con un muro roto: L = 25 + 25 = 50 → Grito 20 + 25 = 45, Arremetida
15 + 25 = 40 → 15/45/0/0/40. Tabla "Gana 4 a 3" 20/25/15/20/20 con Llamada agotada: L = 15 →
Grito 25 + 7 = 32, Arremetida 20 + 8 = 28 → 20/32/0/20/28. Las dos suman 100.

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
  primero y el registro guarda ese orden, así que la repetición da lo mismo. Si entra
  primero Resolver, el turno pasa y se espera la tirada del rival: el `TurnTimeout` viejo se
  rechaza con `EsperandoTirada` y, si llega después de esa tirada, con `TimeoutObsoleto`
  (su turno ya no es el actual). Si entra primero el `TurnTimeout`, el Resolver tardío se
  rechaza con `EsperandoTirada` o, después de la tirada, con `NoEsTuTurno`. Si la
  resolución terminó la partida, los dos con `PartidaTerminada`.
- **If the move-stage `TurnTimeout` arrives just after TerminarMovimiento**: la etapa ya es
  atacar, así que se rechaza con `TimeoutObsoleto` y no resuelve los ataques que el
  jugador todavía está asignando.
- **If `TurnTimeout` arrives while waiting for a roll**: `EsperandoTirada`. El reloj del
  turno no corre hasta que llega la tirada; si la autoridad no la envía, lo resuelve el
  Servidor (#28).
- **If the move-stage `TurnTimeout` arrives with a mandatory recruit pending**: la librería
  recluta el primer tipo disponible en el orden del Catálogo y pasa a atacar (regla 17).
- **If the attack-stage `TurnTimeout` arrives with no attacks assigned**: el turno termina
  con `TurnoTerminado`; las coberturas asignadas siguen valiendo.
- **If client and server end up with different state hashes**: manda el servidor. El
  cliente aplica Resincronizar con motivo Divergencia (regla 14), el jugador ve el aviso y
  la diferencia se reporta como error S1 (no debería pasar nunca).
- **If the server rejects a command the client had already predicted**: no es una
  divergencia. El cliente vuelve a su último estado confirmado y reaplica los comandos
  todavía pendientes; no hay aviso ni S1.
- **If a client reconnects**: Resincronizar con motivo Reconexion; sin aviso ni S1.
- **If the server state sent for Resincronizar does not match its own hash**:
  `EstadoInvalido`; el cliente conserva su estado y vuelve a pedirlo (reconexión del #28).
- **If Resincronizar receives an older state or one from another match**: `EstadoViejo` u
  `OtraPartida`; el cliente conserva su estado. Un estado viejo pero válido nunca hace
  retroceder la partida.
- **If a player sends `Rendirse` outside their turn or while waiting for a roll**: se
  acepta; gana el otro jugador con motivo Rendicion.

**Deshacer y vista previa**

- **If Deshacer is sent with an empty stack**: `NadaQueDeshacer`.
- **If Deshacer is sent after TerminarMovimiento**: `FaseIncorrecta`. La pila se cierra al
  pasar a atacar y se vacía al empezar cada turno: una tirada o un reclutamiento nunca se
  deshacen (provisional hasta la Máquina de turno, #3).
- **If a preview is requested while waiting for a roll**: `EsperandoTirada`. La vista
  previa nunca puede usar ni adelantar una tirada futura.
- **If a preview is requested with no attacks assigned**: devuelve lo mismo que Resolver,
  es decir el evento `TurnoTerminado`; no es un error.
- **If a preview is requested outside the attack phase or by the player without the
  turn**: el mismo código que daría Resolver (`FaseIncorrecta`, `NoEsTuTurno`).
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
- **If a wall breaks or a recruit face runs out**: la tabla efectiva pone esas caras en 0 y
  reparte su parte con `reparto_liberado`; siempre suma 100.
- **If an arithmetic operation overflows anyway** (un error de programación): Aplicar
  devuelve ErrorInterno, el estado recibido queda intacto y se reporta como S1; nunca se
  devuelve un valor que dio la vuelta.
- **If several pieces appear at once** (despliegue o reclutamiento de 4): los IDs siguen el
  orden fijo de la regla 7, igual en todas las máquinas.
- **If a piece takes more damage than its life**: la vida queda en 0 y el evento trae la
  vida realmente perdida.

**Versiones, datos y fin de partida**

- **If a match or replay from another rules version or Catalog hash is loaded**:
  `VersionNoSoportada`; no se reproduce nada a medias.
- **If a saved state does not match its own hash** (archivo dañado o editado):
  `EstadoInvalido`; la partida no se carga.
- **If both Kings die in the same resolution**: gana el jugador del turno, el que resolvió,
  con motivo DobleRey (regla 16, provisional hasta la Máquina de turno, #3). La librería
  garantiza que el resultado sea siempre el mismo, y la vista previa ya lo muestra.
- **If one of the k castle options is invalid**: CrearPartida devuelve `ConfigInvalida`
  aunque esa opción nunca llegue a salir.
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

**Dónde corren.** El código de la librería tiene un asmdef con `noEngineReferences`. Los
tests viven en `Assets/Tests/EditMode/LibreriaReglas/` con su propio asmdef, sin
referencias a Unity, y un `.csproj` compila esos mismos archivos (sin copiarlos) para que
corran también con `dotnet test`. Así los ve el runner de Unity (`game-ci/unity-test-runner`)
y corren en .NET en cada PR (decisión del 2026-11-19). La evidencia es el resultado de esos
tests. Solo la parte de AC-33 que mide dentro de Unity vive en
`Assets/Tests/PlayMode/LibreriaReglas/`.

**Tipos de prueba.** Son **Lógica** salvo estos:
- AC-6 en su parte web es **Integración**.
- AC-24, AC-25 y AC-33 son **Rendimiento**. Quedan ADVISORY hasta que el ADR de la
  librería nombre la máquina, el navegador y el build de referencia; desde entonces son
  BLOCKING en esa referencia.
- AC-7 corre como analizador en la compilación.
- AC-32 queda **DEFERRED** hasta el GDD del Servidor (#28).

Ninguno necesita captura.

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
- **AC-7 (prohibiciones)** — **GIVEN** el ensamblado de la librería con un analizador de
  APIs prohibidas que resuelve símbolos (por ejemplo `BannedApiAnalyzers` con un
  `BannedSymbols.txt`) configurado como error, **WHEN** se compila, **THEN** hay 0 usos de
  los símbolos de la lista, que el ADR mantiene y que incluye como mínimo:
  - los tipos de coma flotante;
  - `DateTime`, `DateTimeOffset`, `TimeSpan`, `Stopwatch` y `Environment`;
  - `System.Random` y `Guid`;
  - los miembros de coma flotante de `System.Math`;
  - `GetHashCode`, `HashCode` y `CultureInfo`;
  - `System.Reflection`, `System.Threading` (también `Task`) y `System.IO`;
  - `UnityEngine`;
  - la enumeración de `Dictionary` o `HashSet`.

  Además, el ensamblado no referencia a Unity, y un archivo de prueba con un uso de cada
  símbolo prohibido hace fallar la compilación.
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
  y la vista previa no cambió la huella. **GIVEN** 0 ataques asignados, **THEN** las dos
  devuelven la misma lista, con `TurnoTerminado`. **GIVEN** la fase de mover, **THEN** las
  dos devuelven `FaseIncorrecta`.
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
  **THEN** la vista previa pedida antes ya trae el evento de fin con ese jugador como ganador
  y motivo DobleRey, y la partida termina igual. **WHEN** en cambio la autoridad envía
  `TurnTimeout` en la etapa de atacar, **THEN** el ganador es el mismo jugador del turno.
  El test corre dos veces, con el jugador 0 y con el jugador 1 en el turno. **GIVEN** que
  muere un solo Rey, **THEN** gana el otro jugador con motivo ReyMuerto.

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
- **AC-25 (comando rápido)** — **GIVEN** ese mismo fixture, **WHEN** se aplica un Mover
  válido 100 veces, cada vez sobre una copia nueva del fixture (la copia no se mide), después
  de 20 de calentamiento, **THEN** las 100 devuelven Aceptado y el percentil 95 es de 1 ms o
  menos en la misma referencia. En el navegador se mide por lotes y se toma el promedio de
  cada lote.
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
- **AC-29 (orden de los rechazos)** — Test de tabla: **GIVEN**, para cada par de filas
  consecutivas de la regla 4 que se aplican al mismo tipo de emisor, un fixture que falla
  las dos, **WHEN** se envía el comando, **THEN** sale el código de la fila de número menor.
  Incluye como mínimo estos casos:
  - Un jugador envía Tirada(150) con la partida terminada: `PartidaTerminada`. Con la
    partida en curso: `SoloAutoridad`.
  - La autoridad envía Tirada(150) sin tirada pendiente: `TiradaNoEsperada`. Con tirada
    pendiente: `TiradaInvalida`.
  - El jugador que no tiene el turno envía Mover mientras se espera tirada:
    `EsperandoTirada`.
  - Un comando de tipo desconocido con la partida terminada: `PartidaTerminada`.
  - El jugador sin turno envía Mover con un ID inexistente: `NoEsTuTurno`.
  - El jugador del turno mueve una pieza del rival en la fase equivocada: `NoEsTuPieza`.
- **AC-30 (huella)** — **GIVEN** dos secuencias de comandos que mueven las mismas piezas a
  las mismas casillas en distinto orden, **WHEN** llegan al comienzo del turno siguiente,
  **THEN** las dos huellas son iguales. **GIVEN** un estado, **WHEN** se cambia un solo
  campo (un test por cada campo de la regla 11), **THEN** la huella cambia. **GIVEN** 3
  estados de fixture con su huella guardada en el repositorio, **WHEN** se calcula la huella
  en .NET y en el build web, **THEN** coincide con la guardada.
- **AC-31 (QuitarAtaque y Cubrir)** (PROVISIONAL hasta la Máquina de turno, #3) —
  **GIVEN** la fase de movimiento y una pieza viva del jugador del turno (un escudero para
  Cubrir, con un aliado vecino), **WHEN** ese jugador envía QuitarAtaque o Cubrir,
  **THEN** se rechazan con `FaseIncorrecta` (no con `ComandoDesconocido`).
- **AC-32 (emisor sellado)** (DEFERRED hasta el Servidor, #28) — **GIVEN** una partida
  online en el servidor, **WHEN** por la conexión del jugador 1 llega una Tirada cuyo
  mensaje dice venir de la autoridad, **THEN** el servidor la aplica con emisor jugador 1 y
  se rechaza con `SoloAutoridad`. Lo que sí se prueba en la librería es AC-39.
- **AC-33 (vista previa sin memoria)** — **GIVEN** el fixture de AC-24 y 10 vistas previas
  de calentamiento, **WHEN** se piden 100 vistas previas más, **THEN** en .NET la diferencia
  de `GC.GetAllocatedBytesForCurrentThread()` es 0, y en un build de desarrollo de Unity un
  test de PlayMode con `ProfilerRecorder` cuenta 0 reservas de memoria del GC en esas 100.
- **AC-34 (Resolver no consume tiradas)** (PROVISIONAL hasta la Máquina de turno, #3) —
  **GIVEN** ataques asignados en un turno cuyo
  siguiente turno necesita tirada, **WHEN** se envía Resolver, **THEN** los eventos de daño
  son iguales a los de la vista previa, el estado resultante queda esperando tirada y
  ningún evento de esa resolución trae una tirada.
- **AC-35 (timeout esperando tirada)** — **GIVEN** que se espera una tirada, **WHEN** la
  autoridad envía `TurnTimeout`, **THEN** se rechaza con `EsperandoTirada` y la huella no
  cambia.
- **AC-36 (sorteo de castillos)** (PROVISIONAL hasta el Tablero, #4) — **GIVEN** una config
  con castillos sorteados entre 3 pares, **WHEN** se llama CrearPartida, **THEN** la
  partida queda esperando tirada con 0 piezas; un Mover se rechaza con `EsperandoTirada`;
  **WHEN** la autoridad envía Tirada(50), **THEN** se colocan los castillos del par 1 con
  sus piezas y muros y la partida queda En curso. Con Tirada(0) sale el par 0 y con
  Tirada(99), el par 2.

**Contrato agregado en la segunda revisión del 2026-11-19**

- **AC-37 (timeout viejo)** — **GIVEN** la etapa de mover del turno T, **WHEN** el jugador
  envía TerminarMovimiento y después la autoridad envía `TurnTimeout(T, mover)`, **THEN**
  se rechaza con `TimeoutObsoleto`, la huella no cambia y los ataques no se resuelven.
  **GIVEN** que se aceptó Resolver en el turno T, **WHEN** llega `TurnTimeout(T, atacar)`,
  **THEN** se rechaza con `EsperandoTirada`; después de la tirada del turno T + 1, con
  `TimeoutObsoleto`. **GIVEN** que se aceptó `TurnTimeout(T, atacar)`, **WHEN** el jugador
  envía Resolver, **THEN** `EsperandoTirada`; después de la tirada, `NoEsTuTurno`.
  Reproducir el registro de la autoridad en cualquiera de los dos órdenes da las mismas
  huellas que en la partida original.
- **AC-38 (qué hace el timeout)** (PROVISIONAL hasta la Máquina de turno, #3) — **GIVEN** un
  Reclutar obligatorio pendiente, **WHEN** llega `TurnTimeout(T, mover)`, **THEN** se recluta
  el primer tipo disponible en el orden del Catálogo y la fase pasa a atacar. **GIVEN** la
  etapa de atacar con 0 ataques y una cobertura asignada, **WHEN** llega
  `TurnTimeout(T, atacar)`, **THEN** el turno termina con `TurnoTerminado` y la cobertura
  sigue en el estado.
- **AC-39 (comandos de autoridad)** — **GIVEN** una tirada pendiente, **WHEN** la autoridad
  envía Tirada(50), **THEN** se acepta (nunca `NoEsTuTurno`). **WHEN** la autoridad envía
  Mover, **THEN** `SoloJugador`. **GIVEN** la API pública, **THEN** el emisor es un
  parámetro de Aplicar y no hay ningún tipo de comando con un campo de emisor (test de
  forma de la API).
- **AC-40 (tabla efectiva)** — **GIVEN** la tabla 15/20/25/25/15 y un muro roto, **WHEN** se
  arma la tabla efectiva, **THEN** es 15/45/0/0/40. **GIVEN** 20/25/15/20/20 con Llamada
  agotada, **THEN** 20/32/0/20/28. **GIVEN** cada tabla base del Modificador y cada
  combinación de caras de reclutar liberadas, **THEN** la tabla efectiva suma 100 y n = 99
  siempre da una cara. **GIVEN** una base con Grito y Arremetida en 0, **THEN** igual suma
  100.
- **AC-41 (orden de los IDs)** — **GIVEN** un despliegue inicial con castillos fijos y otro
  con castillos sorteados, **WHEN** se colocan las piezas, **THEN** los IDs coinciden con un
  archivo dorado guardado en el repositorio (jugador 0 primero, después tipo del Catálogo,
  después fila y columna). **GIVEN** un reclutamiento de 4 piezas, **THEN** sus IDs crecen
  por casilla (fila, columna).
- **AC-42 (resolución simultánea)** (valores PROVISIONALES hasta la Resolución, #9) —
  **GIVEN** que A (ID 7) y B (ID 3) se matan entre sí y T tiene 3 de vida y recibe 5 de
  daño neto, **WHEN** se envía Resolver, **THEN** A y B igual golpean, los eventos salen en
  orden de ID ascendente (B antes que A), la vida de T queda en 0 y su evento trae 3 de vida
  perdida y la muerte.
- **AC-43 (desbordamiento)** — **GIVEN** las operaciones aritméticas verificadas de la
  librería, **WHEN** reciben `int.MaxValue + 1` o `int.MaxValue × 2`, **THEN** el resultado
  es ErrorInterno en .NET, en el editor de Unity y en el build web, y Aplicar nunca devuelve
  un valor que dio la vuelta.
- **AC-44 (Resincronizar, rechazos)** — **WHEN** se aplica Resincronizar:
  - en modo local: `SoloOnline`;
  - con otra versión: `VersionNoSoportada`;
  - con otro id de partida: `OtraPartida`;
  - con un `contadorComandos` menor que el local: `EstadoViejo`;
  - desde un estado local Terminada: `PartidaTerminada`.

  En todos esos casos el estado del cliente no cambia. **WHEN** el estado del servidor
  está Terminada y el del cliente no, **THEN** se acepta. **WHEN** el motivo es
  Reconexion, **THEN** el evento `Resincronizado` lo trae y no se marca como S1.
- **AC-45 (huella exhaustiva)** — **GIVEN** la lista de campos del estado (obtenida en el
  test, no en la librería), **WHEN** corre el test de exhaustividad, **THEN** falla si un
  campo no cambia la huella al cambiarlo. **GIVEN** dos estados que solo difieren en cómo
  se reparten los elementos entre dos colecciones consecutivas (por ejemplo, una pieza
  menos y más tramos de muro), **THEN** sus flujos canónicos y sus huellas son distintos.
- **AC-46 (k opciones de castillo)** — **GIVEN** una config con castillos sorteados en la
  que solo la opción 2 de 3 se sale del tablero, **WHEN** se llama CrearPartida, **THEN**
  `ConfigInvalida` y no se crea partida.
- **AC-47 (Rendirse)** — **GIVEN** una partida en curso, **WHEN** el jugador sin turno envía
  `Rendirse`, también mientras se espera una tirada, **THEN** la partida termina, gana el
  otro jugador y el motivo es Rendicion. **WHEN** la autoridad envía `Rendirse`, **THEN**
  `SoloJugador`.
- **AC-48 (combinado, fuego amigo)** — **GIVEN** un fixture de resolución, **WHEN** una
  catapulta propia y un enemigo golpean a la misma pieza, **THEN** su evento de daño no es
  combinado. **WHEN** un mismo atacante golpea dos veces el mismo objetivo, **THEN** no es
  combinado. **WHEN** dos enemigos atacan a un aliado cubierto por un escudero, **THEN** el
  daño lo recibe el escudero y su evento es combinado.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| Estructura del código, target del ensamblado (`netstandard2.1`, C# 9 fijado también en el `.csproj`), orden exacto de los campos de la forma canónica, algoritmo de la huella (sin depender de `System.Security` en WebGL) y su costo, cómo se escribe la aritmética verificada, firma de Aplicar con destino y buffers, lista de `BannedSymbols.txt` | ADR de la librería (`/architecture-decision`) | Antes de escribir código del MVP |
| Cómo se conservan las versiones viejas de la librería para reproducir repeticiones | ADR y Registro de partida (#26) | Antes de la Fase 2b |
| Máquina, navegador y build (release o development) de referencia para AC-24 y AC-25 | ADR de la librería | Antes de la primera prueba de rendimiento |
| Compromiso verificable de las tiradas online (semilla comprometida al crear la partida) | Servidor de partida (#28) | Antes de lanzar el online |
| Números de secuencia, reconexión, modelo de red del cliente (solo confirmado o con predicción) y qué pasa si la autoridad no envía la tirada | Servidor de partida (#28) y Timer (#12) | Antes de lanzar el online |
| Política del tope de 25 minutos (pilar 4): cuándo se usa CierrePorTiempo o Tablas y quién gana | Máquina de turno (#3) y Timer (#12) | En sus GDD |
| Semántica definitiva de `TurnTimeout` (regla 17: recluta automático, fin automático del turno) | Máquina de turno (#3) | En su GDD |
| Regla definitiva cuando mueren los dos Reyes (hoy: gana el jugador del turno) | Máquina de turno (#3) | En su GDD |
| ¿El reclutamiento y la asignación de ataques entran en la pila de deshacer? (en la v4 solo entran los movimientos) | Máquina de turno (#3) | En su GDD |
