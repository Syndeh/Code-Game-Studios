# Game Concept: Ajedrez táctico (título provisional; código rolchess)

*Created: 2026-11-17*
*Status: Draft (2026-11-17, después de la v3; actualizado 2026-11-19 con `/map-systems`)*
*Fuentes: `design/game-brief.md`, `design/reglas-cuaderno.md`,
`prototypes/rolchess-concept/REPORT.md`, `prototypes/rolchess-v2-concept/REPORT.md`,
`prototypes/rolchess-v3-concept/README.md`*

---

## Elevator Pitch

> Como el ajedrez, pero todos tus ataques caen a la vez: los golpes se suman, el escudo se
> resta una sola vez y la victoria está en combinar. Duelos tácticos 1 contra 1 de 15 a 20
> minutos donde movés tu ejército, planeás cada golpe y derribás al Rey rival.

*Por qué este pitch y no el del brief* ("ajedrez con vida, escudo y dado"): el director
creativo (CD-PILLARS, 2026-11-17) marcó que el dado y la vida son lo menos distintivo y que
el dado genera desconfianza en el público táctico. Lo único es combinar ataques, y eso es lo
que el pitch vende ahora.

---

## Core Identity

| Aspect | Detail |
| ---- | ---- |
| **Genre** | Estrategia táctica por turnos sobre tablero; duelo PvP |
| **Platform** | Web primero, hasta completar el juego (local, online, tienda y torneos) → móvil (iOS / Android) con online en la Fase 5 → PC (Steam) y consolas en la Fase 6, solo si tiene éxito en teléfonos |
| **Target Audience** | Estrategas y competidores que quieren duelos cortos (ver Target Player Profile) |
| **Player Count** | 2 jugadores: local en la misma pantalla (Fase 1), online 1 contra 1 con cuenta de Google (Fase 2). Modo de 3 jugadores, sin plan por ahora |
| **Session Length** | Partida de 15–20 min, nunca más de 25 (meta revisada el 2026-11-19; la primera de la v3 duró 33 y la primera de la v4, 23); timer por defecto de 60 s para mover + 30 para atacar, 40 + 20 desde el minuto 10; en partidas locales también Fijo 90, Fijo 60 y Sin límite |
| **Monetization** | Fase 1 gratis. Desde la Fase 3: skins cosméticas (packs de compra directa, logros, packs de la comunidad). Sin pagar para ganar ni cajas aleatorias |
| **Estimated Scope** | Large (4–6 años para la visión completa, solo, <15 h/semana). Fase 1: 14–22 meses, ~18 esperados (PR-SCOPE 2026-11-19) |
| **Comparable Titles** | Ajedrez; Into the Breach; The Battle of Polytopia. Cifras de ventas: no investigado |

---

## Core Fantasy

En palabras del jugador: *"Al mover estás armando la estrategia, estás planificando, y al
atacar ejecutás el plan y ves los frutos de tu estrategia."*

Sos el general de un ejército pequeño que juega como un todo. Cada turno armás una maniobra
con hasta 12 piezas y, al resolver, ves caer todo junto lo que planeaste: tres arqueros que
solos no atraviesan un escudo lo rompen juntos. La promesa emocional es la del plan que sale:
pensaste mejor que el otro y el tablero lo muestra.

---

## Unique Hook

**"Es como el ajedrez, Y ADEMÁS todos tus ataques se resuelven a la vez: el daño sobre un
mismo objetivo se suma y el escudo se resta una sola vez."**

Esa aritmética hace que coordinar sea siempre mejor que atacar suelto, y convierte cada turno
en un problema de asignación (quién golpea a quién) en lugar de una serie de duelos uno a uno.
En la v2 se combinaron ataques en el 81% y el 91% de los turnos y los jugadores lo valoraron
como una decisión real.

*Alternativas descartadas como hook:* el dado (no diferencia y genera desconfianza), el Rey
con vida (existe en otros híbridos), el castillo con muros (es soporte, no el centro).

---

## Reglas vigentes (resultado de los prototipos)

Este concepto reemplaza al cuaderno donde no coinciden. Cada regla se detalla en su GDD.

| Tema | Regla | Origen |
|---|---|---|
| Tablero | Cuadrado, 24 × 15 (pensado para 16:9). Hexagonal descartado por ahora | v2 |
| Ejército inicial | 4 soldados, 4 arqueros, 4 escuderos y el Rey; en 3 columnas de 4 (arqueros atrás, escuderos al frente), mirando al rival | cuaderno + v2 |
| Turno | Modificador (automático) → mover hasta 12 piezas más el Rey → asignar ataques → resolver. **El primer turno de la partida no tira el Modificador**: mueve y ataca normal, sin reclutar | cuaderno + v2 + v3 + Máquina de turno regla 3 (2026-11-20) + jugador 2026-11-24 (v5) |
| Movimiento | En cruz en el tablero cuadrado; se atraviesan aliados, nunca enemigos (salvo el jinete); se pueden deshacer todos los movimientos del turno, como una pila, del último al primero | v1 notas 1 y 7 (cruz: supuesto del prototipo) + jugador 2026-11-19 |
| Jinete | Pasa por encima de los enemigos: no lo bloquean, pero no termina el movimiento sobre ellos. Los muros sí lo frenan | jugador 2026-11-17 (v3, sin jugar) |
| Escudero | En la fase de ataque, en lugar de atacar, puede cubrir a un aliado a un cuadro (las 8 vecinas). Durante el turno rival, **todos** los ataques enemigos a ese aliado (área de catapulta y explosión de demoledor incluidas) los recibe el escudero (menos su escudo). Dura hasta el siguiente turno propio. Un aliado solo puede estar cubierto por un escudero (jugador 2026-11-19). El cuaderno lo limitaba a los arqueros | cuaderno + jugador 2026-11-17 (v3, sin jugar) |
| Alcance | Circular: una casilla está en alcance si dx² + dy² ≤ alcance² + 1 (con 6, hasta 37). Catapulta 2–6: no alcanza las 8 vecinas. Las piezas de alcance 1 atacan a las 8 vecinas | v1 notas 3 y 18 + v3 |
| Ataques | Se asignan con flechas y se resuelven todos juntos; el daño se suma por objetivo y el escudo se resta una vez; vista previa del resultado antes de resolver | v1 notas 2, 8 y 10 |
| Catapulta | Dispara a cualquier casilla en alcance: 10 de daño ahí y 6 en las 8 casillas alrededor, a enemigos, aliados y muros. Escudo 2 | v2 + jugador 2026-11-19 (primero 14/10, después 12/8) + jugador 2026-11-24 (10/6, escudo 3 → 2; v5) |
| Demoledor | Explota sobre todas las vecinas (aliados y muros incluidos) y muere | v1 nota 12 |
| Castillo | Muro perimetral de 5 × 5; 16 tramos de 30 de vida sin escudo; cualquier pieza enemiga en alcance lo ataca; un tramo roto abre un hueco; con 2 tramos rotos ese castillo deja de reclutar | v1 nota 11 + v2 + jugador 2026-11-24 (antes 1 tramo; v5) |
| Arqueros en el muro | Un arquero puede subir a un tramo libre de su propio muro (uno por tramo; el muro no se atraviesa). Arriba tiene +2 de escudo. El daño a esa casilla lo reciben el arquero (menos su escudo) y el muro (completo); si el muro se rompe, el arquero queda en el hueco | v3 |
| Rey | Vida 30. Empieza en el centro de su castillo. Sus propios muros no lo bloquean: entra y sale con su movimiento normal, sin regla especial. No puede subir a los muros. Muere el Rey, termina la partida | cuaderno + jugador 2026-11-19 |
| Reclutas | Aparecen del lado del castillo que mira al rival. Reclutar es obligatorio cuando sale la cara (no hay "no reclutar"); si no queda nada disponible, se pasa a mover | v1 nota 4 + jugador 2026-11-17 |
| Límite de reclutas | Por jugador y por partida. **Llamada al servicio** sale como mucho 6 veces. **Cuerno de guerra**: llamadas por tipo, jinetes 3, catapultas 4, demoledores 4 (jugador 2026-11-19; antes 8 y 4/4/4). Se mantienen los topes de piezas vivas (12/16/12/8/8/4) | jugador 2026-11-17 (v3, sin jugar) |
| Modificador (el dado) | 5 caras; en pantalla se ve solo el nombre. **Distracción**: no hay fase de movimiento, se pasa directo a atacar (cambio del 2026-11-19; antes: perdés el turno) · **Grito de guerra**: +2 de ataque y +2 de movimiento a todas tus piezas en ese turno, no se acumula · **Llamada al servicio**: reclutás 4 soldados, arqueros o escuderos · **Cuerno de guerra**: reclutás 4 jinetes, 2 catapultas o 1 demoledor · **Arremetida**: movés todas las piezas que quieras. Las elecciones aparecen en el centro del tablero | v2 + v3 |
| Probabilidades del Modificador | Al empezar su turno, antes de tirar, se comparan las piezas vivas del jugador (Rey incluido, muros no) con las del rival. Normal 20/20/20/20/20 · Pierde con 3/4 o menos 20/20/25/20/15 · Pierde con 1/2 o menos 15/20/25/25/15 · Gana 4 a 3 o más 20/25/15/20/20 · Gana 2 a 1 o más 20/25/15/15/25. Con 2 muros rotos, las dos caras de reclutar bajan a 0% y su parte va mitad a Grito de guerra y el resto a Arremetida. Lo mismo con cada cara de reclutar agotada: Llamada al servicio después de 6 veces, y Cuerno de guerra cuando ningún tipo especial tiene llamadas (sin castillos, los demoledores no cuentan). Las de los dos jugadores se pueden consultar en cualquier momento (ícono de signo de pregunta, arriba a la derecha). Valores provisionales: se ajustan en el MVP **Arremetida** solo sale con más de 12 piezas sin contar el Rey (con 12 o menos no agrega nada al límite normal): si no, su porcentaje, incluida la parte que recibe de las caras de reclutar, pasa entero a Grito de guerra (jugador 2026-11-24, v5) | v3 |
| Timer | Por etapas: 60 s para mover (reclutar incluido) y 30 para atacar; desde el minuto 10 de partida, 40 + 20 (modo Variable 90 → 60). Se acaba el de mover → pasa a atacar; se acaba el de atacar → se resuelven los ataques asignados y termina el turno. El turno también termina solo si no quedan acciones. En partidas locales se elige el modo: Variable 90 → 60, Fijo 90, Fijo 60 o Sin límite; online usa el Variable **Reloj de partida (v5):** además del timer de cada etapa, cada jugador tiene 12:30 en total (la mitad del techo de 25 min), que corre solo en su turno; si se le acaba, pierde. Reemplaza el cierre a los 25 min con desempate de la Máquina de turno. Sin límite no tiene reloj | v1 notas 14 y 15 + v3 + jugador 2026-11-19 y 2026-11-24 |
| Variante sin castillos | Descartada | jugador 2026-11-19 |
| Rendirse | Solo lo ve el jugador del turno mientras tiene la mitad de las piezas del rival o menos (tabla «Pierde con 1/2 o menos»); el que va ganando no. Gana el rival | jugador 2026-11-24 (v5) |
| Estadísticas | Tabla grande del cuaderno con demoledor de ataque 30, catapulta de ataque 10 y escudo 2, soldado de escudo 3, arquero de alcance 1–4, escudero de alcance de ataque 1–2 y vidas de arquero y escudero invertidas (arquero 15, escudero 20); se ven ataque, escudo y vida en cada pieza. Valores en `design/gdd/catalogo-de-piezas.md` | cuaderno + v1 nota 19 + jugador 2026-11-17, 2026-11-19 y 2026-11-24 (v5) |

**Modificador: reglas decididas, números provisionales.** La v3 implementó el rumbo acordado
después de la v2: las probabilidades se adaptan a cómo va la partida (con menos piezas, más
chances de reclutar), se pueden consultar antes de tirar y ninguna cara cae sin sentido (sin muros no salen
las caras de reclutar, ni las que ya agotaron sus llamadas; el Grito de guerra se usa en el
mismo turno). Distracción sigue por decisión del jugador; el director creativo la marcó en
contra de los pilares 1 y 3 cuando hacía perder el turno. Desde el 2026-11-19 solo quita la
fase de movimiento: el jugador sigue atacando con lo que ya tiene posicionado. En la primera partida de la v3 salió en el 19% de las tiradas y nadie sintió que el dado
decidiera. Porcentajes y umbrales se ajustan con partidas del MVP; se documentan en
`/design-system modificador`.

---

## Player Experience Analysis (MDA Framework)

### Target Aesthetics (What the player FEELS)

| Aesthetic | Priority | How We Deliver It |
| ---- | ---- | ---- |
| **Challenge** | 1 | Calcular combinaciones contra el escudo, leer al rival, gestionar 12 movimientos en el timer |
| **Fellowship** | 2 | Duelo contra otra persona: local, online, torneos |
| **Expression** | 3 | Cómo arma cada uno su maniobra; skins de ejército por packs temáticos |
| **Sensation** | 4 | La "cosecha": golpes que caen juntos, figuras animadas, un golpe combinado que se siente distinto |
| **Fantasy** | 5 | Ser el general de un ejército medieval |
| **Discovery** | 6 | Descubrir combinaciones y formaciones (escuderos al frente, soldados que pasan entre ellos) |
| **Narrative** | N/A | Sin historia; cada partida cuenta su propia remontada |
| **Submission** | N/A | Es un duelo con timer, no un juego para relajarse |

### Key Dynamics (Emergent player behaviors)

- Los jugadores buscan el punto donde dos o tres piezas juntas rompen un escudo que ninguna
  rompe sola.
- Arman formaciones: escuderos al frente, soldados y arqueros que avanzan atravesándolos
  (visto en la v1 en cuanto se permitió atravesar aliados).
- Ablandan con área (catapulta, demoledor) para que otros rematen.
- Presionan el castillo rival para cortarle los refuerzos.
- El que va perdiendo se repliega, junta fuerzas y busca la remontada con los reclutas (el
  mejor momento de los dos prototipos).

### Core Mechanics (Systems we build)

1. Ataques planificados y resolución simultánea, con daño sumado y escudo único por objetivo.
2. Movimiento de hasta 12 piezas más el Rey, atravesando aliados.
3. Modificador: dado adaptativo que da recursos (Grito de guerra, reclutas, movimiento libre).
4. Castillo con muros con vida y reclutamiento atado a los muros.
5. Timer de turno y fin automático.

---

## Player Motivation Profile

### Primary Psychological Needs Served

| Need | How This Game Satisfies It | Strength |
| ---- | ---- | ---- |
| **Autonomy** | Cada turno decidís qué piezas mover, cómo repartir los ataques y cuándo gastar el refuerzo | Core |
| **Competence** | El daño es fijo y se ve antes de resolver: si ganás, es porque calculaste mejor | Core |
| **Relatedness** | Duelo contra una persona (en la misma pantalla primero), torneos más adelante | Supporting |

### Player Type Appeal (Bartle Taxonomy)

- [ ] **Achievers** — Secundario: logros que dan skins (Fase 3).
- [ ] **Explorers** — Menor: entender las combinaciones y el dado adaptativo.
- [ ] **Socializers** — Menor: jugar con amigos, packs de skins de la comunidad.
- [x] **Killers/Competitors** — Principal: ganarle a otra persona, torneos, ranking.

Según Quantic Foundry: principal **Estrategia** (planear y decidir) y **Competencia**;
secundario **Diseño** (personalizar el ejército). No es para quien busca historia, campaña o
partidas largas de 4X.

### Flow State Design

- **Onboarding curve**: la primera partida empieza con pocas piezas y el dado explicado en
  pantalla; la vista previa del daño enseña la regla del escudo sin texto. Tutorial básico de 3 pasos (turno, piezas
  comunes, piezas especiales) con las estadísticas de todas las piezas, y aparte una partida
  tutorial completa contra un Rival IA básico.
- **Difficulty scaling**: la dificultad la pone el rival. El timer se ajusta por experiencia
  (más tiempo para principiantes).
- **Feedback clarity**: flechas de ataque, vista previa del daño y de lo que muere, dado grande
  con su efecto.
- **Recovery from failure**: la partida dura 15–20 min; perder no cuesta nada y se juega otra.

---

## Core Loop

### Moment-to-Moment (30 seconds)
Sale el dado y ves qué recurso tenés. Movés hasta 12 piezas armando la maniobra; asignás
ataques con flechas y la vista previa te dice qué cae. Resolvés y todo cae junto. Lo que
satisface por sí solo es la cosecha: ver que el cálculo era correcto.

### Short-Term (5-15 minutes)
La presión de ida y vuelta: abrir un muro para cortar los refuerzos rivales, defender el
propio, decidir cuándo gastar la carga de refuerzo, buscar el hueco hacia el Rey. Acá viven
las remontadas: un reclutamiento a tiempo da vuelta la partida.

### Session-Level (30-120 minutes)
Una partida dura 15–20 min, así que una sesión son 2 a 6 partidas. Cada caída del Rey es un
punto natural para cortar o para pedir la revancha. Más adelante, los torneos (dos horarios
por día de torneo) dan una sesión con forma.

### Long-Term Progression
No hay progresión que cambie el poder: todos juegan con el mismo ejército. Se progresa en
habilidad (y en ranking cuando haya online) y en colección: skins por logros y por packs.

### Retention Hooks
- **Curiosity**: nuevos packs de skins; combinaciones por descubrir.
- **Investment**: el ejército personalizado; el ranking.
- **Social**: revancha con el mismo rival; torneos; packs de la comunidad.
- **Mastery**: calcular mejor, jugar más rápido para timers más cortos.

---

## Game Pillars

Fijados el 2026-11-17 después de la revisión CD-PILLARS (CONCERNS; ajustes aplicados salvo
el de la cara 1, hoy Distracción, que el jugador decidió mantener).

### Pillar 1: Planificar y cosechar
Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos.

*Design test*: entre un resultado incierto y uno que sale como se planeó, gana el que sale
como se planeó. El daño es fijo y se ve antes de resolver. El azar se resuelve antes de
planificar, nunca entre el plan y la cosecha.

### Pillar 2: La fuerza está en combinar
Ninguna pieza gana sola: el poder sale de juntar varias sobre un mismo objetivo.

*Design test*: entre hacer más fuerte una pieza o premiar la coordinación, gana la
coordinación. Las piezas de área habilitan combos (ablandan para que otros rematen), no los
reemplazan. En imagen y sonido, un golpe combinado se siente distinto de varios golpes
sueltos.

### Pillar 3: Siempre hay vuelta, con límites
El que va perdiendo tiene con qué remontar.

*Design test*: una ayuda al que pierde vale solo si (a) da opciones (más chances de reclutar),
nunca estadísticas (más daño o escudo); (b) sus probabilidades se pueden consultar en
cualquier momento antes de tirar (no hace falta tenerlas siempre a la vista: la ayuda se
percibe jugando; ajuste del jugador, 2026-11-17); y (c) el que tiene más piezas sigue ganando la mayoría de las partidas (umbral a fijar
en el GDD del Modificador; propuesta: 2 de cada 3).

### Pillar 4: Duelo corto
La partida dura entre 15 y 20 minutos y nunca pasa de 25 (jugador, 2026-11-19, después de la primera partida de la v4).

*Design test*: si una mecánica alarga la partida, pierde; si enriquece la decisión dentro del
turno sin alargarla, gana. Las remontadas entran en el presupuesto: aun con remontada, la
partida no pasa de 25 minutos. El timer es presión buscada: quedarse sin tiempo para mover o
para atacar no es una falla sino lo que obliga a concentrarse; los jugadores lo dijeron así
después de la primera partida de la v4.

*Por qué 15–20 y no 10–15:* la primera partida de la v4 duró 23 minutos, se sintió pareja y
los jugadores dijeron que se divirtieron más que en las anteriores. El techo de 25 mantiene
el duelo dentro de una sesión corta.

**Tensiones buscadas:** 1 contra 3 (el plan pesa, pero la suerte puede dar vuelta la partida;
se concilian con "el dado da recursos, no decide golpes"), 2 contra 4 (coordinar lleva tiempo;
lo acota el timer). A vigilar: 3 contra 4 (cada remontada alarga la partida).

### Anti-Pillars (What This Game Is NOT)

- **NOT un dado que decide golpes**: el dado no decide si un ataque acierta ni cuánto daña,
  solo ofrece recursos; rompería el pilar 1. Excepción vigente: Distracción (sin fase de
  movimiento; desde el 2026-11-19 ya no hace perder el turno y el ataque se mantiene), que el
  jugador decidió mantener.
- **NOT información oculta al resolver**: ni niebla de guerra ni cartas ocultas; protege el
  pilar 1.
- **NOT pagar para ganar**: las skins son solo estéticas y todos juegan con el mismo ejército;
  si no, el plan deja de mandar (pilar 1) y el duelo deja de ser justo.
- **NOT cajas aleatorias pagas**: pedido del jugador en el brief.
- **NOT partidas de más de 25 minutos ni campañas**: rompería el pilar 4.

---

## Visual Identity Anchor: Campo de Batalla

Dirección del jugador (2026-11-17), elegida por sobre las tres del director de arte (Mesa de
Guerra, Heráldica Viva, Ejército de Juguete); de ellas se conservan las restricciones comunes.
Referencia: imagen de figuras pintadas sobre un tablero de tierra (`design/references/campo-de-batalla-ref.jpg`).

**Regla visual:** la figura cuenta *quién es* (la skin); la ficha del sistema dice *qué es*
(ícono de tipo y estadísticas). Una pieza se reconoce sin conocer la skin que lleva.

**Principios:**
1. **El tablero es un campo de batalla medieval.** Pasto, tierra y algunas piedras; las matas
   de pasto marcan las casillas, sin cuadrícula dibujada. *Prueba:* si un elemento del
   terreno compite con las piezas por la atención, se apaga.
2. **El tipo manda sobre la skin.** Cada tipo tiene silueta y arma propias (el soldado, arma
   corta: espada y armadura o espada y capa; el arquero, arco; etc.); cada pack reinterpreta
   el tipo con su estilo (por ejemplo, Japón medieval: el soldado es un samurái o un ronin)
   sin cambiar silueta ni arma. *Prueba:* si una skin hace dudar del tipo a tamaño de juego y
   en grises, no se aprueba.
3. **La capa del sistema es intocable.** Ícono de tipo, ataque, escudo, vida y el color del
   bando viven en una ficha que ninguna skin modifica. *Prueba:* si una skin necesita cambiar
   la ficha, se rechaza la skin.

**Animación:** cada pieza tiene reposo, movimiento y ataque, mirando a 8 direcciones (4 rectas
y 4 diagonales). **En el MVP se reduce:** 1 o 2 orientaciones espejadas, reposo corto, y
movimiento y ataque animados por código; las 8 direcciones llegan con el primer pack de skins.

**Filosofía de color:**
- El terreno va en tonos apagados de tierra, pasto y piedra, para que las piezas resalten.
- El color del bando es un color reservado de la capa del sistema; las facciones (bosque, río,
  desierto) se expresan con materiales y motivos, nunca con ese color.
- Colores fijos con significado: ataque cálido o rojo, escudo azul o acero, vida verde, cada
  uno acompañado de forma o ícono (daltonismo). La muerte en la vista previa tiene su propio
  estado.

**Pendiente:** en móvil no entran tres números por casilla (unos 26–45 pt de alto). Zoom y
desplazamiento, o vida siempre visible y el resto al tocar: lo deciden el GDD de interfaz y
`/art-bible`.

---

## Inspiration and References

| Reference | What We Take From It | What We Do Differently | Why It Matters |
| ---- | ---- | ---- | ---- |
| Ajedrez | Duelo 1 contra 1, Rey como objetivo, información a la vista | Las piezas tienen vida y escudo; se mueven muchas por turno y los golpes se combinan | Valida que un duelo de tablero sin azar en la resolución engancha |
| Into the Breach | Resultado determinista y visible antes de actuar | Es PvP y el centro es combinar contra el escudo, no empujar | Valida que mostrar el resultado no quita tensión |
| The Battle of Polytopia | Estrategia en sesiones cortas, en web y móvil | Sin economía 4X; todo pasa en un duelo de 15–20 min | Valida sesiones cortas de estrategia en las plataformas objetivo |
| Advance Wars / Fire Emblem | Ejército por turnos con tipos de unidad | Sin porcentajes de acierto; escala de duelo corto | Contraste: lo que el pilar 1 evita |

**Non-game inspirations**: soldaditos y miniaturas pintadas sobre un campo de batalla; el
tablero de ajedrez como lenguaje común. Cifras de mercado de estos títulos: no investigado.

---

## Target Player Profile

| Attribute | Detail |
| ---- | ---- |
| **Age range** | 16–40 *(supuesto)* |
| **Gaming experience** | Mid-core |
| **Time availability** | Ratos de 20 min; sesiones de varias partidas los días de torneo |
| **Platform preference** | Navegador y móvil; PC después |
| **Current games they play** | Ajedrez online, juegos tácticos por turnos *(supuesto)* |
| **What they're looking for** | Un duelo táctico profundo que entre en 20 minutos y que se gane pensando |
| **What would turn them away** | Que la partida la gane el dado; pagar para ganar; partidas largas; no encontrar rival |

---

## Technical Considerations

Revisión TD-FEASIBILITY (2026-11-17): **CONCERNS**. El núcleo es factible; las decisiones de
abajo conviene tomarlas temprano.

| Consideration | Assessment |
| ---- | ---- |
| **Recommended Engine** | Unity 6.3 LTS (elegido por el jugador; fijado en el proyecto). Cubre web, móvil y Steam |
| **Key Technical Challenges** | Reglas en una sola librería C# pura (sin Unity) usada por cliente, servidor, tests y repeticiones; Firebase desde Unity Web (puente jslib) y SDK nativo en móvil detrás de una interfaz común; dado y timer resueltos en el servidor; resolución determinista (solo enteros, orden de resolución explícito) |
| **Art Style** | 2D pintado, figuras con volumen vistas desde arriba en ángulo |
| **Art Pipeline Complexity** | Alta en la visión completa (unos 2.000 cuadros por pack en 8 direcciones); media en el MVP reducido |
| **Audio Needs** | Moderado: golpes, resolución combinada con acento que crece con los atacantes, dado; música ambiente |
| **Networking** | Cliente-servidor con autoridad en el servidor (Fase 2): Firebase Realtime Database y validación de jugadas (Cloud Run con la librería C# recomendado por el TD, o Cloud Functions en JS con pruebas compartidas) |
| **Content Volume** | 7 tipos de pieza, 1 tablero, 1 skin base en el MVP |
| **Procedural Systems** | Ninguno (salvo la posición de los castillos por sectores al azar) |

Presupuestos propuestos por el TD: primera descarga web menor a 20 MB; menos de 40 MB de
texturas por partida con 2 skins; menos de 150 draw calls; 60 fps en escritorio, 30–60 en web
móvil.

---

## Risks and Open Questions

### Design Risks
- **El dado inclina partidas.** 20% de tiradas perdidas en la v2. En la v3 (Modificador con
  probabilidades por estado) fue 19% y la partida se sintió pareja, pero es una sola partida.
  Mitigación: calibrar con partidas humanas (sin simulador); después de las primeras 20
  partidas online, propuesta de cambios del Modificador y la duración.
- **La remontada puede volverse injusta** si el dado adaptativo es demasiado generoso u oculto
  (sensible en online competitivo). Mitigación: los límites del pilar 3.
- **La partida dura el doble que la meta.** La primera de la v3, con jugadores nuevos, duró
  32:45 (meta de entonces: 15 min o menos), con mucho ida y vuelta por muchas caras de
  reclutar. La primera de la v4 duró 23:01, con menos turnos (28 tiradas contra 42) y el mismo
  turno promedio (0:57). Mitigación (2026-11-19): meta revisada a 15–20 min con techo de 25;
  la Máquina de turno es dueña del presupuesto de duración; el arte final no se produce hasta
  que 3 partidas seguidas duren 25 min o menos.
- **Mover 12 o 13 piezas en 60–90 s en pantalla táctil** puede ser pesado (TD). Probar en un
  teléfono real antes de cerrar las reglas.
- **Dos demoledores matan a un Rey de 30 de vida de una vez** (v2, partida 1). A vigilar en
  balance.

### Technical Risks
- Reglas duplicadas en C# y JS si se valida con Cloud Functions (alto). Mitigación: librería
  C# única desde la Fase 1; ADR antes de la Fase 2.
- Dos integraciones de Firebase (web y móvil) y un tercer caso en Steam (sin proveedor de
  Steam en Firebase Auth).
- Memoria de sprites en navegadores móviles (Safari iOS).
- Timer autoritativo y reconexión en el servidor.

### Market Risks
- **Pocos jugadores para el online y los torneos** (productor): sin rival disponible, el
  online se vacía. No investigado. Pregunta abierta: rival IA o partidas asincrónicas.
- **Pagos por plataforma**: Stripe solo sirve en web; iOS, Android y Steam exigen su propio
  sistema de compras. La tienda de la Fase 3 (web) se rehace en parte para móvil (Fase 5) y,
  si llega, para Steam (Fase 6). Conviene que la lógica de la tienda no dependa de Stripe.
- Tamaño del público de táctica PvP de sesión corta: no investigado.

### Scope Risks
- **Arte**: 105–168 animaciones por pack. Mitigación: arte reducido en el MVP y medir el costo
  por animación.
- **Visión completa de 4–6 años** a menos de 15 h por semana. Mitigación: publicar la Fase 1
  sola; reordenar después de la Fase 2.
- **Corte a mitad de la Fase 2** con un producto roto si la Fase 1 no se publicó antes
  (mitigado: se publica sola) o con un online a medio hacer (mitigado: la Fase 2 se parte en
  2a y 2b, cada una publicable).

### Open Questions
- **Modificador:** reglas cerradas en la v3; porcentajes y umbrales a ajustar con partidas del
  MVP. → `/design-system modificador`.
- **¿Qué tanto se combina de verdad?** Primera medición (v3): ataque combinado decisivo en 21
  de 34 turnos (61%). Falta confirmarlo con más partidas.
- **¿Remontada o inexperiencia?** En la v3 ganó el que estuvo acorralado al principio, con
  jugadores nuevos.
- **Movimiento en cruz o en las 8 direcciones** (en cruz fue un supuesto del prototipo). →
  GDD de movimiento.
- **Estadísticas en móvil:** zoom o dos niveles. → GDD de interfaz y `/art-bible`.
- **Validación en el servidor:** Cloud Run con C# o Cloud Functions en JS. → ADR antes de la
  Fase 2.
- **Rival IA o partidas asincrónicas** para cuando haya pocos jugadores online. (La Fase 1
  ya suma un Rival IA básico, solo para la partida tutorial completa.)

---

## Plan por fases

Orden del jugador (2026-11-17): el juego se completa en web antes de llegar a los teléfonos.
Renumerado el 2026-11-19: las cuentas (antes Fase 3) pasan a la Fase 2, que se parte en dos
mitades publicables. Detalle por sistema en `design/gdd/systems-index.md`.

| Fase | Contenido |
|---|---|
| 1 | Web local: partida de 2 jugadores en la misma pantalla, tutorial básico de 3 pasos, partida tutorial contra un Rival IA básico y métricas de cada partida guardadas en un archivo exportable |
| 2a | Web online mínimo: servidor de partida, cuentas con Google, partida rápida e invitación, emotes y frases predefinidas, analítica online con tablero (balance, retención y rendimiento) |
| 2b | Web online social: lobby con estados y desafíos, chat global con filtro, repeticiones (últimas 10 partidas), estadísticas y ranking |
| 3 | Tienda y skins en web (Stripe, con el proveedor de pago abstraído) y kit de creación de skins para la comunidad |
| 4 | Torneos automatizados (jueves, viernes y sábados, 21 y 23 hora del servidor, brackets, premios) |
| 5 | Móvil (iOS / Android) con online, con las compras dentro de la app de cada tienda |
| 6 | PC (Steam) y consolas, solo si el juego tiene éxito en teléfonos |

---

## MVP Definition

**Core hypothesis**: un duelo local de 15–20 minutos, con ataques combinados y un dado
adaptativo, es divertido y se siente decidido por el plan y no por el dado, para jugadores
que no lo diseñaron.

**Required for MVP** (Fase 1, partida local de 2 jugadores en web):
1. Tablero de 24 × 15 con castillos con muros.
2. Los 7 tipos de pieza con la tabla de estadísticas; ficha con ícono de tipo, ataque, escudo
   y vida.
3. Movimiento (hasta 12 más el Rey, atravesando aliados) con deshacer.
4. Ataques asignados con flechas, vista previa y resolución simultánea.
5. Modificador adaptativo, con sus probabilidades consultables y sus elecciones en el centro del
   tablero.
6. Reclutamiento, timer de turno, fin automático y fin de partida.
7. Arte Campo de Batalla reducido: tablero, una skin base en 1–2 orientaciones, animación por
   código.
8. Build web dentro del presupuesto (menos de 20 MB).

**Explicitly NOT in MVP** (defer to later):
- Online, cuentas, tienda, skins adicionales, torneos (Fases 2 a 4).
- Animación en 8 direcciones (primer pack de skins).
- Móvil (Fase 5, con el juego completo en web).
- PC (Steam) y consolas (Fase 6, solo si el juego tiene éxito en teléfonos).
- Facciones con terreno y modo de 3 jugadores (sin plan).
- Rival IA fuera del tutorial (pregunta abierta).

### Scope Tiers (if budget/time shrinks)

Estimaciones del productor (PR-SCOPE, 2026-11-19, sobre el índice de sistemas): solo, unas
45 h efectivas por mes. Confianza baja: no hay historial de ritmo; se recalibran después de
2–3 sprints con horas reales. No cuentan el Rival IA básico, sumado después.

| Tier | Content | Features | Timeline |
| ---- | ---- | ---- | ---- |
| **MVP (Fase 1, publicable sola)** | 1 tablero, 7 piezas, 1 skin base reducida | Partida local completa en web, tutoriales | 14–22 meses (~18) |
| **Vertical Slice** | Una partida completa con el arte final de una pieza en 8 direcciones | Núcleo + dado final + medir el costo de arte por animación; solo después de pasar la compuerta de duración (3 partidas seguidas de 25 min o menos) | Dentro de la Fase 1 |
| **Fases 1 y 2** | MVP + online | 2a: online 1 contra 1 con cuentas, validación en servidor, reconexión y timer del servidor · 2b: lobby, chat, repeticiones y ranking | 24–34 meses (~29) |
| **Full Vision** | Packs de skins en 8 direcciones, kit de skins para la comunidad, facciones | Fases 1 a 4 en web (local, online, tienda, torneos) → Fase 5 móvil con online → Fase 6 PC (Steam) y consolas si el móvil tiene éxito | 4–6 años o más |

---

## Next Steps

- [x] `/art-bible` — la guía de arte Campo de Batalla (`design/art/art-bible.md`)
- [ ] `/design-review design/gdd/game-concept.md`
- [x] `/map-systems` — `design/gdd/systems-index.md` (2026-11-19)
- [ ] `/design-system` en el orden del índice: librería de reglas primero; el Modificador va
      después de Reclutamiento
- [ ] `/create-architecture` — con la librería de reglas en C# pura desde el inicio
- [ ] `/architecture-decision` — reglas y validación en el servidor; backend por plataforma;
      pagos multiplataforma (antes de la Fase 3); presupuesto de arte
- [ ] `/architecture-review`, `/gate-check`
- [ ] Volver a pasar PR-SCOPE después de `/map-systems`, con plazo concreto
