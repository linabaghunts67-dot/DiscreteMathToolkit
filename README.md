# Discrete Math Toolkit

An interactive desktop application for exploring discrete mathematics: graph algorithms, trees, propositional logic, combinatorics, number systems, and sets & relations. Built with C# / .NET 8 / WPF.

![status](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-106%2F106-brightgreen)
![framework](https://img.shields.io/badge/.NET-8.0-blueviolet)

## Modules

The app is divided into seven feature pages, navigated via the sidebar:

| Module | What you can do |
|---|---|
| **Dashboard** | Tile-based landing page; click any tile to jump to that module. |
| **Graph Theory** | Build a graph (nodes, weighted/unweighted edges, directed/undirected). Run BFS, DFS, Dijkstra's shortest paths, Kruskal's MST, Prim's MST. Watch each algorithm execute step-by-step with live highlights. |
| **Trees** | Build a BST from a number sequence; insert, delete, and search single values. Run all four traversals (Pre-order, In-order, Post-order, Level-order) with animated playback. Reconstruct trees from Pre+In or Post+In sequences. |
| **Logic & Truth Tables** | Parse propositional formulas (with both symbolic `&`,`|`,`!`,`^`,`->`,`<->` and word `AND`,`OR`,`NOT`,...). Build full truth tables, classify expressions as tautology / contradiction / contingency, and apply Boolean simplification rules. |
| **Combinatorics** | BigInteger calculator: factorial, permutations P(n,k), combinations C(n,k), variations and combinations with repetition. Interactive Pascal's triangle, sized 1–16 rows. |
| **Number Systems** | Convert between any two bases 2–36 with step-by-step explanation. Compute two's complement at 4/8/16/32/64 bits. Encode and decode Hamming(7,4) error-correcting codes — automatically detects and corrects single-bit errors. |
| **Sets & Relations** | All standard set operations (∪, ∩, \, △, complement, ×, power set). Relation analyzer that checks reflexivity, symmetry, antisymmetry, transitivity, and identifies equivalence relations and partial orders, with a visual relation matrix. |

## Architecture

The solution is split into five projects following SOLID and clean architecture principles. See [`docs/architecture.md`](docs/architecture.md) for the deep dive.

```
DiscreteMathToolkit/
├── src/
│   ├── DiscreteMathToolkit.Core/             pure algorithms; no UI dependencies (net8.0)
│   ├── DiscreteMathToolkit.Visualization/    layout engines, playback abstraction (net8.0)
│   ├── DiscreteMathToolkit.Infrastructure/   logging, JSON persistence, exporters (net8.0)
│   ├── DiscreteMathToolkit.App/              WPF UI, MVVM, dependency injection (net8.0-windows)
│   └── DiscreteMathToolkit.Tests/            xUnit + FluentAssertions, 106 tests (net8.0)
├── samples/                                   sample graph JSON, logic expressions, tree inputs, relation examples
├── docs/                                      architecture, user guide
└── DiscreteMathToolkit.sln
```

## Build & run

### Requirements
- **Windows 10/11** (WPF requires Windows)
- **.NET 8 SDK** (download from <https://dotnet.microsoft.com/download/dotnet/8.0>)

### Quick start
```powershell
git clone <repo-url>
cd DiscreteMathToolkit
dotnet restore
dotnet build
dotnet test
dotnet run --project src/DiscreteMathToolkit.App
```

The first build will pull NuGet packages (CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Serilog, xUnit, FluentAssertions). Expect ~30 seconds for the first build, and 1-2 seconds for incremental builds afterward.

## Testing

```powershell
dotnet test
```

Should report **`Passed!  -  Failed: 0, Passed: 106, Skipped: 0, Total: 106`**. The test project covers every algorithm in `Core`:

| Area | Tests |
|---|---:|
| Graph algorithms (BFS, DFS, Dijkstra, Kruskal, Prim, adjacency matrix) | 13 |
| Trees (BST, traversals, reconstruction) | 11 |
| Logic (tokenizer, parser, truth table, simplifier) | 22* |
| Combinatorics | 30* |
| Number systems & Hamming(7,4) | 18* |
| Sets & relations | 12 |
| **Total** | **106** |

(\* counts include Theory test expansions)

## Sample data

The `samples/` folder contains files you can load through the app's Load… dialogs to populate the editors with non-trivial examples:

- `graph-konigsberg.json` — Königsberg's seven bridges, the famous Eulerian counterexample
- `graph-petersen.json` — the Petersen graph, useful for testing many graph algorithms
- `graph-pipeline-directed.json` — small directed weighted DAG
- `graph-weighted-small.json` — 5-node weighted graph used as default
- `logic-expressions.txt` — tautologies, contradictions, De Morgan's laws
- `relations-examples.txt` — equivalence relations, partial orders, and counterexamples
- `trees-bst-inputs.txt` — BST insert sequences

## Logs

Logs are written to:
- **Windows:** `%LOCALAPPDATA%\DiscreteMathToolkit\logs\`

Logs roll daily and are kept for 14 days. The Settings page has an "Open log folder" button.

## Keyboard shortcuts

On the Graph Theory and Trees pages:

| Key | Action |
|---|---|
| `Space` | Toggle play/pause |
| `→` | Step forward |
| `←` | Step backward |
| `R` | Reset playback to step 0 |

## Tech stack

- **Language:** C# 12, with nullable reference types enabled
- **Runtime:** .NET 8 / WPF
- **MVVM:** [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) — source-generator-based observables and commands
- **DI:** Microsoft.Extensions.DependencyInjection
- **Logging:** Serilog (rolling daily file sink)
- **Persistence:** System.Text.Json
- **Testing:** xUnit + FluentAssertions

## License

University academic project. See course/instructor for usage policy.

## Documentation

- [Architecture overview](docs/architecture.md) — projects, layering, MVVM, design decisions
- [User guide](docs/user-guide.md) — module-by-module walkthrough with examples
