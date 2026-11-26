# Localización

> **Status**: Designed (pendiente de `/design-review` en una sesión nueva)
> **Author**: Martin Arin + agentes
> **Last Updated**: 2026-11-25
> **Last Verified**: 2026-11-25
> **Creative Director Review (CD-GDD-ALIGN)**: APPROVED 2026-11-25
> **Implements Pillar**: Pilar 1 (Planificar y cosechar) y Pilar 4 (Duelo corto), como apoyo

## Summary

Localización guarda todo texto visible en una tabla por idioma (español latino con voseo e inglés) y convierte claves y códigos de las reglas en frases. El jugador entiende por qué una jugada no vale y puede cambiar de idioma al instante, también durante la partida. Existe para que el núcleo de reglas no lleve texto y sumar un idioma no toque pantallas.

> **Quick reference** — Layer: `Foundation` · Priority: `MVP` · Key deps: `Catálogo de piezas (solo ID de tipo), Librería de reglas (lista de códigos)`

## Overview

Localización es la capa que convierte claves en texto. Cada texto que aparece en pantalla (un botón, el nombre de una pieza, un aviso de jugada inválida, el motivo del fin de partida) vive en una tabla por idioma y se pide por una clave; ningún otro sistema escribe texto visible. Al arrancar se elige el idioma, español latino o inglés, y el jugador lo puede cambiar en cualquier momento sin recargar ni interrumpir la partida.

Es un sistema pasivo: el jugador solo lo toca en el botón ES/EN. Existe porque el núcleo de reglas devuelve **códigos**, nunca texto (Librería, regla 4), y alguien tiene que convertir cada código en una frase que explique por qué. Sin él, los textos quedarían repartidos por las pantallas y agregar un idioma obligaría a tocarlas todas. Vive fuera del núcleo y el núcleo no lo conoce.

Las tablas son propias y simples, sin paquete externo (decisión del jugador, 2026-11-25). El formato del archivo, la carga y el almacenamiento de la preferencia → ADR; no hay ninguno todavía.

## Player Fantasy

**«Entendí por qué.»** (Elegida por el jugador, 2026-11-25.) Es infraestructura que no se nota; lo que se siente es lo que hace posible.

*El momento:* quedan 20 segundos, el jugador lleva un soldado a una casilla ocupada y el juego dice «Esa casilla está ocupada». Lo lee de un vistazo, elige otra y el plan sigue. No hubo duda sobre si falló el clic, la regla o el juego.

Sirve al **Pilar 1** («Al mover armás la estrategia; al atacar ejecutás el plan y ves sus frutos»): un rechazo sin explicación corta el plan. También al **Pilar 4** («La partida dura entre 15 y 20 minutos y nunca pasa de 25»): leer no puede gastar el timer.

**Qué le exige al diseño:** un aviso dice la causa, en una frase corta, con las palabras de las reglas; el mismo concepto se llama siempre igual; y nunca aparece en pantalla una clave cruda, un texto cortado o un cuadrado de glifo faltante.

**Alternativa considerada:** «Está hecho para mí» (el juego habla tu idioma desde la primera pantalla). Queda como consecuencia del idioma automático, no como la meta.

**Lo que el jugador nunca debería sentir:** «no sé qué me quiso decir», «¿tramo y muro son lo mismo?», «esto está traducido a máquina».

## Detailed Design

### Core Rules

1. **Todo texto visible sale de una tabla.** Ningún sistema de presentación escribe texto literal. Excepciones cerradas: los números, los símbolos sin idioma (`:` del reloj, `+`, `×`, `/`), el nombre del juego, las etiquetas «ES» y «EN» del botón y la pantalla fija de la regla 13.
2. **Dos idiomas.** `es` (español latino, trato de **vos**; decisión del jugador, 2026-11-25) y `en` (inglés). Lista cerrada en el MVP. Sumar un idioma es sumar una tabla y una entrada en la lista, sin tocar ninguna pantalla.
3. **Idioma fuente.** El español es la fuente: una clave nace en `es` y el inglés la traduce. Las dos tablas tienen **exactamente el mismo conjunto de claves** (F1).
4. **Forma de la clave.** `grupo.nombre`: el grupo es uno de la tabla; el nombre lleva letras ASCII, números y guion bajo. La clave distingue mayúsculas. Una clave publicada no cambia de significado: si cambia el sentido, se crea otra.

   | Grupo | Qué contiene | De dónde sale el nombre |
   |---|---|---|
   | `pieza` | Nombre de cada tipo | Nombre estable del tipo (`pieza.soldado` … `pieza.rey`), por ID 0–6 del Catálogo |
   | `codigo` | Rechazos y errores para el jugador | Nombre exacto del código (`codigo.NoEsTuTurno`) |
   | `evento` | Frases de eventos | Nombre exacto del evento |
   | `fin` | Motivos de fin de partida | Nombre exacto del motivo (`fin.ReyMuerto`, `fin.DobleRey`, `fin.RelojAgotado`) |
   | `fase` | Nombres de fase | Nombre exacto de la fase |
   | `cara` | Nombres de las caras del Modificador | Las define #11 |
   | `ui` | Botones, títulos, etiquetas, ayuda | `ui.pantalla_elemento`; lo define cada GDD de pantalla |
   | `sistema` | `sistema.aviso_neutro`, `sistema.texto_faltante` | Este GDD |

5. **Códigos: la clave se deriva, no se mapea.** Para un código se pide `codigo.` + su nombre. No hay una lista paralela que se desactualice.
   - **Para el jugador** (marcado «Sí» en la regla 4 de la Librería o en el GDD de su regla): tiene texto propio que dice por qué.
   - **Interno que descarta una acción del jugador:** usa `sistema.aviso_neutro` («Esa jugada ya no se puede hacer»), salvo que su GDD pida un texto propio. La Librería cita como ejemplo «el turno terminó por tiempo».
   - **Interno que no descarta nada** (`TimeoutObsoleto`): sin texto.
   - Un código compartido por dos comandos tiene **un solo texto**, que vale para los dos: `FueraDeAlcance` (mover y atacar) es «No llega hasta ahí».
   - Un código puede ser visible o interno según el comando (`EsperandoTirada`). La presentación decide si se muestra; Localización solo responde por la clave.
6. **Nombres de pieza.** Singular y plural por tipo. Español / inglés: Soldado / Soldier · Arquero / Archer · Escudero / Shieldbearer · Catapulta / Catapult · Jinete / Rider · Demoledor / Demolisher · Rey / King. Los nombres en inglés son **propuesta** hasta la revisión de un hablante nativo.
7. **Parámetros con nombre.** Un texto puede llevar `{nombre}`. Las dos tablas usan los mismos parámetros en la misma clave (F2); el orden puede cambiar. Ningún texto se arma pegando trozos. Un parámetro admite un entero de 32 bits o **otra clave** sin parámetros (el nombre de una pieza), que se resuelve en el idioma actual; si esa clave es plural, se usa su forma `uno`. El nombre de jugador del online (fuera del MVP) será texto libre. La sustitución es de una sola pasada: un valor que contiene `{` se muestra tal cual. `{{` y `}}` son llaves literales.
8. **Plurales.** Una clave es simple o plural. La plural tiene las formas `uno` y `otros` y declara qué parámetro entero la decide (F3). Los dos idiomas usan la misma regla. Una clave plural pedida como texto directo con su parámetro (por ejemplo `pieza.<tipo>` con `n`, #17) usa la forma de F3; sin el parámetro, la singular.
9. **Números y género.** Los enteros se escriben sin separador de miles ni decimales; un negativo lleva `-`. Los textos evitan concordar en género con el nombre de una pieza: «Catapulta: sin objetivo», no «La catapulta está bloqueada».
10. **Elección inicial.** En orden: (a) la preferencia guardada, si es un idioma soportado; (b) el primer idioma de la lista del sistema o navegador: si su subetiqueta primaria es `es` (sin distinguir mayúsculas), español; cualquier otra, inglés; (c) si no se puede leer, inglés. No se recorren los demás idiomas de la lista. El idioma automático no se guarda: solo se guarda lo que el jugador elige.
11. **Cambio de idioma.** El botón ES/EN cambia el idioma y guarda la preferencia. En el MVP está antes de la partida y también a mano durante ella; Ajustes (#20) lo absorbe. El cambio es **instantáneo**: todo texto visible muestra el idioma nuevo en el siguiente cuadro. No pausa timers, no envía comandos ni toca el estado de la partida, la selección, las flechas o la pila de deshacer.
12. **Se guardan claves, no textos.** La presentación conserva clave y parámetros y vuelve a pedir el texto al recibir el aviso de cambio. Un aviso en pantalla cambia de idioma y su tiempo restante no se reinicia.
13. **Carga.** Las dos tablas se cargan completas **antes de la primera pantalla**, sin hilos (la plataforma Web no los admite). Si falla una, se usa la otra y se registra. Si fallan las dos, se muestra un mensaje fijo incrustado y no se ofrece partida: «No se pudieron cargar los textos. Recargá la página. / Could not load text. Reload the page.»
14. **Respaldo ante un texto faltante,** en orden: idioma actual → `es` → `sistema.texto_faltante` del idioma actual → cadena vacía. En un build de desarrollo, en lugar de `sistema.texto_faltante` se muestra `⟦clave⟧`; uno publicado nunca muestra la clave. Cada falta se registra una vez por clave e idioma. El respaldo es una red: los tests de F1 impiden publicar con faltantes.
15. **Vocabulario único.** Un concepto tiene un solo término por idioma, el del GDD dueño. Glosario inicial: tramo / wall section · castillo / castle · muro / wall · turno / turn · alcance / range · escudo / shield · vida / health · ataque / attack · cubrir / cover · reclutar / recruit · deshacer / undo.
16. **Estilo.** Causa primero; una frase; vos en español y *you* en inglés; sin signos de exclamación en los rechazos; sin culpar al jugador. Respeta el presupuesto de largo de la clave (F4).

### States and Transitions

| Estado | Entra cuando | Qué responde | Sale a |
|---|---|---|---|
| `SinCargar` | Arranque | Pedir un texto es un error de programación: se registra y devuelve cadena vacía | `Lista` · `Degradada` · `Fallida` |
| `Lista(idioma)` | Cargaron las dos tablas | Textos del idioma actual | `Lista(otro)` al cambiar de idioma |
| `Degradada(idioma)` | Cargó una sola tabla | Textos de esa tabla; el botón ES/EN queda deshabilitado | Solo al reiniciar |
| `Fallida` | No cargó ninguna | Solo el mensaje fijo de la regla 13 | Solo al reiniciar |

Las tablas no se recargan en caliente en un build publicado. Cambiar al idioma actual no emite aviso de cambio. En `Degradada` la preferencia guardada no se toca.

### Interactions with Other Systems

| Sistema | Entra a Localización | Sale de Localización | Dueño de la interfaz |
|---|---|---|---|
| #2 Catálogo | ID de tipo 0–6 | Nombre de pieza, singular y plural | Localización |
| #1 Librería y #3–#11 reglas | Nombres de códigos, eventos, fases y motivos (los lee la presentación, nunca el núcleo) | Texto de cada uno | El GDD de cada regla define el código y si lo ve el jugador; Localización, la clave y el texto |
| #11 Modificador | Las 5 caras | Nombres | #11 define las caras |
| #12 Timer | Modos locales | Nombres de los modos, pedidos por la pantalla | Localización; el Timer no pide textos |
| #13 Entrada | Código de rechazo | Texto del aviso | Entrada decide si, dónde y cuánto se muestra |
| #15, #16, #17, #18 Presentación | Clave y parámetros | Texto; aviso de cambio de idioma | Cada GDD define sus claves `ui` |
| #19 Métricas | Nada | Nada | Guarda códigos, nunca textos traducidos |
| #20 Ajustes (Vertical Slice) | Idioma elegido | Lista de idiomas e idioma actual | Ajustes, la pantalla; Localización, aplicar el idioma |
| #21 Menú (Vertical Slice) | Claves de menú | Texto | Menú |
| #31 Emotes, #34 Chat | Claves de frases predefinidas | Texto en el idioma **de quien lee** | Cada GDD. El texto libre del chat no pasa por las tablas |

El núcleo no referencia Localización en ninguna dirección.

## Formulas

Son chequeos de test, salvo F3, que corre en el juego. Ninguna afecta la partida. Aprobadas por el jugador, 2026-11-25. Los largos se cuentan en caracteres visibles (grafemas): «ñ» es 1.

### F1. Textos faltantes

The textos_faltantes formula is defined as:

`textos_faltantes(L) = |R − T(L)| + |T(L) − R|`

**Variables:**
| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| L | enum | `es`, `en` | constant (regla 2) | Idioma |
| R | conjunto | 1 o más claves | calculated | Claves requeridas: 7 piezas, códigos visibles e internos con texto propio, eventos con texto, motivos, fases, caras, claves `ui` usadas y las de `sistema` |
| T(L) | conjunto | 0 o más claves | data file | Claves de la tabla L con texto completo (en una plural, sus dos formas). Vacío o solo espacios no cuenta |

**Output Range:** 0 en todo build publicable; mayor que 0 rompe el test y nombra las claves. `R` vacío también lo rompe: sin claves requeridas, el 0 sería falso.
**Example:** `R` de 80 claves; `en` tiene 79 de ellas y 1 que sobra → 1 + 1 = 2, falla.

### F2. Diferencia de parámetros

The dif_param formula is defined as:

`dif_param(k) = |P_es(k) △ P_en(k)|`

**Variables:**
| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| k | clave | cualquiera de `R` | data file | Clave revisada |
| P_L(k) | conjunto | 0–4 nombres | data file | Parámetros de `k` en L. En una plural, la unión de sus formas; el parámetro que decide puede faltar en `uno` |

**Output Range:** 0 siempre; otro valor rompe el test. Sin parámetros en ambos: 0.
**Example:** `es` «Te quedan {n} movimientos» y `en` «You have moves left» → 1, falla.

### F3. Forma plural

The forma_plural formula is defined as:

`forma_plural(n) = uno si n = 1; otros en todo otro caso`

**Variables:**
| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| n | int | −2.147.483.648 a 2.147.483.647 | calculated | Valor del parámetro que decide |

**Output Range:** `uno` u `otros`. 0 y −1 dan `otros`. No usa valor absoluto, que desborda con el mínimo.
**Example:** n = 1 → «Te queda 1 movimiento»; n = 0 → «Te quedan 0 movimientos».

### F4. Largo en el peor caso

The largo_peor formula is defined as:

`largo_peor(k, L) = fijos(k, L) + Σ ancho_max(p, L)` y debe cumplir `largo_peor(k, L) ≤ presupuesto(k)`

**Variables:**
| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| fijos(k, L) | int | 0–1.000 | calculated | Caracteres sin los parámetros; en una plural, se evalúa cada forma |
| ancho_max(p, L) | int | 1–1.000 | data file | Entero: los dígitos declarados para el parámetro (1 a 11). Clave: el texto más largo posible en L. Se suma por cada aparición |
| presupuesto(k) | int | 1–1.000 | data file | Botón 18 · chip 18 · etiqueta 24 · efecto 48 · título 32 · aviso 60 · nota 180 · párrafo 240 (chip, efecto y nota los pidió #17) |

**Output Range:** 1 a 1.000; pasarse del presupuesto rompe el test. *Por qué 60 para un aviso:* unos 4 s de lectura a 15 caracteres por segundo, una velocidad **hipotética** a medir en playtest.
**Example:** «Te quedan {n} movimientos»: 22 fijos + 2 dígitos = 24 ≤ 60, pasa.

### F5. Texto inflado de prueba

The largo_pseudo formula is defined as:

`largo_pseudo = largo + ⌈largo × expansion_pct ÷ 100⌉`

**Variables:**
| Variable | Type | Range | Source | Description |
|---|---|---|---|---|
| largo | int | 0–1.000 | calculated | Largo real del texto |
| expansion_pct | int | 0–100 (valor 40) | data file | Inflado; 40 es una reserva propuesta para otro idioma |

**Output Range:** 0 a 2.000. Con largo 0 da 0; con 1 da 2. Solo en builds de desarrollo; no toca los parámetros.
**Example:** un botón de 18 → 18 + ⌈7,2⌉ = 26.

## Edge Cases

**Elección y guardado**

- **If la preferencia guardada no es un idioma soportado o está dañada:** se ignora y sigue la regla 10 (b). No se borra hasta que el jugador elija otro idioma.
- **If el almacenamiento está bloqueado:** el idioma se elige por (b) o (c); el botón funciona durante la sesión y no se guarda. Sin aviso al jugador; se registra una vez.
- **If el sistema informa `es`, `es-AR`, `ES-419` o `es_MX`:** español. La subetiqueta primaria se separa en el primer `-` o `_`.
- **If informa `pt-BR`, una cadena vacía o un idioma como `est`:** inglés.
- **If hay dos pestañas con idiomas distintos:** cada una conserva el suyo; vale lo último guardado para la próxima carga.

**Cambio de idioma**

- **If se cambia con un aviso en pantalla:** el aviso pasa al idioma nuevo y conserva su tiempo restante.
- **If se cambia en el mismo cuadro en que vence un timer:** las dos cosas ocurren; el cambio no retrasa el comando ni lo cancela, y los textos del turno nuevo salen en el idioma nuevo.
- **If se pulsa ES/EN dos veces en un cuadro:** se aplican en orden; queda el idioma original y los textos no parpadean.
- **If se elige el idioma actual:** no hay aviso de cambio. La preferencia se guarda (era automática y pasa a ser elegida).

**Textos y parámetros**

- **If falta la clave en el idioma actual:** regla 14; se registra una vez por clave e idioma.
- **If falta también `sistema.texto_faltante`:** cadena vacía en un build publicado. La interfaz no debe depender del texto para funcionar.
- **If no se pasa un parámetro que el texto pide:** en desarrollo, `⟦nombre⟧`; publicado, se aplica el respaldo `sistema.texto_faltante`. Nunca `{nombre}`.
- **If se pasa un parámetro que el texto no usa:** se ignora y, en desarrollo, se registra.
- **If el valor contiene `{` o `}`:** se muestra tal cual; no se vuelve a sustituir.
- **If el texto tiene una llave sin cerrar:** el test de formato falla; en un build publicado, se muestra literal.
- **If el parámetro que decide el plural no es un entero:** forma `otros`; se registra.
- **If se pide el nombre de un tipo fuera de 0–6:** `sistema.texto_faltante` y registro; nunca una excepción.
- **If un parámetro es una clave que lleva parámetros:** el test lo rechaza; en un build publicado se muestra `sistema.texto_faltante` en su lugar.
- **If un código nuevo del núcleo no tiene texto:** F1 falla antes de publicar. Si llega igual y está descartando una acción, se usa `sistema.aviso_neutro`, no `sistema.texto_faltante`.
- **If dos claves difieren solo en mayúsculas o una está repetida:** el test de formato lo rechaza. Si una repetida llega a producción, vale la primera.

**Carga y glifos**

- **If una tabla está truncada o ilegible:** se descarta entera y se pasa a `Degradada` con la otra; nunca se mezclan tablas parciales.
- **If fallan las dos:** estado `Fallida`.
- **If la única que cargó no es la preferida:** se usa igual y el botón queda deshabilitado.
- **If un carácter no tiene glifo en la fuente:** el test de glifos falla. Publicado, la fuente de respaldo; si tampoco lo tiene, se muestra el símbolo del motor (no se altera el texto).
- **If el texto cabe en el presupuesto pero no en la caja:** la pantalla ajusta a otra línea o achica hasta su mínimo; nunca corta. Cada GDD de pantalla declara su mínimo.

**Sin estrategia degenerada:** cambiar de idioma no da información, tiempo ni ventaja.

## Dependencies

**De quién depende**

| Sistema | Tipo | Interfaz | Estado del GDD |
|---|---|---|---|
| #2 Catálogo de piezas | Blanda | Solo el ID de tipo, 0–6 | Existe; ya lista a #14 |
| #1 Librería de reglas | Blanda (contenido) | Lista de códigos y su marca «para el jugador». El núcleo no se referencia: la lista llega a los tests y a la presentación | Existe; ya lista a #14 |

Blanda significa que el sistema funciona sin esas listas, pero no puede probar que están todos los textos.

**Quién depende de él**

| Sistema | Tipo | Qué usa | Estado |
|---|---|---|---|
| #13 Entrada y selección | Dura | Textos de rechazo y aviso neutro | En diseño, en paralelo |
| #15 HUD, #16 Vista previa, #17 Interfaz del Modificador | Dura | Claves `ui`, piezas, caras; aviso de cambio de idioma | #15 y #17 existen (tanda 3) y definen sus claves; #16 pendiente |
| #18 Render | Blanda | MUERE y ROTO si van como texto | Pendiente |
| #20 Ajustes, #21 Menú | Dura | Lista de idiomas, idioma actual, claves | Pendientes (Vertical Slice) |
| #23 Accesibilidad, #24 Tutorial | Blanda | Textos; espacio para texto escalado | Pendientes |
| #31 Emotes, #34 Chat | Dura | Frases predefinidas | Pendientes (Alpha) |
| #6 Movimiento, #7–#11 reglas | Blanda | Textos de sus códigos visibles | #6, #7, #8, #10, #11 y #13 lo listan; #9 no tiene códigos de rechazo ni textos propios |
| #12 Timer | Blanda | Nombres de los cuatro modos | Existe y lo lista |

**Supuestos provisionales:** claves `ui` de #16, #18 y #21 y qué eventos de Render llevan texto. Las de #15 y #17 y el lugar del botón ES/EN en la partida (#15, regla 21) quedaron fijados en la tanda 3.

## Tuning Knobs

| Perilla | Valor | Rango seguro | Si es muy alta | Si es muy baja | Interacción |
|---|---|---|---|---|---|
| `presupuesto` botón | 18 | 12–26 | Botones anchos que tapan el tablero | Traducciones forzadas | Con `expansion_pct` |
| `presupuesto` etiqueta | 24 | 16–34 | Etiquetas que no entran en la ficha | Abreviaturas | Con `expansion_pct` |
| `presupuesto` título | 32 | 20–45 | Títulos en dos líneas | Títulos vagos | Con `expansion_pct` |
| `presupuesto` chip | 18 | 12–26 | Chips que no entran en la celda de una cara (#17, F3) | Motivos crípticos | Con `expansion_pct` |
| `presupuesto` efecto | 48 | 30–70 | El efecto de la cara no entra en la placa (#17) | Efectos incompletos | Con `expansion_pct` |
| `presupuesto` nota | 180 | 120–260 | Notas del panel «?» que nadie lee | Falta la explicación del reparto | — |
| `presupuesto` aviso | 60 | 40–90 | No se lee dentro del turno | No alcanza para decir la causa | Con la duración del aviso (#13) |
| `presupuesto` párrafo | 240 | 120–400 | Nadie lo lee | Reglas incompletas | — |
| `expansion_pct` | 40 | 20–60 | Fallan pantallas que están bien | No descubre textos cortados | Multiplica el efecto de los presupuestos |
| Idioma si no se reconoce ninguno | `en` | `es`, `en` | — | — | Regla 10 |

Los textos son datos: cambiar una frase no toca código. La lista de idiomas no es una perilla: sumar uno exige tabla, revisión y fuente. La **duración** de los avisos es de #13. No hay perillas que toquen el balance.

## Visual/Audio Requirements

Categoría `Meta`: opcional.

- **Fuente:** la de la art bible §Tipografía (sans condensada, de licencia libre). Debe tener `á é í ó ú ü ñ Á É Í Ó Ú Ü Ñ ¿ ¡ « »` y los caracteres ingleses. La fuente medieval de títulos necesita los mismos glifos si se usa en textos traducidos.
- **Peso:** las dos tablas y la fuente cuentan dentro de los 20 MB de la primera descarga. Meta: tablas ≤ 200 KB en total (propuesta).
- **MUERE y ROTO** llevan texto además de calavera o grieta: en inglés hay que respetar el ancho de la placa. Propuesta: DIES y BROKEN.
- **Audio:** ninguno propio. No hay voces.

## Game Feel

Opcional en `Meta`. Solo una meta: el cambio de idioma se ve en el siguiente cuadro y no supera 50 ms de trabajo en el cuadro en que ocurre (propuesta, a medir). Sin animación ni fundido.

## UI Requirements

- **Botón ES/EN (MVP):** muestra los dos idiomas y marca el activo con forma, no solo color. Está antes de la partida y en la partida; no tapa el tablero ni el timer. Tamaño táctil: el de la pantalla que lo aloja (#13, #15).
- **Cajas de texto:** aceptan el presupuesto de su tipo y el texto inflado de F5 sin cortar.
- **Avisos:** en un solo lugar fijo: la zona de avisos del HUD, banda superior a la derecha del timer (#15, regla 18). **Un término por concepto:** el castillo que no recluta es «Castillo cortado» en todas las pantallas (#10, #17, #5).
- **Deshabilitado (`Degradada`):** el botón se ve deshabilitado y no responde.
- **Ayuda para lectores de pantalla:** todo botón sin texto tiene una clave `ui` con su nombre. Alcance exacto: #23.

## Cross-References

| Esta regla usa | GDD | Parte | Naturaleza |
|---|---|---|---|
| Códigos, no texto; marca «para el jugador»; aviso neutro | `libreria-de-reglas.md` | Regla 4, UI Requirements | Regla |
| IDs 0–6 y nombres sin texto en el Catálogo | `catalogo-de-piezas.md` | Reglas 2 y 9 | Dato |
| Motivos `ReyMuerto`, `DobleRey`, `RelojAgotado`; fases | `maquina-de-turno.md` | Reglas 13 y 14 | Dato |
| Tramo, muro y castillo como términos | `castillo-y-muros.md` | Core Rules | Vocabulario |
| Textos de códigos visibles | `movimiento.md` (en diseño) | Dependencies | Regla |
| Cuatro modos de timer | `timer-de-turno.md` (en diseño) | Core Rules | Dato |
| Idioma desde Ajustes | `systems-index.md` | Decisiones, fila #20 | Regla; **agrega** el botón del MVP |
| Tipografía, MUERE y ROTO | `design/art/art-bible.md` | Tipografía, placas de estado | Restricción |
| Primera descarga < 20 MB | `game-concept.md` | Technical Considerations | Presupuesto |

## Acceptance Criteria

Automáticos (Logic) salvo que se indique. «Tablas de prueba» son fixtures del repositorio; el idioma del sistema y el almacenamiento se inyectan.

- **AC-1 (regla 1)** — **GIVEN** todo el código y los recursos de presentación, **WHEN** un chequeo busca literales asignados a textos visibles, **THEN** solo encuentra las excepciones de la regla 1. **GIVEN** un literal plantado, **THEN** falla y lo nombra.
- **AC-2 (regla 2)** — **GIVEN** una tercera tabla de prueba y su entrada en la lista, **WHEN** se elige, **THEN** todo texto sale de ella sin cambiar código de pantallas.
- **AC-3 (regla 3, F1)** — **GIVEN** las tablas del repositorio, **WHEN** se calcula F1, **THEN** da 0 en `es` y `en`. **GIVEN** una clave quitada, una con solo espacios, una de más y `R` vacío (cada una por separado), **THEN** el test falla.
- **AC-4 (regla 4)** — **GIVEN** claves con espacio, acento, grupo inexistente, repetida o distinta solo en mayúsculas, **WHEN** corre el test de formato, **THEN** rechaza cada una.
- **AC-5 (regla 5, derivación)** — *(Integration con #1)* **GIVEN** los códigos que la Librería marca para el jugador, **WHEN** se pide `codigo.` + nombre en los dos idiomas, **THEN** cada uno da un texto no vacío. **GIVEN** un código visible agregado sin texto, **THEN** F1 falla y lo nombra.
- **AC-6 (regla 5, interno)** — **GIVEN** `ComandoViejo` que descarta una acción, **WHEN** la presentación pide el aviso, **THEN** sale `sistema.aviso_neutro`. **GIVEN** `TimeoutObsoleto`, **THEN** no hay texto.
- **AC-7 (regla 6)** — *(Integration con #2)* **GIVEN** los IDs 0 a 6, **WHEN** se piden en `es`, **THEN** salen Soldado, Arquero, Escudero, Catapulta, Jinete, Demoledor y Rey, con sus plurales; 7 y −1 dan, en un build publicado, `sistema.texto_faltante` y un registro.
- **AC-8 (regla 7, F2)** — **GIVEN** las tablas del repositorio, **THEN** F2 es 0 en toda clave. **GIVEN** el texto `{{A}} {nombre}` y el valor de prueba `{x}`, **WHEN** se arma, **THEN** sale `{A} {x}`. **GIVEN** `{n}` quitado en `en`, **THEN** F2 falla.
- **AC-9 (regla 7, clave como parámetro)** — **GIVEN** «{pieza}: sin objetivo» con `pieza.catapulta`, **WHEN** se pide en `es` y `en`, **THEN** sale «Catapulta: sin objetivo» y el nombre inglés.
- **AC-10 (regla 8, F3)** — **GIVEN** n = 1, 0, −1, 2, −2.147.483.648 y 2.147.483.647, **WHEN** se elige forma, **THEN** `uno` solo con 1 y sin error en ningún caso.
- **AC-11 (regla 9)** — **GIVEN** 1000000 y −5, **WHEN** se muestran, **THEN** «1000000» y «-5» en los dos idiomas.
- **AC-12 (regla 10)** — **GIVEN** sin preferencia y sistema `es`, `es-AR`, `ES-419`, `es_MX`, `pt-BR`, `est`, vacío y error de lectura, **WHEN** arranca, **THEN** los cuatro primeros dan `es` y los demás `en`. **GIVEN** preferencia `en` y sistema `es`, **THEN** `en`. **GIVEN** preferencia `fr` guardada, **THEN** vale el sistema.
- **AC-13 (regla 10, guardado)** — **GIVEN** arranque automático sin tocar el botón, **THEN** no se guardó preferencia. **WHEN** se pulsa ES/EN y se reinicia, **THEN** queda el idioma elegido. **GIVEN** almacenamiento que lanza error, **THEN** el cambio funciona en la sesión y no se cae.
- **AC-14 (regla 11)** — *(Integration)* **GIVEN** una partida con una pieza seleccionada, dos flechas, un movimiento deshacible y el timer en marcha, **WHEN** se cambia el idioma, **THEN** en el cuadro siguiente todo texto visible está en el idioma nuevo, la huella del estado, la selección, las flechas y la pila son iguales y no se envió ningún comando.
- **AC-15 (regla 11, timer)** — *(Integration con #12)* **GIVEN** 30 s restantes y un reloj de prueba, **WHEN** se cambia 20 veces sin avanzar el reloj, **THEN** quedan 30 s. **WHEN** el cambio coincide con el vencimiento, **THEN** se envía un solo `TurnTimeout`.
- **AC-16 (regla 12)** — **GIVEN** un aviso con 2 s restantes, **WHEN** se cambia el idioma, **THEN** cambia su texto y quedan 2 s.
- **AC-17 (regla 13)** — *(Integration)* **GIVEN** tablas sanas, **WHEN** se muestra la primera pantalla, **THEN** ya se puede pedir todo texto. **GIVEN** `en` truncada, **THEN** `Degradada(es)`, botón deshabilitado y la preferencia intacta. **GIVEN** las dos truncadas, **THEN** aparece el mensaje fijo y no hay forma de iniciar partida.
- **AC-18 (regla 14)** — **GIVEN** una clave solo en `es`, **WHEN** se pide en `en`, **THEN** sale el español. **GIVEN** ninguna, **THEN** en publicado `sistema.texto_faltante` y en desarrollo `⟦clave⟧`; pedirla 10 veces deja un solo registro.
- **AC-19 (parámetro faltante)** — **GIVEN** un texto con `{n}` sin valor, **WHEN** se arma en publicado, **THEN** sale `sistema.texto_faltante` y nunca `{n}`.
- **AC-20 (regla 15)** — *(Manual)* **GIVEN** las dos tablas y el glosario, **WHEN** un revisor recorre los textos, **THEN** ningún concepto del glosario aparece con otro término.
- **AC-21 (regla 16 y F4)** — **GIVEN** las tablas del repositorio, **WHEN** se calcula F4, **THEN** toda clave cumple su presupuesto. **GIVEN** un aviso de 60 y otro de 61 caracteres, **THEN** pasa el primero y falla el segundo. **GIVEN** un rechazo con «¡» o «!», **THEN** falla.
- **AC-22 (F5)** — **GIVEN** largos 0, 1, 18 y 1.000 con 40 %, **THEN** 0, 2, 26 y 1.400.
- **AC-23 (glifos)** — **GIVEN** las dos tablas y la fuente del juego, **WHEN** se recorren sus caracteres, **THEN** todos tienen glifo. **GIVEN** uno ausente plantado, **THEN** falla.
- **AC-24 (frontera)** — **GIVEN** la librería de reglas, **WHEN** se revisan sus referencias, **THEN** ninguna apunta a Localización.
- **AC-25 (UI, capturas en `production/qa/evidence/`)** — Cada pantalla del MVP en `es`, en `en` y con texto inflado: ningún texto cortado, encimado, con clave cruda o glifo faltante.
- **AC-26 (rendimiento)** — **GIVEN** la pantalla con más textos en el build web de referencia, **WHEN** se cambia el idioma, **THEN** el cuadro del cambio usa ≤ 50 ms y las dos tablas pesan ≤ 200 KB. *(PROVISIONAL: valores propuestos.)*

**A playtest:** que un jugador nuevo, ante 5 rechazos distintos, explique la causa de al menos 4 sin ayuda; velocidad real de lectura de avisos; revisión por un hablante nativo de inglés y otro de español de fuera del Río de la Plata (por el voseo).

## Open Questions

| Pregunta | Dueño | Para cuándo |
|---|---|---|
| Formato y carga de tablas, almacenamiento de la preferencia y lectura del idioma del navegador | ADR (technical-director) | Antes del primer texto en pantalla |
| **NOT SOURCEABLE:** lectura del idioma, glifos de respaldo y carga sin hilos en la build web no están cubiertos por `docs/engine-reference/unity/` | ADR | Antes de implementar |
| Nombres en inglés de las 7 piezas, MUERE/ROTO y glosario | localization-lead, hablante nativo | Antes de la primera build con textos |
| ¿El voseo molesta a jugadores de otras regiones de Latinoamérica? | Playtest | Primeros playtests del MVP |
| Nombre del juego: ¿igual en los dos idiomas? | creative-director | Cuando deje de ser provisional |
| ~~Textos de las 5 caras y sus recuadros~~ **Resuelta por #17 (regla 13)** | — | — |
| ~~Lugar exacto del botón ES/EN y duración de los avisos~~ **Resuelta por #15 (reglas 18 y 21)** y Entrada (#13) | — | — |
| Qué eventos llevan texto | #15, #18 | En sus GDD |

