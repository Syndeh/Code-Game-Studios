# Interfaz del Modificador

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + Claude (lentes: game-designer, ux-designer, ui-programmer, art-director, technical-artist)
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: Pilar 3 Siempre hay vuelta, con límites (prueba 3b: probabilidades consultables); respeta 1 Planificar y cosechar y 4 Duelo corto
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-25. (1) Pilar 3b contra 4: el panel modal no pausa el reloj, así que consultar cuesta tiempo real; el costo está declarado pero sin medir (`ms_abierto`, reglas 7 y 17). (2) El nombre neutro de la tabla base (regla 11) no impide inferir quién va perdiendo: se mide en playtest (Open Questions). Pendiente de confirmar con los GDD vecinos: las consultas pedidas al núcleo (id de la tabla base, `motivo_bloqueo`).

## Summary

La **Interfaz del Modificador** es lo que el jugador ve y toca del dado: la placa que anuncia la cara al empezar el turno, la etiqueta que la deja escrita hasta el cierre, el recuadro donde elige qué reclutar y el panel «?» que muestra las probabilidades de los dos jugadores con el motivo de cada cara que no puede salir. Existe para cumplir el pilar 3b (las probabilidades se consultan en cualquier momento) sin que el dado le cueste tiempo al turno: la placa no bloquea, el recuadro se toca desde el primer cuadro y el panel no pausa el reloj. No decide reglas ni calcula probabilidades: lee el núcleo y le pide a Entrada que mande el comando.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `Modificador (#11), Reclutamiento y límites (#10), Entrada y selección (#13), Localización (#14), Máquina de turno (#3), Librería de reglas (#1)`

## Overview

La **Interfaz del Modificador** es todo lo que el jugador ve y toca del dado adaptativo (#11) y del reclutamiento (#10): la **placa** que anuncia la cara al empezar el turno, la **etiqueta** que mantiene su nombre a la vista hasta cerrarlo, el **recuadro de reclutar** donde elige el tipo de pieza cuando sale Llamada al servicio o Cuerno de guerra, y el **panel «?»** que muestra, en cualquier momento, las probabilidades de la próxima tirada de los dos jugadores con el motivo de cada cara que no puede salir. No decide nada de reglas: lee las consultas puras del núcleo (`ProbabilidadesSiguienteTirada`, `estado_cara`, `llamadas_restantes`, `tipo_disponible`) y los eventos (`Tirada`, `Reclutado`, `ReclutaPerdido`), y para elegir un tipo le pide a Entrada (#13) que mande `Reclutar(tipo)`. Todo texto sale de Localización (#14); nada se arma en la interfaz.

Este sistema es dueño de cuatro cosas: **la disposición** (qué va dónde, con qué tamaño y en qué orden), **los tiempos de presentación** (cuánto dura la placa, cuándo se acopla a la etiqueta, cuándo se abre el recuadro), **los toques sobre sus elementos** (el «?», el panel y las opciones del recuadro) y **los textos de las cinco caras y de sus recuadros** (la pregunta abierta que Localización le dejó). Las reglas de cada cara son del Modificador, las reglas de reclutar son de Reclutamiento y el gesto de mandar un comando es de Entrada: este GDD no las redefine.

**Por qué existe.** El pilar 3 pide que las probabilidades «se puedan consultar en cualquier momento antes de tirar» (prueba 3b), y los pilares 1 y 4 piden que el dado se resuelva a la vista, antes de planificar y sin costarle tiempo al turno. Con probabilidades adaptativas que se reparten entre caras, un panel que solo mostrara porcentajes parecería un dado arbitrario: el jugador tiene que poder ver *por qué* el Grito está en 45% o por qué Llamada está en 0. Sin esta interfaz, el dado sería correcto y, para el jugador, indistinguible de uno tramposo.

**Qué no es.** No es la ficha de pieza ni los relojes (HUD, #15), ni la vista previa de daño (#16), ni el dibujo de las piezas que aparecen o las casillas numeradas (Render, #18), ni el sonido (#22). No calcula probabilidades ni decide qué cara puede salir. No es un simulador: el panel muestra la tabla con el tablero *de ahora*, no la que resultaría de los ataques asignados.

**Decisiones de esta sesión (jugador, 2026-11-25):**

1. El «?» abre un **panel modal central**: mientras está abierto el tablero no recibe toques y **el reloj del turno no se pausa** (el Timer, #12, regla 10, no tiene un motivo de pausa para esto).
2. El panel explica cada cara con **su porcentaje efectivo, «base X, ahora Y» y un motivo corto**, más una línea fija sobre el reparto. No hay una frase por cada destino de reparto.
3. El panel muestra el **nombre de la tabla base con un nombre neutro** (según cuánto se inclina hacia reclutar o hacia el empuje), sin decir quién va perdiendo.
4. La fantasía de la sección es **«El azar, a la vista»**.

## Player Fantasy

**El azar, a la vista.** Nada del dado queda escondido. Vas perdiendo 6 contra 13, tocás el «?» y lo ves: tu tabla con el Grito de guerra en 45%, Llamada y Cuerno en 0 con la palabra «Castillo cortado» al lado, y la del rival con las suyas. Sabés qué dado vas a tirar, por qué y qué te sirve; y lo que sale al empezar tu turno cae al centro, se asienta y se queda escrito arriba hasta que termines. Si algo te pareció injusto, la interfaz tiene la cuenta.

**Momento ancla:** 6 piezas contra 13 y dos tramos de tu castillo rotos. Abrís el «?» en medio de la planificación. El panel central te muestra, en tu columna, **Distracción 15 · Grito 45 (base 20, ahora 45) · Llamada 0 «Castillo cortado» · Cuerno 0 «Castillo cortado» · Arremetida 40**, y en la del rival la suya. Lo cerrás tocando afuera; el reloj siguió corriendo todo ese rato y lo sabías.

**Pilar que sirve:** Pilar 3, prueba 3b: *«sus probabilidades se pueden consultar en cualquier momento antes de tirar»*. Esta interfaz es esa frase hecha pantalla. Respeta el pilar 1 (*«el azar se resuelve antes de planificar, nunca entre el plan y la cosecha»*: la placa llega primero, se ve entera y no bloquea) y el pilar 4 (nada de lo que muestra suma tiempo al turno).

**Qué le exige al resto del diseño:**

- La tabla que muestra el panel es la **efectiva**, la que realmente se usa, nunca la base (Modificador, regla 10).
- Cada 0% y cada cara que sube tiene su motivo escrito: un número que cambió sin explicación se lee como trampa.
- La placa y el recuadro de reclutar **no pueden hacerle perder tiempo al jugador**: la primera orden válida se acepta apenas la fase abre.
- El nombre de la cara de este turno está a la vista durante todo el turno, no solo mientras cae la placa.
- Todo dato se alcanza con un toque, nunca con el mouse encima.

**Lo que este encuadre no promete:** que el panel enseñe la estrategia. Muestra probabilidades y motivos, no consejos ni qué pasaría si resolvieras los ataques asignados. Tampoco promete que el jugador lo abra: es una consulta, no una pantalla obligatoria. Cuánto lo usan y si entienden los repartos se mide (Open Questions).

*Descartados:* **«La mesa servida, sin demora»** (la rapidez de la placa es una restricción de esta interfaz y ya vive en el pilar 4, pero no es su fantasía) y **«Elegir mirando el tablero»** (es la fantasía de Reclutamiento, «Llegan los refuerzos»; aquí queda como requisito de las casillas numeradas).

## Detailed Design

### Core Rules

**Vistas por rol.** *game-designer:* el panel es una consulta, no una pantalla: si costara turno, el jugador dejaría de usarlo y el pilar 3b quedaría cumplido solo en el papel; por eso el modal no pausa el reloj y no simula resultados. *ux-designer:* consultar cuesta un toque; la respuesta está completa en el primer cuadro; el velo solo cierra; ningún dato depende del hover. *ui-programmer:* todo es función pura del estado público y de los eventos; la interfaz guarda claves y parámetros, no textos, para que el cambio de idioma sea instantáneo (Localización, regla 12). *art-director:* cada cara se reconoce por silueta en grises y la placa es el único «golpe» de la interfaz (art bible §7). *technical-artist:* las casillas numeradas viven en el tablero (Render, #18) y el panel y el recuadro viven en la capa de UI; el borde entre las dos capas no está cubierto por la referencia del motor.

1. **Qué es y de dónde lee.** La interfaz es una función del estado público de la partida y de los eventos. **No escribe** en el estado ni en la huella: lo único que guarda es estado local de presentación (panel abierto, animación en curso, aviso en cola). Lee `ProbabilidadesSiguienteTirada(estado, jugador)` (Librería, regla 6), `estado_cara`, `llamadas_restantes` y `tipo_disponible` (Reclutamiento, F1 a F3), `motivo_bloqueo(jugador, tipo)` (Reclutamiento, **consulta pedida**, ver Interactions), `orden_aparicion` (Reclutamiento, F4), la fase, el jugador del turno y `contadorTirada`; y los eventos `Tirada`, `Reclutado`, `ReclutaPerdido`, `FaseIniciada` y `TurnoTerminado`. Un evento `Tirada` lleva `n`, la cara, la tabla efectiva usada y el nombre de la tabla base (Librería, AC-8; Modificador, regla 9).

2. **Cuatro elementos.**

   | Elemento | Existe | Muestra | Toques |
   |---|---|---|---|
   | **Placa** de la cara | Al aplicarse una `Tirada` válida (turno 2 en adelante) con una cara que no recluta | Arriba, el renglón del turno (emblema y `Tu turno` / `Turno de [bando]`, que reemplaza al banner del HUD, #15 regla 13); debajo, ícono y nombre de la cara, al centro | Ninguno: los toques la atraviesan hacia el tablero |
   | **Etiqueta** de la cara | Todo el turno, desde el primero | Emblema del bando de turno, ícono y nombre de la cara (o su variante, regla 5) | Ninguno |
   | **Recuadro de reclutar** | Con una cara de reclutar, mientras dura la fase `Reclutar` | Encabezado con el renglón del turno y la cara, y hasta 3 tarjetas, una por tipo | Una tarjeta = un tipo (regla 6) |
   | **Botón «?» y panel** | Todo el tiempo con la partida en curso | Tabla efectiva de los dos jugadores con motivos (reglas 7 a 11) | El botón, la X y el velo (regla 7) |

3. **Cuándo se dispara la placa.** Con el evento `Tirada` aplicado, **una sola vez por `contadorTirada`**. En local la tirada se aplica cuando termina la presentación de la resolución del turno anterior, que no se salta (decisión del 2026-11-26), o, si el turno se cerró sin resolución, cuando terminan la placa en curso y la leyenda central (la inyecta el cliente, Modificador, regla 9; Render, reglas 24 y 25); el renglón del turno que lleva la placa reemplaza al banner del turno del HUD (decisión del jugador, 2026-11-26); online, cuando llega del servidor. **No hay placa** en el primer turno (Máquina, regla 3), ni con una `Tirada` rechazada (`TiradaInvalida`, contador viejo: nada cambia en pantalla), ni al reconectar con una tirada ya aplicada (se muestra la etiqueta directo, sin animación). Contenido: el ícono de la cara y su nombre (`cara.<id>`, regla 13) en 34 a 40 px (art bible §7). Tamaño máximo: 9 columnas por 2 filas, centrada.

4. **Línea de tiempo de la placa (F1).** Caída de 0,20 s con el golpe seco al asentarse, 0,60 s de pausa legible, acople de 0,20 s hacia la etiqueta y 0,20 s de pulso de la etiqueta: **1,2 s** en total y **1,0 s como máximo tapando el tablero** (art bible §2: nunca más de 1 s sobre una casilla con pieza). **Distracción** suma 0,30 s en la franja superior (el estandarte del HUD se dobla y cae, con la etiqueta desaturada): 1,5 s en total, pero la placa gris sobre el tablero dura lo mismo que las otras. La placa **no bloquea nada**: la fase siguiente ya está abierta, el primer comando válido se acepta de inmediato y los toques sobre la placa llegan al tablero (Entrada, reglas 4 y 6). Tampoco cuesta reloj: en `EsperandoTirada` no corre ninguno (Máquina, regla 14) y, una vez abierta la fase, la placa no frena un comando. Con la opción «Reducir efectos» (art bible §7) la placa aparece por fundido de 0,10 s, sin caída ni golpe, y la etiqueta no pulsa; el resto de los tiempos no cambia.

5. **Etiqueta de la cara (siempre a la vista).** Franja superior de 60 px, a la izquierda del timer (posición confirmada por el HUD, #15, regla 15). Contiene el emblema del bando de turno, el ícono de la cara y su nombre (presupuesto de etiqueta, 24 caracteres). Variantes:

   | Situación | Qué muestra |
   |---|---|
   | Primer turno de la partida (sin tirada) | `ui.modif_etiqueta_apertura` («Apertura: sin dado»), sin ícono de cara |
   | `EsperandoTirada` visible (solo online) | Ícono de dado y `ui.modif_etiqueta_tirando` («Tirando el dado…»). **Sin cara ni predicción** |
   | Tirada resuelta | Ícono y nombre de la cara, hasta cerrar el turno |
   | Turno cerrado | Se limpia con el cierre (Máquina, regla 11) y no se muestra entre turnos |

   La etiqueta no repite los bonos: el +2 del Grito se lee en la ficha (#15) y en la vista previa (#16). Con el Grito la etiqueta no cambia de forma.

6. **Recuadro de reclutar.** Se abre cuando la cara es Llamada al servicio o Cuerno de guerra y la Máquina abre `Reclutar`.
   - **Encabezado = la placa.** La placa de la cara es el encabezado del recuadro: cae en 0,20 s como cualquier placa, y bajo ella aparece `ui.reclutar_titulo`. **Las tarjetas ya están en su lugar y reciben toques desde el primer cuadro** (aparecen con un fundido de 0,10 s que no bloquea): el reloj de la etapa de mover corre desde que se abre `Reclutar` (Máquina, regla 14) y la placa no puede costarle nada al jugador. El encabezado se queda mientras el recuadro está abierto, a diferencia de la placa normal.
   - **Tarjetas.** Una por tipo de la cara, en el orden del Catálogo (Llamada: soldado, arquero, escudero; Cuerno: catapulta, jinete, demoledor), en fila horizontal: 3 columnas de ancho (204 px) y 3 filas de alto (204 px) cada una (F5). El recuadro completo mide 668 px por 336 px (unas 10 columnas por 5 filas), centrado: **no toca los anillos** (columnas 0 a 5 y 18 a 23; F5).
   - **Contenido de una tarjeta, de arriba abajo:** ícono del tipo; `{n} ×` y el nombre del tipo en plural (`pieza.<tipo>` con `n` = piezas por llamada, Reclutamiento, regla 1); `ui.reclutar_casillas` («Casillas 1 a 4»); vivas contra tope (`{v}/{tope}`) con ícono de piezas; llamadas que quedan (`{c}`) con ícono de la cara. Los números van con ícono, así que no dependen de una palabra para leerse.
   - **Tarjeta bloqueada.** Atenuada **con forma** (candado y trama, no solo menos color) **en lugar de** la línea de casillas (F4: no promete casillas que no hay) y con el motivo debajo: el texto de `codigo.<Código>` (Localización, regla 5) para el código que devuelve `motivo_bloqueo(jugador, tipo)`: `CastilloSinReclutar`, `SinLlamadas`, `TopeDePiezas` o `SinLugarParaReclutar`. Sigue siendo tocable: el toque manda el comando y el aviso de rechazo lo muestra Entrada (regla 14 de Entrada); **este recuadro no agrega un segundo mensaje**.
   - **Casillas de aparición.** Mientras el recuadro está abierto, el tablero marca numeradas las primeras `K` casillas libres del anillo propio, en el orden `orden_aparicion` (F4 de esta página). Render las dibuja; esta interfaz fija cuáles y cuándo. Cada tarjeta dice qué números ocupa (`Casillas 1 a {n}`): el vínculo es por número, no por color. Las de 1 o 2 piezas son las primeras de las de 4 (Reclutamiento, UI Requirements).
   - **Elegir.** Tocar una tarjeta le entrega el `tipo` a Entrada, que manda `Reclutar(tipo)` con su `contadorEsperado` (Entrada, regla 12). Esta interfaz **no manda comandos por su cuenta**: así el contador, la ventana antidoble y los avisos de rechazo siguen en un solo lugar.
   - **Cerrar.** El recuadro se cierra cuando el estado deja de estar en `Reclutar` (por una elección aceptada o por el timeout): 0,20 s, y el encabezado se acopla a la etiqueta. Si el estado cambió mientras una animación corría, la animación salta a su estado final en el cuadro siguiente.
   - **Timeout en `Reclutar`.** La Máquina recluta sola (Reclutamiento, regla 10): el recuadro se cierra igual y el texto `ui.reclutar_auto` («Elegido por el tiempo») sale como aviso transitorio en la zona de avisos del HUD, sin tono de error; no hay ícono ni marca sobre la pieza ni la ficha (HUD, #15, reglas 18 y 19; decisión del jugador del 2026-11-26).
   - **`ReclutaPerdido`** (inalcanzable, Reclutamiento, regla 6): no se abre el recuadro; se muestra `ui.reclutar_perdido` («Sin reclutas disponibles», ≤ 1 s) en la placa central de leyendas de la Máquina (HUD, #15, regla 19).
   - **Quién lo ve.** Solo el jugador que decide: en local, el del turno; online, el propio. El rival online ve la placa y la etiqueta, y después las piezas nuevas (Render), pero no el recuadro.

7. **Botón «?» y modal.**
   - **Botón:** arriba a la derecha (art bible §7), blanco táctil de 60 × 60 px como mínimo. Visible en toda la partida en curso (cualquier fase, turno propio o ajeno, también en `EsperandoTirada`); oculto en `Terminada`.
   - **Abrir:** un toque. Transición ≤ 0,25 s; el contenido completo está dibujado **desde el primer cuadro** (sin entradas escalonadas).
   - **Modal (decisión del jugador, 2026-11-25).** Un velo del 40% cubre el tablero. Mientras el panel está abierto, **ni el tablero, ni los botones del HUD (Deshacer, avanzar, Rendirse), ni el recuadro de reclutar reciben toques**. Siguen a la vista y sin velo: el timer, los dos relojes, la etiqueta y el propio «?» (la franja superior).
   - **Cerrar:** tocar el «?», la X del panel o el velo. **El toque sobre el velo solo cierra** y nunca llega al tablero (evita mover o asignar por error al salir).
   - **El reloj no se pausa** (Timer, regla 10: no es uno de los motivos de pausa). Ni el panel ni sus textos dicen lo contrario.
   - **Se actualiza solo.** Mientras está abierto se vuelve a dibujar con cada cambio de estado (por ejemplo, el timeout de una etapa).
   - **Se cierra solo** al aplicarse una `Tirada` (la placa tiene prioridad sobre el panel y la tabla que mostraba dejó de ser la «próxima») y al terminar la partida. Un aviso que llega con el panel abierto (por ejemplo un rechazo de Entrada) se muestra al cerrarlo, con su duración completa; si llega más de uno, queda el último.
   - **Tamaño:** 12 columnas de ancho (816 px a 1080p, columnas 6 a 17) y como máximo 9 filas de alto (filas 3 a 11), centrado (F5). No tapa los anillos de reclutamiento ni los castillos. En pantalla estrecha lo decide #41 (provisional).

8. **Contenido del panel.** Una tabla de **5 filas** (las caras en el orden fijo de la config: Distracción, Grito de guerra, Llamada al servicio, Cuerno de guerra, Arremetida) y tres columnas, más un pie.
   - **Columna de la cara:** ícono, nombre (`cara.<id>`) y una línea de efecto (`cara.<id>_efecto`, regla 13). Es la misma para los dos jugadores.
   - **Columnas «Vos» y «Rival»:** cada una con el emblema del bando, `ui.modif_panel_vos` o `ui.modif_panel_rival`, `ui.modif_panel_piezas` (las piezas que cuentan para elegir la tabla: con el Rey, sin muros) y el nombre de la tabla base (regla 10). **Quién es «Vos»:** en local, en la misma pantalla, el **jugador del turno**; online, siempre el jugador local, aunque el turno sea del rival.
   - **Subtítulo** común: `ui.modif_panel_subtitulo` («Con el tablero de ahora»). Dice que no es una predicción de cómo quedaría tras resolver.
   - **Pie:** llamadas restantes de cada jugador (regla 12) y la nota fija `ui.modif_panel_nota` (regla 13).
   - **Datos:** las dos columnas salen de `ProbabilidadesSiguienteTirada(estado, jugador)` para cada jugador: con la tirada pendiente coincide con la guardada; para el otro es la que tendría si su turno empezara ahora (Librería, regla 6).

9. **Celda de una cara (por jugador).** De arriba abajo:
   - una **barra** de largo proporcional al porcentaje efectivo, con una **marca** en el largo de la base;
   - `{efectivo}%` en tamaño grande;
   - si `efectivo ≠ base`: `ui.modif_base_ahora` («base 25, ahora 0») y un ícono de **subió** o **bajó** (forma, no color; F3);
   - si la cara no puede salir por su causa: un **chip de motivo** (regla 10).

   Una cara en 0% **no se oculta ni se reordena**: queda en su fila, con trama en la barra y su chip. **Distracción nunca muestra diferencia** (no recibe reparto, Modificador, regla 5). Una cara con base 0 en la tabla elegida muestra `0%` sin chip, porque nada la bloqueó.

10. **Motivo de una cara (F2).** El chip dice por qué una cara que la tabla base daba como posible no puede salir:

    | Motivo | Chip (`ui.modif_motivo_…`) | Cuándo |
    |---|---|---|
    | `CastilloSinReclutar` | «Castillo cortado» | El castillo no puede reclutar (2 tramos rotos, Castillo, F7) |
    | `SinLlamadas` | «Sin llamadas» | Ningún tipo de la cara tiene llamadas |
    | `TopeDePiezas` | «Tope de piezas» | La cara está «Sin lugar» y su primer tipo con llamadas pasaría el tope de piezas vivas |
    | `SinLugarParaReclutar` | «Sin lugar» | La cara está «Sin lugar» y su primer tipo con llamadas no entra en las casillas libres del anillo |
    | (propio) | «Ya movés todas» | Arremetida con `piezas_sin_rey ≤ limite_normal`: ya se pueden mover todas las piezas (Modificador, regla 5) |

    Los cuatro primeros son los códigos de `Reclutar` (Reclutamiento, regla 7) a nivel de cara; el último es de Arremetida. **Los nombres de los motivos y su orden son los de esos códigos**, no una lista paralela. Las caras que reciben el reparto no llevan chip: su cambio se lee en «base X, ahora Y» y en la nota fija.

11. **Nombre neutro de la tabla base (decisión del jugador, 2026-11-25).** Cada columna lleva, bajo `ui.modif_panel_tabla` («Tabla»), un nombre que describe hacia dónde se inclina la tabla y **no dice quién va perdiendo ni ganando**:

    | Tabla base (id de la config) | Clave | Español | Inglés (propuesta) |
    |---|---|---|---|
    | `pierde_medio` | `ui.modif_tabla_pierde_medio` | Muchos refuerzos | Many reinforcements |
    | `pierde_tres_cuartos` | `ui.modif_tabla_pierde_tres_cuartos` | Más refuerzos | More reinforcements |
    | `gana_dos_a_uno` | `ui.modif_tabla_gana_dos_a_uno` | Mucho empuje | Heavy push |
    | `gana_cuatro_a_tres` | `ui.modif_tabla_gana_cuatro_a_tres` | Más empuje | More push |
    | `normal` | `ui.modif_tabla_normal` | Normal | Normal |

    El «empuje» es el peso del Grito y de Arremetida; los «refuerzos», el de Llamada y Cuerno. Los ids son los nombres de las cinco tablas del Modificador (regla 4), que son datos de la config; **el id lo entrega el núcleo** (consulta pedida, Interactions). Las claves llevan el id; los textos, nunca las palabras «pierde» ni «gana» (AC-21).

12. **Llamadas restantes (pie del panel).** Para cada jugador, cuatro fichas con ícono y número de `llamadas_restantes(jugador, tipo)` (Reclutamiento, F2): la de **Llamada al servicio** (un solo contador compartido por soldado, arquero y escudero, con el ícono de la cara) y las de **catapulta, jinete y demoledor** (con el ícono del tipo). Solo el número: los máximos son datos de la config y no se repiten acá. Es el límite público que Reclutamiento exige («un límite que no se ve se siente injusto»).

13. **Textos y claves (Localización, #14).** Todo texto visible sale de una clave; el panel y el recuadro guardan clave y parámetros y vuelven a pedir el texto al recibir el aviso de cambio de idioma, sin reiniciar animaciones ni cerrar nada (Localización, regla 12). Las líneas de efecto son **frases completas** con parámetros, nunca trozos pegados. Los textos en inglés son **propuesta** hasta la revisión de un hablante nativo. Presupuestos de largo (Localización, F4): etiqueta 24, título 32, aviso 60, párrafo 240 y tres clases nuevas, **chip de motivo 18**, **línea de efecto 48** y **nota del panel 180** (F5 las justifica con el texto inflado; Cross-References).

    | Clave | Español | Inglés (propuesta) | Presupuesto | Parámetros |
    |---|---|---|---|---|
    | `cara.distraccion` | Distracción | Distraction | etiqueta | — |
    | `cara.grito` | Grito de guerra | War cry | etiqueta | — |
    | `cara.llamada` | Llamada al servicio | Call to arms | etiqueta | — |
    | `cara.cuerno` | Cuerno de guerra | War horn | etiqueta | — |
    | `cara.arremetida` | Arremetida | Charge | etiqueta | — |
    | `cara.distraccion_efecto` | Sin mover; atacás con lo que ya tenés | No move phase; you still attack | efecto 48 | — |
    | `cara.grito_efecto` | +{ataque} de ataque y +{movimiento} de movimiento por un turno | +{ataque} attack and +{movimiento} move for one turn | efecto 48 | `ataque`, `movimiento` (config del Grito) |
    | `cara.llamada_efecto` | Reclutás {n} soldados, arqueros o escuderos | Recruit {n} soldiers, archers or shieldbearers | efecto 48 | `n` (piezas por llamada) |
    | `cara.cuerno_efecto` | Reclutás jinetes, catapultas o un demoledor | Recruit riders, catapults or a demolisher | efecto 48 | — |
    | `cara.arremetida_efecto` | Movés todas las piezas que quieras | Move as many pieces as you like | efecto 48 | — |
    | `ui.modif_abrir_panel` | Ver probabilidades del dado | See dice odds | nombre accesible del botón | — |
    | `ui.modif_cerrar_panel` | Cerrar | Close | botón 18 | — |
    | `ui.modif_panel_titulo` | Próxima tirada | Next roll | título 32 | — |
    | `ui.modif_panel_subtitulo` | Con el tablero de ahora | With the board as it is | etiqueta 24 | — |
    | `ui.modif_panel_vos` / `ui.modif_panel_rival` | Vos / Rival | You / Opponent | etiqueta 24 | — |
    | `ui.modif_panel_piezas` (plural) | {n} pieza / {n} piezas | {n} piece / {n} pieces | etiqueta 24 | `n` (decide la forma) |
    | `ui.modif_panel_tabla` | Tabla | Table | etiqueta 24 | — |
    | `ui.modif_tabla_<id>` | (regla 11) | (regla 11) | etiqueta 24 | — |
    | `ui.modif_base_ahora` | base {base}, ahora {ahora} | base {base}, now {ahora} | etiqueta 24 | `base`, `ahora` |
    | `ui.modif_subio` / `ui.modif_bajo` | Subió / Bajó | Went up / Went down | nombre accesible del ícono | — |
    | `ui.modif_motivo_<Código>` (4) | Castillo cortado · Sin llamadas · Tope de piezas · Sin lugar | Castle cut off · No calls left · Piece cap · No room | chip 18 | — |
    | `ui.modif_motivo_ya_mueves_todas` | Ya movés todas | Can move all | chip 18 | — |
    | `ui.modif_panel_llamadas` | Llamadas que quedan | Calls left | etiqueta 24 | — |
    | `ui.modif_panel_nota` | Cada tabla se recalcula al empezar el turno con el tablero de ese momento. Lo que una cara no puede hacer se reparte entre las otras; Distracción no recibe nada. | Each table is recalculated at the start of the turn with the board at that moment. What a face can't do is shared among the other faces; Distraction receives none of it. | nota 180 | — |
    | `ui.modif_panel_error` | No se pudieron mostrar las probabilidades | Could not show the odds | aviso 60 | — |
    | `ui.modif_etiqueta_apertura` | Apertura: sin dado | Opening: no dice | etiqueta 24 | — |
    | `ui.modif_etiqueta_tirando` | Tirando el dado… | Rolling the die… | etiqueta 24 | — |
    | `ui.reclutar_titulo` | Elegí qué reclutar | Choose what to recruit | título 32 | — |
    | `ui.reclutar_casillas` (plural) | Casilla {n} / Casillas 1 a {n} | Square {n} / Squares 1 to {n} | etiqueta 24 | `n` (decide la forma) |
    | `ui.reclutar_vivas` / `ui.reclutar_llamadas` | Piezas vivas contra el tope / Llamadas que quedan | Living pieces against the cap / Calls left | nombre accesible de la ficha | — |
    | `ui.reclutar_auto` | Elegido por el tiempo | Chosen by the clock | aviso 60 | — |
    | `ui.reclutar_perdido` | Sin reclutas disponibles | No recruits available | aviso 60 | — |

14. **Accesibilidad.** Nada se distingue solo por color: las cinco caras se reconocen por silueta en grises; subió y bajó, por forma; una cara bloqueada, por candado y trama; el vínculo entre tarjetas y casillas, por número. Todo dato se alcanza con un toque, sin hover ni mantener pulsado. Todo botón sin texto tiene una clave con su nombre accesible. El panel se anuncia a un lector de pantalla como un diálogo con una tabla y devuelve el foco al «?» al cerrarse (alcance exacto: #23). El texto escala con el tamaño de texto del jugador (#23); el panel y el recuadro admiten el texto inflado de Localización F5 sin cortarlo (F5 de esta página). Con «Reducir efectos» ver regla 4.

15. **Lo que el jugador no puede hacer.** Elegir, relanzar o anticipar la cara (online no hay predicción en el cliente); cambiar la tabla de su turno; elegir las casillas de aparición; ocultar el recuadro de reclutar; mover o redimensionar el panel; pausar el reloj con el panel; abrir el panel con la partida terminada.

16. **El estado manda, la animación decora.** La presentación nunca retiene un estado: si el estado cambia mientras una animación corre, esta salta a su fin en el cuadro siguiente. Ninguna animación de este sistema suma tiempo al turno (pilar 4).

17. **Medición de uso.** Cada apertura del panel emite un evento de presentación `PanelAbierto(jugador, turno, fase, ms_abierto)` que Métricas (#19) puede guardar en el archivo local de la partida. Sin él, «¿se usa el panel?» y «¿se entienden los repartos?» no se pueden contestar con datos (Open Questions). No entra al núcleo ni a la huella.

18. **Identificadores estables para el tutorial (#24).** `modif_boton_ayuda`, `modif_panel`, `modif_etiqueta`, `modif_placa` y `modif_recuadro_reclutar`, para que el tutorial pueda resaltarlos sin conocer la disposición.

### States and Transitions

La interfaz no guarda estado de partida: solo estado de presentación, derivado de la fase y de los eventos. Dos máquinas independientes: la del **turno** (etiqueta, placa, recuadro) y la del **panel**.

**Máquina del turno (por turno):**

| Estado | Qué se ve | Entra cuando |
|---|---|---|
| `Apertura` | Etiqueta «Apertura: sin dado» | Turno 1: no hay tirada (Máquina, regla 3) |
| `Esperando` | Etiqueta «Tirando el dado…» con ícono de dado | Se abre `EsperandoTirada` y la tirada no se aplicó en el mismo cuadro (online) |
| `Placa` | Placa cayendo y asentándose (1,0 s sobre el tablero) | `Tirada` aplicada con Distracción, Grito o Arremetida |
| `Recuadro` | Encabezado-placa y tarjetas; casillas numeradas | `Tirada` aplicada con Llamada o Cuerno y fase `Reclutar` abierta |
| `Etiqueta` | Solo la etiqueta con la cara | Fin de la placa, o cierre del recuadro |
| `Limpia` | Sin etiqueta | Cierre del turno |

| De | A | Cuándo |
|---|---|---|
| `Limpia` o inicio | `Apertura` | Se abre el turno 1 |
| `Limpia` | `Esperando` | Se abre `EsperandoTirada` sin tirada aplicada (turno ≥ 2, online) |
| `Limpia` o `Esperando` | `Placa` | `Tirada` válida con cara que no recluta |
| `Limpia` o `Esperando` | `Recuadro` | `Tirada` válida con cara de reclutar y `Reclutar` abierta |
| `Placa` | `Etiqueta` | A los 1,2 s, o antes si el estado ya cambió (regla 16) |
| `Recuadro` | `Etiqueta` | `Reclutar` aceptado, recluta automático por timeout, o `ReclutaPerdido` |
| `Apertura`, `Etiqueta` | `Limpia` | `TurnoTerminado` |
| Cualquiera | `Limpia` | `CerrarPartida` o `Rendirse` |

No hay transición `Etiqueta` → `Placa`: una tirada nunca se repite (Modificador, regla 9).

**Máquina del panel:**

| Estado | Qué ocurre |
|---|---|
| `Cerrado` | Solo el botón «?» |
| `Abierto` | Velo, panel y tabla; el tablero y los botones del HUD no reciben toques; el reloj corre |

| De | A | Cuándo |
|---|---|---|
| `Cerrado` | `Abierto` | Toque en el «?» con la partida en curso |
| `Abierto` | `Cerrado` | Toque en el «?», la X o el velo; una `Tirada` aplicada; el fin de la partida |
| `Abierto` | `Abierto` | Cualquier cambio de estado: se vuelve a dibujar con los datos nuevos |

### Interactions with Other Systems

| Sistema | Entra a esta interfaz | Sale de esta interfaz | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | `ProbabilidadesSiguienteTirada`, evento `Tirada` (cara, `n`, tabla, tabla base), `contadorTirada`, config (bonos del Grito) | Nada (solo lectura) | Librería (contrato). **Pedido:** la consulta debe devolver también el **id de la tabla base** de cada jugador |
| #11 Modificador | Reglas de las caras, las 5 tablas, motivos de la tabla efectiva | Cumple su regla 10 (panel) y su regla 3 (qué hace cada cara) | Modificador (reglas); esta interfaz (cómo se ven) |
| #10 Reclutamiento | `estado_cara`, `tipo_disponible`, `llamadas_restantes`, `orden_aparicion`, códigos de `Reclutar`, evento `Reclutado` | El tipo elegido (vía Entrada) | Reclutamiento (reglas y contenido); esta interfaz (disposición y toques). **Pedido:** `motivo_bloqueo(jugador, tipo)` |
| #5 Castillo y muros | `puedeReclutar` (vía `motivo_bloqueo`) | — | Castillo |
| #2 Catálogo | Orden de tipos, `topeVivas`, nombres | — | Catálogo |
| #4 Tablero | Anillo, casillas libres, columna del frente | Las casillas a marcar (la lista `K`) | Tablero (geometría); Render (cómo se dibujan) |
| #3 Máquina | Fase, `FaseIniciada`, `ReclutaPerdido`, jugador del turno | — | Máquina |
| #12 Timer | Nada | Nada: el panel no pausa el reloj; la placa no lo cobra | Timer |
| #13 Entrada y selección | Comando `Reclutar(tipo)` enviado; avisos de rechazo; estado «modal abierto» | El `tipo` tocado; «el tablero no recibe toques mientras el panel está abierto» | Entrada (comandos y avisos); esta interfaz (los blancos de sus elementos) |
| #14 Localización | Texto por clave, aviso de cambio de idioma | Claves y parámetros de la regla 13 | Localización (mecanismo); esta interfaz (las claves) |
| #15 Ficha de pieza y HUD | Posición de la franja superior, relojes, bando de turno | La etiqueta, el «?» y su blanco táctil | HUD (disposición general); esta interfaz (sus elementos) |
| #16 Vista previa | — | Nada: el bono del Grito ya está en `EffectiveStats` | Vista previa |
| #18 Render | — | Qué casillas numerar y cuándo; el resplandor y el ambiente de la placa (la placa la anima esta interfaz, en la capa 9) | Render (cómo se dibuja en el tablero) |
| #19 Métricas | — | `PanelAbierto(...)` | Métricas (esquema) |
| #22 Audio | — | Una señal por cara, apertura del panel | Audio |
| #23 Accesibilidad, #41 Adaptación móvil | Tamaño de texto, «Reducir efectos», escala de pantalla | Nombres accesibles de los elementos | Cada GDD |
| #24 Tutorial | — | Identificadores estables (regla 18) | Tutorial |
| #25 Rival IA | — | Nada: la IA usa la consulta pura, no esta capa | Rival IA |
| #28 Servidor | La tirada cuando llega (online) | Nada | Servidor |

**Supuestos sobre GDD vecinos:** confirmados al cerrar la tanda 4 (2026-11-26): Render (#18, regla 13) dibuja las casillas numeradas; la placa vive en la capa 9 y la anima esta interfaz, y Render solo pone el resplandor y el ambiente de Distracción y de la tirada; Métricas (#19, regla 4) guarda `PanelAbierto` tal como está. Confirmados al cerrar la tanda 3 (2026-11-25): el HUD (#15) acepta la etiqueta a la izquierda del timer; Entrada (#13) suma el estado «panel abierto» y el reparto de toques de la regla 6; la Librería expone el id de la tabla base y Reclutamiento `motivo_bloqueo`.

## Formulas

Todas son de presentación: corren en el cliente, fuera del núcleo (que sigue sin `float`), y no entran en la huella. Usan sin redefinirlas `estado_cara` y `llamadas_restantes` (Reclutamiento), `ProbabilidadesSiguienteTirada` (Librería) y los presupuestos de largo y el inflado de prueba de Localización (F4 y F5). F2 y F3 son lógica y se prueban automáticamente; F1, F4 y F5 fijan tiempos y medidas que se verifican con capturas y mediciones.

### F1. Línea de tiempo de la placa

The `t_placa` formula is defined as:

`t_cubre = t_caida + t_pausa + t_acople` y `t_total = t_cubre + t_pulso`; con Distracción, `t_total_D = t_total + t_extra_distraccion`; con «Reducir efectos», `t_total_R = t_fundido + t_pausa + t_acople`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `t_caida` | float (s) | 0,10–0,25 | data file (cliente) | Caída de la placa; la interfaz no pasa de 0,25 s (art bible §7) |
| `t_pausa` | float (s) | 0,40–0,70 | data file (cliente) | Tiempo con la placa asentada |
| `t_acople` | float (s) | 0,10–0,25 | data file (cliente) | Viaje de la placa hacia la etiqueta |
| `t_pulso` | float (s) | 0–0,25 | data file (cliente) | Pulso de la etiqueta; ya no tapa el tablero |
| `t_extra_distraccion` | float (s) | 0–0,30 | data file (cliente) | Estandarte que cae en la franja superior, solo con Distracción |
| `t_fundido` | float (s) | 0,05–0,15 | data file (cliente) | Aparición con «Reducir efectos» |
| `t_cubre_max` | float (s) | 1,0 | constant (art bible §2) | Tiempo máximo sobre una casilla con pieza |
| `t_total_max` | float (s) | 1,2 | constant (Modificador, Visual/Audio) | Presupuesto de la placa |

**Output Range:** `t_cubre` ≤ 1,0 s; `t_total` ≤ 1,2 s; `t_total_D` ≤ 1,5 s; `t_total_R` ≤ 1,2 s. Es una restricción: una combinación de perillas que pase de 1,0 s tapando el tablero o de 1,2 s en total es **inválida** y se rechaza al cargar la config del cliente.
**Límites trabajados:** con `t_pausa` = 0 la placa no se alcanza a leer (por eso el mínimo es 0,40 s: el nombre sigue en la etiqueta todo el turno y un nombre de ≤ 19 caracteres no se lee entero en 0,6 s a los 15 caracteres por segundo hipotéticos de Localización, de modo que **la placa anuncia y la etiqueta es la referencia**); con todos los máximos (0,25 + 0,70 + 0,25 = 1,20 s) `t_cubre` rompe el 1,0 s: las tres perillas se limitan **entre sí**, no cada una por su cuenta (Tuning Knobs).
**Example:** 0,20 + 0,60 + 0,20 = 1,00 s tapando el tablero (justo en el límite); `t_total` = 1,00 + 0,20 = 1,20 s; Distracción: 1,20 + 0,30 = 1,50 s; «Reducir efectos» con `t_fundido` = 0,10: 0,10 + 0,60 + 0,20 = 0,90 s.

### F2. Motivo de una cara

The `motivo_cara` formula is defined as:

`motivo_bloqueo(j, t) = CastilloSinReclutar si ¬puedeReclutar(j); si no SinLlamadas si llamadas_restantes(j, t) = 0; si no TopeDePiezas si vivas(j, t) + n_t > topeVivas_t; si no SinLugarParaReclutar si libres(j) < n_t; si no Ninguno` · `motivo_cara(j, c) = Ninguno si estado_cara(j, c) = Disponible; CastilloSinReclutar si ¬puedeReclutar(j); SinLlamadas si Agotada por llamadas; si no (SinLugar) motivo_bloqueo(j, t*)`, con `t*` el primer tipo de `c`, en el orden del Catálogo, con `llamadas_restantes(j, t*) > 0` · `motivo_arremetida(j) = YaMueveTodas si piezas_sin_rey(j) ≤ limite_normal y la base de Arremetida es > 0; si no Ninguno`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `puedeReclutar(j)` | bool | — | calculated (#5, F7) | Menos de 2 tramos rotos |
| `llamadas_restantes(j, t)` | int | 0–20 (hoy 0–6) | calculated (#10, F2) | Llamadas sin usar |
| `vivas(j, t)` | int | 0–`topeVivas_t` | calculated (estado) | Piezas vivas del tipo |
| `n_t` | int | 1–17 (hoy 4, 2 o 1) | data file (config, #10) | Piezas por llamada |
| `topeVivas_t` | int | 4–16 | data file (#2) | Tope de vivas del tipo |
| `libres(j)` | int | 0–17 | calculated (#4) | Casillas libres del anillo |
| `estado_cara(j, c)` | enum | Agotada / SinLugar / Disponible | calculated (#10, F3) | Estado de la cara |
| `piezas_sin_rey(j)` | int | 0–60 | calculated (estado) | Piezas vivas sin el Rey ni muros |
| `limite_normal` | int | 1–37 (hoy 12) | data file (#6) | Límite normal de movimientos |

**Output Range:** uno de `Ninguno`, `CastilloSinReclutar`, `SinLlamadas`, `TopeDePiezas`, `SinLugarParaReclutar` o, para Arremetida, `YaMueveTodas`. **Cobertura:** `motivo_bloqueo` sigue el orden de los códigos de `Reclutar` (Reclutamiento, regla 7); `motivo_cara` nunca devuelve `Ninguno` para una cara Agotada o Sin lugar, y el caso `SinLugar` siempre llega a `TopeDePiezas` o `SinLugarParaReclutar` (si `t*` tiene llamadas y el castillo puede reclutar, solo el tope o el anillo pueden bloquearlo). Distracción y Grito no tienen motivo.
**Límites trabajados:** `llamadas_restantes` = 0 en todos los tipos → `SinLlamadas`; castillo cortado **y** sin llamadas → gana `CastilloSinReclutar` (primero en el orden: la causa permanente); `libres(j)` = 0 → `SinLugarParaReclutar`; un tipo con `n_t` = 17 y `libres` = 17 pasa. Una cara con varios tipos bloqueados por causas distintas muestra la causa del primero con llamadas: **el panel resume por cara y el recuadro detalla por tipo** (Open Questions).
**Example:** Cuerno con jinetes sin llamadas, 7 catapultas y 4 demoledores vivos, castillo sano, llamadas de los otros dos (el ejemplo de Reclutamiento F3): la cara está `SinLugar`; `t*` es la catapulta (primer tipo con llamadas); 7 + 2 = 9 > 8 → **`TopeDePiezas`** («Tope de piezas»). Con 13 piezas sin el Rey y la cara Llamada con 2 tramos rotos → **`CastilloSinReclutar`** («Castillo cortado»). Con 12 piezas sin el Rey, Arremetida → **`YaMueveTodas`** («Ya movés todas»).

### F3. Celda de una cara

The `celda_cara` formula is defined as:

`delta_i = efectivo_i − base_i` · `largo_barra_i = ⌊ancho_barra × efectivo_i / 100⌋` · `marca_base_i = ⌊ancho_barra × base_i / 100⌋` · `mostrar_delta_i = (delta_i ≠ 0)` · `icono_i = subió si delta_i > 0, bajó si delta_i < 0` · validez: `Σ efectivo_i = 100` y `Σ delta_i = 0` y `efectivo_Distracción = base_Distracción`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `base_i` | int | 0–100 | data file (tabla base elegida, #11) | Porcentaje de la cara `i` en la tabla base |
| `efectivo_i` | int | 0–100 | calculated (Librería, F2 del Modificador) | Porcentaje de la cara `i` en la tabla efectiva |
| `delta_i` | int | −100 a 100 | calculated | Cambio que hicieron los repartos |
| `ancho_barra` | int (px) | 160 | data file (cliente) | Largo de una barra al 100% |
| `i` | id | 1–5 | data file (config) | Cara, en el orden fijo de la tabla |

**Output Range:** `largo_barra` de 0 a `ancho_barra`; porcentajes enteros, sin decimales. **Si la validez no se cumple** (la Librería garantiza que no pasa, Modificador, F2), el panel muestra `ui.modif_panel_error` en lugar de la tabla, **registra el caso y no renormaliza ni corrige**: un panel que mostrara números arreglados dejaría de ser una consulta de lo que realmente se usa.
**Límites trabajados:** `efectivo` = 0 → barra de largo 0 con trama y, si corresponde, su chip; `efectivo` = 100 (no ocurre con las cifras vigentes, el máximo es 85, pero la config lo permite) → barra completa; `base` = 0 → sin chip, sin diferencia si el efectivo también es 0; con `base` = 0 y `efectivo` > 0 la cara recibió un reparto sin haber sido posible antes: `delta` > 0 y se muestra «base 0, ahora N».
**Example:** Pierde 1/2, 13 piezas sin el Rey, 2 muros rotos (Modificador, F2): base 15/20/25/25/15 → efectiva 15/45/0/0/40. `delta` = 0/+25/−25/−25/+25 (Σ = 0); barras de 24/72/0/0/64 px con `ancho_barra` = 160; Distracción sin diferencia ni ícono; Llamada y Cuerno con chip «Castillo cortado»; Grito y Arremetida con «base 20, ahora 45» y «base 15, ahora 40» y el ícono de subió.

### F4. Casillas de aparición marcadas

The `casillas_marcadas` formula is defined as:

`K = máx de n_t sobre los tipos t de la cara con tipo_disponible(j, t)` · las casillas marcadas son las primeras `K` libres del anillo en `orden_aparicion` (Reclutamiento, F4), numeradas de 1 a `K` · la tarjeta del tipo `t` disponible muestra `ui.reclutar_casillas` con `n = n_t`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `n_t` | int | 1–17 (hoy 4, 2 o 1) | data file (config, #10) | Piezas por llamada |
| `tipo_disponible(j, t)` | bool | — | calculated (#10, F1) | Opción elegible |
| `K` | int | 1–17 | calculated | Casillas a marcar; 0 solo si el recuadro no se abre |

**Output Range:** `K` entre 1 y 17 con el recuadro abierto; `K` nunca supera las casillas libres, porque un tipo disponible exige `libres(j) ≥ n_t` (F1 de Reclutamiento). Una tarjeta **bloqueada no muestra la línea de casillas**: muestra su motivo en ese lugar (no promete casillas que no hay).
**Límites trabajados:** ningún tipo disponible → `K` = 0: no hay recuadro (`ReclutaPerdido`, inalcanzable); un solo tipo disponible de 1 pieza → `K` = 1 y una sola casilla numerada.
**Example:** Cuerno con el jinete bloqueado por tope: disponibles catapulta (2) y demoledor (1) → `K` = 2; se marcan las casillas 1 y 2 del orden; la catapulta dice «Casillas 1 a 2» y el demoledor «Casilla 1»; el jinete muestra «Tope de piezas». Llamada con todo libre → `K` = 4 y las tres tarjetas dicen «Casillas 1 a 4».

### F5. Geometría y capacidad de texto

The `geometria_ui` formula is defined as:

`col_ini_panel = (cols_tablero − panel_cols) / 2` y exige `col_ini_panel ≥ anillo_cols` · `alto_panel = alto_titulo + alto_cabecera + 5 × alto_fila + alto_pie + alto_nota` y exige `alto_panel ≤ panel_filas_max × casilla_px` · `ancho_recuadro = 3 × ancho_tarjeta + 2 × sep_tarjeta + 2 × margen_recuadro` y exige `(ancho_tablero − ancho_recuadro) / 2 ≥ anillo_cols × casilla_px` · capacidad de texto: `cap_linea = ⌊ancho_texto / ancho_car⌋` y exige `cap_linea × lineas ≥ ⌈presupuesto × (100 + expansion_pct) / 100⌉` para cada clase de texto

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| `cols_tablero` | int | 24 | constant (#4) | Columnas del tablero |
| `casilla_px` | int | 68 | constant (art bible §7, a 1080p) | Lado de una casilla |
| `ancho_tablero` | int (px) | 1632 | calculated (`cols_tablero × casilla_px`) | Ancho del tablero |
| `anillo_cols` | int | 6 | constant (#4, #10 F4) | Columnas del anillo de cada castillo (0 a 5 y 18 a 23) |
| `panel_cols` | int | 8–12 | data file (cliente) | Ancho del panel en columnas (hoy 12 = 816 px) |
| `panel_filas_max` | int | 9 | data file (cliente) | Alto máximo del panel en filas (612 px) |
| `alto_titulo`, `alto_cabecera`, `alto_fila`, `alto_pie`, `alto_nota` | int (px) | 56, 72, 72, 48, 60 | data file (cliente) | Bandas del panel |
| `ancho_tarjeta`, `sep_tarjeta`, `margen_recuadro` | int (px) | 204, 12, 16 | data file (cliente) | Medidas del recuadro de reclutar |
| `ancho_car` | int (px) | 8 | data file (cliente), **NOT SOURCEABLE** | Ancho medio de un carácter a 16 px: lo fija la fuente elegida (art bible, Tipografía) y se mide |
| `ancho_texto`, `lineas` | int | por clase | calculated | Ancho útil y líneas disponibles (tabla abajo) |
| `presupuesto` | int | por clase | data file (Localización F4 y clases nuevas) | Largo máximo de la clave |
| `expansion_pct` | int | 40 | data file (Localización, F5) | Inflado de prueba |

**Output Range:** todas las condiciones se cumplen con las medidas vigentes. **Si una falla, el diseño no cierra** y se cambia una medida, no el texto en silencio.

| Chequeo | Cuenta | Resultado |
|---|---|---|
| Panel, horizontal | `col_ini_panel` = (24 − 12) / 2 = 6 ≥ 6 | Cumple, justo: deja libres los anillos (columnas 0 a 5 y 18 a 23) |
| Panel, vertical | 56 + 72 + 5 × 72 + 48 + 60 = **596** ≤ 9 × 68 = 612 | Cumple con 16 px de margen |
| Recuadro, horizontal | 3 × 204 + 2 × 12 + 2 × 16 = **668**; (1632 − 668) / 2 = 482 ≥ 6 × 68 = 408 | Cumple con 74 px de margen |
| Recuadro, vertical | 72 + 28 + 204 + 2 × 16 = **336** (4,9 filas) | Cabe en el centro del tablero |
| Línea de efecto | `ancho_texto` = 336 − 24 − 8 − 16 = 288 → 36 por línea × 2 = 72 ≥ ⌈48 × 1,4⌉ = 68 | Cumple |
| Chip de motivo | `ancho_texto` = 240 − 32 = 208 → 26 × 1 línea ≥ ⌈18 × 1,4⌉ = 26 | Cumple, justo |
| Nota fija | `ancho_texto` = 816 − 32 = 784 → 98 por línea × 3 = 294 ≥ ⌈180 × 1,4⌉ = 252 | Cumple |
| Motivo en tarjeta (`codigo.*`, aviso 60) | 204 − 24 = 180 → 22 por línea × 4 = 88 ≥ ⌈60 × 1,4⌉ = 84 | Cumple |

**Límites trabajados:** con `ancho_car` = 9 px **fallan** el chip (23 < 26) y la línea de efecto (32 × 2 = 64 < 68) bajo el texto inflado: por eso el 8 es un supuesto a medir con la fuente real (Open Questions) y no una constante. Con `panel_cols` = 13 el panel toca las columnas 5 y 18 (anillos) y la condición falla.
**Example:** el panel a 1080p ocupa las columnas 6 a 17 y 596 px de alto (8,8 filas), así que si abre sobre el recuadro de reclutar, los anillos y las casillas numeradas siguen a la vista.

### Aporte a la duración (pilar 4)

Esta interfaz **no suma tiempo de turno por diseño**: la placa no bloquea y el recuadro recibe toques desde el primer cuadro (F1, regla 6). El tiempo que el jugador elige gastar mirando el panel no se descuenta de nada, pero sí del reloj que corre, así que lo mide `ms_abierto` del evento `PanelAbierto` (regla 17) para que Métricas (#19) lo cruce con la duración del turno. El tiempo de elegir un tipo es de Reclutamiento (F6, Δturnos).

### Lazos de retroalimentación

**Ninguno en el estado:** la interfaz es de solo lectura y no entra a la huella. Hay un lazo de **percepción**, no de reglas: ver el Grito en 45% puede hacer que el jugador que va ganando sienta que el dado favorece al que pierde. Lo amortigua la nota fija, el nombre neutro de la tabla y la columna del rival a la vista (Open Questions).

## Edge Cases

### Placa, etiqueta y tirada

- **If es el primer turno de la partida:** no hay placa ni tirada; la etiqueta dice «Apertura: sin dado» y el «?» funciona: muestra la tabla de la próxima tirada con el tablero actual (con 12 contra 12 piezas, la normal 20/40/20/20/0 de la apertura, Modificador F2).
- **If llega una `Tirada` rechazada** (`TiradaInvalida`, contador viejo o `TiradaNoEsperada`): no se dibuja placa ni cambia la etiqueta, y no se muestra aviso: es un comando de autoridad, no del jugador (Entrada, regla 17), y el estado no cambió.
- **If el servidor tarda en mandar la tirada (online):** la etiqueta sigue en «Tirando el dado…» sin cara ni predicción y sin reloj corriendo (Máquina, regla 14). Cuánto se espera y qué pasa si nunca llega es de Servidor (#28); esta interfaz no inventa una cara ni cierra la etiqueta sola.
- **If se reconecta con una tirada ya aplicada** (mismo `contadorTirada`): la etiqueta aparece directo con su cara y **no se repite la placa**; si la fase es `Reclutar` y el jugador local debe elegir, el recuadro se vuelve a abrir (se deriva de la fase) con su encabezado ya asentado.
- **If el turno se cierra mientras la placa está en pantalla** (Distracción sin acción posible: la Máquina salta `Atacar` y cierra, Máquina, regla 10): la placa se cierra con un fundido de 0,10 s y la etiqueta nunca llega a verse. El jugador no perdió ninguna decisión; el evento `Tirada` queda en las métricas (si eso se siente como «me comí un turno» es una pregunta abierta del Modificador).
- **If cambia el estado mientras una animación corre:** salta a su fin en el cuadro siguiente (regla 16). La excepción es la de arriba: una placa nunca se corta de golpe, siempre se cierra con el fundido.
- **If sale Distracción:** placa gris de la misma duración sobre el tablero que las otras (F1); la franja superior suma 0,30 s de estandarte que cae. No se muestra ninguna fase de mover: la etiqueta ya dice la cara y el botón de avanzar lo define el HUD.
- **If «Reducir efectos» está activo:** la placa aparece por fundido de 0,10 s (F1) y la etiqueta no pulsa; el contenido y el orden no cambian.

### Recuadro de reclutar

- **If el jugador toca una tarjeta bloqueada:** Entrada manda el comando y muestra el rechazo con el texto de su código; el recuadro **sigue abierto** y el reloj sigue corriendo. Un segundo mensaje no se agrega.
- **If el timeout de mover llega con el recuadro abierto:** la Máquina recluta sola, el recuadro se cierra en 0,20 s y la pieza aparece sin marca; el aviso «Elegido por el tiempo» sale como texto en la zona de avisos (#15, regla 18; decisión del 2026-11-26). Si un toque a una tarjeta llega **después** de ese timeout, la Máquina lo rechaza (`FaseIncorrecta`) y Entrada muestra el aviso neutro: el jugador ve que ya se eligió por él.
- **If el estado de la mesa cambia con el recuadro abierto:** no pasa, porque en `Reclutar` no se mueve nada ni se rompe un muro (Reclutamiento, regla 6); por eso una tarjeta disponible no se vuelve bloqueada entre que se muestra y se toca.
- **If una tarjeta de 1 pieza y una de 4 se disponen a la vez** y solo hay 2 casillas libres: la de 4 está bloqueada («Sin lugar»), solo se marcan las casillas 1 y 2 y la tarjeta de 1 pieza dice «Casilla 1» (F4).
- **If ningún tipo está disponible** (inalcanzable): no se abre el recuadro y se muestra «Sin reclutas disponibles» (≤ 1 s) en la placa central de leyendas (#15, regla 19); la placa corre como la de cualquier cara.
- **If el que recluta es el Rival IA (#25) o el rival online:** no se muestra el recuadro (regla 6); se ven la placa, la etiqueta y, después, las piezas que aparecen.
- **If el jugador toca dos veces seguidas la misma tarjeta:** el primer toque cambia la fase y cierra el recuadro; el segundo cae sobre el tablero y se resuelve contra el estado nuevo (Entrada, regla 15): como la selección se soltó al cambiar de fase, no puede mandar un comando.
- **If cambia el idioma con el recuadro abierto:** los textos se piden de nuevo en el cuadro siguiente, sin reiniciar la caída ni cerrar el recuadro (Localización, regla 11); si un texto más largo no cabe, se aplica el presupuesto de F5 y no se corta.

### Panel «?»

- **If el panel se abre con el recuadro de reclutar abierto:** el panel va encima, el recuadro no recibe toques y el reloj sigue; al cerrar, el recuadro vuelve como estaba. Si el timeout recluta por el jugador con el panel abierto, el panel muestra el resultado (llamadas y piezas nuevas) y el aviso espera al cierre.
- **If se aplica una `Tirada` con el panel abierto** (un timeout cambió el turno, o el rival online tiró): el panel se cierra solo y la placa corre sin tapa; el jugador puede volver a abrirlo.
- **If la partida termina con el panel abierto:** se cierra y el «?» desaparece.
- **If el jugador cierra el panel con dos toques seguidos sobre el velo:** el primero cierra; para que el segundo no llegue a una casilla con una pieza elegida y mande un `Mover` o un `AsignarAtaque`, **el tablero ignora los toques durante `guarda_cierre_panel_ms`** (250 ms) después de cerrarse el panel. Los botones del HUD no tienen guarda.
- **If los porcentajes efectivos no suman 100, o Distracción cambió, o `Σ delta ≠ 0`:** el panel muestra `ui.modif_panel_error` en lugar de la tabla, registra el caso y **no renormaliza** (F3). La Librería lo hace inalcanzable (Modificador, F2); el panel no confía en eso.
- **If una cara tiene base 0 y efectivo 0:** muestra `0%` sin chip ni diferencia; el chip solo aparece si algo la bloqueó.
- **If dos jugadores tienen la misma tabla** (por ejemplo la apertura): las dos columnas se muestran completas e iguales; no se colapsan en una.
- **If el jugador con 12 piezas sin el Rey o menos tiene los dos reclutas bloqueados** (15/85/0/0/0): Llamada y Cuerno muestran su motivo, Arremetida muestra «Ya movés todas» y el Grito 85% con «base 20, ahora 85». Es el caso que más pregunta provoca y por eso la nota fija lo explica.
- **If la tabla base que devuelve el núcleo no tiene clave de nombre** (la config sumó una tabla): Localización aplica su respaldo (Localización, regla 14): en desarrollo se ve `⟦clave⟧` y en un build publicado, vacío; los tests de claves faltantes impiden publicarlo (AC-23).
- **If el idioma está `Degradada`** (una sola tabla cargada): el panel y el recuadro se muestran en ese idioma y el botón ES/EN está deshabilitado; nada de esta interfaz cambia.
- **If la partida es online y el turno es del rival:** el «?» sigue disponible; «Vos» es el jugador local y la columna «Rival» muestra al que tiene el turno.

### Tiempo, accesibilidad y datos

- **If el jugador deja el panel abierto y no hace nada:** nada lo cierra y nada pausa el reloj (Timer, regla 10). Al vencer la etapa, el estado avanza por el timeout; el panel muestra el cambio y el aviso espera. Es una consecuencia aceptada de la decisión del jugador y la mide `ms_abierto`.
- **If un lector de pantalla está activo:** la placa se anuncia **una vez** («Tirada: {cara}»); el panel se anuncia como diálogo y cada fila se lee con cara, porcentaje y diferencia; los motivos se leen como parte de la fila.
- **If la config cambia los bonos del Grito:** la línea de efecto se actualiza sola (parámetros `ataque` y `movimiento`).
- **If la config cambia los tipos o la cantidad de una cara de reclutar:** `n` se actualiza solo en `cara.llamada_efecto`, pero **las listas de tipos de `cara.llamada_efecto` y `cara.cuerno_efecto` están escritas en el texto**: hay que reescribirlas a mano (Open Questions).
- **If una estrategia degenerada se hace visible:** cortar el propio castillo o quedar con 12 piezas o menos para subir el Grito es una estrategia que el panel hace más descubrible (el jugador ve «Castillo cortado» y el Grito en alto). Es la pregunta abierta del Modificador y de Reclutamiento, y se mide con las métricas de #19; esta interfaz no la oculta.

## Dependencies

**Depende de:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #11 Modificador | Dura | Las cinco caras y su orden, las cinco tablas base, la tabla efectiva y sus repartos, los bonos del Grito (para la línea de efecto), regla 10 (panel) |
| #1 Librería de reglas | Dura | `ProbabilidadesSiguienteTirada` (con el **id de la tabla base**, pedido), evento `Tirada`, `contadorTirada`, config |
| #10 Reclutamiento y límites | Dura | `estado_cara`, `tipo_disponible`, `llamadas_restantes`, `orden_aparicion`, `motivo_bloqueo` (pedido), códigos de `Reclutar`, eventos `Reclutado` y `ReclutaPerdido` |
| #13 Entrada y selección | Dura | Manda `Reclutar(tipo)` y muestra los rechazos y avisos; acepta el estado «modal abierto» y los blancos táctiles de esta interfaz |
| #14 Localización | Dura | Todo texto por clave; aviso de cambio de idioma; presupuestos de largo y texto inflado (F4 y F5) |
| #3 Máquina de turno | Dura | Fase, `FaseIniciada`, `ReclutaPerdido`, jugador del turno, reloj que no corre en `EsperandoTirada` |
| #12 Timer de turno | Blanda | Que el panel no es un motivo de pausa y la placa no cobra reloj (no lee nada del Timer) |
| #4 Tablero y despliegue inicial | Blanda | Anillo, columnas del frente y casillas libres (dimensiones de F5) |
| #5 Castillo y muros | Blanda | `puedeReclutar`, vía `motivo_bloqueo` |
| #2 Catálogo de piezas | Blanda | Orden de los tipos, `topeVivas`, nombres de pieza |
| `design/art/art-bible.md` | Documento | Secciones 2 y 7, gramática de formas de la 3, daltonismo de la 4 |

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #18 Render, animación y feedback | Dura | La lista de casillas a numerar y cuándo; resplandor y ambiente de la placa (existe desde la tanda 4) |
| #15 Ficha de pieza y HUD | Blanda | Lugar de la etiqueta y del «?» en la franja superior |
| #19 Métricas locales | Blanda | El evento `PanelAbierto(...)` |
| #22 Audio y SFX | Blanda | Una señal por cara y la apertura del panel |
| #24 Tutorial básico | Blanda | Identificadores estables de la regla 18 |
| #23 Accesibilidad, #41 Adaptación móvil | Blanda | Nombres accesibles, «Reducir efectos», escala del panel en pantalla estrecha |

**Bidireccionalidad.** Ya nombran a la Interfaz del Modificador (#17): el Modificador (sus reglas 10 y su tabla de dependencias), Reclutamiento (Player Fantasy, UI Requirements y dependencias), Entrada (Interactions) y Localización (Dependencies, Interactions y Open Questions); Castillo (UI Requirements) la nombra como «#17». **No la nombran todavía** y hay que sumarla: la Librería (la consulta con el id de la tabla base), la Máquina (etiqueta y placa en `EsperandoTirada`), el Timer (la no-pausa del panel), el Tablero (anillo en F5) y el Catálogo (orden de tipos). **Provisionales:** HUD (#15), Render (#18), Métricas (#19), Audio (#22) y Tutorial (#24) no tienen GDD; sus aristas se confirman al escribirlos. Al cerrar la tanda 4 (2026-11-26) la nombran también Vista previa (#16), Render (#18) y Métricas (#19).

## Tuning Knobs

Todas son datos del cliente: no entran al estado ni a la huella. Se derivan de las variables `data file` de la sección Formulas. Las cifras marcadas «propuesta» no tienen fuente y se ajustan con las primeras partidas.

| Perilla | Vigente | Rango seguro | Si es demasiado alta | Si es demasiado baja | Interactúa con |
|---|---|---|---|---|---|
| `t_caida` | 0,20 s | 0,10–0,25 | La placa tapa el tablero más de 1 s en total | La caída no se percibe y el golpe seco desaparece | `t_pausa`, `t_acople`: su suma ≤ 1,0 s (F1) |
| `t_pausa` | 0,60 s | 0,40–0,70 | Rompe `t_cubre_max` | La placa no se alcanza a leer; queda solo la etiqueta | `t_caida`, `t_acople` |
| `t_acople` | 0,20 s | 0,10–0,25 | Rompe `t_cubre_max` | La placa «desaparece» sin enseñar dónde queda la etiqueta | `t_caida`, `t_pausa` |
| `t_pulso` | 0,20 s | 0–0,25 | Se pasa de 1,2 s en total | Sin confirmación de que la etiqueta tomó la cara | `t_total_max` |
| `t_extra_distraccion` | 0,30 s | 0–0,30 | Pasa de 1,5 s: Distracción pesa más que las otras caras (art bible §2) | La cara más fastidiosa casi no se nota | `t_total_D` |
| `t_fundido` (Reducir efectos) | 0,10 s | 0,05–0,15 | La placa «tarda» en aparecer | Corte brusco: lo que «Reducir efectos» quería evitar | F1 |
| `t_panel_transicion` | 0,25 s | 0,10–0,25 | Rompe la regla de la interfaz de ≤ 0,25 s | — | Game Feel |
| `t_recuadro_cierre` | 0,20 s | 0,10–0,25 | El recuadro «se queda» tapando casillas | Parpadeo | — |
| `t_tarjetas_fundido` | 0,10 s | 0–0,15 | Las tarjetas «no están» cuando el jugador quiere tocar | Aparición brusca | Regla 6: tocables desde el primer cuadro |
| `scrim_opacidad` | 40% (propuesta) | 25–60% | El tablero deja de verse y el modal se siente como cambio de pantalla | El panel no se separa del tablero | Contraste del texto del panel |
| `guarda_cierre_panel_ms` | 250 (propuesta, igual a `ventana_antidoble_ms` de Entrada) | 0–500 | El primer toque legítimo tras cerrar se pierde | Un doble toque sobre el velo manda un comando con una pieza elegida | `ventana_antidoble_ms` (Entrada) |
| `panel_cols` | 12 | 8–12 | Tapa los anillos (columnas 5 y 18) | El texto no cabe (F5) | `ancho_car`, `presupuesto` |
| `panel_filas_max` | 9 | 7–9 | Tapa los castillos | El panel no entra | `alto_fila` |
| `ancho_tarjeta` | 204 px (3 columnas) | 136–204 | El recuadro tapa los anillos | El motivo de una tarjeta bloqueada no cabe | `presupuesto` de `codigo.*` |
| `ancho_barra` | 160 px | 120–200 | Las barras desbordan la celda | Diferencias de 5 puntos no se distinguen | Celda de 240 px |
| `ancho_car` | 8 px (supuesto, a medir) | 7–9 | Fallan el chip y la línea de efecto con el texto inflado (F5) | — | `presupuesto`, `expansion_pct` (Localización) |
| `presupuesto` línea de efecto / chip / nota | 48 / 18 / 180 | 40–60 / 14–22 / 140–240 | Texto que no cabe en 2 líneas, en 1 línea o en 3 | Textos forzados a abreviaturas | Localización F4 y F5; `ancho_car` |

**Perillas de otros GDD que afectan a esta interfaz** (se señalan, no se duplican): las cinco tablas base y los bonos del Grito (Modificador, #11); `limite_normal` (Movimiento, #6: decide cuándo aparece «Ya movés todas»); los topes de llamadas y de piezas vivas y las piezas por llamada (Reclutamiento, #10 y Catálogo, #2); `aviso_duracion_ms` y `ventana_antidoble_ms` (Entrada, #13); `expansion_pct` y los presupuestos de largo (Localización, #14); los modos y duraciones del timer (#12).

**No son perillas** (constantes del contrato): el orden fijo de las cinco caras; que el panel sea modal y **no pause** el reloj; que el toque sobre el velo solo cierre; que la placa no bloquee y que las tarjetas reciban toques desde el primer cuadro; `t_cubre_max` (1,0 s) y `t_total_max` (1,2 s); que el panel muestre la tabla efectiva y no la base; que ninguna información dependa del hover. Cambiarlas cambia el diseño, no una cifra.

**Interacción entre perillas:** `t_caida + t_pausa + t_acople ≤ t_cubre_max` las limita entre sí. `panel_cols`, `ancho_tarjeta`, `ancho_car` y los presupuestos de texto se limitan por F5: subir uno obliga a revisar los demás.

## Visual/Audio Requirements

*Lente del director de arte, con `design/art/art-bible.md`: secciones 2 (estados y reglas de ritmo) y 7 (HUD, íconos, animación, accesibilidad), la gramática de formas de la interfaz de la sección 3 y los colores semánticos y la tabla de daltonismo de la sección 4. **No se leyeron enteras** la sección 1 ni el resto de la 3 y la 4.*

**1. Feedback visual por evento**

| Evento | Qué se ve | Fuente |
|---|---|---|
| `Tirada` con Distracción, Grito o Arremetida | **Placa** al centro con el ícono y el nombre de la cara (34–40 px): cae en 0,20 s con el golpe seco, queda 0,60 s y se acopla a la etiqueta en 0,20 s; ≤ 1,0 s tapando el tablero | Art bible §2 (*Tirada del Modificador*) y §7; F1 |
| `Tirada` con Distracción | La misma placa en gris, terreno desaturado un 40% (ambiente propio del art bible §2) y el estandarte del HUD que se dobla y cae; 1,5 s en total | Art bible §2 (*Distracción*) |
| `Tirada` con Llamada o Cuerno | La placa es el **encabezado del recuadro**, que queda abierto con las tarjetas; el resplandor tenue del lado del castillo propio que mira al rival lo pone Render | Art bible §2 (*Planificar*) y §7 |
| Etiqueta de la cara | Franja superior: emblema del bando, ícono y nombre, durante todo el turno; pulso de 0,2 s al recibir la placa | Art bible §7 |
| Tarjeta de reclutar | Ícono del tipo, `{n} ×` y nombre, línea de casillas, fichas con ícono (vivas/tope y llamadas) | Art bible §7 (*Íconos*) |
| Tarjeta bloqueada | Candado y trama sobre la tarjeta atenuada, y el texto del motivo debajo | Art bible §4 (*Nada solo por color*) |
| Casillas de aparición | Marcas numeradas (≥ 14 px, negrita, borde Tinta) sobre las `K` casillas; las dibuja Render | Art bible §7 (números de la ficha) |
| Abrir el «?» | Velo del 40% y panel central (≤ 0,25 s); tabla de 5 filas con barras, porcentajes, diferencias, chips y el pie de llamadas | Art bible §7 |
| Cara que sube o baja | Ícono de **subió** o **bajó** y «base X, ahora Y», sin usar verde ni rojo | Art bible §4 (colores semánticos) |
| `ui.modif_panel_error` | Texto de aviso en lugar de la tabla, con ícono | Art bible §7 |

Si `TramoFinalIniciado` coincide con la placa de la cara o con el banner del turno, `Tramo final` es un renglón de esa placa y no hay banner propio (#15, regla 12; decisión del 2026-11-26).

**2. Restricciones de estilo y animación.**
- **Capa del sistema.** El panel, el recuadro, la etiqueta y las tarjetas son capa del sistema: lenguaje geométrico y plano, esquinas redondeadas como la ficha. El **marco** del panel y del recuadro puede hacer el eco leve de la piedra de los muros (art bible §3, *Menús*); las celdas, tarjetas y textos van planos.
- **Los colores semánticos no se reutilizan.** El verde de la vida, el bermellón del ataque, el acero del escudo y el ámbar del timer significan algo en la ficha y en el reloj: **no se usan en barras, chips, íconos de diferencia ni candados**, o el panel parecería hablar de estadísticas. Las barras y chips usan los neutros de la interfaz (Tinta, Hueso, grises de la paleta de interfaz, art bible §4) y la forma lleva el significado.
- **Ninguna animación suma tiempo al turno** (F1) y el único golpe de toda la interfaz es el de la placa al asentarse. La placa no destella más de 3 veces por segundo y no mueve la cámara ni el tablero. «Reducir efectos» la reemplaza por un fundido (regla 4).
- **Orden de lectura** (art bible §1, regla visual): en `Atacar` la vista previa y MUERE/ROTO ganan a todo; esta interfaz no tiene nada abierto en esa fase salvo el «?», que es una consulta del jugador.

**3. Principios del art bible que aplican.** *Lo importante, lo más visible* (el nombre de la cara y el porcentaje van primero; el motivo, segundo). *Nada solo por color* (cada cara por silueta; subió y bajó por forma; bloqueada por candado y trama; tarjetas y casillas por número). *Animación de la interfaz ≤ 0,25 s* (todas las transiciones; la placa suma su pausa de 0,6 s, que no es una transición). *El ritmo del turno manda sobre la animación* (la placa y el recuadro no bloquean). *Controles con un toque, sin hover.*

**4. Íconos nuevos** (planos, de un solo color, trazo grueso, grilla de 24 px, art bible §7). Las siluetas son **propuesta** de esta sesión y se distinguen en grises; las valida el director de arte:

| Ícono | Forma propuesta | Se distingue de los demás por |
|---|---|---|
| Distracción | Estandarte doblado hacia abajo | Silueta vertical que cuelga |
| Grito de guerra | Banderín recto con tres rayas de sonido | Rayas horizontales delante |
| Llamada al servicio | Campana de mano | Cúpula redondeada |
| Cuerno de guerra | Cuerno curvo | Cono curvo |
| Arremetida | Tres chevrones hacia adelante | Ángulos repetidos |
| Subió / bajó | Triángulo hacia arriba / hacia abajo | Orientación |
| Bloqueada | Candado | Arco sobre un cuerpo cuadrado |
| Esperando | Dado | Cuadrado con puntos |
| Elegido por el tiempo (solo dentro del aviso de la zona de avisos, nunca sobre la pieza ni la ficha) | Reloj | Círculo con agujas |
| Piezas vivas | Grupo de fichas | Tres rectángulos juntos |
| Cerrar | X | Dos diagonales |

El «?», los siete íconos de tipo, el puño, el escudo y los emblemas de los dos bandos **ya están** en el art bible (§7, *Íconos*). **Falta en el art bible** y se pide al director de arte: los cinco íconos de cara con su forma, los demás de la tabla, el marco del panel y del recuadro, la placa de cada cara (Distracción gris) y el estilo de la barra y de la marca de base.

**Audio (para #22, propuesta sin validar con el director de audio).** Una señal por cara que se distinga sin mirar la pantalla, con el golpe seco de la placa como base común y ninguna de más de 1,2 s: Distracción, tono caído y breve; Grito, golpe agudo y corto; Llamada, campana; Cuerno, nota de cuerno; Arremetida, redoble ascendente. Abrir y cerrar el panel: un clic suave, el mismo para los dos. Tocar una tarjeta bloqueada: ningún sonido propio (el rechazo suena neutro, Entrada). Casillas numeradas y etiqueta: sin sonido. Sin voces (Localización).

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:interfaz-del-modificador` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

El jugador no opera el dado, pero sí opera el panel y el recuadro: la sensación que se diseña es **respuesta inmediata en lo que toca** y **peso breve en lo que no**.

| Evento | Qué debe sentirse | Objetivo medible |
|---|---|---|
| Placa de la cara | Un golpe seco y claro, sin angustia: el azar ya pasó | `t_cubre` ≤ 1,0 s y `t_total` ≤ 1,2 s (F1); el primer comando válido se acepta en el cuadro en que abre la fase |
| Distracción | Fastidio corto que se cierra rápido | ≤ 1,5 s en total; la placa sobre el tablero dura como las otras |
| Etiqueta | «Esto es lo que me tocó», siempre ahí | Visible desde el acople hasta el cierre del turno, sin parpadeos |
| Recuadro de reclutar | Tomar una decisión, no esperar un menú | Tarjetas tocables en el **mismo cuadro** en que abre `Reclutar`; elegir cuesta un toque; se cierra en ≤ 0,25 s |
| Tarjeta bloqueada | Se explica, no se reprende | El motivo está visible sin tocar; el toque no suma un segundo mensaje |
| Abrir el «?» | Una consulta inmediata, sin menú | ≤ 0,25 s, con todo el contenido en el primer cuadro |
| Cerrar el «?» | Volver al tablero sin riesgo | El toque sobre el velo no llega al tablero; `guarda_cierre_panel_ms` de 250 ms |
| Reloj con el panel abierto | Honestidad: el tiempo sigue | Timer y relojes visibles y sin pausa; ningún mensaje que prometa lo contrario |
| Tirada online | La cara aparece cuando la decide el servidor | Sin predicción de la cara en el cliente |

**Lo que no se puede automatizar y va a playtest:** que el panel **se use** y **se entienda** (¿lo abren?, ¿cuántas veces por partida?, ¿cuánto tiempo?, ¿saben decir por qué el Grito está alto?), que el Grito en 85% no se perciba como una ventaja injusta del que va ganando, que el nombre neutro de la tabla no termine funcionando como la etiqueta que se quiso evitar, y que perder la cara en un turno de Distracción sin acciones (la placa se cierra sola) no se sienta como un turno comido.

## UI Requirements

Esta interfaz **es** la UI del dado: no hay una pantalla propia, sino cuatro elementos sobre el tablero y la franja superior. Inventario (los identificadores son los de la regla 18):

| Elemento | Datos que muestra | Estados | Blanco táctil | Accesibilidad |
|---|---|---|---|---|
| **Placa** (`modif_placa`) | Ícono y nombre de la cara | `Placa`, o encabezado del recuadro | Ninguno (los toques la atraviesan) | Se anuncia una vez («Tirada: {cara}») |
| **Etiqueta** (`modif_etiqueta`) | Emblema, ícono y nombre de la cara; o «Apertura…» / «Tirando…» | `Apertura`, `Esperando`, `Etiqueta`, `Limpia` | Ninguno | Texto leíble; nombre por clave |
| **Recuadro de reclutar** (`modif_recuadro_reclutar`) | Hasta 3 tarjetas: tipo, `{n} ×`, casillas, vivas/tope, llamadas, motivo si está bloqueada | Abierto o cerrado; cada tarjeta, disponible o bloqueada | Cada tarjeta: 204 × 204 px a 1080p | Cada tarjeta se lee como botón con tipo, cantidad y estado; la bloqueada dice su motivo |
| **Botón «?»** (`modif_boton_ayuda`) | Ícono de signo de pregunta | Visible u oculto (`Terminada`) | 60 × 60 px como mínimo | Nombre accesible `ui.modif_abrir_panel` |
| **Panel** (`modif_panel`) | Tabla efectiva de los dos jugadores con motivos y llamadas | `Cerrado`, `Abierto` | Cerrar: la X, el velo y el «?» | Diálogo con tabla; devuelve el foco al «?» |
| **Aviso** (zona de avisos del HUD, #15 regla 18) | Los rechazos de Entrada y el aviso «Elegido por el tiempo» (texto en la zona de avisos; sin ícono ni marca sobre la pieza ni la ficha, #15 regla 18; decisión del 2026-11-26). «Sin reclutas disponibles» va en la placa central (#15, regla 19) | Uno a la vez | Ninguno | Lo anuncia el lector de pantalla |

- **Disposición.** Todo lo que está sobre el tablero respeta los anillos de reclutamiento y los castillos (F5). El «?» y la etiqueta viven en la franja superior de 60 px (art bible §7); la posición exacta de la etiqueta la confirma el HUD (#15).
- **Datos que otros sistemas deben poder leer:** la cara del turno, la tabla efectiva y el id de la tabla base de cada jugador, el motivo de cada cara y las llamadas restantes (Interactions).
- **Texto:** sale de Localización; admite el inflado de prueba de F5; escala con el tamaño de texto de #23.
- **Móvil (#41):** el panel se dimensiona en pantalla (no con el tablero); su paso a dos pestañas en pantalla estrecha lo decide #41 (provisional).

> **📌 UX Flag — Interfaz del Modificador**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly. Specs previstos: **panel de probabilidades**, **recuadro de reclutar** y **placa y etiqueta de la cara**.

## Cross-References

| Documento | Elemento referenciado | Qué toma este GDD |
|---|---|---|
| `modificador.md` | Reglas 1, 3, 4, 5, 6, 9, 10 y 12; F1 y F2; UI Requirements; Open Questions | Las cinco caras y su orden, las cinco tablas base, la tabla efectiva y sus repartos, el panel «?» (regla 10), los motivos de cada 0% y la pregunta abierta de si se entienden los repartos |
| `libreria-de-reglas.md` | Regla 6 (`ProbabilidadesSiguienteTirada`), evento `Tirada` (AC-8), regla 3 (comandos) | La consulta pura de las probabilidades, el evento con la tabla base y `contadorTirada` |
| `reclutamiento-y-limites.md` | Reglas 1, 4, 5, 7 y 10; F1 a F5; Player Fantasy y UI Requirements | Tabla de reclutas, disponibilidad, estado de cada cara, orden de los códigos de rechazo, orden de aparición, recluta automático, límites públicos |
| `entrada-y-seleccion.md` | Reglas 2, 4, 6, 12, 14, 15 y 16; Interactions (#17); perillas | Quién manda `Reclutar(tipo)`, la franja de avisos, el reparto de toques, la ventana antidoble |
| `localizacion.md` | Reglas 1, 4, 5, 7, 12 y 14; F1, F4 y F5; Open Questions | Claves por grupo, parámetros y plurales, presupuestos de largo, texto inflado y la pregunta abierta de los textos de las cinco caras |
| `maquina-de-turno.md` | Reglas 3, 4, 10, 11 y 14; fase `EsperandoTirada`; `ReclutaPerdido`; UI Requirements | Primer turno sin tirada, que el reloj no corre esperando la tirada, borrado de la cara al cerrar, leyendas pendientes |
| `timer-de-turno.md` | Regla 10 (pausa, lista cerrada de motivos) | Que el panel no es un motivo de pausa |
| `castillo-y-muros.md` | F7 (`puedeReclutar`); UI Requirements (panel «?») | La causa «Castillo cortado» |
| `catalogo-de-piezas.md` | Orden de tipos, `topeVivas`, nombres | El orden de las tarjetas y el tope de vivas del motivo |
| `tablero-y-despliegue-inicial.md` | Anillo del castillo y columnas del frente | Dimensiones de F5 |
| `design/art/art-bible.md` | Secciones 2, 3 (formas de la interfaz), 4 (semánticos y daltonismo) y 7 | Placa, estado *Distracción*, franja superior, íconos, animación ≤ 0,25 s, accesibilidad |
| `game-concept.md` | Pilares 1, 3 (prueba 3b) y 4; anti-pilar del dado | La prueba que esta interfaz cumple y las que respeta |
| `systems-index.md` | Fila 17 | Descripción del sistema y su capa |

**Cambios en GDD vecinos que este documento pide (aplicados al cerrar la tanda 3, 2026-11-25):**

- **Librería (#1):** que `ProbabilidadesSiguienteTirada` (regla 6) devuelva también el **id de la tabla base** de cada jugador (o que haya una consulta pura `TablaBaseSiguienteTirada`); sumar #17 a su tabla de dependencias y a Interactions.
- **Modificador (#11):** fijar como datos de la config los cinco ids de tabla base (`pierde_medio`, `pierde_tres_cuartos`, `gana_dos_a_uno`, `gana_cuatro_a_tres`, `normal`); levantar el supuesto PROVISIONAL sobre #17; **AC-19 queda cubierta por AC-16 de este GDD** (misma evidencia) y AC-30 comparte la captura en grises; cerrar la pregunta abierta de los repartos hacia esta interfaz y la de los íconos.
- **Reclutamiento (#10):** agregar la consulta pura `motivo_bloqueo(jugador, tipo)` (F2 de este GDD) con el orden de los códigos de la regla 7; aclarar que **#17 es dueño de la disposición y de los toques del recuadro** y #10 de las reglas y el contenido; la tarjeta bloqueada no muestra la línea de casillas.
- **Entrada y selección (#13):** regla 4: sumar «panel «?» abierto» como estado sin comandos de tablero ni de HUD y la `guarda_cierre_panel_ms`; regla 12 e Interactions: #17 decide el blanco, Entrada manda `Reclutar(tipo)`; la placa no recibe toques; la franja de avisos es la zona de avisos del HUD (#15, regla 18).
- **Localización (#14):** cerrar la pregunta abierta «Textos de las 5 caras y sus recuadros» (regla 13 de este GDD); sumar las clases de presupuesto chip 18, efecto 48 y nota 180 a F4 y a Tuning Knobs; alinear el vocabulario «castillo cortado» con «muros rotos» (regla 15, un término por concepto); confirmar que una clave plural pedida como texto directo (`pieza.<tipo>` con `n`) usa la forma plural.
- **Máquina de turno (#3):** nombrar #17 en Interactions; cerrar en su Visual/Audio las leyendas «Elegido por el tiempo» (aviso de texto en la zona de avisos, sin marca sobre la pieza ni la ficha; decisión del 2026-11-26) y «Sin reclutas disponibles» (placa central), según #15 (decisión del jugador al cerrar la tanda 3) y que la etiqueta dice «Tirando el dado…» en `EsperandoTirada`.
- **Timer (#12):** nombrar que el panel «?» no es un motivo de pausa (Interactions o Edge Cases).
- **Tablero (#4):** anotar en UI Requirements que ningún recuadro tapa las columnas 0 a 5 y 18 a 23.
- **Castillo (#5):** su texto «muros rotos» del panel pasa a «castillo cortado» (o se elige el otro término en los dos GDD).
- **`art-bible.md` §7 (íconos):** sumar los íconos y el marco de la tabla de Visual/Audio.
- **`systems-index.md`:** fila 17 en «Designed»; dependencias: sumar Librería (#1), Máquina (#3) y Entrada (#13) (duras) y Timer (#12, blanda); actualizar el orden de diseño de la capa Presentation y el Progress Tracker.

## Acceptance Criteria

*Tipos: **Logic** y **Integration** (automáticos); **UI** y **Visual/Feel** (capturas retenidas en `production/qa/evidence/`); los estados de partida de los criterios son estados de prueba construidos a mano.*

### Reglas (Sección C)

- **AC-1 (regla 1, solo lectura)** *(Logic)* — **GIVEN** un estado cualquiera, **WHEN** se abre y se cierra el panel, se muestra y se cierra el recuadro y se reproduce una placa, **THEN** la huella del estado es idéntica y no se emitió ningún comando.
- **AC-2 (regla 3, una placa por tirada)** *(Integration)* — **GIVEN** el turno 2 con la cara Grito, **WHEN** se aplica `Tirada` (contador N), **THEN** se dibuja una sola placa. **GIVEN** la misma `Tirada` entregada de nuevo con el contador N (reconexión), **THEN** no hay segunda placa y la etiqueta aparece directo. **GIVEN** una `Tirada` rechazada, **THEN** no cambia nada ni se muestra aviso.
- **AC-3 (regla 3, primer turno)** *(UI)* — **GIVEN** el turno 1, **THEN** no hay placa y la etiqueta dice «Apertura: sin dado» (inglés: «Opening: no dice»); y **WHEN** se abre el «?» con 12 contra 12 piezas, **THEN** las dos columnas muestran 20/40/20/20/0.
- **AC-4 (regla 4 y F1, tiempos)** *(Visual/Feel)* — **GIVEN** una tirada de cada una de las cinco caras, **WHEN** se mide una grabación a 60 cuadros por segundo, **THEN** la placa tapa el tablero ≤ 1,0 s, el conjunto dura ≤ 1,2 s (Distracción ≤ 1,5 s) y ningún cuadro tiene la placa a opacidad plena sobre una casilla con pieza más de 1,0 s. **GIVEN** una config del cliente con `t_caida + t_pausa + t_acople` = 1,05 s, **THEN** se rechaza al cargar *(Logic)*.
- **AC-5 (regla 4, no bloquea)** *(Integration)* — **GIVEN** la placa en pantalla con la fase `Mover` abierta, **WHEN** el jugador toca una pieza propia a los 0,3 s y después un destino legal, **THEN** la pieza se elige y `Mover` se acepta como sin placa, y el restante de la etapa del Timer ya venía corriendo desde que abrió la fase.
- **AC-6 (regla 5, etiqueta)** *(UI)* — **GIVEN** un turno con Arremetida resuelta, **THEN** la etiqueta muestra ícono y «Arremetida» hasta `TurnoTerminado` y se limpia al cerrar. **GIVEN** online esperando la tirada, **THEN** la etiqueta dice «Tirando el dado…» sin ningún nombre de cara.
- **AC-7 (regla 6, primer cuadro)** *(Integration)* — **GIVEN** Cuerno resuelto en local, **WHEN** se abre `Reclutar`, **THEN** un toque inyectado en ese mismo cuadro sobre una tarjeta disponible produce `Reclutar(tipo)` y se acepta.
- **AC-8 (regla 6, contenido y casillas)** *(UI)* — **GIVEN** Llamada con 9 soldados vivos (9 + 4 = 13 > 12), arquero y escudero disponibles y el anillo libre, **THEN** la tarjeta del soldado muestra candado, trama y el texto de `codigo.TopeDePiezas` **sin** línea de casillas; las otras dicen «Casillas 1 a 4» y el tablero marca las casillas 1 a 4 en el orden de `orden_aparicion`. Evidencia: `production/qa/evidence/interfaz-modificador-recuadro-reclutar.png`.
- **AC-9 (F4, `K`)** *(Logic)* — **GIVEN** Cuerno con el jinete bloqueado y catapulta y demoledor disponibles, **THEN** `K` = 2, se marcan las dos primeras casillas libres, la catapulta dice «Casillas 1 a 2» y el demoledor «Casilla 1». **GIVEN** ningún tipo disponible, **THEN** `K` = 0, no se abre el recuadro y se muestra «Sin reclutas disponibles».
- **AC-10 (regla 6, tarjeta bloqueada)** *(Integration)* — **GIVEN** una tarjeta bloqueada, **WHEN** se toca, **THEN** Entrada manda `Reclutar(tipo)`, la Librería lo rechaza con el código esperado, el recuadro sigue abierto, el reloj sigue corriendo y se muestra un solo aviso con el texto de ese código.
- **AC-11 (regla 6, timeout)** *(Integration)* — **GIVEN** el recuadro abierto, **WHEN** llega el `TurnTimeout` de mover, **THEN** la Máquina recluta el primer tipo disponible, el recuadro se cierra en ≤ 0,25 s la pieza reclutada aparece sin marca sobre su figura ni su ficha y la zona de avisos muestra «Elegido por el tiempo»; un `Reclutar` posterior se rechaza y se muestra el aviso neutro.
- **AC-12 (regla 6, quién lo ve)** *(Integration)* — **GIVEN** online con el rival decidiendo, **THEN** el cliente local ve la placa y la etiqueta y **no** el recuadro. **GIVEN** local contra el Rival IA reclutando, **THEN** no hay recuadro.
- **AC-13 (regla 7, modal)** *(Integration)* — **GIVEN** el panel abierto, una pieza elegida y la fase `Mover`, **WHEN** se toca una casilla, Deshacer, avanzar, Rendirse o una tarjeta, **THEN** no se manda ningún comando y la selección no cambia; **WHEN** se toca el velo, **THEN** el panel se cierra y no se manda comando. Timer y relojes siguen visibles y corriendo.
- **AC-14 (regla 7, guarda de cierre)** *(Integration)* — **GIVEN** el panel abierto con una pieza elegida cuyo destino legal queda bajo el velo, **WHEN** llegan dos toques sobre ese destino separados por 150 ms, **THEN** no se manda `Mover`; **WHEN** un toque llega 300 ms después del cierre, **THEN** se resuelve con normalidad.
- **AC-15 (regla 7, cierre automático)** *(Integration)* — **GIVEN** el panel abierto, **WHEN** se aplica una `Tirada` por un timeout, **THEN** el panel se cierra en el cuadro siguiente y corre la placa. **GIVEN** `Terminada`, **THEN** el panel está cerrado y el «?» oculto.
- **AC-16 (reglas 8 y 9, F3; cubre el AC-19 del Modificador)** *(UI)* — **GIVEN** un estado de prueba con 2 tramos rotos, 13 piezas sin el Rey y la tabla base `pierde_medio`, **WHEN** se abre el «?», **THEN** la columna del jugador muestra 15/45/0/0/40 (no 15/20/25/25/15); Llamada y Cuerno con «Castillo cortado»; Grito «base 20, ahora 45» y Arremetida «base 15, ahora 40», con el ícono de subió; Distracción sin diferencia; barras de 24/72/0/0/64 px. Evidencia: `production/qa/evidence/interfaz-modificador-panel.png`.
- **AC-17 (F3, validez)** *(Logic)* — **GIVEN** una tabla efectiva inyectada que suma 99, o con Distracción distinta de su base, **THEN** el panel muestra `ui.modif_panel_error`, registra el caso y ninguna cifra se corrige.
- **AC-18 (regla 10 y F2, motivos)** *(Logic)* — **GIVEN** estos casos, **THEN** el motivo es: (a) castillo cortado y sin llamadas → `CastilloSinReclutar`; (b) castillo sano y llamadas en 0 → `SinLlamadas`; (c) Cuerno con jinetes sin llamadas, 7 catapultas y 4 demoledores vivos → `TopeDePiezas`; (d) llamadas, sin tope y `libres` = 0 → `SinLugarParaReclutar`; (e) cara Disponible → `Ninguno`; (f) Arremetida con `piezas_sin_rey` = 12 → `YaMueveTodas`, con 13 → `Ninguno`.
- **AC-19 (regla 10, el caso del 85%)** *(UI)* — **GIVEN** 12 piezas sin el Rey y el castillo cortado (15/85/0/0/0), **THEN** Llamada y Cuerno muestran «Castillo cortado», Arremetida «Ya movés todas» y el Grito «base 20, ahora 85». Evidencia: `production/qa/evidence/interfaz-modificador-panel-85.png`.
- **AC-20 (regla 8, columnas)** *(Integration)* — **GIVEN** local con el turno del jugador 1, **THEN** «Vos» es el jugador 1 y, tras el cambio de turno, el 0. **GIVEN** online con el jugador local 0 y el turno del rival, **THEN** «Vos» sigue siendo el 0.
- **AC-21 (regla 11, nombre neutro)** *(Logic, test de datos)* — **GIVEN** las cinco claves `ui.modif_tabla_*` en español y en inglés, **THEN** existen las cinco y ningún texto contiene `pierd`, `gana`, `losing`, `winning`, `behind` ni `ahead`.
- **AC-22 (regla 12, llamadas)** *(UI)* — **GIVEN** el jugador 0 con 2 llamadas de Llamada gastadas y 1 de jinetes, **THEN** su pie muestra Llamada 4, catapulta 4, jinete 2 y demoledor 4, y el del rival sus propios valores.
- **AC-23 (regla 13, claves y presupuestos)** *(Logic)* — **GIVEN** las tablas `es` y `en`, **WHEN** corre el test de Localización (F1, F4 y F5) sobre todas las claves de la regla 13, **THEN** no falta ninguna, ninguna supera su presupuesto (efecto 48, chip 18, nota 180, aviso 60, etiqueta 24) y las dos pasan con el texto inflado un 40%. **GIVEN** un `ancho_car` medido de 9 px, **THEN** F5 falla y nombra el chequeo.
- **AC-24 (regla 13, idioma)** *(Integration)* — **GIVEN** el panel abierto, **WHEN** se cambia ES a EN, **THEN** en el cuadro siguiente todos los textos están en inglés, el panel sigue abierto con los mismos datos y ninguna animación se reinició.
- **AC-25 (regla 14, grises y colores)** *(Visual/Feel)* — **GIVEN** las cinco placas, los íconos de subió, bajó y candado y una tarjeta bloqueada en escala de grises, **THEN** se distinguen por forma o texto; y las barras, chips e íconos de diferencia **no usan** el verde de la vida, el bermellón del ataque, el acero del escudo ni el ámbar del timer. Evidencia: `production/qa/evidence/interfaz-modificador-grises.png`.
- **AC-26 (regla 14, sin hover)** *(Integration)* — **GIVEN** un puntero táctil sin hover, **THEN** el panel y el recuadro muestran los mismos datos y motivos que con mouse.
- **AC-27 (regla 17, métricas)** *(Integration)* — **GIVEN** el jugador abre y cierra el panel, **THEN** se emite `PanelAbierto(jugador, turno, fase, ms_abierto)` con `ms_abierto` igual al tiempo real ±1 cuadro.
- **AC-28 (regla 4, «Reducir efectos»)** *(Visual/Feel)* — **GIVEN** «Reducir efectos» activo, **THEN** la placa aparece por fundido de 0,10 s, sin golpe ni pulso, y dura ≤ 0,9 s en total.
- **AC-29 (reloj sin pausa)** *(Integration)* — **GIVEN** el panel abierto 10 s en `Mover`, **THEN** el restante de la etapa del Timer bajó esos 10 s y no se emitió ninguna `Pausar`.
- **AC-30 (placa y turno que se cierra)** *(Integration)* — **GIVEN** Distracción sin acciones posibles en `Atacar` (la Máquina salta la fase), **THEN** la placa se cierra con un fundido de 0,10 s y ningún estado queda esperándola.

- **AC-33 (regla 18, identificadores)** *(Logic)* — **GIVEN** la escena de UI del dado, **THEN** existen elementos con los identificadores `modif_boton_ayuda`, `modif_panel`, `modif_etiqueta`, `modif_placa` y `modif_recuadro_reclutar`, y el tutorial puede resolver cada uno por nombre. Las reglas 2 y 15 quedan cubiertas por AC-6, AC-8, AC-12 y AC-13, y la 16 por AC-11 y AC-30.

### Fórmulas (Sección D)

- **AC-31 (F5, geometría)** *(Logic)* — **GIVEN** las constantes de layout vigentes, **THEN** `col_ini_panel` = 6 ≥ `anillo_cols`, `alto_panel` = 596 ≤ 612 y el recuadro mide 668 con un margen de 482 px ≥ 408. **GIVEN** `panel_cols` = 13, **THEN** la verificación falla y nombra el chequeo.
- **AC-32 (F1, F2, F3 y F4)** *(Logic)* — los casos de AC-4, AC-9, AC-16, AC-17 y AC-18 cubren una vez cada fórmula en su valor típico; los de frontera son: `t_pausa` en 0,40 s y la suma de las tres perillas en exactamente 1,0 s (válida) y 1,05 s (inválida); `efectivo` = 0 y 100 en F3; `libres` = 0 y `piezas_sin_rey` en 12 y 13 en F2; un solo tipo disponible de 1 pieza y ninguno en F4.

### Huecos que quedan (lente de QA)

- **Sin criterio automático:** que el panel «se entienda» y «se use», que el nombre neutro no funcione como etiqueta y que el Grito en 85% no se sienta injusto: son preguntas de playtest medidas con `PanelAbierto` (Open Questions), no criterios.
- **PROVISIONAL:** ninguno. AC-8, AC-9 (Render, #18) y AC-27 (Métricas, #19) dejaron de serlo al cerrar la tanda 4. AC-5, AC-6, AC-13, AC-16 y AC-18 dejaron de serlo al cerrar la tanda 3 (Entrada, HUD, Librería y Reclutamiento ya lo recogen).
- **NOT SOURCEABLE:** `docs/engine-reference/unity/` no cubre UI Toolkit sobre un tablero dibujado fuera de ella, el puntero único mouse/dedo en WebGL ni la escala de texto en WebGL, así que AC-5, AC-13, AC-14 y AC-26 **no se pueden dar por pasados desde la referencia**: se verifican con el prototipo en el navegador y en un teléfono real.
- **Sin presupuesto propio de rendimiento:** el panel hace dos consultas puras, las derivaciones de F2 y el dibujo de 10 celdas; hereda el presupuesto de la Librería (ADR) y no se mide acá.

## Open Questions

| Pregunta | Dueño | Cuándo |
|---|---|---|
| ¿Se entienden los dos repartos con el chip, «base X, ahora Y» y la nota fija? Medir cuántas partidas abren el «?» y cuánto tiempo (`PanelAbierto`), y preguntar «¿por qué el Grito está tan alto?» | Playtest y Métricas (#19) | Primeras partidas del MVP |
| ¿El nombre neutro de la tabla («Muchos refuerzos», «Más empuje») igual funciona como la etiqueta «vas perdiendo» que se quiso evitar? Alternativa: «Tabla 1 a 5» o no mostrarlo | Playtest | Primeras partidas del MVP |
| ¿Un panel modal sin pausa se siente justo cuando se lo deja abierto? ¿Hace falta un aviso en el panel de que el reloj corre, o el timer a la vista alcanza? | Playtest | MVP |
| `motivo_cara` resume por cara con el primer tipo con llamadas: ¿se entiende cuando otro tipo de la misma cara está bloqueado por otra causa? El recuadro detalla por tipo, pero solo cuando la cara sale | Playtest, Reclutamiento (#10) | MVP |
| Medir `ancho_car` con la fuente elegida: con 9 px fallan el chip y la línea de efecto con el texto inflado (F5) | art-director, localization-lead | Antes de la primera build con textos |
| Íconos de las cinco caras, de subió, bajó, candado, dado, reloj y piezas vivas; marco del panel y del recuadro; estilo de la barra | Director de arte | Antes de `/asset-spec` |
| ~~Lugar exacto de la etiqueta en la franja superior~~ **Resuelta 2026-11-25:** el HUD (#15, regla 15) confirma la izquierda del timer y el «?» en el extremo derecho | — | — |
| Pantalla estrecha: ¿el panel pasa a dos pestañas «Vos» y «Rival»? ¿Cómo se escala el recuadro? | #41 Adaptación móvil | En su GDD |
| Las listas de tipos de `cara.llamada_efecto` y `cara.cuerno_efecto` están escritas en el texto: si la config cambia los tipos, quedan viejas. ¿Se arma la línea desde `tablaReclutas` o se deja a mano con un test? | Localización (#14), Reclutamiento (#10) | Antes de cambiar la tabla de reclutas |
| «Castillo cortado» (Reclutamiento y este GDD) contra «muros rotos» (Castillo): un solo término por concepto | Localización (#14), Castillo (#5) | Antes de la primera build con textos |
| ¿Mostrar en el panel «qué pasaría si resolvieras así» (la tabla con el tablero después de los ataques asignados)? Sería una herramienta del pilar 1, pero suma una simulación y puede inclinar la planificación | game-designer | Post-MVP, si las partidas lo piden |
| Cuando el turno se cierra mientras la placa está en pantalla (Distracción sin acciones), ¿se siente como un turno «comido» y conviene sostener la placa 1 s? | Playtest, Modificador (#11) | MVP |
| En el online, ¿cuánto se espera la tirada antes de avisar, y qué pasa si nunca llega? | Servidor (#28) | Antes de lanzar el online |
| Nombres en inglés de las caras («Call to arms», «War horn», «Charge», «Heavy push»…) y de los motivos | localization-lead, hablante nativo | Antes de la primera build con textos |
| **NOT SOURCEABLE:** UI Toolkit sobre un tablero dibujado fuera de ella, puntero único mouse/dedo y escala de texto en WebGL no están cubiertos por `docs/engine-reference/unity/` | unity-ui-specialist, prototipo en el navegador y en un teléfono | Antes de implementar |
| ¿Hace falta un botón para ocultar el recuadro de reclutar y ver el tablero que tapa (unas 10 columnas por 5 filas en el centro)? Hoy el jugador no puede ocultarlo | ux-designer | MVP |
| **Pedido del Tablero (#4, UI Requirements, 2026-11-27):** en el turno 2 (primera tirada, de rojo) el recuadro no tapa ninguna casilla con pieza azul ni ninguna en alcance rojo de ese turno (lo calcula Alcance, #7); como guía, no tapa las filas 4 a 10 entre las columnas 5 y 18 y puede ir en las filas 0-3 u 11-14. ¿Cómo se ubica (F5)? | Interfaz del Modificador (#17), con #7 | MVP |

**Resuelto en este GDD (2026-11-25):** la disposición y los toques del recuadro de reclutar son de #17 (las reglas y el contenido, de #10); el panel es modal, sin pausa y con un velo que solo cierra; el panel muestra la tabla efectiva con «base X, ahora Y», un chip de motivo y una nota fija; el nombre de la tabla base es neutro; los textos de las cinco caras y de sus recuadros quedaron definidos (regla 13).
