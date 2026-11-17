# Art Bible: Ajedrez táctico (título provisional; código rolchess)

> **Status**: Draft
> **Owned By**: art-director
> **Last Updated**: 2026-11-17
> **Art Director Sign-Off (AD-ART-BIBLE)**: REVISED 2026-11-17 — secciones 1 a 4. Corregidos el color de las flechas de ataque (Sección 4) y la fila Victoria (Sección 2). Secciones 5 a 9 sin escribir (fuera del alcance de la sesión): estándares de assets y producción externa NOT ASSESSED hasta completarlas

`/art-bible` writes these nine sections in this order and replaces each
section's `[To be designed]` line as that section is approved. A section still
holding its placeholder is incomplete; a later run fills only those.

Fuente: `design/gdd/game-concept.md`, sección "Visual Identity Anchor: Campo de Batalla".

## 1. Visual Identity Statement

### Regla visual

**Lo que decide la jugada se lee primero, en la casilla de ~60 px de la web. Después la
figura, después el bando y al final el terreno. Ante la duda, se apaga lo de abajo para que
se lea lo de arriba.**

**Tamaño de diseño (decisión del jugador, 2026-11-17):** el arte se diseña para la casilla de
la web, ~60-64 px de alto (1080p: 15 filas de 72 px, menos el HUD). El móvil llega en la
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
  reconoce por silueta, en grises y a ~60 px de alto de casilla.

**2. El combo se ve como un solo golpe.** Sirve al **pilar 2: La fuerza está en combinar**
("un golpe combinado se siente distinto de varios golpes sueltos").
- Cómo se hace: en la vista previa y al resolver, los atacantes sobre un mismo objetivo se
  dibujan con trazos que convergen en él, y el efecto sobre el objetivo crece con la cantidad
  de atacantes. Vive en la capa del sistema: ninguna skin lo toca.
- *Prueba de diseño:* cuando es ambiguo si resaltar cada golpe o el conjunto, gana el
  conjunto. Tres arqueros contra un escudo se leen como una jugada, no como tres.

**3. El ritmo del turno manda sobre la animación.** Sirve al **pilar 4: Duelo corto** (10 a
15 minutos).
- Cómo se hace: todos los ataques de una resolución se animan a la vez, no uno detrás de
  otro. Propuesta a validar en prototipo: la resolución completa de un turno no pasa de ~2 s.
  Esto también ayuda a que el MVP reducido (movimiento y ataque por código, 1 o 2
  orientaciones) no se sienta pobre.
- *Prueba de diseño:* cuando una animación más rica alarga la resolución, gana la que entra
  en esa ventana. Las 8 direcciones y los cuadros extra se suman solo si no alargan la
  resolución.

### Riesgos a validar

- **Tamaño de la ficha.** En una casilla de ~60 px entran el ícono de tipo, dos íconos, tres
  números y la figura. Se prueba en la Sección 7 (interfaz) y en el primer prototipo con arte.
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
| **Tirada del Modificador** (≤ ~1,2 s) | Expectativa breve, sin angustia: el azar se resuelve antes del plan (pilar 1) | Base, con el terreno un 15% más oscuro y viñeta suave | breve, central, legible, ceremonial | 3 | Una placa con el nombre de la cara cae al centro del tablero y se asienta con un golpe seco. Las probabilidades de los dos jugadores ya estaban en pantalla y no se mueven |
| **Planificar** (reclutar + mover) | Concentración tranquila: "estoy armando algo" | Base de media tarde, contraste de terreno normal. Estado de referencia | claro, despejado, paciente, constructivo | 2 | Casillas de movimiento marcadas con puntos. En reclutamiento, el recuadro central de elecciones y un resplandor tenue en el lado del castillo propio que mira al rival |
| **Asignar ataques** | Tensión enfocada: "apunto y veo el resultado" | Terreno un 20% más oscuro, viñeta media, contraste de la capa del sistema al máximo | afilado, enfocado, previsor, contenido | 3 | Las flechas convergen y la vista previa de daño queda sobre cada objetivo. Las placas MUERE y ROTO son lo más contrastado de la pantalla |
| **Resolución / cosecha** (≤ ~2 s) | Ver el plan cumplido; en un combo, un golpe grande | Pulso de luz blanca cálida (~4000 K) desde cada objetivo, ~0,3 s; después el terreno vuelve a la base | simultáneo, contundente, rápido, concluyente | 5 | Todos los impactos en el mismo cuadro. Un objetivo con varios atacantes recibe un impacto más grande que la suma de los chicos. Las piezas muertas caen juntas |
| **Turno del rival** | Vigilancia: "veo lo que hace y preparo la respuesta" | Base, con el terreno un 10% más frío (~5800 K) | atento, expuesto, paciente, legible | 2 | Banner del turno con el bando del rival. Sus acciones se ven a medida que ocurren (anti-pilar: sin información oculta). Pesa sobre todo en el online; en el juego local casi no se nota |
| **Distracción (turno perdido)** (≤ ~1,5 s) | Fastidio corto que se cierra rápido, no castigo largo | Nublado: terreno desaturado un 40%, ~6500 K, contraste bajo | apagado, caído, breve, seco | 1 | El estandarte propio del HUD se dobla y cae. Una placa gris dice "Distracción" y el turno pasa enseguida |
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
- **Prueba de aprobación de skins:** las siluetas rellenas en negro, a ~60 px y sin ficha,
  tienen que dar ≥ 90% de aciertos de tipo con 5 personas que no conozcan el pack. Si falla,
  se rechaza la skin. Es la regla que garantiza que el tipo se lea aunque cada pack
  reinterprete el rasgo.
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
- **Fondo:** Tinta al 85-90% en la ficha y en los paneles.
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

[To be designed] — the player character's visual archetype, how players tell
enemies, NPCs and allies apart at a glance, expression and pose style, and how
much detail survives at game camera distance (LOD philosophy).

## 6. Environment Design Language

[To be designed] — architectural style and its tie to the world's culture,
texture philosophy (painted, PBR or stylised, and why), prop density rules per
area type, and environmental storytelling guidelines.

## 7. UI/HUD Visual Direction

[To be designed] — diegetic versus screen-space HUD, typography (personality,
weight, size hierarchy, minimum readable size), iconography style, UI animation
feel, and any art-direction versus readability conflict and how it was decided.

## 8. Asset Standards

[To be designed] — file formats, naming convention, texture resolution tiers,
LOD levels, export settings, and the engine's hard limits (poly budgets per
asset category, texture memory, material slots). Where an art preference and a
technical limit disagree, record both and the trade-off chosen.

## 9. Reference Direction

[To be designed] — 3–5 references (games, films, art styles or artists). For
each: the specific element to take from it, and what to avoid so the result
does not read as a copy.
