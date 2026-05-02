# Discrete Math Toolkit

An interactive desktop application for exploring discrete mathematics: graph algorithms, trees, propositional logic, combinatorics, number systems, and sets & relations. Built with C# / .NET 8 / WPF.

![build](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-106%2F106-brightgreen)
![framework](https://img.shields.io/badge/.NET-8.0-blueviolet)
![platform](https://img.shields.io/badge/platform-Windows-0078d6)
![license](https://img.shields.io/badge/license-Academic-lightgrey)

---

## Table of contents

- [Features](#features)
- [Modules](#modules)
- [Getting started](#getting-started)
- [Project structure](#project-structure)
- [Testing](#testing)
- [Sample data](#sample-data)
- [Keyboard shortcuts](#keyboard-shortcuts)
- [Logs](#logs)
- [Tech stack](#tech-stack)
- [Documentation](#documentation)
- [License](#license)

---

## Features

- Seven interactive modules covering the core topics of an introductory discrete-math course
- Step-by-step animated playback of every graph and tree algorithm
- Full propositional-logic engine: tokenizer, parser, truth tables, and Boolean simplification
- BigInteger combinatorics with an interactive Pascal's triangle (1–16 rows)
- Base conversion (any base 2–36) and Hamming(7,4) error correction with single-bit recovery
- Light and dark themes; load and save graphs as JSON
- 106 unit tests, fully passing, covering every algorithm in `Core`

---

## Modules

The app is divided into seven feature pages, navigated via the sidebar.

### Dashboard

Tile-based landing page; click any tile to jump to that module.

### Graph Theory

Build a graph (nodes, weighted/unweighted edges, directed/undirected). Run **BFS**, **DFS**, **Dijkstra's** shortest paths, **Kruskal's MST**, and **Prim's MST**. Watch each algorithm execute step-by-step with live highlights.

### Trees

Build a BST from a number sequence; insert, delete, and search single values. Run all four traversals (Pre-order, In-order, Post-order, Level-order) with animated playback. Reconstruct trees from Pre+In or Post+In sequences.

### Logic & Truth Tables

Parse propositional formulas using either symbolic operators (`&`, `|`, `!`, `^`, `->`, `<->`) or word forms (`AND`, `OR`, `NOT`, …). Build full truth tables, classify expressions as tautology / contradiction / contingency, and apply Boolean simplification rules.

### Combinatorics

BigInteger calculator: factorial, permutations P(n, k), combinations C(n, k), variations and combinations with repetition. Includes an interactive Pascal's triangle, sized 1–16 rows.

### Number Systems

Convert between any two bases 2–36 with step-by-step explanation. Compute two's complement at 4 / 8 / 16 / 32 / 64 bits. Encode and decode Hamming(7,4) error-correcting codes — automatically detects and corrects single-bit errors.

### Sets & Relations

All standard set operations (∪, ∩, \, △, complement, ×, power set). Relation analyzer that checks reflexivity, symmetry, antisymmetry, and transitivity, identifies equivalence relations and partial orders, and renders a visual relation matrix.

---

## Getting started

### Requirements

- **Windows 10 or 11** (WPF requires Windows)
- **.NET 8 SDK** — [download here](https://dotnet.microsoft.com/download/dotnet/8.0)

### Quick start

```powershell
git clone https://github.com/linabaghunts67-dot/DiscreteMathToolkit
cd DiscreteMathToolkit
dotnet restore
dotnet build
dotnet test
dotnet run --project src/DiscreteMathToolkit.App
```

The first build pulls NuGet packages (CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, Serilog, xUnit, FluentAssertions) and takes ~30 seconds. Incremental builds afterward take 1–2 seconds.

---

## Project structure

The solution is split into five projects following SOLID and clean-architecture principles. See [`docs/architecture.md`](docs/architecture.md) for the deep dive.

```
DiscreteMathToolkit/
├── docs/                  # Architecture & user-guide markdown
├── samples/               # Example graphs, logic expressions, BST inputs
└── src/
    ├── DiscreteMathToolkit.Core/            # Pure algorithms — no UI dependencies
    │   ├── Algorithms/    # Shared step-recording primitive
    │   ├── Combinatorics/ # Factorial, P(n,k), C(n,k), Pascal's triangle
    │   ├── Graphs/        # Graph model + BFS, DFS, Dijkstra, Kruskal, Prim
    │   ├── Logic/         # Tokenizer, parser, truth tables, simplifier
    │   ├── NumberSystems/ # Base conversion, Hamming(7,4)
    │   ├── Sets/          # Set ops, relation analyzer
    │   └── Trees/         # BST, traversals, reconstruction
    │
    ├── DiscreteMathToolkit.Visualization/   # Layout engines & playback control
    │   ├── Layout/        # Circular, force-directed, tree layout
    │   └── Playback/      # Step controller for animated algorithms
    │
    ├── DiscreteMathToolkit.Infrastructure/  # File I/O, logging, persistence
    │   ├── Export/
    │   ├── Logging/       # Serilog wiring
    │   └── Persistence/   # JSON graph repository
    │
    ├── DiscreteMathToolkit.App/             # WPF UI — Views, ViewModels, themes
    │   ├── Mvvm/          # Navigation, base view-model
    │   ├── Services/      # Theme service
    │   ├── Themes/        # Light/dark color palettes, control styles
    │   ├── ViewModels/
    │   └── Views/         # XAML pages + custom GraphCanvas, TreeCanvas
    │
    └── DiscreteMathToolkit.Tests/           # xUnit + FluentAssertions, 106 tests
```

---

## Testing

```powershell
dotnet test
```

Should report `Passed! - Failed: 0, Passed: 106, Skipped: 0, Total: 106`. Coverage by area:

| Area                                                                   |   Tests |
| ---------------------------------------------------------------------- | ------: |
| Graph algorithms (BFS, DFS, Dijkstra, Kruskal, Prim, adjacency matrix) |      13 |
| Trees (BST, traversals, reconstruction)                                |      11 |
| Logic (tokenizer, parser, truth table, simplifier)                     |    22 † |
| Combinatorics                                                          |    30 † |
| Number systems & Hamming(7,4)                                          |    18 † |
| Sets & relations                                                       |      12 |
| **Total**                                                              | **106** |

† Counts include Theory test expansions.

---

## Sample data

The `samples/` folder contains files you can load through the app's **Load…** dialogs to populate the editors with non-trivial examples:

| File                           | Description                                                     |
| ------------------------------ | --------------------------------------------------------------- |
| `graph-konigsberg.json`        | Königsberg's seven bridges — the famous Eulerian counterexample |
| `graph-petersen.json`          | The Petersen graph, useful for testing many graph algorithms    |
| `graph-pipeline-directed.json` | Small directed weighted DAG                                     |
| `graph-weighted-small.json`    | 5-node weighted graph (default)                                 |
| `logic-expressions.txt`        | Tautologies, contradictions, De Morgan's laws                   |
| `relations-examples.txt`       | Equivalence relations, partial orders, counterexamples          |
| `trees-bst-inputs.txt`         | BST insert sequences                                            |

---

## Keyboard shortcuts

Available on the Graph Theory and Trees pages:

| Key     | Action                   |
| ------- | ------------------------ |
| `Space` | Toggle play / pause      |
| `→`     | Step forward             |
| `←`     | Step backward            |
| `R`     | Reset playback to step 0 |

---

## Logs

Logs are written to `%LOCALAPPDATA%\DiscreteMathToolkit\logs\`. They roll daily and are kept for 14 days. The Settings page has an **Open log folder** button.

---

## Tech stack

| Concern              | Choice                                                                                                                               |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| Language             | C# 12, with nullable reference types enabled                                                                                         |
| Runtime              | .NET 8 / WPF                                                                                                                         |
| MVVM                 | [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) — source-generator-based observables and commands |
| Dependency injection | Microsoft.Extensions.DependencyInjection                                                                                             |
| Logging              | Serilog (rolling daily file sink)                                                                                                    |
| Persistence          | System.Text.Json                                                                                                                     |
| Testing              | xUnit + FluentAssertions                                                                                                             |

---

## Documentation

- [Architecture overview](docs/architecture.md) — projects, layering, MVVM, design decisions
- [User guide](docs/user-guide.md) — module-by-module walkthrough with examples

---

## License

University academic project. See course/instructor for usage policy.
