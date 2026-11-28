# Vista previa de ataques

> **Status**: Approved (decisión del jugador al cerrar la tanda B, 2026-11-28: queda aprobado tal como lo marcó el índice el 2026-11-26, commit 6b9a4f6; el `/design-review` del 2026-11-26 dio NEEDS REVISION y se aplicaron W-16, W-17, W-19, W-20 y las Recommended Revisions; sin log propio en `design/gdd/reviews/`)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-26
> **Last Verified**: 2026-11-26
> **Implements Pillar**: Pilar 1 (Planificar y cosechar); Pilar 2 (La fuerza está en combinar) en lo que enseña; Pilar 4 (Duelo corto) como restricción
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-26 — pilar 2 (regla 7 y Visual/Audio: la placa no crece con la cantidad de atacantes; el combo lo llevan la punta fundida de la flecha y la fila Atacantes de la tarjeta, Open Questions 10), pilar 4 (F4: el «mismo cuadro» depende de un presupuesto de dibujo web sin medir, Open Questions 6) y pilares 1 y 4 (regla 12: la derrota propia sin confirmación se valida en playtest, Open Questions 4). El gate lo aplicó la sesión misma, sin agentes.

## Summary

La vista previa de ataques muestra sobre el tablero, durante la fase de ataque y antes de resolver, el resultado exacto de las flechas asignadas. Sobre cada pieza o tramo que va a recibir golpe, una placa en el lugar de su ficha dice cuánto golpe suma, cuánto frena el escudo y con cuánta vida queda, o MUERE y ROTO. También marca el golpe desviado al escudero, lo propio dañado y si la partida terminaría. Existe para que el pilar 1 (*«el daño es fijo y se ve antes de resolver»*) se cumpla a la vista y para enseñar sin texto la regla del pilar 2. No calcula nada: dibuja los eventos de `Resolver` aplicado sobre una copia del estado, así que lo previsto y lo resuelto no pueden diferir.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `Librería de reglas (#1), Resolución simultánea del daño (#9), Alcance y asignación (#7), Cobertura del escudero (#8), Ficha y HUD (#15), Localización (#14), Entrada y selección (#13), Máquina de turno (#3); blandas: Castillo y muros (#5), Modificador (#11), Catálogo de piezas (#2), Tablero (#4)`

## Overview

La vista previa de ataques es la capa del cliente que, durante la fase `Atacar` del jugador del turno, muestra **sobre el tablero el resultado exacto de los ataques asignados antes de resolver**: cuánto golpe recibe cada pieza y cada tramo, cuánto frena su escudo, con cuánta vida queda y qué muere o se rompe. Cada vez que se acepta un comando que cambia lo asignado (una flecha nueva, una reemplazada, una quitada, una cobertura que desplaza un ataque), el cliente pide a la Librería `VistaPrevia`, que es **`Resolver` aplicado sobre una copia del estado** (Librería, regla 9; Resolución, regla 15). Recibe la misma lista de eventos que daría la resolución y la dibuja: una **placa** en el lugar de la ficha de cada receptor (Ficha y HUD, #15, regla 10), las placas **MUERE** y **ROTO**, el golpe desviado al escudero, las marcas de fuego amigo y, si la partida terminaría, quién gana. No calcula nada por su cuenta: no hay un segundo cálculo del daño que pueda contradecir al primero.

**Del lado del dato**, es un traductor de eventos a dibujo, sin estado de juego: lee `DanoRecibido`, `DanoATramo`, `TramoRoto`, `PiezaMuerta`, `CoberturaQuitada` y el evento de fin de la Máquina (Resolución, regla 14; Librería, regla 8) y no entra al núcleo, a la huella ni a la forma canónica. **Del lado del jugador**, es la herramienta del pilar 1 y la manera en que el juego enseña su regla central sin texto. El jugador la usa de forma **activa y continua**: cada flecha que traza cambia las placas en el mismo cuadro, así que probar una combinación, ver que no alcanza y sumar una pieza más es el gesto normal de la fase de ataque. El jugador ve el golpe sumado al lado del escudo que lo frena, y así entiende que tres golpes que solos rebotan, juntos pasan (pilar 2).

**Por qué existe.** Sin ella, el pilar 1 (*«el daño es fijo y se ve antes de resolver»*) sería una promesa de reglas que el jugador tendría que verificar sumando a mano bajo el timer. La suma por objetivo, el escudo restado una vez, la cobertura que desvía, el área que toca lo propio y la explosión del demoledor sobre los muros son reglas exactas pero difíciles de calcular de cabeza. La vista previa hace que el resultado se **lea**, y que la cosecha (Resolución, #9) se sienta como un plan cumplido y no como azar. Los GDD vecinos ya le dejaron pedidos explícitos: Alcance (#7) le delega el fuego amigo sin confirmación (CD-GDD-ALIGN aceptado), Cobertura (#8) el desvío de las flechas, Castillo (#5) el daño y el ROTO del tramo, Resolución (#9) el exceso perdido y Modificador (#11) el Grito incluido antes de resolver.

*Alternativas descartadas:* (1) calcular la vista previa en el cliente con una fórmula propia, más simple que la resolución: es el «segundo cálculo» que la Librería prohíbe (regla 9) y la primera fuente de diferencias entre lo previsto y lo resuelto; (2) un panel lateral con la lista de resultados: el tablero ocupa toda la pantalla (art bible §7) y la referencia elegida es poner el resultado **sobre** el objetivo (art bible §9, Into the Breach); (3) mostrar la vista previa solo al pedirla (un botón o al mantener presionado): obliga a un gesto extra por flecha y depende de algo que no se ve hasta tocarlo, contra la regla del proyecto de no esconder información detrás del hover.

## Player Fantasy

**La mesa de guerra que no miente.** Sos el general inclinado sobre el mapa con las fichas de madera: movés una, la corrés una casilla, sumás otra, y el mapa te contesta al instante qué va a pasar. No adivinás ni sumás de cabeza bajo el reloj: el tablero ya te lo dice. Cuando tocás Terminar turno ya sabés lo que va a caer, y la cosecha (#9) solo lo confirma.

**Momento ancla:** el soldado rival tiene escudo 3 y vida 25. Trazás la flecha de un soldado: la placa sobre él dice golpe **8**, escudo **3**, vida **25 → 20**. Sumás un segundo soldado: golpe **16**, vida **25 → 12**. Sumás un arquero: golpe **22**, vida **25 → 6**. Sumás el tercer soldado: la placa se vuelve **MUERE** (golpe 30, escudo 3). Cuatro flechas, cuatro respuestas, y en ninguna tuviste que hacer la cuenta. El escudo nunca cambió: lo que cambió fue cuántos le pegaron juntos.

**Dos momentos más que la sostienen.**
- **Ver el desvío.** Apuntás al arquero que el rival cubrió y la flecha termina en el escudero de al lado; la placa del escudero dice golpe 22, escudo 5, **20 → 3**, y el arquero conserva su ficha intacta. Entendés que tenés que matar primero al escudero o ir a otro blanco. Lo entendés antes de gastar el turno, no después.
- **Ninguna sorpresa propia.** Apuntás la catapulta al centro de la línea rival y aparece una placa con **contorno discontinuo** sobre tu propio escudero, que quedó en el anillo. Corrés el disparo una casilla y la placa se va. Si la placa estuviera sobre tu Rey, la zona de avisos lo diría con todas las letras.

**Pilares que sirve:**

- Pilar 1, *«Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos.»* Su prueba de diseño, *«El daño es fijo y se ve antes de resolver»*, es literalmente este sistema. El azar ya se resolvió antes de planificar (Modificador); entre el plan y la cosecha no hay nada oculto ni sorpresivo.
- Pilar 2, *«Ninguna pieza gana sola: el poder sale de juntar varias sobre un mismo objetivo.»* La placa pone el golpe sumado al lado del escudo que lo frena. El jugador ve, flecha a flecha, el punto en que la suma pasa el escudo: así se enseña la regla sin texto (índice de sistemas, fila 16).
- Pilar 4 como límite: la mesa contesta en el mismo cuadro y no agrega pasos. Probar combinaciones entra dentro del timer de atacar, que es la presión buscada.

**Qué le exige al resto del diseño:** que la vista previa sea exactamente la resolución (Librería, regla 9) y que todo lo que la resolución vaya a hacer tenga una forma visible antes. Eso incluye el desvío, el área, la explosión, el tramo, la muerte del Rey y el fin de la partida. También exige que la placa se lea sin contar casillas ni pasar el mouse, y que nunca muestre un número que la cosecha contradiga. El riesgo propio de esta fantasía es la **saturación**: con doce flechas, un área y una explosión, una mesa que lo dice todo puede volverse ilegible. Por eso la placa ocupa el lugar de la ficha y nunca se suma a ella (#15, regla 10), y el detalle completo vive en la tarjeta de la pieza en inspección, no en el tablero.

*Encuadres considerados (lente `creative-director`, sin agente):* **A** «la mesa de guerra que no miente» (pilar 1: anticipación exacta), **B** «ver el escudo ceder» (pilar 2: la suma que atraviesa) y **C** «sin sorpresas propias» (pilar 1: el costo a la vista). Se escribió A como encuadre, y B y C quedaron como los momentos que la sostienen. A pide al diseño la identidad con la resolución y la respuesta en el mismo cuadro; B pide que la placa muestre golpe y escudo por separado (decisión del jugador, 2026-11-26); C pide la marca de fuego amigo por forma y el aviso del Rey propio. Elegido en `guided` por recomendación; cambiable si preferís otro encuadre.

## Detailed Design

*Vistas por rol (Section C, modo `full`, sin agentes; fila «UI systems» del enrutado).* **game-designer:** la decisión del jugador en `Atacar` es a quién sumar; la placa tiene que contestar «¿alcanza?» de un vistazo, y MUERE es la respuesta binaria. Lo que la vista previa no hace es parte del diseño: no simula movimientos, no muestra las amenazas del rival para su turno ni pide confirmación. **ux-designer:** lo que decide la jugada (MUERE, ROTO, el Rey propio) es lo más contrastado; nada depende del hover; el detalle va a la tarjeta de la pieza en inspección, no al tablero; cada marca se distingue por su forma. **ui-programmer:** hasta 154 receptores a la vez (122 piezas y 32 tramos), así que el redibujo tiene su propio presupuesto (F4); las placas reusan el ancla de la ficha, las claves de texto se guardan como clave y nunca se arma texto pegando trozos. **art-director:** la placa es capa del sistema, como la ficha; faltan en el art bible el ícono de impacto, el contorno discontinuo de lo propio, el marco de fin y la línea de cobertura cortada. **technical-artist:** el arquero sobre su tramo tiene dos placas en una casilla de 68 px; entran si la del tramo es de un solo renglón (F1). *Decisiones del jugador (2026-11-26):* placa sobre **todo receptor**, no solo sobre el objetivo de una flecha; contenido **golpe, escudo y vida**; **sin confirmación** cuando el Rey propio muere.

### Core Rules

#### A. Qué es y cuándo se ve

1. **Qué es.** Una capa de cliente **de solo lectura**, fuera del núcleo (índice de sistemas, *Frontera del núcleo puro de C#*). Su única fuente de resultados es la operación de vista previa de la Librería (`VistaPrevia(estado)`; el nombre exacto es del ADR), que aplica `Resolver` sobre una copia y devuelve **exactamente** la lista de eventos que devolvería `Resolver` (Librería, regla 9). La vista previa **no calcula ningún número del juego**: cada cifra que dibuja es un campo de un evento, salvo `exceso` (F3), que es la resta de dos campos del mismo evento (`neto − vidaPerdida`; decisión del 2026-11-26). Además de esos eventos, solo lee consultas puras de la Librería (`CubiertoPor`, `Coberturas()`, el tipo y el dueño de una pieza, la fase y el jugador del turno) y lo que le entregan Entrada (#13: la pieza elegida o en inspección), Ficha y HUD (#15: el ancla de la ficha, la tarjeta y la zona de avisos) y Localización (#14: los textos). Su estado propio (la última lista recibida, las placas visibles, el aviso activo) es del cliente: no entra en la forma canónica ni en la huella (Librería, regla 11).
2. **Cuándo se ve.** Solo durante la fase **`Atacar` del jugador del turno J**, en el cliente que tiene sus comandos habilitados (en local, la pantalla compartida). En cualquier otra fase o estado (`EsperandoTirada`, `Reclutar`, `Mover`, el turno del rival, `Terminada`) no hay placas y cada pieza muestra su ficha. Con la lista de asignaciones vacía, la vista previa no trae eventos de Resolución (Resolución, regla 2) y no se dibuja nada: las fichas se ven como siempre. *El turno del rival visto en el online* es una pregunta abierta (Open Questions 1); el MVP es local.
3. **Cuándo se recalcula.** Se pide una vista previa nueva, y se reemplaza entera la anterior:
   - al **abrirse `Atacar`** para J (después de `TerminarMovimiento`, de un `TurnTimeout` de mover o de un salto de fase de la Máquina; con Distracción, al abrirse el turno directo en `Atacar`). Al abrirse la fase la lista de asignaciones está vacía y la vista previa no trae nada (regla 2). La petición existe para que una partida **cargada** en medio de `Atacar`, con ataques ya asignados, muestre sus placas sin esperar otro comando;
   - después de **cada comando aceptado en `Atacar`** cuyo resultado incluye `AtaqueAsignado`, `AtaqueQuitado`, `CoberturaAsignada` o `CoberturaQuitada` (Alcance, regla 15; Cobertura, reglas 5 y 15): es decir, todo lo que cambia qué ataques hay;
   - después de una `Resincronizar` que deje la partida en `Atacar` (Librería, regla 14; online).
   Un comando **rechazado** no produce eventos ni recálculo. Un cambio de idioma **no** recalcula: solo vuelve a pedir los textos (regla 15).
4. **En el mismo cuadro.** Las placas de una lista nueva aparecen en el **mismo cuadro** que la flecha del comando que la causó (Alcance, Game Feel: la flecha aparece en el cuadro en que se acepta el comando), y en ese mismo cuadro desaparecen las placas que ya no corresponden y vuelven sus fichas (#15, regla 10). Nunca se ven a la vez placas de dos listas distintas, ni una placa al lado de una ficha sobre la misma pieza. El costo total entra en el cuadro (F4). Si en un mismo cuadro se aceptan varios comandos, se pide **una sola** vista previa, después del último, y se dibuja esa lista (decisión del 2026-11-26): con una por comando, tres comandos costarían 3 × 5 + 4 = 19 ms, más que el cuadro.

#### B. Qué se dibuja

5. **De evento a dibujo (tabla cerrada).** La vista previa dibuja solo estos eventos de la lista; todos los demás (`AtaqueEjecutado`, `TurnoTerminado`, `TurnoIniciado`, caducidades) no se dibujan, porque las flechas de los ataques ya están en pantalla (Alcance, Visual/Audio):

   | Evento de la lista | Qué dibuja la vista previa | Dónde |
   |---|---|---|
   | `DanoRecibido` | **Placa de pieza** (regla 7) | En el ancla de la ficha de `pieza` |
   | `PiezaMuerta` con motivo `Dano` | La placa de esa pieza pasa a **MUERE** (regla 8) | Ídem |
   | `PiezaMuerta` con motivo `Explosion` | **MUERE** con el ícono de área sobre el demoledor que explota, tenga o no `DanoRecibido` (regla 8) | Ancla del demoledor |
   | `DanoATramo` | **Placa de tramo** (regla 9) | Su casilla |
   | `TramoRoto` | La placa de ese tramo pasa a **ROTO** (regla 9) | Ídem |
   | `CoberturaQuitada` con motivo `EscuderoMuerto` o `AliadoMuerto` | La línea eslabonada de esa cobertura se dibuja **cortada** (regla 10) | Entre las dos piezas |
   | `DanoRecibido` con `brutoDesviado` > 0 | La línea eslabonada de esa cobertura se dibuja **resaltada** (regla 10) | Entre el escudero y el aliado |
   | Evento de fin de la Máquina (`PartidaTerminada`, con ganador y motivo) | **Marco de fin** en las placas MUERE de los Reyes y el **aviso de resultado** (regla 12) | Placas de los Reyes y zona de avisos |

   La vista previa no reordena, no filtra por lógica de juego ni completa nada que la lista no traiga: si la lista no tiene `DanoRecibido` para una pieza, esa pieza conserva su ficha.
6. **Todo receptor tiene placa (decisión del jugador, 2026-11-26).** La placa aparece sobre **cada pieza con `DanoRecibido`** y **cada tramo con `DanoATramo`**, sea o no la casilla apuntada por una flecha: también las vecinas del área de la catapulta, las de la explosión del demoledor, el escudero que absorbe golpes desviados y todo lo propio. Esto **amplía la regla 10 de Ficha y HUD (#15)**, que hoy cede el ancla solo al «objetivo de al menos una asignación» (cambio pedido en Cross-References). Una pieza cubierta cuyo golpe se desvió entero no tiene `DanoRecibido` (Resolución, regla 14), así que **conserva su ficha intacta**: lo que le iba a llegar se lee sobre su escudero (regla 10).
7. **Placa de pieza: contenido (decisión del jugador, 2026-11-26).** La placa ocupa el ancla de la ficha (centrada en el ancho de la casilla y pegada a la parte baja; #15, regla 3), tiene la misma forma (rectángulo de esquinas redondeadas, plano, fondo Tinta `#1E1A17` con la opacidad de la ficha, `ficha_opacidad_pct`) y mide como mucho lo mismo, 64 × 40 px (F1). Dos renglones, con números de 14 px o más, negrita, cifras de ancho fijo y borde Tinta de 2 px:
   - **Renglón A, el resultado:** ícono de tipo y `vidaAntes → vidaDespues` en verde vida `#8EDB6A` (por ejemplo, `25 → 12`). Si `neto` es 0 (el escudo frenó todo), solo `vidaAntes`, sin flecha: la vida no cambia.
   - **Renglón B, la causa:** ícono de **impacto** y el `bruto` en bermellón `#E55A40` · escudo heráldico y el `escudo` en acero `#7F9BB8`. Es la cuenta de la regla central puesta a la vista: el golpe sumado al lado del escudo que se le resta **una sola vez** (Resolución, regla 6). Con `neto` 0, el ícono de impacto y el `bruto` pasan al gris apagado `#A8A49B` (el golpe no pasó), y el escudo queda en acero.
   - El ícono de impacto **no es el puño** de la ficha: el puño dice «esta pieza pega tanto» y el impacto dice «esta pieza recibe tanto». Usar el mismo ícono confundiría el ataque de la pieza con el golpe que recibe. Es un ícono nuevo para el art bible (§7, Íconos).
   - Los valores son los del evento: `bruto`, `escudo` (el `escudo_ef` de la foto, con el +2 del arquero sobre su tramo), `vidaAntes`, `vidaDespues`. El Grito ya está dentro del `bruto` porque Resolución usa `ataque_ef` (Modificador, regla 7): la vista previa no suma bonos.
   - Con `bruto` de 100 o más, el renglón B muestra `99+` y omite el escudo, porque esa pieza muere seguro (F2). Con el Catálogo vigente, el caso aparece con combos muy grandes.
8. **MUERE.** Una pieza con `PiezaMuerta` en la lista muestra su placa en estado **MUERE**: el renglón A pasa a **calavera y la palabra MUERE** (`ui.vista_previa_muere`) y la figura se dibuja **desaturada** (art bible §4, *MUERE (vista previa)*: Tinta con texto `#F5F1E8` y borde bermellón de 1 px). El renglón B se mantiene, para que se vea por qué muere. MUERE se lee por la calavera, la palabra y la figura desaturada, nunca por el color (art bible §4, *Daltonismo*). Para el **demoledor que explota** (motivo `Explosion`), el renglón A lleva el ícono de **área** en lugar de la calavera, con la misma palabra: muere porque explota, no porque le pegaron. Si además recibe golpes, el renglón B muestra su `bruto` y su `escudo`; si no, el renglón B no se dibuja y la placa queda de un renglón.
9. **Placa de tramo y ROTO.** Un tramo no tiene ficha (#15, regla 9), así que su placa ocupa el mismo ancla que tendría la ficha (abajo y al centro de la casilla), salvo que haya una pieza en esa casilla (regla 11). Es de **un solo renglón**: ícono de **muro** y `vidaAntes → vidaDespues` en verde vida. El tramo no tiene escudo ni redirección, así que el golpe recibido es `vidaAntes − vidaDespues` y no se repite. Si el tramo tiene `TramoRoto` en la lista, la placa pasa a **ROTO**: grieta en zigzag y la palabra ROTO (`ui.vista_previa_roto`), y el tramo se dibuja **como hueco** (art bible §4, *ROTO (vista previa)*; Castillo, Visual/Audio). El exceso sobre un tramo no se muestra (Castillo, Edge Cases: «ROTO sin mostrar el exceso»).
10. **Cobertura: desvío y corte.** Las flechas que apuntan a una pieza cubierta por un escudero del rival **terminan en el escudero** (Cobertura, Visual/Audio y UI Requirements). Esa geometría es de la capa de flechas, que la deduce de `CubiertoPor`, no del daño. La vista previa agrega dos cosas:
    - Si un escudero tiene `DanoRecibido` con `brutoDesviado` > 0, la línea eslabonada de su cobertura se dibuja **resaltada** (trazo doble de grosor). El escudero lleva su placa normal con el `bruto` total. El aliado cubierto no lleva nada si no recibió golpes propios (regla 6). La parte desviada se lee en la tarjeta (regla 13).
    - Si la lista trae `CoberturaQuitada` (`EscuderoMuerto` o `AliadoMuerto`), la línea se dibuja **cortada**: el mismo trazo con un hueco en el centro. Le dice al jugador que, si resuelve así, la protección se rompe y el aliado queda expuesto el turno siguiente.
11. **Arquero sobre su tramo: dos placas.** En la casilla de un arquero sobre un tramo en pie, la pieza y el tramo reciben los dos del mismo golpe (Resolución, regla 5; Castillo, regla 8), y la lista trae `DanoRecibido` y `DanoATramo`. La placa de la pieza va en el ancla de la ficha (abajo) y la del tramo va **arriba de la casilla**, de un renglón; las dos entran en los 68 px de la casilla sin tocarse (F1). Si el tramo pasa a ROTO y el arquero sobrevive, la placa del arquero sigue mostrando el escudo de la foto (con el +2). Eso es lo que la resolución usará, aunque después de resolver el arquero quede como uno común (Castillo, regla 9).

#### C. Lo propio, el fin y los avisos

12. **Fuego amigo y fin de partida.**
    - **Lo propio.** Toda placa cuyo receptor es de **J** (una pieza de J o un tramo del castillo de J) lleva un **contorno discontinuo** de 2 px `#F5F1E8` alrededor de la placa. Es una señal de forma, no de color (art bible §4, *Daltonismo*; Alcance, UI Requirements: *«la vista previa marca todo lo propio que recibe daño»*). Las placas de las piezas del rival no lo llevan. En la resolución de J todos los golpes son de J (Resolución, regla 1), así que lo propio que recibe daño solo puede ser fuego amigo.
    - **Fin de partida.** Si la lista trae el evento de fin de la Máquina, las placas MUERE de los Reyes que caen llevan un **marco de fin**: un borde de 3 px `#F5F1E8` por fuera del borde bermellón. La zona de avisos muestra el **aviso de resultado** (regla 14) según el ganador y el motivo: `ReyMuerto` con ganador J → `ui.vista_previa_victoria` («Ganás la partida»); `ReyMuerto` con ganador K, cuando cae solo el Rey propio → `ui.vista_previa_derrota` («Tu Rey muere: perdés»); `DobleRey` (gana J; Máquina, regla 13) → `ui.vista_previa_doble_rey` («Caen los dos Reyes: ganás»).
    - **Sin confirmación (decisión del jugador, 2026-11-26).** Ni la derrota propia ni ningún otro resultado de la vista previa agrega un paso: Terminar turno sigue siendo un solo toque sin diálogo (#15, regla 14; pilar 4), y un `TurnTimeout` de atacar resuelve igual. La protección contra el error es que la derrota se vea antes, con la placa, el marco y el aviso, no un diálogo.
13. **La tarjeta: el desglose.** Durante `Atacar`, si la pieza **elegida o en inspección** (Entrada, regla 5) tiene `DanoRecibido` en la lista vigente, la tarjeta de la ficha extendida (#15, regla 8, margen izquierdo) agrega al final un bloque **Vista previa**. El contenido es de este GDD y el lugar es de #15 (cambio pedido en Cross-References). Una fila por dato, con ícono y número, y solo las que aplican:

    | Fila | Valor (campo del evento) | Se muestra |
    |---|---|---|
    | Golpe | `bruto` (sin tope de 99: la tarjeta tiene lugar para hasta 5 cifras) | Siempre |
    | Desviado | `brutoDesviado`, con el ícono de cobertura (eslabón) | Si es mayor que 0 |
    | Escudo | `escudo` | Siempre |
    | Pasa | `neto` | Siempre |
    | Vida | `vidaAntes → vidaDespues` | Siempre |
    | Sobra | `exceso` (F3), con el texto `ui.vista_previa_sobra` | Si es mayor que 0 |
    | Atacantes | `atacantesRivales`, con el ícono de flecha | Si es 2 o más |

    Con un **tramo** en inspección (#15, regla 9), el bloque tiene solo Golpe (`bruto`) y Vida. La fila **Sobra** es la que pidió Resolución (#9, UI Requirements: *«la vista previa marca el exceso perdido»*), y vale para toda pieza que muere, no solo para el escudero. Le dice al jugador cuánto golpe está desperdiciando sobre un objetivo ya muerto, para que lo mande a otro. No va en el tablero porque no cabe en la placa sin invadir la casilla (F1).
14. **Aviso de la vista previa.** La vista previa tiene **un solo aviso** a la vez en la zona de avisos de #15 (regla 18: banda superior, x 1170 a 1840). Es el primero de esta lista que se cumpla en la lista vigente:

    | Prioridad | Condición (en la lista vigente) | Clave |
    |---|---|---|
    | 1 | Fin con el Rey propio caído y ganador K (derrota propia) | `ui.vista_previa_derrota` |
    | 2 | Fin con ganador J (`ReyMuerto` o `DobleRey`) | `ui.vista_previa_victoria` o `ui.vista_previa_doble_rey` |
    | 3 | Algún `TramoRoto` de J con `tramosRotosDelDueno` ≥ `TRAMOS_ROTOS_CORTE` (2), cuando antes de esta lista J tenía menos de `TRAMOS_ROTOS_CORTE` tramos rotos (cortás tu propio reclutamiento; Castillo, regla 10). Una explosión puede llevar el contador de 1 a 3 (Resolución, regla 8): también cuenta (decisión del 2026-11-26) | `ui.vista_previa_corte_propio` |
    | 4 | El Rey propio tiene `DanoRecibido` con `vidaPerdida` > 0 y no muere | `ui.vista_previa_rey_herido` |
    | 5 | Algún `TramoRoto` de K con `tramosRotosDelDueno` ≥ `TRAMOS_ROTOS_CORTE` (2), cuando antes de esta lista K tenía menos de `TRAMOS_ROTOS_CORTE` tramos rotos (decisión del 2026-11-26) | `ui.vista_previa_corte_rival` («Castillo cortado») |

    El aviso de la vista previa es **persistente**: se ve mientras su condición se cumpla y se va en el mismo cuadro en que una lista nueva deja de cumplirla. Un aviso transitorio de Entrada (un rechazo) lo **tapa** durante su duración y, al terminar, el aviso de la vista previa vuelve si sigue vigente. Esto precisa la regla 18 de #15, que hoy dice que un aviso nuevo reemplaza al anterior (cambio pedido en Cross-References). Ningún aviso de la vista previa suena a error; el de la prioridad 1 sí tiene un cue propio de advertencia (Visual/Audio).
15. **Textos.** La vista previa solo lleva texto en MUERE, ROTO, los avisos de la regla 14, el título del bloque de la tarjeta y la fila Sobra. Todo lo demás es número, ícono o forma. Todas las claves son del grupo `ui` con el prefijo `ui.vista_previa_` (Localización, regla 4); se guarda la clave y no el texto, así que el cambio de idioma es instantáneo (Localización, regla 12). MUERE y ROTO tienen un presupuesto propio de **6 caracteres** (`presupuesto_placa`, F1). Los avisos usan `presupuesto_aviso` (60). La flecha `→` y el signo `−`, si se usa en la tarjeta, son símbolos sin idioma: piden entrar en la lista cerrada de la regla 1 de Localización, que hoy admite `:`, `+`, `×` y `/` (cambio pedido).
16. **Orden de capas.** De abajo arriba: terreno, figuras, marcas de alcance, flechas y líneas de cobertura, **placas de la vista previa**, placas MUERE y ROTO, marco de fin. Las placas tapan la parte de las flechas que pasa por debajo, nunca al revés: un número no puede quedar cruzado por un trazo (art bible §7, *Lo importante, lo más visible*: en `Atacar`, la vista previa y las placas MUERE y ROTO ganan a todo).
17. **Opciones del jugador.** Con «Ficha simple» encendida (#15, regla 7), las placas se ven **completas**, con los dos renglones: la vista previa de daño se ve siempre (art bible §7; pilar 1). «Reducir efectos» no cambia nada porque la vista previa no tiene pulsos ni destellos: aparece y desaparece en un cuadro.

#### D. Errores y límites

18. **Una lista que no es de `Atacar`.** Si la operación de vista previa devuelve un **código de rechazo** (lo hace en cualquier estado que no sea `Atacar` de J; Librería, regla 9 y Edge Cases), no se dibuja nada y cada pieza conserva su ficha. Si eso pasa en `Atacar` de J, es un error del cliente: en un build de desarrollo se registra. La vista previa **nunca** muestra un resultado estimado ni la última lista válida como si fuera la actual.
19. **Entrega a la cosecha.** Al aceptarse `Resolver` (o llegar `TurnTimeout` de atacar), las placas desaparecen **en el mismo cuadro** en que Render (#18) empieza la cosecha con los eventos reales. El número flotante de la cosecha es la vida que perdió el receptor, igual a `vidaAntes − vidaDespues` de su placa; si la placa decía MUERE o ROTO, en la cosecha aparece esa placa sin número (Resolución, Visual/Audio; decisión del 2026-11-26). Como la lista es idéntica (Librería, regla 9), no puede haber un número distinto. La vista previa no se queda en pantalla durante la animación, que nunca se puede saltar: un toque durante la cosecha se ignora (Render, #18; decisión del 2026-11-26).
20. **Lo que el jugador no puede hacer con la vista previa.** Ver el resultado de una jugada que no asignó (por ejemplo, el de un movimiento hipotético; Open Questions 2); ver qué le haría el rival en su turno siguiente (no hay «intenciones» del rival: su plan todavía no existe); ver un resultado distinto del que va a pasar; ocultar las placas mientras está en `Atacar`; pedir una confirmación extra.

### States and Transitions

**Estado de la capa** (por cliente):

| Estado | Qué significa | Qué se ve |
|---|---|---|
| **Inactiva** | Fuera de `Atacar` de J, o la partida terminó | Fichas normales; sin placas ni aviso de la vista previa |
| **Activa vacía** | `Atacar` de J con una lista sin eventos de Resolución (0 asignaciones, o asignaciones que no tocan a nadie: un área sobre casillas vacías) | Fichas normales; las flechas y el área de Alcance |
| **Activa con resultado** | `Atacar` de J con al menos un evento dibujable (regla 5) | Placas, líneas resaltadas o cortadas y el aviso de la regla 14, si hay |

| De → a | Disparador | Efecto |
|---|---|---|
| Inactiva → Activa vacía o con resultado | Se abre `Atacar` de J (regla 3) | Se pide la vista previa; se dibuja según la lista |
| Activa → Activa (cualquiera) | Comando aceptado con `AtaqueAsignado`, `AtaqueQuitado`, `CoberturaAsignada` o `CoberturaQuitada` | Se reemplaza la lista entera, en el mismo cuadro (regla 4) |
| Activa → la misma | Comando rechazado; cambio de idioma | Nada; con el idioma, solo cambian los textos |
| Activa → Inactiva | `Resolver` o `TurnTimeout` de atacar aceptados | Las placas se van en el cuadro en que empieza la cosecha (regla 19) |
| Activa → Inactiva | `Rendirse` o `CerrarPartida` durante `Atacar` | Las placas se van; los ataques no se resuelven (Alcance, States) |

**Estado de cada receptor en la lista vigente:**

| Estado | Condición (campos del evento) | Dibujo |
|---|---|---|
| Ficha | La pieza no tiene `DanoRecibido` (también el aliado cubierto con todo desviado) | Su ficha de #15 |
| Herida | `DanoRecibido` con `neto` > 0 y `vidaDespues` > 0 | Placa: `vidaAntes → vidaDespues`; impacto y escudo |
| Sin pérdida | `DanoRecibido` con `neto` = 0 | Placa: `vidaAntes`; impacto en gris y escudo |
| MUERE | `PiezaMuerta` (`Dano`) | Placa MUERE, figura desaturada |
| MUERE por explosión | `PiezaMuerta` (`Explosion`) | Placa MUERE con ícono de área |
| Tramo dañado | `DanoATramo` sin `TramoRoto` | Placa de tramo: `vidaAntes → vidaDespues` |
| Tramo ROTO | `TramoRoto` | Placa ROTO; tramo dibujado como hueco |

Cualquiera de los de pieza suma el contorno discontinuo si el dueño es J, y los Reyes en MUERE suman el marco de fin si la lista trae el evento de fin. No hay transiciones entre estados de receptor dentro de una lista: cada lista nueva vuelve a asignar el estado de cada receptor desde cero.

### Interactions with Other Systems

| Sistema | Qué entra | Qué sale | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | La operación de vista previa (regla 9), la lista de eventos (regla 8) y las consultas puras `CubiertoPor` y `Coberturas()` (regla 10) | Nada (no manda comandos) | Librería |
| #3 Máquina de turno | La fase, el jugador del turno y el evento de fin con ganador y motivo (reglas 13 y 16 de la Librería) | — | Máquina |
| #5 Castillo y muros | Por los eventos: `DanoATramo`, `TramoRoto`, `tramosRotosDelDueno`; la constante `TRAMOS_ROTOS_CORTE` (2) | — | Castillo (reglas); este GDD (dibujo) |
| #7 Alcance y asignación | Los eventos `AtaqueAsignado` y `AtaqueQuitado` como disparadores del recálculo; las flechas y el área ya dibujados | Nada | Alcance |
| #8 Cobertura del escudero | `CoberturaAsignada` y `CoberturaQuitada` como disparadores; `CubiertoPor` para la línea; `brutoDesviado` del evento | Línea resaltada o cortada | Cobertura (quién cubre); este GDD (cómo se marca el desvío en la vista previa) |
| #9 Resolución | La forma de los eventos (regla 14): `DanoRecibido`, `DanoATramo`, `TramoRoto`, `PiezaMuerta`, `CoberturaQuitada` | Confirmación de que los campos alcanzan (Dependencies) | Resolución (forma); este GDD (lectura) |
| #11 Modificador | Nada directo: el Grito ya está en el `bruto` del evento | — | Modificador |
| #13 Entrada y selección | La pieza elegida o en inspección (regla 5) y la aceptación de cada comando | — | Entrada |
| #14 Localización | Textos de las claves `ui.vista_previa_*` y el aviso de cambio de idioma | Las claves nuevas y su presupuesto | Localización (sistema); este GDD (claves) |
| #15 Ficha y HUD | El ancla de la ficha (regla 10), la tarjeta (regla 8) y la zona de avisos (regla 18) | Las placas en el ancla, el bloque de la tarjeta y el aviso persistente | #15 (lugares); este GDD (contenido) |
| #18 Render | — | El momento de la entrega (regla 19); la misma lista que la cosecha | Render dibuja la cosecha; este GDD, la previa |
| #22 Audio | — | Las transiciones a MUERE, ROTO y derrota propia como momentos de cue (Visual/Audio) | Audio |
| #23 Accesibilidad, #24 Tutorial | — | Las placas y avisos como contenido que se puede señalar | Cada GDD, solo lectura |
| #25 Rival IA | Nada de este GDD: la IA usa la operación de la Librería, no esta capa | — | Librería |

## Formulas

*Lente `systems-designer` (modo `full`, sin agente). Cada fórmula se probó con cada variable en su mínimo, su máximo y en 0.* La vista previa **no define ningún número de juego**: daño, escudo, vida y área vienen de los eventos de Resolución (#9), que a su vez leen el Catálogo (#2), Castillo (#5) y el Modificador (#11). Lo que este GDD define son reglas numéricas de **lectura y rendimiento**: cuánto cabe en una placa (F1), cuándo se puede abreviar un golpe sin esconder información (F2), cuánto daño se desperdicia (F3) y cuánto tarda en aparecer el resultado (F4). No hay lazos de retroalimentación: es una capa de lectura y ninguna fórmula alimenta al núcleo.

**Supuesto sin fuente:** el ancho de una cifra de 14 px y el de la flecha `→` no están en `design/art/art-bible.md` ni en `docs/engine-reference/unity/`. El art bible nombra la familia (tipo Barlow Condensed o Roboto Condensed), pero no elige una. Se usan **7 px** por carácter, igual que #15 (F1, `NOT SOURCEABLE`), y la flecha se cuenta como un carácter más. Se verifica con la fuente elegida (AC-3 de #15 y AC-14 de este GDD). En la placa, `vidaAntes→vidaDespues` va **sin espacios**; los ejemplos de texto de este documento llevan espacios solo para que se lean.

### F1. Dimensión de la placa

La fórmula `ancho_placa` se define así:

`ancho_A = icono_ficha_px + gap_icono_px + n_A × ancho_digito_px`
`ancho_B = icono_ficha_px + gap_icono_px + d_bruto × ancho_digito_px + gap_grupo_px + icono_ficha_px + gap_icono_px + d_escudo × ancho_digito_px` (con `bruto` ≥ 100: `icono_ficha_px + gap_icono_px + 3 × ancho_digito_px`, F2)
`ancho_placa = max(ancho_A, ancho_B) + 2 × margen_placa_px`
`alto_placa = renglones × alto_renglon_px + (renglones − 1) × gap_renglon_px + 2 × margen_placa_px`
`alto_casilla_doble = alto_placa(pieza, 2 renglones) + alto_placa(tramo, 1 renglón) + separacion_placas_px`

donde `n_A` es la cantidad de caracteres del renglón A: `d_antes + 1 + d_despues` (herida, o tramo dañado), `d_antes` (sin pérdida) o `n_texto` (MUERE o ROTO).

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| icono_ficha_px | int | 12 | constant (#15, F1) | Lado de los íconos dentro de la placa (tipo, impacto, escudo, muro, calavera, grieta, área) |
| gap_icono_px | int | 2 | constant (#15, F1) | Espacio entre un ícono y su número |
| gap_grupo_px | int | 4 | constant (#15, F1) | Espacio entre el grupo del impacto y el del escudo |
| ancho_digito_px | int | 7 | constant (#15, F1; **NOT SOURCEABLE**) | Ancho de una cifra o de la flecha |
| margen_placa_px | int | 2 | constant (#15, F1) | Aire entre el contenido y el borde |
| alto_renglon_px | int | 16 | constant (#15, F1) | Altura de un renglón de 14 px con borde |
| gap_renglon_px | int | 2 | constant (#15, F1) | Espacio entre los dos renglones |
| d_antes, d_despues | int | 1–2 | calculated (cifras de `vidaAntes` y `vidaDespues`) | Las cifras de la vida; #15 AC-4 garantiza 2 como máximo para las piezas |
| d_bruto | int | 1–2 (o el caso `99+`) | calculated (cifras de `bruto`) | Cifras del golpe; desde 100 se aplica F2 |
| d_escudo | int | 1–2 | calculated (cifras de `escudo`) | Cifras del escudo efectivo; #15 AC-4 |
| n_texto | int | 1–6 | data file (Localización; `presupuesto_placa` = 6) | Caracteres de MUERE o ROTO en el idioma activo |
| renglones | int | 1–2 | calculated | 2 para la placa de pieza; 1 para el tramo y para el demoledor que explota sin recibir golpe |
| separacion_placas_px | int | 2–10 | data file (este GDD; 2 propuesto) | Aire entre la placa del arquero y la de su tramo |
| casilla_px | int | 68 | data file (Tablero y art bible §7, a 1080p) | Lado de una casilla |

**Output Range:** `ancho_placa` de **39** a **64 px** y `alto_placa` de **20** (un renglón) a **38 px** (dos renglones). Límites: `ancho_placa` ≤ 64 y `alto_placa` ≤ 40, los mismos de la ficha (#15, regla 3), así que la placa ocupa exactamente el lugar que deja la ficha y nunca invade la casilla vecina. Para el arquero sobre su tramo, `alto_casilla_doble` ≤ `casilla_px`.

**Example:**
- Soldado herido (`25→12`, golpe 16, escudo 3): A = 12 + 2 + 5 × 7 = **49**; B = 12 + 2 + 14 + 4 + 12 + 2 + 7 = **53**; `ancho_placa` = 53 + 4 = **57 px**.
- Peor caso de pieza (golpe 99, escudo 10 o más): B = 12 + 2 + 14 + 4 + 12 + 2 + 14 = 60 → **64 px**. MUERE en 6 caracteres: A = 12 + 2 + 42 = 56; el ancho lo sigue dando B.
- Mínimo de dos renglones (sin pérdida, vida 9, golpe 2, escudo 5): A = 12 + 2 + 7 = 21; B = 12 + 2 + 7 + 4 + 12 + 2 + 7 = 46 → **50 px**.
- Un renglón: demoledor que explota sin recibir golpe (`MUERE`, 5 caracteres) → 12 + 2 + 35 + 4 = **53 px**; `ROTO` (4) → **46 px**; tramo de 3 cifras en el caso abreviado de abajo (`985`) → **39 px**, el menor ancho que producen las claves y valores previstos. Un texto de 1 carácter daría 25 px; ninguna clave lo tiene.
- Tramo (`30→14`): 12 + 2 + 35 + 4 = **53 px**, 20 de alto. ROTO de 6 caracteres: 12 + 2 + 42 + 4 = **60 px**.
- Arquero sobre su tramo: 38 + 20 + 2 = **60 px** de 68 (sobran 8).

*Resultado de los límites:*
- **`vida_tramo` de 3 o 4 cifras** (la config admite de 1 a 1.000; Castillo): `1000→985` son 8 caracteres, y el renglón queda en 12 + 2 + 56 + 4 = **74 px**, más que la casilla. **No se corrige en silencio:** cuando `ancho_A` del tramo pasa de 60, la placa muestra solo `vidaDespues` (`985`: 12 + 2 + 21 + 4 = 39 px), y el número exacto de antes y después sigue en la tarjeta del tramo (regla 13). Con el valor vigente (30) no pasa.
- **`n_texto` mayor que 6** (una traducción larga de MUERE o ROTO): con 7 caracteres, A = 12 + 2 + 49 = 63 y la placa llega a 67 px. Se bloquea en origen con el presupuesto `presupuesto_placa` = 6 de Localización (F4 de #14), que falla la verificación si una traducción se pasa. No se trunca. Hoy: `MUERE` (5) y `ROTO` (4) en español; en inglés, `DIES` (4) y `BROKEN` (6), a confirmar por Localización.
- **Con `separacion_placas_px` = 10**: 38 + 20 + 10 = 68, justo la casilla. Por eso 10 es el techo.
- **Con 0 en cada variable calculada:** `d_*` = 0 no existe (un número siempre tiene al menos una cifra), `bruto` = 0 no tiene evento (Resolución, regla 14) y `vidaAntes` = 0 no existe (una pieza viva tiene vida ≥ 1; Catálogo). `vidaDespues` = 0 es MUERE, no `25→0`. `escudo` = 0 se muestra como `0`.

### F2. Golpe abreviado sin esconder información

La regla `golpe_abreviado` se define así:

`golpe_abreviado = (bruto ≥ bruto_tope_mostrado + 1)` y, como condición del Catálogo que lo hace seguro, `invariante_muerte_segura = (max_t (vida_t + escudo_ef_max_t) ≤ bruto_tope_mostrado + 1)`, donde `t` recorre los tipos del Catálogo y `escudo_ef_max_t` es `escudo_t` más `bono_escudo_muro` solo si `t` es el arquero (forma por tipo; decisión del 2026-11-26)

Con `golpe_abreviado`, el renglón B muestra `99+` y omite el escudo.

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| bruto | int | 1–61.610 | calculated (`DanoRecibido.bruto`; cota de Resolución, F1 y F2) | Golpe sumado sobre el receptor |
| bruto_tope_mostrado | int | 99 | constant (sale de las 2 cifras de la placa, F1) | El mayor golpe que se muestra con su número |
| vida_t | int | 1–99 | data file (`vida` del tipo `t` en el Catálogo) | Vida del tipo `t` (hoy, Rey 30; arquero 15) |
| escudo_ef_max_t | int | 0–99 | calculated (`escudo` del tipo `t` del Catálogo, más `bono_escudo_muro` de Castillo solo para el arquero) | El mayor escudo efectivo del tipo `t` (hoy, Rey 5; arquero 2 + 2 = 4) |

**Output Range:** verdadero o falso. Si `invariante_muerte_segura` vale, todo `golpe_abreviado` es una muerte: `neto = bruto − escudo ≥ 100 − escudo_ef_max_t ≥ vida_t ≥ vida`, con `t` el tipo del receptor, así que el receptor está en MUERE y no hay ningún número de vida ni de escudo que el jugador necesite para decidir. Abreviar no esconde nada (pilar 1). Lo que se pierde es cuánto sobra, y eso sigue en la tarjeta (F3).
**Example:** dieciséis arqueros con Grito sobre un soldado: `bruto` = 16 × 8 = 128 → `99+`, MUERE. Invariante hoy: el máximo es el Rey, 30 + 5 = 35 ≤ 100 (el arquero, con el +2 del muro, da 15 + 4 = 19). Holgura de 65.

*Resultado de los límites:* si un cambio del Catálogo o del bono del muro llevara `vida_t + escudo_ef_max_t` de algún tipo por encima de 100 (por ejemplo, vida 99 y escudo 7), un `99+` podría caer sobre una pieza que sobrevive. Mostrar `99+` y el escudo ocuparía 12 + 2 + 21 + 4 + 12 + 2 + 14 + 4 = **71 px**, más que la casilla. **No se resuelve en silencio:** un test del repositorio verifica el invariante sobre el Catálogo y los bonos (AC-11), igual que #15 verifica las 2 cifras (su AC-4), y queda como restricción para el Catálogo (Dependencies). Alternativas si algún día hace falta: (a) subir `casilla_px` (zoom móvil, #41) o (b) pasar el escudo al renglón A cuando el golpe se abrevia.

### F3. Golpe que sobra

La fórmula `exceso` se define así:

`exceso = neto − vidaPerdida`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| neto | int | 0–61.610 | calculated (`DanoRecibido.neto`; Resolución, F3) | Golpe que pasó el escudo |
| vidaPerdida | int | 0–`vidaAntes` | calculated (`DanoRecibido.vidaPerdida`; Resolución, F4) | Vida que el receptor pierde de verdad |

**Output Range:** de 0 a `neto − 1`. Vale 0 para todo receptor que sobrevive (`neto ≤ vidaAntes`, así que `vidaPerdida = neto`) y para el que muere justo. Solo se muestra si es mayor que 0 (regla 13).
**Example:** un escudero (vida 20, escudo 5) que absorbe 30 de golpe desviado: `neto` = 25, `vidaPerdida` = 20, `exceso` = **5**. Son 5 puntos que no llegan al aliado (Resolución, regla 7) ni a nadie: la fila Sobra le dice al jugador que puede sacar un arquero de ese blanco. Un soldado que recibe 16: `neto` = 13 ≤ 25 → `exceso` = 0, sin fila.

*Límites:* con `neto` = 0, `vidaPerdida` = 0 y `exceso` = 0. No hay división ni resta que dé negativo, porque `vidaPerdida = min(neto, vidaAntes)` (Resolución, F3 y F4). Es una resta de dos campos del mismo evento, no un segundo cálculo del daño.

### F4. Tiempo hasta que se ve el resultado

La fórmula `tiempo_resultado` se define así:

`tiempo_resultado = t_vista_previa + t_dibujo(n_placas) ≤ cuadro_ms`

**Variables:**

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| t_vista_previa | float (ms) | 0–5 (p95) | constant (Librería, *Constantes del contrato*, `vista_previa_presupuesto_ms` = 5; AC-24) | Lo que tarda la operación de la Librería |
| t_dibujo | float (ms) | 0–4 (p95) | data file (`vista_previa_dibujo_ms` = 4; este GDD, propuesto) | Lo que tarda el cliente en reemplazar las placas de la lista anterior por las nuevas |
| n_placas | int | 0–154 | calculated (`DanoRecibido` + `DanoATramo` de la lista: como mucho 122 piezas y 32 tramos; Resolución, regla 15) | Cuántas placas hay que dibujar |
| cuadro_ms | float (ms) | 16,6 | constant (60 fps; Librería, Tuning Knobs) | El cuadro en que la flecha y las placas tienen que aparecer juntas |

**Output Range:** de ~0 ms (lista vacía) a **9 ms** en el peor caso medido (5 + 4; la suma de dos p95 no es el p95 de la suma, así que el límite que vale es el de `t_vista_previa + t_dibujo` medido junto, AC-21), lo que deja 7,6 ms del cuadro para el resto del dibujo. Con eso, la regla 4 (placas en el mismo cuadro que la flecha) se cumple a 60 fps.
**Example:** el fixture del peor tablero de la Librería (AC-24: 122 piezas, 32 tramos, 61 ataques) da `t_vista_previa` ≤ 5. Si las 61 asignaciones tocan 40 receptores, `t_dibujo(40)` tiene que entrar en 4 ms, y en 4 ms también con 154 (AC-21).

*Límites:* con `n_placas` = 0 el dibujo solo retira las placas anteriores. Las placas se reutilizan (pool) y reemplazar la lista no reserva memoria, para que los 4 ms de AC-21 no dependan del recolector de basura en web. Con 154 es el techo teórico (todo el ejército y todos los muros con golpe a la vez) y define el fixture de AC-21. Si `t_dibujo` no entrara en 4 ms, la regla 4 cae primero en el cliente más lento (web en un equipo modesto). Una salida es dibujar las placas en el cuadro siguiente y la flecha en el actual, pero eso deja un cuadro con la flecha sin placa: **no se hace sin decisión del jugador** (Open Questions 6).

## Edge Cases

*Lente `systems-designer` aplicada a las secciones C y D releídas del archivo: cero, máximo, dos reglas a la vez, orden de los eventos y los límites de los GDD vecinos que este sistema podría romper.*

### Lista y fases

- **If no hay ataques asignados:** la lista no trae eventos de Resolución, no hay placas ni aviso y las fichas se ven normales (regla 2). Terminar turno resuelve sin golpes (Resolución, regla 2).
- **If los ataques asignados no tocan a nadie** (un área sobre casillas vacías o huecos sin pieza): la lista no tiene `DanoRecibido` ni `DanoATramo`. Se ven la flecha y el anillo del área (Alcance), sin placas: la mesa dice «este disparo no le pega a nadie».
- **If un comando se rechaza** (por ejemplo, `FueraDeAlcance`): no hay eventos ni recálculo. Las placas siguen las de la lista anterior y el aviso de rechazo tapa al de la vista previa durante su duración (regla 14).
- **If la vista previa devuelve un código de rechazo en `Atacar` de J:** no se dibuja nada y se registra el error en un build de desarrollo (regla 18). Nunca se muestra la lista anterior como si fuera la actual.
- **If se acaba el timer de atacar:** el `TurnTimeout` resuelve lo asignado, las placas se van en el cuadro en que empieza la cosecha y la cosecha muestra los mismos números (regla 19). No hay un cuadro con las placas sobre el estado ya resuelto.
- **If se rinde o se cierra la partida en `Atacar`:** las placas se van sin cosecha (los ataques no se resuelven; Alcance, States).
- **If se carga una partida guardada en medio de `Atacar`, con ataques asignados:** la vista previa se pide al abrir esa fase en el cliente y las placas aparecen sin esperar otro comando (regla 3).
- **If el jugador cambia de idioma con placas a la vista:** cambian MUERE, ROTO, el aviso y las filas de la tarjeta en el mismo cuadro, sin recalcular (regla 15; Localización, regla 12).

### Receptores y geometría

- **If la explosión de un demoledor no alcanza a nadie:** la lista trae solo `AtaqueEjecutado` y `PiezaMuerta` (`Explosion`). El demoledor muestra MUERE con el ícono de área en una placa de un renglón (regla 8). El sacrificio también se ve antes.
- **If el demoledor que explota además recibe golpes** (por ejemplo, de su propia catapulta): su placa es MUERE con el ícono de área y un renglón B con su `bruto` y su `escudo`. El motivo de la muerte sigue siendo `Explosion` (Resolución, regla 9).
- **If una pieza cubierta recibe solo golpes desviados:** no tiene `DanoRecibido`, conserva su ficha intacta y la placa está en su escudero, con la línea resaltada (reglas 6 y 10). **If además recibe fuego amigo** (el área propia la toca): tiene su propia placa con el contorno discontinuo y solo ese golpe. Lo rival sigue en el escudero.
- **If el escudero muere y el golpe sobre él supera su vida:** la placa del escudero es MUERE, el aliado sigue intacto, la línea se ve **cortada** (`CoberturaQuitada`, `EscuderoMuerto`) y la tarjeta del escudero muestra la fila Sobra (F3).
- **If muere el aliado cubierto y no su escudero:** con una cobertura **de K** no puede pasar en la resolución de J, porque todo golpe de J sobre ese aliado se desvía al escudero (Cobertura, F2; Resolución, regla 7). Pasa con una cobertura **de J**: el área o la explosión propias matan al aliado que cubre un escudero de J (el fuego amigo no se desvía). La lista trae `CoberturaQuitada` con `AliadoMuerto`, el aliado muestra MUERE con contorno discontinuo y la línea eslabonada de J se dibuja cortada.
- **If un arquero sobre su tramo recibe un golpe:** dos placas en la casilla, la de la pieza abajo y la del tramo arriba (regla 11; F1: 60 de 68 px). **If el tramo pasa a ROTO y el arquero sobrevive:** la placa del arquero muestra el escudo con el +2 de la foto, porque es el que la resolución usa (Resolución, regla 3).
- **If un tramo ya está roto:** no recibe golpe ni tiene placa (Resolución, regla 3). La pieza que esté parada en el hueco tiene su placa normal.
- **If un tramo propio de J recibe golpe** (catapulta o demoledor propios): placa con contorno discontinuo. **If con eso J pasa a 2 tramos rotos o más** (una explosión puede llevarlo de 1 a 3): aviso de la prioridad 3 (corte propio), por encima del aviso del Rey propio herido (regla 14).
- **If un mismo golpe lleva el castillo rival a 2 tramos rotos y además mata al Rey rival:** gana la prioridad 2 (victoria). El corte rival no se avisa porque la partida terminaría.
- **If el golpe sobre una pieza es de 100 o más:** el renglón B muestra `99+` sin escudo. La placa ya está en MUERE (F2), y la tarjeta muestra el golpe completo y lo que sobra.
- **If una placa cae en una casilla del borde del tablero:** ocupa el mismo ancla que la ficha, dentro de la casilla, y no sale del tablero.
- **If la flecha de un atacante pasa por encima de la placa de otra pieza:** la placa va encima de la flecha (regla 16). La flecha se interrumpe bajo la placa, que sigue legible.

### Fin de partida

- **If muere solo el Rey rival:** placa MUERE con marco de fin sobre él y aviso `Ganás la partida` (prioridad 2).
- **If muere solo el Rey propio** (área o explosión propias; Alcance, Edge Cases): placa MUERE con contorno discontinuo y marco de fin, y aviso `Tu Rey muere: perdés` (prioridad 1). **No hay confirmación** (decisión del jugador, 2026-11-26): un toque en Terminar turno o el fin del timer resuelven igual.
- **If mueren los dos Reyes:** dos placas MUERE con marco de fin (la propia, además, con contorno discontinuo) y aviso `Caen los dos Reyes: ganás`, porque `DobleRey` da la victoria al jugador del turno (Máquina, regla 13).
- **If el Rey propio pierde vida y no muere:** aviso de la prioridad 4 (`Tu Rey recibe daño`), salvo que haya uno de mayor prioridad.

### Rendimiento y carga

- **If 61 asignaciones tocan 154 receptores a la vez:** el dibujo tiene que entrar en `vista_previa_dibujo_ms` (F4; AC-21). Si un cliente no llega, es una falla de rendimiento a medir, no una licencia para saltear placas o mostrarlas en otro cuadro (Open Questions 6).
- **If el jugador traza flechas más rápido de lo que se dibuja:** cada comando aceptado reemplaza la lista entera, y con varios en un cuadro se pide una sola vista previa, después del último (regla 4). Se dibuja siempre la **última**, y nunca una mezcla de dos (regla 4). La Librería ya acepta y valida los comandos en orden, uno por vez.
- **If una traducción de MUERE o ROTO pasa de 6 caracteres:** falla la verificación de largos de Localización (F4 de #14) antes de publicar. La placa no trunca.

### Estrategias degeneradas revisadas

- *Probar combinaciones con la vista previa hasta encontrar la mejor:* es el uso buscado (pilar 1). Lo acota el timer de atacar (pilar 4), y cada intento cuesta solo un toque.
- *Usar la vista previa para leer el plan del rival:* no hay plan del rival que leer. La vista previa solo resuelve los ataques de J (regla 20), y las coberturas del rival ya son públicas (anti-pilar «NOT información oculta al resolver»).
- *Asignar y quitar en bucle para saturar el cliente:* cada comando cuesta lo mismo que uno normal (`comando_presupuesto_ms` más F4) y no cambia el estado de juego. Fuera de `Atacar` no hay vista previa.

## Dependencies

**De qué depende (aguas arriba):**

| Sistema | Tipo | Interfaz | ¿El otro GDD nombra a #16? |
|---|---|---|---|
| #1 Librería de reglas | Dura | La operación de vista previa = `Resolver` sobre una copia (regla 9); la lista de eventos (regla 8); `CubiertoPor` y `Coberturas()` (regla 10); `vista_previa_presupuesto_ms` | Sí (reglas 9 y 13, UI Requirements, Dependencies) |
| #9 Resolución simultánea del daño | Dura | La forma de los eventos de la regla 14: `DanoRecibido` (`bruto`, `brutoDesviado`, `escudo`, `neto`, `vidaAntes`, `vidaDespues`, `vidaPerdida`, `atacantesRivales`), `DanoATramo`, `TramoRoto` (`tramosRotosDelDueno`), `PiezaMuerta` (`motivo`), `CoberturaQuitada` | Sí («Pendiente (sin GDD)» → pasa a «Existe»; cambio pedido) |
| #7 Alcance y asignación de ataques | Dura | `AtaqueAsignado` y `AtaqueQuitado` disparan el recálculo; las flechas y el área ya dibujados | Sí (Interactions, Dependencies, UI Requirements) |
| #8 Cobertura del escudero | Dura | `CoberturaAsignada` y `CoberturaQuitada` disparan el recálculo; la línea eslabonada y la regla de la flecha que termina en el escudero | Sí (Dependencies, UI Requirements, AC-28) |
| #15 Ficha y HUD | Dura | El ancla de la ficha (regla 10), la tarjeta de la ficha extendida (regla 8), la zona de avisos (regla 18) y las medidas de F1 | Sí («Pendiente» → «Existe»; reglas 10 y 18 a ajustar; cambio pedido) |
| #14 Localización | Dura | Claves `ui.vista_previa_*`, presupuestos de largo, aviso de cambio de idioma, símbolos sin idioma | Sí («#16 pendiente» → claves definidas; cambio pedido) |
| #13 Entrada y selección | Dura | La pieza elegida o en inspección (regla 5); la aceptación de cada comando | Sí (Interactions, Dependencies, AC-36) |
| #3 Máquina de turno | Dura | La fase `Atacar`, el jugador del turno y el evento de fin con ganador y motivo (regla 13) | Indirecto (por la Librería); no hace falta cambio |
| #5 Castillo y muros | Blanda | Por los eventos: daño, ROTO y `tramosRotosDelDueno`; la constante `TRAMOS_ROTOS_CORTE` para el aviso de corte | Sí (Dependencies, Visual/Audio, UI Requirements) |
| #11 Modificador | Blanda | Ninguna directa: el Grito ya viene dentro del `bruto` | Sí (Dependencies, UI Requirements, AC-31) |
| #2 Catálogo de piezas | Blanda | Restricción: para cada tipo, `vida_t + escudo_ef_max_t ≤ 100`, con el +2 del muro solo en el arquero (F2; hoy el máximo es el Rey, 30 + 5 = 35) | Restricción nueva (cambio pedido) |
| #4 Tablero | Blanda | Geometría de la casilla (68 px) y su conversión a pantalla | Sí (Interactions) |

**Quién depende de este sistema (aguas abajo):**

| Sistema | Tipo | Qué usa |
|---|---|---|
| #18 Render, animación y feedback de combate | Blanda | El momento de la entrega (regla 19): la cosecha arranca en el cuadro en que se van las placas, con los mismos números. El índice hoy no lista a #16 entre las dependencias de #18, #22 ni #23 (cambio pedido) |
| #22 Audio | Blanda | Las transiciones a MUERE, ROTO y derrota propia como momentos de cue |
| #23 Accesibilidad | Blanda | La lista de marcas por forma (Visual/Audio) |
| #24 Tutorial básico | Blanda | Las placas y avisos como contenido que se señala («mirá: el escudo frena 3») |
| #19 Métricas locales | Ninguna obligatoria | Si mide uso de la vista previa, lo hace con eventos del cliente que este GDD no define (Open Questions 5) |

**No depende ni dependen de este sistema:** Movimiento (#6), Reclutamiento (#10), Timer (#12: solo manda `TurnTimeout`, que la vista previa ve como cambio de fase), Interfaz del Modificador (#17: no abre nada en `Atacar` salvo el «?»). **La Rival IA (#25) no depende de este GDD**: usa la operación de vista previa de la Librería, no esta capa de dibujo. Hoy el índice lista a #16 como dependencia del Tutorial (#24) y no de la IA, y está bien así.

**Supuestos sobre Render (#18): confirmados** al cerrar la tanda 4 (2026-11-26): la cosecha empieza en el cuadro en que se van las placas, el número flotante es la vida perdida del receptor y no aparece sobre MUERE ni ROTO (decisión del 2026-11-26, que reemplaza a «neto de la pieza, bruto del tramo»), y las placas anclan con `casillaEnPantalla` (#18, F3). Sobre Audio (#22), sin GDD: los cues de la sección Visual/Audio.

## Tuning Knobs

*Lente `systems-designer`: cada variable marcada `data file` en Formulas es una perilla; las `constant` no lo son. Las perillas de otro GDD se nombran, no se duplican.*

| Perilla | Valor | Rango seguro | Demasiado alta | Demasiado baja | Dueño |
|---|---|---|---|---|---|
| `presupuesto_placa` (caracteres de MUERE y ROTO) | 6 | 6 (piso atado a `BROKEN`; bajaría a 4 solo si cambia el inglés; decisión del 2026-11-26) | Con 7 la placa pasa de 64 px y toca la casilla vecina (F1) | El piso efectivo es el largo de la traducción más larga (hoy `BROKEN`, 6): con menos, no entra en inglés; con menos de 4 tampoco entra `ROTO` en español | Localización (#14), pedido por #16 |
| `vista_previa_dibujo_ms` (p95) | 4 | 2–7 | Con más de 7,6 la flecha y las placas no entran juntas en el cuadro de 16,6 ms con los 5 de la Librería (F4) | Con menos de 2 se exige más de lo que la plataforma web garantiza para 154 placas; no da nada visible | Este GDD |
| `separacion_placas_px` (arquero sobre su tramo) | 2 | 2–10 | Con 10 las dos placas llenan la casilla y tapan la figura entera (F1) | Con 0 las dos placas se tocan y se leen como una | Este GDD |
| `bruto_tope_mostrado` | 99 | **No es perilla:** sale de las 2 cifras de la placa (F1) y del invariante de F2 | — | — | — |

**Perillas ajenas que cambian lo que muestra la vista previa** (no se duplican acá):

- `ficha_opacidad_pct` (96, #15) y las medidas de la ficha (#15, F1): la placa usa las mismas. Cambiarlas en #15 cambia la placa.
- `casilla_px` (68, Tablero y art bible §7): todo F1 se mide contra la casilla.
- `presupuesto_aviso` (60, Localización): largo de los avisos de la regla 14.
- `TRAMOS_ROTOS_CORTE` (2, Castillo, regla 14: constante del contrato, no perilla): define cuándo aparecen los avisos de corte.
- `vida_tramo` (30, Castillo): con 100 o más, la placa del tramo se abrevia (F1).
- Las estadísticas del Catálogo y el bono del muro: el invariante de F2 las acota (para cada tipo, `vida_t + escudo_ef_max_t ≤ 100`; el +2 del muro cuenta solo para el arquero).

**Interacciones entre perillas:** `vista_previa_dibujo_ms` comparte el cuadro con `vista_previa_presupuesto_ms` (5 ms), que **no es perilla**: es una constante del contrato de la Librería (#1, *Constantes del contrato*) y cambiarla es una decisión de UX (decisión del 2026-11-26). Si pasara de 12,6 ms, ya no entraría en el cuadro ni con `vista_previa_dibujo_ms` = 4 (F4). `separacion_placas_px` y `casilla_px` también: con un zoom móvil que achique la casilla (#41), el techo de la separación baja.

**Reglas fijas (no son perillas):** que la vista previa sea la misma operación que `Resolver` (Librería, regla 9); que la placa ocupe el ancla de la ficha y nunca vaya al lado (art bible §7); que no haya confirmación (decisión del jugador, 2026-11-26); que todo receptor tenga placa (decisión del jugador, 2026-11-26).

## Visual/Audio Requirements

*Lente `art-director` (modo `full`, sin agente), con `design/art/art-bible.md` §1–4, §7 y §9 leídas por sección.* La vista previa vive en el estado **«Asignar ataques»** del art bible (§2): *afilado, enfocado, previsor, contenido*; terreno un 20% más oscuro, viñeta media y contraste de la capa del sistema al máximo; *«las flechas convergen y la vista previa de daño queda sobre cada objetivo. Las placas MUERE y ROTO son lo más contrastado de la pantalla»*. El oscurecimiento del terreno es del estado de la fase (Render, #18). La vista previa solo pone sus placas encima.

**Principios del art bible que más aplican:**
- *Regla visual (orden de lectura)* y §7 *Lo importante, lo más visible:* en `Atacar` la vista previa y las placas MUERE y ROTO ganan a todo. Por eso van arriba de las flechas (regla 16) y en el lugar de la ficha, no al lado.
- *Principio 1, «El plan se lee antes que el detalle»* (pilar 1): la placa dice el resultado en el renglón A y la causa en el B. El detalle (desviado, sobra, atacantes) va a la tarjeta.
- *Principio 2, «El combo se ve como un solo golpe»* (pilar 2): en la vista previa el combo lo llevan las flechas que se funden (×1 / ×1,5 / ×2; §4 *Flechas de ataque*) y **un solo número de golpe** por receptor, nunca uno por atacante. *«Cuando es ambiguo si resaltar cada golpe o el conjunto, gana el conjunto.»*
- §3 *Gramática de formas de la interfaz:* placa plana de esquinas redondeadas, la forma de la ficha; MUERE con calavera y texto; ROTO con grieta en zigzag y texto.
- §4 *Colores semánticos:* vida, ataque y escudo con su único significado. El bermellón del impacto es el del ataque, porque lo que se recibe es ataque. Hueso y Ciruela no aparecen en la placa (exclusivos del bando).
- §9 *Into the Breach:* *«el daño previsto reemplaza a la ficha del objetivo y las amenazas se leen como una capa limpia por encima del mundo»*. Se evita su neón y sus paneles laterales.

### Feedback por elemento

| Elemento | Visual (dirección) | Sonido (dirección; la especificación es de #22) |
|---|---|---|
| Placa de pieza herida | La forma de la ficha. A: tipo y `25→12` en verde vida. B: impacto y golpe en bermellón, escudo en acero. Aparece en el mismo cuadro que la flecha, **sin animación de entrada** | Ninguno propio: el trazo de la flecha ya suena (Alcance) |
| Placa sin pérdida (`neto` 0) | Como la herida, pero A muestra solo la vida y el impacto queda en gris `#A8A49B`. Se lee «rebota» por la forma (sin flecha en A, impacto apagado) | Ninguno |
| MUERE | Calavera y `MUERE` en `#F5F1E8` sobre Tinta, borde bermellón de 1 px, figura desaturada (§4). Lo más contrastado de la pantalla | **Cue de MUERE previsto**, una vez cuando un receptor **pasa** a MUERE con un comando (no se repite si sigue en MUERE). Pedido del art bible §4: *«una señal distinta para MUERE y ROTO al confirmar la vista previa»* |
| MUERE por explosión | Ícono de área en lugar de la calavera, la misma palabra y borde | El mismo cue de MUERE |
| Placa de tramo | Un renglón: muro y `30→14`. El tramo sigue dibujado entero o mellado según su estado real (Castillo) | Ninguno |
| ROTO | Grieta en zigzag y `ROTO`, y el tramo dibujado **como hueco** con escombro bajo (§3, §4) | **Cue de ROTO previsto**, una vez por transición, distinto del de MUERE |
| Fuego amigo | **Contorno discontinuo** de 2 px `#F5F1E8` alrededor de la placa. Forma, no color (§4 *Daltonismo*). **Falta en el art bible** (también lo pidió Alcance) | El aviso discreto de Alcance, una vez por asignación |
| Desvío (escudero con `brutoDesviado` > 0) | Línea eslabonada en acero, **resaltada** con trazo doble. La flecha termina en el escudero (Cobertura). **Falta en el art bible** (también lo pidió Cobertura) | El golpe de escudo de Cobertura (Visual/Audio) |
| Cobertura que se rompería | Línea eslabonada **cortada** (hueco en el centro). **Falta en el art bible** | Ninguno |
| Marco de fin | Borde de 3 px `#F5F1E8` por fuera del borde bermellón de MUERE, en los Reyes que caen. **Falta en el art bible** | Con victoria, ninguno extra (el de MUERE ya suena). Con **derrota propia**, un **cue de advertencia** propio, con prioridad sobre cualquier otro de la fase |
| Aviso de la vista previa | En la zona de avisos de #15, con ícono además del texto y nunca solo en rojo (#15, regla 18) | Solo el de la derrota propia |

### Restricciones

- **Nada por color solo.** MUERE (calavera, palabra, figura desaturada), ROTO (grieta, palabra, hueco), sin pérdida (impacto gris y sin flecha), fuego amigo (contorno discontinuo), desvío (trazo doble) y corte (hueco en la línea) se distinguen en grises y con los tres filtros de daltonismo (AC-17).
- **Sin animación.** Las placas aparecen y se van en un cuadro. No hay conteo de números, pulso ni destello: la vista previa cambia con cada flecha y una animación haría que la lectura llegue tarde (art bible §7, *Animación de la interfaz*: en la resolución, los números cambian de una vez).
- **Capa del sistema.** Las placas, los íconos y los marcos se dibujan con los activos del sistema. Ninguna skin los toca (anti-pilar «NOT pagar para ganar»; #15, regla 2).
- **Los números de la cosecha son los de la placa** (regla 19): Render no puede mostrar un número distinto.
- **Audio:** los cues de MUERE y ROTO previstos no se apilan. Si un comando hace pasar a MUERE a tres piezas a la vez, suena **uno**, más pleno, igual que el golpe combinado de la cosecha (pilar 2). El de derrota propia gana la mezcla. La columna de audio es dirección: no pasó por la lente de `audio-director`.

**Pedidos al art bible** (sin aplicar; Cross-References): ícono de **impacto** (golpe recibido, distinto del puño), **contorno discontinuo** de lo propio, **marco de fin**, **línea de cobertura resaltada y cortada**, y la placa de **un renglón** (tramo, demoledor que explota) como variante de la ficha.

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:vista-previa-de-ataques` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **La mesa contesta en el mismo cuadro.** La flecha y las placas aparecen juntas (regla 4; F4: hasta 9 ms de los 16,6 del cuadro). Si el resultado llegara un cuadro después de la flecha, el jugador miraría la flecha, después la placa, y la fase perdería su ritmo de «trazo y leo».
- **Probar no cuesta.** Asignar, reemplazar y quitar son un toque cada uno (Alcance, Game Feel) y la vista previa cambia con cada uno. No hay que pedirla, mantener presionado ni pasar el mouse.
- **El cambio importante se nota sin buscarlo.** Al pasar a MUERE o ROTO hay un cue y el contraste más alto de la pantalla. Al ganar o perder la partida, el marco y el aviso. El resto de las placas cambia en silencio.
- **Peso del combo antes de resolver (pilar 2).** Cada atacante que se suma se ve en dos lugares a la vez: la punta de la flecha más gruesa y el número de golpe que sube al lado de un escudo que no cambia.
- **Coincidencia total con la cosecha (pilar 1).** Ningún número, MUERE o ROTO de la cosecha difiere de lo que mostró la vista previa (Resolución, Game Feel). Es lo que hace que el daño fijo se perciba como fijo.
- **Sin frenos.** No hay diálogo, confirmación ni animación que demore Terminar turno (pilar 4). La derrota propia se avisa, no se bloquea.
- **Verificación:** capturas retenidas de AC-15 a AC-18 y playtest con tres preguntas: *«¿alguna vez lo que pasó al resolver no coincidió con lo que viste antes?»*, *«¿algo propio recibió daño sin que lo esperaras?»* (la de Alcance) y *«¿entendiste adónde fue el golpe que apuntaste a la pieza cubierta?»* (la de Resolución y Cobertura).

## UI Requirements

La vista previa no tiene pantalla propia: vive en el tablero y en dos lugares que le presta #15.

- **Sobre el tablero:** la placa de cada receptor en el ancla de su ficha (reglas 6 a 9), la segunda placa del tramo en la casilla del arquero (regla 11), las marcas sobre las líneas de cobertura (regla 10) y el marco de fin (regla 12).
- **Tarjeta de la ficha extendida** (#15, regla 8; margen izquierdo, 144 px): el bloque **Vista previa** de la regla 13, al final de la tarjeta, cuando lo elegido o en inspección tiene golpe en la lista. Cada fila con ícono y número. El texto solo en el título del bloque y en Sobra, para que entre en 144 px con el texto inflado de Localización (F5 de #14). El alto de la tarjeta crece con las filas: lo verifica F7 de #15 (cambio pedido).
- **Zona de avisos** (#15, regla 18): un aviso persistente de la vista previa a la vez (regla 14), tapado por los transitorios de Entrada.
- **Inspección sin asignar:** el jugador lee la tarjeta de un objetivo tocándolo **sin pieza elegida** (Entrada, regla 5: queda en inspección); con una pieza propia elegida, el toque asigna. Es el gesto de Entrada, no uno nuevo. **Costo aceptado (decisión del 2026-11-26):** en el flujo normal de `Atacar`, J tiene elegida a su atacante, que no recibe golpe; para ver Desviado o Sobra sobre el blanco tiene que soltar la selección y tocarlo, son dos gestos y pierde la pieza elegida. Se acepta porque la placa ya dice lo que decide la jugada (¿alcanza?, MUERE, ROTO) y la tarjeta es el detalle. Se revisa en playtest con Open Questions 3.
- **Táctil y mouse por igual:** nada depende del hover. La placa se ve sin tocar nada y la tarjeta con un toque.
- **Mínimo 1080p** (art bible §7): a menos, los números de la placa bajan de 14 px, igual que los de la ficha.
- **Claves de texto (Localización, #14):** `ui.vista_previa_muere` (`MUERE`), `ui.vista_previa_roto` (`ROTO`), `ui.vista_previa_titulo` (`Vista previa`, título del bloque de la tarjeta), `ui.vista_previa_sobra` (`Sobra`), `ui.vista_previa_victoria` (`Ganás la partida`), `ui.vista_previa_derrota` (`Tu Rey muere: perdés`), `ui.vista_previa_doble_rey` (`Caen los dos Reyes: ganás`), `ui.vista_previa_corte_propio` (`Cortás tu propio reclutamiento`), `ui.vista_previa_corte_rival` (`Castillo cortado`) y `ui.vista_previa_rey_herido` (`Tu Rey recibe daño`). Los textos en español son propuestas, con trato de vos y sin signos de exclamación (Localización, regla 16). MUERE y ROTO tienen presupuesto de 6 caracteres y los avisos, de 60. «Castillo cortado» es el término único del glosario (cierre de la tanda 3).
- **Accesibilidad (#23):** cada marca se distingue por forma, ícono o texto (Visual/Audio, *Restricciones*). La vista previa no tiene destellos, así que «Reducir efectos» no la cambia.

📌 **UX Flag — Vista previa de ataques**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Qué | Documento | Dónde | Relación |
|---|---|---|---|
| Vista previa = `Resolver` sobre una copia; eventos; consultas; presupuesto de 5 ms | `design/gdd/libreria-de-reglas.md` | Reglas 8, 9, 10, 11 y 14; Edge Cases (*Deshacer y vista previa*); Tuning Knobs; UI Requirements; AC-11, AC-19, AC-24, AC-33a, AC-33b | Respeta; no pide cambios |
| Forma y orden de los eventos; número flotante = vida perdida, sin número sobre MUERE ni ROTO (BL-1); exceso | `design/gdd/resolucion-simultanea-del-dano.md` | Reglas 2, 3, 5 a 9, 14 y 15; Visual/Audio; UI Requirements; Open Questions | Lee; confirma que los campos alcanzan |
| Disparadores del recálculo; flechas; fuego amigo a cargo de #16 | `design/gdd/alcance-y-asignacion-de-ataques.md` | Reglas 5 a 7, 15; Visual/Audio; Game Feel; UI Requirements; AC-28; Open Questions | Cumple el pedido del fuego amigo |
| Flecha que termina en el escudero; MUERE sobre el escudero; aliado intacto | `design/gdd/cobertura-del-escudero.md` | Reglas 5, 9, 15; Visual/Audio; Game Feel; UI Requirements; AC-28; Open Questions | Cumple y agrega la línea resaltada y la cortada |
| Ancla de la ficha, tarjeta, zona de avisos, medidas de la placa | `design/gdd/ficha-de-pieza-y-hud.md` | Reglas 2, 3, 7, 8, 9, 10, 14, 18; F1, F7; Dependencies; AC-7, AC-12, AC-33 | Amplía la regla 10 y precisa la 18 (abajo) |
| Daño y ROTO del tramo; `TRAMOS_ROTOS_CORTE` | `design/gdd/castillo-y-muros.md` | Reglas 8 a 10; Edge Cases; Visual/Audio; UI Requirements | Cumple |
| El Grito dentro del golpe | `design/gdd/modificador.md` | Regla 7; Player Fantasy; UI Requirements; AC-31 | Cumple; no suma bonos |
| Claves `ui`, presupuestos, cambio de idioma, símbolos | `design/gdd/localizacion.md` | Reglas 1, 4, 12, 15, 16; F4, F5; Dependencies | Agrega claves y pide dos símbolos (abajo) |
| Inspección, aceptación de comandos | `design/gdd/entrada-y-seleccion.md` | Regla 5; Interactions; AC-36 | Lee |
| `DobleRey` y ganador | `design/gdd/maquina-de-turno.md` | Regla 13 | Lee |
| Estadísticas máximas | `design/gdd/catalogo-de-piezas.md` | Estadísticas; Tuning Knobs | Agrega una restricción (F2) |
| Estado «Asignar ataques», placas, colores, flechas, ficha, Into the Breach | `design/art/art-bible.md` | §1 (regla visual, principios 1 y 2), §2, §3, §4, §7, §9 | Aplica; pide activos (abajo) |
| Pilar 1, 2 y 4; anti-pilar de información oculta | `design/gdd/game-concept.md` | Game Pillars; Anti-Pillars | Sirve |

### Cambios pedidos a otros GDD (aplicados al cerrar la tanda 4, 2026-11-26)

Esta sesión corrió en paralelo con las de #18 y #19 y **no editó ningún otro documento**. Los cambios de abajo los aplica la sesión que cierre la tanda 4. Por documento:

| Documento | Cambio pedido |
|---|---|
| **Ficha y HUD (#15)** | **Regla 10:** cambiar «una pieza o un tramo que es **objetivo de al menos una asignación**» por «una pieza o un tramo que **recibe golpe en la vista previa vigente** (`DanoRecibido` o `DanoATramo`; #16, regla 6), sea o no la casilla apuntada». La ficha vuelve en el mismo cuadro en que la pieza deja de recibir golpe en la lista. Agregar que la pieza cubierta con todo desviado conserva su ficha (decisión del jugador, 2026-11-26). Ajustar la fila «Cedida» de States, la fila #16 de Interactions («reemplaza la ficha de cada receptor») y **AC-12** (agregar un caso de área sobre una pieza no apuntada). **Regla 8:** la tarjeta agrega al final el bloque **Vista previa** de #16 (regla 13) durante `Atacar`; **F7** tiene que sumar hasta 7 filas de ese bloque al alto de la tarjeta. **Regla 18:** distinguir el aviso **persistente** de la vista previa (dura mientras su condición se cumpla) de los transitorios: un transitorio lo tapa durante su duración y después vuelve. **Dependencies**, fila #16: «Pendiente» → «Existe». Quitar *PROVISIONAL: depende de #16* de AC-12 y AC-33, y #16 de la lista «Provisional» de Interactions |
| **Localización (#14)** | **Regla 1:** agregar `→` (y `−` si se usa en la tarjeta) a la lista cerrada de símbolos sin idioma. **Dependencies**, fila #16: «#16 pendiente» → «#16 define sus claves `ui.vista_previa_*`» (lista en UI Requirements de #16). **Supuestos provisionales:** quitar las claves `ui` de #16. Agregar el presupuesto **`presupuesto_placa`** (6 caracteres) para MUERE y ROTO en F4, y confirmar el inglés (`DIES` / `BROKEN` o alternativa de 6 o menos) |
| **Resolución (#9)** | **Dependencies**, fila #16: «Pendiente (sin GDD)» → «Existe; lee los eventos de la regla 14 sin pedir campos nuevos». **Interactions**, nota de supuestos: quitar #16 de la lista de lectores sin GDD. **Open Questions**, fila «¿El jugador entiende adónde fue el golpe del aliado cubierto...?»: pasa a #16 (Open Questions 3) |
| **Alcance (#7)** | **Open Questions**, filas «¿Se juega algún fuego amigo por sorpresa...?» y «¿Mostrar el alcance desde un destino hipotético...?»: anotar que #16 las recoge (Open Questions 2 y 4). El encabezado CD-GDD-ALIGN («lo cubre la Vista previa, #16») queda cumplido por las reglas 12 y 14 de #16 |
| **Cobertura (#8)** | **Visual/Audio**, fila «Golpe desviado»: agregar la línea **resaltada** (trazo doble) y la línea **cortada** cuando la cobertura se rompería (#16, regla 10). **Open Questions**, fila «¿Se entiende a ojo sobre quién cae cada flecha?»: compartida con #16 (Open Questions 3) |
| **Modificador (#11)** | Quitar *PROVISIONAL hasta que exista #16* de AC-31 (en Acceptance Criteria y en la lista de PROVISIONAL) |
| **Castillo y muros (#5)** | **Dependencies**, fila #16: confirmar que #16 existe. **UI Requirements:** la vista previa del tramo es una placa de un renglón con `vidaAntes→vidaDespues` (#16, regla 9) |
| **Catálogo de piezas (#2)** | **Tuning Knobs / restricciones:** agregar que, para cada tipo, `vida_t + escudo_ef_max_t ≤ 100`, con el +2 del muro solo en el arquero (#16, F2; hoy el máximo es el Rey, 30 + 5 = 35), con el test de #16 AC-11, igual que la restricción de 2 cifras de #15 |
| **Entrada y selección (#13)** | **Interactions**, fila #16: el recálculo lo disparan también `CoberturaAsignada` y `CoberturaQuitada` (no solo las asignaciones). Sin cambio de gestos |
| **Art bible** (no es un GDD; lo cambia `/art-bible` o el director de arte) | §7 *Íconos:* ícono de **impacto**. §3 *Gramática de formas:* **contorno discontinuo** de lo propio, **marco de fin**, **placa de un renglón**, **línea de cobertura resaltada y cortada**. §4: confirmar que el bermellón del impacto es el del ataque |
| **Índice de sistemas** | Fila 16: estado *Designed*, enlace a este GDD, dependencias + Librería, Ficha y HUD, Entrada (las que faltan en la fila). Agregar Vista previa (#16) como dependencia blanda en las filas 18 (Render), 22 (Audio) y 23 (Accesibilidad). Contadores de progreso (17/19 si cierran #18 y #19, según corresponda) |
| **Render (#18) y Métricas (#19)** (se escriben en paralelo) | **#18:** confirmar la entrega de la regla 19 (la cosecha empieza en el cuadro en que se van las placas) y que el número flotante es la vida perdida, sin número sobre MUERE ni ROTO (decisión del 2026-11-26; antes decía «el `neto` y el `bruto`»). **#19:** decidir si mide el uso de la vista previa (Open Questions 5); hoy #16 no define eventos para eso |
| **Registro (`entities.yaml`)** | Candidatos del informe de la sesión, aplicados al cerrar la tanda 4 |

## Acceptance Criteria

*Valores de los ejemplos: los del Catálogo vigente (soldado 8 / 3 / 25, arquero 6 / 2 / 15, escudero 4 / 5 / 20, catapulta 10 / 2 / 15 con área 6, demoledor 30 / 1 / 10, Rey 8 / 5 / 30; ataque / escudo / vida) y `vida_tramo` = 30. Los fixtures se guardan en el repositorio. J es el jugador del turno, en `Atacar`, y K el rival.* Tipo de historia entre corchetes: **[Logic]** con test automático del cliente sobre la capa de la vista previa (sin dibujar, verificando qué placas, estados y textos produce de una lista), **[Integration]** con la Librería real, **[UI]** o **[Visual]** con captura retenida en `production/qa/evidence/`.

### Reglas (Sección C)

- **AC-1 (regla 2) [Logic]** — **GIVEN** una partida en `Mover`, `Reclutar`, `EsperandoTirada`, el turno de K o `Terminada`, **WHEN** se dibuja el tablero, **THEN** no hay ninguna placa de la vista previa ni aviso de la vista previa, y cada pieza viva muestra su ficha.
- **AC-2 (reglas 2 y 3) [Integration]** — **GIVEN** `Atacar` de J recién abierto con 0 asignaciones, **WHEN** se pide la vista previa, **THEN** la lista no trae eventos de Resolución, no hay placas y las fichas no cambian. **GIVEN** una catapulta de J apuntada a una casilla vacía con su anillo sin piezas ni tramos, **THEN** tampoco hay placas.
- **AC-3 (reglas 3 y 4) [Integration]** — **GIVEN** un soldado de K sin golpe, **WHEN** J asigna un soldado contra él, **THEN** en el mismo cuadro en que aparece la flecha, la ficha del soldado de K desaparece y aparece su placa. **WHEN** J quita ese ataque, **THEN** en un mismo cuadro la placa se va y vuelve la ficha. En ningún cuadro se ven la ficha y la placa sobre la misma pieza.
- **AC-4 (reglas 1 y 5) [Integration]** — **GIVEN** 6 asignaciones de J que incluyen un área, una explosión y un golpe sobre una pieza cubierta, **WHEN** se dibuja la vista previa, **THEN** cada número de cada placa es igual al campo correspondiente del evento de la lista (`bruto`, `escudo`, `vidaAntes`, `vidaDespues`), hay exactamente una placa por `DanoRecibido` y por `DanoATramo`, y ninguna pieza sin evento tiene placa.
- **AC-5 (regla 6) [Integration]** — **GIVEN** una catapulta de J apuntada a un soldado de K con un arquero de K en una vecina y sin flecha hacia él, **THEN** el arquero de K tiene placa (golpe 6, escudo 2, `15→11`). **GIVEN** un arquero de K cubierto por un escudero de K y un soldado de J apuntándole, **THEN** el arquero conserva su ficha y la placa está sobre el escudero (golpe 8, escudo 5, `20→17`).
- **AC-6 (regla 7) [UI]** — **GIVEN** dos soldados de J contra un soldado de K, **THEN** la placa muestra en el renglón A el ícono de soldado y `25→12`, y en el B el ícono de impacto con `16` y el escudo con `3`. **GIVEN** un escudero de J contra el Rey de K (golpe 4, escudo 5), **THEN** el renglón A muestra solo `30`, sin flecha, y el impacto y el `4` están en gris `#A8A49B`.
- **AC-7 (regla 8) [UI]** — **GIVEN** tres soldados y un arquero de J contra un soldado de K, **THEN** su placa es MUERE (calavera y palabra), su figura está desaturada y el renglón B muestra `30` y `3`. **GIVEN** un demoledor de J con la explosión asignada y nadie en sus 8 vecinas, **THEN** el demoledor tiene una placa de un renglón con el ícono de área y MUERE.
- **AC-8 (regla 9) [UI]** — **GIVEN** dos soldados de J contra un tramo de K en pie, **THEN** la placa del tramo es de un renglón: ícono de muro y `30→14`. **GIVEN** cuatro soldados de J contra ese tramo (golpe 32), **THEN** la placa es ROTO (grieta y palabra) y el tramo se dibuja como hueco.
- **AC-9 (regla 10) [UI]** — **GIVEN** un arquero de K cubierto por un escudero de K y tres soldados de J apuntando al arquero, **THEN** las flechas terminan en el escudero, la línea eslabonada se dibuja con trazo doble y el arquero conserva su ficha. **GIVEN** que se suma un cuarto atacante y el escudero muere, **THEN** su placa es MUERE y la línea se dibuja cortada.
- **AC-10 (regla 11) [UI]** — **GIVEN** un arquero de K sobre su tramo en pie y un soldado de J que lo apunta, **THEN** en esa casilla hay dos placas: la del arquero abajo (golpe 8, escudo 4, `15→11`) y la del tramo arriba (`30→22`), sin superponerse, con al menos `separacion_placas_px` (2) entre ellas.
- **AC-12 (regla 12, fuego amigo) [UI]** — **GIVEN** una catapulta de J apuntada a un soldado de K, con un escudero de J en una vecina del centro, **THEN** el escudero de J tiene una placa (golpe 6, escudo 5, `20→19`) con contorno discontinuo de 2 px, y las placas de K no lo tienen.
- **AC-13 (regla 12, fin de partida) [Integration]** — **GIVEN** el Rey de J con 5 de vida pegado a un demoledor de J con la explosión asignada, y el Rey de K vivo, **THEN** la placa del Rey de J es MUERE con contorno discontinuo y marco de fin, la zona de avisos muestra `ui.vista_previa_derrota`, y **WHEN** J toca Terminar turno una sola vez, **THEN** se envía `Resolver` sin ningún diálogo. **GIVEN** que la explosión también mata al Rey de K, **THEN** los dos Reyes tienen marco de fin y el aviso es `ui.vista_previa_doble_rey`. **GIVEN** que solo cae el Rey de K, **THEN** el aviso es `ui.vista_previa_victoria`.
- **AC-15 (regla 13; F3) [UI]** — **GIVEN** un arquero de K cubierto por un escudero de K, tres soldados de J apuntando al arquero y un arquero de J apuntando al escudero, **WHEN** J toca al escudero sin pieza elegida (queda en inspección), **THEN** el bloque Vista previa de la tarjeta muestra Golpe 30, Desviado 24, Escudo 5, Pasa 25, Vida `20→0`, Sobra 5 y Atacantes 4.
- **AC-16 (regla 14) [Integration]** — **GIVEN** J con 1 tramo roto y una catapulta de J apuntada a un tramo propio con 10 de vida, **THEN** el aviso es `ui.vista_previa_corte_propio`. **WHEN** J manda un ataque rechazado (`FueraDeAlcance`), **THEN** el aviso del rechazo lo tapa durante su duración y, al terminar, vuelve el de corte propio. **WHEN** J quita la catapulta, **THEN** el aviso se va en el mismo cuadro. **GIVEN** además el Rey de J con 5 de vida pegado a un demoledor de J con la explosión asignada, **THEN** se ve solo `ui.vista_previa_derrota` (prioridad 1). **GIVEN** J con 1 tramo roto y un demoledor de J que explota junto a dos tramos propios en pie (el contador pasa de 1 a 3), **THEN** el aviso es `ui.vista_previa_corte_propio`.
- **AC-30 (regla 14, prioridad 4) [Integration]** — **GIVEN** el Rey de J con 30 de vida en el anillo de su propia catapulta (área 6, escudo 5) y ningún otro aviso de mayor prioridad, **THEN** la placa del Rey es `30→29` con contorno discontinuo y el aviso es `ui.vista_previa_rey_herido`.
- **AC-31 (regla 14, prioridad 5; Edge Cases, victoria sobre corte) [Integration]** — **GIVEN** K con 1 tramo roto y cuatro soldados de J contra un tramo de K en pie (golpe 32), **THEN** el aviso es `ui.vista_previa_corte_rival`. **GIVEN** además que la misma lista mata al Rey de K, **THEN** se ve solo `ui.vista_previa_victoria` (prioridad 2) y el corte rival no se avisa.
- **AC-18 (reglas 15 y 3) [Integration]** — **GIVEN** placas MUERE y ROTO y un aviso a la vista, **WHEN** el jugador cambia de ES a EN, **THEN** las palabras y el aviso cambian en el mismo cuadro y la operación de vista previa **no** se vuelve a pedir (se cuenta con un contador de llamadas en el test).
- **AC-19 (regla 16) [UI]** — **GIVEN** una flecha de J que pasa por encima de una pieza de K con placa, **THEN** la placa se dibuja por encima de la flecha y ninguno de sus números queda cruzado por el trazo.
- **AC-20 (regla 17) [UI]** — **GIVEN** «Ficha simple» activa y un ataque asignado, **THEN** la placa del objetivo muestra los dos renglones completos.
- **AC-22 (regla 18) [Logic]** — **GIVEN** que la operación de vista previa devuelve un código de rechazo, **THEN** no se dibuja ninguna placa ni aviso aunque hubiera una lista anterior, y cada pieza muestra su ficha.
- **AC-23 (regla 19; cruza #18) [Integration]** — **GIVEN** placas a la vista, **WHEN** se acepta `Resolver` (y, en otra corrida, `TurnTimeout` de atacar), **THEN** las placas se van en el cuadro en que empieza la cosecha, y cada número flotante de la cosecha es igual al de la placa que estaba sobre ese receptor (`vidaAntes − vidaDespues` de la placa; ningún número sobre un receptor con MUERE o ROTO).
- **AC-27 (regla 4) [Integration]** — **GIVEN** tres comandos de asignación aceptados en el mismo cuadro, **THEN** las placas dibujadas son las de la lista del tercero, sin placas de las listas del primero o del segundo, y la operación de vista previa se llama una sola vez en ese cuadro (contador de llamadas en el test).
- **AC-32 (regla 3, disparadores) [Integration]** — **GIVEN** una partida guardada en `Atacar` con 2 asignaciones de J sobre un soldado de K, **WHEN** se carga, **THEN** las placas aparecen en el primer cuadro dibujado, sin ningún comando. **GIVEN** una `Resincronizar` que deja la partida en `Atacar` de J con asignaciones, **THEN** la vista previa se pide una vez y las placas son las del estado resincronizado. **WHEN** J manda un comando cuyo resultado trae solo `CoberturaAsignada`, **THEN** la vista previa se pide una vez (contador de llamadas en el test).
- **AC-33 (Visual/Audio, cue de MUERE) [Logic]** — **GIVEN** un soldado de K herido en la lista vigente, **WHEN** un comando lo pasa a MUERE, **THEN** la capa pide un cue de MUERE previsto; **WHEN** otro comando lo deja en MUERE, **THEN** no pide otro. **GIVEN** un comando que pasa a MUERE a tres piezas a la vez, **THEN** pide un solo cue.

### Fórmulas (Sección D)

- **AC-14 (F1) [UI]** — **GIVEN** la fuente elegida a 1080p, **WHEN** se dibujan (a) la placa con golpe 99 y escudo 10, (b) MUERE y ROTO con 6 caracteres, (c) el arquero sobre su tramo y (d) un tramo con `vida_tramo` = 1.000 que recibe 15, **THEN** (a) y (b) miden ≤ 64 × 40 px, (c) suma ≤ 68 px de alto en la casilla y (d) muestra solo `985`. Si la fuente da más de 7 px por carácter, el criterio falla y se revisa F1 antes de cerrar.
- **AC-11 (F2) [Logic, automático en el repositorio]** — **GIVEN** el Catálogo vigente y el bono del muro, **WHEN** corre el test, **THEN** para cada tipo `vida_t + escudo_ef_max_t ≤ 100`, con el +2 del muro solo en el arquero (hoy el máximo es el Rey, 30 + 5 = 35; arquero 15 + 4 = 19). **GIVEN** dieciséis arqueros de J con Grito contra un soldado de K (golpe 128), **THEN** el renglón B muestra `99+`, sin escudo, y la placa está en MUERE. **GIVEN** golpe 99, **THEN** muestra `99` y el escudo. **Caso límite** (pedido del Catálogo, AC-37, 2026-11-27): **GIVEN** un fixture con la `vida` del Rey en 96 (96 + 5 = 101), **THEN** el test falla y nombra al Rey; con 95 (100) pasa. Corre en el mismo job de CI que #15 AC-4, sobre el mismo fixture del Catálogo.
- **AC-26 (F3, borde) [Logic]** — **GIVEN** un soldado de K que recibe golpe 28 (neto 25 = su vida), **THEN** está en MUERE y la tarjeta no muestra la fila Sobra. **GIVEN** que recibe 30 (neto 27), **THEN** Sobra muestra 2. **GIVEN** que recibe 16 (sobrevive), **THEN** no hay fila Sobra.
- **AC-21 (F4) [Integration, rendimiento]** — **GIVEN** el fixture del peor tablero de la Librería (AC-24: 122 piezas, 32 tramos, 61 ataques) con un reparto de ataques que da golpe a los 154 receptores, **WHEN** se reemplaza la lista 100 veces después de 20 de calentamiento, **THEN** el percentil 95 de `t_dibujo` es ≤ 4 ms y el de `t_vista_previa + t_dibujo` ≤ 9 ms en la máquina, el navegador y el build de referencia de la Librería.

### Casos borde y cruces entre sistemas

- **AC-24 (cruza #11, AC-31) [Integration]** — **GIVEN** Grito activo para J y un arquero de J contra un soldado de K, **THEN** la placa muestra golpe 8 (no 6). **GIVEN** la catapulta de J, **THEN** el centro muestra golpe 12 y las vecinas golpe 6. **GIVEN** el demoledor, **THEN** sus vecinas muestran golpe 30.
- **AC-25 (cruza #8, AC-28) [UI]** — La captura de AC-9 sirve también como evidencia de AC-28 de Cobertura: flechas que terminan en el escudero con la punta fundida, el aliado sin placa y MUERE sobre el escudero con el golpe letal.
- **AC-28 (Edge Cases, demoledor herido) [UI]** — **GIVEN** un demoledor de J con la explosión asignada y la catapulta de J apuntada a su casilla, **THEN** su placa es MUERE con el ícono de área y un renglón B con golpe 10 y escudo 1.
- **AC-29 (Edge Cases, arquero en el muro roto) [Integration]** — **GIVEN** un arquero de K sobre su tramo con 8 de vida y un soldado de J que lo apunta, **THEN** la placa del tramo es ROTO y la del arquero muestra escudo 4 (con el +2) y `15→11`. **WHEN** se resuelve, **THEN** la cosecha muestra 4 (vida perdida) sobre el arquero y la placa ROTO sin número sobre el tramo (BL-1).

### Visual

- **AC-17 (Visual/Audio, *Restricciones*) [Visual]** — **GIVEN** un tablero con una placa herida, una sin pérdida, MUERE, MUERE por explosión, un tramo dañado, ROTO, fuego amigo, desvío, cobertura cortada y marco de fin, **WHEN** se toman capturas en color, en grises y con los filtros de protanopía, deuteranopía y tritanopía, en los dos bandos, **THEN** dos revisores que no leyeron el GDD identifican los 10 elementos por su forma, ícono o texto en cada una de las 5 capturas, con 0 confusiones entre elementos. Lo firma el director de arte.

### Revisión de QA

*Lente `qa-lead` (modo `full`, sin agente), con las secciones C, D y E releídas del archivo.* Las 20 reglas tienen al menos un criterio: 1 (AC-4), 2 (AC-1, AC-2), 3 (AC-2, AC-3, AC-18, AC-32), 4 (AC-3, AC-27), 5 (AC-4), 6 (AC-5), 7 (AC-6), 8 (AC-7, AC-28), 9 (AC-8), 10 (AC-9), 11 (AC-10, AC-29), 12 (AC-12, AC-13), 13 (AC-15), 14 (AC-16, AC-30, AC-31), 15 (AC-18), 16 (AC-19), 17 (AC-20), 18 (AC-22), 19 (AC-23) y 20 (por exclusión: AC-1 y AC-22; no tiene comportamiento propio que probar). Las cuatro fórmulas tienen criterio con valor de borde: F1 (AC-14, el máximo y el caso abreviado), F2 (AC-11, 99 contra 100), F3 (AC-26, neto igual a la vida) y F4 (AC-21, 154 receptores). Cruces con otros sistemas: #11 (AC-24), #8 (AC-25), #5 (AC-29), #18 (AC-23) y #1 (AC-4, AC-21). **Huecos encontrados y resueltos en esta pasada:** faltaba el caso de comandos seguidos en el mismo cuadro (se agregó AC-27) y el del arquero cuyo tramo se rompe (AC-29). En el `/design-review` del 2026-11-26 se agregaron las prioridades 4 y 5 de la regla 14 (AC-30, AC-31), los disparadores de carga, `Resincronizar` y `CoberturaAsignada` (AC-32) y el cue de MUERE previsto (AC-33). **Sin criterio automatizable, a playtest:** si el jugador entiende el desvío sin leer la tarjeta y si alguna vez lo resuelto no coincidió con lo previsto (Game Feel, *Verificación*). Numeración: los AC se agrupan por sección, así que los números no van en orden; no hay números repetidos ni salteados del 1 al 33.

## Open Questions

| # | Pregunta | Dueño | Cuándo |
|---|---|---|---|
| 1 | En el online, ¿el rival ve la vista previa de J mientras J asigna (todo es público), o solo las flechas? Ver las placas ajenas sirve para leer la jugada, pero puede saturar la pantalla del que espera | Vista previa (#16) con Red (Fase 2) | Al diseñar el online |
| 2 | ¿Mostrar el resultado de un movimiento hipotético durante `Mover` («si pongo el arquero acá, ¿a quién alcanza y cuánto le hace?»)? Viene de Alcance (#7, Open Questions). Necesita una operación de la Librería sobre un estado que todavía no existe, y alarga la fase de mover | Entrada (#13), Vista previa (#16), Librería | Después del MVP, con datos de duración del turno |
| 3 | ¿El jugador entiende adónde fue el golpe del aliado cubierto (flecha al escudero, línea resaltada) y lo que sobra sin abrir la tarjeta? Viene de Resolución (#9) y Cobertura (#8) | Vista previa (#16); playtest | MVP |
| 4 | ¿Se juega algún fuego amigo por sorpresa, sobre todo contra el Rey propio, con el contorno discontinuo y los avisos de la regla 14? Viene del CD-GDD-ALIGN de Alcance (#7). Medir con la pregunta de Game Feel | Vista previa (#16); playtest | MVP |
| 5 | ¿Métricas (#19) necesita medir el uso de la vista previa (cuántas asignaciones se prueban y quitan por turno, cuántas veces se resuelve con una derrota propia a la vista)? Hoy #16 no emite eventos propios; si hacen falta, son eventos del cliente, no del núcleo | Métricas (#19) | Al escribir #19 |
| 6 | Si un cliente web no llega a dibujar 154 placas en `vista_previa_dibujo_ms` (4 ms), ¿se acepta dibujar las placas un cuadro después de la flecha (rompe la regla 4) o se simplifica la placa? | Vista previa (#16), rendimiento; decide el jugador | Al medir AC-21 en el build web |
| 7 | ¿Las dos placas del arquero sobre su tramo (60 de 68 px) dejan ver lo suficiente de la figura? El art bible ya aceptó que la ficha tape el 61% de la altura (#15, F1); con la placa del tramo arriba queda poco más | Director de arte; playtest | Primer prototipo con arte |
| 8 | Las palabras en inglés de MUERE y ROTO en 6 caracteres o menos (`DIES` y `BROKEN` son propuestas) | Localización (#14) | Al cerrar la tanda 4 |
| 9 | ¿El cue de «MUERE previsto» se vuelve ruido cuando el jugador prueba muchas combinaciones? Si molesta, se limita a la primera vez que cada receptor pasa a MUERE en el turno | Audio (#22); playtest | Vertical Slice |
| 10 | ¿Alcanza con la punta fundida de la flecha (×1 / ×1,5 / ×2) para que el combo se lea en la vista previa, o la placa tiene que marcarlo (por ejemplo, un borde más grueso con 2 o más atacantes)? El art bible §1, principio 2, pide que *«el efecto sobre el objetivo crezca con la cantidad de atacantes»* también en la vista previa (CD-GDD-ALIGN, aceptado) | Director de arte; playtest | Primer prototipo con arte |
| 11 | ¿Hace falta una opción de escala de texto para la placa? Con números de 14 px y mínimo 1080p (UI Requirements) no se puede agrandar sin pasar los 64 × 40 px de la ficha (F1) | Accesibilidad (#23) con Ficha y HUD (#15) | Al escribir #23 |
| 12 | **Capa de amenaza del turno siguiente del rival** (pedido del Tablero, #4, Player Fantasy y Open Questions, 2026-11-27): lo que el rival puede golpear en su próximo turno, movimiento en cruz + alcance en círculo, calculado **antes de su tirada**, con la franja base (sin bono, que contiene la de Distracción) y la franja del Grito aparte. La calcula Alcance (#7) y la activa un gesto de Entrada (#13); ¿cómo convive con las placas? | Vista previa (#16), con #15 | MVP |

