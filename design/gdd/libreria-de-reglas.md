# Librería de reglas (contrato)

> **Status**: Approved
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-27 (décima pasada de /design-review: APPROVED tras revisión en la sesión; `ConfigInvalida` con código F1–F9 / T4–T6 y orden de CrearPartida, AC-56 y AC-21 con Resincronizar online)
> **Last Verified**: 2026-11-27
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
asignar un ataque, elegir un recluta, resolver, `TurnTimeout`, cerrar la partida) y devuelve el estado
siguiente, o un código de rechazo que dice por qué el comando no es válido. No dibuja, no
mide el tiempo, no guarda archivos y no conoce a Unity: el mismo código corre en el cliente
local, en el servidor online, en los tests y en el reproductor de repeticiones.

Su promesa central es el **determinismo**: el mismo estado inicial y la misma secuencia de
comandos dan siempre el mismo resultado, en cualquier máquina. Para cumplirla usa solo
números enteros, resuelve todo en un orden fijo por ID de pieza y no toma azar de ningún
lado: el azar del juego (la tirada del Modificador) entra como un dato registrado que
envía la autoridad.

Los demás sistemas del núcleo (Máquina de turno, Tablero, Castillo, Movimiento, Ataques,
Cobertura, Resolución, Reclutamiento, Modificador y Registro de partida) son reglas escritas
dentro de este contrato. Este GDD fija el contrato, no cada regla.

## Player Fantasy

**"Lo que ves es lo que pasa."** La vista previa marca MUERE sobre el Rey rival; apretás
Resolver a tiempo (con el reloj de partida agotado no se resuelve nada; regla 3) y cae
exactamente eso, ni un punto más ni uno menos. El jugador nunca ve a la
librería, pero siente su promesa en cada resolución: el tablero cumple lo que mostró. Es la
base del pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus
frutos"*, y de su prueba de diseño, *"El daño es fijo y se ve antes de resolver"*.

De esa promesa salen otras dos sensaciones:

- **"El tablero no hace trampa."** Después de perder una partida online pareja, la
  repetición reproduce cada movimiento y cada tirada tal como pasaron, igual en las dos
  pantallas: se perdió por una jugada, no por el sistema (anti-pilar *"NOT información
  oculta al resolver"*; pilar 3b, probabilidades consultables). **No promete que la
  autoridad elija las tiradas, el momento de los timeouts ni el ganador de
  un abandono de forma justa:** en el MVP la autoridad es de confianza (regla 6), y un
  compromiso verificable de las tiradas, si hace falta, lo agrega el Servidor de partida
  (#28) sin cambiar Aplicar.
- **"Pensé mejor que el otro y el tablero lo muestra"** (fantasía central del concepto).
  Durante el turno el jugador prueba órdenes de movimiento con el deshacer y cada vez el
  tablero responde igual y al instante; si un movimiento no vale, el juego dice por qué.

**Qué le exige al resto del diseño:**

- La vista previa y la resolución usan la misma función de la librería; una diferencia
  entre las dos es un error de severidad máxima (S1).
- Las repeticiones son exactas y la tirada del Modificador queda registrada.
- Deshacer y vista previa no tienen demora perceptible.
- Ninguna acción del jugador se rechaza ni se descarta sin un aviso que la interfaz pueda
  mostrar (los códigos internos de la regla 4 se muestran como un aviso neutro).

**Lo que el jugador nunca debería sentir:** "el juego me mostró una cosa e hizo otra", "en
la pantalla del otro pasó distinto", "perdí por algo que no se veía".

## Detailed Design

### Core Rules

1. **Dos puertas, ninguna más.** Una partida solo cambia con **Aplicar(contexto, estado,
   comando)** o con **Resincronizar** (regla 14). El **contexto** es el Catálogo ya
   validado con su huella, que arma **PrepararContexto(catálogo)**: valida el contenido y
   **calcula** su huella, nunca la toma de afuera. CrearPartida lo usa por dentro y lo
   devuelve; **Cargar(contexto, flujo)**, Aplicar, las consultas (regla 10) y
   Resincronizar lo reciben y nunca leen datos del Catálogo de otro lado. Cargar crea un
   estado, no cambia una partida, y valida en este orden: `VersionNoSoportada` (versión, o
   huella del Catálogo distinta de la del contexto); `EstadoInvalido` si los 8 bytes de
   huella con que termina el flujo guardado no son la huella recalculada, o si el estado no
   cumple la tabla de dominios (regla 11). Si el comando es válido,
   Aplicar devuelve **Aceptado** con el estado nuevo y una lista de eventos; si no, devuelve
   **Rechazado** con un código de motivo y el estado queda igual. Hay un tercer resultado,
   **ErrorInterno**, solo para un error de programación (regla 5): el estado recibido queda
   intacto y el error se reporta como S1. Las dos son funciones puras: no modifican el
   estado que reciben y no tocan reloj, archivos, red, registros de consola ni Unity (pueden
   escribir en un destino ya reservado, nunca el mismo objeto que el origen; firma en el ADR).
2. **Creación.** Una partida nace con **CrearPartida(config)**. La config incluye: la
   versión de reglas; el **id de partida** (un entero que pone quien crea la partida); el
   **modo** (local u online); la tabla del Catálogo (su contenido; la huella, un resumen
   que identifica esa tabla exacta, la calcula PrepararContexto); las tablas de probabilidad del Modificador, con el papel
   de cada cara (reclutar, Grito de guerra, Arremetida u otra); la posición de los
   castillos, fija (la define el Tablero, #4); el **límite normal** de piezas que se mueven
   por turno, sin el Rey (`limite_normal`, 1 a 37; hoy 12, su dueño es Movimiento, #6); la **vida del tramo** de muro (`vida_tramo`, 1 a 1.000; hoy 30, su dueño es Castillo y muros, #5); la **tabla de reclutas** (`tablaReclutas`: por tipo, su cara, piezas por llamada y llamadas; dueño Reclutamiento, #10); los **bonos del Grito de guerra** (`bono_grito_ataque` y `bono_grito_movimiento`, 0 a 10; hoy 2 y 2; dueño Modificador, #11); y el modo de timer, solo como dato
   informativo que no entra al estado (la librería no mide tiempo y ninguna regla depende
   de él). CrearPartida valida **toda** la config antes
   de crear nada: Catálogo no vacío y con el formato del Catálogo (#2); cada tabla de
   probabilidad según *Formulas* (`ProbabilidadesInvalidas`); los **papeles de las caras**
   de cada tabla (`ProbabilidadesInvalidas`): un papel por cara, exactamente una cara de
   Grito de guerra y una de Arremetida, distintas entre sí y distintas de las caras de
   reclutar; `limite_normal` entre 1 y 37; `vida_tramo` entre 1 y 1.000; `tablaReclutas` según Reclutamiento (#10, *Config*); los bonos del Grito entre 0 y 10 y no los dos en 0; la cota de la pila de deshacer con el bono de movimiento (Movimiento, F5) no mayor que 256; con dos caras de reclutar como mucho en las tablas base y cada una con al menos un tipo; y los castillos dentro del tablero, sin
   superponerse y en el par que fija el Tablero (#4, regla 4). **Orden** (jugador,
   2026-11-27): primero las tablas y sus papeles (`ProbabilidadesInvalidas`, gana sobre
   todo lo demás); después `PrepararContexto` (F1 → F8 del Catálogo, #2, regla 6, la
   primera que falla); después los bonos del Grito (0 a 10 y no los dos en 0; antes de F9, que los suma); después F9 del Catálogo (`movimiento_t + bono_grito_movimiento ≤` 12); después los del Tablero (#4, regla 6) **T4** (castillos) y **T5** (`vida_tramo`); después `limite_normal`, `tablaReclutas` con las caras de reclutar y la pila con bono (`cota_grito`, Máquina D4; código propuesto `PilaGrito`); y al final **T6** (ejército inicial). `ConfigInvalida` **lleva el código de lo que falló**: `F1` a `F9`, `T4` a `T6` o el del campo (sus nombres, en el ADR). CrearPartida recibe el Catálogo o un
   contexto ya preparado; con un contexto no repite F1 a F8, pero sí F9. En los dos casos
   de error no se crea partida; si no, devuelve el estado y el contexto (regla 1). La partida
   siempre nace **En curso**, con castillos, piezas y muros ya colocados (decisión del
   2026-11-25: no hay sorteo de castillos). **El primer turno de la partida no tira el
   Modificador** (Máquina de turno, regla 3): después de crear la
   partida empieza directo en mover. La versión, el id de partida, la huella del
   Catálogo, el modo y los campos de esta lista no cambian en toda la partida.
3. **Comandos: una lista cerrada.** Mover(pieza, destino) · Deshacer · TerminarMovimiento ·
   AsignarAtaque(pieza, objetivo) · QuitarAtaque(pieza) · Cubrir(escudero, aliado) · QuitarCobertura(escudero) ·
   Reclutar(tipo) · Resolver · Rendirse · TurnTimeout(turno, etapa) ·
   Tirada(n 0–99, contadorTiradaEsperado) · CerrarPartida(motivo, ganador, turno).
   Las reglas de cada uno las define su GDD (#3 a #11); agregar un comando es un cambio de
   contrato y sube la versión de reglas. Cada comando lleva su **emisor**: jugador 0,
   jugador 1 o la **autoridad**. **El emisor es un parámetro de Aplicar que pone quien
   llama, nunca un campo del mensaje:** en modo local, el cliente; en modo online, el
   servidor, que lo deduce de la conexión autenticada y descarta cualquier emisor que
   traiga el mensaje. **Comandos de autoridad:** Tirada, `TurnTimeout` y `CerrarPartida`,
   y la autoridad no envía ningún otro. **Comandos de jugador:** todos los demás. En modo
   online el cliente nunca actúa como autoridad: los comandos de autoridad solo los aplica
   tal como se los reenvía el servidor.
   - **`contadorEsperado`.** Todo comando de jugador, salvo `Rendirse`, lleva el
     `contadorComandos` (regla 11) del estado sobre el que se decidió. Si no coincide con
     el del estado actual, se rechaza con `ComandoViejo`. `contadorComandos` cuenta **todo
     comando aceptado, de jugador o de autoridad** (decisión del 2026-11-19: el freno a
     duplicados y comandos tardíos vive en la librería, no solo en el transporte).
   - **Tirada lleva `contadorTiradaEsperado`**, el `contadorTirada` (regla 6) para el que
     se emitió. Si no coincide, se rechaza con `TiradaVieja`.
   - `TurnTimeout(turno, etapa)` nombra el turno (`numeroTurno`, regla 11) y la etapa del
     timer (mover o atacar) que vencieron. Un timeout que no coincide con el turno y la
     etapa actuales es viejo y se rechaza con `TimeoutObsoleto`, sin efecto.
   - `Rendirse` lo puede enviar cualquiera de los dos jugadores en cualquier momento de una
     partida en curso, también fuera de su turno y mientras se espera una tirada: termina
     la partida, gana el otro jugador y el motivo es Rendición. La interfaz lo ofrece solo
     cuando `RendirseDisponible` (regla 10) es verdadero (jugador, 2026-11-24); Aplicar no
     lo exige. El online no se lanza sin él.
   - `CerrarPartida(motivo, ganador, turno)` lo envía solo la autoridad, en cualquier momento de
     una partida en curso, también mientras se espera una tirada. Cierra la partida cuando
     la decisión no sale del tablero: un jugador que abandona (motivo **Abandono**), el
     reloj de partida agotado (**RelojAgotado**, Máquina de turno, regla 14) o un cierre
     técnico sin ganador (**Tablas**; en el MVP no hay oferta de empate, decisión del
     2026-11-27). La forma se valida: Abandono lleva un ganador (0 o 1), que dice
     la autoridad de confianza; RelojAgotado lleva como ganador al rival del jugador del
     turno y como `turno` el `numeroTurno` en que venció, y no vale mientras se espera una
     tirada, porque el reloj no corre ahí; Tablas lleva el ganador vacío; Abandono y Tablas
     llevan `turno` vacío; cualquier otro motivo o combinación se rechaza con
     `CierreInvalido`. Así, un RelojAgotado que llega con el turno ya cerrado se rechaza,
     también si el mismo jugador vuelve a tener el turno. Lo ya movido queda y los ataques
     asignados no se resuelven.
     Cuándo hay abandono y cuándo se agota el reloj lo deciden el Timer (#12) y el Servidor
     (#28): la librería no mide tiempo.
4. **Validación y códigos.** Todo comando se valida entero antes de cambiar nada: no hay
   comandos aplicados a medias. **Todo campo de un comando (casilla, ID, tipo, n, turno,
   contador) se valida por rango antes de entrar en cualquier cuenta:** un valor fuera de
   rango es un Rechazado con su código, nunca un ErrorInterno, sea cual sea el int32 que
   traiga. Los rechazos son **códigos**, nunca texto; la presentación
   los traduce con Localización (#14). La validación sigue **siempre este orden** y devuelve
   el primer código que falla. Las filas 6 a 9 solo se aplican a comandos de autoridad y las
   filas 10 a 14, solo a comandos de jugador. `Rendirse` válido pasa la fila 4 y se acepta
   (se salta las filas 5 y 10 a 15); `CerrarPartida` se salta las filas 5 y 15:

   | Orden | Código | Cuándo | Para el jugador |
   |---|---|---|---|
   | 1 | `VersionNoSoportada` | Esta librería no soporta la versión de reglas o la huella del Catálogo del estado, o la huella del contexto no es la del estado (los comandos no llevan versión: la lleva el estado) | Sí ("hay que actualizar") |
   | 2 | `PartidaTerminada` | La partida ya terminó | Sí |
   | 3 | `ComandoDesconocido` | El tipo de comando no está en la lista cerrada | Interno |
   | 4 | `EmisorInvalido` · `SoloAutoridad` · `SoloJugador` | El emisor no es 0, 1 ni la autoridad · un jugador envía un comando de autoridad · la autoridad envía un comando de jugador | Interno |
   | 5 | `EsperandoTirada` | Se espera una tirada y llega cualquier comando que no sea Tirada, `Rendirse` ni `CerrarPartida` (también `TurnTimeout`) | Sí ("esperando la tirada"); interno con `TurnTimeout` |
   | 6 | `TiradaNoEsperada` | Llega una Tirada y no se espera ninguna | Interno |
   | 7 | `TiradaVieja` · `TiradaInvalida` | Su `contadorTiradaEsperado` no es el `contadorTirada` actual · n fuera de 0–99 (se revisa primero el contador) | Interno |
   | 8 | `TimeoutObsoleto` | `TurnTimeout` con un turno o una etapa que no son los actuales | Interno (se descarta sin aviso) |
   | 9 | `CierreInvalido` | `CerrarPartida` con un motivo que no es Abandono, RelojAgotado ni Tablas, con un ganador que no corresponde al motivo (Abandono sin ganador 0 o 1; RelojAgotado con un ganador que no es el rival del jugador del turno o un `turno` que no es `numeroTurno`; Tablas con ganador; Abandono o Tablas con `turno`), o RelojAgotado esperando una tirada | Interno |
   | 10 | `ComandoViejo` | El `contadorEsperado` del comando no es el `contadorComandos` actual | Interno (el cliente vuelve a su último estado confirmado) |
   | 11 | `NoEsTuTurno` | El emisor no es el jugador del turno | Sí |
   | 12 | `PiezaInexistente` · `NoEsTuPieza` | El ID de la pieza que actúa no existe (o la pieza murió) · es del rival. El objetivo de AsignarAtaque y el aliado de Cubrir los valida la regla de cada uno (fila 14) | Sí |
   | 13 | `FaseIncorrecta` | El comando no corresponde a la fase actual | Sí |
   | 14 | Códigos de cada regla del núcleo | Movimiento (#6, regla 13): `DestinoFueraDelTablero`, `DestinoIgualOrigen`, `PiezaAgotada`, `SinMovimientos`, `DestinoOcupado`, `DestinoBloqueado`, `FueraDeAlcance`. Alcance (#7, regla 12): `ObjetivoFueraDelTablero`, `ObjetivoInvalido`, `FueraDeAlcance`, `ObjetivoPropio`, `SinObjetivo`, `SinAtaqueAsignado`. Reclutamiento (#10, regla 7): `TipoNoReclutable`, `CastilloSinReclutar`, `SinLlamadas`, `TopeDePiezas`, `SinLugarParaReclutar`. Cobertura (#8, regla 14): `NoEsEscudero`, `AliadoInexistente`, `AliadoNoEsTuyo`, `AliadoEsElMismo`, `AliadoLejos`, `EscuderoCubierto`, `AliadoYaCubre`, `AliadoYaCubierto`, `SinCobertura`. Máquina (#3): `NadaQueDeshacer`. Resolución (#9) no tiene códigos. Cada GDD define el orden de los suyos y si los ve el jugador | Según su GDD |
   | 15 | `LimitePartida` | Lista cerrada, solo defensas (decisión del 2026-11-19; los datos válidos no las alcanzan): un Mover con la pila de deshacer en 256; un Reclutar o un `TurnTimeout` con recluta automático que llevaría el próximo ID por encima de C_MAX. El tope de `contadorComandos` no rechaza: termina la partida (regla 16) | Interno (aviso neutro) |

   Fuera de Aplicar hay más códigos: `ConfigInvalida` con su código (CrearPartida y
   PrepararContexto, regla 2) y `ProbabilidadesInvalidas` (CrearPartida); `EstadoInvalido` (cargar o Resincronizar un estado que no es válido,
   regla 11); `VersionNoSoportada` (cargar o Resincronizar con otra versión); y
   `SoloOnline`, `OtraPartida`, `EstadoViejo` y `PartidaTerminada` (Resincronizar, regla
   14). Los códigos internos indican un error del cliente o del servidor, o una carrera de
   red que el cliente corrige solo, y se reportan; si descartan una acción del jugador, la
   interfaz muestra un aviso neutro.
5. **Determinismo y enteros.**
   - Todos los valores son enteros de 32 bits con signo y la aritmética es **verificada**:
     un desbordamiento es un error de programación, nunca un valor que da la vuelta. Aplicar
     lo devuelve como **ErrorInterno** (regla 1), con el estado de entrada intacto (el
     mecanismo lo fija el ADR). La detección del desborde es **explícita y no depende de
     excepciones** (funciona igual con `exceptionSupport` reducido en WebGL). **Ninguna
     entrada válida llega a ese límite:** los campos de los comandos y el emisor se validan
     antes de usarse (regla 4); todo estado cargado o resincronizado está dentro de la tabla
     de dominios (regla 11); la matriz de clausura (regla 11) y la fila 15 garantizan que de
     un estado válido solo sale un estado válido; y PrepararContexto rechaza con
     `ConfigInvalida` un Catálogo fuera de sus topes. Con los topes de la tabla, el daño
     bruto sobre una casilla es como mucho 61 × 1.010 = 61.610 (cada pieza golpea una casilla
     a lo sumo una vez, área incluida; Resolución, #9, F1) y sobre un receptor 123.220 (un
     escudero suma la suya y la de su aliado, #9 F2); toda una
     resolución, aun con área sobre las 360 casillas, suma menos de 25.000.000, lejos de
     2.147.483.647. El valor más grande del estado es la cota de `vidaQuitada`, 1.000.015.000.
     **Única excepción:** el escritor y el lector del flujo canónico y la huella (regla 11)
     separan bytes y usan aritmética modular sin signo a propósito, con máscaras (`& 0xFF`)
     y bloques sin verificar explícitos, en un módulo aparte que no contiene reglas.
   - La división entera se aplica solo a valores no negativos y redondea hacia abajo.
   - Una resta que puede quedar negativa se pone en 0: el daño neto es
     `max(0, bruto − escudo)`, donde **bruto es la suma de todo el daño sobre ese objetivo
     en la resolución** (regla 15), y la vida de una pieza o un muro nunca baja de 0. Los
     bonos van de 0 a 10 (regla 11), así que el daño y el escudo efectivos nunca son
     negativos y el neto nunca supera al bruto.
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
   tirada del Modificador), el estado queda **esperando tirada**,
   con sus probabilidades a la vista, y solo acepta Tirada(n), con n entre 0 y 99, de la
   autoridad. La librería convierte n en un resultado con la fórmula de *Formulas*. El
   número queda en los eventos y en el registro. Una tirada nunca se pide en medio de
   Resolver: el momento de tirada es al empezar cada turno
   salvo el primero (Modificador, regla 2; el momento exacto lo define la Máquina de turno, #3).
   - **Tabla efectiva del Modificador.** La librería calcula la tabla al entrar en
     "esperando tirada" y la guarda en el estado: elige la tabla base según la regla del
     Modificador (#11) y pone en 0 las caras de reclutar que no pueden salir: las *Agotadas* (2 muros rotos o
     llamadas agotadas) reparten su parte con `reparto_liberado` (*Formulas*) y las *SinLugar*
     (tope o casillas) con `reparto_sin_lugar` (Reclutamiento, #10, F5). Los dos se calculan desde
     la tabla base y se suman; al final se bloquea Arremetida con `piezas_sin_rey ≤ limite_normal`. La
     tabla efectiva se valida igual que en CrearPartida; si no pasa, es ErrorInterno, que
     la validación de los papeles en CrearPartida (regla 2) más la fórmula hacen
     inalcanzable.
   - **`ProbabilidadesSiguienteTirada(estado, jugador)`**, consulta pura para cualquier
     jugador en cualquier fase (pilar 3b), devuelve la tabla efectiva que usaría su próxima
     tirada si su turno empezara con el tablero tal como está, junto con el **id de la tabla
     base** elegida (Modificador, #11, regla 4; lo muestra la Interfaz, #17); con esa tirada pendiente,
     coincide con la guardada; en una partida terminada, vacío.
   - El estado cuenta las tiradas aplicadas (`contadorTirada`); cada Tirada nombra la suya
     (regla 3).
   - **La autoridad es de confianza:** la librería no puede probar que n se eligió sin
     mirar el tablero. Un compromiso de semilla atado a `contadorTirada` lo puede agregar el
     Servidor (#28) sin cambiar Aplicar.
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
   roto, cara, tabla de probabilidades efectiva y nombre de la tabla base (#11, regla 9). `PartidaTerminada` (#3) trae ganador, motivo, `numeroTurno`, jugador del turno, fase y los ataques asignados que no se resolvieron, así un fin a mitad de turno se puede cerrar sin `TurnoTerminado`. El evento de daño trae además la vida que
   realmente perdió el objetivo (con 30 de daño neto y 5 de vida, pierde 5). Resolver sin
   ataques emite igual `TurnoTerminado`. Un rechazo no produce eventos. Cada evento de daño
   dice si fue **combinado** (dos o más piezas distintas **del rival del dueño del
   objetivo** lo golpearon en esa resolución, directo o por área, aunque el escudo absorba
   todo) y la **cantidad** de atacantes rivales distintos. El fuego amigo nunca cuenta
   (decisión del 2026-11-19); el daño que recibe un escudero al cubrir cuenta como golpe al
   escudero; la explosión de un demoledor también cuenta como golpe de su dueño. Con ese dato Audio (#22) y Render (#18) acentúan el golpe combinado (pilar 2).
   **Lista cerrada de eventos de `Resolver`** (Resolución, #9, regla 14), en este orden de
   tipos: `AtaqueEjecutado` (por ID de atacante), `DanoRecibido` (por ID de pieza),
   `DanoATramo` y `TramoRoto` (por casilla), `PiezaMuerta` (por ID) y `CoberturaQuitada`
   (por ID de escudero); después, los de la Máquina (`TurnoTerminado` y el fin o el turno
   siguiente).
9. **Vista previa = la misma operación.** La vista previa aplica Resolver sobre una
   **copia** del estado y devuelve **exactamente** lo que devolvería Resolver: la misma
   lista de eventos (también `TurnoTerminado` sin ataques, y el evento de fin con ganador y
   motivo si la partida terminaría) o el mismo código de rechazo (por ejemplo
   `EsperandoTirada`, `NoEsTuTurno`, `FaseIncorrecta` o `PartidaTerminada`). No existe un
   segundo cálculo del daño. En régimen estable la vista previa no reserva memoria; usa
   buffers propios de cada partida, nunca compartidos (cómo, lo fija el ADR).
10. **Consultas puras.** MovimientosLegales(pieza), ObjetivosValidos(pieza),
    `EffectiveStats(estado, pieza)` (catálogo + bonos del turno, sin bono de ataque en el modo Explosión del demoledor (#11, regla 7) + posición, como el arquero
    sobre el muro), `ProbabilidadesSiguienteTirada(estado, jugador)` (regla 6) y
    `EvaluateMatchEnd(estado)` leen el estado sin cambiarlo. Las fórmulas de *Formulas*
    también son funciones puras públicas: `CaraPorTirada(tabla, n)` y
    `TablaEfectiva(tablaBase, agotadas, sinLugar, piezasSinRey, limiteNormal)` (`reparto_liberado`, `reparto_sin_lugar` y el bloqueo de Arremetida). `RendirseDisponible(estado, jugador)` es verdadero si y solo si la partida está en curso, el jugador es el del turno y sus piezas vivas por 2 no pasan de las del rival (con el Rey). También son consultas puras `CasillasEnAlcance(pieza)` (#7),
    `hayMovimientoLegal(jugador)` (#6), y `tipo_disponible` y `estado_cara` (#10), y `AliadosCubribles`, `CubiertoPor`, `Cubre` y `Coberturas()` (#8), y las cuatro de Castillo y muros (#5, regla 10; cierre de la tanda B, 2026-11-28): `enMuro(estado, pieza)` (**total**: falso para un ID inexistente o una pieza que no es arquero), `tramosRotos(estado, jugador)` y `puedeReclutar(estado, jugador)` (con **resultado tipado**: un jugador fuera de {0, 1} da `ErrorInterno`, como `ladoDelRival`, Tablero AC-30) y `vidaTramo(estado, casilla)` (**total**: «sin tramo» fuera del muro o de la grilla). Ninguna reserva memoria (el resultado tipado es un struct; Castillo AC-16b; lo fija el ADR). Para los tests del Catálogo (#2): `EvaluarI(catalogo, config)` (chequeos I: pares que fallan y excepciones aprobadas) y `ValidarF2(fila)` (firmas en el ADR).
11. **Todo a la vista y una huella.** El estado contiene toda la partida: no hay
    información oculta (lo único que no existe todavía es la próxima tirada). Se guarda y
    se lee completo en una **forma canónica**: campos en orden fijo, enteros de 4 bytes
    little-endian escritos byte a byte, **la cantidad de elementos antes de cada colección**
    y **un byte de presencia antes de cada campo opcional**, con las colecciones ordenadas
    por la regla 5, así dos estados distintos nunca dan el mismo flujo (detalle en el ADR).
    La **huella** es un resumen de 64 bits de ese flujo con un algoritmo fijo del ADR, nunca
    el `GetHashCode` de la plataforma. Cubre **todos los campos del estado, sin
    excepción**: lo que no esté en la forma canónica no puede estar en el estado. Hoy son los campos de la tabla
    de dominios de abajo (cada campo en una fila; algunas filas agrupan varios) (la huella del Catálogo, no su contenido). Al terminar la partida, por cualquier motivo, la tirada pendiente se
    borra. La huella depende solo de los campos: estados iguales campo por campo dan la
    misma huella y cambiar cualquier campo la cambia (AC-45). Aplicar no la calcula: es la
    consulta aparte `Huella(estado)`. El **flujo guardado** que lee Cargar es la forma
    canónica seguida de los 8 bytes de su huella (decisión del 2026-11-19). Los **eventos**
    también tienen forma canónica (ADR), y el registro que reproduce una partida guarda cada
    comando aceptado **con su emisor** (#26).

    **Estado válido: tabla de dominios.** La huella prueba integridad, no validez.
    `EsValido(contexto, estado)` es verdadero **si y solo si** cada campo del estado cumple
    su fila de esta tabla; la tabla tiene una fila por cada campo de la forma canónica y un
    campo nuevo no existe sin su fila. Cargar y Resincronizar rechazan con `EstadoInvalido`
    todo estado que no la cumpla; CrearPartida y todo Aceptado de Aplicar producen solo
    estados que la cumplen (**clausura**: matriz de abajo y fila 15 de la regla 4). Cada tope es de su GDD
    dueño (estadísticas y 61 piezas, #2; pila, #3; tablero, #4; los de config, regla 2);
    cambiar uno cambia el contrato y la versión de reglas. **C_MAX = 1.000.000**
    (PROVISIONAL; ninguna partida de 25 minutos se acerca).

    | Campo | Dominio | Invariante con otros campos |
    |---|---|---|
    | Versión, huella del Catálogo | Soportadas; huella = la del contexto (si no, `VersionNoSoportada`) | — |
    | Id de partida, modo | Cualquier int32 · local u online | — |
    | Tablas base con papeles, posición de los castillos | Válidas según la regla 2 y *Formulas* | Castillos dentro del tablero y sin superponerse |
    | `limite_normal` | 1 a 37 | Con el bono de movimiento del Grito y el Catálogo de la partida, la pila no pasa de 256 (Movimiento, F5; se valida en CrearPartida) |
    | `tablaReclutas` | Piezas por llamada 1 a 17 y ≤ `topeVivas`; llamadas 0 a 20 | Cada tipo en una sola cara; sin el Rey (#10) |
    | `vida_tramo` | 1 a 1.000 | Tope de la vida de cada tramo de muro (#5) |
    | `bono_grito_ataque`, `bono_grito_movimiento` | 0 a 10 cada uno | No los dos en 0; la pila con el bono de movimiento ≤ 256 (#6, F5; #11) |
    | Tablero | 24 columnas (0–23) × 15 filas (0–14), constante (Tablero, #4) | Toda casilla del estado está dentro |
    | `contadorTirada`, `contadorComandos` · `numeroTurno` | 0 a C_MAX · 1 a C_MAX + 1 (empieza en 1) | `contadorTirada` ≤ `contadorComandos`; `numeroTurno` ≤ `contadorComandos` + 2 (CrearPartida y cada Aceptado avanzan como mucho un turno, porque una cascada se detiene en el turno siguiente, que necesita tirada); En curso ⇒ `contadorComandos` ≤ C_MAX − 2 (el Aceptado que llega a C_MAX − 1 termina en Tablas, regla 16) |
    | Fase, etapa, jugador del turno | Valores de sus enums (#3) · 0 o 1 | Etapa mover con reclutar o mover; atacar con atacar; ninguna etapa esperando tirada. Resolver no es una fase en reposo (#3, regla 2) |
    | Tabla base del turno, cara del Modificador, bonos | Vacía o índice de una tabla base · vacía o 1 al largo de esa tabla · cada bono 0 a 10 (#11) | Tabla y cara, las dos vacías (primer turno y esperando la tirada) o las dos presentes (decisión del 2026-11-19); la cara tiene p > 0 en esa tabla base o es la de Grito, la de Arremetida o una de reclutar (pueden recibir lo liberado; lo que pase después en el turno no la invalida); bono > 0 solo con la cara de Grito |
    | Piezas | Como mucho 61 vivas por jugador y, por tipo, su tope de piezas vivas (#2); una pieza que muere sale del estado | IDs únicos, de 1 a próximo ID − 1; tipo en el Catálogo; vida 1 a la de su tipo; movimientos usados 0 a movimiento del tipo + 10; nunca dos piezas en una casilla (un arquero **de su dueño** puede estar sobre un tramo **en pie**: invariante de ocupación, Castillo #5, regla 1, y Tablero AC-31; fe de erratas del cierre de la tanda B, 2026-11-28); En curso ⇒ un Rey por jugador |
    | Muros | 16 tramos por castillo | Casillas del perímetro de su castillo; vida 0 a la del tramo (#5) |
    | Ataques asignados, coberturas | Uno por pieza como mucho | Ataque: atacante vivo del jugador del turno, objetivo = una casilla que `AsignarAtaque` aceptaría (#7), solo en la etapa atacar (una partida Terminada ahí los conserva). Un escudero del jugador del turno no ataca y cubre a la vez. Cobertura: la que `Cubrir` aceptaría (#8, regla 14; sin cadenas), como mucho `coberturas_max` por jugador; las del jugador del turno solo en su etapa atacar. Cobertura: escudero y aliado vivos del mismo jugador (siguen en el turno rival, #8) |
    | Pila de deshacer | 0 a 256 entradas (`pila_tope`, #3) | IDs de piezas vivas y casillas dentro del tablero; vacía fuera de la fase de mover |
    | Contadores de reclutas | 4 por jugador (Común, Catapulta, Jinete, Demoledor), de 0 a su tope de llamadas (`tablaReclutas`, #10) | — |
    | `vidaQuitada` de cada jugador | 0 a `estadistica_max × (C_MAX − 1) + 16 × vida_tramo` = 1.000.015.000 con `vida_tramo` 1.000 (confirmada por #9) | Vida realmente quitada a piezas y muros del rival en toda la partida; sin fuego amigo; lo que absorbe un escudero al cubrir cuenta. Es estadística: ninguna regla la lee (jugador, 2026-11-25). Solo dominio: `EsValido` no puede comprobar lo histórico |
    | Próximo ID | 1 a C_MAX | Mayor que todo ID de pieza |
    | Tirada pendiente | Vacía o {Modificador, jugador, tabla base, tabla efectiva} | Solo En curso; Modificador: jugador = jugador del turno y tabla efectiva = la que se recalcula |
    | Motivo y ganador | Vacíos, o un motivo de la regla 16 con el ganador que pide su fila | Terminada si y solo si hay motivo; ReyMuerto ⇒ queda un solo Rey y gana su dueño; DobleRey ⇒ 0 Reyes |

    **Matriz de clausura.** Cada transición que produce un estado, con las filas que toca y
    por qué quedan dentro. Una transición nueva no existe sin su fila (AC-62).

    | Transición | Filas que cambia | Por qué queda dentro |
    |---|---|---|
    | CrearPartida | Todas | Valida toda la config (regla 2); `numeroTurno` 1, contadores y `vidaQuitada` 0, próximo ID 27 (Tablero, regla 8); su primera fase de mover tiene un movimiento legal (despliegue fijo); castillos, piezas y muros colocados por el despliegue del Tablero (#4), con IDs por la regla 7 |
    | Tirada del Modificador | Tabla base y cara del turno, bonos, tirada, `contadorTirada` (+1), fase y etapa | La tabla efectiva se validó al entrar en espera; `cara_por_tirada` da una cara con p > 0 en ella; bonos del Modificador (#11) dentro de 0 a 10 |
    | Mover · Deshacer | Piezas (casilla, movimientos usados), pila | Fila 14 valida destino y movimientos; la pila ≤ 256 por la fila 15; no tocan la cara del turno |
    | Reclutar · recluta automático | Piezas, próximo ID, contadores de reclutas | Todo o nada; topes de piezas por tipo y de llamadas (fila 14); próximo ID ≤ C_MAX por la fila 15; no tocan la cara del turno |
    | AsignarAtaque · QuitarAtaque · Cubrir · QuitarCobertura | Ataques, coberturas | Fila 14 valida atacante, objetivo, escudero y aliado |
    | TerminarMovimiento · `TurnTimeout` de mover | Pila (vacía), fase y etapa | Igual que TerminarMovimiento; cascadas solo al empezar una fase (regla 17) |
    | Resolver · `TurnTimeout` de atacar | Vida, piezas muertas (salen), movimientos usados (a 0, también si el turno cierra por saltos; #3, regla 11), muros, ataques, coberturas, `vidaQuitada`, fin, `numeroTurno`, tirada | Vida en 0 sale; `vidaQuitada` suma solo vida realmente perdida (cota en la Máquina de turno, regla 15); resto con `max(0, …)`; `EvaluateMatchEnd` fija motivo y ganador de la regla 16; si sigue, abre el turno siguiente (tabla, cara y bonos vacíos) y su tirada |
    | Cascada al empezar una fase (tras Tirada, Reclutar o recluta automático, TerminarMovimiento o `TurnTimeout` de mover) | Fase y etapa; si el turno termina: movimientos usados (a 0), ataques, coberturas del jugador que entra (`Caducada`, #8 regla 9), `numeroTurno`, jugador del turno, tabla, cara y bonos (vacíos), tirada | Regla 17: cada salto es una fase que empieza sin acción posible (el evento `ReclutaPerdido` no es un campo); se detiene en el turno siguiente, que espera tirada |
    | `Rendirse` · `CerrarPartida` | Motivo, ganador, tirada (se borra) | Motivo y ganador por la regla 16 |
    | Todo Aceptado | `contadorComandos` (+1); con Tablas automáticas, motivo, ganador y tirada (se borra) | Si llega a C_MAX − 1 y la partida sigue, termina ahí en Tablas (regla 16): nunca queda En curso un estado en C_MAX − 1 |

    El decodificador estricto valida cada prefijo de cantidad contra el tope de su
    colección **antes** de reservar memoria. Solo los tests construyen estados fuera de esta
    tabla (un constructor interno, AC-43).
12. **Versión.** El estado lleva la versión de reglas y la huella del Catálogo. Una
    librería que no soporta esa versión rechaza la partida con `VersionNoSoportada`. Cómo
    se conservan las versiones viejas para las repeticiones lo decide el ADR.
13. **Fuera de la librería:** el tiempo (solo recibe `TurnTimeout`), los textos, el
    dibujo, el sonido, los archivos, la red, las métricas y la IA (que usa la misma API que
    un jugador). Los números de secuencia de los mensajes, la reconexión, la vida de la
    autoridad (qué pasa si deja de responder mientras se espera una tirada) y el modelo de
    red del cliente son del Servidor de partida (#28), y el contrato es neutral respecto de
    ese modelo. Una predicción rechazada se
    deshace volviendo al último estado confirmado, nunca con Resincronizar. Un comando
    pendiente cuyo `contadorEsperado` ya no coincide con el confirmado (porque avanzó con
    un comando del rival o de la autoridad) da `ComandoViejo` y se descarta: el cliente
    nunca reescribe contadores, y reemitirlo decidido sobre el estado nuevo es del #28.
    La huella prueba **integridad, no origen**: el origen lo garantiza el transporte
    autenticado (#28), y el servidor nunca toma un estado de un mensaje de cliente.
14. **Resincronizar(contexto, confirmado, estadoAutoridad, huellaDeclarada).** Corrige un cliente
    con el estado que manda el servidor. `confirmado` es el **último estado confirmado**
    del cliente, nunca uno con comandos predichos encima; `huellaDeclarada` es la huella
    que el servidor envía junto con su estado. **El motivo lo deriva la librería**, nunca
    quien llama: mismo `contadorComandos` y huella distinta es **Divergencia**; cualquier
    otro caso aceptado (el servidor va adelante) es **Reconexion**. Con el servidor
    adelante la librería no puede ver una divergencia anterior: detectarla comparando
    huellas con el mismo contador, y cada cuánto, lo decide #28.

    Valida en este orden:
    1. `confirmado` está en modo local → `SoloOnline`.
    2. `estadoAutoridad` trae una versión de reglas o una huella del Catálogo que esta
       librería no soporta, o distintas de las de `confirmado` o del contexto →
       `VersionNoSoportada`.
    3. `Huella(estadoAutoridad)` no es `huellaDeclarada` (estado dañado en el camino),
       `estadoAutoridad` no es un estado válido (regla 11), o algún campo inmutable
       de la regla 2 (modo, tablas base con sus papeles, límite normal, vida del tramo, tabla de reclutas, bonos del Grito, posición de los castillos) difiere del
       de `confirmado` → `EstadoInvalido`.
    4. El id de partida de `estadoAutoridad` no es el de `confirmado` → `OtraPartida`.
    5. El `contadorComandos` de `estadoAutoridad` es menor que el de `confirmado` →
       `EstadoViejo` (un estado viejo, aunque sea válido, nunca hace retroceder la
       partida). Con el mismo contador se acepta: es el caso de Divergencia.
    6. `confirmado` está Terminada y su huella es `huellaDeclarada` → `PartidaTerminada`
       (no hay nada que corregir).

    Hacia un estado Terminado sí se puede, y **desde** uno también cuando las huellas
    difieren: el final del servidor manda, también si el cliente terminó por error con
    otro ganador. Si es válido, el resultado es el estado del servidor tal cual (también su
    pila de deshacer y su tirada pendiente) y un evento `Resincronizado` con el motivo, la
    huella local vieja y la nueva. Con Divergencia, la interfaz avisa al jugador y la Analítica
    online (#32) lo registra como S1 (Métricas, #19, solo corre en local); con Reconexion, ninguno de los dos. Una carrera esperable
    (un comando que llega después del `TurnTimeout`) nunca termina en Resincronizar.
15. **Resolución simultánea.** Resolver calcula todo el daño sobre el estado de **antes**
    de resolver: una pieza que muere en esa resolución igual golpea, y el orden por ID solo
    fija el orden de los eventos, nunca el resultado. El daño sobre un mismo objetivo se
    suma (bruto) y el escudo se resta una sola vez (la fórmula es del GDD de Resolución,
    #9).
16. **Fin de partida.** Después de cada resolución, `EvaluateMatchEnd` decide si hay
    ganador. Si muere un solo Rey, gana el otro jugador (motivo **ReyMuerto**). **Si mueren
    los dos Reyes en la misma resolución, gana el jugador del turno**, el que resolvió, sea
    con Resolver o porque `TurnTimeout` disparó la resolución de los ataques asignados
    (motivo **DobleRey**; confirmado en la Máquina de turno, #3, regla 13). `Rendirse` termina la partida con motivo **Rendicion**.
    `CerrarPartida` la termina con motivo **Abandono** (ganador de la autoridad),
    **RelojAgotado** (gana el rival del jugador del turno) o **Tablas**
    (sin ganador). **Tablas también es automático** (decisión del 2026-11-27): el Aceptado
    que deja `contadorComandos` en C_MAX − 1 con la partida en curso **después de toda su
    cascada** la termina en ese mismo Aplicar, sin ganador, con su `PartidaTerminada` como
    último evento; si el comando ya terminó la partida, su motivo se conserva; así ninguna partida queda trabada, haya
    timer o no. Lista cerrada de motivos, con el ganador que pide cada uno: **ReyMuerto**,
    el dueño del único Rey vivo; **DobleRey**, el jugador del turno; **Rendicion** y
    **Abandono**, 0 o 1; **RelojAgotado**, el rival del jugador del turno; **Tablas**, vacío.
17. **Qué hace `TurnTimeout`** (definitivo: Máquina de turno, #3, regla 12).
    - **Etapa mover:** si hay un Reclutar obligatorio pendiente, la librería recluta el
      primer tipo **disponible** de la cara en el orden del Catálogo (decisión del 2026-11-19: el
      jugador no pierde el recurso). Disponible es `tipo_disponible` (#10, F1): el castillo
      recluta, le quedan llamadas, entran en el tope y hay lugar para todas sus piezas (todo o nada). El evento del
      reclutamiento lleva la marca `automatico`, así la interfaz dice que lo eligió el
      timeout. (Un Reclutar pendiente siempre tiene un tipo disponible: si no lo hubiera,
      se habría perdido al empezar la fase.) Después cierra la pila de deshacer y pasa a
      atacar, igual que TerminarMovimiento.
    - **Recluta imposible** (decisión del 2026-11-19): si al empezar la fase de reclutar
      ningún tipo está disponible, el reclutamiento se pierde en ese momento
      (`ReclutaPerdido`) y la partida sigue en la fase de mover, así el turno nunca se
      traba. Es una defensa: la tabla y la fase usan la misma consulta (#10, regla 6).
    - **Etapa atacar:** resuelve los ataques asignados, como Resolver. Las coberturas ya
      asignadas siguen valiendo. Sin ataques, el turno termina con `TurnoTerminado`.
    - **Fin automático, solo al empezar una fase** (decisión del 2026-11-19): una fase que
      **empieza** sin ninguna acción posible se salta con el evento `FaseSaltada`. Acción
      posible, lista cerrada: en reclutar, algún tipo disponible; en mover, una pieza con
      algún movimiento legal (la fase no existe con Distracción); en atacar,
      una pieza con algún objetivo válido o un escudero con un aliado que cubrir. Una fase
      ya empezada nunca termina sola: la cierra el jugador (TerminarMovimiento, Resolver) o
      `TurnTimeout`. Las cascadas al empezar una fase vienen en los eventos del comando que
      la abrió (*"el turno también termina solo si no quedan acciones"*, del concepto).
    - **Etapas y fases:** la etapa mover del timer cubre las fases reclutar y mover; la
      etapa atacar cubre atacar y resolver. Mientras se espera una tirada no corre ninguna
      etapa (la tabla definitiva es de la Máquina de turno, #3).

### States and Transitions

| Desde | Hacia | Cuándo |
|---|---|---|
| (ninguno) | En curso | CrearPartida con config válida: castillos, piezas y muros colocados |
| (ninguno) | (sin partida) | Config inválida: `ConfigInvalida` o `ProbabilidadesInvalidas` |
| En curso | En curso · esperando tirada | Empieza un turno que necesita tirada (el momento lo define la Máquina de turno, #3). Si es al terminar un Resolver o un `TurnTimeout` de atacar, el estado que devuelven ya queda esperando, pero ningún cálculo de esa resolución usa la tirada |
| En curso · esperando tirada | En curso | Tirada(n) válida de la autoridad. Cualquier otro comando se rechaza: `SoloAutoridad` (Tirada, `TurnTimeout` o `CerrarPartida` de un jugador) o `EsperandoTirada` (el resto, `TurnTimeout` de la autoridad incluido), salvo `Rendirse` y `CerrarPartida` |
| En curso (también esperando tirada) | Terminada (ganador, motivo) | Después de resolver, `EvaluateMatchEnd` encuentra un ganador (regla 16: ReyMuerto o DobleRey); un jugador envía `Rendirse` (Rendicion); la autoridad envía `CerrarPartida` (Abandono, RelojAgotado salvo esperando tirada, o Tablas); o un Aceptado deja `contadorComandos` en C_MAX − 1 (Tablas). La tirada pendiente se borra |
| En curso o esperando tirada (online) | El estado del servidor, también Terminada | Resincronizar válido (regla 14) |
| Terminada (online) | El estado del servidor | Resincronizar con una huella distinta de la local (el cliente terminó por error) |
| Terminada | Terminada | Todo comando se rechaza con `PartidaTerminada`, y Resincronizar también si la huella coincide; ningún comando la saca de ahí |

Las fases del turno (tirada, reclutar, mover, atacar, resolver) son de la Máquina de turno
(#3), que las define dentro de este contrato. El reloj del turno no corre mientras se
espera una tirada; el momento lo elige la autoridad (en local, al terminar la presentación,
Render #18 r25; online, #28).

### Interactions with Other Systems

| Sistema | Le da a la librería | Recibe de la librería | Dueño de la interfaz |
|---|---|---|---|
| 2 Catálogo | Tabla de piezas (archivo de datos) | Se fija en la config con su huella | Catálogo |
| 3 Máquina de turno | Pipeline de fases, pila de deshacer, `EvaluateMatchEnd` | Corre dentro del contrato | Librería (contrato) y #3 (fases) |
| 4–11 Reglas del núcleo | Sus comandos, códigos y eventos | Respetan las reglas 1–17 | Cada GDD |
| 12 Timer | Comando `TurnTimeout(turno, etapa)`; reloj por jugador (`CerrarPartida` con RelojAgotado) | `numeroTurno` y etapa actuales | Timer |
| 13 Entrada | Traduce clics en comandos | Consultas para resaltar casillas; códigos de rechazo | Entrada |
| 14 Localización | Textos de códigos y eventos | Códigos | Localización |
| 15–18, 22 Presentación | Nada | Estado, eventos y vista previa | Cada GDD |
| 19 Métricas | Nada | Eventos, `Rechazado` con su código y `ErrorInterno` (S1) | Métricas |
| 25 Rival IA | Comandos por la misma API | Estado público y consultas | Rival IA |
| 26 Registro | Config, comandos y tiradas guardados | Reproduce aplicando los mismos comandos | Registro |
| 28 Servidor | Es la autoridad de Tirada, `TurnTimeout` y `CerrarPartida` (abandono y reloj agotado); transporte autenticado | Corre la misma librería; compara (`contadorComandos`, huella) con el cliente | Servidor |

Con los sistemas sin GDD (#20 en adelante), las interfaces son contrato provisional.

## Formulas

La librería casi no tiene números de juego propios: daño, alcance y probabilidades son de
otros GDD. `cara_por_tirada` convierte una tirada inyectada (regla 6)
en un resultado; `reparto_liberado` arma la tabla efectiva del Modificador. No
tienen ciclos de retroalimentación: la librería no contiene balance.

The `cara_por_tirada` formula is defined as:

`cara_por_tirada = la primera cara f (de 1 a k) tal que n < S_f, donde S_f = p_1 + p_2 + … + p_f`

**Variables:**
| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| n | int | 0–99 | inyectado (Tirada de la autoridad) | Número que envía la autoridad |
| k | int | 2–10 | largo de la tabla | Cantidad de caras: siempre es el largo de la tabla, no un dato aparte (el Modificador tiene 5). Mínimo 2 porque la regla 2 exige una cara de Grito y otra de Arremetida |
| p_i | int | 0–100 | data file (GDD del Modificador) | Probabilidad de la cara i, en % |
| S_f | int | 0–100 | calculado | Suma acumulada hasta la cara f |
| tirada_rango | int | 100 | constante del contrato | Cantidad de valores posibles de n (0 a 99) |

**Output Range:** 1 a k (k ≤ 10). Las caras con p_i = 0 nunca salen.
**Validación de la tabla, en CrearPartida y en este orden:** la tabla tiene entre 2 y 10
caras; cada p_i está entre 0 y 100; recién entonces se suman (el total nunca pasa de 1.000)
y la suma es exactamente `tirada_rango` = 100. Si algo falla, la partida no se crea y se
devuelve `ProbabilidadesInvalidas` (no se corrige en silencio). La tabla efectiva que arma
`reparto_liberado` se valida al entrar en "esperando tirada", nunca al llegar la tirada, así
que una partida nunca queda esperando una tirada imposible (una tabla que sumara 99 dejaría
n = 99 sin cara). Un n fuera de 0–99 se rechaza con `TiradaInvalida`.
**Example:** tabla "Pierde 3/4" 20/20/25/20/15 y n = 47: S = 20, 40, 65…; 47 < 65 → cara 3,
Llamada al servicio. Con n = 99: S_5 = 100 y 99 < 100 → cara 5, Arremetida. Con n = 0 y
p_1 = 0: S_1 = 0, 0 < 0 es falso, sigue a la primera cara con probabilidad mayor que 0.

*(La fórmula `opcion_por_sorteo` se retiró el 2026-11-25: el Tablero (#4) usa castillos fijos y no hay sorteo. El azar de la librería es solo la tirada del Modificador.)*

The `reparto_liberado` formula is defined as:

`L = Σ p_i de las caras de reclutar que no pueden salir` · esas caras pasan a 0 ·
`p_Grito' = p_Grito + L / 2` (división entera hacia abajo) · `p_Arremetida' = p_Arremetida + (L − L / 2)`

Después, **si el jugador tiene `piezas_sin_rey ≤ limite_normal`** (Arremetida no le agrega nada; jugador, 2026-11-24):
`p_Grito'' = p_Grito' + p_Arremetida'` · `p_Arremetida'' = 0`, con las primas ya sumando lo de `reparto_sin_lugar` (#10, F5): pasa a Grito toda la Arremetida final

**Variables:**
| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| p_i | int | 0–100 | tabla base elegida (Modificador, #11) | Probabilidad de cada cara antes del reparto |
| L | int | 0–100 | calculado | Suma de las caras de reclutar liberadas: por 2 muros rotos, las dos; por llamadas agotadas, la que se agotó. Se calcula siempre desde la tabla base y sobre el conjunto entero de caras liberadas, nunca encima de una tabla efectiva anterior |
| piezas_sin_rey | int | 0–60 | calculado | Piezas vivas del jugador, sin el Rey y sin muros |
| limite_normal | int | 1–37 (hoy 12) | data file (config, regla 2; dueño Movimiento, #6) | Límite normal de piezas que se mueven por turno, sin el Rey |
| p_Grito', p_Arremetida' | int | 0–100 | calculado | Caras que reciben lo liberado; cuáles son lo dice el papel de cada cara en la config (regla 2) |

**Output Range:** una tabla de las mismas k caras que suma exactamente `tirada_rango`, porque
`L / 2 + (L − L / 2) = L`. Se reparte **una sola vez sobre el total liberado**, no cara por
cara (decisión del 2026-11-19: un solo redondeo, y el punto impar siempre va a Arremetida).
Con L = 0 el reparto no cambia la tabla (Arremetida se puede bloquear igual, y la suma sigue en 100). Si la base tiene Grito y Arremetida en 0, igual reciben L / 2 y
L − L / 2 (con L = 1, Grito queda en 0) y la suma sigue en 100. La fórmula necesita exactamente una cara de Grito y una de Arremetida, y
CrearPartida lo exige (regla 2). La tabla resultante se valida como en CrearPartida
(regla 6).
**Example:** tabla "Pierde con 1/2" 15/20/25/25/15 (Distracción, Grito, Llamada, Cuerno,
Arremetida) con 2 muros rotos: L = 25 + 25 = 50 → Grito 20 + 25 = 45, Arremetida
15 + 25 = 40 → 15/45/0/0/40. Tabla "Gana 4 a 3" 20/25/15/20/20 con Llamada agotada: L = 15 →
Grito 25 + 7 = 32, Arremetida 20 + 8 = 28 → 20/32/0/20/28. Las dos son con más de 12 piezas
sin el Rey. Con 12 o menos, la primera queda 15/85/0/0/0 y la tabla Normal sin nada liberado,
20/40/20/20/0. Todas suman 100.

La huella del estado y la del Catálogo no son fórmulas de diseño: su algoritmo lo elige el
ADR de la librería.

## Edge Cases

**Quién manda cada comando**

- **If a player sends Tirada, `TurnTimeout` or `CerrarPartida`**: `SoloAutoridad`; online,
  el emisor sale de la conexión (regla 3).
- **If several checks fail at once**: el primer código en el orden de la regla 4 (AC-29).

**Repeticiones y carreras online**

- **If the same command arrives twice, or one from an earlier turn arrives late**: su
  `contadorEsperado` ya no es el actual y se rechaza con `ComandoViejo`, también un
  Deshacer duplicado o un Resolver en la misma fase de un turno posterior. Una Tirada repetida o
  demorada se rechaza con `TiradaNoEsperada` si no se espera ninguna, y con `TiradaVieja`
  si ya se espera la de otro turno (su contador es viejo).
- **If `TurnTimeout` and Resolver arrive almost at once**: gana el que la autoridad aplica
  primero y el registro guarda ese orden; el otro se rechaza (AC-37).
- **If the move-stage `TurnTimeout` arrives just after TerminarMovimiento**:
  `TimeoutObsoleto`; no resuelve los ataques que el jugador todavía está asignando.
- **If a mandatory recruit has no available type** (defensa, inalcanzable jugando): se pierde
  al empezar la fase (`ReclutaPerdido`, regla 17); el turno nunca espera un `TurnTimeout`.
- **If client and server disagree** (misma cantidad de comandos y otra huella, cliente
  que terminó por error, reconexión): manda el servidor vía Resincronizar (regla 14);
  estados dañados, viejos, de otra partida o con otros campos inmutables se rechazan y el
  cliente conserva el suyo.
- **If the server rejects a command the client had already predicted**: no es una
  divergencia; el cliente vuelve a su último estado confirmado (regla 13), sin S1.
- **If a player disconnects and does not come back**: el Servidor (#28) decide cuándo es
  abandono y envía `CerrarPartida(Abandono, otro jugador, vacío)`, también mientras se espera
  una tirada.
- **If `CerrarPartida(RelojAgotado)` arrives after the turn already closed**, o mientras se
  espera una tirada: `CierreInvalido` (su `turno` ya no es `numeroTurno`, también cuando el
  mismo jugador vuelve a tener el turno); la partida sigue (regla 3).
- **If a match reaches `contadorComandos` = C_MAX − 1** (por ejemplo, Mover y Deshacer
  en bucle sin timer): ese Aceptado la termina en Tablas (regla 16). Un `LimitePartida` de
  la fila 15 es una defensa; si llega a un comando de autoridad **o de un jugador** (también las dos defensas de la fila 15: un recluta que pasaría C_MAX y un `Mover` con la pila en 256), la autoridad cierra con
  `CerrarPartida(Tablas, vacío, vacío)` (#12 regla 9, #28).
- **If `CerrarPartida(RelojAgotado)` and Resolver or TerminarMovimiento race**: manda el orden
  en que la autoridad los aplica; el segundo da `PartidaTerminada`, `CierreInvalido` o
  `ComandoViejo`, y el registro guarda ese orden.

**Deshacer y vista previa**

- **If Deshacer is sent after TerminarMovimiento**: `FaseIncorrecta`. La pila se cierra al
  pasar a atacar y se vacía al empezar cada turno: una tirada o un reclutamiento nunca se
  deshacen (Máquina de turno, #3, regla 8).
- **If a preview is requested in any state** (esperando tirada, sin ataques, fuera de la
  fase de atacar, sin el turno, con un final de partida): devuelve exactamente lo que
  devolvería Resolver (regla 9); nunca usa ni adelanta una tirada futura.

**Enteros y resolución**

- **If a probability table sums to 100 but some recruit faces are at 0**: es válida; esas
  caras solo salen si reciben lo liberado por `reparto_sin_lugar` (#10, F5). Sin cara de Grito o de Arremetida, o con dos: `ProbabilidadesInvalidas`.

**Versiones, datos y fin de partida**

- **If another rules version or Catalog hash is loaded**: `VersionNoSoportada` (AC-14).
- **If a saved state does not match its own hash, or matches it but breaks a row of the
  domain table** (archivo dañado o editado, huella recalculada, por ejemplo un bono de
  2.147.483.600 o un ataque a un ID inexistente): `EstadoInvalido`; la partida no se carga
  y nunca llega a ErrorInterno.
- **If a castle is outside the board, overlaps the other or is not the Tablero's pair, or a Catalog stat is above its cap**:
  `ConfigInvalida` (reglas 2 y 5).

## Dependencies

**Depende de: Catálogo de piezas (#2), solo su esquema de datos (blanda).** La librería es
la base de todo el núcleo y no necesita la lógica de ningún otro sistema. La tabla del
Catálogo entra como dato de la config: la librería la recibe, la valida contra el formato
que define el GDD del Catálogo y la fija con su huella. El GDD del Catálogo existe y
nombra a la librería como consumidora de su esquema.

**Dependen de ella:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| 3 Máquina de turno | Dura | Escribe sus fases dentro del contrato; pila de deshacer; `EvaluateMatchEnd` |
| 4 Tablero y despliegue | Dura | CrearPartida con castillos fijos (par 4–6), IDs estables; no hay sorteo |
| 5 Castillo y muros | Dura | Define `vida_tramo` (campo de la config, 1 a 1.000), el bono por posición del arquero en `EffectiveStats` y las consultas `enMuro`, `tramosRotos`, `puedeReclutar` y `vidaTramo` (regla 10; `tramosRotos` y `puedeReclutar` con resultado tipado, sin reservar memoria) |
| 6–11 Reglas del núcleo | Dura | Escriben sus reglas, comandos, códigos y eventos dentro del contrato |
| 25 Rival IA | Dura | Juega con la misma API y las consultas |
| 26 Registro de partida | Dura | Guarda config, comandos y tiradas; reproduce con Aplicar |
| 28 Servidor de partida | Dura | Corre la misma librería; es la autoridad; compara huellas |
| 15 Ficha de pieza y HUD | Dura | `EffectiveStats`, consultas puras (vida, tipo, casilla, dueño, `enMuro`, `tramosRotos`, `Cubre`/`CubiertoPor`) y la condición de `Rendirse`; solo lectura |
| 17 Interfaz del Modificador | Blanda | `ProbabilidadesSiguienteTirada` con el id de la tabla base, evento `Tirada`, `contadorTirada`; solo lectura |
| 12 Timer | Dura | `numeroTurno`, etapa y fin; envía `TurnTimeout` y `CerrarPartida` (RelojAgotado con turno, Tablas ante `LimitePartida`) |
| 14 Localización | Blanda | Códigos de rechazo y eventos |
| 13, 16, 18, 19, 22, 24 Cliente | Blanda | Consultas, eventos y vista previa; funcionan con cualquier implementación que cumpla el contrato |

**Bidireccionalidad:** cada uno, al escribirse, nombra a la Librería en sus Dependencies.

## Tuning Knobs

**La librería no tiene perillas de balance propias.** Todo valor que un ajuste de balance
tocaría vive en el GDD que lo define; la librería solo lo recibe como dato:

| Valor | Dónde se ajusta | Nota |
|---|---|---|
| Estadísticas de las piezas | Catálogo (#2), archivo de datos | Cada partida fija la tabla que usó con su huella |
| Probabilidades p_i de cada tabla | Modificador (#11), archivo de datos | La librería solo exige que sumen 100 (`cara_por_tirada`) |

**Constantes del contrato (no son perillas):**

- **Rango de la tirada, 0–99 (`tirada_rango` = 100, `tirada_max` = 99).** Las
  probabilidades se expresan en porcentajes enteros y las dos fórmulas usan `tirada_rango`,
  nunca un 100 suelto.
  Pasar a una granularidad más fina (por ejemplo 0–999) cambia el contrato y la versión de
  reglas.
- **Solo enteros de 32 bits con aritmética verificada y orden por ID.** Son la base del
  determinismo; no se ajustan.
- **Topes de la tabla de dominios (regla 11)**, cada uno de su GDD dueño; C_MAX sigue
  PROVISIONAL. No son balance, garantizan que ninguna cuenta desborde, y cambiarlos sube
  la versión de reglas.
- **Presupuestos de rendimiento, constantes del contrato (no datos de la config): vista previa 5 ms (`vista_previa_presupuesto_ms`) y comando 1 ms (`comando_presupuesto_ms`) (percentil 95, AC-24 y
  AC-25).** Salen del cuadro de 16,6 ms a 60 fps; cambiarlos es una decisión de UX, no de
  balance.

## Visual/Audio Requirements

No aplica: la librería no dibuja ni suena. Sus eventos (regla 8) son la fuente de todo lo
visual y lo sonoro; Render (#18), Audio (#22) y Ficha y HUD (#15) definen cómo se ve y cómo
suena cada uno.

## Game Feel

No aplica como sensación directa: la sostienen los presupuestos de *Tuning Knobs* (AC-24,
AC-25, AC-33a, AC-33b) y la identidad entre vista previa y resolución (AC-11).

## UI Requirements

La librería no tiene interfaz. Le exige a la interfaz:

- Todo código que la regla 4 marca para el jugador tiene un texto traducido que dice por
  qué (Entrada #13, Localización #14).
- Las probabilidades de la próxima tirada se pueden consultar desde el estado (Interfaz del
  Modificador #17; pilar 3b).
- La vista previa muestra exactamente los eventos que devuelve la librería, sin calcular
  nada por su cuenta (Vista previa #16).
- Un comando del jugador descartado por un código interno (`ComandoViejo`,
  `LimitePartida`) muestra un aviso neutro, por ejemplo "el turno terminó por tiempo".

## Cross-References

Ver *Interactions with Other Systems* y *Dependencies*.

## Acceptance Criteria

**Dónde corren.** Los tests viven en `Assets/Tests/EditMode/LibreriaReglas/`, sin usar
Unity en su código, y un `.csproj` fuera de `Assets/` compila esos mismos archivos para
`dotnet test` en cada PR (decisión del 2026-11-19; configuración en el ADR). Solo AC-33b
vive en `Assets/Tests/PlayMode/LibreriaReglas/`. La evidencia es el resultado de los tests.

**Tipos de prueba y compuertas.** Son **Lógica** (BLOCKING) salvo estos:
- Las partes web de AC-6 y AC-30 son **Integración**, corren cada noche en el harness web
  que define el ADR (todavía no existe) y son **ADVISORY**: Unity Test Framework no corre
  dentro de un build WebGL. La compuerta BLOCKING es .NET más el editor de Unity.
- AC-24, AC-25 y AC-33b son **Rendimiento**. Quedan ADVISORY hasta que el ADR de la
  librería nombre la máquina, el navegador y el build de referencia; desde entonces son
  BLOCKING en esa referencia. AC-33a (.NET) es BLOCKING desde el primer día.
- AC-7 es una compuerta de compilación: la autoritativa es la compilación del `.csproj`
  en CI.
- AC-32 queda **DEFERRED** hasta el GDD del Servidor (#28).

Ninguno necesita captura.

**Fixtures.** Los ACs usan un Catálogo y reglas de prueba guardados en el repositorio, con
los valores de sus GDD dueños (#2 a #11, todos escritos); cambiar uno de esos valores
regenera los fixtures. AC-49 depende además del algoritmo de huella del ADR.

**Precondiciones alcanzables.** Cada fixture se alcanza desde CrearPartida con comandos
públicos (el fixture guarda esa secuencia) o lo arma el constructor de pruebas, y entonces
el AC lo dice. Ningún AC afirma un resultado sobre un estado que el juego no puede alcanzar
sin decirlo.

**Archivos dorados y oráculos.** Un archivo dorado lo genera el mismo código que prueba:
demuestra que las plataformas coinciden, no que el resultado sea correcto. Por eso cada
archivo dorado se regenera solo con un script explícito y una revisión humana, y AC-49 los
ancla a oráculos escritos a mano. En todos los ACs, "H" es la huella del estado del GIVEN y
todo comando de jugador lleva el `contadorEsperado` correcto, y toda Tirada su
`contadorTiradaEsperado`, salvo que el AC diga otra cosa. Todo `ConfigInvalida` se afirma
con su código (regla 2). Todo estado del constructor cumple `EsValido`, salvo que el AC diga
que no. En toda Resincronizar de un estado mutado, `confirmado` es el estado sin mutar, con el
fixture base en modo online, y `huellaDeclarada` es la `Huella` del mutado.

**Reglas centrales**

- **AC-1 (pura, todos los comandos)** — Test de tabla con un caso por cada tipo de comando
  de la regla 3 (los 13). **GIVEN** un estado S con huella H, una copia profunda C de S y un
  comando válido, **WHEN** se aplica, **THEN** S es igual a C campo por campo, su huella
  sigue siendo H, el resultado es Aceptado con al menos un evento, y `contadorComandos`
  subió exactamente 1 (`contadorTirada` también, solo en Tirada). **GIVEN** un comando
  inválido de cada tipo, **THEN** el resultado es Rechazado con 0 eventos y ni el estado ni
  sus contadores cambian.
- **AC-2 (rechazo sin cambios)** — **GIVEN** un estado S en la fase de mover, **WHEN** el
  jugador del turno envía Mover de una pieza suya a una casilla ocupada dentro de su alcance, **THEN** el resultado es Rechazado con `DestinoOcupado`, la huella
  sigue siendo H y hay 0 eventos.
- **AC-3 (creación)** — **GIVEN** una config válida con la tabla de Catálogo T y castillos
  fijos, **WHEN** se llama CrearPartida, **THEN** el estado está En curso, sin tirada
  pendiente (el primer turno no tira, regla 2), y lleva la versión de reglas, el modo y la
  huella de T. Con una tabla de Catálogo vacía, `ConfigInvalida(F1)`; con un ataque de 1.001, `ConfigInvalida(F2)`; en los dos, no se crea ninguna partida. Orden (regla 2): tablas inválidas y F1 a la vez dan `ProbabilidadesInvalidas`; F9 y un castillo fuera del tablero, `ConfigInvalida(F9)`. Con el contexto de T ya preparado, la misma partida que con T.
- **AC-4 (lista cerrada)** — **GIVEN** una partida en curso, **WHEN** se envía un comando
  con un tipo cuyo valor numérico no está en la lista cerrada (por ejemplo 99), **THEN** se
  rechaza con `ComandoDesconocido`.
- **AC-5 (todo o nada)** — **GIVEN** un fixture con 3 casillas libres para reclutar y un
  tipo que recluta 4 piezas, **WHEN** se envía Reclutar de ese tipo, **THEN** se rechaza
  con el código de Reclutamiento (#10; `SinLugarParaReclutar`), 0 piezas
  nuevas, la huella sigue siendo H y hay 0 eventos. **GIVEN** 4 casillas libres, **THEN**
  se acepta con 4 piezas nuevas.
- **AC-6 (determinismo entre plataformas)** — **GIVEN** la misma config y la misma
  secuencia de 500 comandos con sus tiradas, y un archivo dorado guardado en el repositorio
  con la huella después de cada comando y los eventos en su forma canónica (regla 11),
  **WHEN** se reproduce en .NET sobre Windows y sobre Linux (en cada PR, BLOCKING) y en un
  build web de Unity (cada noche, ADVISORY), **THEN** cada corrida coincide byte a byte con
  el archivo dorado.
- **AC-7 (prohibiciones)** — **GIVEN** el `.csproj` de la librería compilado en CI con un
  analizador de APIs prohibidas configurado como error, **WHEN** se compila, **THEN** hay 0
  usos de los símbolos de la lista que mantiene el ADR. Como mínimo: coma flotante, reloj y
  tiempo, azar y `Guid`, `GetHashCode` y `HashCode`, cultura, reflexión, hilos y tareas,
  archivos, `UnityEngine`, `System.Linq`, y recorrer un `Dictionary` o un `HashSet`. Lo que
  un analizador de símbolos no detecta lo cubre un analizador propio, según el ADR.
  Además, el ensamblado no referencia a Unity, y un proyecto de prueba negativa con un uso
  de cada símbolo prohibido tiene que fallar con el diagnóstico esperado para cada uno.
- **AC-8 (azar inyectado)** — **GIVEN** que se espera una tirada con la tabla
  20/20/25/20/15, **WHEN** la autoridad envía Tirada(47), **THEN** sale la cara 3, el evento
  trae n = 47 y la tabla usada, y el estado deja de esperar tirada. En el estado anterior
  (esperando), un Mover del jugador del turno se rechaza con `EsperandoTirada`.
- **AC-9 (IDs)** — **GIVEN** que muere la pieza con ID 5, **WHEN** aparece una pieza nueva
  (reclutada o colocada por el fixture), **THEN** su ID es mayor que cualquier ID asignado
  antes, y un Mover del jugador del turno, en la fase de mover, con el ID 5 se rechaza con
  `PiezaInexistente`.
- **AC-10 (eventos)** — **GIVEN** un fixture con los atacantes A (daño 3) y B (daño 2)
  contra el objetivo T (vida 10, escudo 1), **WHEN** se envía Resolver, **THEN** hay un
  evento de daño sobre T con bruto 5, escudo 1, neto 4, vida restante 6 y combinado
  verdadero (valores confirmados por la Resolución, #9), y repetir la misma resolución
  desde el mismo estado da una lista idéntica.
- **AC-11 (vista previa = resolución)** — **GIVEN** ataques asignados, **WHEN** se pide la
  vista previa y después se envía Resolver, **THEN** las dos listas de eventos son idénticas
  y la vista previa no cambió la huella. **GIVEN** 0 ataques asignados, **THEN** las dos
  devuelven la misma lista, con `TurnoTerminado`. **GIVEN** la fase de mover, **THEN** las
  dos devuelven `FaseIncorrecta`; pedidas por el jugador sin turno, `NoEsTuTurno`; con la
  partida terminada, `PartidaTerminada`.
- **AC-12a (consultas puras)** — **GIVEN** un estado S, **WHEN** se llama 100 veces a cada
  una de MovimientosLegales, ObjetivosValidos, `EffectiveStats`,
  ProbabilidadesSiguienteTirada y `EvaluateMatchEnd`, **THEN** la huella no cambia y cada
  consulta devuelve siempre el mismo resultado.
- **AC-12b (estadísticas efectivas)** — **GIVEN** un fixture con un bono de turno de +2 en ataque y movimiento
  sobre una pieza, **WHEN** se llama `EffectiveStats`, **THEN** devuelve los valores del
  catálogo +2 en ataque y movimiento (el demoledor, solo en movimiento: #11, regla 7). El bono por posición (arquero sobre el muro: +2 de
  escudo y +2 de alcance máximo mientras `enMuro`) lo define Castillo y muros (#5) y lo
  cubren sus AC-9 y AC-22.
- **AC-13 (guardar y cargar)** — **GIVEN** una partida a la mitad de un fixture, **WHEN** se
  guarda y se carga, **THEN** la huella es igual y los 50 comandos siguientes del fixture
  dan los mismos eventos que sin haber guardado.
- **AC-14 (versión)** — **GIVEN** un registro de una versión de reglas no soportada, o de
  la misma versión con otra huella de Catálogo, **WHEN** se carga, **THEN** se rechaza con
  `VersionNoSoportada` y no se aplica ningún comando. **GIVEN** una partida en curso y un
  contexto con otro Catálogo, **WHEN** se aplica un comando válido, **THEN**
  `VersionNoSoportada` y la huella sigue siendo H.

**Fórmulas**

- **AC-15 (`cara_por_tirada`)** — **GIVEN** la tabla 20/20/25/20/15, **WHEN** llegan
  n = 0, 19, 20, 47 y 99, **THEN** salen las caras 1, 1, 2, 3 y 5. **GIVEN** la tabla
  0/50/0/50/0, **WHEN** llegan n = 0, 49 y 50, **THEN** salen 2, 2 y 4. En CrearPartida, las
  tablas que suman 99, que tienen un −1, que tienen 11 caras o que son
  [2147483647, 2147483647, 102], [101, −1] (suma 100 pero un término fuera de rango) o
  que tienen 1 cara se rechazan con `ProbabilidadesInvalidas`. Con la autoridad
  esperando tirada, n = −1 y n = 100 se rechazan con `TiradaInvalida`.
- **AC-16 (tirada sin espera)** —
  **GIVEN** una partida recién creada, que nace En curso sin esperar tirada, **WHEN** la
  autoridad envía Tirada(50), **THEN** `TiradaNoEsperada` y la huella no cambia.

**Casos borde**

- **AC-17** — **GIVEN** el jugador 0 y después el jugador 1, **WHEN** cada uno envía
  Tirada, `TurnTimeout` o `CerrarPartida`, **THEN** los seis casos se rechazan con
  `SoloAutoridad`.
- **AC-18** — **GIVEN** un Mover válido, **WHEN** se envía dos veces seguidas con el mismo
  `contadorEsperado`, **THEN** la segunda copia es Rechazado con `ComandoViejo`, la huella
  es la misma que después de la primera copia y en total hay un solo evento de movimiento.
- **AC-19** — **GIVEN** que se espera una tirada, **WHEN** se pide la vista previa, **THEN**
  se rechaza con `EsperandoTirada`.
- **AC-20** — **GIVEN** la pila de deshacer
  vacía, **WHEN** se envía Deshacer, **THEN** `NadaQueDeshacer`; **GIVEN** que ya se envió
  TerminarMovimiento con al menos un ataque posible (así la fase de atacar no se salta),
  **WHEN** se envía Deshacer, **THEN** `FaseIncorrecta`.
- **AC-21** — **GIVEN** un flujo guardado (forma canónica más los 8 bytes de su huella,
  regla 11), **WHEN** para cada posición de byte del flujo, también la huella final, y cada
  uno de sus 8 bits se invierte ese bit y se carga, **THEN** ninguna carga se acepta: el
  código es `VersionNoSoportada` si el bit cae en la versión o en la huella del Catálogo,
  y `EstadoInvalido` en cualquier otro byte (por huella o por dominio). **WHEN** el flujo está
  truncado, tiene bytes de más o un prefijo de cantidad de 2.147.483.647, **THEN** se
  rechaza con `EstadoInvalido` y `GC.GetAllocatedBytesForCurrentThread()` sube menos de
  64 KB durante la carga (en .NET); lo mismo con un prefijo de 1.000.000. **GIVEN** un
  test de tabla generado desde la tabla de dominios de la regla 11, con un caso por campo
  que sale de su dominio y uno por invariante entre campos que se rompe (también un bono de
  2.147.483.600, un ataque a un ID inexistente, una Terminada con Tablas y ganador, y una
  ReyMuerto cuyo ganador no es el dueño del Rey vivo), cada flujo con su **huella final
  recalculada**, **WHEN** se carga y se resincroniza, **THEN** las dos devuelven
  `EstadoInvalido`, salvo los campos de versión y huella del Catálogo, que dan `VersionNoSoportada`. Un test falla si un campo de la tabla no tiene caso.
- **AC-22** — **GIVEN** un fixture en el que los
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
  repositorio: 2 × 61 = 122 piezas (los `topeVivas` del Catálogo, #2 F6), los 2 × 16 tramos de muro, y
  61 ataques válidos asignados por el jugador del turno (Alcance, #7, F3),
  **WHEN** se pide la vista previa 100 veces después de 20 corridas de calentamiento,
  **THEN** el percentil 95 es de 5 ms o menos en la máquina, el navegador y el build de
  referencia (razón en *Tuning Knobs*).
- **AC-25 (comando rápido)** — **GIVEN** la variante del
  fixture de AC-24 en la fase de mover (las mismas piezas y muros, 0 ataques asignados),
  **WHEN** se aplica un Mover válido 100 veces, cada vez sobre una copia nueva del fixture
  (la copia no se mide), después de 20 de calentamiento, **THEN** las 100 devuelven
  Aceptado y el percentil 95 es de 1 ms o menos en la misma referencia (en el navegador,
  con reloj grueso, el método lo fija el ADR).
- **AC-26 (Resincronizar)** — **GIVEN** un cliente en modo online con el mismo
  `contadorComandos` que el servidor y otra huella, **WHEN** se aplica Resincronizar con el
  estado del servidor y su huella, **THEN** el estado del cliente es
  igual al del servidor (también la pila de deshacer y la tirada pendiente), las huellas
  coinciden y hay un solo evento `Resincronizado` con motivo Divergencia, la huella vieja y
  la nueva. **WHEN** la huella enviada no es la del estado enviado, **THEN** se rechaza con
  `EstadoInvalido` y el estado del cliente no cambia.

- **AC-27 (combinado)** — **GIVEN** un fixture de resolución, **WHEN** dos atacantes golpean
  un mismo objetivo, **THEN** su evento de daño es combinado; con un solo atacante no lo es;
  con dos atacantes contra dos objetivos distintos ninguno lo es; con dos atacantes cuyo
  daño absorbe entero el escudo, el neto es 0 y es combinado.
- **AC-28 (config de castillos)** — **GIVEN** una config con un castillo fuera del tablero,
  o con dos castillos superpuestos, **WHEN** se llama CrearPartida, **THEN** se devuelve
  `ConfigInvalida` y no se crea partida.
- **AC-29 (orden de los rechazos)** — Test de tabla: **GIVEN**, para cada par de filas
  de la regla 4 que **un mismo comando** puede fallar a la vez (no hay par entre filas de
  comandos distintos, como 8 y 9), un fixture que falla las dos, **WHEN** se envía el
  comando, **THEN** sale el código de la fila de número menor. Incluye como mínimo:
  - Con tirada pendiente, la autoridad envía Tirada con un contador viejo y n = 150:
    `TiradaVieja`.
  - Un jugador envía Tirada(150) con la partida terminada: `PartidaTerminada`. Con la
    partida en curso: `SoloAutoridad`.
  - La autoridad envía Tirada(150) sin tirada pendiente: `TiradaNoEsperada`. Con tirada
    pendiente: `TiradaInvalida`.
  - El jugador que no tiene el turno envía Mover mientras se espera tirada:
    `EsperandoTirada`.
  - Un comando de tipo desconocido con la partida terminada: `PartidaTerminada`.
  - Un estado de una versión no soportada, ya terminado (armado con el constructor de
    pruebas, porque la carga lo rechaza): `VersionNoSoportada`.
  - La autoridad envía Mover mientras se espera tirada: `SoloJugador`.
  - La autoridad envía `CerrarPartida(Tablas, 0, vacío)` mientras se espera tirada:
    `CierreInvalido` (se salta la fila 5).
  - El jugador sin turno envía Mover con un `contadorEsperado` viejo: `ComandoViejo`.
  - El jugador sin turno envía Mover con un ID inexistente: `NoEsTuTurno`.
  - El jugador del turno mueve una pieza del rival en la fase equivocada: `NoEsTuPieza`.
- **AC-30 (huella)** — **GIVEN** dos secuencias con la misma cantidad de comandos que
  mueven las mismas piezas a las mismas casillas en distinto orden, **WHEN** llegan al
  comienzo del turno siguiente (pila de deshacer vacía), **THEN** las dos huellas son
  iguales. **GIVEN** Mover, Deshacer, Mover frente a un solo Mover al mismo destino,
  **THEN** las huellas son distintas (`contadorComandos` difiere). Que cada campo cambie la
  huella lo prueba AC-45. **GIVEN** 3 estados de fixture con su huella guardada en el
  repositorio, **WHEN** se calcula la huella en .NET (BLOCKING) y en el build web
  (ADVISORY), **THEN** coincide con la guardada.
- **AC-31 (QuitarAtaque, Cubrir y QuitarCobertura)** —
  **GIVEN** la fase de movimiento y una pieza viva del jugador del turno (un escudero para
  Cubrir, con un aliado vecino), **WHEN** ese jugador envía QuitarAtaque, Cubrir o QuitarCobertura,
  **THEN** se rechazan con `FaseIncorrecta` (no con `ComandoDesconocido`).
- **AC-32 (emisor sellado)** (DEFERRED hasta el Servidor, #28) — **GIVEN** una partida
  online en el servidor, **WHEN** por la conexión del jugador 1 llega una Tirada cuyo
  mensaje dice venir de la autoridad, **THEN** el servidor la aplica con emisor jugador 1 y
  se rechaza con `SoloAutoridad`. Lo que sí se prueba en la librería son AC-17 y AC-39.
- **AC-33a (vista previa sin memoria, .NET)** — **GIVEN** el fixture de AC-24 y 10 vistas
  previas de calentamiento, **WHEN** se piden 100 vistas previas más, **THEN** la
  diferencia de `GC.GetAllocatedBytesForCurrentThread()` es 0. BLOCKING.
- **AC-33b (vista previa sin memoria, Unity)** — Lo mismo en un test de PlayMode con
  `ProfilerRecorder`, que cuenta 0 reservas de memoria del GC en esas 100. ADVISORY: corre
  en el editor, no en IL2CPP.
- **AC-34 (Resolver no consume tiradas)** —
  **GIVEN** ataques asignados en un turno cuyo
  siguiente turno necesita tirada, **WHEN** se envía Resolver, **THEN** los eventos de daño
  son iguales a los de la vista previa, el estado resultante queda esperando tirada y
  ningún evento de esa resolución trae una tirada.
- **AC-35 (timeout esperando tirada)** — **GIVEN** que se espera una tirada, **WHEN** la
  autoridad envía `TurnTimeout`, **THEN** se rechaza con `EsperandoTirada` y la huella no
  cambia.
- **AC-36** — Retirado el 2026-11-25 (sin sorteo); lo cubren AC-54 y el AC-4 del Tablero.

- **AC-37 (timeout viejo)** — **GIVEN** la etapa de mover del turno T con al menos un
  ataque posible, **WHEN** el jugador
  envía TerminarMovimiento y después la autoridad envía `TurnTimeout(T, mover)`, **THEN**
  se rechaza con `TimeoutObsoleto`, la huella no cambia y los ataques no se resuelven.
  **GIVEN** un fixture en el que el turno T + 1 necesita tirada y que se aceptó Resolver en
  el turno T, **WHEN** llega `TurnTimeout(T, atacar)`, **THEN** se rechaza con
  `EsperandoTirada`; después de la tirada del turno T + 1, con `TimeoutObsoleto`. **GIVEN**
  ese fixture y que se aceptó `TurnTimeout(T, atacar)`, **WHEN** el jugador envía Resolver
  con el `contadorEsperado` de antes del timeout, **THEN** `EsperandoTirada`; después de la
  tirada, `ComandoViejo`. En cada uno de los dos órdenes (Resolver antes del timeout, y
  timeout antes de Resolver), reproducir los comandos aceptados
  del registro de la autoridad da, comando por comando, las mismas huellas que esa partida.
- **AC-38 (qué hace el timeout)** — **GIVEN** un Reclutar obligatorio de Llamada pendiente
  y el primer tipo de esa cara (orden del Catálogo) en su tope de piezas vivas (estado del
  constructor), **WHEN** llega
  `TurnTimeout(T, mover)` con al menos un ataque posible después, **THEN** se recluta el
  siguiente tipo disponible de la cara, su evento trae `automatico`, la pila de deshacer queda cerrada y la fase
  pasa a atacar. **Defensa** (inalcanzable jugando: la tabla efectiva pone en 0 toda cara
  sin tipo disponible): **GIVEN** un estado del constructor, fuera de `EsValido` a propósito,
  esperando una tirada cuya tabla guardada da Llamada con p > 0 sin ningún tipo de Llamada
  disponible, **WHEN** `Tirada(n)` cae en Llamada, **THEN** salen `ReclutaPerdido`, la fase es mover y
  TerminarMovimiento se acepta. **GIVEN** la
  etapa de atacar con 0 ataques y una cobertura asignada, **WHEN** llega
  `TurnTimeout(T, atacar)`, **THEN** el turno termina con `TurnoTerminado` y la cobertura
  sigue en el estado.
- **AC-39 (comandos de autoridad)** — **GIVEN** una tirada pendiente, **WHEN** la autoridad
  envía Tirada(50), **THEN** se acepta (nunca `NoEsTuTurno`). **WHEN** la autoridad envía
  Mover, **THEN** `SoloJugador`. **GIVEN** la API pública, **THEN** el emisor es un
  parámetro de Aplicar y no hay ningún tipo de comando con un campo de emisor (test de
  forma de la API).
- **AC-40 (tabla efectiva)** (tablas base del Modificador, #11) — **GIVEN** la tabla 15/20/25/25/15 y 2 muros rotos, **WHEN** se
  arma la tabla efectiva (`TablaEfectiva` sobre esa base dada), **THEN** es 15/45/0/0/40. **GIVEN** 20/25/15/20/20 con Llamada
  agotada, **THEN** 20/32/0/20/28. **GIVEN** cada tabla base del Modificador y cada
  combinación de caras de reclutar liberadas, **THEN** la tabla efectiva suma 100 y n = 99
  siempre da una cara. **GIVEN** una base con Grito y Arremetida en 0, **THEN** igual suma
  100. Los casos anteriores usan 13 piezas sin el Rey. **GIVEN** 12 piezas sin el Rey,
  **THEN** el primero da 15/85/0/0/0 y 20/20/20/20/20 sin nada liberado da 20/40/20/20/0;
  con 13, queda 20/20/20/20/20; con `limite_normal` 5, 5 piezas bloquean y 6 no. El Rey no cuenta en ninguno. Las caras SinLugar (tope o
  casillas) las prueban AC-18 y AC-19 de Reclutamiento (#10).
- **AC-41 (orden de los IDs)** — **GIVEN** un despliegue inicial con castillos fijos,
  **WHEN** se colocan las piezas, **THEN** los IDs coinciden con un
  archivo dorado guardado en el repositorio (jugador 0 primero, después tipo del Catálogo,
  después fila y columna). **GIVEN** un reclutamiento de 4 piezas, **THEN** sus IDs crecen
  por casilla (fila, columna).
- **AC-42 (resolución simultánea)** (valores confirmados por la Resolución, #9) —
  **GIVEN**, sin bonos, que el jugador 0 asignó a su soldado A (ID 3, del despliegue, con 3 de vida
  por daño anterior del fixture) contra E, soldado enemigo con 3 de vida fuera del área, y a
  su catapulta reclutada B (ID mayor que 26) contra un enemigo vecino de A, cuya área le hace
  a A 6 − 3 = 3 de neto, **WHEN** se envía Resolver, **THEN** A muere e igual golpea a E,
  los `AtaqueEjecutado` salen en orden de ID ascendente (A antes que B), y el evento de E
  trae 8 − 3 = 5 de neto, 3 de vida perdida y la muerte.
- **AC-43 (límite y desbordamiento)** — **GIVEN** un estado armado con el constructor de
  pruebas que cumple `EsValido`, En curso, con `contadorComandos` = C_MAX − 2, **WHEN** se
  aplica, en una copia por tipo, cada comando que ahí sería Aceptado (también un Mover en
  modo local sin timer), **THEN** se acepta, `contadorComandos` queda en C_MAX − 1, la
  partida termina en Tablas sin ganador con su `PartidaTerminada` (salvo `Rendirse`,
  `CerrarPartida`, o un Resolver o `TurnTimeout` de atacar que ya la terminan con su propio motivo, que se conserva) y
  el estado cumple `EsValido`; un comando siguiente da `PartidaTerminada`.
  **GIVEN** la pila de deshacer con 256 entradas (constructor), **WHEN** un Mover válido, **THEN**
  `LimitePartida`. **GIVEN** un estado armado con el constructor de pruebas (la carga lo
  rechazaría) con `contadorComandos` en 2.147.483.647 (valor leído en ejecución), **WHEN**
  se aplica `Rendirse`, que se salta la fila 15, **THEN** ErrorInterno, la huella del
  estado de entrada sigue siendo H y ningún campo dio la vuelta. Corre en .NET y en el
  editor de Unity (BLOCKING); la plataforma web es del ADR.
- **AC-44 (Resincronizar, rechazos y orden)** — **WHEN** se aplica Resincronizar:
  - en modo local: `SoloOnline`;
  - con otra versión, o con una versión soportada pero distinta de la de `confirmado`, o
    con una huella de Catálogo distinta de la del contexto: `VersionNoSoportada`;
  - con otro modo, otras tablas base, otro `limite_normal`, otra `vida_tramo`, otra `tablaReclutas`, otro bono del Grito u otra posición de los castillos (huella correcta):
    `EstadoInvalido`;
  - con otro id de partida: `OtraPartida`;
  - con un `contadorComandos` menor que el confirmado: `EstadoViejo`;
  - desde un confirmado Terminada con la misma huella que la enviada: `PartidaTerminada`.

  En todos esos casos el estado del cliente no cambia. Test de tabla del orden: con modo
  local y otra versión a la vez, `SoloOnline`; con huella dañada y otro id de partida,
  `EstadoInvalido`; con otro id y un contador menor, `OtraPartida`. **WHEN** el contador
  del servidor es igual al confirmado y la huella distinta, **THEN** se acepta. **WHEN** el
  estado del servidor está Terminada y el del cliente no, **THEN** se acepta. **WHEN** el
  cliente está Terminada con otro ganador que el servidor, **THEN** se acepta y queda el
  ganador del servidor. **WHEN** el contador del servidor es mayor, **THEN** el evento
  `Resincronizado` trae motivo Reconexion; con el mismo contador y otra huella, siempre
  Divergencia (la API no tiene parámetro de motivo); con el mismo contador y la misma huella,
  sin Terminada, Reconexion.
- **AC-45 (huella exhaustiva)** — **GIVEN** la lista de campos del estado (obtenida en el
  test, no en la librería), **WHEN** corre el test de exhaustividad, **THEN** falla si un
  campo no cambia la huella al cambiarlo. **GIVEN** dos estados que solo difieren en cómo
  se reparten los elementos entre dos colecciones consecutivas (por ejemplo, una pieza
  menos y más tramos de muro), **THEN** sus flujos canónicos y sus huellas son distintos.
- **AC-46 (castillos fuera del tablero)** — **GIVEN** una config con un castillo que se sale
  del tablero (o con dos castillos que se superponen, o con `limite_normal` 0 o 38, o con `vida_tramo` 0 o 1.001, o con un bono del Grito en −1 u
  11 o los dos en 0, o con una cota de pila con el bono de movimiento mayor que 256), **WHEN**
  se llama CrearPartida, **THEN** `ConfigInvalida` y no se crea partida; con `limite_normal`
  1 y 37 se acepta, con `vida_tramo` 1 y 1.000 también, y con bonos 0 y 10.
- **AC-47 (Rendirse)** — **GIVEN** una partida en curso, **WHEN** el jugador sin turno envía
  `Rendirse`, también mientras se espera una tirada, **THEN** la partida termina, gana el
  otro jugador, el motivo es Rendicion y no queda tirada pendiente. **WHEN** lo envía el
  jugador del turno, **THEN** gana el otro. **WHEN** la autoridad envía `Rendirse`,
  **THEN** `SoloJugador`. **GIVEN** una partida Terminada, **WHEN** un jugador envía
  `Rendirse`, **THEN** `PartidaTerminada`.
- **AC-48 (combinado, fuego amigo)** — **GIVEN** un fixture de resolución del jugador del
  turno, **WHEN** el área de su catapulta y otra pieza suya golpean a una pieza suya,
  **THEN** su evento de daño no es combinado y trae 0 atacantes rivales. **WHEN** el área
  de su catapulta golpea dos veces el mismo objetivo enemigo (solo pasa por cobertura: el
  centro desviado y el área sobre el escudero, #9), **THEN** no es combinado.
  **WHEN** dos de sus piezas atacan a un enemigo cubierto por un escudero del rival,
  **THEN** el daño lo recibe el escudero, su evento es combinado y trae 2 atacantes
  rivales.

- **AC-49 (oráculos escritos a mano)** — **GIVEN** un estado mínimo válido armado con el
  constructor de pruebas (Terminada con Tablas y sin ganador; castillos fijos con sus
  2 × 16 tramos; solo los 2 Reyes), cuyos bytes canónicos están escritos a mano en el test
  (little-endian, prefijos de cantidad, bytes de presencia, huella final) y con
  `contadorComandos` en 1.000.000 = C_MAX (bytes 40 42 0F 00, válido por estar Terminada), **WHEN** se
  serializa y se vuelve a cargar, **THEN** el flujo coincide byte a byte con esos bytes y
  ninguna de las dos operaciones da ErrorInterno. **GIVEN** los vectores de prueba publicados del algoritmo de huella
  que elija el ADR, **THEN** la huella de cada vector coincide. **GIVEN** un despliegue de
  2 piezas por jugador, **THEN** sus IDs coinciden con una lista escrita a mano.
- **AC-50 (CerrarPartida)** — **GIVEN** una partida en curso, también esperando tirada,
  **WHEN** la autoridad envía `CerrarPartida(Abandono, 1, vacío)`, **THEN** la partida termina con
  ganador 1, motivo Abandono y sin tirada pendiente. **WHEN** envía
  `CerrarPartida(Tablas, vacío, vacío)`, **THEN** termina sin ganador con motivo Tablas. **WHEN**
  envía `CerrarPartida` con Tablas y ganador 0, con un motivo fuera de la lista, con Abandono
  y ganador vacío, 2 o −1, o con ReyMuerto y ganador 0 (todos con `turno` vacío), **THEN** `CierreInvalido`. **GIVEN** una partida Terminada por
  `CerrarPartida`, **THEN** Cargar(Serializar(estado)) la acepta igual campo por campo.
  **WHEN** un jugador la envía, **THEN** `SoloAutoridad`.
  (Fuera de RelojAgotado, `turno` va vacío; con un turno, `CierreInvalido`.)
  **RelojAgotado** — **GIVEN** el turno T del jugador 0 en reclutar, mover o atacar, **WHEN**
  la autoridad envía `CerrarPartida(RelojAgotado, 1, T)`, **THEN** termina con ganador 1;
  piezas, vidas, muros, pila, ataques asignados y `vidaQuitada` quedan iguales campo por
  campo a los de antes, y no hay eventos `AtaqueEjecutado` ni `DanoRecibido`; con ganador 0
  o vacío, o con turno T − 1, `CierreInvalido`. **GIVEN** que se espera la tirada del turno
  T + 1 (del jugador 1), **WHEN** llega `CerrarPartida(RelojAgotado, 1, T)` o
  `CerrarPartida(RelojAgotado, 0, T + 1)`, **THEN** `CierreInvalido`, y el segundo se acepta
  después de esa tirada;
  **GIVEN** el turno T + 2, otra vez del jugador 0, **THEN** también `CierreInvalido` (turno
  viejo) y la huella no cambia.
- **AC-51 (ComandoViejo)** — **GIVEN** dos movimientos hechos, **WHEN** llegan dos copias
  de Deshacer con el mismo `contadorEsperado`, **THEN** se deshace un solo movimiento y la
  segunda copia se rechaza con `ComandoViejo`. **GIVEN** que el jugador 0 vuelve a estar en
  la fase de atacar en el turno T + 2, **WHEN** llega un Resolver suyo con el contador del
  turno T, **THEN** `ComandoViejo` y la huella no cambia. **WHEN** después el jugador
  envía `Rendirse` (que no lleva contador), **THEN** se acepta.
- **AC-52 (papeles de las caras)** — **WHEN** se llama CrearPartida con una tabla sin cara
  de Grito, sin cara de Arremetida, con dos de Grito, con dos de Arremetida o con menos
  papeles que caras, **THEN** `ProbabilidadesInvalidas` y no se crea partida.
  **WHEN** la tabla tiene papeles inválidos y además un castillo fuera del tablero,
  **THEN** `ProbabilidadesInvalidas` (gana sobre `ConfigInvalida`). **GIVEN** cada tabla
  aceptada y cada subconjunto de caras de reclutar liberables, **THEN** la tabla efectiva
  suma 100.
- **AC-53 (probabilidades consultables)** — **GIVEN** un estado del constructor en la fase
  de atacar del jugador 0 (28 piezas contra 13 sin el Rey, un tramo roto del jugador 1), **WHEN** se llama `ProbabilidadesSiguienteTirada` para el jugador 1, **THEN**
  devuelve la tabla efectiva que usaría su próxima tirada (con la base 15/20/25/25/15,
  esa misma, con 13 piezas sin el Rey); después de un Resolver que le rompe el segundo muro
  al jugador 1 sin bajarlo de 13 piezas, devuelve
  15/45/0/0/40; cuando la tirada del
  jugador 1 queda pendiente, la consulta coincide con la tabla guardada en el estado. Con 12
  piezas sin el Rey y el segundo muro roto, 15/85/0/0/0.
- **AC-54 (creación sin tirada y orden total)** — **GIVEN** una config con castillos fijos,
  **WHEN** se llama CrearPartida, **THEN** la partida nace En curso sin esperar tirada.
  **GIVEN** dos estados iguales campo por campo armados con el constructor de pruebas
  insertando las piezas en distinto orden, con dos objetivos del mismo daño y la misma vida, **WHEN** se resuelve en
  cada uno, **THEN** las dos listas de eventos son idénticas (desempate por ID).

- **AC-55 (Tirada vieja)** — **GIVEN** que se aceptó Tirada(37) con `contadorTirada` c y
  el estado vuelve a esperar tirada, **WHEN** llega otra copia de Tirada(37) con el
  contador c, **THEN** `TiradaVieja` y la huella no cambia; con el contador c + 1, se acepta.
- **AC-56 (fuzz de robustez)** — **Oráculo de dominios:** la tabla de dominios de la regla
  11 escrita de nuevo en el test (la misma de AC-21), nunca `EsValido`. **GIVEN** un fixture
  en cada fase y un generador propio de la suite (xorshift) con semilla fija que elige el
  tipo de comando entre los 13 y cada campo (también el emisor), con la misma
  probabilidad, entre un valor válido de su dominio (un ID existente, una casilla del
  tablero, n de 0 a 99, el contador correcto), un extremo (`int.MinValue`, `int.MaxValue`,
  −1, 100) y un int32 uniforme, **WHEN** se aplican 10.000 comandos, cada uno sobre una
  copia nueva del fixture, **THEN** cada resultado es Aceptado o Rechazado, nunca
  ErrorInterno; todo Rechazado deja la huella igual; y todo Aceptado cumple el oráculo.
  **GIVEN** 1.000 configs válidas con uno o dos campos mutados, **WHEN** se llama
  CrearPartida, **THEN** ningún ErrorInterno. **GIVEN** 1.000 estados válidos en modo online
  con uno o dos campos mutados (dentro o fuera de su dominio; se descartan las mutaciones que
  no cambian nada) y la huella final recalculada, **WHEN** se cargan y se resincronizan
  (`confirmado` y `huellaDeclarada` como en *Archivos dorados y oráculos*), **THEN** cada uno
  da el primer código del orden de la regla 1 (Cargar) o de la regla 14 (Resincronizar) que
  su mutación dispara (`VersionNoSoportada`, `EstadoInvalido`, `OtraPartida`, `EstadoViejo`;
  `SoloOnline` y `PartidaTerminada` no salen así y los prueba AC-44), se aceptan exactamente
  los que no disparan ninguno, y sobre los aceptados el fuzz de comandos tampoco da
  ErrorInterno. Las cuotas de Aceptados por tipo son de AC-62.
- **AC-57 (fin automático al empezar)** — **GIVEN** un turno en el
  que, al cerrar el movimiento, ninguna pieza tiene objetivo ni hay escudero con aliado,
  **WHEN** se envía TerminarMovimiento, **THEN** sus eventos traen `FaseSaltada` y
  `TurnoTerminado`, en ese orden. **GIVEN** un estado del constructor en el que, al empezar
  la fase de mover, ninguna pieza tiene un movimiento legal y alguna tiene objetivo, **THEN** el comando que la abrió trae `FaseSaltada` y la fase
  es atacar. **GIVEN** la fase de mover ya empezada, **WHEN** el jugador mueve su
  última pieza con movimientos y queda sin ninguna acción, **THEN** la fase sigue abierta
  hasta TerminarMovimiento o `TurnTimeout`.
- **AC-58 (cobertura de la suite)** — **GIVEN** la suite completa, en la que cada caso
  registra el código que afirma a través del mismo helper que lo comprueba (nunca una
  declaración aparte), **THEN** cada fila 1 a 13 y 15 de la regla 4,
  cada código de Resincronizar y de fuera de Aplicar, y cada comando de la regla 3 tienen
  al menos un caso Rechazado y, si el comando puede aceptarse, uno Aceptado; un test
  compara el registro con los códigos del enum y falla si falta alguno. La fila 14 también, con
  los códigos de #3 a #11.
- **AC-59 (dorados versionados)** — **GIVEN** un archivo dorado guardado con la versión de
  reglas V, **WHEN** CI lo encuentra cambiado y la versión de reglas sigue en V, **THEN** CI
  falla.

- **AC-60 (emisor fuera de rango)** — **GIVEN** una partida en curso, **WHEN** se aplica
  cualquier comando con el emisor 2 o −1 (también `Rendirse` y Tirada), **THEN**
  `EmisorInvalido` y la huella sigue siendo H; con la partida terminada,
  `PartidaTerminada`.
- **AC-61 (contexto calculado)** — **GIVEN** dos Catálogos que difieren en una sola
  estadística, **WHEN** se llama PrepararContexto con cada uno, **THEN** las huellas son
  distintas y la API no acepta una huella de afuera (test de forma). **GIVEN** un estado
  creado con el Catálogo X, **WHEN** se carga, o se aplica un comando, con el contexto del
  Catálogo Y, **THEN** `VersionNoSoportada`.

- **AC-62 (clausura generativa)** — **GIVEN** la matriz de clausura de la regla 11, el
  oráculo de dominios de AC-56 y un generador con semilla fija que, desde CrearPartida con
  castillos fijos, elige en cada paso un comando **legal** (sacado de
  `MovimientosLegales`, `ObjetivosValidos` y los comandos aceptables de la fase, también
  Tirada, `TurnTimeout`, `Rendirse` y `CerrarPartida`), **WHEN** se juegan 200 partidas
  hasta su fin o hasta 2.000 comandos, más estas secuencias dirigidas: `Rendirse` y
  `CerrarPartida(Tablas)` mientras se espera una tirada; un recluta automático por `TurnTimeout`; y, con el
  constructor de pruebas, una Tirada que saca Distracción a un jugador sin objetivos ni escudero con aliado (atacar
  se salta en cascada, caducan las coberturas del rival y el turno siguiente queda esperando tirada) y
  un estado con `contadorComandos` = C_MAX − 2 que recibe un Mover (cierre en Tablas), **THEN** después de
  cada Aceptado el estado cumple el oráculo y `Cargar(Serializar(S))` es igual a S campo
  por campo; cada fila de la matriz se ejercita al menos una vez (un test falla si una
  queda sin caso); y cada tipo de comando se acepta al menos 10 veces.

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| Detalle técnico: `netstandard2.1`, C# 9, `LangVersion` explícito, polyfill `internal` de `IsExternalInit`; dos `.csproj` con nombre distinto del de Unity (librería con analizador; tests con `Stopwatch`, sin él), mismos `.cs` que el asmdef con `noEngineReferences` y la misma opción de desborde en `csc.rsp` y en el `.csproj`, sin `checked`; mecanismo sin excepciones de la regla 5; huella de 64 bits (FNV-1a o xxHash64), little-endian, con vectores de prueba y sensible a un solo byte (AC-21); decodificador estricto (topes por prefijo, presencia 0/1, orden, IDs únicos, re-codificar da el mismo flujo); firma de Aplicar con destino y buffers (destino indefinido tras Rechazado o ErrorInterno; búfer de la vista previa de quien llama); símbolos prohibidos y analizador propio de AC-7 (`decimal`, `BitConverter`, `Enum.ToString`, cultura, estáticos mutables, float/double, `Environment`, `Dictionary`/`HashSet` enteros); referencia de rendimiento (máquina, navegador, build), harness web nocturno con el dorado embebido, patas web de AC-24/25, 0-GC en IL2CPP/WebGL con el método de AC-33b, calentamiento de AC-33a con JIT escalonado; AC-6 en Mono y en IL2CPP | ADR de la librería (`/architecture-decision`) | Antes de escribir código del MVP |
| Época del servidor: un reinicio que restaura un estado con menor `contadorComandos` hoy da `EstadoViejo` sin salida; si hace falta un campo `epoca`, el ADR lo reserva en la forma canónica | Servidor (#28) y ADR | Antes de lanzar el online |
| Política ante ErrorInterno en una partida online (cierre, recuperación) y registro canónico de repeticiones (el del servidor, con emisor) | Servidor (#28) y Registro (#26) | Antes de lanzar el online |
| Cómo se conservan las versiones viejas de la librería para reproducir repeticiones | ADR y Registro de partida (#26) | Antes de la Fase 2b |
| Compromiso verificable de las tiradas online (semilla comprometida al crear la partida) | Servidor de partida (#28) | Antes de lanzar el online |
| Reconexión, modelo de red del cliente y reemisión de pendientes descartados, cada cuánto se comparan huellas, límite de pedidos de estado completo, y qué pasa si la autoridad no envía la tirada | Servidor de partida (#28) y Timer (#12) | Antes de lanzar el online |
| Cuándo una desconexión es abandono (`CerrarPartida` con Abandono) | Servidor de partida (#28) | Antes de lanzar el online |
| Borde con la red y la presentación (séptima pasada): eventos de un comando predicho antes de confirmar; varios comandos sin confirmar frente a `contadorEsperado`; relé con emisor, en orden y sin huecos; primer comando fallido de una cascada; tope de comandos por turno | Servidor (#28) para la predicción, la cola y el relé; Vista previa (#16) para presentar eventos | Antes de lanzar el online |
| Resincronizar: ¿exigir que `contadorTirada`, `numeroTurno` y próximo ID no bajen (sin trabar la corrección de una divergencia)?, código para un estado idéntico, `ComandoAdelantado` frente a `ComandoViejo`, `Rendirse` sin id de partida (id asignado por el servidor y único), `TurnTimeout` y `CerrarPartida` sin contador ante un reenvío reordenado | Servidor (#28) y ADR | Antes de lanzar el online |
| Juego y presentación: el recluta automático del timeout no se ve antes; falta un AC de fuego amigo que mata al Rey propio; aviso de que con RelojAgotado los ataques asignados no se resuelven y listado en la pantalla final; mostrar `FaseSaltada` y un turno vaciado en cascada; dueño de la pantalla de Tablas; Rendirse solo con desventaja puede empujar a abandonar | Máquina (#3), Reclutamiento (#10), Vista previa (#16), Ficha y HUD (#15); Rendirse, #28 | En sus GDD |
| QA: generar AC-29 desde el enum de filas (todos los pares); mutante plantado en AC-45; AC-54 con un constructor que conserve el orden de inserción; AC-59 frente a la regeneración de dorados mientras las reglas son PROVISIONALES | qa-lead | Antes de escribir los tests |
