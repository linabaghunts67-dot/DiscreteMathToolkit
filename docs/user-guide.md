# User Guide

A walkthrough of every module with worked examples. If you've built and run the app (`dotnet run --project src/DiscreteMathToolkit.App`), you're ready.

## Layout

The app has three main regions:

- **Sidebar (left)** — the eight modules. Click to switch.
- **Content area (center)** — the active module's interactive page.
- **Status bar (bottom)** — shows the most recent result or error.

The bottom of the sidebar has a **Toggle theme** button that switches between dark and light modes instantly.

---

## 1. Dashboard

Tile-based landing page. Each tile shows a short description; click any tile to open that module. The dashboard is mainly for orientation — power users will use the sidebar directly.

---

## 2. Graph Theory

The most feature-rich page. The center has a graph canvas and a step explanation strip; the right column has four panels: Algorithm, Playback, Editor, Graph File.

### Quick example: Dijkstra on a small weighted graph

The page starts with a 5-node weighted graph (`A` through `E`) loaded.

1. In the **Algorithm** panel, choose `Dijkstra`.
2. Set **Start node** to `A` (or `0`).
3. Click **Run algorithm**.
4. The status bar will read something like *"Loaded 12 step(s) for Dijkstra. Use Next or Play to step through."*
5. Press the **▶** button (or hit `Space`) to start animated playback. Distances appear under each node — `0` for the start, `∞` for unreached, the running shortest distance otherwise.
6. Use **◀ / ▶▶ / ⟲** to step backward, step forward, or reset to step 0.
7. Drag the **Speed** slider to make playback faster or slower.

### Editor

- **Add node:** type a label (or leave blank for an auto-numeric label) and click **+**.
- **Add edge:** type the from-node and to-node (by label or numeric id), the weight, and click **+**. Weight is ignored if `Weighted` is unchecked.
- **Directed / Weighted** toggles change graph type. Toggling rebuilds the graph from existing edges.
- **Sample** loads the default 5-node weighted graph. **Clear** empties the graph.

### Graph File

- **Load…** opens a JSON file (see `samples/` for examples).
- **Save…** writes a JSON file. The format includes node labels, positions, edges, weights, and the `directed`/`weighted` flags.
- **Export result…** dumps the current algorithm's result as CSV, Markdown, or HTML. Each algorithm has its own column layout (e.g. Dijkstra has Node / Distance / Previous / Path; Kruskal has From / To / Weight; etc.).

### Algorithm-specific behaviors

- **BFS:** node annotations show distance from start (`d=2`).
- **DFS:** node annotations show visit order (`#3` = third visited).
- **Dijkstra:** node annotations show running shortest distance, including `∞` for unreached.
- **Kruskal:** edges flash blue when considered, turn green when accepted into MST, dashed red when rejected (would form a cycle). Components are colored.
- **Prim:** like Kruskal but grows from a starting node — only edges incident to the current tree are considered.

---

## 3. Trees

Two main capabilities: BST manipulation with animated traversals, and tree reconstruction from sequence pairs.

### Building a BST

Either:
- Type a sequence in **Build BST from sequence** (e.g. `4 2 6 1 3 5 7`) and click **Build BST**, or
- Insert one value at a time via the **Insert single value** field.

The tree is laid out top-down with in-order positioning, so the order on the canvas matches what you'd see on a whiteboard.

### Animated traversals

1. Pick a traversal in the **Traversal** panel — `Pre`, `In`, `Post`, or `Level` order.
2. Click **Run traversal**. The result string ("Pre-order sequence: 4, 2, 1, 3, 6, 5, 7") shows up.
3. Press play. Each node lights up in its visit order; small badges show its visit number.

### Reconstruction

The **Reconstruct** card has three text boxes (Pre-order, In-order, Post-order) and two buttons:

- **From Pre + In** uses the classic algorithm: the head of pre-order is the root; locate the root in in-order to split into left/right subtrees; recurse.
- **From Post + In** does the same but the root is the *last* element of post-order.

If the sequences don't form a valid pair (e.g. different sets of values, or in-order isn't a permutation of pre-order), you get a parse error in the status bar.

### Search and Delete

The **Search / delete** field on the Tree Editor uses a single value:
- The 🔍 button reports whether the value exists.
- The ✕ button deletes it (using Hibbard deletion: replace with in-order successor for two-child case).

---

## 4. Logic & Truth Tables

Type a propositional formula in the expression box. Operators accepted (mix and match):

| Symbol | Word form | Meaning |
|---|---|---|
| `!`, `~`, `¬` | `NOT` | Negation |
| `&`, `&&`, `∧` | `AND` | Conjunction |
| `|`, `||`, `∨` | `OR` | Disjunction |
| `^`, `⊕` | `XOR` | Exclusive or |
| `->`, `=>`, `→` | `IMPLIES` | Material conditional |
| `<->`, `<=>`, `↔`, `≡` | `IFF`, `EQUIV` | Biconditional |

Variables are letters or letter-digit identifiers (`p`, `q`, `r`, `q1`, `result_2`).

### Worked examples (load these from the **Samples** panel)

- `p OR !p` → tautology (always true)
- `p AND !p` → contradiction (always false)
- `!(p AND q) <-> (!p OR !q)` → De Morgan's first law (tautology)
- `(p -> q) <-> (!p OR q)` → conditional rewriting (tautology)

### Build table

Click **Build table**. The right side shows the parse tree for verification, and the truth table appears with one row per assignment of values to variables. The `⊨` column is the formula's value. Below the table, the classification reads as **Tautology**, **Contradiction**, or **Contingency** (with the count of true rows for contingencies).

### Simplify

Click **Simplify** to apply algebraic rewrites: identity (`x AND 1 = x`), domination (`x OR 1 = 1`), idempotence (`x OR x = x`), double negation, complementation. The result appears in the **Simplified form** card.

The simplifier is **not** a full SAT/QBF solver — it applies a fixed set of rules to a fixpoint. For most class examples, that's plenty.

### Export truth table

The **Export truth table…** button writes the table as CSV, Markdown, or HTML.

---

## 5. Combinatorics

Two halves: a calculator on the left, Pascal's triangle on the right.

### Calculator

Enter `n` and `k`, then click **Compute**. You get five values:

| Formula | Description |
|---|---|
| `n!` | Factorial |
| `P(n, k) = n! / (n-k)!` | Permutations |
| `C(n, k) = n! / k!(n-k)!` | Combinations |
| `n^k` | Variations with repetition (sequences of length k from an alphabet of size n) |
| `C(n+k-1, k)` | Combinations with repetition (multisets of size k from n options) |

All computed via `BigInteger`, so `n=200` works fine and produces, e.g., `Factorial(200) ≈ 7.88 × 10^374`.

### Pascal's triangle

Drag the **Rows** slider 1–16 and click **Build**. Each row's edges are 1; each interior cell is the sum of the two above. Useful for showing students that row `n` sum equals `2^n`, and that the entries are binomial coefficients `C(n, k)`.

**Export Pascal's triangle…** writes the table as a CSV/Markdown/HTML grid (with empty cells where the triangle has no value).

---

## 6. Number Systems

Three independent cards.

### Base conversion (any base 2–36)

Enter a value, pick the source base and target base, click **Convert**. Below the output, you get a step-by-step explanation:

For input `255` from base 10 to base 16:
- Step 1: parse "255" as base 10 → decimal value 255
- Step 2: 255 ÷ 16 = 15 remainder 15 (digit `F`)
- Step 3: 15 ÷ 16 = 0 remainder 15 (digit `F`)
- Step 4: read remainders bottom-up → `FF`

This makes the algorithm transparent to students — no black box.

### Two's complement

Enter a signed decimal value and pick a bit width (4, 8, 16, 32, or 64). The output is the bit string. For example, `-1` in 8 bits → `11111111`; `-128` in 8 bits → `10000000`. Out-of-range values give a clear error message.

### Hamming(7,4)

Two interactive halves: an encoder and a decoder.

**Encoder** takes 4 data bits (`1 0 1 1`) and produces 7 encoded bits with three parity bits inserted at positions 1, 2, and 4. The **explanation steps** show how each parity bit is computed.

**Decoder** takes 7 received bits, computes the syndrome from the three parity checks, and:
- If syndrome is `000`, no error detected.
- Otherwise the syndrome equals the position of the corrupted bit (1–7); the decoder flips it and reports the correction.

Try it: encode `1 0 1 1`, copy the output to the decoder, flip one bit, decode. The decoder reports which bit was corrected.

---

## 7. Sets & Relations

Two cards.

### Set operations

Enter sets `A`, `B`, and an optional `U` (universe, used only for complement). Sets accept any whitespace-separated or comma-separated integers, with optional `{ }` decorators. Click **Compute**.

Outputs:
- `A ∪ B` (union)
- `A ∩ B` (intersection)
- `A \ B` (difference)
- `A △ B` (symmetric difference)
- `U \ A` (complement of A in U) — error if `A` isn't a subset of `U`
- `A × B` (Cartesian product) — list of pairs
- `P(A)` (power set) — list of subsets, capped at `|A| ≤ 20`

### Relation analyzer

Enter:
- A **base set** (e.g. `1, 2, 3`)
- **Relation pairs** in any of these formats: `(1,2),(2,3)` or `1 2, 2 3` or even `1,2,2,3`

Click **Analyze**. The properties card lists yes/no for: reflexive, irreflexive, symmetric, antisymmetric, transitive, equivalence, partial order. The verdict line summarizes ("This is an equivalence relation.").

The **Failures** list shows specific counterexamples — e.g., "Transitivity fails: (1,2) and (2,3) are in R but (1,3) is not." This is invaluable for teaching, where seeing *why* a relation fails a property is more useful than just *that* it fails.

The **Relation matrix** card on the right shows the boolean matrix. Row `i`, column `j` is `1` if `(i, j)` is in the relation.

---

## 8. Settings

The Settings page lets you:

- Switch theme (Dark / Light)
- Open the log folder in Windows Explorer
- See the current app version and a list of keyboard shortcuts

Logs are written daily to `%LOCALAPPDATA%\DiscreteMathToolkit\logs\` and kept for 14 days.

---

## Keyboard shortcuts (Graph Theory and Trees pages)

| Key | Action |
|---|---|
| `Space` | Toggle play/pause |
| `→` | Step forward |
| `←` | Step backward |
| `R` | Reset playback to step 0 |

The shortcuts only fire when no text input has focus — so typing in the Add Node textbox doesn't accidentally start playback.

---

## Common gotchas

- **Graph is empty / no algorithm runs.** Make sure your start-node label or id matches an existing node.
- **"Could not parse insert sequence."** The Trees page expects integers separated by spaces or commas. `4 2 6` works; `4, 2, 6` works; `four two six` doesn't.
- **"Hamming(7,4) requires exactly 7 received bits."** The decoder is strict — count your bits.
- **Power set didn't appear.** If `|A| > 20`, the Sets module refuses to compute the power set (it would have over a million subsets).
- **Theme didn't switch.** Try clicking elsewhere first to lose focus, then click Toggle theme. (If you find a reproducible case where it doesn't switch, that's a bug — the logs in `%LOCALAPPDATA%\DiscreteMathToolkit\logs\` will help diagnose.)
