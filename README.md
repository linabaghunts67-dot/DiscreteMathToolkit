# Discrete Math Toolkit

An interactive desktop application for exploring discrete mathematics: graph algorithms, trees, propositional logic, combinatorics, number systems, and sets & relations. Built with C# / .NET 8 / WPF.

![status](https://img.shields.io/badge/build-passing-brightgreen)
![tests](https://img.shields.io/badge/tests-106%2F106-brightgreen)
![framework](https://img.shields.io/badge/.NET-8.0-blueviolet)

## Modules

The app is divided into seven feature pages, navigated via the sidebar:

| Module                   | What you can do                                                                                                                                                                                                                              |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Dashboard**            | Tile-based landing page; click any tile to jump to that module.                                                                                                                                                                              |
| **Graph Theory**         | Build a graph (nodes, weighted/unweighted edges, directed/undirected). Run BFS, DFS, Dijkstra's shortest paths, Kruskal's MST, Prim's MST. Watch each algorithm execute step-by-step with live highlights.                                   |
| **Trees**                | Build a BST from a number sequence; insert, delete, and search single values. Run all four traversals (Pre-order, In-order, Post-order, Level-order) with animated playback. Reconstruct trees from Pre+In or Post+In sequences.             |
| **Logic & Truth Tables** | Parse propositional formulas (with both symbolic `&`,`                                                                                                                                                                                       | `,`!`,`^`,`->`,`<->`and word`AND`,`OR`,`NOT`,...). Build full truth tables, classify expressions as tautology / contradiction / contingency, and apply Boolean simplification rules. |
| **Combinatorics**        | BigInteger calculator: factorial, permutations P(n,k), combinations C(n,k), variations and combinations with repetition. Interactive Pascal's triangle, sized 1–16 rows.                                                                     |
| **Number Systems**       | Convert between any two bases 2–36 with step-by-step explanation. Compute two's complement at 4/8/16/32/64 bits. Encode and decode Hamming(7,4) error-correcting codes — automatically detects and corrects single-bit errors.               |
| **Sets & Relations**     | All standard set operations (∪, ∩, \, △, complement, ×, power set). Relation analyzer that checks reflexivity, symmetry, antisymmetry, transitivity, and identifies equivalence relations and partial orders, with a visual relation matrix. |

## Architecture

The solution is split into five projects following SOLID and clean architecture principles. See [`docs/architecture.md`](docs/architecture.md) for the deep dive.

```
DiscreteMathToolkit/
├── .gitignore
├── Directory.Build.props
├── DiscreteMathToolkit.sln
├── README.md
├── global.json
├── docs/
│   ├── architecture.md
│   └── user-guide.md
├── samples/
│   ├── graph-konigsberg.json
│   ├── graph-petersen.json
│   ├── graph-pipeline-directed.json
│   ├── graph-weighted-small.json
│   ├── logic-expressions.txt
│   ├── relations-examples.txt
│   └── trees-bst-inputs.txt
└── src/
    ├── DiscreteMathToolkit.Core/
    │   ├── DiscreteMathToolkit.Core.csproj
    │   ├── Algorithms/
    │   │   └── AlgorithmStep.cs
    │   ├── Combinatorics/
    │   │   └── CombinatoricsCalculator.cs
    │   ├── Common/
    │   │   └── Guard.cs
    │   ├── Graphs/
    │   │   ├── Graph.cs
    │   │   ├── GraphNode.cs
    │   │   └── Algorithms/
    │   │       ├── BreadthFirstSearch.cs
    │   │       ├── DepthFirstSearch.cs
    │   │       ├── Dijkstra.cs
    │   │       ├── Kruskal.cs
    │   │       └── Prim.cs
    │   ├── Logic/
    │   │   ├── BooleanSimplifier.cs
    │   │   ├── LogicNode.cs
    │   │   ├── LogicParser.cs
    │   │   ├── LogicTokenizer.cs
    │   │   ├── Token.cs
    │   │   └── TruthTable.cs
    │   ├── NumberSystems/
    │   │   ├── BaseConverter.cs
    │   │   └── ErrorCorrectingCodes.cs
    │   ├── Sets/
    │   │   ├── RelationAnalyzer.cs
    │   │   └── SetOperations.cs
    │   └── Trees/
    │       ├── BinaryTree.cs
    │       ├── TreeReconstruction.cs
    │       └── TreeTraversals.cs
    ├── DiscreteMathToolkit.Visualization/
    │   ├── DiscreteMathToolkit.Visualization.csproj
    │   ├── Layout/
    │   │   ├── CircularLayoutEngine.cs
    │   │   ├── ForceDirectedLayoutEngine.cs
    │   │   ├── GraphLayout.cs
    │   │   └── TreeLayoutEngine.cs
    │   └── Playback/
    │       └── StepController.cs
    ├── DiscreteMathToolkit.Infrastructure/
    │   ├── DiscreteMathToolkit.Infrastructure.csproj
    │   ├── Export/
    │   │   └── FileExportService.cs
    │   ├── Logging/
    │   │   └── AppLogger.cs
    │   └── Persistence/
    │       └── JsonGraphRepository.cs
    ├── DiscreteMathToolkit.App/
    │   ├── DiscreteMathToolkit.App.csproj
    │   ├── App.xaml
    │   ├── App.xaml.cs
    │   ├── MainWindow.xaml
    │   ├── MainWindow.xaml.cs
    │   ├── Converters/
    │   │   └── Converters.cs
    │   ├── Mvvm/
    │   │   ├── NavigationService.cs
    │   │   └── ViewModelBase.cs
    │   ├── Services/
    │   │   └── ThemeService.cs
    │   ├── Themes/
    │   │   ├── ColorsDark.xaml
    │   │   ├── ColorsLight.xaml
    │   │   └── ControlStyles.xaml
    │   ├── ViewModels/
    │   │   ├── DashboardViewModel.cs
    │   │   ├── MainViewModel.cs
    │   │   └── Pages/
    │   │       ├── CombinatoricsViewModel.cs
    │   │       ├── GraphAlgorithmAdapters.cs
    │   │       ├── GraphRenderState.cs
    │   │       ├── GraphTheoryViewModel.cs
    │   │       ├── LogicViewModel.cs
    │   │       ├── NumberSystemsViewModel.cs
    │   │       ├── PlaceholderPageViewModel.cs
    │   │       ├── SetsRelationsViewModel.cs
    │   │       ├── SettingsViewModel.cs
    │   │       ├── TreeRenderState.cs
    │   │       └── TreesViewModel.cs
    │   └── Views/
    │       ├── CombinatoricsView.xaml
    │       ├── CombinatoricsView.xaml.cs
    │       ├── DashboardView.xaml
    │       ├── DashboardView.xaml.cs
    │       ├── GraphTheoryView.xaml
    │       ├── GraphTheoryView.xaml.cs
    │       ├── LogicView.xaml
    │       ├── LogicView.xaml.cs
    │       ├── NumberSystemsView.xaml
    │       ├── NumberSystemsView.xaml.cs
    │       ├── PlaceholderView.xaml
    │       ├── PlaceholderView.xaml.cs
    │       ├── SetsRelationsView.xaml
    │       ├── SetsRelationsView.xaml.cs
    │       ├── SettingsView.xaml
    │       ├── SettingsView.xaml.cs
    │       ├── TreesView.xaml
    │       ├── TreesView.xaml.cs
    │       └── Controls/
    │           ├── GraphCanvas.cs
    │           └── TreeCanvas.cs
    └── DiscreteMathToolkit.Tests/
        ├── DiscreteMathToolkit.Tests.csproj
        ├── Combinatorics/
        │   └── CombinatoricsCalculatorTests.cs
        ├── Graphs/
        │   ├── BfsTests.cs
        │   ├── DfsTests.cs
        │   ├── DijkstraTests.cs
        │   ├── GraphTests.cs
        │   ├── KruskalTests.cs
        │   └── PrimTests.cs
        ├── Logic/
        │   ├── BooleanSimplifierTests.cs
        │   ├── LogicParserTests.cs
        │   ├── LogicTokenizerTests.cs
        │   └── TruthTableTests.cs
        ├── NumberSystems/
        │   ├── BaseConverterTests.cs
        │   └── HammingTests.cs
        ├── Sets/
        │   ├── RelationAnalyzerTests.cs
        │   └── SetOperationsTests.cs
        └── Trees/
            ├── BinaryTreeTests.cs
            ├── TreeReconstructionTests.cs
            └── TreeTraversalsTests.cs
```

## Build & run

### Requirements

- **Windows 10/11** (WPF requires Windows)
- **.NET 8 SDK** (download from <https://dotnet.microsoft.com/download/dotnet/8.0>)

### Quick start

```powershell
git clone <https://github.com/linabaghunts67-dot/DiscreteMathToolkit>
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

| Area                                                                   |   Tests |
| ---------------------------------------------------------------------- | ------: |
| Graph algorithms (BFS, DFS, Dijkstra, Kruskal, Prim, adjacency matrix) |      13 |
| Trees (BST, traversals, reconstruction)                                |      11 |
| Logic (tokenizer, parser, truth table, simplifier)                     |    22\* |
| Combinatorics                                                          |    30\* |
| Number systems & Hamming(7,4)                                          |    18\* |
| Sets & relations                                                       |      12 |
| **Total**                                                              | **106** |

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

| Key     | Action                   |
| ------- | ------------------------ |
| `Space` | Toggle play/pause        |
| `→`     | Step forward             |
| `←`     | Step backward            |
| `R`     | Reset playback to step 0 |

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
