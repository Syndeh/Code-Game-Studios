# Art Bible: Ajedrez táctico (título provisional; código rolchess)

> **Status**: Draft — 9 secciones escritas
> **Owned By**: art-director
> **Last Updated**: 2026-11-17
> **Art Director Sign-Off (AD-ART-BIBLE)**: REVISED 2026-11-17 — las 9 secciones. Corregidas las frases que dejó el HUD a pantalla completa (Secciones 4 y 6) y la pose de reposo (Sección 5). Pendiente de medir: peso de la primera descarga web (Sección 8)

`/art-bible` writes these nine sections in this order and replaces each
section's `[To be designed]` line as that section is approved. A section still
holding its placeholder is incomplete; a later run fills only those.

Fuente: `design/gdd/game-concept.md`, sección "Visual Identity Anchor: Campo de Batalla".

## 1. Visual Identity Statement

### Regla visual

**Lo que decide la jugada se lee primero, en la casilla de 68 px de la web. Después la
figura, después el bando y al final el terreno. Ante la duda, se apaga lo de abajo para que
se lea lo de arriba.**

**Tamaño de diseño (decisión del jugador, 2026-11-17):** el arte se diseña para la casilla de
la web: 68 px a 1080p, con el tablero a pantalla completa (Sección 7). El móvil llega en la
Fase 6, donde el tablero entero en un teléfono horizontal deja casillas de ~26-29 pt; cómo se
juega ahí (zoom y desplazamiento, figuras simplificadas o menos datos en la ficha) se decide
en esa fase.

Orden de lectura, de arriba hacia abajo:

1. **Ficha:** el ícono de tipo junto con el número de vida; un ícono de puño con el número
   de ataque; un ícono de escudo con el número de defensa.
2. **Figura:** la skin de la pieza.
3. **Bando:** el suelo de la casilla de la pieza se resalta con el color del bando. Las
   skins no llevan ese color.
4. **Terreno:** campo de batalla apagado. Una casilla con pieza se enmarca con matas de
   pasto.

Regla del jugador (2026-11-17). Amplía la del ancla ("la figura cuenta quién es, la ficha
dice qué es") con un orden de lectura. Los tres principios del ancla (terreno de campo de
batalla, el tipo manda sobre la skin, la capa del sistema es intocable) siguen vigentes y se
desarrollan en las secciones de terreno, piezas e interfaz.

### Principios de apoyo

**1. El plan se lee antes que el detalle.** Sirve al **pilar 1: Planificar y cosechar** (el
daño es fijo y se ve antes de resolver).
- Cómo se hace: la figura es lo más contrastado de su casilla, con contorno oscuro continuo.
  El terreno usa tierra, pasto y piedra poco saturados (propuesta: saturación HSV por debajo
  de ~35%, a validar con capturas) y nunca tiene un elemento más contrastado que una figura.
- *Prueba de diseño:* cuando un adorno de la skin, una textura del suelo o un brillo compite
  con la lectura del tipo o de un número, gana la lectura. Verificación: cada pieza se
  reconoce por silueta, en grises y a 68 px de alto de casilla.

**2. El combo se ve como un solo golpe.** Sirve al **pilar 2: La fuerza está en combinar**
("un golpe combinado se siente distinto de varios golpes sueltos").
- Cómo se hace: en la vista previa y al resolver, los atacantes sobre un mismo objetivo se
  dibujan con trazos que convergen en él, y el efecto sobre el objetivo crece con la cantidad
  de atacantes. Vive en la capa del sistema: ninguna skin lo toca.
- *Prueba de diseño:* cuando es ambiguo si resaltar cada golpe o el conjunto, gana el
  conjunto. Tres arqueros contra un escudo se leen como una jugada, no como tres.

**3. El ritmo del turno manda sobre la animación.** Sirve al **pilar 4: Duelo corto** (15 a
20 minutos, nunca más de 25).
- Cómo se hace: todos los ataques de una resolución se animan a la vez, no uno detrás de
  otro. Propuesta a validar en prototipo: la resolución completa de un turno no pasa de ~2 s.
  Esto también ayuda a que el MVP reducido (movimiento y ataque por código, 1 o 2
  orientaciones) no se sienta pobre.
- *Prueba de diseño:* cuando una animación más rica alarga la resolución, gana la que entra
  en esa ventana. Las 8 direcciones y los cuadros extra se suman solo si no alargan la
  resolución.

### Riesgos a validar

- **Tamaño de la ficha.** En una casilla de 68 px la ficha va sobre la parte baja de la figura (Sección 7). Se
  valida en el primer prototipo con arte que tipo, vida, ataque y defensa se lean.
- **Móvil (Fase 6).** A ~26 pt por casilla, la ficha completa no entra (el director de arte
  estima unos 2 elementos) y la casilla queda por debajo del área táctil recomendada (~44 pt).
  Se decide en esa fase qué se ve siempre y qué al tocar la pieza; el daño previsto sobre el
  objetivo se ve siempre (pilar 1). Puede pedir una versión simplificada de cada figura por
  pack: más trabajo de arte.
- **La imagen de referencia** (`design/references/campo-de-batalla-ref.jpg`) tiene casillas
  dibujadas, suelo saturado y figuras azules, verdes y rojas. Se usa como atmósfera, no como
  paleta ni como referencia de formas (lleva marca de agua de un generador de imágenes).

## 2. Mood & Atmosphere

### Regla de ambiente

El tablero es el mismo toda la partida. El ambiente cambia con cuatro recursos, y ninguno
puede subirle el contraste ni la saturación al terreno:

1. **Gradación del terreno:** temperatura y brillo. Solo puede bajar o correr el tono del
   terreno, nunca subirlo por encima del límite de la Sección 1 (saturación HSV < ~35%).
2. **Viñeta:** oscurece los bordes del tablero.
3. **Interfaz:** banners, recuadros y el timer.
4. **Efectos breves de la capa del sistema:** flechas, pulsos y destellos.

La gradación **no** toca la figura, la ficha ni el suelo de bando: se dibujan encima con
exposición fija, así que el orden de lectura (ficha, figura, bando, terreno) queda igual en
todos los estados.

La luz base del mundo es de **media tarde**: blanca levemente cálida (~5000 K), con sombras
cortas pintadas hacia abajo a la derecha. Las figuras vienen pintadas con esa luz y no se
reiluminan.

### Estados

| Estado | Emoción objetivo | Luz (hora, temperatura, contraste) | Descriptores | Energía (1-5) | Elemento que carga el ambiente |
|---|---|---|---|---|---|
| **Menú y opciones** | Calma de cuartel antes de salir: "elijo con tiempo" | Mañana temprana, ~5500 K neutra-fría, contraste bajo | quieto, ordenado, abierto, sobrio | 1 | Tablero vacío de fondo, desenfocado y un 25% más oscuro, con los dos castillos y sin piezas. El estandarte de cada bando cuelga quieto |
| **Tirada del Modificador** (≤ ~1,2 s) | Expectativa breve, sin angustia: el azar se resuelve antes del plan (pilar 1) | Base, con el terreno un 15% más oscuro y viñeta suave | breve, central, legible, ceremonial | 3 | Una placa con el nombre de la cara cae al centro del tablero y se asienta con un golpe seco. Las probabilidades se consultan con el ícono de signo de pregunta (Sección 7) |
| **Planificar** (reclutar + mover) | Concentración tranquila: "estoy armando algo" | Base de media tarde, contraste de terreno normal. Estado de referencia | claro, despejado, paciente, constructivo | 2 | Casillas de movimiento marcadas con puntos. En reclutamiento, el recuadro central de elecciones y un resplandor tenue en el lado del castillo propio que mira al rival |
| **Asignar ataques** | Tensión enfocada: "apunto y veo el resultado" | Terreno un 20% más oscuro, viñeta media, contraste de la capa del sistema al máximo | afilado, enfocado, previsor, contenido | 3 | Las flechas convergen y la vista previa de daño queda sobre cada objetivo. Las placas MUERE y ROTO son lo más contrastado de la pantalla |
| **Resolución / cosecha** (≤ ~2 s) | Ver el plan cumplido; en un combo, un golpe grande | Pulso de luz blanca cálida (~4000 K) desde cada objetivo, ~0,3 s; después el terreno vuelve a la base | simultáneo, contundente, rápido, concluyente | 5 | Todos los impactos en el mismo cuadro. Un objetivo con varios atacantes recibe un impacto más grande que la suma de los chicos. Las piezas muertas caen juntas |
| **Turno del rival** | Vigilancia: "veo lo que hace y preparo la respuesta" | Base, con el terreno un 10% más frío (~5800 K) | atento, expuesto, paciente, legible | 2 | Banner del turno con el bando del rival. Sus acciones se ven a medida que ocurren (anti-pilar: sin información oculta). Pesa sobre todo en el online; en el juego local casi no se nota |
| **Distracción (sin movimiento)** (≤ ~1,5 s) | Fastidio corto que se cierra rápido, no castigo largo | Nublado: terreno desaturado un 40%, ~6500 K, contraste bajo | apagado, caído, breve, seco | 1 | El estandarte propio del HUD se dobla y cae. Una placa gris dice "Distracción" y el turno pasa enseguida a la fase de ataque |
| **Tramo final** (desde el minuto 10, 40 + 20) | Urgencia sostenida: "se acaba, hay que cerrar" | Atardecer: ~3800 K, sombras de viñeta más largas, terreno un 10% más oscuro, hasta el final de la partida | apurado, dorado, decisivo, estrecho | 4 | Cambio de luz permanente, anunciado una sola vez con un banner "Tramo final". El timer pasa a anillo grueso ámbar |
| **Timer en los últimos 10 s** | Presión puntual | Sin cambios en el tablero | pulsante, puntual | 4 | Solo pulsa el timer del HUD, una vez por segundo. El tablero **no** destella: competiría con la ficha |
| **Victoria** | Orgullo y cierre: "el plan funcionó" | El cielo se despeja: el ámbar del tramo final se va en 0,25 s y el terreno vuelve a la luz base (~5000 K). El Rey propio gana contraste porque baja lo de alrededor: viñeta circular centrada en él y el resto del terreno un 25% más oscuro. La figura no se toca | triunfal, claro, limpio, corto | 4 | Cae el Rey rival, el estandarte del ganador se iza en el centro, resumen de la partida y revancha a un toque (pilar 4) |
| **Derrota** | Aceptación con ganas de revancha, sin humillación | Crepúsculo frío (~7000 K), terreno desaturado un 30% | sereno, frío, digno, breve | 2 | Cae el Rey propio y baja su estandarte. Revancha a un toque y la misma pantalla de resumen |

### Reglas de ritmo

- Las transiciones de estado duran ≤ 0,25 s, salvo la del tramo final (1 s, una sola vez).
  No le suman tiempo al turno (pilar 4).
- La placa del Modificador, el banner de Distracción y el de Tramo final aparecen al centro y
  nunca tapan una casilla con pieza por más de 1 s.
- Victoria y derrota son pantallas espejo: la misma composición con luz opuesta ("luz que
  vuelve" contra "luz que se va"). Victoria no usa el atardecer del tramo final, para que los
  dos estados no se confundan.
- Distracción tiene ambiente propio pero corto (≤ 1,5 s), para no darle dramatismo a una cara
  que el director creativo marcó en contra de los pilares 1 y 3.

## 3. Shape Language

### Siluetas de las piezas

Cada tipo se define por un rasgo, no por un diseño fijo: cada pack de skins reimagina el arma,
el escudo o la montura con su estilo. La skin cambia materiales y estilo; nunca cambia el
rasgo del tipo. Esto concreta "el tipo manda sobre la skin" (ancla, principio 2) y "el plan se
lee antes que el detalle" (Sección 1, principio 1; pilar 1). Definiciones del jugador
(2026-11-17):

| Tipo | Rasgo que lo identifica |
|---|---|
| **Soldado** | Porta un arma de corto alcance (espada, maza, palo…) |
| **Arquero** | Usa un arma a distancia |
| **Escudero** | Lleva un escudo de gran tamaño, de hasta el 60% de la figura |
| **Catapulta** | Ocupa toda la casilla. Es un dispositivo capaz de lanzar objetos contundentes |
| **Jinete** | Va montado en algo que puede transportarlo |
| **Demoledor** | Lleva un explosivo. Es una persona descuidada, capaz de sacrificarse |
| **Rey** | A pie, con un arma de corto alcance. Se distingue por su vestimenta y por ser más alto que el soldado. No va montado, para no confundirse con el jinete |

**Reglas de silueta:**
- La figura ocupa ~70-80% de la casilla (la catapulta, la casilla entera).
- Contorno oscuro continuo en todas las figuras, con el color Tinta de la Sección 4.
- Dos tipos no pueden confundirse en silueta. Los pares a vigilar son Rey y soldado (los dos
  con arma corta: los separan la vestimenta y la altura) y jinete y catapulta (los dos
  anchos).
- **Prueba de aprobación de skins:** las siluetas rellenas en negro, a 68 px y sin ficha,
  tienen que dar ≥ 90% de aciertos de tipo con 5 personas que no conozcan el pack. Si falla,
  se rechaza la skin. Es la regla que garantiza que el tipo se lea aunque cada pack
  reinterprete el rasgo. La prueba se hace con el cuadro de reposo **y** con el cuadro del
  golpe del ataque, porque la animación exagerada puede deformar el rasgo (Sección 9).
- En el MVP (1 o 2 orientaciones) el arma o la montura apuntan hacia el rival, lo que también
  respalda el bando (Sección 4).

### Terreno

- **Geometría orgánica y de baja frecuencia:** manchas redondeadas de tierra y pasto, piedras
  redondeadas que no pasan de ~0,3 casilla y nunca están en el centro de una casilla. No hay
  líneas rectas ni cuadrícula dibujada (ancla, principio 1).
- **Matas de pasto como marco:** en una casilla con pieza, cuatro matas en las esquinas
  forman el marco. Son esquinas, no un borde continuo, para no dibujar la cuadrícula. Son de
  apoyo: bordes blandos y sin contorno.
- **Sin detalle de alta frecuencia** (ramitas, grietas finas) que a tamaño de casilla se lea
  como ruido o como un ícono.

### Castillo y muros

- Son **la única geometría rectilínea del mundo:** bloques cuadrados y 16 tramos de una
  casilla cada uno. La recta dice "hecho por personas, se puede romper" y lo separa del
  terreno.
- **Jerarquía intermedia:** tienen contorno, pero más fino y más claro que el de las figuras.
  Están por encima del terreno y por debajo de las piezas.
- **El daño se ve en la forma, no solo en un número:** el tramo entero tiene borde recto; el
  dañado (< 50% de vida) tiene melladuras en el borde superior; el roto queda como hueco con
  escombro bajo y borde quebrado. ROTO se lee por la forma, sin depender del color.
- **Arquero en el muro:** la figura se apoya sobre el tramo y un parapeto marca el +2 de
  escudo. La silueta del arquero queda entera.

### Gramática de formas de la interfaz

**La capa del sistema tiene un lenguaje propio, geométrico y plano.** El marco de los menús
hace un eco leve del mundo.
- **Por qué:** la ficha tiene que verse distinta de cualquier skin (ancla, principio 3). Si
  estuviera pintada como el mundo, un pack podría confundirse con ella.
- **Ficha:** rectángulo de esquinas redondeadas, plano, sin textura.
- **Íconos:** el puño es redondeado (nudillos); el escudo es heráldico, con la punta abajo.
  Los íconos de tipo son planos y no copian ninguna skin: representan el rasgo del tipo (arma
  corta, arma a distancia, escudo grande, lanzador, montura, explosivo, Rey).
- **Flechas de ataque:** arcos curvos. Cuando varios atacantes van al mismo objetivo, sus
  flechas **se funden en una sola punta**, más gruesa según la cantidad de atacantes (Sección
  1, principio 2; pilar 2).
- **Marcas sobre el tablero:** movimiento con puntos; alcance de ataque con corchetes en las
  esquinas; área de catapulta con un anillo doble (centro con daño completo, anillo exterior
  con la mitad); cobertura del escudero con una línea eslabonada entre el escudero y el aliado
  cubierto.
- **Placas de estado:** MUERE con calavera y texto; ROTO con una grieta en zigzag y texto.
- **Menús:** el marco toma la piedra de los muros y los estandartes, pero los botones y
  textos usan el lenguaje plano del sistema.

### Formas protagonistas y de apoyo

| | Protagonistas | De apoyo |
|---|---|---|
| Elementos | Ficha, figuras, flechas, placas MUERE y ROTO, recuadros del centro (reclutar, Modificador) | Terreno, matas, piedras, muros enteros, marco de menús |
| Borde | Duro, con contorno | Blando, sin contorno (muros: contorno fino) |
| Contraste | El máximo de su zona | Nunca más que una figura |
| Movimiento | Solo en su fase | Quieto (el terreno no se anima) |

## 4. Color System

Luminosidad (L\*, 0 = negro, 100 = blanco) y contraste sobre Tinta calculados el 2026-11-17
(fórmulas sRGB → L\* y contraste WCAG). Para texto de la ficha se exige contraste ≥ 4,5.

### Paleta primaria

| Color | Hex | Qué significa en este mundo |
|---|---|---|
| **Tierra** | `#6E6150` | Suelo del campo de batalla. Fondo neutro de todo (saturación ≈ 27%) |
| **Pasto viejo** | `#6F7451` | Pasto y matas que enmarcan casillas. Verde oliva apagado, para no confundirse con la vida (saturación ≈ 30%) |
| **Piedra de muro** | `#8A867C` | Castillos y muros: lo construido y lo que se puede romper (saturación ≈ 10%) |
| **Tinta** | `#1E1A17` | Contornos de figuras, fondo de la ficha y placas MUERE y ROTO. Ancla de contraste (L\* 10) |
| **Hueso** (bando claro) | `#EFE4C8` | Suelo de casilla del bando claro. Reservado para la capa del sistema |
| **Ciruela** (bando oscuro) | `#3B2350`, borde interior `#7A5A94` | Suelo de casilla del bando oscuro. Reservado para la capa del sistema |

### Colores semánticos (capa del sistema)

| Significado | Hex | L\* | Contraste sobre Tinta | Respaldo de forma (obligatorio) |
|---|---|---|---|---|
| **Vida** | Verde claro `#8EDB6A` | 80 | 10,3 | El ícono de tipo **y** el número de vida van los dos en este color, en la primera posición de la ficha (decisión del jugador) |
| **Ataque** | Bermellón `#E55A40` | 56 | 4,8 | Ícono de puño y número |
| **Escudo** | Acero `#7F9BB8` (azul grisáceo, poco saturado) | 63 | 6,0 | Ícono de escudo heráldico y número |
| **MUERE** (vista previa) | Tinta con texto `#F5F1E8` y borde bermellón de 1 px | — | — | Calavera, la palabra MUERE y la figura desaturada |
| **ROTO** (vista previa) | Tinta con texto `#F5F1E8` | — | — | Grieta en zigzag, la palabra ROTO y el tramo dibujado como hueco |
| **Alerta del timer** | Ámbar `#E89A2C` | 70 | 7,5 | Anillo grueso y pulso, en una posición fija del HUD |

**Vida contra ataque:** 24 puntos de L\* de diferencia. Con daltonismo rojo-verde los dos
tonos se parecen, pero uno se ve claro y el otro medio. La vida, lo primero que se lee, tiene
el mejor contraste de la ficha. El bermellón del borrador (`#D9442B`, contraste 4,0) se
aclaró para llegar a 4,5.

### Colores de bando

Decisión del jugador (2026-11-17): **Hueso contra Ciruela.**

- **El problema:** en los prototipos los bandos eran azul y rojo. El ataque es rojo y el
  escudo azul, y el bando se pinta en el suelo, justo debajo de la ficha: un suelo rojo al
  lado de un puño rojo se lee como "esta pieza pega más".
- **Ni rojo, ni azul, ni verde:** esos tonos quedan para ataque, escudo y vida. El violeta no
  aparece en ningún otro lugar del juego y Hueso es un claro casi sin tono.
- **Separación por luminancia** (L\* ~91 contra ~19), como blancas contra negras en el
  ajedrez. Es la única diferencia que sobrevive a los tres tipos de daltonismo y a la escala
  de grises. El terreno está en L\* ~40-55: Hueso queda muy arriba y Ciruela muy abajo.
- **Borde interior** de 2 px `#7A5A94` en el suelo Ciruela, para que el contorno Tinta de la
  figura no se pierda sobre el suelo oscuro.
- **Respaldo sin color:** (1) el arma o la montura apuntan hacia el rival; (2) cada bando
  tiene un emblema distinto en el HUD y en el banner de turno.
- Hueso y Ciruela son exclusivos del bando: ningún botón, menú ni efecto los usa.

Descartadas: Oro contra Violeta (con daltonismo rojo-verde el oro se acerca al ataque y a la
alerta; en desierto se acerca al terreno) y azul contra rojo (obligaba a cambiar ataque y
escudo, contra el ancla).

### Flechas de ataque

- **Color:** bermellón de ataque `#E55A40` al 100%, con un borde Tinta `#1E1A17` de 2 px a
  cada lado del trazo. El bermellón dice "esto es un ataque", igual que el puño de la ficha.
  El borde separa la flecha del terreno (L\* ~40-55, casi la misma luminancia que el
  bermellón, L\* 56) y de las figuras. Sobre Ciruela se lee por luminancia (56 contra 19) y
  sobre Hueso por el borde Tinta.
- **Sin color de bando:** solo se ven las flechas del jugador de turno, y el banner de turno
  ya dice quién es. Hueso y Ciruela no se usan en las flechas: una flecha Hueso desaparece
  sobre un suelo Hueso y rompería la exclusividad del bando.
- **Punta fundida:** el mismo bermellón con el mismo borde Tinta. La cantidad de atacantes se
  cuenta con el grosor, no con el color: 1 atacante, trazo y punta base; 2, ×1,5; 3 o más, ×2
  (tope). La forma lleva el combo, así que se lee igual en grises y con daltonismo (pilar 2).
- **Contra el alcance:** el alcance también es bermellón, pero al 70% y con corchetes quietos
  en las esquinas. La flecha va al 100% y es un arco con punta, así que siempre gana en
  contraste.
- **Contra la cobertura del escudero:** la flecha es un arco continuo con punta; la cobertura
  es una línea recta eslabonada, acero, sin punta, y une dos piezas del mismo bando.

### Temperatura de color

- **Mundo base:** neutro tirando a cálido (media tarde). El terreno es la única capa que
  recibe la gradación de los estados de la Sección 2.
- **Variante sin castillos:** la misma paleta sin Piedra de muro. Ningún color del sistema
  cambia.
- **Futuras facciones de terreno:** se expresan con materiales y motivos, nunca con el color
  de bando (ancla).
  - **Bosque:** más frío, verdes azulados, saturación < 35% y tono lejos del verde de vida.
  - **Río:** agua gris verdosa (`~#6C7A72`), nunca azul acero, para no parecer escudo.
  - **Desierto:** ocres, con el valor máximo del terreno ≤ 70% para que Hueso siga siendo lo
    más claro.
- **Regla general:** ningún terreno usa tonos dentro de ±15° de los cuatro semánticos
  (vida, ataque, escudo, alerta) con saturación > 35%, ni la luminancia de los dos bandos.

### Paleta de interfaz

- **Cómo se ve:** colores planos, sin textura ni gradiente pintado, con contraste de texto
  ≥ 4,5:1 (WCAG AA).
- **Fondo:** Tinta al 85-90% en la ficha, los recuadros del centro y los botones.
- **Texto:** blanco hueso `#F5F1E8`, distinto del Hueso de bando (más claro y en otro uso).
- **Botón primario:** latón `#C9A55A`. Es el único acento de la interfaz sin significado
  propio; va siempre con texto y forma de botón.
- **Marco de menús:** Piedra de muro.
- **Diferencia con el mundo:** el mundo está pintado, apagado y recibe gradación. La interfaz
  es plana, saturada solo en los semánticos y nunca recibe gradación.

### Daltonismo: pares en riesgo y su respaldo

| Par en riesgo | Tipo | Respaldo |
|---|---|---|
| Vida verde contra ataque bermellón | Protan y deutan | 24 puntos de L\* de diferencia; la vida comparte color con el ícono de tipo, el ataque va con el puño |
| Ataque bermellón contra alerta ámbar | Protan y deutan | El ámbar solo aparece en el timer, en una posición fija del HUD, con forma de anillo |
| Vida verde contra pasto | Protan y deutan | La vida va siempre sobre la placa Tinta de la ficha, nunca sobre el terreno |
| Escudo acero contra piedra de muro | Tritan | Ícono de escudo; el muro tiene forma de bloque |
| Bando Hueso contra Ciruela | Todos (en tono) | Pasan por luminancia; además, orientación de la figura y emblema en el HUD |
| MUERE / ROTO | Todos | Calavera o grieta, más texto y figura desaturada o tramo hueco. El borde bermellón es decorativo |
| Movimiento, alcance, área, cobertura | Todos | Movimiento: puntos blancos al 60%. Alcance: corchetes en las esquinas, bermellón al 70%. Catapulta: anillo doble. Cobertura del escudero: línea eslabonada acero. Se distinguen por forma en grises |
| Placa del Modificador | Todos | Nombre escrito más un ícono por cara |
| Flecha de ataque contra cobertura del escudero | Todos (en grises, L\* 56 contra 63) | La flecha es un arco continuo con punta y borde Tinta; la cobertura es una línea recta eslabonada sin punta. El combo se lee por el grosor de la punta fundida, no por el color |

**Respaldo sonoro:** se pide al director de audio una señal distinta para MUERE y ROTO al
confirmar la vista previa, para el tramo final y para los últimos 10 s del timer.

**Prueba de validación:** capturas de un turno de ataque pasadas por los filtros protan,
deutan y tritan y por escala de grises. En las cuatro versiones se tienen que leer bando,
tipo, vida, MUERE y ROTO.

## 5. Character Design Direction

### Arquetipo del jugador

El jugador no tiene figura propia: es el general, y su presencia es el **estandarte y el
emblema de su bando** en el HUD y en los banners. El Rey es la pieza más importante del
tablero, pero no es un avatar. Así ninguna figura de "héroe" compite con la lectura de las
demás piezas (Sección 1).

### Aliados y enemigos de un vistazo

Se distinguen por tres cosas a la vez, y ninguna depende de la skin:

1. **Suelo de la casilla** en el color del bando (Hueso o Ciruela, Sección 4).
2. **Orientación:** el arma o la montura apuntan hacia el rival.
3. **Ficha:** igual para los dos bandos; nunca cambia de forma ni de color por bando.

Las dos tropas pueden usar el mismo pack de skins y se siguen distinguiendo; si cada jugador
usa un pack distinto, también. No hay NPC ni piezas neutrales.

### Pose y expresión

- **Reposo:** pose quieta (puede tener gracia, Sección 9), con el arma o el escudo a la vista, como una miniatura
  sobre su base. Respiración mínima: 1-2 px de desplazamiento y ≤ 4 cuadros, para que el
  tablero no "tiemble" con 25 o más piezas a la vez (Sección 3: lo de apoyo no se anima).
- **Ataque:** gesto exagerado y corto (anticipación, golpe, vuelta) que se lee aunque dure
  poco, porque todos los ataques caen juntos en ≤ 2 s (Sección 1, principio 3).
- **Movimiento:** desplazamiento por código en el MVP, sin ciclo de caminata.
- **Rostros:** a 68 px no aportan, así que no se invierte en expresión facial. El carácter
  de cada pack lo dan materiales, siluetas de casco o tocado y sus colores propios.
- **Golpe y muerte:** la pieza que muere se desatura y cae, en el mismo momento que las demás
  (Sección 2, resolución).

### Detalle a la distancia de juego

Hay una sola distancia de cámara (el tablero entero en pantalla), así que no hay niveles de
detalle por distancia. La regla es por tamaño de casilla (68 px, Sección 1):

- **Se conserva:** silueta, arma o montura, contorno Tinta, 2-3 masas grandes de color propio
  del pack y la luz fija.
- **No se dibuja:** detalle menor a 2 px (costuras, grabados, texturas finas): a ese tamaño
  es ruido.
- **Colores prohibidos en las skins:** Hueso y Ciruela, y bermellón, acero o verde claro con
  saturación alta. Son de la capa del sistema (Sección 4).
- **Móvil (Fase 6):** a ~26 pt puede hacer falta una versión simplificada de cada figura. Se
  decide en esa fase (Sección 1).

### Luz de las figuras

Una sola luz fija desde arriba a la izquierda, igual para todos los packs y pintada en el
sprite, con sombras cortas hacia abajo a la derecha (Sección 2: luz base de media tarde). Así
figuras de packs distintos se ven parte del mismo tablero.

## 6. Environment Design Language

### Estilo del castillo

- **Castillo base:** medieval europeo sobrio. Muros bajos de piedra seca, de una casilla de
  alto, con almenas simples y sin torres (las torres están fuera de las reglas). Interior de
  5 × 5 de empedrado gastado, más claro y más plano que la tierra de afuera. Es la única obra
  humana del tablero: se lee como "lo que se defiende".
- **Castillo por pack (decisión del jugador, 2026-11-17):** cada pack de skins puede traer su
  propio castillo (por ejemplo, Japón medieval: piedra en talud y tejas). Tiene que respetar
  la Sección 3: geometría recta, 16 tramos de una casilla y los tres estados de daño (entero,
  mellado, roto). Suma trabajo de arte por pack: muros en tres estados más el interior.
- **Variante sin castillos:** campo abierto; el Rey sobre una casilla de tierra apisonada.

### Textura: pintada a mano, sin PBR

Pinceladas visibles pero suaves. Tres razones:
1. Combina con figuras que parecen miniaturas pintadas (ancla).
2. Permite controlar exactamente la saturación y el contraste del suelo (regla de la Sección
   1).
3. Es liviano para web (primera descarga < 20 MB según el concepto).

Sin luces en tiempo real: la luz está pintada, y los estados de la Sección 2 solo aplican
gradación al terreno.

### Densidad de elementos

| Zona | Densidad | Regla |
|---|---|---|
| **Campo** | Escasa | Manchas de tierra y pasto; piedras < 0,3 casilla, como mucho una cada 3-4 casillas y nunca en el centro de una. Ningún elemento ocupa una casilla ni parece un obstáculo: el terreno no tiene reglas |
| **Variantes de casilla** | 4-6 del mismo suelo, rotadas | Para que el tablero no se vea repetido |
| **Interior del castillo** | Mínima | Solo empedrado, sin muebles ni decorado |
| **Borde del tablero** | — | El terreno sigue un poco más allá de las 24 × 15 casillas y se oscurece hacia los bordes de la pantalla (Sección 7). Sin marco de madera ni de mesa |

### El terreno cuenta la batalla

Sin texto ni números:
- **Catapulta:** donde cae deja una mancha de tierra removida.
- **Demoledor:** deja una marca chamuscada.
- **Muro roto:** deja escombros bajos.
- **Castillo:** cuantos más tramos dañados, más escombros alrededor del muro; se lee de
  lejos qué castillo viene perdiendo.

**Límites:** las marcas quedan por debajo del contraste de cualquier figura (Sección 1), no
superan la saturación del terreno y se desvanecen a los 3 turnos, para que el tablero no se
ensucie. Las piezas muertas no dejan cuerpo: caen y desaparecen, y la casilla queda libre a la
vista.

## 7. UI/HUD Visual Direction

### Pantalla y HUD

Decisión del jugador (2026-11-17): **el tablero ocupa toda la pantalla**, sin franja lateral.
El HUD es de pantalla (no diegético) y mínimo; solo la ficha, las marcas y los recuadros del
centro viven sobre el tablero.

**Medidas a 1080p:** casillas de **68 px**. El tablero mide 24 × 68 = 1632 px de ancho y
15 × 68 = 1020 px de alto. Quedan 60 px arriba para el timer y 144 px a cada lado para los
botones, así que ningún elemento del HUD tapa una casilla (los castillos pueden caer en las
esquinas).

| Lugar | Qué va |
|---|---|
| **Contorno del tablero** | Del color del bando de turno (Hueso o Ciruela): se sabe de quién es el turno desde cualquier parte de la pantalla |
| **Arriba al centro** | Timer, siempre visible, con el emblema del bando de turno |
| **Arriba a la derecha** | Ícono de signo de pregunta: abre las probabilidades del Modificador de los dos jugadores (pilar 3: se pueden consultar, no hace falta tenerlas siempre a la vista) |
| **Abajo a la derecha** | Botón de avanzar ("Atacar" en la fase de mover, "Terminar turno" en la de atacar) y, arriba de él, el contador de piezas movidas (por ejemplo, 5/12) |
| **Abajo a la izquierda** | "Deshacer último movimiento". Va en la esquina opuesta al botón de avanzar para que no se toque uno por el otro |
| **Centro del tablero** | Recuadros del Modificador y de reclutar (Sección 2) |
| **Sobre cada pieza** | La ficha (ver abajo) |

**Sin historial durante la partida.** El registro de lo que pasó sirvió en los prototipos para
entender las reglas; en el juego final no va. Lo que pasa se ve en el tablero: daño flotante
sobre cada objetivo en la resolución y las placas MUERE y ROTO. **Al terminar la partida**, un
botón "Ver cómo se desarrolló" abre una repetición turno por turno.

**Mínimo 1080p** para jugar en web (decisión del jugador): debajo de eso los números de la
ficha bajan de ~14 px y no se leen.

### La ficha

Decisión del jugador (2026-11-17): la ficha va **sobre la parte baja de la figura**, con fondo
Tinta semitransparente. La figura conserva el 70-80% de la casilla (Sección 3) y la ficha la
tapa en parte.

- **Contenido:** ícono de tipo y número de vida (verde claro), puño y número de ataque
  (bermellón), escudo y número de defensa (acero). Números ≥ 14 px, negrita, con borde Tinta.
- **Opción "Ficha simple":** el jugador que ya conoce las estadísticas puede ocultar ataque y
  defensa; quedan el tipo y la vida. Arranca apagada. Con la opción activa, ataque y defensa
  siguen viéndose en la pieza seleccionada, y la vista previa de daño de la fase de ataque se
  ve siempre (pilar 1).
- **En la fase de ataque**, la vista previa del daño reemplaza a la ficha del objetivo
  mientras dura la fase, en lugar de ponerse al lado: es lo que más importa en ese momento.

### Tipografía

Una familia sans serif condensada y negrita, de licencia libre (tipo Barlow Condensed o Roboto
Condensed), con números de ancho fijo para que no salten al cambiar. Una fuente con carácter
medieval solo para títulos de menú y banners (victoria, Tramo final), nunca en números ni en
texto de reglas.

| Uso | Tamaño a 1080p |
|---|---|
| Números de la ficha | ≥ 14 px, negrita, con borde Tinta |
| Texto de menús y recuadros | 16 px |
| Títulos | 20-24 px |
| Timer | 28-36 px |
| Nombre en la placa del centro | 34-40 px |

### Íconos

Planos, de un solo color, con trazo grueso, en una grilla común de 24 px: puño, escudo, los 7
íconos de tipo (representan el rasgo de cada uno, Sección 3), los 5 de las caras del
Modificador, los emblemas de los dos bandos, el signo de pregunta, deshacer y avanzar.

### Animación de la interfaz

Rápida, ≤ 0,25 s, con frenado suave al final. El único "golpe" es la placa del Modificador al
asentarse (Sección 2). En la resolución, los números de vida cambian de una vez, sin contar
hacia abajo (pilar 4).

### Usabilidad y accesibilidad

- **Lo importante, lo más visible:** en la fase de ataque, la vista previa y las placas MUERE y
  ROTO ganan a todo (Sección 2).
- **Nada solo por color:** cada color semántico tiene forma o ícono (Sección 4). El turno se
  marca con el contorno **y** con el emblema junto al timer.
- **Destellos:** el pulso del timer (1 por segundo) y el destello de resolución (0,3 s) están
  por debajo del límite de 3 por segundo. Opción "Reducir efectos" para apagarlos.
- **Controles:** todo funciona con un clic o un toque, sin información que aparezca solo al
  pasar el mouse (regla del proyecto). Navegación con teclado y mando cuando lleguen las
  consolas.
- **Qué pasó y por qué:** daño flotante y placas en el tablero durante la resolución;
  repetición completa al final.

### Conflictos resueltos

| Conflicto | Arte | Usabilidad | Decisión del jugador |
|---|---|---|---|
| Lugar de la ficha contra tamaño de la figura | Figura al 70-80% para que el pack luzca | Ficha completa siempre visible (pilar 1) | Ficha sobre la parte baja de la figura, con opción "Ficha simple" |
| Resoluciones chicas | — | A 720p los números quedan en ~10 px | Mínimo 1080p |
| Probabilidades sin franja lateral | — | El pilar 3 pedía verlas siempre | Ícono "?"; se ajustó el pilar 3 del concepto |

## 8. Asset Standards

### Mirada de arte

**Formatos y resoluciones**

| Nivel | Tamaño por casilla | Para qué |
|---|---|---|
| Original (PSD o Krita, por capas) | 272 px (4×) | Fuente de todo; sirve para pantallas grandes y para la versión simplificada del móvil (Fase 6) |
| Exportación (PNG con transparencia) | 136 px (2×) | Lo que entra al juego. A 1080p se ve a 68 px; a 4K, a 136 px sin perder nitidez |
| Íconos (SVG) | 24 y 48 px | Interfaz (Sección 7) |

- **Figuras:** recortadas al borde, con el punto de apoyo en el centro de los pies, la luz
  fija ya pintada (Sección 5) y nunca con colores de bando (Sección 4).
- **Sin niveles de detalle por distancia** (Sección 5). La versión simplificada para el móvil
  se decide en la Fase 6.

**Nombres de archivo:** `[categoría]_[nombre]_[variante]_[estado].png`, en minúsculas y con
guion bajo. Ejemplos: `pieza_soldado_base_ataque_03.png`, `pack_japon_arquero_reposo_01.png`,
`castillo_base_muro_mellado.png`, `terreno_tierra_v04.png`, `ui_icono_puno_24.png`. La regla
PascalCase de `project.yaml` (`naming.files`) es para archivos de código y escenas, no para
assets de arte.

**Cuadros por pieza**

| | Reposo | Ataque | Movimiento | Orientaciones |
|---|---|---|---|---|
| **MVP** | ≤ 4 | ~6 | Por código | 1 o 2, espejadas |
| **Arte completo** (primer pack de skins) | ≤ 4 | ~6 | Caminata ~8 | 8 direcciones; izquierda y derecha espejadas: 5 dibujadas |

**Terreno y castillo:** 4-6 variantes de casilla; muros en 3 estados (entero, mellado, roto);
interior empedrado; marcas de batalla (cráter, quemadura, escombros). Cada pack que traiga
castillo entrega lo mismo (Sección 6).

### Mirada técnica

**Presupuestos.** El proyecto fija solo **60 fps (16,6 ms por cuadro)**
(`.claude/docs/technical-preferences.md`). Llamadas de dibujo y techo de memoria figuran como
`[TO BE CONFIGURED]`: no hay presupuesto configurado. Se usan como objetivos provisionales los
que propuso el director técnico en `design/gdd/game-concept.md` (Technical Considerations):
primera descarga web < 20 MB, < 40 MB de texturas por partida con 2 skins, < 150 llamadas de
dibujo. **Pendiente:** fijarlos en `project.yaml` (`performance.*`).

**Memoria de texturas, sin comprimir** (un cuadro de figura a 136 × 136 px RGBA ≈ 74 KB):

| Escenario | Por tipo de pieza | Por pack (7 tipos) | 2 packs en partida | Objetivo 40 MB |
|---|---|---|---|---|
| MVP: 2 orientaciones, ~10 cuadros | ~1,5 MB | ~10 MB | ~20 MB | Cumple |
| Arte completo: 5 direcciones dibujadas, ~18 cuadros | ~6,7 MB | ~46 MB | ~93 MB | **No cumple** |

Con compresión de texturas el peso suele bajar entre 4 y 8 veces (~12-23 MB para el arte
completo). **NO VERIFICADO:** qué formatos de compresión admite Unity 6.3 en web no está en
`docs/engine-reference/unity/`; confirmarlo antes de contar con esa reducción.

**Llamadas de dibujo.** Cada pack en un atlas de sprites y el terreno como mosaico: < 150
llamadas es alcanzable con 25-50 piezas y 360 casillas. **NO VERIFICADO:** el detalle del
atlas de sprites en Unity 6.3 no está en la referencia del motor.

**Descarga.** El MVP trae un solo pack base (~10 MB de texturas sin comprimir) más terreno,
castillo y el motor de Unity para web, contra un objetivo de 20 MB: probablemente entra con
texturas comprimidas (no verificado). **Se mide con el primer build web.** Los packs siguientes se
descargan aparte con Addressables (`docs/engine-reference/unity/plugins/addressables.md`).

**Motor en web.** Según `docs/engine-reference/unity/breaking-changes.md`, Unity 6 usa WebGPU
por defecto, con WebGL 2.0 de respaldo.

### Conflicto: 8 direcciones contra el presupuesto de memoria

| | Estándar |
|---|---|
| **Ideal (arte)** | 8 direcciones (5 dibujadas), ~18 cuadros por tipo, a 136 px: ~93 MB sin comprimir con 2 packs |
| **Ajustado (técnico)** | Dentro de 40 MB: packs a 68 px (nítido a 1080p, pierde a 4K) o menos cuadros por animación |
| **Decisión del jugador (2026-11-17)** | Se documentan los dos. Se decide con el primer pack de skins, midiendo el peso real comprimido. No afecta al MVP |

## 9. Reference Direction

Cuatro referencias, cada una para un aspecto distinto (elegidas por el jugador después de ver
capturas, 2026-11-17). Ninguna se toma como paleta general ni como estilo a copiar. Si una
referencia choca con una sección de esta guía, gana la sección.

| Aspecto | Referencia |
|---|---|
| Tono (carácter de las figuras) | Fat Princess |
| Silueta, terreno y ambiente del terreno | Bad North |
| Luz y pintura de la figura | Miniaturas de mesa pintadas |
| Vista previa y lectura del sistema | Into the Breach |

### 1. Fat Princess (PS3, 2009): el tono

| | |
|---|---|
| **Qué se toma** | Solo el **tono de humor**: las figuras tienen gracia y personalidad y la guerra no es solemne. Los tipos se comportan como personajes (el demoledor descuidado, el escudero que se esconde detrás del escudo), no como estatuas |
| **Sirve a** | Sección 5 (pose y expresión) y Sección 3 (cada pack reinterpreta el rasgo del tipo) |
| **Qué se evita** | Sus proporciones de cabeza grande, sus colores saturados, su 3D con sombreado plano, el color de equipo pintado en la figura (prohibido por las Secciones 4 y 5) y el humor apoyado en caras |

**Dónde vive el humor** (decisión del jugador: **solo en las piezas**; el ambiente, la
interfaz, las placas y el Rey quedan serios):

| Lugar | Cómo | Límite |
|---|---|---|
| **Pose de reposo** | En la pose congelada, no en el movimiento: el demoledor con la mecha encendida mirando para otro lado; el escudero medio escondido detrás del escudo; el Rey con el pecho inflado | Respiración de 1-2 px y ≤ 4 cuadros (Sección 5); pasa la prueba de silueta (Sección 3) |
| **Gesto de ataque** | Anticipación exagerada y golpe con estiramiento y aplastamiento: el soldado revolea el arma de más; la catapulta se sacude; el arquero se echa para atrás al soltar | ~6 cuadros, todos los ataques a la vez (Sección 1, principio 3). La deformación no tapa el rasgo del tipo |
| **Caída al morir** | Caída con carácter propio de cada tipo (un tropezón, un giro, el casco que sale volando), con la desaturación de la Sección 5 | Todas las caídas duran lo mismo y terminan en el mismo cuadro (Secciones 2 y 6). **El Rey no tiene caída cómica**: la derrota es digna (Sección 2) |
| **Rasgos del pack** | Armas, escudos o monturas desproporcionados o improvisados dentro del rasgo del tipo (el palo del soldado, una montura absurda para el jinete) | Escudo ≤ 60% de la figura, figura al 70-80% de la casilla (Sección 3); sin colores de bando ni del sistema (Sección 5) |
| **Ficha, flechas, placas, banners, luz** | **Sin humor** | Capa del sistema intocable (ancla, principio 3) |
| **Sonido** | Lo decide el director de audio; desde el arte solo se pide que acompañe el tono liviano de las figuras y nunca el de la capa del sistema | — |

El humor vive en la capa 2 del orden de lectura (la figura): no le quita lugar a la ficha.

### 2. Bad North (Plausible Concept, 2018): silueta, terreno y ambiente del terreno

| | |
|---|---|
| **Qué se toma** | **Silueta (Sección 3):** unidades minimalistas que se reconocen por un solo objeto (espada, lanza, arco, escudo), nunca por el detalle. **Terreno que guarda la batalla (Sección 6):** el suelo queda marcado después del combate y se lee de un vistazo, sin texto. **Ambiente del terreno (Sección 2):** paleta apagada de pocos tonos, cambios de ambiente con gradación de temperatura y brillo, y bordes que se funden hacia afuera (la viñeta y el borde que se oscurece) |
| **Sirve a** | Secciones 2 (solo terreno y viñeta), 3 y 6 |
| **Qué se evita** | El 3D de pocos polígonos con luz en tiempo real (nuestra luz está pintada, Sección 6); el tablero como isla o diorama (el terreno sigue más allá del borde); las unidades diminutas en escuadra (la nuestra es una figura al 70-80% de la casilla); la sangre permanente (nuestras marcas son tierra, quemadura y escombros, y se desvanecen a los 3 turnos); los colores de comandante en la unidad |

### 3. Miniaturas de mesa pintadas: la luz y la pintura de la figura

| | |
|---|---|
| **Qué se toma** | La **luz cenital pintada** en la figura: fija desde arriba a la izquierda, caras de arriba más claras y sombras cortas hacia abajo a la derecha. **2-3 masas grandes de color** por figura, la postura de "pieza sobre su base" en reposo y la sensación de que todos los packs comparten la misma mesa |
| **Sirve a** | Sección 5 (luz, detalle, reposo) y Sección 2 (la luz base aplicada a la figura). El ambiente del terreno lo toma Bad North; de las miniaturas sale solo la luz de la figura, que tiene exposición fija |
| **Qué se evita** | El detalle fino de colección (filos, grabados, degradés de menos de 2 px); la base pintada con el color del bando (eso lo hace el suelo de la casilla); el marco de madera o de mesa (Sección 6); las texturas realistas de fotografía. Nuestras figuras llevan contorno Tinta, que una miniatura real no tiene |

### 4. Into the Breach (Subset Games, 2018): la vista previa sobre el objetivo

| | |
|---|---|
| **Qué se toma** | El **resultado se muestra antes de confirmar**, sobre la pieza afectada: el daño previsto reemplaza a la ficha del objetivo y las amenazas se leen como una capa limpia por encima del mundo. Cada color del sistema tiene un solo significado |
| **Sirve a** | Sección 7 (vista previa en la fase de ataque, placas) y Sección 4 (colores semánticos reservados) |
| **Qué se evita** | El pixel art y la estética de ciencia ficción; la cuadrícula dibujada (ancla, principio 1); su interfaz de neón; los paneles laterales con información (nuestro tablero ocupa la pantalla) |

### Tensiones a validar

1. **Humor sin rostros (Sección 5).** Casi todo el humor de Fat Princess sale de la cabeza
   grande y la cara. Sin las dos, depende de la pose y los objetos y puede quedar flojo. Se
   valida en el primer prototipo con arte; si no alcanza, el jugador decide si reabre "sin
   rostros" (por ejemplo, una expresión mínima de 2-3 px) o acepta un humor más sutil.
2. **Rasgos exagerados contra la prueba de silueta.** Un arma desproporcionada puede acercar
   al soldado al Rey o al demoledor; una montura absurda, al jinete y la catapulta. Decide la
   prueba de silueta de la Sección 3.
