# Render, animación y feedback de combate

> **Status**: Designed (`/design-review` 2026-11-26: NEEDS REVISION; correcciones aplicadas, pendiente de re-review)
> **Author**: Martin Arin + Claude
> **Last Updated**: 2026-11-26
> **Last Verified**: 2026-11-26
> **Implements Pillar**: Pilar 2 (La fuerza está en combinar) y Pilar 1 (Planificar y cosechar); Pilar 4 (Duelo corto) como restricción de ritmo
> **Creative Director Review (CD-GDD-ALIGN)**: CONCERNS (accepted) 2026-11-26 — pilar 4 (regla 25: la Tirada que espera a la presentación saca la animación del reloj, pero suma ≤ 2 s de tiempo de pared por turno con ataques, ~1 min por partida, por encima de los 25:00 como las esperas de la tirada); pilar 1 (Edge Cases: con un área que salpica, el pulso puede salir más grande que la punta de la flecha, aunque el número sea exacto); pilar 2 (F2: la escala satura en 3 atacantes, el tope del art bible, y un combo de 6 se ve como uno de 3). Los tres están en Open Questions, para medir en el MVP. El gate lo aplicó la sesión misma, sin agentes.

## Summary

Render dibuja el campo de batalla: el terreno, los castillos con sus tramos en tres estados, las figuras leídas de una skin como datos y las marcas del plan (destinos, alcance, flechas con punta fundida, área, explosión, cobertura). También anima lo que el núcleo ya decidió. Su momento central es la cosecha: en 1,65 s (techo 2 s), todos los golpes de la resolución caen en el mismo cuadro, el golpe combinado se ve más grande que la suma de los chicos con el mismo escalón que prometía la flecha, y los muertos caen juntos. No se puede saltar: un toque durante la cosecha se ignora (decisión del 2026-11-26). En local no le cuesta reloj al jugador siguiente. Define el formato de skin que heredan la colección y el Kit, y nunca toca la ficha ni ninguna cifra.

> **Quick reference** — Layer: `Presentation` · Priority: `MVP` · Key deps: `Librería (#1), Catálogo (#2), Máquina (#3), Tablero (#4), Castillo (#5), Movimiento (#6), Alcance (#7), Cobertura (#8), Resolución (#9), Interfaz del Modificador (#17), Entrada (#13), Ficha y HUD (#15); blandas: Reclutamiento (#10), Modificador (#11), Timer (#12), Localización (#14), Vista previa (#16)`

## Overview

Render es la capa del cliente que **dibuja el mundo y anima lo que el núcleo ya decidió**. Dibuja el terreno, los dos castillos con sus tramos en tres estados (entero, mellado, roto), las figuras de las piezas y las marcas sobre el tablero que piden los sistemas de reglas: puntos de movimiento, corchetes de alcance, flechas de ataque con punta fundida, el anillo doble del área, la zona de explosión, la línea eslabonada de cobertura y las casillas de aparición numeradas del reclutamiento. Además traduce en movimiento, luz y efectos cada evento del núcleo: el desplazamiento de `PiezaMovida`, la llegada de `Reclutado`, el cambio de luz de `TramoFinalIniciado` y, sobre todo, la **resolución del turno**. En ≤ ~2 s, todos los golpes caen en el mismo cuadro, el golpe combinado se ve más grande que la suma de los chicos, los muertos caen juntos y el terreno queda marcado.

Render **no decide nada del juego**. Lee el estado público y los eventos de la Librería, de solo lectura, como el HUD (#15). Cuando llega un evento, el estado ya es el final, así que acelerar o cortar una animación no cambia el resultado (regla 2). La cosecha, igual, se ve siempre entera (regla 24). Tampoco dibuja la capa del sistema que vive sobre las piezas: la ficha y el HUD son de #15, y el contenido de la vista previa (números, MUERE, ROTO) es de #16. Render les da la posición en pantalla de cada casilla y respeta su lugar en el orden de capas. Así, ninguna skin puede tapar una cifra.

Render **define el formato de skin**. Toda figura se dibuja leyendo una skin como datos: un manifiesto que asigna a cada tipo del Catálogo (ID 0 a 6) sus cuadros de reposo y de ataque, y que opcionalmente trae un castillo en sus tres estados. La skin básica es la primera skin en ese formato, no un caso especial. Así el Kit de creación de skins (#38) y la colección (#37) heredan el formato sin rehacer el render (índice de sistemas, #18). En el MVP el render está **reducido**: una sola orientación dibujada, espejada para el bando derecho, con reposo corto y movimiento y ataque animados por código. No hay ciclo de caminata ni 8 direcciones, porque no se produce arte final hasta pasar la compuerta de duración de partida (índice, *High-Risk Systems*).

Sin este sistema, las reglas existen pero el jugador no las ve. El pilar 2 promete que *"un golpe combinado se siente distinto de varios golpes sueltos"*, y eso solo lo puede cumplir la presentación. El pilar 1 necesita que lo que se vio en la vista previa sea literalmente lo que se ve caer.

## Player Fantasy

**El plan, hecho miniatura.** El tablero es una mesa de figuras pintadas sobre un campo de batalla, y esas figuras hacen **exactamente** lo que planeaste. Mientras planificás, el mundo se queda quieto y callado: el terreno apagado, las figuras respirando apenas, y encima las flechas de tu plan, más gruesas donde juntaste más piezas. Cuando tocás Resolver, todo pasa a la vez. Las flechas se vuelven golpes, los golpes caen en el mismo cuadro, y donde juntaste varias piezas el impacto es uno solo y grande. Los que tenían que caer caen juntos, y la tierra queda removida donde cayó la piedra de la catapulta.

**Momento ancla:** tres soldados y un arquero tuyos apuntan al soldado rival, y sus cuatro flechas ya se funden en una punta del doble de grosor. Tocás Resolver. Los cuatro hacen su gesto corto al mismo tiempo, el pulso de luz sale del objetivo una sola vez y más ancho que el de un golpe suelto, la placa MUERE aparece sobre él igual que en la vista previa, y el soldado se desatura y cae. En menos de dos segundos el tablero queda quieto otra vez, listo para el que sigue.

**Una cláusula que la sostiene: se lee de un vistazo.** El tablero cuenta la batalla sin texto. Los huecos y los escombros dicen qué castillo viene perdiendo, las marcas de tierra y de quemado dicen dónde cayó lo pesado, y cada pieza se reconoce por su silueta aunque lleve una skin que nunca viste. Ningún adorno compite con la ficha ni con una flecha.

**Pilares que sirve:**

- Pilar 2, *"Ninguna pieza gana sola: el poder sale de juntar varias sobre un mismo objetivo."* Su prueba de diseño lo exige en la presentación: *"En imagen y sonido, un golpe combinado se siente distinto de varios golpes sueltos."* Este sistema es el que lo hace ver.
- Pilar 1, *"Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos."* Los frutos se ven acá, y se ven iguales a lo anunciado: ningún número, caída ni rotura difiere de la vista previa.
- Pilar 4 como restricción: la cosecha es rápida (≤ ~2 s) y no le cuesta reloj al jugador siguiente (Core Rules).

**Qué le exige al resto del diseño:**
- Que la duración de la resolución no dependa de cuántos ataques hubo (todo es simultáneo; Formulas, F1).
- Que el combinado escale con la misma escala que la punta fundida, para que la promesa de la flecha y la del impacto sean una sola (F2).
- Que la skin nunca toque la capa del sistema.
- Que el MVP reducido (una orientación, tweens por código) no se sienta pobre: el peso lo llevan el sincronismo, la luz y la escala del impacto, no la cantidad de cuadros (art bible, principio 3).

*Encuadres considerados:* «Se lee de un vistazo» (solo legibilidad, cubre el pilar 1 pero no el combo) y «El golpe que pesa» (solo sensación, cubre el pilar 2 pero empuja a alargar la animación contra el pilar 4). Se eligió el plan hecho miniatura, con el combo como cima y la legibilidad como cláusula. Elegido en `guided` por recomendación, siguiendo a Resolución (#9), cuya fantasía es «la cosecha»; se puede cambiar.

## Detailed Design

### Core Rules

*Lentes aplicadas sin agentes (modo `full`, filas «UI systems» y «Visual effects» de la tabla de ruteo, más la de «Animation» porque el sistema anima piezas):*
- **game-designer:** la duración de la cosecha no puede depender de cuántos ataques hubo, y el reloj del siguiente no la paga.
- **ux-designer:** cada marca se distingue por forma, y ninguna forma significa dos cosas.
- **ui-programmer:** orden de capas fijo y posición de pantalla por casilla, que #15 y #16 leen.
- **art-director:** se fijan las marcas que faltaban en el art bible.
- **technical-artist:** render determinista para las repeticiones, y el 2D y los atlas sin verificar en la referencia del motor (ADR).
- **systems-designer:** una cola de presentación ordenada.

*Conflictos resueltos en `guided` (menores, cambiables):*
- (a) Alcance: «relleno tenue» (Entrada) contra corchetes (art bible §3 y Alcance). Gana **corchetes**.
- (b) Destinos: «costo en número» (Entrada) contra puntos (art bible y Movimiento). Va un **punto con el costo adentro**.
- (c) El contorno discontinuo lo usaban la inspección (Entrada, «por ejemplo») y el fuego amigo (#9). La inspección pasa a **contorno doble fino**.
- (d) Flecha desviada: «termina en el escudero» (Cobertura) contra «la punta es el blanco táctil» (Entrada, regla 8). La flecha **llega al cubierto y se quiebra hacia el escudero** (regla 12).

#### A. Qué es y qué lee

1. **Capa de solo lectura.** Render es una función del **estado público** y de los **eventos** que devuelve la Librería (regla 8 de la Librería). Lee:
   - las consultas de Tablero (#4: `dentro`, `ocupante`, `hayTramo`, anillo, interior), Castillo (#5: vida de cada tramo, `enMuro`, `tramosRotos`), Catálogo (#2: **solo el ID de tipo**, regla 9 del Catálogo), Movimiento (#6: `MovimientosLegales`), Alcance (#7: `CasillasEnAlcance`, objetivos y la lista de asignaciones), Cobertura (#8: `Coberturas()`, `AliadosCubribles`) y Reclutamiento e Interfaz del Modificador (#10, #17: la lista de casillas a numerar);
   - la selección y la inspección de Entrada (#13);
   - los eventos de Movimiento, Reclutamiento, Resolución, Cobertura, Máquina y Timer.

   **Render no escribe en el estado ni en la huella y no recalcula ninguna cifra del juego.** Su estado propio (animaciones en curso, cola de presentación, marcas del terreno, variantes de casilla) es del cliente, igual que el estado del HUD (#15, regla 1).
2. **El estado ya es el final.** Cuando un evento llega, el comando ya se aplicó. Una animación es solo el camino visual del estado anterior al nuevo. **Acelerar u omitir una animación nunca cambia lo que queda dibujado al terminar.** Si llega un estado nuevo mientras una animación corre, esa animación **de la misma entidad** (figura, tramo, marca) salta a su cuadro final en el cuadro siguiente y empieza la nueva. Las animaciones de otras entidades siguen. **La resolución (grupo 1) no se interrumpe por un estado nuevo**: lo que llega espera en la cola (regla 19). El único corte es la rendición durante la cosecha (Edge Cases) (decisión del 2026-11-26). Es la misma regla que usa la Interfaz del Modificador (#17).
3. **Determinismo de la presentación.** Dados el mismo estado y la misma secuencia de eventos, Render dibuja lo mismo:
   - la variante de suelo de cada casilla sale de su coordenada, no del azar;
   - las marcas del terreno se ubican por casilla;
   - ninguna animación usa números aleatorios sin semilla derivada del estado.

   Las Repeticiones (#35) reproducen la partida alimentando a Render con los eventos guardados, sin un segundo dibujante.

#### B. Capas y skin

4. **Orden de capas (de abajo arriba; ninguna capa de arriba se tapa con una de abajo).**

   | # | Capa | Dueño del contenido | Recibe gradación |
   |---|---|---|---|
   | 1 | Terreno: tierra, pasto, piedras, las 4–6 variantes de casilla y las **marcas de batalla** (tierra removida, quemado) | Render (skin básica) | **Sí, la única** |
   | 2 | Castillo: interior empedrado, tramos en 3 estados, escombros | Render (castillo de la skin de su dueño) | No |
   | 3 | **Relleno del suelo de bando** de la casilla con pieza (Hueso o Ciruela) y las cuatro matas de pasto de las esquinas. Una skin opaca lo puede tapar; el respaldo de bando que no se tapa es el **marco de bando** de la capa 7 | Render (capa del sistema, colores del art bible §4) | No |
   | 4 | Marcas bajas: puntos de movimiento, corchetes finos de alcance (sin objetivo), casillas de aparición numeradas, casilla pulsada | Render (forma), cada sistema (qué casillas) | No |
   | 5 | **Figuras** (la skin), ordenadas por fila (la de más abajo encima) y, en una misma casilla, el arquero sobre su tramo con el parapeto | Skin | No |
   | 6 | Efectos de la resolución: proyectiles, pulsos, explosión, polvo de la rotura | Render | No |
   | 7 | Marcas altas, **encima de la figura**: **marco de bando** (Hueso o Ciruela, con el borde interior de 2 px en Ciruela), **marcas de objetivo** (corchetes gruesos de objetivo válido, anillo doble del área, contorno dentado de la explosión), flechas de ataque, línea eslabonada de cobertura, contornos de elegida, inspección y «propia en zona» (decisión del 2026-11-26) | Render | No |
   | 8 | Capa del sistema sobre la pieza: ficha y banderín (#15), vista previa (#16), placas MUERE y ROTO, números flotantes | #15 y #16 (contenido); Render (ancla y momento en la resolución) | No |
   | 9 | HUD de pantalla: timer, botones, etiqueta y recuadros del Modificador | #15 y #17 | No |

   La gradación de los estados de ambiente (regla 26) **solo toca la capa 1**. Figura, ficha y suelo de bando se dibujan con exposición fija (art bible §2, *Regla de ambiente*).

   **Marcas de objetivo y bando sobre la figura (decisión del 2026-11-26).** Así una skin opaca que llena su lienzo no tapa el bando ni un objetivo (regla 5, anti-pilar «NOT pagar para ganar»). El suelo de bando entero encima de la figura la taparía, así que en la capa 7 va como **marco**: una franja sobre el borde de la casilla, y el relleno sigue en la capa 3. **El ancho de la franja lo fija el director de arte** (art bible §4; TBD). El lienzo de la skin no cambia. La casilla pulsada y los corchetes finos de alcance quedan en la capa 4: sobre una casilla con pieza los puede tapar la skin, y la elegida ya tiene su contorno en la capa 7.
5. **La skin dibuja solo la capa 5.** Una skin no puede dibujar en otra capa ni fuera de la casilla de su pieza, ni leer o cambiar una cifra, un color del sistema o la posición de la ficha. Ese es el anti-pilar «NOT pagar para ganar» y el principio 3 del ancla («la capa del sistema es intocable»). Las capas 3, 4, 6, 7 y 8 usan **solo** activos del sistema, iguales con cualquier skin.
6. **Formato de skin (manifiesto de datos).** Una skin es un paquete de datos sin código:
   - **Identidad:** `skinId`, `versionFormato` (hoy 1) y la clave de su nombre (Localización).
   - **Figuras, obligatorio, una por cada ID de tipo 0 a 6:**
     - `reposo`: 1 a 4 cuadros;
     - `ataque`: 1 a 8 cuadros, con el índice del **cuadro de golpe** (el que coincide con el impacto, regla 20);
     - `pivote`: el punto de apoyo en el centro de los pies (art bible §8);
     - `miraHacia`: el lado al que apunta el arma o la montura en el dibujo (derecha o izquierda).
   - **Castillo, opcional:** `tramoEntero`, `tramoMellado`, `tramoRoto` (hueco con escombro bajo y borde quebrado), `interior` (1 a 6 variantes) y `parapeto` (el que marca al arquero en el muro).
   - **Lienzo:** cada cuadro mide exactamente **una casilla** (136 × 136 px de exportación, art bible §8); lo que caiga fuera del lienzo no existe.

   El manifiesto no tiene campos para la ficha, los colores del sistema, las marcas ni el terreno. **No hay dónde escribirlos**, y así la regla 5 se cumple por construcción.
7. **Validación al cargar una skin (en Render; el Kit #38 suma sus pruebas).** Una skin se acepta si:
   - (a) trae los 7 IDs de tipo, del 0 al 6, y ninguno fuera de ese rango (Catálogo, Edge Cases y AC-31);
   - (b) cada animación tiene cuadros dentro de su rango y el índice del cuadro de golpe existe;
   - (c) cada cuadro mide exactamente el lienzo;
   - (d) su `versionFormato` es una que este cliente sabe leer.

   Una skin que falla **no se dibuja a medias**: se usa la **skin básica completa** en su lugar y se registra el motivo para las Métricas (#19, errores técnicos). Una skin válida sin castillo usa el castillo de la skin básica. Las pruebas humanas (silueta ≥ 90% en grises, colores prohibidos, art bible §3 y §5) son del validador del Kit (#38), no de la carga.
8. **Una skin por jugador.** Cada jugador dibuja sus piezas con su skin (en el MVP los dos usan la básica). Los dos pueden usar la misma skin y se siguen distinguiendo por el suelo de bando, la orientación y el emblema (art bible §5). El castillo de cada jugador sale de su skin. El terreno es siempre el de la skin básica en el MVP; un terreno por pack o por facción queda fuera del formato 1.
9. **Orientación (decisión del jugador, 2026-11-26).** Cada tipo se dibuja en **una sola orientación**. Si la `miraHacia` de la skin no coincide con el lado del rival (`ladoDelRival`, Tablero, regla 11), Render espeja la figura en horizontal. Así el arma o la montura siempre apuntan al rival, que es el respaldo del bando sin color (art bible §4). La figura **no gira** hacia su objetivo ni al moverse.

#### C. Marcas sobre el tablero

Todas las marcas son de la capa del sistema (art bible §3, *Gramática de formas de la interfaz*), se distinguen en grises por su forma y entran o salen con la animación de interfaz (≤ 0,25 s, frenado suave; art bible §7). **Qué casillas se marcan lo decide el sistema dueño; Render decide cómo se ven.**

10. **Selección e inspección (Entrada, #13).**
    - **Pieza elegida:** contorno grueso continuo alrededor de la figura.
    - **En inspección:** contorno **doble fino** (conflicto c).
    - **Casilla pulsada:** relleno breve mientras dura la pulsación.
    - **Tramo en inspección:** el mismo contorno doble sobre el tramo (#15, regla 9).
11. **Movimiento (Movimiento #6, Entrada #13).**
    - Cada destino legal lleva un **punto blanco al 60%** con el **costo** del destino adentro en número (≥ 14 px, negrita, borde Tinta; conflicto b).
    - Un hueco transitable se marca igual que una casilla libre (Castillo, Visual/Audio).
    - **Nunca hay flechas para mover**: las flechas son de ataque (Movimiento, Visual/Audio).
12. **Ataque y cobertura (Alcance #7, Cobertura #8).**
    - **Alcance:** corchetes quietos en las esquinas, bermellón al 70% (conflicto a). La zona ciega queda sin corchete, y los objetivos válidos llevan corchetes más gruesos, en la capa 7, encima de la figura (regla 4).
    - **Flecha:** una por asignación, en arco desde el atacante hasta la casilla objetivo, bermellón `#E55A40` al 100% con borde Tinta de 2 px. Las flechas que llegan a la misma casilla **se funden en una sola punta**, con un grosor según `escala_combo` (F2). La punta queda sobre la casilla objetivo, que es el blanco táctil (Entrada, regla 8).
    - **Área:** el arco más un **anillo doble** en el centro elegido. El anillo interior marca el centro y el exterior las vecinas dentro del tablero (las de afuera no se dibujan).
    - **Explosión:** sin flecha. Lleva una **mecha** sobre el demoledor y un **contorno dentado** alrededor de cada una de sus 8 vecinas dentro del tablero, muros incluidos. Falta en el art bible y lo define este GDD.
    - **Lo propio dentro de un área o una explosión:** contorno **discontinuo** sobre la figura o el tramo propio, el mismo que marca el fuego amigo en la resolución (Resolución, Visual/Audio). Falta en el art bible.
    - **Desvío:** si el objetivo es un aliado cubierto del rival, la flecha llega a su casilla, **se quiebra en ángulo** y sigue en un tramo corto hasta el escudero, donde se suma a la punta fundida del escudero. Ese desvío cuenta en `escala_combo` del escudero, no en la del cubierto. Mientras haya un desvío, la línea eslabonada del par **se resalta**.
    - **Cobertura:** línea recta eslabonada, acero `#7F9BB8`, sin punta, entre el escudero y el aliado. La ven **los dos jugadores** hasta que se borra (Cobertura, Visual/Audio: la línea sigue a la vista durante el turno del rival).
    - **Solo se dibujan las flechas del jugador de turno** (art bible §4).
13. **Reclutamiento (Reclutamiento #10, Interfaz del Modificador #17).**
    - Mientras el recuadro de reclutar está abierto, Render numera las `K` casillas que le da la interfaz, en el orden de `orden_aparicion`: número ≥ 14 px, negrita, borde Tinta, sobre una marca de casilla (#17, regla 6 y Visual/Audio).
    - Dibuja también el **resplandor tenue** del lado del castillo propio que mira al rival (art bible §2, *Planificar*).
    - Las dos marcas se van en el cuadro en que el estado deja `Reclutar`.

#### D. Animación fuera del combate

14. **Reposo.** Cada figura cicla sus cuadros de `reposo` con respiración mínima (1–2 px, ≤ 4 cuadros; art bible §5). El terreno, los muros enteros y las matas no se animan (art bible §3).
15. **`PiezaMovida`.** La figura se desplaza por código **por el camino** que trae el evento, con frenado suave y ≤ 0,25 s en total, sin ciclo de caminata (Movimiento, Visual/Audio). El jinete que pasa sobre otra pieza se dibuja por encima mientras pasa. `MovimientoDeshecho` (Máquina, #3) la devuelve de `destino` a `origen` en ≤ 0,25 s **recorriendo al revés el camino del último `PiezaMovida` de esa pieza**, que Render guarda en su estado de cliente (una pila de caminos por pieza, que se vacía al cerrar `Mover`). Es determinista porque sale de los eventos. Si no hay camino guardado, vuelve en línea recta. **La animación no bloquea**: el siguiente comando se acepta mientras la figura se desplaza (Movimiento, Game Feel).
16. **`Reclutado`.** Las piezas de la tanda **aparecen a la vez** en sus casillas, con su suelo de bando, en ≤ 0,5 s (Reclutamiento, Visual/Audio). Si la tanda es `automatico`, no hay marca sobre la pieza: el aviso «Elegido por el tiempo» es texto en la zona de avisos de #15 (regla 18) (decisión del 2026-11-26).
17. **Arquero en el muro.** El arquero que sube a un tramo propio se dibuja sobre el tramo con el **parapeto** del castillo, sin que la silueta pierda su forma (art bible §3). Al bajar o al romperse su tramo, el parapeto se va en el mismo cuadro.

#### E. La resolución

18. **Qué la dispara.** La resolución se presenta cuando llega la lista de eventos de Resolución (#9, regla 14), sea por `Resolver` o por `TurnTimeout` de atacar. Sin eventos de Resolución (lista de asignaciones vacía, o `FaseSaltada` de atacar) **no hay presentación**.
19. **Cola de presentación.** Los eventos de una misma respuesta se presentan **por grupos y en este orden**:
    1. la resolución (los tipos 1 a 6 de la regla 14 de #9);
    2. `TurnoTerminado` y, según el caso, `PartidaTerminada`, o bien las `CoberturaQuitada` por caducidad (las líneas se desvanecen) y `TurnoIniciado`. El contorno del tablero, el banner y el emblema del turno nuevo son de #15.
    3. Si llega después, la `Tirada` (placa de #17).

    El grupo 2 no empieza antes de que termine el grupo 1, que no se salta (regla 24); solo lo corta una rendición (Edge Cases). Los eventos que llegan con otro comando (por ejemplo `Tirada`) se presentan cuando llegan, sobre lo que esté en pantalla.
20. **Línea de tiempo única y simultánea (F1).** Toda la resolución comparte un solo reloj de presentación, que empieza en 0 al empezar el grupo:
    - **Gesto (0 → `t_impacto`):** en el cuadro 0 se van las placas de la vista previa (#16, regla 19), y todos los atacantes de `AtaqueEjecutado` hacen a la vez su animación de `ataque`. El **cuadro de golpe** de cada skin se cuadra con `t_golpe` (F1b): `t_impacto` en el cuerpo a cuerpo, y el momento de soltar para quien lanza un proyectil. Los proyectiles (flecha del arquero, piedra de la catapulta) recorren el arco de su flecha y llegan exactamente en `t_impacto`. Las flechas del plan se apagan en cuanto empieza el gesto («de plan a golpe», #9).
    - **Impacto (`t_impacto`, un solo cuadro):** se disparan juntos todos los `DanoRecibido` y `DanoATramo`, la explosión de cada demoledor y la caída de la piedra del área. En ese cuadro aparecen los números flotantes, las placas MUERE y ROTO, y la vida de cada ficha cambia de una vez, sin cuenta regresiva (#15, Edge Cases). La ficha de cada pieza que muere se va en ese mismo cuadro, junto con la aparición de su placa MUERE, y no con la figura (#15, regla 11) (decisión del 2026-11-26).
    - **Pulso (`t_impacto` → `t_impacto + t_pulso`):** un pulso de luz blanca cálida (~4000 K) sale de cada receptor con daño, con el tamaño de F2.
    - **Caída y rotura (después del pulso, durante `t_caida`):** cada `PiezaMuerta` por daño se desatura y cae, todas en el mismo cuadro. Cada `TramoRoto` pasa de su estado a hueco. Cada `CoberturaQuitada` corta su línea en ese mismo cuadro (Cobertura, Visual/Audio).
    - **Lectura (`t_lectura`):** quedan a la vista los números, MUERE y ROTO sobre las casillas.
    - **Salida (`t_salida`):** se desvanecen los números y las placas, el terreno vuelve a la luz base, y la figura muerta ya no está.

    **La duración no depende de cuántos eventos hay**, y la resolución completa dura `t_resolucion` ≤ `resolucion_max_s` (2,0 s).
21. **Qué se ve por evento.**
    - **Número flotante:** `vidaPerdida` del evento, solo si el receptor sobrevive. Una pieza que muere muestra solo MUERE y un tramo que se rompe solo ROTO, sin número (decisión del 2026-11-26). Es el mismo que mostró la vista previa (#16, regla 19; #9, Visual/Audio, *Restricciones*), y ningún número puede ser mayor que lo que el jugador pudo calcular.
    - **`neto` 0:** ícono de escudo y «0», **sin pulso de daño**.
    - **Escudero con `brutoDesviado` > 0:** su número lleva el ícono de escudo, y el tramo quebrado de la flecha y la línea eslabonada se resaltan durante el pulso. Un aliado cubierto con `bruto_pieza` 0 no tiene evento y no muestra nada.
    - **Fuego amigo** (receptor del jugador del turno): el mismo pulso, con el contorno discontinuo sobre la figura o el tramo.
    - **`DanoATramo`:** el tramo cambia de estado según `estado_tramo` (F4).
    - **Explosión (`PiezaMuerta` con motivo `Explosion`):** la figura del demoledor **no cae**, desaparece dentro de la explosión en `t_impacto`.

    El detalle de cada evento está en Visual/Audio.
22. **El combinado se ve como un solo golpe (pilar 2).** Para un receptor con `combinado` verdadero hay **un solo** pulso y un solo número, con el tamaño de `escala_combo(atacantesRivales)` (F2), el mismo escalón que la punta fundida de su flecha. No hay un pulso por atacante. Así lo que prometió la flecha es lo que cae.
23. **Marcas de batalla en el terreno (art bible §6).**
    - Cada área deja **tierra removida** en su casilla central.
    - Cada explosión deja **quemado** en la casilla del demoledor.
    - Cada tramo roto suma **escombros** junto al castillo.

    La tierra removida y el quemado se desvanecen según `marca_visible` (F5). Los escombros y el hueco del tramo son estado del castillo y **no se desvanecen nunca** (Castillo, Visual/Audio; el art bible §6 queda por corregir). Ninguna marca supera el contraste de una figura ni la saturación del terreno.
24. **No se salta (decisión del 2026-11-26).** La cosecha se ve siempre **entera**, de 0 a `t_resolucion`: ningún toque, clic ni tecla la acorta. Un toque o clic sobre el tablero mientras dura **se ignora**: no salta, no elige, no inspecciona y no manda comandos (Entrada, #13, Edge Cases). Los controles del HUD y de la placa siguen con sus propias reglas (#15, #17), y `Rendirse` es el único que corta la cosecha (Edge Cases). Un toque en el cuadro siguiente al fin va a Entrada como siempre.
25. **La Tirada espera a la presentación (decisión del jugador, 2026-11-26).** En partida local, el cliente es la autoridad que envía `Tirada` (Modificador, regla 9). La envía **cuando termina el grupo 1 de la cola** (la resolución, que siempre corre entera, regla 24), no antes. El grupo 2 (cambio de turno, #15) empieza en el mismo cuadro. En el centro hay una sola placa: desde el turno 2 la placa de la tirada (#17) lleva el renglón del turno y reemplaza al banner del HUD (#15, regla 13; decisión del jugador, 2026-11-26). Como el reloj del jugador no corre en `EsperandoTirada` (Máquina, regla 14), **la animación de la resolución no le cuesta tiempo al jugador siguiente** y la Open Question del Timer queda cerrada para el modo local. El costo va al tiempo de pared: ≤ `resolucion_max_s` por turno con ataques, fuera de los relojes, igual que las esperas de la tirada que el pilar 4 ya excluye. **Si el turno se cerró sin resolución** (sin asignaciones, o `FaseSaltada` de atacar), el cliente envía la `Tirada` cuando terminan la placa de #17 en curso y la leyenda central de #15 (`leyenda_ms`, ≤ 1,5 s), o en el mismo cuadro si no hay ninguna, fuera de los relojes (decisión del 2026-11-26). En el turno 1 no hay tirada (Máquina, regla 3) ni resolución previa. En el modo online la autoridad es el servidor (#28), y la espera queda como pregunta abierta.

#### F. Ambiente y pantalla

26. **Estados de ambiente (art bible §2).** Render aplica la gradación del terreno y la viñeta de cada estado. Los recursos de interfaz de esos estados (banners, placas, estandarte) son de #15 y #17.

    | Estado | Lo dispara | Terreno y viñeta | Transición |
    |---|---|---|---|
    | Planificar | `FaseIniciada` Reclutar o Mover | Base de media tarde (~5000 K) | ≤ 0,25 s |
    | Asignar ataques | `FaseIniciada` Atacar | −20% de brillo, viñeta media | ≤ 0,25 s |
    | Resolución | Grupo 1 de la cola | Pulso desde cada objetivo, después la base | La de F1 |
    | Tirada del Modificador | `Tirada` | −15%, viñeta suave, mientras está la placa | ≤ 0,25 s |
    | Distracción | `Tirada` con Distracción | Desaturado 40%, ~6500 K, ≤ 1,5 s | ≤ 0,25 s |
    | Tramo final | `TramoFinalIniciado` (Timer) | Atardecer ~3800 K, −10%, viñeta larga, **permanente** | 1 s, una vez |
    | Victoria / Derrota | `PartidaTerminada` | La de cada una en el art bible §2 (Victoria: la luz vuelve y la viñeta se centra en el Rey del ganador; Derrota: crepúsculo frío) | ≤ 0,25 s |

    Con el tramo final activo, los estados de fase se aplican **sobre** el atardecer, nunca lo reemplazan. *Turno del rival* (−10%, más frío) solo existe online: en local los dos jugadores miran la misma pantalla y no hay rival que mirar.
27. **Pantalla y escala (cierra la política de resolución que pidió #15).**
    - El diseño es de 1920 × 1080: casillas de 68 px, tablero de 1632 × 1020 en x 144 a 1776 e y 60 a 1080 (#15, regla 15).
    - Para cualquier otra ventana, todo se escala de forma **uniforme** con `escala_pantalla` (F3) y se centra. El terreno sigue más allá del tablero y se oscurece hacia los bordes (art bible §6), así que el sobrante de otra proporción se ve como terreno, no como bandas negras.
    - Con la escala por debajo de 1, #15 muestra su aviso de 1080p.
    - Los activos se exportan a 136 px por casilla (2×), así que a 4K se ven nítidos.
    - Render expone **`casillaEnPantalla(casilla)`** (rectángulo en píxeles), que #15 y #16 usan para anclar la ficha y la vista previa, y su inversa **`casillaEnPunto(x, y)`** (F3), que usa Entrada (#13, regla 3) para saber qué casilla se tocó. Nadie más convierte coordenadas.
28. **Sin cámara.** Hay una sola vista: el tablero entero, sin zoom ni desplazamiento en el MVP. El zoom táctil del móvil es de la Adaptación móvil (#41; Entrada, E3). Ningún efecto mueve, sacude ni acerca la vista.
29. **«Reducir efectos» (#23; opción guardada por el cliente hasta #20).**
    - Reemplaza el pulso de luz por un contorno breve del mismo tamaño y la explosión por un fundido.
    - Mantiene los números, MUERE, ROTO y la caída.
    - No cambia la línea de tiempo de F1.
    - Ningún efecto de Render, con o sin la opción, destella más de 3 veces por segundo (art bible §7).

#### G. Lo que Render no hace

30. Render no decide qué casillas son legales, qué recibe daño ni quién murió. No dibuja la ficha, el HUD ni el contenido de la vista previa. No muestra información oculta ni anticipa un resultado que el núcleo no emitió (anti-pilar «NOT información oculta»). No anima un orden entre golpes de una misma resolución. No deja que una skin cambie una marca o una cifra. No pausa ni frena ningún reloj (Timer, regla 11).

### States and Transitions

Render no tiene estado de juego. Tiene tres máquinas de **presentación**, todas del cliente y fuera de la huella.

**1. Presentación de la partida (una por pantalla de partida).**

| Estado | Entra con | Sale con | Qué dibuja |
|---|---|---|---|
| Inicio | `CrearPartida` aceptada | Primer cuadro dibujado | El despliegue completo **de golpe**, sin animación de entrada (Tablero, Visual/Audio) |
| Libre | Fin de cualquier grupo de la cola | Un grupo nuevo en la cola | Reposo, marcas de la fase, animaciones de comando (mover, reclutar, asignar), que no bloquean |
| Resolviendo | Grupo 1 de la cola (regla 19) | Fin de F1, o una rendición durante la cosecha (Edge Cases) | La línea de tiempo de la regla 20; los toques sobre el tablero se ignoran (regla 24) |
| Cambio de turno | Grupo 2 | Fin de sus transiciones (≤ 0,25 s; el banner es de #15) | Caducidad de coberturas, estado de ambiente de la nueva fase |
| Final | `PartidaTerminada` después de su resolución, o al cortar una rendición la cosecha | Salida de la pantalla de partida (#21) | Luz de Victoria o Derrota; el tablero queda como lo dejó el último evento |

| De → a | Disparador | Efecto |
|---|---|---|
| Libre → Resolviendo | Llegan eventos de Resolución | Se apagan las flechas del plan y empieza el gesto |
| Resolviendo → Libre | `t_resolucion` cumplido | Estado final dibujado; en local, el cliente envía `Tirada` (regla 25) |
| Resolviendo → Final | La resolución trae `PartidaTerminada` | Primero termina la resolución entera (no se salta, regla 24); después la luz del final (≤ 0,25 s). Render avisa a #15 el **fin del grupo 1** y el **fin de la luz del final**; la pantalla de resultado (#21) entra recién ahí |
| Resolviendo → Final | `PartidaTerminada` por rendición durante la cosecha | La cosecha se corta: en el cuadro siguiente todo pasa al estado final de la resolución y empieza la luz del final; no hay `Tirada` (decisión del 2026-11-26) |
| Libre → Final | `PartidaTerminada` sin resolución (rendición, reloj agotado, abandono) | Directo a la luz del final |
| Cualquiera → el mismo | Estado nuevo mientras anima | La animación en curso **de la misma entidad** salta a su final; las de otras entidades siguen y la cosecha no se interrumpe (regla 2) |

**2. Figura (una por pieza viva dibujada).**

| Estado | Entra con | Sale con |
|---|---|---|
| Apareciendo | `Reclutado` o Inicio | ≤ 0,5 s (reclutar) o 0 s (inicio) → Reposo |
| Reposo | Por defecto | Moviéndose, Atacando, Muriendo |
| Moviéndose | `PiezaMovida` o `MovimientoDeshecho` | Fin del camino (≤ 0,25 s) → Reposo |
| Atacando | Gesto de la regla 20 | Último cuadro de `ataque` → Reposo, o Muriendo si murió en esa resolución |
| Muriendo | `PiezaMuerta` motivo `Dano`, después del pulso | `t_caida` cumplido → **se borra**; no deja cuerpo (art bible §6) |
| Estallando | `PiezaMuerta` motivo `Explosion` | En `t_impacto` → se borra dentro de la explosión |

Una figura que ataca **y** muere en la misma resolución hace su gesto completo y después cae: la pieza muerta igual golpea (#9, regla 3).

**3. Tramo de muro (uno por tramo).** Sus estados los fija Castillo (#5): Entero, Mellado y Roto. Render los dibuja según `estado_tramo` (F4):

| De → a | Disparador | Efecto |
|---|---|---|
| Entero → Mellado | `DanoATramo` con `vidaDespues × 2 < vida_tramo` y `vidaDespues` > 0 | Melladuras en el borde superior, en el cuadro de impacto |
| Entero o Mellado → Roto | `TramoRoto` | Hueco con escombro bajo y borde quebrado, en `t_caida` (≤ 0,25 s); suma escombros al castillo |
| Entero → Entero, Mellado → Mellado | `DanoATramo` que no cruza el umbral | Solo el pulso y el número |
| Roto → cualquiera | **No existe** | El tramo no se repara (Castillo, regla 5) |

### Interactions with Other Systems

| Sistema | Qué entra a Render | Qué sale de Render | Dueño de la interfaz |
|---|---|---|---|
| #1 Librería | Estado público, eventos (regla 8), consultas puras | — | Librería |
| #2 Catálogo | Solo el ID de tipo (0 a 6) | — | Catálogo |
| #3 Máquina | `TurnoTerminado`, `TurnoIniciado`, `FaseIniciada`, `FaseSaltada`, `PartidaTerminada`; el lado del jugador del turno | — | Máquina (qué se emite); Render (cola y ambiente) |
| #4 Tablero | Grilla 24 × 15, casillas, castillos, `ladoDelRival`, despliegue | `casillaEnPantalla` para el cliente | Tablero (geometría); Render (pantalla) |
| #5 Castillo | Vida de cada tramo, `vida_tramo`, `enMuro`, `tramosRotos`, `DanoATramo`, `TramoRoto` | Los tres estados dibujados, el parapeto y los escombros | Castillo (estados y umbral); Render (dibujo) |
| #6 Movimiento | `MovimientosLegales` con costo, `PiezaMovida` con camino; `MovimientoDeshecho` (lo emite la Máquina, #3, regla 16) | — | Movimiento |
| #7 Alcance | `CasillasEnAlcance`, objetivos válidos, asignaciones con modo, zona de explosión | — | Alcance |
| #8 Cobertura | `Coberturas()`, `AliadosCubribles`, `CoberturaAsignada` / `Quitada` | — | Cobertura |
| #9 Resolución | Los eventos de la regla 14 con `combinado`, `atacantesRivales` y `brutoDesviado` | — | Resolución (forma); Render (presentación) |
| #10 Reclutamiento, #17 Interfaz del Modificador | `Reclutado`; lista de `K` casillas a numerar y cuándo; pedido de resplandor | — | #17 (cuáles y cuándo); Render (cómo) |
| #11 Modificador | `Tirada` con la cara (para el ambiente de Distracción y de la tirada) | En local, **el momento** en que el cliente envía `Tirada` (regla 25) | Modificador (la tirada); Render (cuándo termina la presentación) |
| #12 Timer | `TramoFinalIniciado` | — | Timer |
| #13 Entrada | Pieza elegida o en inspección, casilla pulsada | `casillaEnPunto` (F3); si la cosecha está en curso, para que Entrada ignore los toques sobre el tablero (regla 24) | Entrada (qué se elige); Render (cómo se ve) |
| #14 Localización | Nada: Render no tiene textos propios (MUERE y ROTO son de #16) | — | — |
| #15 Ficha y HUD | — | `casillaEnPantalla`, orden de capas, el cuadro de impacto (para cambiar la vida de una vez y quitar la ficha de las piezas que mueren), el fin del grupo 1, el corte por rendición y el fin de la luz del final | #15 (ficha y HUD); Render (anclas y tiempos) |
| #16 Vista previa | Contenido de las placas y números de la fase de ataque | Ancla en pantalla; las mismas placas y números durante la resolución | #16 (qué se muestra); Render (dónde y cuándo) |
| #19 Métricas | — | Skin rechazada al cargar (motivo), fps y tiempo de carga (errores y medidas técnicas) | Métricas (esquema) |
| #22 Audio | — | El reloj de presentación: `t_impacto`, `t_caida` y el corte por rendición, para sincronizar los golpes | Audio (qué suena); Render (cuándo) |
| #23 Accesibilidad, #20 Ajustes | «Reducir efectos», «Mostrar cuadrícula» (Tablero, UI Requirements) | — | #23 / #20 |
| #35 Repeticiones | Eventos guardados | El mismo dibujo (regla 3) | Render |
| #37 Skins, #38 Kit, #39 Tienda | Skins en el formato de la regla 6 | Validación de carga (regla 7); dibujo de una skin fuera de partida (vista previa de la tienda, Full Vision) | Render (formato); #38 (pruebas humanas) |

**«Mostrar cuadrícula»** (Tablero, UI Requirements; opción apagada por defecto): Render dibuja una línea fina y tenue entre casillas sobre la capa 1, por debajo del suelo de bando.

**Supuestos PROVISIONALES:** Audio (#22), Accesibilidad (#23), Ajustes (#20), Repeticiones (#35) y Skins, Kit y Tienda (#37 a #39) no tienen GDD. Vista previa (#16) y Métricas (#19) se escribieron en paralelo y se verificaron al cerrar la tanda 4. Con #16 se verificaron, al cerrar esta sesión, el momento de la entrega (su regla 19) y la casilla del arquero (su regla 11). Con #19 se verificó al cerrar la tanda 4: trata los eventos de cliente de Render en su Open Question (l.590), donde `ResolucionSaltada` queda cerrada porque la cosecha no se salta (regla 24).

## Formulas

*Lente `systems-designer` aplicada sin agente. Cada fórmula se probó con cada variable en su mínimo, en su máximo y en 0. Los bordes que rompen algo están en la nota de cada una.* Ninguna fórmula entra al núcleo ni a la huella: son reglas de presentación del cliente. Todos los tiempos son de presentación (reloj del cliente) y ningún resultado depende de ellos.

### F1. `t_resolucion` (duración de la cosecha)

La fórmula `t_resolucion` se define así:

`t_impacto = t_anticipacion + t_vuelo`
`t_resolucion = t_impacto + t_pulso + t_caida + t_lectura + t_salida`, con la condición `t_resolucion ≤ resolucion_max_s` y `t_pulso + t_caida + t_lectura ≥ lectura_min_s`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `t_anticipacion` | float (s) | 0,10–0,40 | data file | Desde el inicio hasta el cuadro de golpe de un atacante cuerpo a cuerpo, sin vuelo |
| `t_vuelo` | float (s) | 0,00–0,30 | data file | Vuelo del proyectil (flecha, piedra). El que no lanza nada golpea en `t_impacto` igual |
| `t_pulso` | float (s) | 0,15–0,40 | data file | Pulso de luz desde cada receptor (art bible §2: ~0,3 s) |
| `t_caida` | float (s) | 0,15–0,25 | data file | Caída de los muertos y rotura de tramos (art bible §7: transiciones ≤ 0,25 s) |
| `t_lectura` | float (s) | 0,30–1,00 | data file | Números, MUERE y ROTO quietos a la vista |
| `t_salida` | float (s) | 0,10–0,25 | data file | Fundido de números y placas, y vuelta del terreno a la base |
| `resolucion_max_s` | float (s) | 2,0 | constant | Techo del art bible (principio 3; Resolución, Game Feel). Subirlo es una decisión de pilar 4, no de ajuste |
| `lectura_min_s` | float (s) | 0,75 | constant | Tiempo mínimo en que el número flotante está a la vista. Por debajo de eso, un número de 2 cifras no se alcanza a leer junto con su placa |

**Output Range:** con los valores por defecto (0,20 + 0,15 + 0,30 + 0,25 + 0,50 + 0,25) da **1,65 s**, con el impacto en 0,35 s y el número a la vista 1,05 s. El mínimo de los rangos da 0,80 s, pero viola `lectura_min_s` (0,15 + 0,15 + 0,30 = 0,60 < 0,75), así que el mínimo válido con esas cifras sube `t_lectura` a 0,45 s y da **0,95 s**. El máximo de los rangos sumaría 2,60 s.

**Validación:** la config se rechaza al cargar si `t_resolucion` > `resolucion_max_s` o si no se cumple la condición de `lectura_min_s`. Es el mismo patrón que las perillas de la placa del Modificador (#17, F1). **No depende de la cantidad de ataques**: 1 ataque o 61 duran lo mismo. Con 0 eventos de Resolución no hay presentación (regla 18), y la duración es **0**, no `t_resolucion`.

**Example:** cuatro atacantes sobre un soldado. Todos empiezan el gesto en 0. El arquero suelta en 0,20 s, su flecha vuela 0,15 s, y los cuatro impactan en 0,35 s. El pulso dura hasta 0,65 s, la caída hasta 0,90 s, la lectura hasta 1,40 s y la salida hasta 1,65 s. En local, la `Tirada` sale en 1,65 s.

**F1b. Ritmo del gesto (cuadro de golpe).** Para una skin con `n` cuadros de `ataque` y el cuadro de golpe en la posición `k` (1 ≤ `k` ≤ `n`):

`t_golpe = t_impacto − t_vuelo` si la pieza lanza un proyectil, si no `t_impacto`
`dur_pre = t_golpe / (k − 1)` si `k ≥ 2`; con `k = 1` el cuadro de golpe se muestra desde 0 hasta `t_golpe`, y los cuadros 2 a `n` ocupan [`t_golpe`, `t_golpe + t_pulso`], cada uno `dur_post`
`dur_post = t_pulso / max(1, n − k)`

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `n` | int | 1–8 | data file (skin) | Cuadros de `ataque` de la skin (regla 6) |
| `k` | int | 1–`n` | data file (skin) | Posición del cuadro de golpe |
| `proyectil` | bool | — | calculated | Verdadero si el modo es Área, o si es Directo y la casilla objetivo **no** está en el contorno de 8 de la pieza (Tablero, regla 2). La explosión nunca lanza |

Bordes: con `k = 1` no hay división por cero (rama aparte). Con `n = k`, después del golpe se sostiene el último cuadro durante `t_pulso`. Con `n = 1` la figura muestra un solo cuadro de ataque desde 0 hasta el fin del pulso. Ejemplo: arquero con `n` = 6 y `k` = 4, por defecto: `t_golpe` 0,20 s, `dur_pre` 0,067 s (3 cuadros de anticipación), `dur_post` 0,15 s (2 cuadros de vuelta).

### F2. `escala_combo` (tamaño del golpe combinado)

La fórmula `escala_combo` se define así:

`escala_combo(a) = 1` si `a ≤ 1`; `= escala_2` si `a = 2`; `= escala_tope` si `a ≥ 3`
`radio_pulso_px = radio_pulso_base × escala_combo(a) × casilla_px / 68`

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `a` | int | 0–61 | calculated | En la **resolución**, `atacantesRivales` del receptor (#9, F5). En la **flecha** de la fase de ataque, la cantidad de piezas distintas cuya flecha termina en esa punta (con el desvío, en la del escudero) |
| `escala_2` | float | 1,5 | constant | Escalón de 2 atacantes (art bible §4, *Punta fundida*) |
| `escala_tope` | float | 2,0 | constant | Escalón de 3 o más (art bible §4: «tope») |
| `radio_pulso_base` | int (px a 1080p) | 24–34 | data file | Radio del pulso de un golpe simple. Por defecto 30: media casilla menos el margen del contorno |
| `casilla_px` | float | — | calculated (F3) | Lado de la casilla en pantalla |

**Output Range:** el radio va de 30 px (`a` 0 o 1) a 60 px (`a` ≥ 3) a 1080p, y nunca supera 2 × `radio_pulso_base` = 68 px: con el tope, el pulso cubre a lo sumo la casilla propia y la mitad de las vecinas, y solo durante `t_pulso`. Con `a` = 0 (fuego amigo) la escala es 1: el pulso del fuego amigo no se agranda, y su marca es el contorno discontinuo (regla 21).

**Por qué los mismos escalones que la flecha:** la punta fundida promete el combo antes de resolver y el pulso lo cumple. Con escalas distintas, la flecha y el impacto dirían cantidades distintas. El tope en 3 es del art bible. Más escalones (4, 5…) no se distinguen a 68 px y empujan a efectos que tapan fichas.

**Example:** 3 soldados y 1 arquero sobre un soldado: `a` = 4 → escala 2, radio 60 px, el mismo grosor ×2 que tenía la punta. Un soldado solo: radio 30 px. Dos atacantes cuyo daño absorbe el escudo: neto 0, así que **no hay pulso** (regla 21) aunque `a` = 2. Hay ícono de escudo y «0».

### F3. `escala_pantalla` (política de resolución)

La fórmula `escala_pantalla` se define así:

`escala_pantalla = min(W / 1920, H / 1080)`
`casilla_px = 68 × escala_pantalla`
`origen_x = (W − 1920 × escala_pantalla) / 2 + 144 × escala_pantalla`, `origen_y = (H − 1080 × escala_pantalla) / 2 + 60 × escala_pantalla`
`casillaEnPantalla(c, f) = (origen_x + c × casilla_px, origen_y + f × casilla_px, casilla_px, casilla_px)`
`casillaEnPunto(x, y) = (c, f)` con `c = floor((x − origen_x) / casilla_px)` y `f = floor((y − origen_y) / casilla_px)`; si `c` cae fuera de 0–23 o `f` fuera de 0–14, es «ninguna». Con la escala en 0 (ventana minimizada) devuelve «ninguna» sin dividir. **Example:** a 2560 × 1440, (1000, 500) da (8, 4), y (191, 500) da «ninguna».

**Variables:**

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `W`, `H` | int (px) | 0–7680 / 0–4320 | calculated (ventana) | Tamaño de la ventana del juego |
| `c`, `f` | int | 0–23 / 0–14 | calculated (Tablero) | Columna y fila |
| 1920, 1080, 68, 144, 60 | int | — | constant | Diseño de 1080p (art bible §7; HUD, regla 15) |

**Output Range:** a 1920 × 1080, 1 (casilla de 68 px); a 3840 × 2160, 2 (136 px, el tamaño de exportación); a 2560 × 1080 (21:9), 1 con 320 px de terreno extra a cada lado; a 1280 × 720, 0,667 (casilla de 45 px: #15 muestra el aviso de 1080p). **Con `W` o `H` en 0** (ventana minimizada) la escala es 0 y Render **no dibuja ese cuadro**, en lugar de dividir o anclar en 0. Las animaciones siguen su reloj (regla 2).

**Example:** 2560 × 1440 da escala 1,333, casilla de 90,7 px y origen (192, 80). La casilla (5, 7) empieza en x 645,3 e y 714,7, con 90,7 px de lado.

### F4. `estado_tramo` (dueño: Castillo, #5)

La regla es de Castillo (Visual/Audio y AC-31) y Render solo la aplica. Se repite para que el dibujo tenga una fuente cerrada:

`estado_tramo(v) = Roto` si `v = 0`; `Mellado` si `v × 2 < vida_tramo`; `Entero` si no

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `v` | int | 0–`vida_tramo` | calculated (estado) | Vida actual del tramo (`vidaDespues` en la resolución) |
| `vida_tramo` | int | 1–1.000 (hoy 30) | data file (Castillo, config) | Vida inicial del tramo |

**Output Range:** tres estados. Con `vida_tramo` 30: 30 a 15 es Entero (15 × 2 = 30 no es < 30), 14 a 1 es Mellado y 0 es Roto. Con `vida_tramo` 1, el tramo pasa de Entero a Roto sin pasar por Mellado. **Example:** un tramo de 30 que recibe 16 queda en 14 → Mellado, en el cuadro de impacto.

### F5. `marca_visible` (marcas de batalla en el terreno)

La fórmula `marca_visible` se define así:

`marca_visible(m) = turnos_desde(m) < marcas_turnos`

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `turnos_desde(m)` | int | 0–∞ | calculated | Cantidad de `TurnoIniciado` desde la resolución que dejó la marca `m` (tierra removida o quemado) |
| `marcas_turnos` | int | 0–10 | data file | Turnos de partida que dura una marca. Por defecto 3 (art bible §6, «se desvanecen a los 3 turnos») |

**Output Range:** booleano. La marca se desvanece en ≤ 0,25 s en el cuadro del `TurnoIniciado` que la vence. Con `marcas_turnos` = 0 no se dibuja ninguna marca. **Con más marcas que casillas** no hay tope que romper: una casilla guarda **una sola** marca, la más reciente (si cae una piedra donde ya había un quemado, queda la tierra removida y su cuenta vuelve a 0). Así hay como mucho 360 marcas. **No aplica** a los escombros ni al hueco del tramo, que son estado (regla 23). **Example:** con 3, una explosión del turno 8 deja el quemado visible en los turnos 9 y 10 y se va al iniciar el 11.

### F6. `memoria_skin` (presupuesto de texturas por skin)

La fórmula `memoria_skin` se define así:

`memoria_skin = (Σ_tipo (n_reposo + n_ataque) + n_castillo) × 136 × 136 × 4 bytes`

| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| `n_reposo`, `n_ataque` | int | 1–4 / 1–8 | data file (skin) | Cuadros por tipo (regla 6) |
| `n_castillo` | int | 0 o 5–10 | data file (skin) | 3 estados del tramo + 1 a 6 interiores + 1 parapeto, o 0 sin castillo |
| 136 × 136 × 4 | int (bytes) | 73.984 | constant | Un cuadro RGBA de 136 px, sin comprimir (art bible §8) |

**Output Range:** la skin más chica (7 tipos × 2 cuadros, sin castillo) ocupa 1,04 MB. La skin básica del MVP (7 × (4 + 6) + 10) ocupa **5,92 MB**. La más grande que admite el formato (7 × (4 + 8) + 10) ocupa **6,95 MB**. Con dos skins al máximo son 13,9 MB, **dentro de los 40 MB por partida** (art bible §8) sin contar compresión, que no está verificada en web (NO VERIFICADO, art bible §8). Los topes de cuadros del formato son los que garantizan el presupuesto: subirlos exige rehacer este cálculo. **Example:** la skin básica del MVP más el terreno (6 variantes y 2 marcas, 0,59 MB) da 6,5 MB; con dos jugadores usando la básica se carga **una sola vez**.

**Nota de atlas (F6 cuenta cuadros, no atlas).** Empacados a 136 px en un atlas de potencia de 2, los 80 cuadros de la básica piden 2048 × 1024 RGBA, unos **8,4 MB**, contra los 5,92 MB de F6. Con mipmaps, que hacen falta porque a 1080p el arte se ve a la mitad, se suma ~33% (~11,2 MB). La skin al tope (94 cuadros) entra en el mismo atlas. F6 subestima alrededor de un 40%, y no cuenta efectos, fuentes ni marcas del sistema. Dos skins al tope con mipmaps (~22,4 MB) más el terreno siguen entrando en 40 MB, pero el presupuesto real lo mide el ADR de render con el atlas. Los 40 MB y las 150 llamadas de dibujo son **provisionales**: `project.yaml` no tiene `performance.*` (art bible, l.608-613).

### Límites y lazos

- **Ningún lazo de retroalimentación:** la presentación no cambia el estado, así que nada de lo que dibuja Render puede volver al juego.
- **La única variable que sale del núcleo con cota abierta** es `a` (0 a 61), y F2 la satura en 3.
- **Los divisores** son `k − 1` en F1b, con rama aparte para `k = 1`, y `casilla_px` en `casillaEnPunto` (F3), con rama aparte para la escala 0. El resto de F3 divide por constantes.

## Edge Cases

*Lente `systems-designer` sobre las Secciones C y D (releídas del archivo). Valores del Catálogo v5 y `vida_tramo` 30.*

### Resolución

- **If un demoledor que explota también recibe golpes** (#9, regla 9: igual tiene `DanoRecibido`): su figura desaparece en `t_impacto` dentro de la explosión y **no se muestra su número**. Una pieza que deja de existir en ese cuadro no tiene dónde mostrarlo, y su salida no depende de su vida. Las Métricas sí reciben el evento.
- **If dos demoledores pegados explotan juntos:** desaparecen los dos en `t_impacto`, con dos explosiones en el mismo cuadro y dos quemados. No hay cascada visual: una pieza fuera de sus vecinas no muestra nada (#9, AC-40).
- **If una pieza ataca y muere en la misma resolución:** hace su gesto completo y su proyectil llega en `t_impacto`, y después del pulso cae con las demás (#9, regla 3; States, figura).
- **If una casilla tiene un arquero sobre su tramo y recibe un golpe:** hay **dos números** en la misma casilla, con la disposición de las dos placas de la vista previa (#16, regla 11). El de la pieza (vida perdida) va **abajo**, en el ancla de la ficha, y el del tramo (vida perdida del tramo) va **arriba de la casilla**, con el ícono de muro a su izquierda. Si el arquero muere o el tramo se rompe, en ese lugar va la placa MUERE o ROTO y no el número. Hay un solo pulso, con la escala mayor de los dos receptores.
- **If el tramo bajo un arquero se rompe y el arquero sobrevive:** en `t_caida` el tramo pasa a hueco, el parapeto se va en ese cuadro y la figura queda en la casilla, ahora sobre el hueco (#9, AC-21).
- **If el arquero muere y su tramo también se rompe:** la figura cae y el tramo pasa a hueco en el mismo `t_caida`. La casilla queda libre y sin tramo.
- **If el escudo absorbe todo (`neto` 0), aunque sea combinado:** no hay pulso. Se ven el ícono de escudo y «0», y la vida de la ficha no cambia. El combinado no se dibuja más grande cuando no pasó nada: la escala solo agranda un daño que existe.
- **If un área o una explosión caen solo sobre casillas vacías o huecos sin pieza:** se ven el gesto, la caída de la piedra o la explosión y la marca de tierra removida o de quemado. No hay pulso ni número (sin `DanoRecibido`).
- **If el centro del área está en un borde o una esquina:** el anillo doble y la piedra solo marcan las casillas dentro del tablero (6 o 4 casillas). La tierra removida queda en el centro.
- **If el escudero recibe golpes desviados y golpes propios a la vez:** un solo número (la vida perdida total), con el ícono de escudo, o MUERE sin número si muere. Su escala es la de sus `atacantesRivales`, que ya cuenta a los desviados (#9, regla 12).
- **If varias piezas propias golpean una pieza propia (fuego amigo combinado):** `a` = 0 y la escala es 1 (F2). El combo es una promesa contra el rival, y el fuego amigo se marca con el contorno discontinuo, no con un impacto más grande.
- **If la punta de la flecha prometió menos que el pulso** (por ejemplo, un soldado con flecha directa y el área de una catapulta sobre el mismo objetivo: 1 flecha en esa punta, `atacantesRivales` 2): el pulso sale ×1,5. La escala del pulso cuenta los golpes reales; la flecha del área termina en su centro, no en cada vecina. El número es el de la vista previa (#16), que es el dato que no puede diferir.
- **If la resolución mata a los dos Reyes** (`DobleRey`): las dos figuras caen en el mismo cuadro, termina la resolución, y después viene la luz de Victoria del jugador del turno (Máquina, regla 13; States).
- **If la resolución termina la partida:** la cosecha se ve entera (regla 24) y después sigue la luz del final. No hay `Tirada` (no hay turno siguiente). La pantalla de resultado (#21) y la placa de #15 entran recién al terminar la luz del final (States).
- **If el jugador se rinde durante la cosecha** (en local, `Rendirse` vale en `EsperandoTirada`; Máquina, matriz de comandos): la cosecha **se corta** (decisión del 2026-11-26). En el cuadro siguiente al `PartidaTerminada` todo pasa al estado final de la resolución, como si hubiera terminado la salida, y empieza directo la luz del final (Resolviendo → Final). No hay `Tirada`. Es el único corte de la cosecha, y lo dispara un control de #15, no un toque sobre el tablero (regla 24).
- **If el jugador se rinde, se agota su reloj o llega `CerrarPartida` durante `Atacar`:** no hay eventos de Resolución, así que no hay presentación ni flechas convertidas en golpes. Las flechas se borran y sigue la luz del final (Libre → Final).
- **If la lista de asignaciones está vacía o la fase de atacar se saltó (`FaseSaltada`):** no hay presentación y, en local, la `Tirada` sale al terminar la placa y la leyenda pendientes (≤ 1,5 s), o enseguida si no hay ninguna (regla 25).

### Tiempo, entrada y cola

- **If el jugador toca el tablero durante la resolución:** el toque se ignora (regla 24): la cosecha sigue hasta `t_resolucion`, y el toque no elige, no inspecciona ni manda comandos. **Un toque que llega en el cuadro siguiente al fin** va a Entrada como siempre. Un toque sobre `Rendirse` es de #15 y corta la cosecha (Resolución). Mientras dura el estado `EsperandoTirada`, Entrada no tiene comandos que mandar.
- **If el cliente pierde un tramo de cuadros** (un tirón de rendimiento) **y el reloj de presentación pasa por encima de `t_impacto` o del inicio de `t_caida`:** esos hitos se dibujan **al menos un cuadro**, aunque llegue tarde. Así el cambio de vida y la caída nunca se saltean, y las Repeticiones siguen dibujando lo mismo.
- **If la ventana se minimiza (`W` o `H` en 0) durante la resolución:** no se dibuja (F3), el reloj de presentación sigue, y la `Tirada` sale a su hora. Perder el foco no pausa nada, igual que el Timer (decisión de la tanda 1). Al volver, se dibuja el instante en que va el reloj de presentación, o el estado final si la cosecha ya terminó: minimizar no la salta ni la alarga.
- **If la ventana cambia de tamaño durante una animación:** F3 se recalcula en el cuadro siguiente y la animación sigue en las casillas nuevas, sin reiniciarse.
- **If llega un `MovimientoDeshecho` mientras la figura todavía se desplaza:** el desplazamiento salta a su final y empieza la vuelta (regla 2).
- **If llega un estado nuevo con varios grupos pendientes:** la cola los presenta en orden y ninguno se pierde. Un grupo cuya animación ya no tiene sentido (la pieza ya no existe) salta a su final.
- **If el tramo final empieza en el mismo comando que cierra una resolución:** el atardecer de 1 s empieza con el grupo 2, nunca durante la resolución (el pulso es del estado *Resolución*).

### Skins y carga

- **If la skin de un jugador falla la validación** (falta un tipo, trae el tipo 7, un cuadro con otro tamaño, una versión de formato desconocida): ese jugador juega con la **skin básica completa** y el motivo se registra (regla 7). La partida no se frena.
- **If una skin válida no trae castillo:** su castillo es el de la skin básica, y sus figuras siguen siendo las de la skin.
- **If los dos jugadores eligen la misma skin:** se carga una sola vez (F6). El bando lo dicen el suelo, la orientación y el emblema.
- **If la `miraHacia` de la skin ya coincide con el lado del rival:** no se espeja. Si no coincide, se espeja (regla 9). Una skin con `miraHacia` izquierda y una con derecha se ven iguales en partida.
- **If la skin trae 1 solo cuadro de ataque, o el golpe es el primer cuadro:** F1b sin división por cero. La figura sostiene ese cuadro hasta el fin del pulso.

### Carga y rendimiento

- **If una resolución llega al tope** (393 eventos; hasta 61 piezas y 32 tramos receptores con pulso, menos las casillas compartidas por un arquero sobre su tramo; #9, regla 15): todos los pulsos, números y placas se dibujan en el mismo cuadro con efectos de un **conjunto reservado por tipo**, sin reservar memoria en la resolución (Acceptance Criteria):
  - pulsos ≤ receptores de pieza más tramos (93);
  - números y placas ≤ lo mismo (93);
  - proyectiles ≤ `AtaqueEjecutado` con proyectil (61);
  - explosiones ≤ demoledores que explotan;
  - polvo de rotura ≤ 32 tramos.

  Las cotas salen de los sumandos de `eventos_resolucion_max` del registro; su reparto por evento lo confirma la historia del pool. Si el p95 de los últimos 10 cuadros pasa de 16,6 ms durante el grupo 1, los pulsos pasan a contorno, como con «Reducir efectos» (regla 29). Se mantiene la línea de tiempo y no se omite ningún número, MUERE ni ROTO: se degrada el efecto, nunca la información. **El tope de partículas y de overdraw es TBD** (performance-analyst, con el primer build web): al tope son 93 pulsos de hasta 120 px de diámetro superpuestos, y el riesgo está en web móvil.
- **If hay más marcas de terreno que casillas:** una por casilla, la más reciente (F5).

### Estrategias degeneradas

- *Tocar sin parar durante la cosecha para apurarla:* no tiene efecto. La cosecha no se salta y el toque se ignora (regla 24) (decisión del 2026-11-26). Si se siente lenta, es una pregunta de playtest (Open Questions).
- *Usar una skin que tapa la ficha o imita el color de bando:* imposible por formato (regla 6: no hay campos para eso, y el lienzo es una casilla). Los colores prohibidos los rechaza el Kit (#38).

## Dependencies

**Upstream (lo que Render necesita):**

| Sistema | Tipo | Interfaz | Estado del GDD |
|---|---|---|---|
| #1 Librería de reglas | Dura | Estado público, eventos (regla 8), consultas puras | Existe |
| #2 Catálogo de piezas | Dura | Solo el ID de tipo 0 a 6 (regla 9 del Catálogo; AC-31 de skin inválida) | Existe; nombra a #18 |
| #3 Máquina de turno | Dura | Eventos de turno y fase, `PartidaTerminada` | Existe; nombra a #18 |
| #4 Tablero | Dura | Grilla, castillos, `ladoDelRival`, despliegue | Existe; nombra a #18 |
| #5 Castillo y muros | Dura | Vida de tramos, `enMuro`, `DanoATramo`, `TramoRoto`, umbral de mellado | Existe; nombra a #18 (AC-31 es de este GDD) |
| #6 Movimiento | Dura | `PiezaMovida` con camino, `MovimientosLegales` con costo | Existe; nombra a #18 |
| #9 Resolución simultánea | Dura | Eventos de la regla 14 | Existe; nombra a #18 (AC-43 es de este GDD) |
| #7 Alcance, #8 Cobertura | Dura | Alcance, objetivos, asignaciones, coberturas | Existen; nombran a #18 |
| #10 Reclutamiento, #17 Interfaz del Modificador | Dura (#17) / Blanda (#10) | `Reclutado`; casillas a numerar; resplandor | Existen; nombran a #18 |
| #13 Entrada y selección | Dura | Selección, inspección, pulsación | Existe; nombra a #18 |
| #11 Modificador | Blanda | `Tirada` (ambiente); el envío local de la `Tirada` espera a Render | Existe; nombra a #18 (l.105, l.149, l.350) |
| #12 Timer | Blanda | `TramoFinalIniciado` | Existe; nombra a #18 |
| #14 Localización | Blanda | Ninguna clave propia | Existe; nombra a #18 (pregunta cerrada: Render no lleva texto) |
| #15 Ficha y HUD | Dura | Orden de capas compartido; consume `casillaEnPantalla` | Existe; nombra a #18 |
| #16 Vista previa | Blanda | Contenido de placas y números | Existe (tanda 4); nombra a #18 |

**Downstream (lo que depende de Render):**

| Sistema | Tipo | Qué recibe | Estado |
|---|---|---|---|
| #15 Ficha y HUD | Dura | `casillaEnPantalla`, política de escala; los tiempos de la presentación (impacto, caída, fin del grupo 1, corte por rendición, fin de la luz del final); #15 muestra el estado presentado (#15, regla 1) | Existe |
| #16 Vista previa | Blanda | Ancla en pantalla y la presentación en la resolución | Existe (tanda 4) |
| #19 Métricas | Blanda | Skin rechazada, fps, tiempo de carga | Existe (tanda 4); los eventos de cliente siguen en su Open Question |
| #22 Audio | Dura | El reloj de presentación (`t_impacto`, `t_caida`, corte por rendición) | Sin GDD |
| #23 Accesibilidad | Blanda | «Reducir efectos», «Mostrar cuadrícula» | Sin GDD |
| #35 Repeticiones | Dura | Dibujo determinista desde eventos | Sin GDD |
| #37 Skins, #38 Kit, #39 Tienda | Dura | Formato de skin, validación de carga, dibujo fuera de partida | Sin GDD |

**Bidireccionalidad:** todos los existentes nombran a #18, #11 Modificador incluido (l.105, l.149, l.350) desde el cierre de la tanda 4. Lo que falta va en *Cambios pedidos a otros GDD* (Cross-References).

## Tuning Knobs

*Lente `systems-designer`: las perillas salen de las variables `data file` de las fórmulas. Las `constant` no son perillas. Todas viven en la **config de presentación del cliente**, no en la config de reglas: cambiarlas no sube la versión de reglas ni toca la huella.*

| Perilla | Por defecto | Rango seguro | Qué cambia | Si sube mucho | Si baja mucho |
|---|---|---|---|---|---|
| `t_anticipacion` | 0,20 s | 0,10–0,40 | Cuánto dura el gesto antes del golpe | El golpe se siente lento y come la lectura (F1) | El gesto no se lee: los ataques parecen teletransportados |
| `t_vuelo` | 0,15 s | 0,00–0,30 | Vuelo de flecha y piedra | Los proyectiles se ven, pero el impacto se demora | 0: el arquero «golpea» a distancia sin proyectil, y no se lee de dónde vino |
| `t_pulso` | 0,30 s | 0,15–0,40 | Duración del pulso de luz (art bible §2) | Pasa de destello a brillo sostenido y tapa la casilla | No se nota el combo |
| `t_caida` | 0,25 s | 0,15–0,25 | Caída de muertos y rotura de tramos | Por encima de 0,25 rompe la regla de transiciones del art bible | Las muertes desaparecen sin verse caer |
| `t_lectura` | 0,50 s | 0,30–1,00 | Cuánto quedan números y placas | Alarga la cosecha (pilar 4) | Junto con `t_pulso` y `t_caida` no llega a `lectura_min_s` y la config se rechaza |
| `t_salida` | 0,25 s | 0,10–0,25 | Fundido final | — (≤ 0,25 por el art bible) | Corte brusco |
| `radio_pulso_base` | 30 px | 24–34 | Tamaño del pulso de un golpe simple (F2) | Con el tope ×2 pasa de 68 px e invade dos casillas vecinas | El ×1,5 y el ×2 no se distinguen del simple |
| `marcas_turnos` | 3 | 0–10 | Cuánto duran tierra removida y quemado (F5) | El tablero se ensucia y compite con las figuras (art bible §6) | 0: el terreno deja de contar la batalla |

**Interacciones:**
- Las seis perillas de tiempo **suman contra un techo** (`resolucion_max_s`, 2,0 s), y tres de ellas contra un piso (`lectura_min_s`, 0,75 s). Subir una obliga a bajar otra. La config valida las dos condiciones al cargar (F1).
- `t_vuelo` y `t_anticipacion` mueven juntas `t_impacto`, y con él el cuadro en que Audio (#22) suena el golpe.

**Perillas ajenas que cambian lo que dibuja Render (no se duplican):**
- `vida_tramo` (Castillo): cambia el umbral de mellado (F4).
- La duración del desplazamiento (≤ 0,25 s, Movimiento) y de la aparición (≤ 0,5 s, Reclutamiento).
- `leyenda_ms` (HUD), que fija el banner del grupo 2.

**Constantes que no son perillas:**
- `resolucion_max_s` 2,0 y `lectura_min_s` 0,75 (pilar 4 y lectura).
- `escala_2` 1,5 y `escala_tope` 2,0 (art bible §4, iguales a la punta).
- Las medidas de 1080p (art bible §7).
- Los topes de cuadros del formato de skin, que garantizan F6: subirlos sube `versionFormato`.

## Visual/Audio Requirements

*Lente `art-director` (modo `full`, sin agente), con `design/art/art-bible.md` leído por sección: §1 (regla visual y principios), §2, §3, §4, §5, §6, §7 y §8. Este GDD es el dueño del dibujo; la columna de audio es dirección y la especificación es de #22.*

**Principios del art bible que más aplican:**
- **Regla visual (§1):** lo que decide la jugada se lee primero (ficha, figura, bando, terreno), y ante la duda se apaga lo de abajo.
- **Principio 2, «El combo se ve como un solo golpe»:** reglas 12 y 22, F2.
- **Principio 3, «El ritmo del turno manda sobre la animación»:** F1, regla 25.
- **§3 *Formas protagonistas y de apoyo*:** lo de apoyo no se anima ni supera a una figura en contraste.
- **§6 *El terreno cuenta la batalla*.**

### Feedback por evento

| Evento | Visual | Audio (dirección) |
|---|---|---|
| Inicio de partida | El despliegue aparece completo, sin entrada (Tablero) | El de inicio (#22) |
| Elegir pieza / inspeccionar | Contorno grueso / contorno doble fino (regla 10) | Toque corto; el mismo para piezas rivales |
| Destinos | Puntos blancos al 60% con el costo adentro | — |
| `PiezaMovida` / `MovimientoDeshecho` | Desplazamiento por el camino, ≤ 0,25 s, frenado suave | Uno por comando, no por casilla |
| Arquero sube al muro | Figura sobre el tramo con parapeto | Apoyo sutil |
| `Reclutado` | La tanda aparece a la vez, ≤ 0,5 s; antes, las casillas numeradas y el resplandor del lado del frente | Un golpe de llegada por tanda |
| `AtaqueAsignado` directo / área / explosión | Arco con punta fundida (×1 / ×1,5 / ×2) / arco con anillo doble / mecha con contorno dentado en las 8 vecinas | Trazo que sube con la cantidad / tensión de cuerda / mecha |
| Lo propio en zona | Contorno discontinuo sobre la figura o el tramo propio | Aviso discreto, una vez por asignación |
| `CoberturaAsignada` | Línea eslabonada acero, sin punta; la ven los dos jugadores | Golpe metálico sordo |
| Resolución: gesto | Todos los atacantes a la vez; las flechas del plan se apagan; vuelan los proyectiles | Ataques de cada tipo, sin apilarse |
| Resolución: impacto (`t_impacto`) | Pulso cálido ~4000 K desde cada receptor, con el tamaño de F2; número flotante (vida perdida; ninguno con MUERE o ROTO); MUERE y ROTO; la vida de la ficha cambia de una vez | **Un** golpe por receptor, más pleno con `atacantesRivales`; MUERE y ROTO con prioridad de mezcla |
| Neto 0 | Ícono de escudo y «0», sin pulso | Golpe amortiguado |
| Desvío | La flecha quebrada y la línea eslabonada se resaltan; número con ícono de escudo sobre el escudero | Golpe de escudo sobre el combinado |
| Fuego amigo | El mismo pulso, con contorno discontinuo | Sin cue extra (la sorpresa ya se evitó en la vista previa) |
| Área | La piedra cae en el centro; anillo de polvo sobre las vecinas; deja tierra removida | Disparo y caída grave |
| Explosión | Estallido sobre las 8 vecinas; el demoledor desaparece dentro; deja quemado | El golpe más fuerte de la resolución |
| `DanoATramo` | Pulso sobre la piedra; melladuras si cruza el umbral (F4) | Golpe sobre piedra |
| `TramoRoto` | Hueco con escombro bajo y borde quebrado en `t_caida`; suman escombros | Derrumbe; más fuerte con el segundo tramo del castillo |
| `PiezaMuerta` (daño) | Se desatura y cae, todas juntas; sin cuerpo | Caída, igual para los dos bandos |
| `CoberturaQuitada` (muerte / caducidad) | La línea se corta en la caída / se desvanece en el grupo 2 | Breve cue de escudo roto solo si murió el escudero / ninguno |
| Estados de ambiente | Gradación y viñeta del terreno de la regla 26 | Lo decide #22 |

### Restricciones

- **Nada solo por color.** Cada marca de la regla 10 a la 13 se distingue en grises por su forma:
  - punto (destino);
  - corchete (alcance);
  - arco con punta (ataque);
  - anillo doble (área);
  - contorno dentado (explosión);
  - línea eslabonada (cobertura);
  - contornos grueso, doble y discontinuo (elegida, inspección, propio en zona).
- **Exclusividad del bando.** Hueso y Ciruela solo en el suelo de bando y en el contorno del tablero (#15). Ningún efecto de Render los usa.
- **Destellos:** < 3 por segundo; «Reducir efectos» reemplaza pulsos y explosiones por contornos y fundidos (regla 29).
- **Las figuras no se reiluminan:** luz pintada arriba a la izquierda (§5); el pulso es un efecto encima de la figura, no un cambio de su luz.
- **Contraste del terreno:** marcas de batalla y escombros por debajo de cualquier figura; saturación del terreno < ~35% (§1).
- **MVP:** una orientación, reposo ≤ 4 cuadros, ataque ~6, movimiento por código (§8, *Cuadros por pieza*).

### Lo que este GDD agrega al art bible (para el cierre de la tanda)

1. La **marca de explosión** (mecha más contorno dentado en las 8 vecinas).
2. La marca de **lo propio en zona** (contorno discontinuo, el mismo del fuego amigo).
3. El **contorno doble fino** de inspección.
4. El **punto con el costo** de destino.
5. La **flecha quebrada** del desvío.
6. La **cobertura visible para los dos jugadores**.
7. La **disposición de dos números** en la casilla del arquero en el muro.
8. Una orientación dibujada en el MVP (§8 dice «1 o 2»).
9. Formato de skin con castillo opcional (§6 ya lo pedía por pack).
10. Las dos correcciones del §6 que ya dejó Castillo: el interior es de 3 × 3, y los escombros y huecos no se desvanecen.

> 📌 **Asset Spec** — Visual/Audio requirements are defined. After the art bible is approved, run `/asset-spec system:render-animacion-y-feedback-de-combate` to produce per-asset visual descriptions, dimensions, and generation prompts from this section.

## Game Feel

- **Respuesta:** un comando aceptado se ve **en el cuadro siguiente**. La flecha, la línea o el inicio del desplazamiento aparecen dentro de un cuadro de 60 fps (16,6 ms) tras la respuesta de la Librería, y ninguna animación de comando bloquea el siguiente (Movimiento, Alcance, Reclutamiento).
- **La cosecha es un solo golpe (pilares 1 y 2):** todos los impactos caen en el mismo cuadro (`t_impacto` 0,35 s por defecto). El combinado es más grande que el simple (×1,5 con 2 atacantes, ×2 con 3 o más), con el mismo escalón que prometía la flecha. Tres arqueros contra un escudo se leen como una jugada, no como tres.
- **Ritmo (pilar 4):**
  - La resolución dura 1,65 s por defecto, con techo de 2,0 s, sin importar cuántos ataques hubo.
  - **No se salta** (regla 24): se ve entera, y en local no le cuesta reloj al siguiente (regla 25).
  - No hay cámara lenta, sacudida ni pausa extra.
  - Mover y deshacer duran ≤ 0,25 s; reclutar, ≤ 0,5 s.
- **Peso sin cuadros extra:** en el MVP el peso lo dan el sincronismo del cuadro de golpe, el pulso de luz y su escala, la caída simultánea y la marca en la tierra. No hace falta más animación por pieza (art bible, principio 3).
- **Coincidencia total:** ningún número, MUERE o ROTO de la resolución difiere de lo que mostró la vista previa. El feel no admite variación aleatoria ni de daño ni de animación que cambie lo que se lee (Castillo, Game Feel).
- **Quietud al planificar:** en `Planificar` solo respiran las figuras (1–2 px). Las marcas aparecen en ≤ 0,25 s y no pulsan, salvo la elegida, que puede pulsar y apagarse con «Reducir efectos» (Entrada).
- **Verificación:** las capturas retenidas de los AC Visual, y un playtest con tres preguntas:
  - *«¿algún golpe combinado se sintió igual que uno suelto?»*;
  - *«¿la animación de la resolución te hizo esperar?»*;
  - *«¿te diste cuenta de qué castillo venía perdiendo sin mirar los números?»*.

## UI Requirements

Render **no tiene pantalla propia ni textos propios**: dibuja el mundo y las marcas del tablero dentro de la pantalla de partida que ordenan #15 y #21. Lo que tiene que cumplir para la interfaz:

- **Orden de capas** de la regla 4: nada del mundo ni de una skin tapa la ficha, la vista previa ni el HUD.
- **`casillaEnPantalla`** (F3) es la única conversión de coordenadas del cliente. #15 y #16 anclan con ella, y Entrada (#13) usa su inversa, `casillaEnPunto`, para saber qué casilla se tocó.
- **Escala uniforme** desde el diseño de 1080p y aviso de #15 por debajo de 1 (F3). A 4K, activos al doble sin pérdida.
- **Marcas legibles en grises** y números de marca ≥ 14 px, negrita, borde Tinta (costos de destino, casillas de aparición).
- **Opciones que respeta:** «Reducir efectos» (regla 29) y «Mostrar cuadrícula» (Interactions). Las guarda el cliente hasta que exista Ajustes (#20).
- **Sin hover:** ninguna marca aparece solo al pasar el mouse (art bible §7).
- **Localización:** no hay claves `ui.render_*`. MUERE y ROTO son placas de #16 con sus claves; Render solo las ancla y las mantiene durante la resolución.

📌 **UX Flag — Render, animación y feedback de combate**: This system has UI requirements. In Phase 4 (Pre-Production), run `/ux-design` to create a UX spec for each screen or HUD element this system contributes to **before** writing epics. Stories that reference UI should cite `design/ux/[screen].md`, not the GDD directly.

## Cross-References

| Qué | Documento | Dónde | Relación |
|---|---|---|---|
| Eventos, estado público, consultas puras | `design/gdd/libreria-de-reglas.md` | Reglas 8 a 10 | Lee |
| ID de tipo, skin con ID inválido | `design/gdd/catalogo-de-piezas.md` | Regla 9, Edge Cases, AC-31 | Lee; la validación de carga es de este GDD (regla 7) |
| Eventos de turno, `EsperandoTirada`, reloj de partida, `DobleRey` | `design/gdd/maquina-de-turno.md` | Reglas 3, 4, 13, 14; Visual/Audio | Lee; detalla la caída de los dos Reyes |
| Grilla, castillos, `ladoDelRival`, despliegue sin entrada, «Mostrar cuadrícula» | `design/gdd/tablero-y-despliegue-inicial.md` | Reglas 1 a 11; Visual/Audio; UI Requirements | Lee y dibuja |
| Tres estados del tramo, parapeto, escombros, AC-31 | `design/gdd/castillo-y-muros.md` | Regla 2, Visual/Audio, AC-31 | Dibuja; toma AC-31 como propio (AC-8) |
| `PiezaMovida`, puntos de destino | `design/gdd/movimiento.md` | Visual/Audio, Game Feel | Dibuja |
| Alcance, flechas, área, explosión, lo propio en zona | `design/gdd/alcance-y-asignacion-de-ataques.md` | Reglas 5, 7; Visual/Audio | Dibuja; define las dos marcas que faltaban |
| Línea de cobertura, desvío | `design/gdd/cobertura-del-escudero.md` | Visual/Audio | Dibuja; ajusta la flecha desviada (quebrada) |
| Eventos de la regla 14, combinado, presentación ≤ ~2 s, AC-43 | `design/gdd/resolucion-simultanea-del-dano.md` | Reglas 12, 14; Visual/Audio; Game Feel; AC-43; Open Questions | Presenta; toma AC-43 como propio (AC-21) y cierra dos preguntas |
| `Reclutado`, aparición ≤ 0,5 s | `design/gdd/reclutamiento-y-limites.md` | Regla 12; Visual/Audio | Dibuja |
| Casillas numeradas, resplandor, placa | `design/gdd/interfaz-del-modificador.md` | Regla 6; Visual/Audio; AC-8, AC-9 | Dibuja las casillas y el resplandor |
| `Tirada` enviada por el cliente en local | `design/gdd/modificador.md` | Regla 9 | Fija cuándo (regla 25) |
| `TramoFinalIniciado`; la animación no frena el reloj | `design/gdd/timer-de-turno.md` | Reglas 11, 12; Open Questions | Respeta; cierra la pregunta en local |
| Selección, inspección, blanco táctil de la punta | `design/gdd/entrada-y-seleccion.md` | Regla 8; Visual/Audio | Dibuja; ajusta tres marcas |
| Capa del sistema, ficha, banner, política de resolución | `design/gdd/ficha-de-pieza-y-hud.md` | Reglas 1, 2, 11, 13, 15; Edge Cases; AC-26, AC-31 | Respeta; define la política de resolución |
| Ningún texto propio | `design/gdd/localizacion.md` | Dependencies, fila #18 | Cierra la pregunta |
| Pilares 1, 2, 4; ancla visual; MVP reducido | `design/gdd/game-concept.md` | Game Pillars; Visual Identity Anchor | Sirve |
| Regla visual, estados, formas, colores, figuras, terreno, HUD, estándares de assets | `design/art/art-bible.md` | Secciones 1 a 8 | Aplica; pide agregados (Visual/Audio) |
| Fila 18, compuerta del arte, riesgo de render y skins | `design/gdd/systems-index.md` | Sistemas uno por uno; High-Risk Systems | Detalla |

### Cambios pedidos a otros GDD (aplicados al cerrar la tanda 4, 2026-11-26)

Esta sesión corrió en paralelo con #16 y #19 y no editó ningún otro documento. Lo de abajo es para el cierre de la tanda 4.

| Documento | Cambio pedido |
|---|---|
| **Resolución (#9)** | AC-43 sin *PROVISIONAL*: la cubre AC-21 de este GDD. Open Questions: cerrar «¿Render necesita un evento por golpe?»: **no**, alcanzan `AtaqueEjecutado`, la casilla objetivo y `brutoDesviado` (regla 12 y 21 de #18). «¿La resolución entra en ~2 s?» pasa a «definida en 1,65 s por defecto con techo de 2,0 (F1 de #18); validar en prototipo». Visual/Audio, `PiezaMuerta` (`Explosion`): agregar que el demoledor que explota no muestra número aunque tenga `DanoRecibido` |
| **Castillo (#5)** | AC-31 sin *PROVISIONAL*: la cubre AC-8 de este GDD. Fila #18 de Dependencies: «Existe». La Open Question «¿el hueco y el contador se leen al instante?» sigue siendo de playtest, con dueño #18 y #15 |
| **Modificador (#11)** | Regla 9: «en local el cliente», agregar «**cuando termina la presentación de la resolución del turno anterior** (que no se salta; Render, reglas 24 y 25)» (decisión del 2026-11-26). Agregar #18 a Interactions y Dependencies (blanda) |
| **Timer (#12)** | Open Question «¿La animación de resolución (≤ 1,5 s) corre sobre el reloj del jugador siguiente?»: **cerrada para el modo local**. No corre, porque la `Tirada` espera a la presentación y el reloj no corre en `EsperandoTirada`. La cifra es ≤ 2,0 s (no 1,5). Online, sigue abierta para #28. Fila #18 de Dependencies: «Existe» |
| **Máquina (#3)** | Regla 14 (reloj de partida): la frase «sin contar las esperas de la tirada» incluye, en local, la presentación de la resolución (≤ 2 s por turno con ataques; Render, regla 25). Visual/Audio, `DobleRey`: el detalle que faltaba queda en #18 (las dos figuras caen en el mismo cuadro; después, la luz de Victoria) |
| **Ficha y HUD (#15)** | Edge Case de la ventana menor que 1080p y AC-31: sin *PROVISIONAL*; la política de resolución es F3 de #18 (escala uniforme, centrada, terreno en el sobrante). Regla 13: el banner del turno empieza con el **grupo 2** de la cola de Render, al terminar la resolución. Definir con #17 cómo conviven en el centro el banner y la placa de la tirada, que ahora llegan juntos. Dependencies, fila #18: «Existe»; consume `casillaEnPantalla`. AC-26 sigue *PROVISIONAL* hasta #38, pero el formato de skin de #18 ya no tiene campos que lo puedan violar |
| **Entrada (#13)** | Visual/Audio: alcance con **corchetes** (no relleno tenue); inspección con **contorno doble fino** (el discontinuo queda para lo propio en zona y el fuego amigo); destino con **punto y el costo adentro**. Agregar a Edge Cases o a estados: durante la presentación de la resolución, un toque sobre el tablero se ignora (regla 24) (decisión del 2026-11-26): no elige, no inspecciona ni manda comandos. Interactions y Dependencies, fila #18: «Existe»; las coordenadas de toque salen de `casillaEnPantalla` |
| **Interfaz del Modificador (#17)** | AC-8 y AC-9: quitar *PROVISIONAL* de Render; supuesto provisional «que Render dibuje las casillas numeradas» confirmado (regla 13 de #18). Aclarar «el pedido de animación de la placa» de Interactions y Dependencies: la placa vive en la capa 9 y la anima #17; Render solo pone el resplandor y el ambiente de Distracción y de la tirada. En local, la placa llega cuando termina la resolución del turno anterior |
| **Alcance (#7)** | Visual/Audio: la marca de explosión y la de lo propio en zona dejan de «faltar en el art bible»: las define #18, regla 12 (y se agregan al art bible). Fila #18: «Existe» |
| **Cobertura (#8)** | Visual/Audio, «Golpe desviado»: la flecha **llega a la casilla del aliado y se quiebra hacia el escudero**, donde se funde con su punta (así la punta original sigue siendo el blanco táctil de Entrada, regla 8). La línea visible en el turno del rival queda confirmada por #18. Fila #18: «Existe» |
| **Catálogo (#2)** | AC-31: quitar «PROVISIONAL hasta #18»; la carga de Render rechaza el tipo 7 (regla 7 de #18; AC-4) y el Kit (#38) suma lo suyo. Dependencies, fila #18: «Existe» |
| **Localización (#14)** | Fila #18: «Render no tiene textos propios; MUERE y ROTO son de #16». Supuestos: quitar las claves `ui` de #18 y «qué eventos de Render llevan texto» (respuesta: ninguno) |
| **Tablero (#4)**, **Movimiento (#6)**, **Reclutamiento (#10)** | Filas #18: «Existe». Sin cambios de reglas |
| **Vista previa (#16, en paralelo)** | Compatible con su regla 19, verificado al cierre de esta sesión: las placas de #16 se van en el cuadro en que empieza la cosecha (en #18, el cuadro 0 del gesto), y en `t_impacto` Render muestra el número flotante y MUERE o ROTO con los mismos íconos (calavera, grieta) y las mismas claves de texto de #16. La disposición de la casilla del arquero sigue la regla 11 de #16. Pedido: anclar con `casillaEnPantalla` (F3 de #18); agregar Render (#18) a su supuesto provisional como «confirmado» |
| **Métricas (#19, en paralelo)** | Eventos del cliente que Render puede entregar: `SkinRechazada(jugador, motivo)`, fps y tiempo de carga. `ResolucionSaltada` ya no existe: la cosecha no se salta (regla 24) (decisión del 2026-11-26). Si #19 los quiere, entran a su esquema |
| **Art bible** | Los agregados de Visual/Audio (*Lo que este GDD agrega al art bible*), y en §8 *Cuadros por pieza*, MVP: «1 orientación dibujada, espejada» |
| **Índice de sistemas** | Fila 18: *Designed*, enlace a este GDD, dependencias: sumar Cobertura, Interfaz del Modificador y Entrada (duras) y Ficha y HUD y Modificador (blandas). Contadores de progreso |

## Acceptance Criteria

Valores del Catálogo v5 y `vida_tramo` 30; perillas de tiempo por defecto (F1). Las piezas se nombran por tipo y casilla `(columna, fila)`; «azul» y «rojo» son los jugadores 0 y 1 (Hueso y Ciruela en pantalla).

**Cómo se prueba.** Render separa un **modelo de presentación** puro (qué se dibuja en cada instante del reloj de presentación, dado el estado y los eventos) del dibujo en Unity. Ese modelo se prueba con un reloj simulado, sin pantalla:
- Los criterios **Logic** son tests de EditMode en `Assets/Tests/EditMode/Render/`.
- Los **Integration** corren con la Librería real en `Assets/Tests/EditMode/Render/` o en PlayMode cuando necesitan el cliente.
- Los **Visual** y **UI** piden captura retenida en `production/qa/evidence/` (`render-*.png`).
- Los de **Rendimiento** se miden con `/perf-profile`.

### Reglas (Sección C)

- **AC-1 (regla 1, solo lectura)** *(Integration)* — **GIVEN** una partida con ataques, coberturas y un área asignados, **WHEN** Render presenta la resolución completa y, en otra copia, una rendición la corta a los 0,50 s, **THEN** la huella del estado es idéntica antes y después de dibujar, en las dos copias, y Render no envió ningún comando salvo la `Tirada` local de la regla 25.
- **AC-2 (regla 2, el final no depende de la animación)** *(Logic)* — **GIVEN** la misma resolución, **WHEN** el modelo de presentación (a) corre hasta `t_resolucion` y (b) se corta en 0,10 s, 0,35 s y 1,00 s (el corte de una rendición, Edge Cases), **THEN** en los cuatro casos el modelo final es idéntico: mismas figuras, mismos estados de tramo, mismas vidas, mismas marcas de terreno, sin números ni placas a la vista.
- **AC-3 (regla 3, determinismo)** *(Logic)* — **GIVEN** un estado y una lista de eventos, **WHEN** se generan dos modelos de presentación en procesos distintos, **THEN** coinciden la variante de suelo de cada una de las 360 casillas, la casilla de cada marca y cada valor del modelo en los instantes 0, `t_impacto` y `t_resolucion`.
- **AC-4 (regla 7, validación de skin; cierra Catálogo AC-31)** *(Logic)* — **GIVEN** skins de prueba con (a) el tipo 7, (b) sin el tipo 3, (c) un cuadro de 128 × 128, (d) `versionFormato` 2, (e) 5 cuadros de reposo, (f) cuadro de golpe en la posición 7 de 6, **WHEN** se cargan, **THEN** las seis se rechazan, el jugador juega con la skin básica completa y queda registrado un motivo distinto por caso. **Y GIVEN** una skin válida sin castillo, **THEN** se acepta, con sus figuras y el castillo de la básica.
- **AC-5 (regla 9, orientación)** *(Logic)* — **GIVEN** una skin con `miraHacia` derecha, **THEN** las figuras del azul no se espejan y las del rojo sí. **Y GIVEN** `miraHacia` izquierda, **THEN** al revés. En los dos casos, toda figura apunta al lado de `ladoDelRival` de su dueño.
- **AC-6 (reglas 4 y 5, la skin no toca la capa del sistema)** *(UI)* — **GIVEN** un pack de prueba cuyos cuadros son rectángulos opacos que llenan el lienzo, y la misma partida con la skin básica, **WHEN** se captura el tablero a 1080p, **THEN** la zona de cada ficha, banderín, flecha, línea de cobertura, contorno, marca de objetivo (corchete grueso, anillo doble, contorno dentado) y marco de bando es idéntica píxel a píxel en las dos capturas (completa el AC-26 de #15). Las marcas de la capa 4 (puntos de destino, corchetes finos, casillas de aparición, casilla pulsada) se comparan solo sobre casillas sin pieza. Evidencia: `render-capas-skin-*.png`.
- **AC-7 (reglas 10 a 12, marcas en grises)** *(Visual)* — **GIVEN** una captura a 1080p con, a la vez, una pieza elegida, una en inspección, destinos con costo, alcance con zona ciega, dos flechas fundidas, un área, una explosión con un tramo propio en la zona y una cobertura, **WHEN** se pasa a escala de grises y por los filtros protan, deutan y tritan, **THEN** un evaluador que no participó del diseño, con la leyenda de formas de Visual/Audio (*Restricciones*), nombra sin error cada marca de la captura en cada una de las 4 versiones (art bible §4, *Prueba de validación*). Evidencia: `render-marcas-*.png` y firma del lead.
- **AC-8 (estados del tramo; cierra Castillo AC-31)** *(Visual)* — **GIVEN** tres tramos con vida 30, 14 y 0 y un arquero sobre un cuarto tramo, **WHEN** se dibuja el castillo, **THEN** se ven el borde recto, las melladuras en el borde superior, el hueco con escombro bajo y borde quebrado, y el arquero con parapeto y la silueta entera. Evidencia: `render-tramo-entero.png`, `-mellado.png`, `-roto.png`, `-arquero.png`.
- **AC-9 (regla 13, casillas de aparición; cierra #17 AC-8)** *(Integration + UI)* — **GIVEN** Llamada con `K` = 4 y el anillo libre, **WHEN** se abre `Reclutar`, **THEN** Render numera 1 a 4 exactamente las casillas que da `orden_aparicion` y dibuja el resplandor del lado del frente. **WHEN** el estado deja `Reclutar`, **THEN** las dos marcas ya no están en el cuadro siguiente. Evidencia: `render-casillas-aparicion.png`.
- **AC-10 (regla 15, mover no bloquea)** *(Integration)* — **GIVEN** un soldado con movimiento de 2 pasos, **WHEN** se envía `Mover` y, 0,05 s después (reloj de presentación), un segundo `Mover` de otra pieza, **THEN** los dos se aceptan, cada desplazamiento termina en ≤ 0,25 s desde su comando, y el primero sigue el camino del evento casilla por casilla.
- **AC-11 (regla 16, reclutar a la vez)** *(Logic)* — **GIVEN** `Reclutado` con 4 piezas, **THEN** las 4 figuras empiezan a aparecer en el mismo instante y las 4 están completas a ≤ 0,5 s.
- **AC-12 (reglas 18 a 20, simultaneidad)** *(Logic)* — **GIVEN** una resolución con 5 `DanoRecibido`, 2 `DanoATramo`, 2 `PiezaMuerta` (`Dano`) y 1 `TramoRoto`, **WHEN** corre el modelo, **THEN**:
  - los 7 pulsos, los 4 números de los receptores que sobreviven y las 3 placas (2 MUERE, 1 ROTO) empiezan en el mismo instante, `t_impacto` = 0,35 s;
  - las vidas de las fichas cambian en ese mismo instante;
  - las 2 caídas y la rotura empiezan juntas en 0,65 s;
  - todo termina en 1,65 s.

  **Y GIVEN** la misma resolución con un solo `DanoRecibido`, **THEN** también termina en 1,65 s. **Y GIVEN** una lista sin eventos de Resolución, **THEN** no hay presentación (duración 0).
- **AC-13 (reglas 21 y 22, lo que se ve por evento)** *(Logic)* — **GIVEN** cada fixture, **THEN**:
  - un soldado que sobrevive con `vidaPerdida` 5 muestra «5», con un pulso de radio 30 px;
  - 4 atacantes sobre un objetivo que sobrevive dan **un** pulso de 60 px y **un** número;
  - un neto 0 con 2 atacantes muestra escudo y «0», **sin pulso**;
  - un escudero con `brutoDesviado` 16 que sobrevive muestra su `vidaPerdida` con el ícono de escudo, y el cubierto no muestra nada;
  - un fuego amigo de 2 piezas propias da un pulso de 30 px y el contorno discontinuo;
  - un tramo que sobrevive con `vidaPerdida` 8 muestra «8» (sin escudo);
  - un soldado que muere con neto 27 muestra MUERE y ningún número; un tramo que se rompe muestra ROTO y ningún número.

  Ningún número difiere de la `vidaPerdida` del evento.
- **AC-14 (regla 21, demoledor que explota)** *(Logic)* — **GIVEN** un demoledor azul que explota pegado a otro demoledor azul que también explota (#9, AC-40), **THEN** las dos figuras desaparecen en `t_impacto`, sin número sobre ninguna aunque cada una tiene `DanoRecibido`, y quedan dos quemados.
- **AC-15 (arquero en el muro, dos números)** *(UI)* — **GIVEN** un arquero azul sobre su tramo con 1 soldado rojo atacando (#9, AC-13: neto 4, tramo bruto 8), **WHEN** se captura en `t_impacto + 0,1 s`, **THEN** «4» está abajo, en el ancla de la ficha, y «8» arriba de la casilla con el ícono de muro, en la misma disposición que las dos placas de la vista previa (#16, regla 11). Evidencia: `render-arquero-muro-numeros.png`.
- **AC-16 (regla 23, marcas de batalla)** *(Logic)* — **GIVEN** un área sobre una casilla vacía en el turno 8 y un tramo roto en el mismo turno, **THEN** la tierra removida está en los turnos 8, 9 y 10 y ya no está al iniciar el turno 11, mientras el hueco y sus escombros siguen en el turno 30. **Y GIVEN** un quemado y después una piedra en la misma casilla, **THEN** queda solo la tierra removida, con la cuenta en 0.
- **AC-17 (regla 24, no se salta)** *(Integration)* — **GIVEN** una resolución local en curso, **WHEN** hay un toque sobre una pieza propia en 0,50 s, **THEN** la presentación sigue y termina en `t_resolucion` (1,65 s), Entrada no elige, no inspecciona ni manda comandos, y la `Tirada` se envía en 1,65 s, no antes. **Y WHEN** el toque llega un cuadro después de `t_resolucion`, **THEN** Entrada lo atiende como siempre. **Y WHEN** el jugador toca `Rendirse` en 0,50 s, **THEN** en el cuadro siguiente al `PartidaTerminada` el tablero está en el estado final de la resolución, empieza la luz del final y no se envía `Tirada`.
- **AC-18 (regla 25, la Tirada espera; cierra la pregunta del Timer en local)** *(Integration con #3, #11 y #12)* — **GIVEN** partida local con timer, **WHEN** el azul resuelve un turno con ataques, **THEN** la `Tirada` del rojo se envía en `t_resolucion` (1,65 s), no antes, y el reloj de partida del rojo marca lo mismo al enviar `Resolver` que al recibir la `Tirada`. **Y GIVEN** que el azul resuelve sin asignaciones y con la leyenda «Sin objetivos» de #15 en pantalla, **THEN** la `Tirada` sale a los `leyenda_ms` (1000 ms); sin leyenda ni placa pendiente, en el mismo cuadro.
- **AC-19 (regla 19, cola)** *(Logic)* — **GIVEN** la respuesta de `Resolver` con eventos de Resolución, `TurnoTerminado`, dos `CoberturaQuitada(Caducada)` y `TurnoIniciado`, **THEN** las líneas caducadas empiezan a desvanecerse y el grupo 2 empieza en `t_resolucion`, nunca antes. **Y GIVEN** que la resolución trae `PartidaTerminada(DobleRey)`, **THEN** los dos Reyes caen en el mismo instante y la luz de Victoria del jugador del turno empieza después de `t_resolucion`, y la placa de resultado (#15, #21) no aparece antes de `t_resolucion` + 0,25 s.
- **AC-20 (regla 26, ambiente)** *(Visual)* — **GIVEN** capturas de Planificar, Asignar ataques, Resolución en `t_impacto`, Tirada, Distracción, Tramo final en Planificar, Tramo final en Asignar, Victoria y Derrota, **THEN** el brillo medio de la capa 1 en una región sin marcas difiere del de Planificar en −20% ±2 (Asignar), −15% ±2 (Tirada) y −10% ±2 (Tramo final en Planificar); en Distracción la saturación de esa región baja 40% ±2. En las de tramo final el atardecer sigue presente, y en todas la figura, la ficha y el suelo de bando tienen los mismos valores de píxel que en Planificar. La temperatura de color y las luces de Victoria y Derrota las firma el director de arte. Evidencia: `render-ambiente-*.png`.
- **AC-21 (capturas de la cosecha; cierra Resolución AC-43)** *(Visual)* — **GIVEN** Render, **WHEN** se resuelven:
  - (a) un combo de 4 atacantes;
  - (b) un golpe absorbido por el escudo (neto 0);
  - (c) una cobertura que desvía;
  - (d) una explosión de demoledor sobre un muro de 3 tramos;
  - (e) un tramo roto con el arquero sobreviviente;
  - (f) un fuego amigo,

  **THEN** se guarda una captura de cada una en `t_impacto + 0,1 s` donde se ven todos los impactos del caso en el mismo cuadro, el número igual al de la vista previa, MUERE o ROTO donde corresponde, la flecha quebrada del desvío y el contorno discontinuo del fuego amigo. Evidencia: `render-cosecha-a.png` a `-f.png`. **Par Logic:** para cada fixture (a) a (f), el modelo de presentación en `t_impacto` tiene a todos los receptores del caso, con su número o su placa, en el mismo cuadro.
- **AC-22 (regla 27, pantalla y anclas)** *(Integration + UI)* — **GIVEN** ventanas de 1920 × 1080, 3840 × 2160, 2560 × 1080 y 1280 × 720, **WHEN** se dibuja la partida, **THEN** `casillaEnPantalla(0, 0)` es respectivamente (144, 60, 68), (288, 120, 136), (464, 60, 68) y (96, 40, 45,3). La ficha de #15 queda dentro de esos rectángulos. A 2560 × 1080 el sobrante muestra terreno oscurecido, y a 1280 × 720 aparece el aviso de #15. Evidencia: `render-escala-*.png`.
- **AC-23 (regla 29, Reducir efectos)** *(Visual)* — **GIVEN** la opción encendida, **WHEN** se resuelve el caso (a) de AC-21, **THEN** no hay pulso de luz ni estallido; están el contorno breve, el número, MUERE y la caída; los instantes de F1 son los mismos; y ningún elemento cambia de brillo más de 3 veces en un segundo.

### Fórmulas (Sección D)

- **AC-24 (F1, validación)** *(Logic)* — **GIVEN** la config, **THEN**:
  - los valores por defecto dan `t_impacto` 0,35 s y `t_resolucion` 1,65 s;
  - una config que suma **exactamente 2,00 s** y cumple el piso se acepta;
  - una que suma 2,05 s se rechaza;
  - una con `t_pulso + t_caida + t_lectura` = 0,75 s se acepta;
  - una con 0,60 s se rechaza;
  - el mínimo de los rangos con `t_lectura` 0,45 da 0,95 s y se acepta.
- **AC-25 (F1b, cuadro de golpe)** *(Logic)* — **GIVEN** un arquero con `n` 6 y `k` 4, **THEN** `t_golpe` es 0,20 s, cada cuadro previo dura 0,067 s y cada posterior 0,15 s. **Y GIVEN** `k` = 1, **THEN** el cuadro de golpe se muestra desde 0, sin error. **Y GIVEN** `n` = `k` = 6, **THEN** el sexto cuadro se sostiene durante `t_pulso`. **Y GIVEN** un soldado sobre una vecina, **THEN** `proyectil` es falso y su golpe cae en 0,35 s.
- **AC-26 (F2, escala del combo)** *(Logic)* — **GIVEN** `a` = 0, 1, 2, 3 y 61, **THEN** `escala_combo` es 1, 1, 1,5, 2 y 2, y el radio a 1080p es 30, 30, 45, 60 y 60 px. **Y GIVEN** una flecha a la que llegan 2 asignaciones, **THEN** su punta es ×1,5. Con un desvío, la asignación cuenta en la punta del escudero y no en la del cubierto.
- **AC-27 (F3, escala de pantalla)** *(Logic)* — los cuatro tamaños de AC-22 dan escalas 1, 2, 1 y 0,667. **Y GIVEN** una ventana de 0 × 0, **THEN** la escala es 0, el cuadro no se dibuja y no hay error. **Y GIVEN** 2560 × 1440, **THEN** `casillaEnPunto(1000, 500)` es (8, 4) y `casillaEnPunto(191, 500)` es «ninguna»; y en las 360 casillas, `casillaEnPunto` del centro de `casillaEnPantalla(c, f)` devuelve (c, f). Con 0 × 0, `casillaEnPunto` devuelve «ninguna» sin error.
- **AC-28 (F4, estado del tramo)** *(Logic)* — **GIVEN** `vida_tramo` 30, **THEN** la vida 30 y 15 dan Entero, 14 y 1 Mellado, y 0 Roto. **Y GIVEN** `vida_tramo` 1, **THEN** 1 es Entero y 0 Roto.
- **AC-29 (F5, marcas)** *(Logic)* — los casos de AC-16, más: con `marcas_turnos` 0 no se dibuja ninguna marca, y con 10 una marca del turno 1 se va al iniciar el turno 11.
- **AC-30 (F6, memoria)** *(Logic)* — **GIVEN** la skin básica del MVP, **THEN** sus cuadros suman 80 y `memoria_skin` es 5.918.720 bytes. **Y GIVEN** una skin con los topes del formato, **THEN** 94 cuadros y 6.954.496 bytes. Un test del repositorio verifica que dos skins al tope más el terreno quedan por debajo de 40 MB.

### Casos borde, cruces y rendimiento

- **AC-31 (corte de cuadros)** *(Logic)* — **GIVEN** el reloj de presentación salta de 0,30 s a 0,80 s en un cuadro, **THEN** el modelo igual produce un cuadro con el estado de `t_impacto` (vidas cambiadas, números y placas a la vista) y uno con el inicio de la caída, antes de seguir.
- **AC-32 (desvío dibujado)** *(Visual)* — **GIVEN** un arquero rojo cubierto por un escudero rojo y 2 soldados azules sobre el arquero, **WHEN** se captura en `Atacar` y en `t_impacto + 0,1 s`, **THEN** las dos flechas llegan a la casilla del arquero, se quiebran hacia el escudero y entran en una punta ×1,5 sobre él. La línea eslabonada está resaltada, el número va sobre el escudero con el ícono de escudo, y no hay número sobre el arquero. Evidencia: `render-desvio-*.png`.
- **AC-33 (Repeticiones)** *(Integration)* — **GIVEN** una partida local de 10 turnos guardada como lista de eventos, **WHEN** se le dan los eventos a un modelo de presentación nuevo, **THEN** en el instante final de cada turno el modelo es idéntico al de la partida original.
- **AC-34 (rendimiento de la resolución)** *(Rendimiento, `/perf-profile`)* — **GIVEN** el hardware de referencia de escritorio y de web (TBD, lo fija performance-analyst), una partida con 26 piezas y una resolución con 12 ataques, un área y una explosión, **WHEN** se mide durante el grupo 1, **THEN** el escritorio tiene p95 ≤ 16,6 ms y la web p95 ≤ [umbral TBD, Open Questions], con < 150 llamadas de dibujo (provisional, F6) y 0 B de memoria administrada asignada entre el primer y el último cuadro de la resolución. **Y GIVEN** el fixture al tope de eventos (393), **THEN** se cumplen los mismos umbrales de tiempo de cuadro y de memoria, y los 93 números o placas están presentes en el cuadro `t_impacto`.

### Revisión de QA

- **Cobertura:**
  - Regla 1 → AC-1; 2 → AC-2; 3 → AC-3, AC-33; 4 y 5 → AC-6; 6 y 7 → AC-4; 8 → AC-4 (básica por jugador), AC-30 (una carga); 9 → AC-5.
  - 10 a 12 → AC-7, AC-26, AC-32; 13 → AC-9; 14 → AC-6 y AC-20 (reposo quieto en las capturas); 15 → AC-10; 16 → AC-11; 17 → AC-8.
  - 18 a 20 → AC-12; 21 y 22 → AC-13 a AC-15; 23 → AC-16; 24 → AC-17; 25 → AC-18; 26 → AC-20; 27 y 28 → AC-22; 29 → AC-23; 30 → AC-1 (no escribe), AC-12 (no hay orden entre golpes).
  - F1 → AC-12, AC-24; F1b → AC-25; F2 → AC-13, AC-26; F3 → AC-22, AC-27; F4 → AC-8, AC-28; F5 → AC-16, AC-29; F6 → AC-30.
  - Ninguna regla ni fórmula queda sin criterio.
- **Cruces:** con Resolución (AC-12 a AC-15, AC-19, AC-21, AC-32), Castillo (AC-8), Catálogo (AC-4), Interfaz del Modificador (AC-9), Máquina, Modificador y Timer (AC-18, AC-19), Ficha y HUD (AC-6, AC-22), Movimiento (AC-10) y Reclutamiento (AC-11).
- **No automatizable (a Open Questions):** que el combinado «se sienta» más grande, que la animación no haga esperar y que el castillo que pierde se lea de lejos son preguntas de playtest (Game Feel).
- **Tipos:**
  - **Logic:** AC-2 a 5, 11 a 14, 16, 19, 21 (par Logic), 24 a 31.
  - **Integration:** AC-1, 10, 17, 18, 33; AC-9 y AC-22 son además UI.
  - **Visual o UI con captura retenida:** AC-6 a 9, 15, 20 a 23, 32.
  - **Rendimiento:** AC-34.

## Open Questions

| Pregunta | Dueño | Cuándo |
|---|---|---|
| ¿El golpe combinado (×1,5 / ×2, pulso de 0,3 s) se siente distinto de varios sueltos con el MVP reducido (una orientación, tweens)? Pregunta de playtest de Game Feel; si no alcanza, ¿se sube la escala o se suma un cuadro de golpe propio del sistema, antes de pedir más cuadros por pack? | Render (#18), director de arte; playtest | MVP, primer prototipo con arte |
| ¿1,65 s de cosecha es lo justo? La cosecha no se salta (regla 24) (decisión del 2026-11-26), así que se mide con playtest: la pregunta de Game Feel «¿la animación de la resolución te hizo esperar?». Si se siente lenta, bajar `t_lectura` dentro de su rango (con 0,30 s y el resto por defecto da 1,45 s) antes que tocar el impacto | Render (#18); playtest | MVP |
| Online: el servidor (#28) es la autoridad de la `Tirada`. ¿Espera a la presentación del cliente (con un tope fijo de `resolucion_max_s`) o la envía enseguida y la animación corre sobre el reloj del jugador siguiente? | Servidor (#28), Timer (#12) | Alpha (2a) |
| ~~¿Cómo conviven en el centro el banner del turno y la placa de la tirada?~~ **Resuelta 2026-11-26:** la placa de la tirada (#17) absorbe el renglón del turno; el banner solo, en el turno 1 (#15, regla 13) | — | — |
| ¿La gradación del estado de ambiente toca también el castillo? El art bible dice que el terreno es «la única capa» que la recibe, y un castillo sin atardecer en el tramo final puede verse pegado encima | Director de arte | Antes de `/asset-spec` |
| ¿Unity 6.3 cubre lo que necesita este render 2D en web: renderer 2D de URP o sprites en el renderer universal, atlas de sprites, tweens por código sin paquete externo, compresión de texturas en WebGPU y WebGL 2? Nada de eso está en `docs/engine-reference/unity/` (NO VERIFICADO, riesgo MEDIO). `modules/rendering.md` solo cubre el GPU Resident Drawer y el SRP Batcher, y ninguno de los dos es batching de sprites 2D. `breaking-changes.md` dice que WebGPU es el default desde 6.0, y `VERSION.md` lo da como opción de producción recién en 6.6: el ADR lo resuelve antes de contar con WebGPU en 6.3 | ADR de render (`/architecture-decision`), technical-artist | Antes de la primera historia de Render |
| ¿El conjunto reservado por tipo (Edge Cases, *Carga y rendimiento*) entra en memoria y en presupuesto de cuadro en web móvil (30–60 fps, concepto)? ¿Cuál es el umbral de p95 en web para AC-34, el hardware de referencia y el tope de partículas y overdraw? TBD de playtest y de perfilado (decisión del 2026-11-26) | performance-analyst | Con el primer build web |
| ¿Un terreno por pack o por facción (bosque, río, desierto; art bible §4) entra al formato de skin? Hoy el formato 1 no tiene terreno | Skins (#37), Kit (#38) | Full Vision |
| (CD-GDD-ALIGN, pilar 4) ¿Cuánto tiempo de pared suma la presentación por partida, fuera de los relojes? Medir el tiempo total de presentación de las resoluciones contra la duración de la partida; si pasa de ~1 min, bajar `t_lectura` o volver a la Tirada inmediata | Métricas (#19), Máquina (#3) | MVP |
| (CD-GDD-ALIGN, pilares 1 y 2) ¿Se nota que el pulso sale más grande que la punta cuando un área salpica, y que un combo de 6 se ve igual que uno de 3? Pregunta de playtest. Si molesta, contar los golpes de área en la punta o sumar un escalón ×2,5 | Render (#18), director de arte; playtest | MVP |
| La silueta de cada figura en el **cuadro de golpe** tiene que pasar la prueba del 90% (art bible §3). ¿La prueba la corre el Kit (#38) sobre el índice `k` del manifiesto? | Kit (#38) | Full Vision |
