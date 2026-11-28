# Métricas locales de partida

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: jugador + Claude (`/design-system`, lentes de systems-designer, analytics-engineer, gameplay-programmer, engine-programmer, creative-director y qa-lead)
> **Last Updated**: 2026-11-26
> **Last Verified**: 2026-11-26
> **Implements Pillar**: 4 Duelo corto (compuerta de duración); 3c Siempre hay vuelta, con límites (umbral `W`); 2 La fuerza está en combinar (medición de combinados); 1 Planificar y cosechar (tiradas perdidas, regla 14)
> **Creative Director Review (CD-GDD-ALIGN)**: REVISED 2026-11-26 — CONCERNS en Detailed Design, regla 14: la hipótesis central del MVP («decidido por el plan y no por el dado», pilar 1) no tenía medida. Se sumó la fila de tiradas perdidas. Resto: cada sección sirve a los pilares 4, 3c y 2; la compuerta (regla 12) cumple la promesa de la Player Fantasy A y ninguna regla contradice un pilar ni un anti-pilar
> **Design Review**: pendiente de `/design-review` en una sesión nueva

## Summary

Métricas locales de partida escucha los eventos que el juego ya emite y guarda, por cada partida local, un registro en el dispositivo que solo sale de ahí si alguien lo exporta a mano. Una herramienta aparte lee esos registros y dice si la compuerta del arte está abierta (3 partidas seguidas que terminan en el tablero en 25 minutos o menos, con timer y contra un humano), cuánto duran partidas y turnos, si el que va adelante gana 2 de cada 3 y cuánto se combina. Existe para que los pilares 4, 3c y 2 se comprueben con partidas medidas y no con impresiones. No cambia ninguna regla ni ninguna partida.

> **Quick reference** — Layer: `Feature` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Catálogo (#2), Máquina de turno (#3), Resolución (#9), Reclutamiento (#10), Modificador (#11), Timer de turno (#12)`; blandas: `#4–#8, #13–#15, #17`

## Overview

**Métricas locales de partida** es el servicio de cliente, **fuera del núcleo puro de C#**, que escucha los eventos que ya emiten el núcleo, el Timer y la presentación y escribe, por cada partida local, un **registro de métricas** en el dispositivo. **Nada se envía:** el registro sale del equipo solo cuando alguien lo exporta a mano. Define el **esquema de eventos** del juego (qué se guarda, con qué campos y en qué orden, con una versión propia), el **resumen por turno y por partida**, y la **regla de la compuerta del arte**: qué partida cuenta, cuál pasa y cuándo la racha de 3 se abre. Una **herramienta de informe aparte** (en `tools/`, fuera del juego) lee los registros exportados y calcula lo que los demás GDD le dejaron a medir: la racha de la compuerta, la duración de partida y de turno (Máquina, D1), `N_base`, los `Δturnos` de Reclutamiento (F6) y Modificador (F4), el umbral `W` del pilar 3c (Modificador, F5), los combinados del pilar 2 y las preguntas de playtest abiertas en otros GDD.

El núcleo no sabe que este sistema existe (índice, frontera del núcleo): Métricas solo **lee** eventos y consultas; no manda comandos, no cambia el estado ni la huella, y si falla la partida sigue igual. El jugador no lo usa ni lo ve durante el turno; lo que siente es el efecto: un duelo que se mantiene corto porque las reglas se ajustan con partidas medidas, no con impresiones. Sin este sistema, el pilar 4 («nunca pasa de 25») y el umbral del pilar 3c («el que tiene más piezas gana 2 de cada 3») no tendrían con qué comprobarse, y la compuerta que frena la producción de arte no tendría con qué abrirse.

*Qué no es:* no es el Registro de partida (#26, Alpha), que guarda los comandos para reproducir una partida; no es la Analítica online (#32, Alpha), que envía y agrega datos con consentimiento; no decide ninguna regla del juego ni corre dentro de `Aplicar`. Cómo se guarda el archivo en el navegador y cómo se descarga es implementación → **becomes an ADR**.

## Player Fantasy

**Encuadre: indirecto.** El jugador nunca usa este sistema; siente lo que habilita. Hay dos destinatarios y dos momentos (elegidos por el jugador, 2026-11-26, entre tres candidatas de la lente del director creativo).

**Para el jugador: «El reloj nunca tuvo que decidir».** Pilar 4: *"La partida dura entre 15 y 20 minutos y nunca pasa de 25."* El momento: minuto 18, el Rey rival tiene 4 de vida, a vos te quedan 3 minutos de reloj, y tres arqueros rompen el escudo juntos. La partida termina en el tablero, no porque se agotó un reloj. El jugador no ve ninguna métrica. Siente que el duelo es corto *y* decidido, porque las reglas se ajustaron con partidas que terminaron así y no con partidas que el reloj cortó. Por eso la compuerta del arte cuenta solo las partidas que el tablero cierra (Core Rules, regla 12): un duelo que llega a 25:00 por reloj agotado cumple el tope, pero no la promesa.

**Para el estudio: «El estudio sabe, no adivina».** Pilar 3c: *"el que tiene más piezas sigue ganando la mayoría de las partidas (umbral fijado el 2026-11-25: 2 de cada 3, medido con 20 partidas o más)."* El momento: después del tercer playtest con amigos, el diseñador exporta los registros, corre el informe y lee «Compuerta: abierta (3 de 3)», «W: muestra insuficiente (7 de 20)», «Turno medio: 51 s». Cada cambio de reglas que siga (el Modificador, la duración, la Distracción) sale de esos números. Lo que pide al resto del diseño: veredictos honestos (el informe dice «muestra insuficiente» antes que inventar una tendencia) y versiones separadas (nunca se mezclan partidas de dos balances distintos en un mismo veredicto).

**Restricción que no es fantasía, pero se protege igual:** nada sale del equipo salvo una exportación manual, y el registro no guarda nombres ni textos (Core Rules, regla 9). Si un jugador ve «Exportar métricas», tiene que poder entender que el archivo es suyo.

*Qué este encuadre no promete:* que el jugador vea estadísticas propias. Eso es de Estadísticas y ranking (#36, Alpha 2b).

## Detailed Design

**Vistas por rol** (lentes de la Sección C; Meta → Foundation/Infrastructure, más analytics-engineer porque el índice se lo asigna):
- *systems-designer:* lista cerrada de eventos con versión de esquema; estados del registro explícitos (grabando, completo, incompleto, truncado); cada fórmula que otro GDD dejó a Métricas se calcula solo con datos que el registro tiene.
- *analytics-engineer:* guardar los eventos y no solo conteos, porque las preguntas de playtest cambian; separar todo veredicto por versión de reglas, de balance y de config; no mezclar dispositivos sin un orden definido.
- *gameplay-programmer:* escribir al cerrar el turno, nunca por evento; Métricas no puede frenar un cuadro ni cambiar un resultado.
- *engine-programmer:* en Web no hay hilos ni `Task.Run` (referencia del motor, `modules/core.md`); el almacenamiento del navegador es limitado, así que tope de tamaño y rotación van en el diseño. **Conflicto con analytics-engineer** sobre cuánto guardar: lo resolvió el jugador (2026-11-26) con todos los eventos más resúmenes, con tope y truncado (reglas 5 y 7).

### Core Rules

1. **Qué es y dónde corre.** Un servicio de **cliente**, fuera del núcleo, que solo corre en **partidas locales** (Fase 1). Se suscribe a los eventos que devuelve cada comando aceptado (Librería, regla 8), a los eventos del Timer (#12, regla 12) y a los eventos de presentación que se le entregan (regla 4). Puede llamar a las **consultas puras** de la Librería (regla 10) para leer el estado; **nunca** envía un comando, no escribe en el estado y no entra en la forma canónica ni en la huella. El núcleo no sabe que existe (índice, frontera del núcleo). Online (Fase 2a) lo reemplaza la Analítica online (#32), que reusa este esquema.

2. **Un registro por partida.** Al aceptarse `CrearPartida`, Métricas abre un **registro de partida** nuevo; lo cierra al recibir `PartidaTerminada`. Cada registro tiene tres partes, en este orden: un **encabezado**, las **líneas** (eventos y resúmenes de turno, en orden de llegada) y un **cierre** (resumen de partida). Un registro sin cierre es **incompleto** (regla 8). Aparte, cada arranque del juego abre un **registro de sesión** (regla 6).

3. **Encabezado (campos fijos).** `versionEsquema` (de este GDD; empieza en 1), `idPartida` (aleatorio de 128 bits, sin relación con ninguna persona), `idDispositivo` (aleatorio de 128 bits, creado la primera vez que corre el juego en ese navegador; solo sirve para ordenar partidas de varios equipos y nunca sale del equipo salvo en una exportación), `numeroPartidaLocal` (contador del dispositivo que sube con **cada** `CrearPartida` aceptado, también con las métricas apagadas, y nunca se reinicia, ni siquiera al borrar), `inicioLocal` (fecha y hora local al minuto), `versionBuild`, `versionReglas`, `versionBalance` y huella del Catálogo (#2, regla 7), **la config de partida que es perilla** (las 5 tablas base del Modificador, `limite_normal`, `vida_tramo`, `bono_grito_ataque`, `bono_grito_movimiento`, `T_tope`, la tabla de reclutas), `modoTimer` (#12, regla 3), `rival` (`Humano` o `RivalIA`), `plataforma` (`WebEscritorio` o `WebTactil`), `idioma` (código `es` o `en`) y las opciones al empezar (`FichaSimple`, `ReducirEfectos`). El par (`versionReglas`, `versionBalance`) más la config forman el **grupo de balance**: el informe nunca mezcla dos grupos en un veredicto (regla 13).

4. **Esquema de eventos (lista cerrada, `versionEsquema` 1).** Cada línea lleva `t_ms` (milisegundos enteros desde la creación de la partida, del mismo reloj monotónico inyectado que usa el Timer, #12, regla 4), el tipo y los campos **tal como los define su GDD dueño**. Métricas no renombra ni recalcula un campo ajeno. Se guardan **códigos y enums, nunca textos traducidos** (Localización, Dependencies).

   | Familia | Eventos que se guardan | Dueño de la forma |
   |---|---|---|
   | Turno y fin | `TurnoIniciado`, `FaseIniciada`, `FaseSaltada`, `ReclutaPerdido`, `TurnoTerminado`, `PartidaTerminada` | Máquina (#3, regla 16) |
   | Tirada | Evento de la tirada: `n`, cara, tabla efectiva y nombre de la tabla base | Modificador (#11, regla 9); Librería (regla 8) |
   | Reclutar | `Reclutado(jugador, cara, tipo, ids, casillas, llamadasRestantes, automatico)` | Reclutamiento (#10, regla 12) |
   | Mover | `PiezaMovida(pieza, origen, destino, costo, camino)` y `MovimientoDeshecho(pieza, origen, destino, usadosRestaurados)` | Movimiento (#6, regla 12); Máquina (regla 16, desde el cierre de la tanda 4) |
   | Asignar | `AtaqueAsignado`, `AtaqueQuitado`, `CoberturaAsignada`, `CoberturaQuitada` | Alcance (#7, regla 15); Cobertura (#8, regla 15) |
   | Resolver | `AtaqueEjecutado`, `DanoRecibido`, `DanoATramo`, `TramoRoto`, `PiezaMuerta` (y `CoberturaQuitada` de la resolución) | Resolución (#9, regla 14) |
   | Reloj | `TurnoMedido`, `RelojesAlCierre`, `TiempoAgotado`, `TramoFinalIniciado`, `RelojPausado`, `RelojReanudado` | Timer (#12, regla 12) |
   | Uso | `PanelAbierto(jugador, turno, fase, ms_abierto)` | Interfaz del Modificador (#17, regla 17) |
   | Rechazo | `Rechazo(codigo, comando)`: el cliente lo anota cuando `Aplicar` devuelve Rechazado (un rechazo no produce eventos del núcleo) | Métricas; los códigos son de la Librería y de cada regla |
   | Técnico | `Error(severidad, codigo, origen)` y `Rendimiento` (regla 5) | Métricas |

   **No se guardan:** los eventos de la vista previa (#16), que repiten los de `Resolver` sobre una copia (Librería, regla 9), ni la selección o el gesto de Entrada (#13). Un evento nuevo de otro sistema **no** entra solo: sumarlo es un cambio de este esquema y sube `versionEsquema`.

5. **Resumen de turno.** Al llegar `TurnoTerminado`, Métricas escribe un `ResumenTurno` calculado con las líneas del turno y las consultas puras: `numeroTurno`, `jugador`, cara y tabla base, `pGrito` (probabilidad de Grito de guerra en la tabla efectiva de ese jugador), piezas vivas de cada jugador al abrir el turno (sin el Rey y con el Rey), `piezasMovidas`, deshechos, ataques asignados al resolver, receptores `combinado`, muertes cuyo `DanoRecibido` fue `combinado`, piezas reclutadas, `vidaQuitada` del turno (F4 de Resolución), bajas por jugador, **vida por fuego amigo** (`vidaPerdida` de los receptores con `atacantesRivales` = 0 que son del jugador del turno), tramos rotos (y si alguno era **propio**), `ms_mover`, `ms_atacar`, `vencio_mover`, `vencio_atacar` (de `TurnoMedido`), `ms_panel` (suma de `ms_abierto`), rechazos por código, `rendirseDisponible` (la consulta `RendirseDisponible(estado, jugador)` de la Librería, regla 10, al abrir el turno), `reyPerdioVida` y `reyCubierto` del rival, fases saltadas y el `Rendimiento` del turno: cuadros, milisegundos, cuadros por encima de `cuadro_lento_ms` y el peor cuadro (F6). **Turno parcial:** si la partida termina a mitad de turno (RelojAgotado, Rendirse, Abandono, Tablas automáticas) no llega `TurnoTerminado`; el `ResumenTurno` de ese turno se escribe al llegar `PartidaTerminada`, con su `numeroTurno`, jugador del turno, fase y ataques asignados sin resolver (campos de la Librería, regla 8), y se marca parcial.

6. **Registro de sesión.** Uno por arranque del juego: `versionEsquema`, `versionBuild`, `plataforma`, `Carga(etapa, ms)` (arranque hasta el primer cuadro, hasta el tablero listo), los `Error` que ocurran fuera de una partida, cuántas partidas se crearon y, **reservado para el Tutorial (#24, Vertical Slice)**, `TutorialPaso(paso, resultado)` con resultado `Completado` o `Abandonado`. En el MVP la familia del tutorial existe en el esquema pero no se emite.

7. **Escritura y tamaño.** Las líneas se juntan en memoria y se **escriben en el dispositivo al cerrar cada turno** (`TurnoTerminado`) y al cerrar la partida; nunca una escritura por evento ni durante una animación. Un registro no pasa de `registro_max_kb` (512 KB; F7): si la próxima escritura lo pasaría, desde ese turno se guardan solo los `ResumenTurno` y el cierre, y el registro queda marcado `truncado` (sigue contando para la compuerta: el resumen tiene todo lo que ella usa). El dispositivo guarda los últimos `partidas_guardadas_max` (100) registros de partida y los últimos `sesiones_guardadas_max` (20) de sesión; al pasar el tope se borra el más viejo. Dónde y cómo se guarda en el navegador → **ADR**.

8. **Registro incompleto.** Si el juego se cierra, se recarga o se cuelga antes de `PartidaTerminada`, el registro queda con los turnos ya escritos y sin cierre. Al arrancar, Métricas marca `incompleto` todo registro sin cierre. No se completa ni se adivina el final. Un registro incompleto **no cuenta** para la compuerta (regla 12) y el informe lo lista aparte.

9. **Privacidad.** Nada sale del dispositivo: no hay envío, red ni identificador de cuenta. El registro no guarda nombres, alias, texto escrito por nadie, dirección, huella del navegador ni ningún identificador que no haya creado el propio juego al azar (`idPartida`, `idDispositivo`; regla 3), más allá de `plataforma`. Los jugadores son `0` (azul) y `1` (rojo). Sacar los datos exige una acción manual (regla 10). Cuando llegue el online, la Analítica (#32) pide su propio consentimiento: este registro **no** lo concede.

10. **Encendido, exportar y borrar.** Las métricas están **encendidas por defecto** (jugador, 2026-11-26: nada se envía). Tres acciones, ninguna dentro del turno: **Exportar** descarga un solo archivo con todos los registros guardados (de partida y de sesión) y no borra nada; **Borrar** elimina todos los registros guardados (no reinicia `numeroPartidaLocal`); **Apagar** deja de abrir registros nuevos y no borra los guardados. En el MVP solo existe Exportar (UI Requirements); Ajustes (#20, Vertical Slice) suma Apagar y Borrar. Apagar o borrar en medio de una partida vale desde la próxima.

11. **Métricas nunca cambia la partida.** Si una escritura falla (almacenamiento lleno, permiso denegado), la partida sigue igual: se anota un `Error(S3, EscrituraFallida)` en memoria, se reintenta al cerrar el turno siguiente y, si vuelve a fallar, el registro queda `incompleto` y Métricas deja de escribir en esa partida. Ningún fallo de Métricas produce un aviso durante el turno, un comando, una pausa del reloj ni un cuadro perdido por encima del presupuesto (F6, AC-21). Un `ErrorInterno` de la Librería (regla 1) se registra como `Error(S1, ErrorInterno, comando)` con el código y el comando, y **sin el estado**.

12. **Compuerta del arte (decisión del jugador, 2026-11-26).** *Regla de producción: no se produce arte final hasta que 3 partidas seguidas duren 25 minutos o menos (índice, decisiones del 2026-11-19).* Con timer, los relojes de partida ya impiden pasar de 25:00 de tiempo de turno (Máquina, D2), así que la duración sola la aprobaría siempre. Por eso:
    - **Cuenta** una partida si su registro está completo (truncado vale), `rival` = `Humano` y `modoTimer` tiene timer (Variable, Fijo 90 o Fijo 60). Una partida Sin límite, una contra el Rival IA o una incompleta **no cuenta**: no suma a la racha ni la corta, y el informe la muestra aparte. La duración de las Sin límite se informa sola, porque es la única duración no acotada que se puede medir.
    - **Pasa** si terminó **en el tablero**, con motivo `ReyMuerto`, `DobleRey` o `Rendicion` (Máquina, regla 13; Librería, regla 3), y su `duracion_pared_s` (F1) es ≤ `T_tope` (1.500 s).
    - **No pasa** si terminó por `RelojAgotado` o `Tablas`, o si `duracion_pared_s` > `T_tope`. En ese caso la racha vuelve a 0.
    - **Racha:** dentro de un mismo grupo de balance (regla 3). Las partidas de un mismo dispositivo se ordenan por `numeroPartidaLocal` (nunca por la hora, que el usuario puede cambiar); las de varios dispositivos se intercalan por `inicioLocal` y, con el mismo minuto, por `idDispositivo` y `numeroPartidaLocal`. Una partida que pasa suma 1; una que no pasa la deja en 0. La compuerta de un grupo está **abierta** cuando su racha llegó alguna vez a `compuerta_racha` (3). Cambiar una regla o un número de balance abre un grupo nuevo y la medición empieza de cero, que es lo buscado: la compuerta mide las reglas vigentes.
    - **Huecos a la vista.** Un salto en `numeroPartidaLocal` dentro de la racha (una partida con las métricas apagadas, borrada o rotada) o una partida incompleta entre dos que pasan no cortan la racha, pero el informe la muestra como «racha con huecos (k)», con los números que faltan. Cerrar el juego para salvar una racha queda visible, no escondido.
    - El veredicto lo calcula la herramienta de informe (regla 13), no el juego. Abrir la compuerta habilita la decisión; no la toma.

13. **Herramienta de informe (fuera del juego, decisión del jugador, 2026-11-26).** Un programa en `tools/` lee uno o varios archivos exportados, rechaza los registros con un `versionEsquema` que no conoce (los lista, no los adivina) y produce un informe **por grupo de balance** con: el estado de la compuerta y su racha (regla 12); la duración de partida y de turno contra la meta de 900–1.200 s y el tope (Máquina, D1; F1 de este GDD); `turnos_presupuesto` por partida (Máquina, D1); el líder inicial y `W` con su error estándar (F3; Modificador, F5); `bajas_por_turno` y `delta_reclutamiento` (F4; Reclutamiento, F6); `t_turno`, `t_atacar` y los turnos con Distracción (F4; Modificador, F4); `N_base_estimado` (F4); el porcentaje de turnos con combinado (F5); el rendimiento (F6); y las **medidas de playtest** de la tabla de la regla 14. Un dato que necesita una muestra mínima y no la tiene se informa como «muestra insuficiente (k de n)», nunca como un número que parece decidir.

14. **Medidas de playtest (lo que otros GDD dejaron a #19).** El informe las da por grupo de balance, todas calculables con el registro:

    | Pregunta (GDD de origen) | Medida |
    |---|---|
    | ¿Gana el plan o el dado? Hipótesis central del MVP y riesgo «El dado inclina partidas» (concepto; pilar 1) | Por partida y por jugador, las **tiradas perdidas** (turnos con Distracción o `ReclutaPerdido`, la medida de la v2), los Gritos y las caras de reclutar. Parte de las partidas que ganó el jugador con menos tiradas perdidas, y parte de las que ganó el que iba adelante en piezas antes de la tirada que dio vuelta la tabla base. Es una orientación, no un veredicto: el dado adaptativo da más a quien va perdiendo (pilar 3) |
    | ¿El reloj decide partidas que el tablero no decidió? (Máquina; Timer) | Partidas por `RelojAgotado` y, en ellas, quién iba adelante en piezas vivas y en `vidaQuitada` al cierre |
    | Fijo 90 con uso máximo termina en el turno 17 (Timer) | Duración y `N` por `modoTimer` |
    | ¿Cuántos turnos abren `Atacar` sin ataques útiles? (Alcance, regla 14) | Turnos con `Atacar` abierta y 0 ataques asignados al resolver, y su `ms_atacar` |
    | ¿Vale atacar el muro? ¿Cuánto suma el muro a la duración? (Castillo) | Ataques a tramos por partida, turno del primer tramo roto, cortes de reclutamiento (el **cruce** en una resolución, de `tramosRotosAntes` < `TRAMOS_ROTOS_CORTE` a `tramosRotosDelDueno` ≥ `TRAMOS_ROTOS_CORTE`: una explosión puede dejar 3 sin pasar por 2; Castillo, regla 10) y turnos desde el corte al fin |
    | Cortar el propio castillo, llenar el propio anillo o sacrificar piezas para cruzar un umbral (Modificador; Reclutamiento; Castillo) | Tramos rotos propios, vida por fuego amigo y turnos con una cara de reclutar en 0% |
    | ¿Demasiado Grito para el que pierde? (Modificador) | Turnos del que pierde con `pGrito` ≥ 60% |
    | ¿El tope de 6 Llamadas aprieta? (Reclutamiento) | Partidas en que algún jugador llegó a `llamadasRestantes` = 0 |
    | La tortuga alrededor del Rey (Cobertura; Resolución) | Turnos con el Rey cubierto y turnos en que el Rey atacado no perdió vida |
    | ¿El demoledor corta el reclutamiento demasiado barato? (Resolución) | Partidas con un corte en que un `PiezaMuerta` con motivo `Explosion` rompió un tramo |
    | ¿La muralla traba la partida? ¿El Rey sin cupo es la mejor pieza de ataque? ¿El rango de `limite_normal`? (Movimiento) | Distribución de `piezasMovidas` por turno, turnos con el cupo lleno y parte del daño hecha por el Rey |
    | Azul empieza siempre (Tablero, regla 10) | Por partida: victorias por color; quién da el primer daño (jugador, turno y cara del Modificador) y si gana; quién cruza primero la columna 11/12. Las franjas por columna del turno 2 (8-9 con Grito, el 40%; 10 en adelante, el 80%; Distracción, columna 14). **Disparador** (jugador, 2026-11-27): rojo gana ≥ 14 de 20, o rojo da el primer daño en ≥ 15 de 20; si salta se decide entre (a) azul tira en el turno 1 y (b) rojo no tira en su primer turno |
    | ¿Hace falta una cota de muertes por explosión? (Catálogo, I4) ¿Arqueros en masa? (Catálogo, Open Questions) | Muertes por explosión por partida (`PiezaMuerta` con motivo `Explosion`, sin el Rey) y daño quitado por tipo de pieza atacante |
    | ¿Se usa el panel «?»? (Interfaz del Modificador) | Partidas y turnos con `PanelAbierto` y `ms_panel` medio |
    | Uso de Deshacer y Rendirse y de Ficha simple (HUD) | Deshechos por turno, turnos con `rendirseDisponible` y rendiciones, `FichaSimple` al empezar |
    | Turno de menos de 90 s (hipótesis del prototipo) | `seg_medio` (Máquina, D1) y parte de etapas vencidas |
    | ¿`vidaQuitada` cuenta bien al arquero en el muro? (Resolución, Open Questions) | Se usa tal como la define Resolución (pieza y tramo por separado: es vida realmente perdida); los golpes se cuentan aparte con `AtaqueEjecutado`. Cierra la pregunta |

15. **Lo que Métricas no hace.** No reproduce partidas (Registro, #26), no envía nada (Analítica, #32), no muestra estadísticas al jugador (#36), no decide ninguna regla, no da veredictos dentro del juego y no guarda el estado completo de la partida.

### States and Transitions

Estados de un **registro de partida** (del cliente, no del núcleo):

| Estado | Qué significa |
|---|---|
| `Apagado` | Métricas apagadas (regla 10): no se abre registro |
| `Grabando` | Partida en curso; las líneas se juntan y se escriben al cerrar cada turno |
| `GrabandoTruncado` | Se alcanzó `registro_max_kb`: solo se escriben resúmenes de turno y el cierre |
| `Completo` | Llegó `PartidaTerminada` y se escribió el cierre |
| `Incompleto` | Sin cierre: el juego se cerró, o fallaron dos escrituras seguidas (reglas 8 y 11) |

| Desde | Hacia | Cuándo |
|---|---|---|
| — | `Grabando` | `CrearPartida` aceptado, con las métricas encendidas |
| — | `Apagado` | `CrearPartida` aceptado, con las métricas apagadas |
| `Grabando` | `GrabandoTruncado` | La próxima escritura pasaría `registro_max_kb` |
| `Grabando` o `GrabandoTruncado` | `Completo` | `PartidaTerminada` y cierre escrito |
| `Grabando` o `GrabandoTruncado` | `Incompleto` | Segunda escritura fallida seguida, o arranque que encuentra el registro sin cierre |
| `Completo` o `Incompleto` | (borrado) | Rotación por `partidas_guardadas_max` o Borrar (regla 10) |

No hay transición de vuelta: un registro `Incompleto` nunca se completa y uno `Completo` no se reescribe. Apagar en medio de una partida no cambia el estado del registro abierto (regla 10).

### Interactions with Other Systems

| Sistema | Entra a Métricas | Sale de Métricas | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Eventos de cada comando aceptado, Rechazado con su código, `ErrorInterno`, consultas puras | Nada | Librería (contrato) |
| #2 Catálogo | `versionBalance` y la huella del Catálogo (vía el contexto) | Nada | Catálogo |
| #3 Máquina de turno | Eventos de la regla 16, `vidaQuitada`, `T_tope`, meta de duración; D1 (`N`, `seg_turno_t`) | `N`, `seg_turno_t`, `seg_medio`, `N_base_estimado` en el informe | Máquina (eventos y D1); Métricas (cómo se miden) |
| #6 Movimiento | `PiezaMovida`, `limite_normal` (config) | Distribución de `piezasMovidas` | Movimiento |
| #7 Alcance, #8 Cobertura | Eventos de asignación y de cobertura | Medidas de la regla 14 | Cada GDD |
| #9 Resolución | Eventos de la regla 14 (campos `combinado`, `atacantesRivales`, `vidaPerdida`) | Combinados (F5), fuego amigo, medidas de la regla 14 | Resolución |
| #10 Reclutamiento | `Reclutado`; F6 | `piezas_reclutadas`, `bajas_por_turno`, `delta_reclutamiento` | Reclutamiento (F6); Métricas (medición) |
| #11 Modificador | Evento de la tirada (cara, tabla efectiva, tabla base); F4, F5 | `Δturnos_D`, `t_turno`, `t_atacar`, líder inicial, `W` | Modificador (fórmulas); Métricas (medición) |
| #12 Timer | `TurnoMedido`, `RelojesAlCierre`, `TiempoAgotado`, pausas, `modoTimer`; reloj monotónico | Nada | Timer |
| #14 Localización | — | Nada: guarda códigos, nunca textos | Localización |
| #15 Ficha y HUD | Opciones al empezar (`FichaSimple`, `ReducirEfectos`); la placa de fin de partida aloja Exportar en el MVP | Nada | HUD (dónde está el botón); Métricas (qué hace) |
| #17 Interfaz del Modificador | `PanelAbierto` | Nada | Interfaz del Modificador |
| #20 Ajustes (VS) | Encendido/apagado | Acciones Apagar, Borrar, Exportar | Ajustes (pantalla); Métricas (efecto) |
| #24 Tutorial (VS) | `TutorialPaso` (reservado) | Nada | Tutorial |
| #25 Rival IA (VS) | `rival` = `RivalIA` en el encabezado | Nada | Rival IA |
| #26 Registro (Alpha), #32 Analítica (Alpha) | — | El esquema y su `versionEsquema` | Métricas (esquema); #32 (envío) |

**Supuestos PROVISIONALES:** Ajustes (#20), Tutorial (#24) y Rival IA (#25) todavía no tienen GDD. El evento de `Deshacer` (`MovimientoDeshecho`, Máquina regla 16) y el botón Exportar en la placa de fin (#15, regla 22) quedaron fijados al cerrar la tanda 4.

## Formulas

Ninguna de estas fórmulas corre en el núcleo ni decide una regla del juego. F1, F5 (por turno) y F6 las calcula el cliente al escribir el registro. F2 a F5 (agregadas) y F7 las calcula la herramienta de informe. Las fórmulas que otros GDD le dejaron a Métricas **se usan sin redefinirlas**: D1 y D2 de la Máquina, F4 y F5 del Modificador, F6 de Reclutamiento, F4 y F5 de Resolución. Acá se define solo **cómo se miden sus variables** con el registro. Todo es aritmética entera salvo el error estándar de F3, que es de la herramienta y nunca vuelve al juego.

### F1. Duración de pared de la partida

La fórmula `duracion_pared_s` se define así:

`duracion_pared_s = ⌊(t_cierre_ms − ms_excluido) / 1000⌋`
`ms_excluido = |⋃ intervalos excluidos|` (unión de `[RelojPausado, RelojReanudado)` y de cada hueco entre dos cuadros consecutivos mayor que `salto_reloj_max_ms`, contando una sola vez lo que se solapa)

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `t_cierre_ms` | int (ms) | 0 – 2³¹−1 | calculated (`t_ms` de `PartidaTerminada`) | Milisegundos desde la creación hasta el fin |
| `ms_excluido` | int (ms) | 0 – `t_cierre_ms` | calculated (eventos del Timer y marcas de cuadro) | Tiempo de pausa del sistema o de equipo dormido |
| `salto_reloj_max_ms` | int (ms) | 1.000–30.000 (valor 5.000) | data file (Timer #12, config del cliente) | El mismo umbral que usa el Timer; no se duplica |
| `duracion_pared_s` | int (s) | 0 – 2.147.483 | calculated | Lo que el jugador vivió como partida, con animaciones y esperas de la tirada |

**Output Range:** de 0 en adelante. Con timer, en local es casi igual a `duracion_partida` (Máquina, D1 = Σ `ms_mover` + `ms_atacar` de `TurnoMedido` / 1000), más las esperas fuera de los relojes: la presentación de la cosecha y la espera de la placa (hasta 1,2 s; 1,5 s con Distracción; la placa no cuesta reloj, Timer, regla 17), en total ≤ 3,5 s por turno, y el tiempo en que no corría ninguna etapa. Puede pasar de `T_tope` aunque los relojes no: por eso F2 usa esta duración y no la de D1. Una pausa sin `RelojReanudado` al llegar `PartidaTerminada` se cierra en `t_cierre_ms`, así que `ms_excluido` nunca pasa de `t_cierre_ms` y el resultado nunca es negativo. El tope de 31 bits es de unos 24 días: inalcanzable con timer, y en Sin límite es el borde del caso de E (Edge Cases).
**Example:** `t_cierre_ms` = 1.412.300, una suspensión de 30.000 ms y un hueco de 8.000 ms dentro de ella → `ms_excluido` = 30.000 → `duracion_pared_s` = 1.382. La D1 de esa partida daba 1.381 s.

### F2. Compuerta del arte: cuenta, pasa y racha

La fórmula `racha_compuerta` se define así, para las partidas `k = 1 … m` de un grupo de balance en el orden de la regla 12:

`cuenta(k) = completo(k) ∧ rival(k) = Humano ∧ modoTimer(k) ≠ SinLimite`
`pasa(k) = cuenta(k) ∧ motivo(k) ∈ {ReyMuerto, DobleRey, Rendicion} ∧ duracion_pared_s(k) ≤ T_tope`
`racha(0) = 0` · `racha(k) = racha(k−1) + 1` si `pasa(k)` · `0` si `cuenta(k) ∧ ¬pasa(k)` · `racha(k−1)` si `¬cuenta(k)`
`abierta = max_k racha(k) ≥ compuerta_racha`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `completo(k)` | bool | — | calculated (estado del registro) | Tiene cierre; `truncado` cuenta como completo |
| `motivo(k)` | enum | ReyMuerto, DobleRey, Rendicion, RelojAgotado, Tablas, Abandono | calculated (`PartidaTerminada`) | Motivo de fin (Máquina, regla 13; Librería, regla 3) |
| `T_tope` | int (s) | 1.290–1.500 (valor 1.500) | data file (Máquina #3, config de la autoridad; registro `tope_partida_s`) | El tope de 25 min, leído del encabezado |
| `compuerta_racha` | int | 1–10 (valor 3) | data file (config de la herramienta) | Partidas seguidas que pasan para abrir la compuerta |
| `racha(k)` | int | 0 – m | calculated | Racha vigente después de la partida k |

**Output Range:** `racha` de 0 a `m`; `abierta` es booleano. Con `compuerta_racha` = 1 una sola partida con suerte abre la compuerta; con 10 hacen falta más playtests de los que hay en un mes. El 3 es la regla de producción del índice, no un número de esta fórmula. `Abandono` es online y no llega en local; si llegara, no pasa.
**Example:** grupo con 6 partidas: P1 ReyMuerto 1.180 s (racha 1), P2 Sin límite (no cuenta, sigue en 1), P3 RelojAgotado (0), P4 Rendicion 960 s (1), P5 ReyMuerto 1.420 s (2), P6 DobleRey 1.290 s (3) → **abierta**.

### F3. Líder inicial y umbral `W` (medición de F5 del Modificador)

La fórmula `medicion_w` se define así:

`lider(k) = jugador(e)` si `tablaBase(e) ∈ {Gana 2 a 1, Gana 4 a 3}`, `rival(jugador(e))` si `tablaBase(e) ∈ {Pierde 1/2, Pierde 3/4}`, donde `e` es el **primer** evento de la tirada de la partida (en `t_ms`) cuya tabla base no es `Normal`; sin tal evento, la partida no tiene líder
`n = |{k : completo(k) ∧ rival(k) = Humano ∧ lider(k) existe ∧ ganador(k) existe}|` · `v = |{k de n : ganador(k) = lider(k)}|`
`W_pct = ⌊100 × v / n⌋` · veredicto: `muestra insuficiente` si `n < muestra_min_w`; si no, `cumple` si `3 × v ≥ 2 × n`, y `bajo el umbral` si no
`ee_pct = 100 × √(p × (1 − p) / n)` con `p = v / n` (solo en la herramienta)

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `tablaBase(e)` | enum | las 5 tablas | calculated (evento de la tirada, Modificador regla 9) | Nombre de la tabla base de esa tirada (Modificador, F1) |
| `n` | int | 0 – partidas guardadas | calculated | Partidas con líder inicial y con ganador (Tablas no cuenta) |
| `v` | int | 0 – n | calculated | Las que ganó el líder inicial |
| `muestra_min_w` | int | 20–60 (valor 20) | data file (Modificador #11, Tuning Knobs «Muestra para decidir») | Se apunta, no se duplica |
| `W_pct` | int (%) | 0–100 | calculated | `W` del Modificador (F5) |
| `ee_pct` | decimal | 0–50 | calculated (herramienta) | Error estándar, para no leer como decisión lo que es ruido |

**Output Range:** `W_pct` de 0 a 100; con `n` = 0 no se divide: el veredicto es `muestra insuficiente`. El umbral se compara en enteros (3v ≥ 2n), así 2/3 exacto cumple sin redondeos. Las partidas Sin límite y las terminadas por reloj **sí** cuentan para `W`: miden balance, no duración. Los dos lados de la condición del Modificador («quien cumple primero Gana 4 a 3, o el rival de quien primero cumple Pierde 3/4») se leen del mismo evento, porque la tabla base de una tirada es exactamente esa condición evaluada al abrir el turno (Modificador, F1).
**Example:** 20 partidas con líder, 13 ganadas → 39 < 40 → `W_pct` 65, «bajo el umbral», `ee_pct` ≈ 10,7. Con 14 → 42 ≥ 40 → 70, «cumple» (Modificador, AC-26). Con 19 → «muestra insuficiente (19 de 20)».

### F4. Δturnos medidos y `N_base_estimado`

La fórmula `delta_turnos_medidos` se define así, por partida:

`bajas_por_turno = max(1, ⌊(2 × bajas + N) / (2 × N)⌋)` (media redondeada al entero más cercano, mínimo 1; F6 de Reclutamiento)
`delta_reclutamiento` = F6 de Reclutamiento con `piezas_reclutadas` = Σ `|ids|` de los `Reclutado` de la partida
`D = |{turnos con cara Distraccion}|` (cota superior de `Δturnos_D`; Modificador, F4)
`t_turno = ⌊Σ (ms_mover + ms_atacar) / (1000 × T_m)⌋` sobre los `T_m` turnos con etapa de mover · `t_atacar = ⌊Σ ms_atacar / (1000 × D)⌋` sobre los turnos con Distracción
`ΔResolución = 0` (por definición: los turnos que hacen falta para matar al Rey **son** la base)
`N_base_estimado = max(1, N − delta_reclutamiento − D)`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `N` | int | 1 – `C_MAX` | calculated (último `numeroTurno`) | Turnos de la partida (Máquina, D1) |
| `bajas` | int | 0 – 122 + reclutas | calculated (`PiezaMuerta`, los dos jugadores, todo motivo) | Piezas que salieron del tablero |
| `piezas_reclutadas` | int | 0–96 | calculated (`Reclutado`) | Reclutamiento, F6 |
| `D` | int | 0 – N | calculated (evento de la tirada) | Turnos con Distracción |
| `T_m` | int | 0 – N | calculated (`TurnoMedido` con etapa de mover) | Turnos que tuvieron etapa de mover |
| `t_turno`, `t_atacar` | int (s) | 0 – `turno_max_s` | calculated | Las del Modificador, F4 |
| `N_base_estimado` | int | 1 – N | calculated | Turnos sin el aporte de reclutas ni de Distracción |

**Output Range:** `bajas_por_turno` ≥ 1, así F6 nunca divide por cero. Con `bajas` = 0, `delta_reclutamiento` = `piezas_reclutadas`. `t_turno` con `T_m` = 0 y `t_atacar` con `D` = 0 son «sin dato», no 0. **`N_base_estimado` es una cota inferior**, porque los dos Δ que resta son cotas superiores (F6 supone que toda pieza reclutada debe morir; F4 cuenta un turno entero por Distracción): de ahí el piso de 1. Es la única forma de acercarse a `N_base` con datos observados, porque ninguna partida juega sin remontada (Máquina, D1). **`ΔResolución` = 0** cierra el pedido de la Máquina (AC-29): Resolución no agrega turnos a una base; es lo que la base mide (cambio pedido en Cross-References).
**Example:** partida v4 de 24 turnos, 30 bajas, 16 reclutas, 4 Distracciones: `bajas_por_turno` = ⌊84 / 48⌋ = 1 → `delta_reclutamiento` = 16 → `N_base_estimado` = max(1, 24 − 16 − 4) = 4. Es una cota tan baja que no informa nada: **con remontadas fuertes, `N_base_estimado` no sirve y el informe lo dice** («cota inferior: 4»). Con 60 bajas: 3 por turno → 6 → 24 − 6 − 4 = 14.

### F5. Turnos con ataque combinado

La fórmula `pct_combinado` se define así, por grupo de balance:

`a = |{turnos con algún AtaqueEjecutado}|`
`c = |{turnos de a con algún DanoRecibido o DanoATramo con combinado}|`
`c_letal = |{turnos de a con algún PiezaMuerta(motivo Dano) cuyo DanoRecibido del mismo turno tiene combinado}|`
`pct_combinado = ⌊100 × c / a⌋` · `pct_combinado_letal = ⌊100 × c_letal / a⌋`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `combinado` | bool | — | calculated (Resolución, F5) | Dos o más atacantes rivales distintos; nunca por fuego amigo |
| `a`, `c`, `c_letal` | int | 0 – turnos | calculated | Conteos sobre el grupo |
| `pct_combinado`, `pct_combinado_letal` | int (%) | 0–100 | calculated | Parte de los turnos con ataque que combinaron, y que mataron combinando |

**Output Range:** con `a` = 0, «sin dato». `c_letal` ≤ `c` ≤ `a`. Un combinado que el escudo absorbe entero cuenta en `c` (Resolución, F5), no en `c_letal`. La explosión del demoledor cuenta como golpe de su dueño (Librería, regla 8).
**Example:** v2 combinaba en el 81% y el 91% de los turnos; en v3, el combinado decidió 21 de 34 (concepto, Open Questions). Con 34 turnos con ataque, 28 combinados y 21 letales: `pct_combinado` 82 y `pct_combinado_letal` 61. Es la medida del pilar 2: si `pct_combinado` cae, combinar dejó de ser la jugada natural.

### F6. Rendimiento por turno y presupuesto de Métricas

La fórmula `rendimiento_turno` se define así:

`fps_x10 = ⌊10.000 × cuadros / ms_turno⌋` · `lentos = |{cuadros con dt > cuadro_lento_ms}|` · `peor_ms = max dt`
**Presupuesto propio:** `costo_captura_us` (anotar la lista entera de eventos de un comando, que llega junta) ≤ `metricas_comando_max_us` y `costo_escritura_ms` (al cerrar el turno, en el hilo principal) ≤ `metricas_escritura_max_ms`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `cuadros`, `ms_turno` | int | 0 – …, 1 – … | calculated (marcas de cuadro) | Cuadros del turno y su duración (de `TurnoIniciado` a `TurnoTerminado`) |
| `dt` | int (ms) | 0 – … | calculated | Tiempo entre dos cuadros; los huecos de F1 no cuentan |
| `cuadro_lento_ms` | int (ms) | 33–100 (valor 50) | data file (config de Métricas) | Un cuadro peor que 20 fps |
| `metricas_comando_max_us` | int (µs) | 250–2.000 (valor 1.000) | data file (config de Métricas) | Costo máximo de anotar los eventos de un comando, en el mismo cuadro. El peor caso es `Resolver` con 393 eventos (Resolución, regla 15): unos 2,5 µs por evento, una copia a un buffer sin reservar memoria |
| `metricas_escritura_max_ms` | int (ms) | 1–8 (valor 4) | data file (config de Métricas) | Costo máximo de la escritura del turno en el hilo principal |

**Output Range:** `fps_x10` de 0 en adelante (600 = 60,0 fps). Con `ms_turno` = 0 (un turno sin cuadros, todas las fases saltadas en el mismo cuadro) no hay división: `fps_x10` queda «sin dato» y `lentos` = 0. **Por qué 4 ms:** a 60 fps un cuadro tiene 16,7 ms; la escritura cae en el cuadro de `TurnoTerminado`, cuando empieza la animación de resolución, y 4 ms dejan 12 para el render (#18). **NOT SOURCEABLE:** el costo real de escribir en el almacenamiento del navegador desde Unity Web no está en `docs/engine-reference/unity/`; lo mide el ADR.
**Example:** turno de 52.000 ms con 3.110 cuadros → `fps_x10` 598 (59,8 fps); 4 cuadros de más de 50 ms; `peor_ms` 81.

### F7. Tamaño del registro y del almacén

La fórmula `tamano_registro` se define así:

`kb_registro ≈ ⌈Σ_t (lineas_t × bytes_linea) / 1024⌉` · `kb_almacen_max = partidas_guardadas_max × registro_max_kb + sesiones_guardadas_max × sesion_max_kb`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `lineas_t` | int | 3 – ~430 | calculated | Líneas del turno t: unas 13 de mover, 12 de asignar, de 1 a 393 de resolver (Resolución, regla 15) y el resumen |
| `bytes_linea` | int | ~64 | constant (estimación hasta el ADR del formato) | Tamaño medio de una línea |
| `registro_max_kb` | int (KB) | 256–2.048 (valor 512) | data file (config de Métricas) | Tope por partida (regla 7) |
| `partidas_guardadas_max` | int | 20–500 (valor 100) | data file (config de Métricas) | Registros de partida guardados |
| `sesiones_guardadas_max`, `sesion_max_kb` | int | 5–100 (20), 16–256 KB (64) | data file (config de Métricas) | Registros de sesión |

**Output Range:** una partida típica (34 turnos de unas 70 líneas) ronda los **150 KB**; el peor caso (50 turnos con resoluciones de 393 eventos) pasa de 1,2 MB y se trunca en 512 KB. **Almacén máximo:** 100 × 512 + 20 × 64 = **52.480 KB** (unos 51 MB) en el peor caso; típico, unos 15 MB. **NOT SOURCEABLE:** la cuota del almacenamiento del navegador por plataforma no está en la referencia del motor; si el ADR encuentra una cuota menor, se baja `partidas_guardadas_max`, no el detalle (decisión del jugador: todos los eventos).
**Example:** 24 turnos × 70 líneas × 64 B = 107.520 B → 105 KB.

## Edge Cases

*Revisados con la lente de systems-designer sobre las Secciones C y D ya escritas. En guided, entran todos los casos encontrados (detalle de una dirección ya decidida).*

### Registro y escritura

- **If el juego se cierra en medio de un turno:** quedan los turnos ya cerrados; el turno en curso se pierde y el registro queda `Incompleto` al próximo arranque (regla 8). No cuenta para la compuerta y aparece como hueco de la racha (regla 12).
- **If dos pestañas del juego corren a la vez en el mismo navegador:** cada una abre su propio registro con su `idPartida`; pueden repetir `numeroPartidaLocal`. El informe ordena los empates por `idPartida` y lo marca como «partidas simultáneas». El juego no lo impide.
- **If el almacenamiento está lleno al crear la partida:** la primera escritura falla, se reintenta al cerrar el turno siguiente y, si falla otra vez, el registro queda `Incompleto` (regla 11). No se borra un registro viejo para hacer lugar fuera de la rotación normal, y no hay aviso durante la partida.
- **If una partida llega a `registro_max_kb`:** desde ese turno se escriben solo los resúmenes (regla 7). El resumen de turno lleva todo lo que usan F2 a F5 (cara, tabla base, combinados, muertes combinadas, bajas, reclutas, tiempos), así que la partida sigue contando para la compuerta y para `W`. Las medidas que usan eventos sueltos (la regla 14, por ejemplo el turno del primer tramo roto) se informan «sin dato» para los turnos truncados.
- **If `t_ms` llegaría a 2³¹−1** (unos 24 días, solo posible en Sin límite): Métricas cierra el registro como `Incompleto` y deja de escribir en esa partida. La partida sigue.
- **If se apagan las métricas en medio de una partida:** el registro abierto se completa normalmente y la próxima partida ya no se graba (regla 10). `numeroPartidaLocal` sigue subiendo, así que las partidas sin grabar aparecen como huecos.
- **If se usa Borrar en medio de una partida:** se borran los registros cerrados. El registro abierto sigue y se cierra normalmente.
- **If se exporta en medio de una partida** (posible desde Ajustes, Vertical Slice): el archivo lleva el registro abierto hasta el último turno cerrado, marcado `enCurso`. El informe lo trata como incompleto.
- **If el mismo registro llega dos veces a la herramienta** (dos exportaciones que se solapan): se cuenta una sola vez por `idPartida`. Si las dos copias difieren, gana la que tiene cierre y, entre dos con cierre, la más larga; el informe lo avisa.
- **If un registro trae un `versionEsquema` que la herramienta no conoce:** se lista como «no leído (versión k)» y no entra en ningún cálculo. No se adivinan campos.
- **If la rotación borró partidas antes de exportarlas:** se pierden. Aparecen como huecos de `numeroPartidaLocal` en el informe. La regla es exportar después de cada sesión de playtest (Open Questions).

### Tiempo

- **If el usuario cambia la hora del sistema durante una partida:** no afecta, porque `t_ms` usa el reloj monotónico (regla 4). Entre partidas solo cambia `inicioLocal`, y en un mismo dispositivo el orden sale de `numeroPartidaLocal` (regla 12).
- **If el equipo se duerme o el navegador congela la pestaña más de `salto_reloj_max_ms`:** el hueco no cuenta en `duracion_pared_s` (F1) ni en el rendimiento (F6), igual que en el Timer (regla 10 del Timer). Un congelamiento corto, de menos de 5 s, sí cuenta.
- **If una pausa no se reanudó antes de `PartidaTerminada`:** el intervalo se cierra en `t_cierre_ms` (F1).
- **If un turno tiene todas sus fases saltadas en un solo cuadro:** `ms_turno` es 0; `fps_x10` es «sin dato» (F6), y el turno cuenta para `N` pero no para `t_turno` (F4) ni para el divisor de F5.

### Compuerta y veredictos

- **If una partida termina por Rey muerto pero `duracion_pared_s` > `T_tope`** (esperas o tiempo sin etapa que no descuenta ningún reloj): **no pasa** y la racha vuelve a 0 (F2). La duración que cuenta es la que vive el jugador.
- **If termina por `RelojAgotado` con el que agotó su reloj adelante en el tablero:** no pasa. El informe la cuenta en «¿el reloj decide partidas?» con quién iba adelante (regla 14).
- **If termina en `Tablas`** (cierre automático cuando `contadorComandos` llega a C_MAX − 1, Librería regla 16; o la defensa `LimitePartida`, Timer regla 9): no pasa; no tiene ganador y tampoco entra en `W` (F3).
- **If una partida tuvo un `Error(S1, ErrorInterno)`:** cuenta igual (la partida siguió con el estado intacto, Librería regla 1), pero el informe la lista con su error y no oculta el veredicto. Un S1 se arregla en el código, no se descarta como dato.
- **If cambia el balance o la config entre dos partidas:** forman grupos distintos y la racha del grupo nuevo empieza de 0 (regla 12). Si un playtest alterna dos configs, cada grupo mide lo suyo.
- **If nunca hay una tabla base distinta de Normal en la partida** (partida pareja todo el tiempo): no tiene líder inicial y no entra en `W` (F3). El informe dice cuántas quedaron fuera.
- **If los dos jugadores cumplen condiciones opuestas en el mismo turno:** no puede pasar, porque una tirada evalúa una sola tabla, la del jugador del turno. El primer evento de la tirada que no es Normal decide (F3).
- **If una partida tiene muchas remontadas y `N_base_estimado` cae al piso de 1:** el informe lo muestra como «cota inferior: k», no como `N_base`. Es honesto: no hay forma de separar la base sin partidas sin remontada (Máquina, D1).

### Estrategias degeneradas que Métricas hace visibles (no las impide)

- **Cortar el propio castillo para subir Grito y Arremetida** (Modificador, Castillo): se ve en `tramosRotos` propios por partida y en la tabla efectiva de los turnos siguientes.
- **Sacrificar piezas para cruzar un umbral de tabla** (Modificador, Alcance): se ve en la vida por fuego amigo, cruzada con el cambio de tabla base del turno siguiente.
- **Cerrar el juego para salvar una racha:** queda como hueco o incompleta en la racha (regla 12).

## Dependencies

**Dura** = sin ese sistema no se puede calcular la compuerta ni una fórmula que otro GDD le dejó a Métricas. **Blanda** = sin él falta una medida de playtest, pero el resto funciona.

### Lo que Métricas necesita (upstream)

| Sistema | Tipo | Interfaz | ¿Lo nombra a #19? |
|---|---|---|---|
| #1 Librería de reglas | Dura | Eventos de cada comando aceptado (regla 8), Rechazado con código, `ErrorInterno` (regla 1), consultas puras (regla 10), consulta `RendirseDisponible` (regla 10) | Sí (Interactions, fila 19; regla 14 con S1) |
| #2 Catálogo de piezas | Dura | `versionBalance` y la huella del Catálogo (regla 7) para el grupo de balance | Sí (Dependencies, Blanda) |
| #3 Máquina de turno | Dura | Eventos de la regla 16, `vidaQuitada` (regla 15), motivos de fin (regla 13), `T_tope` y meta de duración; D1 | Sí (Dependencies, Blanda; D1; AC-25; Open Questions) |
| #9 Resolución | Dura | Eventos de la regla 14 con `combinado`, `atacantesRivales` y `vidaPerdida` | Sí (Dependencies, Blanda) |
| #10 Reclutamiento | Dura | `Reclutado` (regla 12); F6 | Sí (Dependencies, Dura) |
| #11 Modificador | Dura | Evento de la tirada con la tabla base (regla 9); F4, F5 y la muestra mínima | Sí (Dependencies, Dura) |
| #12 Timer de turno | Dura | `TurnoMedido`, `RelojesAlCierre`, `TiempoAgotado`, pausas, `modoTimer`, reloj monotónico, `salto_reloj_max_ms` | Sí (Dependencies, Blanda; regla 14) |
| #6 Movimiento | Blanda | `PiezaMovida`, evento de deshacer, `limite_normal` | Sí (Dependencies, Blanda) |
| #7 Alcance y asignación | Blanda | `AtaqueAsignado`, `AtaqueQuitado` | Sí (Dependencies, Blanda) |
| #8 Cobertura del escudero | Blanda | `CoberturaAsignada`, `CoberturaQuitada` | Sí (Dependencies, Blanda) |
| #4 Tablero | Blanda | Constantes de la partida (quién empieza, lados) | Sí (Dependencies, Blanda) |
| #5 Castillo y muros | Blanda | Vía Resolución (`TramoRoto` con `tramosRotosDelDueno`) | **No** en Dependencies (solo en Open Questions): cambio pedido |
| #13 Entrada y selección | Blanda | El cliente que manda el comando anota el `Rechazo(codigo, comando)` | **No**: cambio pedido |
| #14 Localización | Blanda | Ninguna: Métricas guarda códigos | Sí (Dependencies) |
| #15 Ficha y HUD | Blanda | Opciones `FichaSimple` y `ReducirEfectos`; botón Exportar en la placa de fin (MVP) | Sí (Dependencies, Blanda), sin el botón: cambio pedido |
| #17 Interfaz del Modificador | Blanda | `PanelAbierto` (regla 17) | Sí (Dependencies, Blanda) |
| #20 Ajustes (VS) | Blanda | Apagar, Borrar, Exportar | Sin GDD |
| #24 Tutorial (VS), #25 Rival IA (VS) | Blanda | `TutorialPaso` (reservado); `rival` en el encabezado | Sin GDD |

### Lo que depende de Métricas (downstream)

| Sistema | Tipo | Qué usa |
|---|---|---|
| Compuerta del arte (regla de producción del índice) | Dura | La racha de F2 |
| #32 Analítica online (Alpha 2a) | Dura | El esquema de eventos y `versionEsquema`; reusa los resúmenes |
| #36 Estadísticas y ranking (Alpha 2b) | Blanda | Los resúmenes de partida, como referencia de formato |
| Herramienta de informe (`tools/`) | Dura | Los archivos exportados; es parte de este sistema, fuera del juego |
| Calibración del Modificador, la duración y la Distracción (propuesta después de 20 partidas online) | Blanda | `W`, F4 y la regla 14, como base de comparación local |

**Bidireccionalidad:** al cerrar la tanda 4 (2026-11-26), Castillo (#5) y Entrada (#13) pasaron a nombrar a #19 y el HUD (#15) sumó el botón Exportar. Los cambios están en Cross-References. Vista previa (#16) y Render (#18) se escriben en paralelo en esta tanda: Métricas **no** los necesita (los eventos de la vista previa no se guardan, regla 4) y no les pide nada.

## Tuning Knobs

**Perillas propias** (config del cliente de Métricas o de la herramienta; ninguna entra en la config de partida ni en la huella, así que cambiarlas **no** abre un grupo de balance nuevo):

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interactúa con | Dónde vive |
|---|---|---|---|---|---|---|
| `compuerta_racha` | 3 | 1–10 | Más playtests antes de producir arte; la Fase 1 se atrasa | Una partida con suerte abre la compuerta | El orden de la regla 12; los huecos | Herramienta |
| Meta de duración | 900–1.200 s | mínimo 600, máximo `T_tope` | Pegada al tope: el informe nunca avisa | Por debajo de 900, el informe siempre en rojo | `T_tope` y `seg_medio` (Máquina, D1). **Dueño del valor: la Máquina** (Tuning Knobs); acá solo vive | Herramienta |
| `registro_max_kb` | 512 KB | 256–2.048 | El almacén crece hasta pasar la cuota del navegador | Muchas partidas truncadas: la regla 14 pierde medidas | `partidas_guardadas_max` (F7) | Cliente |
| `partidas_guardadas_max` | 100 | 20–500 | Más almacén (F7) | La rotación borra partidas antes de exportarlas (huecos) | `registro_max_kb` (F7) | Cliente |
| `sesiones_guardadas_max`, `sesion_max_kb` | 20, 64 KB | 5–100, 16–256 | Almacén | Se pierden tiempos de carga y errores fuera de partida | F7 | Cliente |
| `cuadro_lento_ms` | 50 ms | 33–100 | Los tirones no se ven | Todo cuadro de 30 fps cuenta como lento | F6 | Cliente |
| `metricas_comando_max_us` | 1.000 µs | 250–2.000 | Métricas se come el cuadro de `Resolver` | Imposible de cumplir; obliga a guardar menos | `vista_previa_presupuesto_ms` y `comando_presupuesto_ms` (Librería): se suma en el mismo cuadro | Cliente (verificación en AC-21) |
| `metricas_escritura_max_ms` | 4 ms | 1–8 | Un tirón al cerrar el turno, justo al empezar la animación de resolución (#18) | Obliga a escribir en varios cuadros; lo decide el ADR | F6, Render (#18) | Cliente |

**Perillas de otros sistemas que Métricas lee** (se apuntan, no se duplican):

| Valor | Dueño | Qué cambia acá |
|---|---|---|
| `T_tope` (`tope_partida_s`, 1.500 s) | Máquina (#3) | El tope de «pasa» (F2) |
| `muestra_min_w` (20 partidas) y umbral 2/3 | Modificador (#11) | El veredicto de `W` (F3) |
| `salto_reloj_max_ms` (5.000 ms) | Timer (#12) | Qué hueco no cuenta en F1 y F6 |
| Modos de timer | Timer (#12) | Qué modos cuentan para la compuerta (regla 12) |
| `limite_normal`, tablas base, `vida_tramo`, bonos del Grito, tabla de reclutas, `versionBalance` | #6, #11, #5, #11, #10, #2 | Definen el grupo de balance (regla 3): cambiar uno empieza una medición nueva |

**Constantes (no son perillas):** la lista cerrada del esquema y `versionEsquema` (cambiarlos es un cambio de este GDD); que nada se envía; que la compuerta solo cuenta partidas con timer y contra un humano.

## Visual/Audio Requirements

**Ninguno en partida.** Métricas no tiene feedback visual ni sonoro: ningún evento de este sistema se ve ni suena durante la partida (regla 11). El único elemento visible es el botón Exportar y su confirmación (UI Requirements), que siguen la capa del sistema del art bible (§7: plana, geométrica, fuera del alcance de las skins) como el resto del HUD. Sin asset propio, así que no hace falta `/asset-spec`.

## Game Feel

**Objetivo: costo nulo perceptible.** Métricas no puede hacer que el turno se sienta más lento ni un cuadro más pesado. Medible: la anotación de los eventos de un comando ≤ `metricas_comando_max_us` (1 ms) y la escritura al cerrar el turno ≤ `metricas_escritura_max_ms` (4 ms), las dos dentro del cuadro (F6, AC-21). El tirón del cierre de turno es el riesgo, porque cae cuando empieza la animación de resolución (pilar 2: el golpe combinado se tiene que sentir). Si el ADR no logra 4 ms en el hilo principal en Web, la escritura se reparte en varios cuadros, y nunca se acorta la animación.

## UI Requirements

1. **Exportar (MVP).** Un botón `Exportar métricas` en la placa de fin de partida (dueño de la placa: HUD #15, regla 22, que lo sumó al cerrar la tanda 4). Al tocarlo, se descarga un solo archivo con todos los registros guardados y la placa dice `Métricas exportadas (k partidas)` durante 2 s. Si no hay registros: `No hay métricas guardadas`. Si la descarga falla: `No se pudo exportar` y el botón sigue activo. Nombre del archivo: `rolchess-metricas-<inicioLocal>-<k>.json` (el formato lo fija el ADR). En los builds de desarrollo, además, un atajo de teclado exporta en cualquier momento (sin texto para el jugador).
2. **Ajustes (Vertical Slice, #20).** Tres controles: `Guardar métricas` (encendido por defecto), `Exportar métricas` y `Borrar métricas` (con confirmación de dos botones, porque no se deshace). Una línea fija debajo: `Las métricas se guardan solo en este equipo. Nada se envía.`
3. **Textos.** Todos salen de Localización con claves `ui.metricas_*` (Localización, regla 12). El contenido del archivo exportado **no** se traduce: lleva códigos.
4. **Nada durante el turno.** Ningún control de Métricas aparece en el HUD de partida ni interrumpe un turno.

> **📌 UX Flag — Métricas locales de partida**: este sistema tiene requisitos de interfaz (botón en la placa de fin de partida y tres controles en Ajustes). En la Fase 4 (Pre-Production), correr `/ux-design` para la placa de fin de partida y la pantalla de Ajustes **antes** de escribir las épicas; las historias que toquen esta interfaz citan `design/ux/[pantalla].md`, no este GDD.

## Cross-References

| Este GDD usa | De | Dónde |
|---|---|---|
| D1 (`duracion_partida`, `seg_medio`, `turnos_presupuesto`, `N_base`), D2 (garantía de 25:00), regla 13 (motivos), 15 (`vidaQuitada`) y 16 (eventos) | `maquina-de-turno.md` | Core Rules 4 y 12; F1, F2, F4 |
| Reglas 1 (`ErrorInterno`), 3 (`Rendirse`), 8 (eventos), 9 (vista previa = misma operación), 10 (consultas) y 11 (huella) | `libreria-de-reglas.md` | Core Rules 1, 4, 5 y 11 |
| Regla 14 (eventos y campos), F4 (`vidaQuitada`), F5 (`combinado`), regla 15 (393 eventos) | `resolucion-simultanea-del-dano.md` | Core Rules 4, 5 y 14; F5, F6, F7 |
| Regla 9 (evento de la tirada), F1 (nombres de las tablas base), F4, F5, muestra de 20 | `modificador.md` | Core Rules 4, 5 y 14; F3, F4 |
| Regla 12 (`Reclutado`), F6 | `reclutamiento-y-limites.md` | F4 |
| Reglas 3 (modos), 4 (reloj monotónico), 10 (pausa y salto), 12 (eventos) y 14 (medición en todos los modos) | `timer-de-turno.md` | Core Rules 4 y 12; F1, F6 |
| Regla 12 (`PiezaMovida`), `limite_normal` | `movimiento.md` | Core Rules 4 y 14 |
| Regla 15 (eventos de asignación); regla 15 (eventos de cobertura) | `alcance-y-asignacion-de-ataques.md`, `cobertura-del-escudero.md` | Core Rules 4 |
| Reglas 7 y 10 (`versionBalance`, huella) | `catalogo-de-piezas.md` | Core Rules 3 |
| Regla 17 (`PanelAbierto`), AC-27 | `interfaz-del-modificador.md` | Core Rules 4 y 14 |
| Reglas 5, 7 y 14 (atenuación, Ficha simple, controles Deshacer y Rendirse) | `ficha-de-pieza-y-hud.md` | Core Rules 3, 5 y 14; UI Requirements |
| Regla 10 (quién empieza) | `tablero-y-despliegue-inicial.md` | Core Rules 14 |
| Guardar códigos, no textos; claves `ui` | `localizacion.md` | Core Rules 4; UI Requirements |
| Pilares 2, 3c y 4; hipótesis de duración | `game-concept.md` | Player Fantasy; Core Rules 12 y 14; F5 |
| Compuerta del arte (decisiones del 2026-11-19), fila 19 | `systems-index.md` | Core Rules 12 |
| Web sin hilos ni `Task.Run` | `docs/engine-reference/unity/modules/core.md` | Vistas por rol; F6 |

### Cambios pedidos a otros GDD (aplicados al cerrar la tanda 4, 2026-11-26)

*Para la sesión que cierra la tanda 4. Ninguno está aplicado: este GDD se escribió sin tocar otros archivos.*

**`maquina-de-turno.md`**
1. **D1, «Aporte de los demás sistemas»:** poner `ΔResolución = 0` por definición (los turnos que hacen falta para matar son la base, Métricas F4) y decir que `N_base` se acerca con `N_base_estimado`, que es una **cota inferior** (Métricas F4). Cambiar «NOT SOURCEABLE — `N_base` no está medido» por «se estima con Métricas F4 (cota inferior)».
2. **AC-29:** con el punto 1, Resolución no declara un `Δturnos` propio; el criterio pasa a «Reclutamiento y Modificador declaran su `Δturnos`; el de Resolución es 0 por definición (Métricas F4)». Quitar PROVISIONAL.
3. **AC-25:** quitar PROVISIONAL. «El archivo marca si la partida entró en 1.500 s» pasa a: el cierre del registro trae `duracion_pared_s` y `pasa` (Métricas F1 y F2); `turnos_presupuesto` lo calcula la herramienta de informe.
4. **Regla 16 (eventos, lista cerrada):** falta el evento que emite `Deshacer` (la Librería, regla 8, exige al menos uno). Propuesta: `MovimientoDeshecho(pieza, origen, destino, usadosRestaurados)`. Métricas lo nombra PROVISIONAL hasta que la Máquina lo fije (y Movimiento, que lo llama «deshechos», lo nombre igual).
5. **Open Questions:** cerrar «¿Las partidas Sin límite cuentan para la compuerta?» (no: Métricas regla 12), «¿El reloj decide partidas…?» (se mide: Métricas regla 14) y «`N_base` y `Δturnos`…» (Métricas F4).
6. **«Criterios que no se automatizan»:** la compuerta es «3 partidas seguidas que terminan en el tablero (Rey muerto, doble Rey o rendición), con timer y contra un humano, en 25:00 o menos de duración de pared» (Métricas regla 12), no solo «≤ 25 min».
7. **Dependencies, fila #19:** de «Pendiente» a «Existe».

**`resolucion-simultanea-del-dano.md`**
1. **Open Questions, `vidaQuitada`:** cerrarla. Métricas usa `vidaQuitada` tal como está, con la pieza y el tramo por separado, y cuenta los golpes aparte con `AtaqueEjecutado` (Métricas regla 14).
2. **Formulas o Dependencies (fila #3):** declarar que el `Δturnos` de Resolución es 0 por definición (Métricas F4; Máquina, punto 1).
3. **Dependencies, fila #19, y «Supuestos provisionales»:** #19 tiene GDD y confirma la forma de los eventos de la regla 14 **sin cambios**.
4. **Open Questions de la tortuga y del demoledor:** las medidas existen (Métricas regla 14).

**`modificador.md`**
1. **F5, variable `líder inicial`:** agregar «se mide con el primer evento de la tirada cuya tabla base no es Normal (Métricas F3)».
2. **Open Questions:** las de `Δturnos_D` y `t_atacar` (Métricas F4), `W` (F3), Grito ≥ 60% y estrategias degeneradas (regla 14) tienen medida.
3. **Dependencies, «Bidireccionalidad»:** #19 tiene GDD; la arista Dura queda confirmada.

**`reclutamiento-y-limites.md`**
1. **Dependencies, «Bidireccionalidad»:** #19 tiene GDD; la arista queda confirmada.
2. **Open Questions:** `bajas_por_turno` (Métricas F4), anillo lleno, tope de 6 Llamadas y Grito al 85% tienen medida (Métricas regla 14).

**`timer-de-turno.md`**
1. **Open Questions:** cerrar «¿Las partidas Sin límite cuentan para la compuerta?» (no) y marcar como medidas «¿El reloj por jugador decide partidas…?» y «Fijo 90… turno 17» (Métricas regla 14).
2. **Regla 10 o 12:** aclarar si el salto de reloj de más de `salto_reloj_max_ms` emite `RelojPausado`/`RelojReanudado`. Métricas F1 lo excluye en los dos casos, pero el registro es más claro si el Timer lo emite.
3. **Dependencies, fila #19:** de «Pendiente» a «Existe».

**`libreria-de-reglas.md`**
1. **Regla 14:** «Con Divergencia, la interfaz avisa al jugador y Métricas (#19) lo registra como S1»: Divergencia es online y Métricas solo corre en local (Métricas regla 1). Cambiar a «la Analítica online (#32) lo registra como S1».
2. **Interactions, fila 19:** sumar `Rechazado` con su código y `ErrorInterno` (S1) como entradas de Métricas.
3. **«Existen los GDD…»:** sumar #19.

**`interfaz-del-modificador.md`**
1. **AC-27 y la lista de PROVISIONAL:** quitar PROVISIONAL de AC-27: el esquema de Métricas guarda `PanelAbierto` tal como está (Métricas regla 4).
2. **«Bidireccionalidad»:** #19 tiene GDD.

**`ficha-de-pieza-y-hud.md`**
1. **UI (placa de fin de partida):** sumar el botón `Exportar métricas` y sus dos mensajes (Métricas UI Requirements 1).
2. **Interactions y Dependencies, fila #19:** Métricas **no** pide eventos nuevos para Deshacer y Rendirse: el deshacer sale del evento de la Máquina y `rendirseDisponible` de la consulta de la Librería. Lee `FichaSimple` y `ReducirEfectos` al empezar la partida. Quitar «si Métricas los pide».

**`castillo-y-muros.md`**
1. **Dependencies:** sumar #19 Métricas (Blanda; vía `TramoRoto` de Resolución).
2. **Open Questions:** «¿Vale la pena atacar el muro…?», «¿Cuánto suma el muro a la duración?» y las dos estrategias degeneradas tienen medida (Métricas regla 14). El `Δturnos` del muro no se separa con una fórmula: se lee en «turnos desde el corte al fin».

**`entrada-y-seleccion.md`**
1. **Dependencies e Interactions:** sumar #19 Métricas (Blanda). Al recibir un Rechazado de `Aplicar`, el cliente anota `Rechazo(codigo, comando)` (Métricas regla 4). Entrada no muestra nada nuevo.

**`movimiento.md`**
1. **Dependencies, fila #19:** nombrar el evento de deshacer con el nombre que fije la Máquina (punto 4 de la Máquina).
2. **Open Questions:** muralla, Rey sin cupo y rango de `limite_normal` tienen medida (Métricas regla 14).

**`catalogo-de-piezas.md`**
1. **Dependencies, fila #19:** de «Pendiente» a «Existe»; `versionBalance` y la huella van en el encabezado de cada registro (Métricas regla 3).

**`alcance-y-asignacion-de-ataques.md`**, **`cobertura-del-escudero.md`**, **`tablero-y-despliegue-inicial.md`**
1. **Open Questions** que nombran a #19: tienen medida (Métricas regla 14). Ningún cambio de reglas.

**`localizacion.md`**
1. Sumar las claves `ui.metricas_*` (Métricas UI Requirements 3).

**`systems-index.md`** (no es un GDD, pero lo nombra la compuerta)
1. Fila 19: Status a «Designed», con el link al GDD.
2. Decisiones del 2026-11-19 («Compuerta del arte») y la fila «Duración de partida» de High-Risk Systems: precisar que las 3 partidas tienen que **terminar en el tablero**, con timer y contra un humano (Métricas regla 12).
3. Sistema 19, «Uso: pasos del tutorial»: queda reservado en el esquema hasta el Tutorial (#24, Vertical Slice).

**`game-concept.md`** (fuera del pedido de esta tanda; para quien cierre)
1. «Scope Tiers», fila Vertical Slice, y «Design Risks»: la misma precisión de la compuerta que en el índice.

## Acceptance Criteria

*Tipos de historia (coding-standards): **Logic** = test automático en `Assets/Tests/EditMode/`; **Integration** = test de integración o playtest documentado; **UI** = captura retenida en `production/qa/evidence/`; **Config/Data** = smoke check. Las fórmulas de la herramienta (F2 a F5, F7) se prueban con registros de prueba armados en el test (archivos de fixtures), nunca con partidas reales.*

### Reglas (Sección C)

- **AC-1 (regla 1, solo lectura)** *(Logic)* — **GIVEN** una partida de prueba con Métricas encendidas y otra igual con Métricas apagadas, **WHEN** se aplica la misma secuencia de 200 comandos en las dos, **THEN** los estados finales tienen la misma `Huella` y las listas de eventos son iguales. Métricas no envió ningún comando.
- **AC-2 (regla 2, ciclo)** *(Logic)* — **GIVEN** `CrearPartida` aceptado, **WHEN** se juega hasta `PartidaTerminada`, **THEN** hay exactamente un registro con encabezado, líneas y cierre, en ese orden, y su estado es `Completo`.
- **AC-3 (regla 3, encabezado)** *(Logic)* — **GIVEN** una partida creada con `limite_normal` 12, `vida_tramo` 30, modo Variable y `versionBalance` 7, **THEN** el encabezado trae esos valores, `rival` = `Humano`, `versionEsquema` = 1 y un `numeroPartidaLocal` una unidad mayor que el de la partida anterior. **GIVEN** las métricas apagadas, **WHEN** se crea una partida, **THEN** no hay registro, pero el `numeroPartidaLocal` de la siguiente subió en 2.
- **AC-4 (regla 4, esquema)** *(Logic)* — **GIVEN** una resolución con 3 soldados sobre un arquero cubierto (Resolución, F4, ejemplo), **THEN** el registro trae los eventos de la regla 14 de Resolución en su orden, con los mismos campos y valores que la lista que devolvió `Aplicar`, y cada línea con `t_ms` no decreciente. **AND** ninguna línea contiene un texto de Localización: solo códigos. **AND** la vista previa de ese turno no agregó ninguna línea.
- **AC-5 (regla 4, rechazo)** *(Integration, #13)* — **GIVEN** `Deshacer` con la pila vacía, **WHEN** el cliente recibe `NadaQueDeshacer`, **THEN** el registro tiene `Rechazo(NadaQueDeshacer, Deshacer)` y el `ResumenTurno` cuenta 1 rechazo con ese código.
- **AC-6 (regla 5, resumen)** *(Logic)* — **GIVEN** un turno en que el jugador 0 mueve 12 piezas, deshace 2, asigna 5 ataques, mata a una pieza con un golpe combinado y abre el panel 3.000 ms, **WHEN** llega `TurnoTerminado`, **THEN** el `ResumenTurno` dice `piezasMovidas` 10, deshechos 2, ataques 5, una muerte combinada y `ms_panel` 3.000, con los `ms_mover` y `ms_atacar` de `TurnoMedido`.
- **AC-7 (regla 5, fuego amigo y tramo propio)** *(Logic)* — **GIVEN** una catapulta propia cuya área quita 6 de vida a un soldado propio y rompe un tramo propio, **THEN** el resumen da 6 de vida por fuego amigo y marca un tramo roto propio; la `vidaQuitada` del turno no cambia por ellos (Resolución, F4).
- **AC-8 (regla 6, sesión)** *(Integration)* — **GIVEN** un arranque del juego, **WHEN** se llega al tablero listo, **THEN** el registro de sesión trae dos `Carga` con sus milisegundos y la versión del build. En el MVP no hay ningún `TutorialPaso`.
- **AC-9 (regla 7, escritura por turno)** *(Logic)* — **GIVEN** un almacenamiento de prueba que cuenta escrituras, **WHEN** se juega un turno con 60 eventos, **THEN** hay exactamente una escritura, al llegar `TurnoTerminado`, y ninguna antes.
- **AC-10 (regla 7, truncado)** *(Logic)* — **GIVEN** `registro_max_kb` de prueba en 8 KB, **WHEN** un turno haría pasar el registro de 8 KB, **THEN** desde ese turno solo se escriben `ResumenTurno` y el cierre, el estado es `GrabandoTruncado` y, al terminar, el registro es `Completo` con la marca `truncado`.
- **AC-11 (regla 7, rotación)** *(Logic)* — **GIVEN** `partidas_guardadas_max` = 100 y 100 registros guardados, **WHEN** se cierra el registro 101, **THEN** quedan 100 y falta el de menor `numeroPartidaLocal`.
- **AC-12 (regla 8, incompleto)** *(Integration)* — **GIVEN** una partida en el turno 9, **WHEN** se recarga la página y el juego vuelve a arrancar, **THEN** ese registro tiene 8 `ResumenTurno` (los turnos cerrados), no tiene cierre y su estado es `Incompleto`.
- **AC-13 (regla 9, privacidad)** *(Logic)* — **GIVEN** un registro exportado de una partida completa, **WHEN** se busca en el archivo cualquier campo fuera de la lista del esquema, **THEN** no hay ninguno. **AND** durante la partida y la exportación el cliente no hizo ninguna petición de red (registro de red del navegador vacío para Métricas).
- **AC-14 (regla 10, exportar y borrar)** *(Integration)* — **GIVEN** 3 registros de partida y 2 de sesión guardados, **WHEN** se exporta, **THEN** se descarga un archivo con los 5 y siguen guardados. **WHEN** se borra (VS), **THEN** quedan 0 y el `numeroPartidaLocal` de la partida siguiente no vuelve a 1.
- **AC-15 (regla 11, fallo de escritura)** *(Logic)* — **GIVEN** un almacenamiento de prueba que falla siempre, **WHEN** se juega una partida completa, **THEN** la partida termina igual (misma `Huella` que sin el fallo), no hay ningún aviso en pantalla, hay una línea `Error(S3, EscrituraFallida)` en memoria y el registro queda `Incompleto` después de la segunda escritura fallida.
- **AC-16 (regla 11, ErrorInterno)** *(Logic)* — **GIVEN** una Librería de prueba que devuelve `ErrorInterno` en un comando, **THEN** el registro trae `Error(S1, ErrorInterno, comando)` con el código y el comando, y sin ningún campo del estado.
- **AC-17 (regla 12, quién cuenta)** *(Logic)* — **GIVEN** registros de prueba de un mismo grupo, **THEN** uno Sin límite, uno con `rival` = `RivalIA` y uno incompleto dan `cuenta` = falso y no cambian la racha; uno con modo Fijo 60 completo da `cuenta` = verdadero.
- **AC-18 (regla 12, grupos y huecos)** *(Logic)* — **GIVEN** las partidas 1 a 3 que pasan con `versionBalance` 7 y la 4 que pasa con `versionBalance` 8, **THEN** el grupo 7 está abierto (racha 3) y el grupo 8 tiene racha 1. **GIVEN** las partidas 5, 7 y 8 que pasan en un grupo y la 6 ausente, **THEN** la racha es 3 con «racha con huecos (1): 6».
- **AC-19 (regla 13, herramienta)** *(Logic)* — **GIVEN** un archivo con 2 registros de `versionEsquema` 1 y uno de `versionEsquema` 9, **WHEN** corre la herramienta, **THEN** el informe usa los 2 primeros y lista «no leído (versión 9)». **GIVEN** el mismo registro dos veces, **THEN** se cuenta una vez.
- **AC-20 (regla 14, medidas)** *(Logic)* — **GIVEN** un registro de prueba con 2 partidas terminadas por `RelojAgotado`, en una de las cuales el que perdió por reloj tenía más piezas vivas, **THEN** el informe da «2 por reloj; en 1, perdió el que iba adelante en piezas». **GIVEN** un turno del que pierde con `pGrito` 65, **THEN** cuenta en «turnos con Grito ≥ 60%».
- **AC-21 (reglas 7 y 11, presupuesto)** *(Integration, rendimiento)* — **GIVEN** el build web de referencia (Librería, ADR) y una resolución de 393 eventos, **WHEN** se mide el cuadro de `Resolver`, **THEN** el costo de Métricas es ≤ `metricas_comando_max_us` (1.000 µs) y la escritura del cierre del turno es ≤ `metricas_escritura_max_ms` (4 ms) en el hilo principal, sin reservas de memoria durante la captura.
- **AC-22 (regla 15)** *(Logic)* — **GIVEN** el código del cliente de Métricas, **WHEN** se revisan sus llamadas a la Librería, **THEN** solo usa consultas puras y la suscripción a eventos; no hay ninguna llamada a `Aplicar`, `Resincronizar` ni `Cargar` (análisis estático del ensamblado).

### Fórmulas (Sección D)

- **AC-23 (F1)** *(Logic)* — **GIVEN** `t_cierre_ms` 1.412.300, una pausa de 30.000 ms y un hueco de cuadro de 8.000 ms dentro de ella, **THEN** `duracion_pared_s` = 1.382. **GIVEN** una pausa sin reanudar que empieza en 1.400.000, **THEN** se cierra en `t_cierre_ms` y da 1.400. **GIVEN** un hueco de cuadro de 4.999 ms, **THEN** cuenta; con 5.001, no.
- **AC-24 (F2)** *(Logic)* — **GIVEN** las 6 partidas del ejemplo de F2, **THEN** las rachas son 1, 1, 0, 1, 2, 3 y la compuerta está abierta. **GIVEN** una partida ReyMuerto de 1.501 s, **THEN** no pasa y la racha vuelve a 0; con 1.500 s, pasa. **GIVEN** una partida `Tablas`, **THEN** racha 0.
- **AC-25 (F3)** *(Logic)* — **GIVEN** 20 partidas con líder y 13 ganadas por él, **THEN** `W_pct` = 65 y «bajo el umbral»; con 14, 70 y «cumple»; con 19 partidas, «muestra insuficiente (19 de 20)» y sin veredicto (Modificador, AC-26). **GIVEN** una partida cuya primera tabla no Normal es «Pierde 3/4» del jugador 1, **THEN** el líder es el jugador 0. **GIVEN** una partida solo con tablas Normal, **THEN** no entra en `n`.
- **AC-26 (F4)** *(Logic)* — **GIVEN** 24 turnos, 30 bajas, 16 reclutas y 4 Distracciones, **THEN** `bajas_por_turno` 1, `delta_reclutamiento` 16 y `N_base_estimado` 4, marcado «cota inferior». **GIVEN** 60 bajas, **THEN** 3, 6 y 14. **GIVEN** 0 bajas, **THEN** `bajas_por_turno` 1. **GIVEN** 0 turnos con Distracción, **THEN** `t_atacar` «sin dato» (Reclutamiento, AC-21: 24 reclutas y 3 bajas por turno dan 8).
- **AC-27 (F5)** *(Logic)* — **GIVEN** 34 turnos con ataque, 28 con combinado y 21 con muerte combinada, **THEN** 82% y 61%. **GIVEN** dos atacantes cuyo daño absorbe entero el escudo, **THEN** el turno cuenta en `c` y no en `c_letal`. **GIVEN** 0 turnos con ataque, **THEN** «sin dato».
- **AC-28 (F6)** *(Logic)* — **GIVEN** un turno de 52.000 ms con 3.110 cuadros, 4 de más de 50 ms y el peor de 81 ms, **THEN** `fps_x10` 598, `lentos` 4, `peor_ms` 81. **GIVEN** `ms_turno` 0, **THEN** «sin dato» y `lentos` 0.
- **AC-29 (F7)** *(Config/Data)* — **GIVEN** la config vigente, **WHEN** se calcula `kb_almacen_max`, **THEN** da 52.480 KB. **GIVEN** 5 partidas reales del MVP, **WHEN** se mide el tamaño de sus registros, **THEN** cada uno es ≤ `registro_max_kb` y el informe da el tamaño medio (para reemplazar la estimación de `bytes_linea`).

### Cruces y presentación

- **AC-30 (cruce con #3 y #12)** *(Integration)* — **GIVEN** una partida en modo Variable jugada hasta Rey muerto, **THEN** la Σ de `ms_mover` + `ms_atacar` de los `TurnoMedido` dividida por 1.000 es la `duracion_partida` de la Máquina (D1) y es ≤ `duracion_pared_s` (F1).
- **AC-31 (cruce con #11)** *(Integration)* — **GIVEN** una partida en que sale Distracción, **THEN** su `ResumenTurno` tiene la cara Distracción, `ms_mover` 0 y `vencio_mover` falso, y cuenta en `D` (F4).
- **AC-32 (UI, Exportar)** *(UI)* — **GIVEN** la placa de fin de partida en el build web, **WHEN** se toca `Exportar métricas`, **THEN** se descarga el archivo y la placa dice `Métricas exportadas (k partidas)` 2 s. **AND** sin registros dice `No hay métricas guardadas`. Evidencia: captura retenida de los dos estados en `production/qa/evidence/`.

**Criterios que no se automatizan:** que el jugador no note a Métricas (playtest: nadie menciona tirones al cerrar el turno); que el informe se entienda sin leer este GDD (lo lee el diseñador después del primer playtest).

**Cobertura (lente de QA):** reglas 1 (AC-1, AC-22), 2 (AC-2), 3 (AC-3), 4 (AC-4, AC-5), 5 (AC-6, AC-7, AC-31), 6 (AC-8), 7 (AC-9, AC-10, AC-11, AC-21), 8 (AC-12), 9 (AC-13), 10 (AC-14), 11 (AC-15, AC-16, AC-21), 12 (AC-17, AC-18, AC-24), 13 (AC-19), 14 (AC-20), 15 (AC-22); F1 (AC-23, AC-30), F2 (AC-24), F3 (AC-25), F4 (AC-26), F5 (AC-27), F6 (AC-28, AC-21), F7 (AC-29). **Huecos:** la regla 14 tiene 16 medidas y AC-20 prueba dos; las demás se prueban con un fixture por medida al escribir las historias. AC-21 depende del build de referencia del ADR de la Librería. AC-5 usa el nombre del rechazo de la Máquina; el del evento de deshacer sigue PROVISIONAL (Cross-References, Máquina punto 4).

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| Formato del archivo (JSON por línea u otro), dónde se guarda en el navegador desde Unity Web, cómo se descarga y cuánto cuesta escribir en el hilo principal (F6, F7: **NOT SOURCEABLE** en la referencia del motor) | ADR de Métricas (`/architecture-decision`) | Antes de implementar #19 |
| ¿Entra al esquema el evento de cliente que Render (#18) puede entregar, `SkinRechazada(jugador, motivo)`? Hoy no (regla 4: un evento nuevo sube `versionEsquema`); el uso de la vista previa (#16) tampoco se guarda (regla 4). ~~`ResolucionSaltada(turno, ms_transcurridos)`~~ **cerrada 2026-11-26:** el evento ya no existe, porque la cosecha no se salta (Render #18, regla 24) (decisión del 2026-11-26) | Este GDD, con Render (#18) y Vista previa (#16) | Después de las primeras partidas del MVP |
| Cuota real del almacenamiento del navegador en las plataformas web objetivo; si es menor que unos 51 MB, bajar `partidas_guardadas_max` | ADR de Métricas | Antes de implementar #19 |
| Lenguaje y entrega de la herramienta de informe (consola C# que reusa los tipos del esquema, u otra) | ADR de Métricas, `tools-programmer` | Antes del primer playtest del MVP |
| ~~Nombre del evento de `Deshacer`~~ **Resuelta 2026-11-26:** `MovimientoDeshecho`, en la lista cerrada de la Máquina (regla 16) | — | — |
| ¿`N_base_estimado` sirve de algo con remontadas fuertes, o hace falta un playtest sin cartas de reclutar para medir `N_base` de verdad? | Máquina (#3), playtest | Primeras 3 partidas del MVP |
| Disciplina de playtest: exportar después de cada sesión para que la rotación no borre partidas | Diseñador (proceso) | Primer playtest del MVP |
| ¿El esquema necesita eventos de la vista previa (cuántas veces se mira antes de resolver)? Hoy no se guardan (regla 4) | Vista previa (#16), playtest | Al escribir #16 o tras los primeros playtests |
| Pasos del tutorial: `TutorialPaso(paso, resultado)` está reservado; el Tutorial confirma los pasos y qué es «abandonado» | Tutorial (#24) | En su GDD (Vertical Slice) |
| ¿Las partidas contra el Rival IA (#25) se informan con su propio grupo (para medir la IA) además de no contar para la compuerta? | Rival IA (#25) | En su GDD |
| Online: la Analítica (#32) reusa el esquema, suma consentimiento y Divergencia (S1). ¿Sube `versionEsquema` o es un esquema hermano? | Analítica (#32) | En su GDD (Alpha 2a) |
| **Pedidos de la tanda B** (Máquina #3, Timer #12 y Castillo #5, Approved; cierre del 2026-11-28): la lista va debajo de esta tabla | Métricas (#19), con #3, #12 y #5 | En su `/design-review` (tanda E) |

### Pedidos de la tanda B (cierre del 2026-11-28)

Contratos y medidas que piden tres GDD aprobados (Máquina #3, Timer #12 y Castillo #5). Se aplican en la revisión de este GDD (tanda E); hasta entonces mandan sobre lo que diga este documento.

**De la Máquina (D1, AC-25 y AC-43):**
- **D1 en ms, sin redondeos intermedios** (reemplaza la fórmula de «bolsa común» `⌊T_tope ÷ seg_medio⌋`; donde este GDD dice `seg_turno_t`, D1 ahora usa `consumido_ms_t`, y `seg_medio` queda solo como dato informativo del informe): `k_J = ⌈(reloj_jugador_s × 1.000 × N_J) ÷ max(duracion_ms_J, 1.000 × N_J)⌉ − 1` (y `reloj_jugador_s − 1` con N_J = 0), `turnos_presupuesto = min(2·k_0, 2·k_1 + 1)`, rango 16–1.498 (registro). La calcula la herramienta de informe.
- **Qué turnos cuentan:** los cerrados con `TurnoTerminado` **y el cortado por `RelojAgotado`**; no cuentan los cortados por `Rendirse`, `CerrarPartida(Abandono)` o Tablas. `consumido_ms_t` = `ms_mover + ms_atacar` del `TurnoMedido`; un `TurnoMedido` con `parcial` verdadero cuenta solo si `PartidaTerminada.motivo` = `RelojAgotado`; un último turno cerrado con ganador (`ReyMuerto`, `DobleRey`) tiene `parcial` falso y cuenta. Métricas necesita, por turno, `consumido_ms` y el motivo del corte del último turno.
- **Espejo de AC-25:** 60 → 24, 75 → 18, 70/72 → 20, piso de 1.000 ms → 1.498, N_J = 0 → 24, 685.000 ms → 20; azul con 8 turnos y 720.000 ms y un 9.º cortado por `Rendirse` a los 5.000 ms → 16 (17 si contara); azul con 8 turnos y 662.000 ms y un 9.º cortado por reloj con 88.000 ms, rojo con 8 y 720.000 ms → 16 (17 si se excluyera); 9.º turno cerrado con `ReyMuerto` a los 20.000 ms → 17; corte por `Abandono` a los 5.000 ms → 16.
- **Cierres:** distinguir los cierres por timeout de los demás (hoy solo `Reclutado` lleva `automatico`); categoría `RelojAgotado` con `ataquesSinResolver` no vacío; piezas vivas de cada lado al cierre (AC-43); bando ganador y motivo desde la primera partida (DW-3). AC-43: con ≥ 5 `RelojAgotado` para la 2.ª condición y el % con ataques sin resolver; el umbral de ≤ 20% sobre 10 partidas es laxo: medir la estrategia de agotar el reloj del rival cerrando turnos rápidos.
- l.202, `T_tope` 1.290–1.500: confirmado (la Máquina se alinea al Timer).
- Medir segundos por pieza movida y por ataque (Open Questions de la Máquina).
- F2 compara la pared con `T_tope`, y la pared suma ≤ 3,5 s por turno de esperas fuera de los relojes: decidir si la compuerta mide pared o reloj consumido (la Máquina dice «la medida que fije #19»).

**Del Timer (esquema y disparadores):**
- `TurnoMedido` suma `relojInicio_ms` y `parcial`; `parcial` = la partida terminó sin el `TurnoTerminado` de ese turno (`ReyMuerto` y `DobleRey` no son parciales). Si la partida termina antes de armarse la primera etapa de un turno (Rendirse o Abandono en la cosecha o la placa, Tablas automáticas al abrirlo), sale `TurnoMedido(n + 1, j, 0, 0, falso, falso, vacío, verdadero)`; un turno abre para la medición cuando sube `numeroTurno`. El de Tablas automáticas y el de un Rendirse en la cosecha son iguales: se distinguen con `PartidaTerminada.motivo`. El Timer emite `TurnoMedido(parcial)` antes de `RelojesAlCierre`.
- Evento nuevo `SaltoDetectado(ms)`; `RelojPausado` y `RelojReanudado` no ocurren en el MVP (ningún motivo de pausa se usa). F1 `ms_excluido`: decidir si los huecos de más de `salto_reloj_max_ms` siguen fuera de la duración de pared (el reloj ahora sí los cobra).
- Contar las etapas vencidas sin ningún comando (DW-4) y separar `vencio_atacar` de las etapas vacías.
- Disparadores: ventaja de rojo (≥ 20 `RelojAgotado` y azul pierde más del 65%); **punto muerto: 20 turnos seguidos sin que suba la `vidaQuitada` de ninguno ni muera ninguna pieza** (también lo pide el Tablero, #4); `SaltoDetectado` con una etapa corriendo en más del 5% de las partidas. AC-35 cruza `TurnoMedido` antes y después del nivel 1 del reloj.

**De Castillo y muros (medidas de sus Open Questions):**
- **DW-6 (a):** cortes causados solo por una explosión: un corte cuenta así si todo el `DanoATramo` de ese castillo en esa resolución viene de la explosión de un demoledor **rival** (meta ≤ 40%). Para derivarlo, cruzar `AtaqueEjecutado` en modo explosión con su zona (o que `DanoATramo` traiga la fuente, con #9).
- **DW-6 (b):** demoledores rivales que terminan un turno de su dueño sin explotar con un recorrido de costo ≤ 8 hasta una casilla vecina de un tramo en pie del defensor, y cuántos mueren antes de explotar (meta ≥ 30%).
- **Corte propio:** en una resolución sin daño a enemigos, con los casos mixtos aparte (techo: 5% de las partidas).
- Tramos rivales rotos por ataques asignados (sin rebote ni fuego amigo).
- **Pilar 3:** diferencia de piezas vivas sin el Rey al terminar el turno 8 de cada jugador; victoria del que iba perdiendo, con corte y sin corte (N ≥ 30 partidas con corte); victoria del que corta primero, con material parejo (±1).
- **Rey en el castillo:** tasa de muerte por turno-Rey adentro (muro entero) y afuera, separada por tramo de la partida (turnos 1–8, 9–16, 17+), con los arqueros en el muro.
- Turnos con al menos 1 arquero en el muro y proporción de turnos-arquero en el muro; mediana de duración de las partidas con corte.
- La lectura del riesgo (las tres preguntas por captura) es de playtest, no de Métricas.
