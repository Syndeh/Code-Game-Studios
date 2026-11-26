# Ficha de pieza y HUD

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: jugador + Claude (design-system)
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Implements Pillar**: 1 Planificar y cosechar (con 3 Siempre hay vuelta, con límites, en su parte visible: el Grito en la ficha, y 4 Duelo corto, en su parte visible: el timer y los botones sin confirmación). El pilar 2 solo de forma indirecta: la regla 10 cede el lugar de la ficha a la vista previa (#16), que es donde se ve el golpe combinado
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS 2026-11-25 (lente aplicada en esta sesión, sin agente) — puntos 1 (el encabezado reclamaba el pilar 2 sin ninguna regla que lo cumpla) y 2 (el bloqueo de 300 ms parecía restar 0,6 s por turno, contra el pilar 4) REVISED; puntos 3 a 5 (atenuación por gris y marco en lugar de la opacidad que eligió el jugador, anillo del tramo final sin ámbar, opacidad de la placa en 96% contra el 85–90% del art bible) **aceptados por el jugador** el 2026-11-25 al cerrar la tanda 3 (gris y marco, anillo de color base, placa al 96%)

## Summary

La **Ficha de pieza y HUD** es la capa de interfaz de solo lectura que muestra sobre cada pieza su tipo, vida, ataque y escudo (con el bono del turno ya sumado y la base al lado cuando difiere) y, en los bordes de la pantalla, de quién es el turno, cuánto tiempo queda, cuántas piezas se movieron y los botones de avanzar, deshacer y rendirse. Existe para que el jugador planifique leyendo en vez de contando, que es la base del pilar «el daño es fijo y se ve antes de resolver», y vive en la capa del sistema para que ninguna skin pueda tocar una cifra.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `Librería de reglas, Catálogo de piezas, Máquina de turno, Timer de turno, Modificador, Localización` (blandas: Entrada y selección, Movimiento, Castillo y muros, Alcance y asignación, Cobertura del escudero, Reclutamiento, Tablero)

## Overview

La **Ficha de pieza y HUD** es la capa de interfaz que le da al jugador, sin menús y sin historial, todo lo que necesita saber para planificar su turno. Tiene dos partes. La **ficha** es una placa plana que va sobre la parte baja de cada pieza con su tipo, su vida, su ataque y su escudo (los valores *efectivos*, con el bono del Grito o del muro ya sumados y la base al lado cuando difiere), más una **ficha extendida** al elegir o inspeccionar una pieza, que agrega movimiento, alcance, área y estado (cubre, cubierta, en el muro, ya movida). El **HUD** son los controles y avisos de pantalla alrededor del tablero: el contorno y el emblema del bando de turno, el timer de la etapa y los dos relojes de partida, el contador de piezas movidas contra el límite, los botones de avanzar, Deshacer y Rendirse, el contador de tramos rotos de cada castillo y las leyendas breves de los eventos de la Máquina.

El sistema **solo lee**. Toma las cifras de la Librería (`EffectiveStats` y las consultas puras), la fase y la pila de la Máquina de turno, la vista de relojes del Timer, las caras y bonos del Modificador, y los textos de Localización. No calcula daño, no guarda estado de la partida y no manda comandos por su cuenta: quien traduce un toque en `Mover`, `Deshacer` o `Resolver` es Entrada (#13); acá se decide qué se ve, dónde y en qué estado queda cada control. Vive en la **capa del sistema** del art bible: plana, geométrica y fuera del alcance de las skins, para que una skin nunca pueda esconder ni alterar una cifra.

El jugador interactúa con la ficha de forma pasiva (la lee) y con el HUD de forma activa solo en los botones. Existe porque el pilar 1 (*«el daño es fijo y se ve antes de resolver»*) empieza por las cifras: si el jugador no puede leer en un vistazo cuánto pega, cuánto aguanta y cuánto vive cada pieza, no puede planificar, y el pilar 3b (ver el Grito y las probabilidades) y el pilar 4 (un duelo de 15 a 20 minutos con timer) pierden su base. **No es** la vista previa del daño (#16), la placa ni el panel «?» del Modificador (#17), ni el dibujo de las figuras (#18): este sistema fija el lugar donde conviven y les cede el protagonismo cuando toca (en la fase de ataque la vista previa reemplaza a la ficha del objetivo).

→ Cómo se implementa la capa (UI Toolkit sobre un tablero que ocupa toda la pantalla, en WebGL y con táctil) queda para un ADR; este documento fija solo qué se ve y cómo se comporta.

## Player Fantasy

**Leo el tablero, no lo cuento.** Nadie mira la ficha por gusto: la mira para decidir. La fantasía de este sistema es la de un jugador que **nunca hace cuentas a mano ni busca un dato escondido**. Cada pieza, propia o rival, dice en un vistazo cuánto pega, cuánto aguanta y cuánto vive, con el bono del turno ya sumado y la base al lado para saber de dónde viene; y el borde de la pantalla dice de quién es el turno y cuánto tiempo queda sin pedir atención. Lo que el jugador siente es **control sin fricción**: toda la información que el juego tiene sobre el plan está a la vista antes de que se resuelva, y lo que no hace falta no se ve.

**Momento ancla (pilar 1).** Turno de doce piezas con Grito de guerra. El jugador apoya el dedo en su arquero: la ficha extendida dice *ataque 8 (6)*, *escudo 2*, *alcance 1–4*, y en el cuadro siguiente ya sabe que con dos arqueros y un soldado le alcanza para el tramo del muro. No contó casillas, no sumó el +2 de memoria, no abrió ningún menú. Cuando resuelve, cae exactamente lo que leyó. *Pilar citado:* «El daño es fijo y se ve antes de resolver.»

**Momento de apoyo (pilar 4).** Quedan diez segundos de la etapa de mover. El anillo se pone ámbar y pulsa, el número baja, y el jugador sabe *sin buscarlo* que el que lo limita es la etapa y no su reloj de partida. Termina el movimiento que tenía en curso y pasa a atacar. La presión es buscada y se lee; no sorprende. *Pilar citado:* «El timer es presión buscada.»

**Qué siente y cuándo:**

- *Al empezar el turno:* el contorno del tablero y el emblema del bando le dicen de quién es, sin leer ningún texto.
- *Al elegir una pieza rival para inspeccionarla:* las mismas cifras que las suyas, sin ventajas ocultas; todo es público (anti-pilar «NOT información oculta al resolver»).
- *Al activarse el Grito:* ve el +2 en cada ficha afectada antes de asignar un solo ataque; el dado no decide nada que no haya visto.
- *Al terminar el turno:* cierra la fase con un solo toque, sin diálogo de confirmación.

**Lo que esta fantasía le exige al resto del diseño:**

- La cifra que se muestra es **siempre la efectiva** y, si difiere, también la base. Nunca una cifra calculada por el HUD: la única fuente es `EffectiveStats`.
- **Nada solo por color** y nada que dependa del hover: todo se lee en grises, con daltonismo y con un toque (art bible §7, regla del proyecto).
- **Un lugar fijo para cada cosa** y ningún control tapa una casilla: el jugador aprende dónde mirar una vez y no vuelve a buscar (pilar 4).
- **Un HUD silencioso:** un aviso solo existe si cambia una decisión; el ámbar existe solo para el timer que de verdad limita.
- **Las skins no tocan la ficha:** si una skin pudiera cambiar un número o su color, el «mismo ejército, duelo justo» del Catálogo dejaría de ser verificable a simple vista (anti-pilar «NOT pagar para ganar»).

**Lo que este encuadre no promete:** una pantalla vacía. La ficha completa tapa parte de la figura (conflicto ya resuelto en el art bible con la opción «Ficha simple») y el HUD tiene muchos elementos; promete que cada uno está donde se espera y que ninguno exige una cuenta mental.

*Candidatas consideradas (lente `creative-director`, modo `full`, sin agente):* **A, «Leo el tablero, no lo cuento»** (elegida, anclada en el pilar 1: lo planeado se lee antes de resolver); **B, «Sé siempre cuánto me queda»** (anclada en el pilar 4; queda como momento de apoyo, porque el reloj es del Timer y la fantasía de este sistema es más amplia que el reloj); **C, «El tablero habla solo cuando hace falta»** (anclada en el pilar 4; descartada como fantasía porque describe una restricción de diseño, no una sensación, y está recogida en «Un HUD silencioso»). **Confirmada por el jugador el 2026-11-26.**

## Detailed Design

### Core Rules

**Vistas por rol (Section C, modo `full`, sin agentes).**

- *game-designer:* la ficha es la puerta del pilar 1: cada decisión del turno (mover, asignar, cubrir, reclutar) arranca leyendo cifras. El jugador decide dónde mirar (elegir una pieza) y cuándo avanzar; nunca decide *si* una cifra se muestra, salvo la opción «Ficha simple» que ya fijó el art bible. Lo que el jugador **no puede** hacer: ocultar el timer, cambiar el color del bando, pedir que una skin cambie la ficha.
- *ux-designer:* lo más importante, lo más prominente: en `Mover` el contador de piezas movidas y la ficha de lo elegido; en `Atacar` la vista previa (que le gana a todo, #16); en todo momento el timer. Cada acción tiene respuesta en el mismo cuadro. **Alerta:** atenuar la ficha para «pieza ya movida», como eligió el jugador, no se puede hacer bajando la opacidad de la placa sin romper la legibilidad (F5); se resuelve apagando el color a gris y sumando un marco (regla 5), y se valida con captura y filtros antes de darla por buena (AC-9, Open Questions 1).
- *ui-programmer:* viable con UI Toolkit sobre un tablero de pantalla completa. Tres riesgos: (1) hasta 61 fichas por jugador (122 en el peor caso) con números que cambian solo con eventos, así que se actualizan por evento y no por cuadro; (2) el ancho de las etiquetas traducidas con +40% (Localización, F5) contra 144 px de margen, resuelto con iconos y renglones que envuelven; (3) UI Toolkit sobre un tablero táctil en WebGL no está cubierto por `docs/engine-reference/unity/modules/ui.md` (**NOT SOURCEABLE**, queda para el ADR).
- *art-director:* la ficha es de la capa del sistema (plana, geométrica); este GDD pide siete íconos que el art bible no tiene (regla 17) y fija un cambio a su sección 2 (regla 12, tramo final).
- *technical-artist:* las fichas son elementos de UI, no geometría del mundo, así que no entran en el presupuesto de las figuras; sí entran en el de dibujo de pantalla. Una placa translúcida por pieza sobre 122 piezas es barata; lo que cuesta es el texto, por eso los números se cachean y solo se redibujan al cambiar.

**Conflictos entre vistas, resueltos en esta sesión:** (a) atenuar la ficha por opacidad (decisión del jugador, 2026-11-25) contra contraste de texto ≥ 4,5 y de íconos ≥ 3 (art bible §4, WCAG): la atenuación pasa a apagar el color a gris con un marco, con la placa intacta (regla 5, F5). (b) Anillo de etapa y reloj de partida sin dos ámbar (decisión del jugador, 2026-11-25): el ámbar va al que limita (regla 12). (c) El anillo grueso ámbar del tramo final (art bible §2) contra «ámbar solo al que limita»: el tramo final pasa a ser un anillo grueso de color base; propuesto, requiere confirmación (Open Questions 2).

#### A. Lecturas y capa del sistema

1. **Qué es y qué lee.** El HUD y la ficha son una capa de cliente de **solo lectura**. Lee, y solo lee, estas fuentes: las consultas puras de la Librería (`EffectiveStats`, vida, tipo, casilla, dueño, `enMuro`, `tramosRotos`, `Cubre`, `CubiertoPor`; Librería regla 10); la fase, el jugador del turno, el número de turno y la pila de la Máquina (#3); `piezasMovidas`, el límite del turno y `usados` de Movimiento (#6); la vista y los eventos del Timer (#12, regla 13 y regla 12); la cara del turno y los bonos activos del Modificador (#11); los textos de Localización (#14); la pieza elegida o en inspección de Entrada (#13); y las opciones guardadas (Ficha simple, Reducir efectos). **El HUD no recalcula ninguna cifra del juego**: lo que muestra es lo que devuelve la consulta. Su estado propio (atenuación, bloqueo del botón, cola de leyendas, selección) es del cliente: no entra en la forma canónica del estado ni en la huella (igual que la selección de Entrada).
2. **Capa del sistema, fuera del alcance de las skins.** La ficha, los íconos del HUD, los números, los marcadores y los avisos se dibujan con los activos del sistema (art bible §3 *Gramática de formas de la interfaz*, §4 colores semánticos, §7). Una skin dibuja la figura y nada más: no lee ni cambia una cifra, un color ni la posición de la ficha (anti-pilar «NOT pagar para ganar»; Catálogo, regla 9). Los colores de la ficha son solo los semánticos: vida `#8EDB6A`, ataque `#E55A40`, escudo `#7F9BB8`; fondo Tinta `#1E1A17`; texto `#F5F1E8`. Hueso y Ciruela son exclusivos del bando (contorno del tablero y emblema).

#### B. La ficha

3. **Ficha compacta: contenido y forma.** Cada pieza viva lleva una placa rectangular de esquinas redondeadas, plana y sin textura, **centrada en el ancho de su casilla y pegada a la parte baja** (art bible §7). Dos renglones:
   - **Renglón A:** ícono de tipo y **vida actual**, los dos en verde vida, en la primera posición.
   - **Renglón B:** puño y **ataque efectivo** (bermellón) · escudo heráldico y **escudo efectivo** (acero).

   Números de **14 px o más** (a 1080p), negrita, cifras de ancho fijo, con borde Tinta de 2 px (art bible §7); fondo Tinta con opacidad `ficha_opacidad_pct` (**96%** propuesto; el art bible pide 85–90%, pero con 88% el puño y el escudo quedan por debajo de contraste 3:1 sobre el suelo Hueso, F5). La placa mide como mucho **64 × 40 px** dentro de la casilla de 68 px (F1). Los tres números existen siempre para una pieza viva (Catálogo: vida y ataque ≥ 1, escudo ≥ 0), así que nunca hay un «?» ni un guion; un escudo de 0 se muestra como `0`.
4. **Qué número se muestra.** El que devuelve `EffectiveStats(estado, pieza)`: el catálogo más los bonos del turno más la posición (Catálogo, regla 5; Modificador F3; Castillo F `estadisticas_en_muro`). La ficha compacta muestra **solo el valor efectivo**. Cuando el efectivo **difiere de la base**, el número lleva un **subrayado de 2 px del mismo color**, que es una forma y no un color nuevo. La base (y la diferencia) se lee en la ficha extendida (regla 8). Las cifras de la ficha son de **hasta 2 dígitos**: el Catálogo vigente y sus techos seguros llegan a 34 de ataque y 30 de vida, y Grito y muro suman como mucho 10; un test del repositorio verifica que ninguna cifra del Catálogo ni su bono máximo pase de 99 (AC-4).
5. **Pieza ya movida: ficha atenuada (decisión del jugador, 2026-11-25; mecanismo ajustado por F5, confirmado por el jugador el 2026-11-25).** Durante `Reclutar` y `Mover` del turno de su dueño, la ficha compacta de una pieza **propia con `usados > 0`** (el Rey incluido) se **apaga**: sus números e íconos pasan del color semántico a un **gris apagado `#A8A49B`** (contraste 6,9 contra Tinta) y la placa gana un **marco interior de 1 px `#F5F1E8`**, a 1 px dentro del borde. La placa conserva su opacidad y el borde Tinta de los números no cambia. *Por qué no bajar solo la opacidad de la placa, que es lo que eligió el jugador:* F5 muestra que sobre el suelo Ciruela eso cambia la placa apenas 4 puntos de L\* (no se ve) y que, sobre Hueso, deja los íconos de puño y escudo por debajo de contraste 3:1; apagar el color y sumar el marco se ve igual sobre los tres suelos y no toca la legibilidad. La marca se calcula de `usados` y por eso `Deshacer` la quita sola (Movimiento, regla 10). Las piezas recién reclutadas (`usados = 0`) no la llevan. No hay atenuación en el turno del rival, en `Atacar` ni en piezas rivales. *Qué significa:* «ya la toqué este turno», no «no se puede mover»: una pieza atenuada con movimiento restante sigue siendo comandable. Si la captura de AC-9 muestra que la marca (color apagado más marco) no se distingue a la distancia de juego o en grises, el plan de respaldo es una **huella de 14 px** junto a la ficha, que el jugador descartó por ahora al elegir la atenuación (Open Questions 1).
6. **Banderín del bono del turno.** Una pieza con un bono temporal activo (hoy solo el Grito de guerra, que da +2 de movimiento y, salvo al demoledor, +2 de ataque) lleva un **banderín de 12 × 12 px** en la esquina superior derecha de su casilla, Tinta con trazo de `#F5F1E8`, en la capa del sistema (propuesta del Modificador, a confirmar por el art bible). Existe porque el demoledor sube su movimiento pero no su ataque: sin banderín, la ficha compacta (que no muestra movimiento) no diría que está bajo el Grito. El banderín **no es** el subrayado: el subrayado dice «este número difiere de la base» (también por el muro) y el banderín dice «esta pieza está bajo el bono del turno».
7. **«Ficha simple» (opción del jugador).** Por defecto **apagada**. Encendida, la ficha compacta muestra solo el renglón A (tipo y vida); el ataque y el escudo desaparecen del tablero pero **siguen visibles en la ficha extendida** de la pieza elegida o en inspección, y la vista previa de daño de la fase de ataque se ve siempre (art bible §7; pilar 1). El banderín y la atenuación de la regla 5 se mantienen. La opción la guarda Ajustes (#20, PROVISIONAL; hasta que exista, el cliente la guarda por su cuenta) y es **por jugador, no por partida**.
8. **Ficha extendida.** Aparece para la pieza **elegida o en inspección** (Entrada, regla 5; una sola a la vez) en una tarjeta fija del **margen izquierdo** (regla 15), y desaparece al soltar la selección. Filas, en este orden:

   | Fila | Contenido | Se muestra |
   |---|---|---|
   | Tipo | Ícono de tipo y nombre (`pieza.<tipo>`, Localización) | Siempre |
   | Vida | `vida actual / vida máxima` | Siempre |
   | Ataque | efectivo, y la base entre paréntesis si difiere | Siempre |
   | Escudo | efectivo, y la base entre paréntesis si difiere | Siempre |
   | Movimiento | `restante / efectivo`, y la base entre paréntesis si difiere. `restante` solo para una pieza propia en `Mover` (Movimiento, F1); en cualquier otro caso, solo el efectivo | Siempre |
   | Alcance | `mín–máx` efectivo, y el base entre paréntesis si difiere (arquero en el muro: `1–6 (1–4)`) | Todo tipo menos el demoledor |
   | Área | `danoArea` (catapulta: 6). Demoledor: su explosión, que es su ataque efectivo sin bono (30); fila con ícono de área | Catapulta y demoledor |
   | Estados | Hasta tres estados de texto, cada uno de una o dos líneas: `En el muro` (con la vida del tramo `n/30`), `Cubre a [tipo]`, `Cubierta por [tipo]`, `Ya movida` | Solo los que aplican |

   Los íconos de movimiento, alcance y área son nuevos (regla 17). Cada fila lleva ícono y número; **el texto solo aparece en el nombre del tipo y en los estados**, así cabe en 144 px con el texto inflado de Localización (regla 16). El Grito sobre una catapulta muestra `ataque 12 (10)` y `área 6` sin cambio; sobre el demoledor, `ataque 30` sin cambio y `movimiento 8 (6)` (Modificador, regla 7).
9. **Tramos.** Un **tramo de muro** no tiene ficha compacta (no es una pieza). Su estado (entero, mellado, roto) lo dibuja el mundo (Castillo). Su vida exacta se lee **al tocarlo**: la tarjeta de la regla 8 muestra el ícono de muro, `Tramo de muro` y `vida actual / 30`, igual con mouse que con dedo y sin hover. *PROVISIONAL:* Entrada hoy elige solo piezas; hace falta que una pulsación sobre un tramo en pie y sin pieza lo deje en inspección (Open Questions 3).
10. **Fase de ataque: la vista previa ocupa el lugar de la ficha.** En `Atacar`, una pieza o un tramo que es **objetivo de al menos una asignación** cede el ancla de su ficha a la vista previa (#16), que ocupa su lugar y no se pone al lado (art bible §7). Cuando se quita la última asignación a ese objetivo, la ficha vuelve **en el mismo cuadro**. La ficha extendida de la pieza elegida no se ve afectada. Este GDD cede el ancla y fija solo que la ficha y la vista previa **nunca están a la vez sobre la misma pieza**; el tamaño y el contenido de la vista previa son de #16.
11. **Pieza que muere.** La ficha desaparece **en el mismo cuadro** que la figura, y es la placa MUERE (#16, #18) la que queda. Si la pieza muerta estaba elegida o en inspección, la tarjeta se va con la selección (Entrada, Edge Cases).

#### C. Turno, tiempo y botones

12. **Timer y relojes (resuelve la pregunta del Timer, #12).** Arriba al centro, de izquierda a derecha: **reloj del jugador 0**, **emblema del bando de turno**, **anillo de la etapa** con el número `mostrado_s` adentro, **número de turno**, **reloj del jugador 1** (el jugador 0 va a la izquierda, como su castillo; Tablero, regla 4). Cada reloj muestra `m:ss` (F2) con el emblema de su bando; el del jugador activo lleva además un subrayado grueso. **Un solo ámbar a la vez** (decisión del jugador, 2026-11-25): el ámbar `#E89A2C` y el pulso van **solo al temporizador que limita**, según `vinculante` de la vista de relojes (F3):
    - Si `vinculante` = `Etapa` y hay `AvisoEtapa`: el **anillo** va en ámbar y pulsa una vez por segundo.
    - Si `vinculante` = `Reloj`: el **reloj del jugador activo** va en ámbar (nivel 1) y pulsa (nivel 2); el anillo queda en su color base.
    - El temporizador **que no limita** y ya está en aviso (nivel 1 o 2 del reloj, o `AvisoEtapa` con el reloj limitando) **no usa ámbar ni pulso**: se marca con **borde grueso y número en negrita** (forma). El reloj del jugador que **no** tiene el turno nunca va en ámbar.
    - «Reducir efectos» apaga el pulso; la forma (borde grueso) y el número se mantienen.
    - **Tramo final** (Timer, `TramoFinalIniciado`): una sola vez, el banner «Tramo final» al centro y el anillo pasa a **trazo grueso permanente de color base**; el ámbar del anillo sigue siendo solo de aviso (confirmado por el jugador el 2026-11-25; el art bible §2 y el Timer ya lo dicen así).
    - **Sin límite:** sin anillo ni relojes; en su lugar una marca con el ícono de infinito y el texto `Sin límite` (`ui.hud_sin_limite`), sin dejar un hueco vacío.
    - **`EsperandoTirada` y pausa:** el anillo queda vacío y quieto con el ícono de dado y el texto `Esperando la tirada` (o `Pausa`); los dos relojes se ven **quietos** (sin subrayado de activo ni pulso) con el mismo indicador (Máquina, regla 14; Timer, regla 10). Online, con la predicción en 0 y sin decisión del servidor, el anillo muestra `Esperando al servidor` (`enEsperaDeTirada` falso; Timer, regla 15).
    - **`TiempoAgotado`:** una marca breve «Tiempo» junto al anillo, ≤ 1 s, sin sonido de error (Timer, Visual/Audio).
13. **Bando de turno.** El **contorno del tablero** toma el color del bando de turno (Hueso o Ciruela) y el **emblema** junto al anillo cambia con él; juntos dicen de quién es el turno sin leer texto (art bible §7, §4). Al `TurnoIniciado` aparece el banner del turno al centro, como las leyendas de la regla 19: entra en ≤ 0,25 s, dura `leyenda_ms` (1.000 ms), no tapa una casilla con pieza más de 1 s y no suma tiempo al turno. Mientras el jugador local es el que tiene los comandos habilitados, el banner dice `Tu turno`; si no, `Turno del rival` (en local, hotseat, dice `Turno de [bando]`).
14. **Controles de la partida (lista cerrada; el «?» es de #17 y el botón ES/EN es de la regla 21).** Cuatro controles, todos con **objetivo táctil ≥ `objetivo_tactil_pt`** (44 pt; Entrada) y texto más ícono, nunca solo color:
    - **Avanzar** (abajo a la derecha): `Atacar` en `Mover`, `Terminar turno` en `Atacar`. Entrada manda `TerminarMovimiento` o `Resolver`. Sin diálogo de confirmación (pilar 4). **Se bloquea `bloqueo_avance_ms` (300 ms) tras cada cambio de fase y tras cada toque aceptado** (decisión del jugador, 2026-11-25) para que un doble clic no cierre la fase y luego el turno; un toque durante el bloqueo se ignora sin aviso. Deshabilitado (y con aspecto de deshabilitado) cuando no es el turno del jugador local, en `EsperandoTirada`, en `Reclutar` y en `Terminada`.
    - **Contador de piezas movidas** (arriba del botón de avanzar, solo en `Mover`): `piezasMovidas / límite` (por ejemplo `5/12`). Con Arremetida (`LimiteMovimientos(sin límite)`) muestra `piezasMovidas` y el ícono de infinito, sin tope. Al llegar al límite, el contador pasa a **borde grueso** (cupo lleno: solo se mueven el Rey y las piezas ya movidas; Movimiento, estados). El Rey no cuenta.
    - **Deshacer** (abajo a la izquierda, `Deshacer último movimiento`): activo solo si la pila de deshacer tiene al menos una entrada y la fase es `Mover` del turno del jugador local; desactivado en cualquier otro caso, de modo que `NadaQueDeshacer` no salga por la interfaz (Máquina, UI Requirements). Es la esquina opuesta a Avanzar.
    - **Rendirse** (**arriba a la izquierda**, simétrico al «?»; decisión del jugador, 2026-11-25): botón chico con bandera blanca y el texto `Rendirse`. **Visible solo** para el jugador del turno que cumple la condición de la Librería (regla 3: tiene la mitad de las piezas vivas del rival o menos, `mias × 2 ≤ rival`); no visible en ningún otro caso ni para el que va ganando. El HUD **consulta** esa condición, no la recalcula. Al tocarlo, Entrada pide la confirmación de dos botones (Entrada, regla 13).
15. **Mapa de pantalla (1080p).** El tablero (1632 × 1020 px) ocupa de y = 60 a y = 1080 y de x = 144 a x = 1776; **ningún elemento del HUD tapa una casilla** salvo las leyendas y placas centrales de ≤ 1 s (art bible §2, reglas de ritmo).

    | Zona | Contenido |
    |---|---|
    | Banda superior (60 px), centro | Regla 12: relojes, emblema, anillo, número de turno |
    | Banda superior, a la izquierda | **Rendirse** (x 8 a 136) y, a la izquierda del timer (x 500 a 750), la **etiqueta de la cara del turno** (ícono y nombre; contenido de #17, que ya propuso este lugar y lo confirma este GDD) |
    | Banda superior, a la derecha | La **zona de avisos** (x 1170 a 1840; regla 18) y, en el extremo, el **«?»** (panel de probabilidades; #17) con su blanco de 60 × 60 px, que cabe justo en los 60 px de la banda |
    | Margen izquierdo (144 px) | Arriba (y 70 a 470): **tarjeta de la ficha extendida** (reglas 8 y 9). Al medio (y 520 a 620): **contador de tramos rotos del castillo izquierdo** (regla 16). Abajo: **Deshacer** |
    | Margen derecho (144 px) | Al medio (y 520 a 620): **contador de tramos rotos del castillo derecho**. Abajo: **contador de piezas movidas** y, debajo, **Avanzar** |
    | Sobre el tablero | Fichas, banderines, leyendas centrales (regla 19) y los recuadros de #10 y #17 |
16. **Contador de tramos rotos (pedido del Castillo).** Uno por castillo, **siempre visible** para los dos jugadores, en el margen del lado de su castillo (izquierdo el del jugador 0, derecho el del jugador 1) y a la altura del bloque del castillo (filas 5 a 9): ícono de **grieta** y el número `tramosRotos` (0, 1, 2…). En **1** muestra un renglón de aviso; al pasar a **2** muestra una sola vez el aviso `Sin reclutamiento` y deja el contador con borde grueso. Nunca solo color (Castillo, Visual/Audio). El valor es `tramosRotos` de Castillo, no una cuenta del HUD.
17. **Íconos nuevos que este GDD le pide al art bible** (grilla de 24 px, un color, trazo grueso; art bible §7): **movimiento** (huella), **alcance** (corchetes de esquina, igual que la marca del tablero), **área** (anillo doble, igual que la marca de la catapulta), **cobertura** (eslabón), **banderín**, **grieta** (la misma de ROTO) y **bandera blanca** (Rendirse); más el de **muro** (bloque) para la tarjeta del tramo. Los íconos de **infinito** y **dado** son de la interfaz. Hasta que existan se usan marcadores de texto en los builds de desarrollo.
18. **Zona de avisos (define el lugar fijo que piden Localización y Entrada).** Los avisos de rechazo de Entrada y cualquier otro aviso de texto que no tenga un lugar propio (`Sin reclutamiento` vive en su contador, regla 16; `Tiempo`, junto al anillo, regla 12) se muestran en **un solo lugar fijo: la banda superior, a la derecha del timer (x 1170 a 1840)**, a una o dos líneas de 16 px, con ícono además del texto y sin usar el rojo como único indicador. Un aviso nuevo **reemplaza** al anterior; la duración la fija Entrada (≤ `presupuesto_aviso` de 60 caracteres, Localización). Esto **reemplaza** la franja «abajo y al centro del tablero» que propone Entrada, que tapaba casillas (Open Questions 4).
19. **Leyendas centrales de la Máquina.** Los eventos `FaseSaltada`, `ReclutaPerdido` y el recluta automático se muestran como **una sola placa al centro del tablero con hasta 3 líneas** (en orden de llegada), durante `leyenda_ms` (1.000 ms) y no más de 1,5 s en total por cascada (Máquina, Visual/Audio). Líneas: `Sin movimientos` o `Sin objetivos` (`FaseSaltada`), `Sin reclutas disponibles` (`ReclutaPerdido`). El recluta automático no usa placa: la pieza reclutada lleva una marca breve `Elegido por el tiempo` sobre su ficha, ≤ 1 s. Ninguna leyenda suena a error (Máquina, Audio). No suman tiempo al turno ni frenan los relojes.
20. **Qué eventos llevan texto (cierra la pregunta de Localización).** Llevan texto: el nombre del tipo (ficha extendida), el nombre de la cara (etiqueta, #17), el banner del turno, `Tramo final`, `Tiempo`, las leyendas de la regla 19, `Sin reclutamiento`, `Sin límite`, `Esperando la tirada`, `Esperando al servidor`, `Pausa`, los rechazos de Entrada y el motivo del fin. **No llevan texto** (solo forma, número o ícono): cada número de la ficha, el subrayado, la atenuación, el banderín, el contador de tramos rotos (ícono y número) y los relojes. Todo texto sale de Localización por clave `ui.hud_*`, y se guarda la clave y no el texto, para que el cambio de idioma sea instantáneo (Localización, regla 12).
21. **Botón ES/EN.** Vive en el **margen derecho, arriba (y 70 a 130)**, debajo del «?»; muestra los dos idiomas y marca el activo con forma (Localización, UI Requirements); el cambio no pausa timers ni toca la selección. Hasta que Ajustes (#20) lo absorba (Vertical Slice), es el único control de idioma durante la partida.
22. **Fin de partida: datos que entrega el HUD.** El HUD entrega a la pantalla de resultado (#21) el **motivo** (`fin.*`) y, para `RelojAgotado`, **los dos relojes** del resumen; el diseño de la pantalla (Victoria, Derrota, revancha, repetición) es de #21 y de la sección 2 del art bible. *PROVISIONAL hasta que exista #21.*

### States and Transitions

Los controles y la ficha derivan su estado de las fuentes de la regla 1; no hay estado de juego propio. Estado de cada elemento:

| Elemento | Estado | Condición (derivada) | Qué se ve |
|---|---|---|---|
| Avanzar | **Activo** | Turno del jugador local, fase `Mover` o `Atacar`, fuera del bloqueo | Botón latón `#C9A55A` con texto: `Atacar` o `Terminar turno` |
| | **Bloqueado** | Pasaron menos de 300 ms desde el último cambio de fase o toque aceptado | Aspecto de deshabilitado; los toques se ignoran |
| | **Deshabilitado** | `EsperandoTirada`, `Reclutar`, turno rival, `Terminada` | Gris sin acento; no responde |
| Deshacer | **Activo** | Fase `Mover` propia con la pila no vacía | Botón con texto e ícono |
| | **Desactivado** | Cualquier otro caso | Gris |
| Rendirse | **Visible** | Jugador del turno con `mias × 2 ≤ rival` | Botón con bandera blanca y texto |
| | **Oculto** | Cualquier otro caso | No ocupa lugar ni deja hueco |
| Contador de movidas | **Abierto** | `Mover`, `piezasMovidas < límite` | `5/12` |
| | **Lleno** | `piezasMovidas = límite` | `12/12` con borde grueso |
| | **Sin tope** | Arremetida | `5` con infinito |
| | **Oculto** | Fuera de `Mover` | — |
| Anillo de etapa | **Corriendo** | Hay etapa y no hay pausa ni espera | Se cierra de forma continua; número `mostrado_s` |
| | **Aviso (ámbar)** | `AvisoEtapa` y `vinculante` = `Etapa` | Ámbar, trazo grueso y pulso de 1 por segundo |
| | **Aviso (forma)** | `AvisoEtapa` y `vinculante` = `Reloj` | Borde grueso y número en negrita, sin ámbar |
| | **Quieto** | `EsperandoTirada`, pausa, o espera del servidor | Vacío, con ícono y texto |
| | **Sin límite** | Modo Sin límite | Marca `Sin límite` |
| Reloj de partida *j* | **Normal** | `nivelAvisoReloj[j] = 0` | `m:ss` |
| | **Aviso (ámbar)** | `j` activo, `vinculante` = `Reloj`, nivel 1 (fijo) o 2 (con pulso) | Ámbar |
| | **Aviso (forma)** | Nivel ≥ 1 y no limita, `j` no activo con nivel ≥ 1, o `j` activo que limita con nivel 0 (F3) | Borde grueso y número en negrita |
| | **Quieto** | `EsperandoTirada`, pausa o espera | Sin subrayado de activo ni pulso |
| Ficha compacta | **Normal** | Pieza viva | Placa Tinta con colores semánticos |
| | **Atenuada** | Propia, `Reclutar` o `Mover`, `usados > 0` | Números e íconos en gris apagado y marco hueso de 1 px |
| | **Cedida** | Objetivo de una asignación en `Atacar` | Se ve la vista previa (#16) |
| | **Simple** | Opción activa | Solo renglón A |
| Ficha extendida | **Cerrada** | Sin selección ni inspección | No ocupa lugar |
| | **Abierta** | Pieza elegida o en inspección, o tramo en inspección | Tarjeta del margen izquierdo |

Transiciones: toda variación visual dura **≤ 0,25 s con frenado suave** (art bible §7), salvo el banner de `Tramo final` (1 s, una sola vez). Los números de vida cambian **de una vez al resolver**, sin contar hacia abajo (art bible §7, pilar 4). Un cambio de fase, de turno o de idioma **no reinicia** el bloqueo de Avanzar ni el tiempo restante de un aviso en pantalla.

### Interactions with Other Systems

| Sistema | Qué entra al HUD | Qué sale del HUD | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | `EffectiveStats`, vida, tipo, casilla, dueño, `enMuro`, `tramosRotos`, `Cubre`/`CubiertoPor`, condición de `Rendirse` (regla 3) | Nada | Librería (contrato de consultas) |
| #2 Catálogo | Cifras base (para la base entre paréntesis) y `vida` máxima; ID de tipo | Nada | Catálogo (requisito: mostrar la efectiva y, si difiere, la base) |
| #3 Máquina de turno | Fase, etapa, jugador y número del turno, estado de la pila, eventos (`TurnoIniciado`, `FaseSaltada`, `ReclutaPerdido`, recluta automático) | Nada | Máquina (qué se emite); #15 (cómo se ve) |
| #4 Tablero | Dimensiones, lado del castillo de cada jugador | Nada | Tablero |
| #5 Castillo y muros | `tramosRotos`, vida de cada tramo, `enMuro` | Contador de tramos rotos y tarjeta del tramo | Castillo (datos); #15 (lugar y forma) |
| #6 Movimiento | `piezasMovidas`, límite, `usados` | Contador y atenuación | Movimiento (datos); #15 (marca) |
| #7 Alcance y asignación | Alcance efectivo (vía `EffectiveStats`); eventos | Fila de alcance | Alcance |
| #8 Cobertura | `Cubre`, `CubiertoPor` | Renglones de estado de la ficha extendida | Cobertura (datos); #15 (texto) |
| #10 Reclutamiento | Evento de recluta automático; contadores (solo para #17) | Marca `Elegido por el tiempo` | Reclutamiento |
| #11 Modificador | Cara del turno, bonos activos (para el banderín) | Banderín y subrayado | Modificador (datos); #15 (marca) |
| #12 Timer | Vista de relojes y eventos | Anillo, relojes, avisos | Timer (qué números y cuándo avisa); #15 (cómo se ve) |
| #13 Entrada | Pieza elegida o en inspección; estado del gesto | Estado de cada control (activo, bloqueado, desactivado) y lugar de los avisos | Entrada (qué hace cada botón); #15 (cómo se ve y en qué estado queda) |
| #14 Localización | Textos por clave `ui.hud_*`; aviso de cambio de idioma | Claves y parámetros | Localización (texto); #15 (claves `ui`) |
| #16 Vista previa | Reemplaza la ficha del objetivo en `Atacar` | El ancla de la ficha, cedida | #16 (contenido); #15 (ancla) |
| #17 Interfaz del Modificador | Etiqueta de la cara y «?» en la banda superior | El lugar de cada uno | #17 (contenido); #15 (lugar) |
| #18 Render | Posición en pantalla de cada casilla y de las figuras | La posición de la ficha, anclada a la casilla | Render (dibujo de las figuras); #15 (la ficha, capa del sistema) |
| #19 Métricas, #20 Ajustes, #21 Menú, #23 Accesibilidad, #24 Tutorial | — | Uso de Deshacer y de Rendirse (eventos), opción Ficha simple, datos del resumen, la ficha como material de la vista de estadísticas | Cada GDD (todos PROVISIONALES: aún sin GDD) |

**Provisional:** #16, #18, #19, #20, #21, #23 y #24 todavía no tienen GDD; #9 y #17 se escribieron en paralelo y no se verificaron contra este documento. Lo que este documento espera de ellos queda en Dependencies y en Open Questions.

## Formulas

*Lente `systems-designer` (modo `full`, sin agente): cada fórmula se probó con cada variable en su mínimo, su máximo y en 0. Este sistema no tiene bucles de retroalimentación: es una capa de lectura sin estado de juego, así que ninguna fórmula alimenta a otra del núcleo. Las cifras de piezas, relojes y topes salen del registro (`design/registry/entities.yaml`); los valores propios de este GDD se proponen para registrar (ver Tuning Knobs).*

**Supuestos sin fuente (obligación de no afirmar lo que no se puede fuentear).** Las medidas de la fuente tipográfica (ancho de una cifra de 14 px) **no están cubiertas por** `design/art/art-bible.md` ni por `docs/engine-reference/unity/`: el art bible nombra la familia (tipo Barlow Condensed o Roboto Condensed) pero no la elige. Se usa 7 px por cifra como **supuesto marcado `NOT SOURCEABLE`** y se verifica con la fuente elegida antes de cerrar F1 (AC-3).

### F1. Dimensión de la ficha compacta

La fórmula `ancho_ficha` se define así:

`ancho_renglon_A = icono_ficha_px + gap_icono_px + d_vida × ancho_digito_px`
`ancho_renglon_B = icono_ficha_px + gap_icono_px + d_ataque × ancho_digito_px + gap_grupo_px + icono_ficha_px + gap_icono_px + d_escudo × ancho_digito_px`
`ancho_ficha = max(ancho_renglon_A, ancho_renglon_B) + 2 × margen_placa_px`
`alto_ficha = 2 × alto_renglon_px + gap_renglon_px + 2 × margen_placa_px`
`fraccion_alto = alto_ficha ÷ casilla_px`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| icono_ficha_px | int | 12 | constant (propuesta de este GDD) | Lado de los íconos del interior de la ficha (puño, escudo, tipo); versión reducida de la grilla de 24 px |
| gap_icono_px | int | 2 | constant | Espacio entre un ícono y su número |
| gap_grupo_px | int | 4 | constant | Espacio entre el grupo de ataque y el de escudo |
| ancho_digito_px | int | 7 | constant (**NOT SOURCEABLE**, supuesto de fuente condensada de 14 px; AC-3) | Ancho de una cifra de ancho fijo |
| d_vida, d_ataque, d_escudo | int | 1–2 | calculated (cantidad de cifras del valor mostrado) | Cifras de cada número; AC-4 garantiza 2 como máximo |
| margen_placa_px | int | 2 | constant | Aire entre el contenido y el borde de la placa |
| alto_renglon_px | int | 16 | constant | Altura de un renglón de 14 px con borde |
| gap_renglon_px | int | 2 | constant | Espacio entre los dos renglones |
| casilla_px | int | 68 | data file (Tablero y art bible §7, a 1080p) | Lado de una casilla |

**Output Range:** `ancho_ficha` de 50 a **64 px** (peor caso, 2 cifras en los tres números), `alto_ficha` = **38 px** (con «Ficha simple», un solo renglón: 20 px). Límite: `ancho_ficha` ≤ 64 px y `alto_ficha` ≤ 40 px (regla 3); así la placa nunca invade la casilla vecina.
**Example:** demoledor (vida 10, ataque 30, escudo 1): renglón A = 12 + 2 + 2×7 = 28; renglón B = 12 + 2 + 14 + 4 + 12 + 2 + 7 = 53; `ancho_ficha` = 53 + 4 = **57 px**. Rey con Grito (vida 30, ataque 10, escudo 5): B = 12 + 2 + 14 + 4 + 12 + 2 + 7 = 53 → 57 px. Peor caso teórico (99/99/99): B = 12 + 2 + 14 + 4 + 12 + 2 + 14 = 60 → **64 px**. `alto_ficha` = 2×16 + 2 + 4 = **38**; `fraccion_alto` = 38 ÷ 68 = **0,56** (Ficha simple: 20 ÷ 68 = 0,29).

*Resultado de los límites:* con 3 cifras en un número, `ancho_ficha` pasa de 64 a **71 px**, mayor que la casilla (68): el diseño no soporta cifras de 100 o más. Esto **no se corrige en silencio**: se acota con el test AC-4 sobre el Catálogo (todas las cifras y su bono máximo ≤ 99) y queda como restricción para el Catálogo (Dependencies). Alternativas si algún día hace falta: (a) subir `casilla_px` con el zoom móvil (#41), (b) abreviar a `99+` (esconde información: contra el pilar 1), (c) pasar a una ficha de tres renglones. **Tapado de la figura:** si la figura mide el 75% de la casilla (51 px) y está centrada, la ficha completa tapa **31 px de 51 (61%)** de su altura y la Ficha simple **13 px (26%)**: es el costo ya aceptado en el art bible (§7, conflicto «lugar de la ficha contra tamaño de la figura»); se mide en playtest (Open Questions 5).

### F2. Formato de los relojes

La fórmula `texto_reloj` se define así:

`minutos = mostrado_s ÷ 60  (división entera)`
`segundos = mostrado_s mod 60`
`texto_reloj = minutos + ":" + segundos con dos cifras`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| mostrado_s | int | 0–750 | calculated (Timer, D4: `⌈restante_ms ÷ 1000⌉`) | Entero que ya redondeó el Timer; el HUD no redondea |
| minutos | int | 0–12 | calculated | Parte entera |
| segundos | int | 0–59 | calculated | Resto |

**Output Range:** `0:00` a `12:30` (`reloj_jugador_s` = 750). `0:00` aparece **solo** si el reloj venció (Timer, D4). El número del anillo es `mostrado_s` sin formato (0 a 60).
**Example:** `mostrado_s` = 61 → `1:01`; 750 → `12:30`; 59 → `0:59`; 60 → `1:00`; 0 → `0:00` (venció). Cota: `reloj_jugador_s` hasta 5.999 cabe en el formato `m:ss` de 4 caracteres; a partir de 6.000 el formato pasaría a 3 cifras de minutos y se rediseña. El Timer lo fija en 750, así que no se alcanza.

### F3. Estado de aviso de los temporizadores

La fórmula `estado_de_aviso` se define así:

`corriendo = ¬pausado ∧ ¬enEsperaDeTirada`
`activo_j = (j = jugadorActivo) ∧ corriendo`
`ámbar_anillo = AvisoEtapa ∧ (vinculante = Etapa) ∧ corriendo`
`ámbar_reloj[j] = activo_j ∧ (vinculante = Reloj) ∧ (nivelAvisoReloj[j] ≥ 1)`
`forma_anillo = AvisoEtapa ∧ ¬ámbar_anillo`
`forma_reloj[j] = ((nivelAvisoReloj[j] ≥ 1) ∨ (activo_j ∧ vinculante = Reloj)) ∧ ¬ámbar_reloj[j]`
`pulso = (ámbar_anillo) ∨ (ámbar_reloj[j] ∧ nivelAvisoReloj[j] = 2)`, apagado con «Reducir efectos»

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| jugadorActivo | int | 0–1 | calculated (vista de relojes del Timer) | Jugador del turno |
| vinculante | enum | `Etapa`, `Reloj`, ninguno | calculated (Timer, regla 13) | Qué vence antes; ninguno en Sin límite |
| nivelAvisoReloj[j] | int | 0–2 | calculated (Timer, D4) | Nivel de aviso del reloj de `j` (60 s y 10 s: `aviso_reloj_s`, `aviso_reloj_critico_s`) |
| AvisoEtapa | bool | 0–1 | calculated (Timer, D4; `aviso_etapa_s` = 10) | La etapa llegó a sus últimos 10 s |
| pausado, enEsperaDeTirada | bool | 0–1 | calculated (vista de relojes) | Reloj detenido |

**Output Range:** como máximo **un** elemento en ámbar por cuadro: `ámbar_anillo` y `ámbar_reloj[j]` no pueden valer a la vez porque `vinculante` tiene un solo valor. Pulsos: como mucho **1 por segundo** (los dos pulsos posibles son excluyentes), dentro del presupuesto de destellos del art bible (< 3 por segundo).
**Example:** etapa de mover con 8 s restantes y reloj del jugador activo en 3:20: `vinculante` = `Etapa` → anillo ámbar con pulso; reloj en `forma` si su nivel es ≥ 1, si no, normal. Misma etapa, reloj en 0:07 (nivel 2): `vinculante` = `Reloj` → **reloj ámbar con pulso**, anillo en `forma` (tiene `AvisoEtapa`, no es el que limita). Reloj del rival en 0:40 (nivel 1): `forma`, nunca ámbar. Sin límite: ningún elemento de aviso.

*Resultado de los límites:* (a) **0:** con `nivelAvisoReloj` en 0 y `vinculante` = `Reloj` (solo posible si se sube el umbral `aviso_reloj_s` por debajo de la duración de etapa, hoy 60 contra etapas de 60, 40 y 20 s) el reloj que limita no tendría aviso: la rama `forma_reloj[j]` del segundo término lo marca igual con borde grueso, para que el jugador vea qué lo limita (Timer, UI Requirements). (b) **Pausa o espera:** `activo_j` = falso apaga ámbar y pulso, y la forma se mantiene. (c) **Invariante verificada:** con los valores vigentes, `vinculante` = `Reloj` implica `restanteReloj ≤ restanteEtapa ≤ 60 = aviso_reloj_s` (con empate, el Timer da prioridad al reloj), o sea `nivelAvisoReloj ≥ 1`: nunca hay un reloj limitando en silencio. Si Timer sube `aviso_reloj_s`, la invariante se conserva; si lo baja de 60, vale la rama (a).

### F4. Bloqueo del botón Avanzar

La fórmula `avance_aceptado` se define así:

`t_ultimo = max(t_cambio_de_fase, t_ultimo_toque_aceptado)`
`avance_aceptado(t) = habilitado ∧ (t − t_ultimo ≥ bloqueo_avance_ms)`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| t | int (ms) | ≥ 0 | calculated (reloj monotónico del cliente) | Instante del toque |
| t_cambio_de_fase | int (ms) | ≥ 0 | calculated (evento `FaseIniciada`) | Cuándo abrió la fase actual |
| t_ultimo_toque_aceptado | int (ms) | ≥ 0 o −∞ | calculated | Último toque que mandó un comando |
| bloqueo_avance_ms | int | 200–600 (valor 300) | data file (config de la interfaz; **perilla de este GDD**) | Ventana en la que Avanzar ignora toques |
| habilitado | bool | 0–1 | calculated (State: Activo) | Turno del jugador local, fase `Mover` o `Atacar` |

**Output Range:** 0 o 1 toque aceptado por ventana. El bloqueo **solo retrasa un toque que llega dentro de la ventana** (como mucho `bloqueo_avance_ms` = 0,3 s por cambio de fase, dos por turno): quien toca una vez y piensa después no pierde nada, así que el costo normal para el pilar 4 es 0 s y el peor caso, de 0,6 s por turno, es el de un toque apurado que se repite. El timer no se frena durante el bloqueo (corre igual).
**Example:** toque en t = 0 cierra `Mover` (manda `TerminarMovimiento`, `t_cambio_de_fase` = 0). Un segundo toque en t = 150 ms: 150 − 0 < 300 → **ignorado**. Un tercero en t = 320 ms: 320 − 0 ≥ 300 → aceptado, manda `Resolver`. *Borde:* con `bloqueo_avance_ms` = 0 no hay bloqueo (el doble clic cierra el turno); en el primer toque de la partida `t_ultimo` = −∞ y se acepta.

*Resultado de los límites:* por debajo de 200 ms el bloqueo no cubre el doble clic típico (100 a 250 ms entre clics); por encima de 600 ms un jugador rápido que asignó todo antes de que termine la fase de mover (o una Distracción que abre `Atacar`) siente el botón «pegado». Se calibra en playtest (Open Questions 6).

### F5. Legibilidad de la ficha y de su estado atenuado

La fórmula `legibilidad_ficha` se define así:

`Y_placa(α, suelo) = α × Y_tinta + (1 − α) × Y_suelo`
`contraste(Y_a, Y_b) = (max(Y_a, Y_b) + 0,05) ÷ (min(Y_a, Y_b) + 0,05)`   (WCAG)
`C1: contraste(Y_numero, Y_tinta) ≥ 4,5`   (un número contra su propio borde Tinta)
`C2: contraste(Y_icono, Y_placa(α, suelo)) ≥ 3`   (un ícono contra la placa que tiene detrás, sobre cada suelo)
`L*(Y) = 116 × Y^(1/3) − 16`   (solo para medir el cambio de la alternativa descartada)

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| α | float | 0,85–1,00 (valor 0,96) | data file (`ficha_opacidad_pct` = 96, **propuesta**; el art bible §4 pide «Tinta al 85-90%») | Opacidad de la placa de la ficha |
| Y_tinta | float | 0,0108 | constant (luminancia de `#1E1A17`) | Tinta |
| Y_suelo | float | 0,027–0,780 | constant (Ciruela `#3B2350` 0,027 · Tierra `#6E6150` 0,124 · Hueso `#EFE4C8` 0,780) | Suelo bajo la ficha |
| Y_numero, Y_icono | float | 0,243–0,575 | constant (vida `#8EDB6A` 0,575 · ataque `#E55A40` 0,243 · escudo `#7F9BB8` 0,314 · gris apagado `#A8A49B` 0,372) | Luminancia del elemento |

**Output Range:** contrastes de 1,9 a 10,3; el criterio es C1 ≥ 4,5 para los números y C2 ≥ 3 para los íconos, sobre los tres suelos.
**Example (cálculo del 2026-11-25 con la fórmula sRGB de la sección 4 del art bible):**

C1, números contra su borde Tinta (no depende de la placa ni del suelo):

| Elemento | Normal | Atenuado (gris `#A8A49B`) |
|---|---|---|
| Vida | 10,3 | 6,9 |
| Ataque | 4,8 | 6,9 |
| Escudo | 6,0 | 6,9 |

C2, íconos contra la placa, con la opacidad del art bible (88%) y con la propuesta (96%):

| Ícono | Hueso, 88% | Hueso, 96% | Tierra, 96% | Ciruela, 96% |
|---|---|---|---|---|
| Vida (tipo) | 4,1 | 6,8 | 9,6 | 10,2 |
| Ataque (puño) | **1,9** | **3,2** | 4,5 | 4,8 |
| Escudo | **2,4** | 4,0 | 5,6 | 5,9 |
| Gris apagado | **2,8** | 4,6 | 6,5 | 6,9 |

*Resultados de los límites (dos problemas encontrados):*

1. **Con el 88% del art bible, el puño (1,9) y el escudo (2,4) fallan C2 sobre el suelo Hueso** (también el gris apagado, 2,8): la placa translúcida deja pasar la luz del suelo claro. El mínimo que cumple C2 para el puño es **α = 0,96** (con 0,95 da 2,95). Por eso `ficha_opacidad_pct` se propone en 96; es un cambio al art bible (Open Questions 7). Alternativa si se prefiere conservar la translucidez: borde Tinta de 1 px en los íconos, como ya llevan los números (pero cada ícono pasa a 14 px de caja y la ficha de 64 a 68 px de ancho, F1).
2. **Atenuar bajando solo la opacidad de la placa** (la opción que eligió el jugador) **no se ve del lado oscuro.** Pasando de α = 0,88 a 0,45, la luminosidad de la placa cambia: Hueso de L\* 38 a 72 (+33), Tierra de 18 a 33 (+15) y **Ciruela de 11 a 15 (+4)**; con α = 0 llegaría a L\* 19, así que **ningún valor de α alcanza un cambio de 10 puntos sobre Ciruela**. Y con α baja los íconos de la ficha ya no se leen sobre Hueso (C2 de 1,3 con α = 0,45). Por eso la atenuación de la regla 5 **apaga el color a gris y suma un marco interior de 1 px `#F5F1E8`**, con la placa en su opacidad normal: se ve igual sobre los tres suelos, los números siguen en 6,9 contra Tinta y los íconos en 4,6 o más con α = 0,96. El marco va **a 1 px dentro del borde** de la placa (rodeado de Tinta), no en el borde: un marco claro en el borde se perdería sobre el suelo Hueso. Se valida con AC-9 en los dos bandos, en grises y con los tres filtros de daltonismo.

### F6. Marca de una cifra que difiere de la base

La fórmula `marca_de_cifra` se define así:

`delta = efectiva − base`
`subrayado = (delta ≠ 0)`
`texto_base = "(" + base + ")"  si  delta ≠ 0,  vacío si no`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| efectiva | int | 1–99 | calculated (`EffectiveStats`; Librería regla 10) | Cifra que se muestra |
| base | int | 1–99 | data file (Catálogo, `vida`, `ataque`, `escudo`, `movimiento`, `alcanceMin`, `alcanceMax`, `danoArea`) | Cifra de la tabla |
| delta | int | −98 a 98 | calculated | Diferencia |

**Output Range:** `delta` hoy entre 0 y +10 (Grito +2, muro +2 de escudo y de alcance, `bono_grito_*`; el bono de turno de la Librería va de 0 a 10). Un `delta` negativo no existe hoy; si apareciera, se subraya igual: el subrayado dice «difiere de la base», no «sube».
**Example:** arquero con Grito, ataque base 6: efectiva 8, `delta` = 2 → ficha `8` subrayado; tarjeta `Ataque 8 (6)`. Arquero en el muro: escudo 2 → 4 (`4 (2)`), alcance 1–4 → 1–6 (`1–6 (1–4)`). Catapulta con Grito: ataque `12 (10)`, área `6` **sin marca** (delta 0; Modificador, regla 7). Demoledor con Grito: ataque `30` sin marca, movimiento `8 (6)`. Soldado sin bonos: sin marcas.

### F7. Alto de la tarjeta de la ficha extendida

La fórmula `alto_tarjeta` se define así:

`alto_tarjeta = alto_cabecera_px + n_filas × alto_fila_px + n_lineas_estado × alto_linea_px + 2 × margen_tarjeta_px`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| alto_cabecera_px | int | 28 | constant | Fila del tipo (ícono y nombre) |
| n_filas | int | 5–6 | calculated | Vida, ataque, escudo, movimiento y alcance; más área si el tipo la tiene (la fila de alcance falta en el demoledor, que sí tiene área) |
| alto_fila_px | int | 24 | constant | Una fila de valores (texto de 16 px) |
| n_lineas_estado | int | 0–6 | calculated | Renglones de estado: hasta 3 estados, cada uno puede envolver a 2 líneas con el texto inflado +40% |
| alto_linea_px | int | 20 | constant | Una línea de texto de estado |
| margen_tarjeta_px | int | 8 | constant | Aire interno |

**Output Range:** de `28 + 5×24 + 0 + 16` = **164 px** (sin estados) a `28 + 6×24 + 6×20 + 16` = **308 px** (peor caso). Límite: ≤ **400 px** (y 70 a 470 del margen izquierdo, regla 15).
**Example:** catapulta elegida en `Mover` con movimiento restante: 6 filas (vida, ataque, escudo, movimiento, alcance, área), un estado (`Ya movida`): 28 + 144 + 20 + 16 = **208 px**. Arquero en el muro, cubierto por un escudero: 5 filas, estados `En el muro (22/30)` y `Cubierta por Escudero`, de 1 línea en español y 2 en inglés (+40%): 28 + 120 + 60 + 16 = **224 px**.
*Resultado de los límites:* el ancho útil de la tarjeta es 128 px (144 − 2×8): una fila de valores de hasta 10 caracteres (`12/12 (12)`) usa ≈ 20 + 4 + 80 = 104 px; el texto de estado envuelve y nunca se corta (Localización, F5).

## Edge Cases

*Lente `systems-designer` (modo `full`, sin agente) sobre las secciones C y D releídas del archivo. Se incluyen todos los casos encontrados; los que dependen de un GDD sin escribir están marcados PROVISIONAL.*

**Cifras y ficha**

- **If una cifra mostrada tiene 3 cifras o más** (el Catálogo o un bono pasó de 99, contra AC-4): la ficha muestra el valor real, **nunca lo trunca ni lo abrevia**; la placa crece lo necesario y queda centrada (F1: 71 px con un número de tres cifras, 85 px con los tres), invadiendo la casilla vecina y el cliente registra un error visible en el build de desarrollo. El test AC-4 impide publicar con esa tabla.
- **If una pieza sobrevive a la resolución con menos vida:** el número de vida cambia de una vez, en el cuadro del destello de resolución, sin contar hacia abajo (art bible §7).
- **If una pieza muere en la resolución:** su ficha desaparece en el mismo cuadro que su figura, también si estaba elegida o en inspección (la tarjeta se va con la selección; Entrada, Edge Cases). Si era objetivo de una asignación, la vista previa deja su lugar a la placa MUERE (#16).
- **If `Deshacer` devuelve una pieza a `usados = 0`:** la ficha recupera sus colores y pierde el marco en el mismo cuadro. **If la pieza se había movido dos veces y se deshace solo la última:** sigue atenuada (`usados > 0`; Movimiento, regla 10).
- **If la pieza es el Rey y `usados > 0`:** su ficha se atenúa como la de cualquiera, pero **no suma** al contador de piezas movidas (Movimiento, regla 8).
- **If una pieza es reclutada (también por el recluta automático):** aparece con la ficha normal (`usados = 0`). El recluta automático suma la marca `Elegido por el tiempo` sobre su ficha, ≤ 1 s.
- **If un arquero sube o baja del muro:** el escudo y el alcance de su ficha (y el subrayado y el renglón `En el muro`) cambian **en el mismo cuadro en que se acepta el `Mover`** (Castillo, Game Feel).
- **If el tramo del muro donde está un arquero se rompe:** en la resolución el renglón `En el muro` desaparece y escudo y alcance vuelven a la base en el mismo cuadro (Castillo, regla 6: el arquero queda en el hueco).
- **If cambia el turno y el Grito de guerra caduca:** los banderines y los subrayados de bono se recalculan en el cuadro de `TurnoIniciado`, no antes ni después.
- **If hay Grito y la pieza es un demoledor:** el banderín está, el ataque **no** se subraya (30 sin cambio) y el movimiento de la tarjeta muestra `8 (6)` (Modificador, regla 7).
- **If el jugador activa «Ficha simple» a mitad del turno:** la ficha pasa a un renglón en el cuadro siguiente; la selección, las flechas y los timers no cambian.

**Tarjeta extendida**

- **If el jugador elige una pieza rival (o una propia sin comandos habilitados):** la tarjeta muestra las mismas filas con los valores efectivos y la base; **no** muestra `restante` de movimiento ni `Ya movida` (son de una pieza propia en `Mover`).
- **If no hay selección ni inspección:** la tarjeta no ocupa lugar (no deja un hueco).
- **If el idioma cambia con la tarjeta abierta:** los textos se vuelven a pedir en el cuadro siguiente; la tarjeta no se cierra y la selección no se toca (Localización, regla 11).
- **If un texto de estado traducido (+40%, Localización F5) pasa de dos líneas:** envuelve hasta tres líneas; si el total de la tarjeta pasaría de 400 px (F7), el exceso se recorta con `…` **y se registra el error** (nunca un recorte silencioso); la prueba de texto inflado de Localización lo detecta antes de publicar.
- **If se inspecciona un tramo en pie sin pieza:** la tarjeta muestra el ícono de muro, `Tramo de muro` y `vida / 30`. **If el tramo está roto:** es una casilla vacía, no tiene tarjeta (Entrada, regla 3). Entrada lo admite (regla 3, cierre de la tanda 3).
- **If la pieza elegida es el demoledor:** la fila de alcance no se muestra (sus alcances son inertes; Catálogo) y la de área muestra la explosión, igual a su ataque efectivo sin bono (30).

**Tiempo y botones**

- **If la etapa y el reloj vencen en el mismo instante:** `vinculante` = `Reloj` (el Timer da prioridad al reloj, regla 8): el ámbar va al reloj.
- **If `vinculante` cambia de `Etapa` a `Reloj` (o al revés) a mitad de un aviso:** el ámbar y el pulso pasan al otro elemento **en el mismo cuadro**; nunca hay dos en ámbar (F3).
- **If hay pausa o espera durante un aviso:** el ámbar y el pulso se apagan, el número queda quieto y el borde grueso se mantiene (F3).
- **If el reloj del rival está en aviso mientras juega el otro:** se marca con forma (borde grueso), nunca con ámbar.
- **If el modo es Sin límite:** no hay anillo ni relojes y los avisos de tiempo no existen; el contador de piezas movidas, los botones y el resto del HUD se muestran igual.
- **If «Reducir efectos» está activo:** no hay pulso ni destello; el borde grueso y el número en negrita se mantienen. Ningún aviso depende solo del pulso.
- **If el jugador toca Avanzar durante el bloqueo de 300 ms:** el toque se ignora sin aviso y sin sonido; no se acumula para después (F4).
- **If cambia la fase mientras un toque de Avanzar está pulsado y se suelta después:** cuenta como toque nuevo para F4 solo si se suelta con el botón habilitado; Entrada, regla 3, ya descarta una pulsación que empieza fuera de su control.
- **If la Distracción salta la fase de mover:** el contador de piezas movidas no se muestra, Avanzar pasa directamente a `Terminar turno` cuando termina la placa (≤ 1,5 s) y el banner `Sin movimientos` no aparece (no es una fase saltada por falta de movimientos: la saltó la cara).
- **If `FaseSaltada` y `ReclutaPerdido` ocurren en la misma cascada:** una sola placa con hasta tres líneas, en orden de llegada, ≤ 1,5 s en total; nunca tres placas en serie (regla 19).
- **If el primer turno de la partida no tira el Modificador:** la etiqueta de la cara muestra «Apertura: sin dado» (`ui.modif_etiqueta_apertura`, #17 regla 5), sin ícono, y no deja un hueco.
- **If el jugador recluta y deja de cumplir `mias × 2 ≤ rival`:** el botón Rendirse desaparece en el cuadro del reclutamiento. **If el turno pasa por tiempo con el diálogo de confirmación abierto:** el diálogo se cierra y no se manda `Rendirse`; el botón se reevalúa con la condición nueva.
- **If hay 12 o más piezas movidas (cupo lleno):** el contador pasa a borde grueso y el rechazo de un `Mover` sobre una pieza sin mover sale por la zona de avisos (Entrada); las piezas ya movidas siguen moviéndose.

**Pantalla, idioma y partida**

- **If la partida termina (`Terminada`):** los relojes se congelan; Avanzar, Deshacer y Rendirse se deshabilitan u ocultan; la tarjeta se cierra; la pantalla de resultado (#21) toma el centro y recibe el motivo y, para `RelojAgotado`, los dos relojes. PROVISIONAL: #21.
- **If es el turno del rival (online):** Avanzar y Deshacer están deshabilitados, la inspección sigue activa y los dos relojes se ven (el del rival corriendo, en ámbar si limita: lo ven los dos jugadores).
- **If la ventana efectiva es menor que 1080p:** el cliente muestra una sola vez el aviso `Para jugar bien necesitás al menos 1080p` (`ui.hud_resolucion_baja`) y escala todo el HUD de forma uniforme; los números pueden bajar de 14 px y AC-1 solo se garantiza a 1080p (art bible §7, decisión del jugador: mínimo 1080p). PROVISIONAL: la política de resolución es de Render (#18).
- **If Localización queda `Fallida`:** no hay partida y el HUD no se dibuja (Localización, regla 13); **si queda `Degradada`:** el HUD funciona con la tabla que cargó y el botón ES/EN se ve deshabilitado.
- **If una clave `ui.hud_*` falta en el idioma actual:** respaldo de Localización (regla 14); el HUD no escribe texto propio.
- **If el jugador tiene 122 fichas en pantalla (61 por bando):** los números se redibujan solo cuando un evento los cambia, no por cuadro (ui-programmer); el presupuesto de cuadro lo fija el ADR de UI.

## Dependencies

**Depende de** (el HUD solo lee: ninguna de estas dependencias es de escritura):

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #1 Librería de reglas | **Dura** | `EffectiveStats` y las consultas puras (vida, tipo, casilla, dueño, `enMuro`, `tramosRotos`, `Cubre`/`CubiertoPor`) y la condición de `Rendirse` (regla 3) | Existe |
| #2 Catálogo de piezas | **Dura** | Cifras base y vida máxima; ID de tipo. **Restricción que se le pide:** toda cifra y su bono máximo ≤ 99 (F1, AC-4) | Existe y ya nombra a #15 |
| #3 Máquina de turno | **Dura** | Fase, etapa, jugador y número del turno, estado de la pila, eventos (`TurnoIniciado`, `FaseSaltada`, `ReclutaPerdido`, recluta automático, `PartidaTerminada`) | Existe y ya nombra a #15 |
| #12 Timer de turno | **Dura** | Vista de relojes (regla 13) y eventos (regla 12): `mostrado_s`, `vinculante`, niveles de aviso, pausa, espera, tramo final | Existe y ya nombra a #15 |
| #11 Modificador | **Dura** | Cara del turno, bonos activos (Grito) para el banderín y el subrayado, estadística efectiva | Existe y ya nombra a #15 |
| #14 Localización | **Dura** | Textos por clave `ui.hud_*`, `pieza.*`, `fin.*`; aviso de cambio de idioma; el presupuesto de largo de cada texto | Existe y ya nombra a #15 |
| #13 Entrada y selección | Blanda | Pieza elegida o en inspección; qué comando manda cada botón; confirmación de Rendirse | Existe y ya nombra a #15 |
| #6 Movimiento | Blanda | `piezasMovidas`, límite del turno, `usados` y `restante` (contador, atenuación, tarjeta) | Existe y ya nombra a #15 |
| #5 Castillo y muros | Blanda | `tramosRotos`, vida de cada tramo, `enMuro`, estados del tramo | Existe y ya nombra a #15 |
| #7 Alcance y asignación | Blanda | Alcance efectivo (vía `EffectiveStats`) y eventos de asignación | Existe y ya nombra a #15 |
| #8 Cobertura del escudero | Blanda | `Cubre`, `CubiertoPor` para los estados de la tarjeta | Existe y ya nombra a #15 |
| #10 Reclutamiento y límites | Blanda | Evento del recluta automático (marca `Elegido por el tiempo`) | Existe y ya nombra a #15 |
| #4 Tablero | Blanda | Medidas (casilla de 68 px, márgenes) y lado de cada castillo | Existe y ya nombra a #15 |
| #9 Resolución simultánea | Blanda | Evento de cambio de vida y muertes (para el cambio de números en un cuadro) | Existe; contrato verificado al cerrar la tanda 3: `DanoRecibido` y `DanoATramo` traen `vidaAntes` y `vidaDespues`, y `PiezaMuerta` la salida (#9, regla 14) |
| `design/art/art-bible.md` §3, §4, §7 | Referencia | Formas, colores semánticos, medidas del HUD, ficha, tipografía, íconos | Existe (con tres cambios pedidos en Open Questions) |

**Dependen de este sistema:**

| Sistema | Tipo | Qué usa | Estado del GDD |
|---|---|---|---|
| #16 Vista previa de ataques | **Dura** | El ancla de la ficha (regla 10): reemplaza la ficha del objetivo en `Atacar`; el lugar de la zona de avisos y de las leyendas | Pendiente |
| #17 Interfaz del Modificador | **Dura** | Los lugares fijos de la etiqueta de la cara (izquierda del timer) y del «?» (extremo derecho) en la banda superior, y la reserva del centro del tablero | Existe (escrito en paralelo el 2026-11-25); se leyeron solo los pasajes que nombran a #15 y este GDD adopta el lugar que propone |
| #18 Render, animación y feedback | Blanda | La ficha como capa del sistema que Render debe dejar intacta; posición de cada casilla en pantalla | Pendiente |
| #23 Accesibilidad | Blanda | Escala de texto, daltonismo, tamaños táctiles de los controles, lector de pantalla de la tarjeta | Pendiente |
| #24 Tutorial básico de 3 pasos | **Dura** | La ficha y la tarjeta como la «vista de estadísticas de todas las piezas» | Pendiente |
| #38 Kit de skins con validador | Blanda | «Ficha del sistema intocable»: el validador comprueba que ningún pack toque la zona o los colores de la ficha | Pendiente |
| #41 Adaptación móvil | Blanda | Zoom o dos niveles de estadísticas; la ficha y los márgenes a otra escala | Pendiente |
| #20 Ajustes, #21 Menú | Blanda | Opción «Ficha simple» y «Reducir efectos»; el motivo del fin y los relojes para la pantalla de resultado | Pendiente |
| #19 Métricas locales | Blanda | Eventos de uso de Deshacer y de Rendirse, y de la opción «Ficha simple» (si Métricas los pide) | Pendiente |

**Bidireccionalidad (verificada con `grep` sobre los GDD el 2026-11-25).** Nombran a #15 como dependiente, con el contrato que este documento cumple: Catálogo (Dura), Máquina de turno (Blanda; este GDD la trata como Dura porque sin fase no hay botón), Timer (Dura), Modificador (Dura), Localización (Dura), Castillo (Blanda), Movimiento (Blanda), Alcance (Blanda), Reclutamiento (Blanda), Cobertura (referencia, en su Visual/Audio), Entrada (en su tabla de interacciones) y Tablero (Blanda). **La Librería (#1) los nombra como «13, 15–19, 22, 24 Cliente»**, sin fila propia para #15: queda pendiente para `/review-all-gdds`. Cada GDD pendiente de la tabla de arriba debe nombrar a #15 en su sección Dependencies al escribirse.

**Cambios pedidos fuera de este archivo** (aplicados al cerrar la tanda 3, 2026-11-25): Entrada (#13): la zona de avisos de la regla 18 reemplaza la franja «abajo y al centro»; admitir la inspección de un tramo. Art bible §2 y §7: el tramo final pasa de «anillo grueso ámbar» a anillo grueso de color base; siete íconos nuevos más el de muro; banderín; marco interior y gris apagado de la ficha atenuada; opacidad de la placa en 96% en lugar de 85–90%. Catálogo (#2): la restricción ≤ 99. Tablero (#4): usa «azul/rojo» para los sectores y el art bible usa Hueso/Ciruela para los bandos. `systems-index.md`: fila 15 pasa a `Designed`, y el Dependency Map debe sumar las aristas #15 → #1, #8, #13 y #16/#17 → #15.

## Tuning Knobs

*Lente `systems-designer` (modo `full`, sin agente): las perillas son las variables `data file` de la sección Formulas; las `constant` no son perillas. Este sistema no tiene perillas de balance: ninguna cambia lo que pega, aguanta o vive una pieza, ni cuánto dura una partida. Todas son de presentación y van en la config de la interfaz del cliente.*

| Perilla | Vigente | Rango seguro | Si es demasiado alta | Si es demasiado baja | Interacción |
|---|---|---|---|---|---|
| `ficha_opacidad_pct` (opacidad de la placa de la ficha) | **96** (confirmada por el jugador el 2026-11-25; art bible §7 corregido) | 96–100 | Con 100 la placa tapa por completo la parte de la figura que cubre (hoy el 61% de su altura, F1) | Con menos de 96 el puño pasa a contraste < 3:1 sobre el suelo Hueso (F5: 88% da 1,9; 95% da 2,95) y la ficha deja de leerse en el bando claro | Con F1 (tapado de la figura), con el color de los íconos y con el gris apagado (F5) |
| `bloqueo_avance_ms` (ventana en que Avanzar ignora toques) | **300** | 200–600 | Un jugador que ya asignó todo siente el botón «pegado»; con 600 ms o más se nota al cambiar de fase | Con menos de 200 ms un doble clic accidental (100 a 250 ms entre clics) vuelve a cerrar el turno | Con `ms_toque_objetivo` de Entrada (1.000 ms entre dos toques de objetivo): el bloqueo debe quedar muy por debajo |
| `leyenda_ms` (duración de la placa central de leyendas) | **1.000** | 600–1.000 | Más de 1.000 contradice a la Máquina (cada leyenda ≤ 1 s, cascada ≤ 1,5 s) y tapa casillas con pieza | Con menos de 600 no da tiempo de leer tres líneas | Con el tope de 1,5 s de la cascada de la Máquina; la placa es una sola, así que no se suman |
| `ficha_simple_por_defecto` (opción «Ficha simple») | **apagada** | apagada, encendida | Encendida por defecto: el jugador nuevo pierde de vista ataque y escudo (pilar 1) | — | Con #20 Ajustes (la guarda) y con #23 Accesibilidad; no es una perilla de balance sino una opción del jugador (art bible §7) |
| `reducir_efectos` (opción) | **apagada** | apagada, encendida | — | — | Apaga el pulso del timer y el destello de resolución; no toca la forma de ningún aviso |

**No son perillas** (constantes de diseño; cambiarlas obliga a rehacer F1, F5 o F7): `casilla_px` (68, de Tablero y del art bible), `icono_ficha_px` (12), `ancho_digito_px` (7, **NOT SOURCEABLE** hasta elegir la fuente, F1), `margen_placa_px`, `alto_renglon_px`, los colores semánticos y el gris apagado `#A8A49B`, el marco de 1 px, el ancho útil de la tarjeta (128 px) y su alto máximo (400 px).

**Perillas de otro GDD (no se duplican acá):** los umbrales de aviso `aviso_etapa_s` (10), `aviso_reloj_s` (60) y `aviso_reloj_critico_s` (10) y `reloj_jugador_s` (750), del Timer (#12); los presupuestos de largo de cada texto y `expansion_pct`, de Localización (#14); `objetivo_tactil_pt` (44) y las duraciones de los avisos de rechazo, de Entrada (#13); el +2 del Grito, del Modificador (#11); `limite_normal` (12), de Movimiento (#6); `vida_tramo` (30) y el corte de 2 tramos rotos, de Castillo (#5).

**Perillas que interactúan:** `ficha_opacidad_pct` con el tapado de la figura (F1: más opaca, más figura tapada) y con la legibilidad de los íconos (F5: menos opaca, íconos ilegibles sobre Hueso); `bloqueo_avance_ms` con el riesgo del doble clic y con la sensación de respuesta (Game Feel). La decisión de `ficha_opacidad_pct` es entre dos costos que ya están medidos: 96% tapa más figura y garantiza lectura, 88% deja ver más figura y falla el contraste de íconos sobre el bando claro.

## Visual/Audio Requirements

*Lente `art-director` (modo `full`, sin agente), con `design/art/art-bible.md` §1–4 y §7 leídas por sección. Todo vive en la capa del sistema; el dibujo de las figuras es de Render (#18).*

| Evento | Qué ve el jugador | Estado en el art bible |
|---|---|---|
| Pieza que entra al tablero (despliegue, recluta) | La ficha aparece junto con su figura en el mismo cuadro, sin animación propia | Definido (§7) |
| Selección o inspección de una pieza | La tarjeta del margen izquierdo entra en ≤ 0,25 s con frenado suave | **Falta**: la tarjeta y su lugar (regla 8 y 15) |
| Cambio de vida en la resolución | El número de vida cambia **de una vez**, en el cuadro del destello, sin contar hacia abajo | Definido (§7) |
| Pieza ya movida | Números e íconos de la ficha pasan a gris apagado `#A8A49B` y la placa gana un marco interior de 1 px `#F5F1E8` (≤ 0,25 s) | **Falta** (regla 5, F5); sustituye la «placa atenuada» elegida por el jugador |
| Bono del turno (Grito) | Banderín de 12 × 12 px en la esquina de la casilla y subrayado de 2 px bajo cada número que difiere de la base | **Falta** el banderín (lo propone el Modificador) y el subrayado |
| `AvisoEtapa` / `AvisoReloj` | Un solo ámbar `#E89A2C`, con pulso de 1 por segundo, en el temporizador que limita; el otro, borde grueso y número en negrita (F3) | **Cambia** §2/§7: el art bible define un solo timer |
| `TramoFinalIniciado` | Banner «Tramo final» al centro (1 s, una vez) y el anillo pasa a trazo grueso **de color base** | §2 corregido en la tanda 3 (antes pedía «anillo grueso ámbar») |
| `TiempoAgotado` | Marca «Tiempo» junto al anillo, ≤ 1 s, sin sonido de error | **Falta** (ya la pide el Timer) |
| Pausa, espera de tirada, espera del servidor | Anillo vacío y quieto con ícono y texto; relojes sin subrayado de activo ni pulso | **Falta** (ya lo pide el Timer) |
| `TurnoIniciado` | Contorno del tablero y emblema cambian de bando; banner del turno (≤ 0,25 s) | Definido (§7, §2 *Turno del rival*) |
| Contador de tramos rotos | Ícono de grieta y número en el margen de cada castillo; en 1 un renglón de aviso; en 2 el aviso `Sin reclutamiento` una vez y borde grueso | **Falta** el ícono en la lista de íconos (Castillo, Visual/Audio) |
| Rendirse disponible | El botón (bandera blanca y texto) aparece arriba a la izquierda (≤ 0,25 s); desaparece igual | **Falta** (bandera blanca) |
| Avanzar bloqueado (300 ms) | El botón toma el aspecto de deshabilitado y vuelve a activarse sin destello | Definido en espíritu (§7: animación ≤ 0,25 s) |
| Leyendas de la Máquina (`FaseSaltada`, `ReclutaPerdido`) | Una placa central de hasta 3 líneas, 1 s, sin sonido de error | **Falta** (ya la pide la Máquina) |
| Recluta automático | Marca breve `Elegido por el tiempo` sobre la ficha de la pieza, ≤ 1 s | **Falta** (ya la pide la Máquina) |

**Restricciones de animación y estilo.** Toda variación dura ≤ 0,25 s con frenado suave (salvo el banner de tramo final: 1 s, una vez). Nada anima en serie con la resolución: los números cambian en el mismo cuadro del destello. Los pulsos (1 por segundo, y nunca dos a la vez, F3) cuentan en el presupuesto de destellos del art bible (< 3 por segundo); «Reducir efectos» los apaga sin quitar ninguna forma. La ficha es **plana, sin textura ni gradiente pintado** y **no recibe la gradación de ambiente** de los estados de la sección 2 (la capa del sistema no se tiñe con el atardecer del tramo final).

**Principios del art bible que aplican:** *Regla visual (orden de lectura)*: lo que decide la jugada se lee primero (por eso la vista previa le gana a la ficha en `Atacar`); *Principio 1, el plan se lee antes que el detalle*; *Principio 3, el ritmo del turno manda sobre la animación*; §3 *Gramática de formas de la interfaz* (rectángulo de esquinas redondeadas, plano; puño redondeado; escudo heráldico); §4 *Daltonismo: pares en riesgo* (vida contra ataque, ámbar contra ataque, escudo contra muro) y su respaldo de forma.

**Lo que falta en el art bible y se le pide al director de arte** (la numeración sigue Open Questions): siete íconos nuevos más el de muro (movimiento, alcance, área, cobertura, banderín, grieta, bandera blanca; regla 17); el marco interior y el gris apagado de la ficha atenuada (F5); la opacidad de la placa en 96% en lugar de 85–90% (F5, Open Questions 7); el anillo del tramo final sin ámbar (Open Questions 2); la tarjeta de la ficha extendida y la zona de avisos en la banda superior (Open Questions 4); y el tapado de la figura por la ficha completa (F1, Open Questions 5).

**Audio (dirección; la especificación es de #22).** El HUD no tiene sonidos propios más allá de un clic corto y suave al tocar un botón (distinto para Avanzar). Los cues del timer (tictac suave de `AvisoEtapa`, un cue distinto de `AvisoReloj` nivel 2, un solo cue de tramo final, silencio en `TiempoAgotado`) son del Timer y de Audio; cada uno tiene su equivalente visual, así que apagar el sonido no esconde ningún aviso. Cambiar de turno es un cue suave (Máquina). Ningún sonido del HUD pasa por encima del impacto de la resolución en la mezcla ni suena como error.

📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:ficha-de-pieza-y-hud` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta en el mismo cuadro.** Tocar una pieza abre la tarjeta (empieza en el mismo cuadro del toque y termina en ≤ 0,25 s); un `Mover` aceptado cambia la atenuación, el contador y los números efectivos en el mismo cuadro en que se envía (Castillo y Movimiento ya lo piden); `Deshacer` quita la atenuación y baja el contador en el mismo cuadro en que la pieza empieza a volver (≤ 0,25 s; Máquina).
- **Números que no mienten.** Un número de la ficha cambia solo con un evento del juego y de una vez, nunca contando. El del anillo es `⌈restante ÷ 1000⌉` y no salta hacia atrás (Timer, Game Feel).
- **Un botón que no se dispara solo.** Avanzar acepta el primer toque y cierra la fase; el bloqueo de 300 ms es casi invisible para quien toca una vez y evita el doble clic. No hay confirmación (pilar 4).
- **Calma visual.** Ninguna animación del HUD compite con la resolución: la ficha, los contadores y los relojes están quietos mientras caen los golpes; el único movimiento propio es el pulso del timer que limita.
- **Presupuesto de dibujo (propuesta, a medir en el ADR de UI).** Las fichas se redibujan por evento, no por cuadro: con 122 fichas y sin eventos, el HUD no vuelve a dibujar números (AC-30).
- **Verificación.** Lo que se puede medir va en los criterios automáticos y de captura de Acceptance Criteria; lo que es de percepción va a playtest (AC-28 y AC-29): que se lee sin contar, que el turno y el temporizador que limita se identifican de un vistazo, y que la ficha completa no tapa tanto la figura como para molestar (Open Questions 5).

## UI Requirements

Este sistema **es** la capa de interfaz de la partida; su especificación completa es el Detailed Design (reglas 3 a 22) y el mapa de pantalla de la regla 15. Lo que sigue son los requisitos transversales que cada pantalla y cada HUD que derive de este GDD debe cumplir.

- **Lugares fijos.** Un elemento, un lugar (regla 15); ningún control tapa una casilla; las leyendas y placas centrales no tapan una casilla con pieza más de 1 s (art bible §2).
- **Sin hover.** Nada aparece solo al pasar el mouse: la tarjeta, la vida de un tramo y la ayuda se leen con un toque (regla del proyecto, art bible §7).
- **Objetivo táctil.** Avanzar, Deshacer, Rendirse, «?» y ES/EN con un blanco de 44 pt o más (`objetivo_tactil_pt`). Las fichas no son controles: el blanco de un toque sobre una pieza es la casilla (68 px).
- **Texto escalable y traducido.** Los textos del HUD y de la tarjeta admiten la escala de texto de Accesibilidad (#23) y el +40% de la traducción sin cortarse; **la ficha compacta no escala** (no hay lugar en 68 px): su equivalente accesible es la tarjeta de la pieza elegida, que sí escala. El tamaño mínimo de los números de la ficha es 14 px a 1080p.
- **Nada solo por color.** Cada color semántico lleva forma o ícono (puño, escudo, tipo en primer lugar, subrayado, banderín, borde grueso, grieta); el turno se marca con el contorno **y** con el emblema; el ámbar solo aparece en el temporizador que limita.
- **Lector de pantalla (alcance exacto en #23).** Cada ficha y la tarjeta exponen una descripción en texto: tipo, vida, ataque, escudo, movimiento, alcance, y estados (`Ya movida`, `Cubre a`, `En el muro`, bono del turno). El timer anuncia el tiempo solo a pedido o en los avisos de nivel 2, no cada segundo (Timer).
- **Daltonismo y grises.** Las capturas de un turno de ataque se prueban con los filtros protan, deutan y tritan y en escala de grises: en las cuatro se leen bando, tipo, vida, atenuación y temporizador que limita (art bible §4, prueba de validación).
- **Opciones.** «Ficha simple» y «Reducir efectos» (regla 7 y Tuning Knobs); las guarda Ajustes (#20).
- **Resolución.** Mínimo 1080p para jugar en web; por debajo, aviso único y escala uniforme (Edge Cases). La adaptación móvil (#41) rediseña la ficha y los márgenes.

> **📌 UX Flag — Ficha de pieza y HUD**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly. Specs sugeridas: `design/ux/hud.md` (mapa de pantalla, relojes, controles) y `design/ux/ficha-de-pieza.md` (ficha compacta y tarjeta).

## Cross-References

| Documento | Qué se referencia | Cómo |
|---|---|---|
| `design/gdd/libreria-de-reglas.md` | Regla 3 (`Rendirse` y su condición), regla 10 (`EffectiveStats` y consultas puras, todas de solo lectura), regla 11 (el estado no incluye la presentación) | El HUD consulta y no recalcula; su estado propio queda fuera de la huella |
| `design/gdd/catalogo-de-piezas.md` | Regla 4 (valores), regla 5 (estático y bonos fuera del Catálogo), regla 9 (sin arte), UI Requirements («mostrar siempre la efectiva y, si difiere, la base») | La ficha cumple ese requisito; la restricción ≤ 99 es nueva para el Catálogo |
| `design/gdd/maquina-de-turno.md` | Regla 2 (fases), regla 14 (relojes quietos en `EsperandoTirada`), Visual/Audio (leyendas y eventos), UI Requirements (qué lee el HUD), Open Questions («cómo se ven los dos relojes») | Reglas 12 a 14 y 19 de este GDD; cierra la pregunta de los relojes |
| `design/gdd/timer-de-turno.md` | Regla 12 (eventos), regla 13 (vista de relojes), D4 (`mostrado_s` y umbrales), Visual/Audio y UI Requirements, Open Questions (ámbar; animación de resolución sobre el reloj) | Regla 12 y F2, F3; el anillo del tramo final cambia (Open Questions 2) |
| `design/gdd/modificador.md` | Regla 7 (el Grito no sube el ataque del demoledor ni el área), F3 (`estadisticas_con_grito`), Visual/Audio (banderín, ficha con base), UI Requirements, AC-20 y AC-31 | Reglas 4 a 6; AC-20 y AC-33 de este GDD |
| `design/gdd/localizacion.md` | Reglas 1, 4, 11, 12, 14 y 16 (tablas, claves, cambio de idioma, respaldo), F4 y F5 (largo y texto inflado), UI Requirements (botón ES/EN, avisos en un lugar fijo) | Reglas 20 y 21; la zona de avisos de la regla 18 |
| `design/gdd/castillo-y-muros.md` | UI Requirements y Visual/Audio (contador de tramos rotos, vida de cada tramo, ficha del arquero en el muro) | Reglas 9 y 16; edge cases del muro |
| `design/gdd/movimiento.md` | Regla 10 (`piezasMovidas` se calcula de `usados`), regla 15 (consultas para el HUD), estados de una pieza, Open Questions («marca de pieza ya movida») | Regla 5 y regla 14; cierra esa pregunta |
| `design/gdd/entrada-y-seleccion.md` | Reglas 3, 5 y 13 (blancos, selección e inspección, Rendirse con confirmación), Open Questions 3, 6 y 7 | Reglas 8, 10 y 14; resuelve el doble clic, la marca de pieza movida y el lugar de Rendirse |
| `design/gdd/cobertura-del-escudero.md` | Visual/Audio y UI Requirements («Cubre a [tipo]» / «Cubierto por [tipo]» en la ficha) | Fila de estados de la regla 8 |
| `design/gdd/tablero-y-despliegue-inicial.md` | Reglas 4 y 5 (castillos a izquierda y derecha, jugador 0 a la izquierda), UI Requirements (medidas de 1080p) | Regla 12, regla 15 y F1 |
| `design/art/art-bible.md` | §2 (estados, ritmo), §3 (gramática de formas), §4 (colores, daltonismo), §7 (pantalla, ficha, tipografía, íconos) | Base visual; los cambios pedidos (Open Questions 2, 4 y 7) se aplicaron al cerrar la tanda 3 |
| `design/gdd/game-concept.md` | Pilares 1, 3 y 4; anti-pilares; tabla «Reglas vigentes» (Rendirse, Timer, Estadísticas) | Player Fantasy y reglas de visibilidad |
| `design/gdd/systems-index.md` | Sistema #15 y sus aristas | Dependencies |
| `design/registry/entities.yaml` | `aviso_etapa_s`, `aviso_reloj_s`, `aviso_reloj_critico_s`, `reloj_jugador_s`, `bono_grito_ataque`, `bono_grito_movimiento`, `limite_normal`, `vida_tramo`, `objetivo_tactil_pt`, `presupuesto_aviso`, `estadisticas_con_grito`, `mostrado_s`, y las siete piezas | Sin conflicto de valores; candidatos nuevos en el informe de la sesión |

## Acceptance Criteria

**Convenciones.** Salvo que se indique otra cosa, un criterio es un test automático de tipo **Logic** (bloqueante) que corre en EditMode contra el estado de la Librería con una fuente de tiempo falsa, en `Assets/Tests/EditMode/FichaYHud/`. **UI** = captura retenida en `production/qa/evidence/` (bloqueante; para este sistema la imagen es el producto). **Playtest** = observación con jugadores, en el MVP. Los criterios marcados **PROVISIONAL** dependen de un GDD o de un activo todavía sin escribir y se actualizan cuando exista. «A 1080p» significa una ventana de 1920 × 1080 con el tablero de 24 × 15 casillas de 68 px.

**Ficha compacta (reglas 2 a 7; F1, F6)**

- **AC-1 (UI + Logic; regla 3, F1)** — **GIVEN** a 1080p una pieza de cada uno de los siete tipos, con los valores vigentes del Catálogo, **WHEN** se dibuja su ficha compacta, **THEN** la placa mide como mucho 64 × 40 px (medido con el diseño de la UI, no a ojo), cada número mide 14 px o más, y la captura de las siete fichas queda en `production/qa/evidence/ficha-compacta-siete-tipos.png`. **Borde:** con «Ficha simple» la placa mide como mucho 64 × 20 px.
- **AC-2 (Logic; regla 4, F6)** — **GIVEN** un arquero (ataque base 6) bajo el Grito, **WHEN** se consulta lo que dibuja su ficha, **THEN** el ataque mostrado es 8, con subrayado de 2 px; **GIVEN** el mismo arquero sin Grito, **THEN** el ataque es 6 y no hay subrayado (`delta` = 0). **GIVEN** un arquero en el muro, **THEN** el escudo es 4 con subrayado.
- **AC-3 (Logic; F1, PROVISIONAL hasta elegir la fuente)** — **GIVEN** la fuente elegida a 14 px negrita, **WHEN** se mide el ancho de las cifras 0 a 9, **THEN** ninguna pasa de 7 px y el `ancho_ficha` del peor caso (99/99/99) es como mucho 64 px.
- **AC-4 (Logic; regla 4, F1)** — **GIVEN** el Catálogo vigente, **WHEN** se calcula cada cifra de cada tipo más su bono máximo (el bono de turno máximo de la Librería, 10), **THEN** ninguna pasa de 99. **Borde:** con una cifra de 99 el test pasa; con 100 falla y nombra el tipo y la cifra.
- **AC-5 (Logic; regla 5)** — **GIVEN** el turno propio en `Mover` y un soldado con `usados = 0`, **WHEN** se mueve una casilla, **THEN** su ficha pasa a *Atenuada* en el mismo cuadro; **WHEN** se hace `Deshacer`, **THEN** vuelve a *Normal* en el mismo cuadro. **GIVEN** el Rey con `usados > 0`, **THEN** su ficha está atenuada y el contador de piezas movidas no sube. **GIVEN** la fase `Atacar` o el turno del rival, **THEN** ninguna ficha está atenuada. **GIVEN** una pieza recién reclutada, **THEN** no está atenuada.
- **AC-6 (UI; regla 6)** — **GIVEN** el Grito de guerra activo y un demoledor, un arquero y una catapulta propios, **WHEN** se muestra el tablero, **THEN** las tres piezas llevan el banderín; el demoledor muestra ataque 30 **sin** subrayado; el arquero, 8 con subrayado; la catapulta, 12 con subrayado. Evidencia: `production/qa/evidence/ficha-grito-banderin.png`.
- **AC-7 (Logic; regla 7)** — **GIVEN** «Ficha simple» activa, **WHEN** se dibuja una pieza, **THEN** la ficha compacta muestra solo tipo y vida; **WHEN** se elige esa pieza, **THEN** la tarjeta muestra ataque y escudo; **THEN** la vista previa de la fase de ataque se ve igual que con la opción apagada. **Borde:** la opción apagada (valor por defecto) muestra los dos renglones.

**Tarjeta extendida (reglas 8, 9; F7)**

- **AC-8 (Logic; regla 8, F6)** — **GIVEN** cada uno de estos casos, **THEN** la tarjeta muestra: catapulta con Grito, `Ataque 12 (10)` y `Área 6` sin base; demoledor con Grito, `Ataque 30`, `Movimiento 8 (6)`, sin fila de alcance y `Área 30`; arquero en el muro, `Escudo 4 (2)`, `Alcance 1–6 (1–4)` y el estado `En el muro`; escudero que cubre a un soldado, el estado `Cubre a Soldado`; pieza propia movida en `Mover`, `Movimiento restante / efectivo` y el estado `Ya movida`; pieza rival, sin `restante` ni `Ya movida`.
- **AC-11 (Logic + UI; regla 9)** — **GIVEN** un tramo en pie de 22 de 30 de vida sin pieza, **WHEN** se lo inspecciona, **THEN** la tarjeta muestra `Tramo de muro` y `22 / 30`; **GIVEN** un tramo roto, **THEN** no hay tarjeta.
- **AC-24 (Logic + UI; reglas 20 y 21, F7)** — **GIVEN** el texto inflado +40% de Localización (F5) en español e inglés, **WHEN** se abre la tarjeta del peor caso (seis filas y tres estados que envuelven), **THEN** su alto es como mucho 400 px, ningún texto se corta y el recorte con `…` (si ocurre) queda registrado. **WHEN** se cambia el idioma con la tarjeta abierta, **THEN** el texto cambia en el cuadro siguiente, la tarjeta sigue abierta, el bloqueo de Avanzar no se reinicia y el aviso en pantalla conserva su tiempo restante. **THEN** ningún texto visible del HUD sale de una cadena literal fuera de las claves `ui.hud_*`, `pieza.*`, `fase.*`, `cara.*` y `fin.*`.

**Legibilidad y atenuación (regla 5; F5)**

- **AC-9 (UI, bloqueante; regla 5)** — **GIVEN** una ficha normal y una atenuada, cada una sobre suelo Hueso y sobre suelo Ciruela, **WHEN** se capturan en color, en escala de grises y con los filtros protan, deutan y tritan (cinco imágenes por suelo, diez en total), **THEN** un revisor que no ve las etiquetas identifica cuál es la atenuada en las 10 imágenes. Evidencia: `production/qa/evidence/ficha-atenuada-*.png`. **Si falla en alguna**, se aplica el plan de respaldo de la regla 5 (huella de 14 px) y se repite.
- **AC-10 (Logic; F5)** — **GIVEN** los colores del art bible y `ficha_opacidad_pct` vigente, **WHEN** se calcula el contraste, **THEN** cada número contra su borde Tinta es 4,5 o más, normal y atenuado (vida 10,3 · ataque 4,8 · escudo 6,0 · gris 6,9), y cada ícono contra la placa es 3 o más sobre Hueso, Tierra y Ciruela. **Borde documentado:** con `ficha_opacidad_pct` = 88 el test falla para el puño sobre Hueso (1,9) y para el escudo (2,4); con 95 el puño da 2,95 y falla; con 96 pasa (3,2).

**Fase de ataque, muerte y skins (reglas 2, 10, 11)**

- **AC-12 (Integration; regla 10, PROVISIONAL: depende de #16)** — **GIVEN** la fase `Atacar`, **WHEN** se asigna un ataque a un enemigo, **THEN** su ficha compacta se oculta y su lugar lo ocupa la vista previa en el mismo cuadro; **WHEN** se quita la única asignación, **THEN** la ficha vuelve en el mismo cuadro. **THEN** en ningún cuadro hay ficha y vista previa a la vez sobre la misma pieza.
- **AC-26 (UI; regla 2, PROVISIONAL: depende de #18 y #38)** — **GIVEN** la misma partida dibujada con la skin básica y con un pack de prueba, **WHEN** se compara la zona de la ficha (placa, íconos, números, banderín), **THEN** es idéntica píxel a píxel.
- **AC-27 (Integration; regla 11)** — **GIVEN** una pieza elegida que muere en la resolución, **WHEN** se resuelve, **THEN** su ficha y la tarjeta desaparecen en el mismo cuadro que su figura y queda la placa MUERE.

**Turno, tiempo y controles (reglas 12 a 14; F2 a F4)**

- **AC-13 (Logic; regla 12, F3)** — **GIVEN** estas combinaciones, **THEN**: etapa con 8 s y reloj activo en 3:20 → anillo ámbar con pulso; etapa con 8 s y reloj activo en 0:07 → **reloj** ámbar con pulso y anillo en borde grueso sin ámbar; reloj del rival en 0:40 → borde grueso, nunca ámbar; pausa o `EsperandoTirada` → sin ámbar ni pulso y con borde grueso donde corresponda; Sin límite → sin anillo ni relojes; etapa y reloj que vencen a la vez → ámbar en el reloj. **Propiedad:** para todas las combinaciones de `vinculante`, `nivelAvisoReloj` (0 a 2), `AvisoEtapa`, activo, pausa y espera, **como mucho un elemento está en ámbar** y como mucho uno pulsa. **Borde:** `vinculante` = `Reloj` con `nivelAvisoReloj` = 0 marca el reloj con borde grueso.
- **AC-14 (Logic; F2)** — **GIVEN** `mostrado_s` = 0, 59, 60, 61 y 750, **THEN** el reloj muestra `0:00`, `0:59`, `1:00`, `1:01` y `12:30`; `0:00` aparece solo si el reloj venció.
- **AC-15 (UI, bloqueante; regla 12, cumple el AC-25 del Timer)** — **GIVEN** a 1080p, **WHEN** se captura cada estado, **THEN** hay una imagen del anillo normal, de `AvisoEtapa`, del tramo final con su banner, de `AvisoReloj` nivel 1 y nivel 2 (reloj limitando), de la partida pausada y de Sin límite, y en cada una el ámbar aparece solo en un elemento. Evidencia: `production/qa/evidence/hud-timer-*.png`.
- **AC-16 (UI; regla 13)** — **GIVEN** un `TurnoIniciado`, **WHEN** cambia el bando de turno, **THEN** el contorno del tablero y el emblema pasan a Hueso o a Ciruela en ≤ 0,25 s y el turno se lee igual en grises (contorno y emblema, sin texto). Evidencia: `production/qa/evidence/hud-bando-de-turno.png`.
- **AC-17 (Logic; regla 14, F4)** — **GIVEN** Avanzar activo en `Mover`, **WHEN** llega un toque en t = 0, **THEN** se manda `TerminarMovimiento`; **WHEN** llega otro en t = 150 ms, **THEN** se ignora sin aviso; **WHEN** llega otro en t = 300 ms exactos, **THEN** se acepta (≥); **GIVEN** `bloqueo_avance_ms` = 0, **THEN** no hay bloqueo. **GIVEN** `EsperandoTirada`, `Reclutar`, turno rival o `Terminada`, **THEN** el botón no responde.
- **AC-18 (Logic; regla 14)** — **GIVEN** la pila de deshacer vacía, **WHEN** se toca Deshacer, **THEN** no se envía ningún comando y el botón está desactivado; **GIVEN** al menos un `Mover` en la pila en `Mover` propio, **THEN** está activo; **GIVEN** `Atacar`, **THEN** está desactivado.
- **AC-19 (Logic; regla 14)** — **GIVEN** `Mover` con 5 piezas movidas y límite 12, **THEN** el contador dice `5/12`; **GIVEN** 12 piezas movidas, **THEN** `12/12` con borde grueso; **GIVEN** Arremetida, **THEN** muestra `5` con el ícono de infinito y sin tope; **GIVEN** el Rey movido, **THEN** no suma; **GIVEN** `Atacar`, **THEN** el contador no se muestra.
- **AC-20 (Integration; regla 14; cruza el AC-20 del Modificador)** — **GIVEN** 6 piezas propias contra 13, **THEN** el jugador del turno ve Rendirse; **GIVEN** 7 contra 13, **THEN** no; **GIVEN** el que va ganando, **THEN** nunca lo ve; **GIVEN** 6 contra 13 y un reclutamiento que lo deja en 7, **THEN** el botón desaparece en el cuadro del reclutamiento; **GIVEN** el diálogo de confirmación abierto y un `TurnTimeout`, **THEN** el diálogo se cierra y no se manda `Rendirse`.
- **AC-21 (Logic + UI; regla 15)** — **GIVEN** a 1080p, **WHEN** se calculan los rectángulos de todos los elementos del HUD, **THEN** ninguno se superpone con el rectángulo del tablero (x 144 a 1776, y 60 a 1080), salvo las leyendas y placas centrales de ≤ 1 s. Evidencia: `production/qa/evidence/hud-mapa-1080p.png`.
- **AC-22 (Logic + UI; regla 16)** — **GIVEN** 0, 1 y 2 tramos rotos en el castillo del jugador 0, **THEN** el contador del margen izquierdo muestra 0, 1 y 2 con el ícono de grieta; en 1 aparece un renglón de aviso; al pasar a 2 aparece `Sin reclutamiento` **una sola vez** y el contador queda con borde grueso; el del castillo derecho no cambia.
- **AC-23 (Logic; reglas 18 y 19)** — **GIVEN** `ReclutaPerdido` y `FaseSaltada(Atacar)` en la misma cascada, **WHEN** se muestran, **THEN** hay una sola placa central con dos líneas durante 1.000 ms, la cascada completa dura como mucho 1,5 s y los relojes siguen corriendo. **GIVEN** un rechazo de Entrada, **THEN** el aviso sale en la zona de la banda superior y un aviso nuevo reemplaza al anterior; **THEN** ningún aviso se dibuja sobre el tablero.
- **AC-36 (Integration; regla 22, PROVISIONAL: depende de #21)** — **GIVEN** una partida que termina por `RelojAgotado`, **THEN** el HUD entrega a la pantalla de resultado el motivo `fin.RelojAgotado` y los dos relojes.

**Solo lectura, rendimiento y bordes (regla 1; Edge Cases)**

- **AC-25 (Logic; regla 1)** — **GIVEN** una partida fija (semilla y comandos), **WHEN** se juega con el HUD activo y sin él, **THEN** la huella del estado es idéntica; **THEN** cada cifra que muestra la ficha es igual a la de `EffectiveStats` en las 61 piezas de un estado de prueba (el HUD no tiene un segundo cálculo).
- **AC-30 (Performance; Game Feel)** — **GIVEN** 122 fichas en pantalla, **WHEN** pasan 60 cuadros sin eventos, **THEN** el HUD no redibuja ningún número; **WHEN** llega un evento que cambia una cifra, **THEN** solo se redibujan las fichas afectadas. El presupuesto de tiempo por cuadro lo fija el ADR de UI (PROVISIONAL).
- **AC-31 (UI; Edge Cases, PROVISIONAL: política de resolución de #18)** — **GIVEN** una ventana menor que 1080p, **THEN** aparece una sola vez el aviso `Para jugar bien necesitás al menos 1080p` y el HUD escala de forma uniforme.
- **AC-32 (Logic; Edge Cases)** — **GIVEN** un Catálogo de prueba con una cifra de 100, **WHEN** se dibuja la ficha, **THEN** muestra `100` completo (no lo trunca), la placa crece a 71 px y se registra un error en el build de desarrollo.
- **AC-35 (Release check; regla 17)** — **GIVEN** un build candidato, **THEN** no contiene ningún marcador de texto sustituto de los íconos de la regla 17.

**Cruces con otros sistemas**

- **AC-33 (UI; cruza el AC-31 del Modificador, PROVISIONAL: depende de #16)** — **GIVEN** el Grito activo y un arquero asignando un ataque, **THEN** su ficha muestra 8 subrayado y la tarjeta `8 (6)`, la vista previa usa 8, la catapulta muestra 12 de impacto y 6 de área, y el demoledor no muestra cambio de ataque. Evidencia: `production/qa/evidence/modificador-grito-ficha-vista-previa.png`.
- **AC-34 (Integration; cruza el Castillo)** — **GIVEN** un arquero propio al lado de un tramo libre, **WHEN** se acepta el `Mover` al tramo, **THEN** en ese mismo cuadro su ficha muestra escudo 4 y alcance 1–6 con subrayado, y la tarjeta agrega `En el muro`.

**Playtest (MVP)**

- **AC-28 (Playtest; pilar 1, regla 4)** — **GIVEN** diez jugadores que no conocen las cifras, con el Grito activo y una pieza afectada, **WHEN** se les pide el ataque de esa pieza, **THEN** al menos nueve dicen el valor efectivo y la base en 3 s o menos, sin ayuda.
- **AC-29 (Playtest; pilares 1 y 4, regla 12)** — **GIVEN** esos diez jugadores con la pantalla en escala de grises, **WHEN** se les pregunta de quién es el turno y cuál temporizador los limita, **THEN** al menos nueve aciertan las dos respuestas en 2 s o menos.

**Cobertura y huecos encontrados por la lente `qa-lead`** (con las secciones C, D y E releídas del archivo): cada una de las 22 reglas y las siete fórmulas tiene al menos un criterio (regla 17 en AC-35; regla 21 en AC-24; regla 22 en AC-36; F7 en AC-24). **No son automatizables y van a playtest o a captura:** que la marca de atenuación se distinga a distancia de juego (AC-9, revisión humana de capturas), que «se lee sin contar» (AC-28) y que la ficha completa no tape demasiado la figura (Open Questions 5). **Sin criterio por depender de un GDD inexistente:** el tamaño y el contenido de la vista previa (de #16), la etiqueta de la cara y el «?» (de #17), la política de resolución (de #18). Los criterios PROVISIONAL son AC-3, AC-11, AC-12, AC-26, AC-27, AC-30, AC-31, AC-33 y AC-36.

## Open Questions

| # | Pregunta | Dueño | Cuándo |
|---|---|---|---|
| 1 | ~~**Mecanismo de la atenuación.**~~ **Resuelta 2026-11-25:** el jugador confirmó gris apagado + marco interior de 1 px (F5); AC-9 lo valida con capturas | — | — |
| 2 | ~~**Tramo final.**~~ **Resuelta 2026-11-25:** el jugador confirmó el anillo grueso de color base; el ámbar queda solo para el aviso. Aplicado al art bible §2 y al Visual/Audio del Timer | — | — |
| 3 | ~~**Inspeccionar un tramo.**~~ **Resuelta 2026-11-25 (cierre de la tanda 3):** Entrada (#13) admite que una pulsación sobre un tramo en pie sin pieza lo deje en inspección; el «pasar sobre el tramo» de Castillo queda como atajo de mouse, no como única vía | — | — |
| 4 | ~~**Zona de avisos.**~~ **Resuelta 2026-11-25 (cierre de la tanda 3):** Entrada (#13) adopta la banda superior a la derecha del timer (regla 18) y #17 usa la misma zona | — | — |
| 5 | **Tapado de la figura.** La ficha completa tapa ≈ 61% de la altura de una figura del 75% de la casilla y la Ficha simple ≈ 26% (F1, supuesto de figura centrada). El art bible aceptó el costo; ¿se mide con las figuras reales en playtest, y se evalúa que la Ficha simple sea el valor por defecto de quien ya conoce las cifras? | Director de arte, Render (#18), playtest | MVP |
| 6 | **Calibrar el bloqueo de Avanzar** (`bloqueo_avance_ms` = 300): ¿evita el doble clic sin sentirse «pegado»? | Playtest | MVP |
| 7 | ~~**Opacidad de la placa.**~~ **Resuelta 2026-11-25:** el jugador confirmó 96%; aplicado al art bible §7 | — | — |
| 8 | ~~**Etiqueta de la cara y «?».**~~ **Resuelta 2026-11-25 (cierre de la tanda 3):** #17 acepta el lugar (regla 15) y ya define el primer turno: «Apertura: sin dado» sin ícono (`ui.modif_etiqueta_apertura`), que este GDD adopta en lugar de `Primer turno` | — | — |
| 9 | **Piezas vivas de los dos jugadores.** De ellas depende la tabla del Modificador y la condición de Rendirse, pero el HUD no las muestra (el art bible no les dio lugar). ¿Entran en el panel «?» de #17? | #17 y jugador | Al diseñar #17 |
| 10 | **Escala de texto contra ficha fija.** La ficha compacta no puede escalar en 68 px; su equivalente es la tarjeta. ¿Accesibilidad acepta ese reparto? | #23 | Al diseñar #23 |
| 11 | **Animación de resolución sobre el reloj** (pregunta del Timer): hoy el reloj del jugador siguiente corre durante la animación de ≤ 1,5 s y el HUD lo muestra corriendo, sin pausa. ¿Se mantiene? | Playtest | MVP |
| 12 | **Pieza agotada contra en marcha.** La atenuación marca «ya la toqué», no «ya no puede moverse». ¿Hace falta distinguir una pieza agotada (movimiento restante 0)? | Playtest | MVP |
| 13 | ~~**Nombres de bando.**~~ **Resuelta 2026-11-26:** convención del jugador: «azul/rojo» = jugadores 0/1 como nombres internos de reglas y tests; Hueso (jugador 0) y Ciruela (jugador 1) son los colores en pantalla (Tablero regla 4, art bible §4) | — | — |
| 14 | **Fuente tipográfica.** El art bible nombra la familia pero no la elige; F1 usa 7 px por cifra como supuesto (AC-3) | Director de arte | Antes de cerrar F1 |
| 15 | ~~**Elección de la Player Fantasy.**~~ **Resuelta 2026-11-26:** el jugador confirmó la A, «Leo el tablero, no lo cuento» | — | — |
