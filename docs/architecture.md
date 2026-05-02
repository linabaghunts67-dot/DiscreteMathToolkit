# Architecture

## Overview

The Discrete Math Toolkit is a five-project WPF/.NET 8 solution structured to separate concerns cleanly. The Core algorithms are isolated from any UI framework and depend only on the .NET BCL; everything UI-specific lives in the App project.

```
┌──────────────────────────────────────────────────────────────────┐
│                      DiscreteMathToolkit.App                     │
│                            (WPF UI)                              │
│  ┌────────────┐  ┌──────────────┐  ┌─────────────────────────┐  │
│  │   Views    │  │  ViewModels  │  │    Services / Mvvm      │  │
│  │   (XAML)   │◄─│ (MVVM via    │─►│  Theme, Navigation      │  │
│  │            │  │  CommToolkit)│  │                         │  │
│  └────────────┘  └──────┬───────┘  └─────────────────────────┘  │
└────────────────────────┬┼──────────────────────────────────────┘
                         ││
                         ▼▼
┌──────────────────────────────────┐  ┌─────────────────────────────────┐
│  DiscreteMathToolkit.Visualization│  │  DiscreteMathToolkit.Infrastructure│
│  (pure algorithms, no UI)        │  │  (cross-cutting concerns)       │
│                                  │  │                                 │
│  Layout/                         │  │  Logging/    Persistence/       │
│   - Graph layouts (force-dir,    │  │   Serilog     JSON graph repo   │
│     circular)                    │  │                                 │
│   - Tree layout                  │  │  Export/                        │
│  Playback/                       │  │   CSV, Markdown, HTML           │
│   - StepController               │  │                                 │
└──────────────┬───────────────────┘  └────────────────┬────────────────┘
               │                                       │
               └───────────────────┬───────────────────┘
                                   ▼
                  ┌─────────────────────────────────────┐
                  │       DiscreteMathToolkit.Core      │
                  │     (the heart — pure algorithms)   │
                  │                                     │
                  │  Graphs/   Trees/   Logic/          │
                  │  Combinatorics/  NumberSystems/     │
                  │  Sets/  Algorithms/  Common/        │
                  └─────────────────────────────────────┘
                                   ▲
                                   │  references
                  ┌─────────────────────────────────────┐
                  │      DiscreteMathToolkit.Tests      │
                  │       (xUnit, 106 tests, all pass)  │
                  └─────────────────────────────────────┘
```

## Layering rules

1. **Core** depends only on `System.*` and `System.Numerics`. No UI, no I/O, no third-party libraries. This keeps it portable, fast to test, and re-usable.
2. **Visualization** depends only on Core. It contains pure data computations (layout positions, playback state machine) that produce values, never side effects. Originally targeted `net8.0-windows` with `UseWPF=true`, but during Gen 2 we removed those because no WPF types are actually needed — the project is now plain `net8.0`, which means it's verifiable on Linux CI/build machines too.
3. **Infrastructure** depends on Core. It provides cross-cutting capabilities (logging, file persistence, exporters) behind interfaces, so the App layer can mock or swap them.
4. **App** depends on all three above. It has all the WPF/Windows-specific code: XAML views, dispatcher timers, file dialogs, theme dictionaries.
5. **Tests** depend on Core only. We don't unit-test view models because they are mostly orchestration over Core; manual testing of the UI is sufficient at this scale.

## Core: shared step-recording pattern

Every "visualizable" algorithm follows the same shape:

```csharp
public sealed class AlgorithmStep<TState> {
    public int Index { get; }
    public string Description { get; }
    public TState State { get; }
    public IReadOnlyCollection<int> HighlightedNodes { get; }
    public IReadOnlyCollection<(int From, int To)> HighlightedEdges { get; }
}

public sealed class AlgorithmTrace<TState, TResult> {
    public IReadOnlyList<AlgorithmStep<TState>> Steps { get; }
    public TResult Result { get; }
    public string TimeComplexity { get; }
    public string SpaceComplexity { get; }
}
```

Each algorithm builds an `AlgorithmTrace`. The view model walks the steps forwards/backwards via a `DispatcherTimer` for animated playback, and the View renders the current step. Because steps are immutable snapshots, you can scrub forward and backward with no reverse-execution logic.

This pattern was a deliberate design choice during Gen 1 — without it, every algorithm would need its own ad-hoc playback infrastructure, and a single `StepController` could not be shared.

## App layer: ViewModel-first navigation with WPF DataTemplates

The App project uses **classic ViewModel-first navigation**: the `MainViewModel` exposes a `CurrentPage` of type `IPageViewModel`, and the `MainWindow.xaml` binds it to a `ContentControl`:

```xml
<ContentControl Content="{Binding CurrentPage}" />
```

In `App.xaml` we register a `DataTemplate` for each ViewModel type:

```xml
<DataTemplate DataType="{x:Type vmp:GraphTheoryViewModel}">
    <v:GraphTheoryView />
</DataTemplate>
```

This means when you change `CurrentPage`, WPF automatically resolves the matching View — no view locator, no string keys, no service-locator anti-pattern.

Pages are registered as **singletons** in the DI container, so each module preserves its editing state across navigations (e.g. your half-built graph survives a trip to the Logic page and back).

## Adapter pattern for graph algorithms

The Graph Theory page can run any of five algorithms (BFS, DFS, Dijkstra, Kruskal, Prim) but the `GraphCanvas` only knows one render state shape. The `GraphAlgorithmAdapter<TState, TResult>` abstract class normalizes the differences:

- `BfsAdapter` translates `BfsState.Visited`, `BfsState.Queue`, `BfsState.Current` → `Visited`, `Frontier`, `Current` node badges
- `DijkstraAdapter` translates `DijkstraState.Settled`, `Frontier`, `Distance` → `Settled` badge plus distance annotations like `"d=3"` or `"∞"`
- `KruskalAdapter` and `PrimAdapter` decorate edges as `Tree`, `Active`, or `Rejected`

The view never knows which algorithm is running. This makes it trivial to add new algorithms in the future (Bellman-Ford, A*, Floyd-Warshall — each would just be a new adapter and a new entry in the `GraphAlgorithmKind` enum).

## MVVM with CommunityToolkit.Mvvm

We use [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) v8.x for source-generator–based observability, which means:

```csharp
[ObservableProperty] private string _expression = "p AND q";
```

generates a public property `Expression` with `INotifyPropertyChanged` plumbing — no manual backing field boilerplate, no risk of typos in property names. Optional `partial void OnExpressionChanged(string value)` hooks let you react to changes.

This source-generator approach was chosen over hand-written MVVM because it cuts ViewModel code by 50–70% and eliminates a whole class of subtle bugs (forgetting to raise `PropertyChanged`, mistyping property names in `RaisePropertyChanged("foo")`, etc.).

## Theming

Themes are XAML `ResourceDictionary` files — `Themes/ColorsDark.xaml` and `Themes/ColorsLight.xaml` — that define the same set of brush keys (`SurfaceBaseBrush`, `AccentPrimaryBrush`, etc.). The `ThemeService` swaps the active dictionary at runtime:

```csharp
public void Apply(AppTheme theme) {
    var dictionaries = Application.Current.Resources.MergedDictionaries;
    // remove old theme dictionary
    // insert new one
}
```

All controls reference these brushes via `DynamicResource` so they update live when the theme changes — no need to restart the app or refresh views.

## Testing strategy

The Core algorithms are tested with xUnit and FluentAssertions. Coverage focuses on:

1. **Known-correct examples** — `Combinations(52, 5) == 2_598_960`, `Factorial(10) == 3_628_800`, `EncodeHamming74([1,0,1,1])` round-trips through `Decode`.
2. **Mathematical invariants** — Pascal's triangle row sums equal powers of two; in-order traversal of any BST yields a sorted sequence.
3. **Error behavior** — `Combinations(3, 5)` throws; `BaseConverter.Convert("9", 8, 10)` throws (digit out of range); `RelationAnalyzer.Analyze` rejects pairs outside the base set.
4. **Edge cases** — empty graphs, single-node graphs, BSTs after every node is deleted.

We don't unit-test the WPF views or view models. Their job is orchestration, and their correctness is best verified by running the app.

## Why no PDF export?

Originally planned. The decision to ship CSV / Markdown / HTML only came down to keeping the dependency graph minimal: a PDF library would have added 10+ MB of native binaries to the deployment and locked us into a particular vendor. The HTML exporter generates fully styled, self-contained HTML that you can print to PDF from any browser — same outcome, no dependency.

## Performance notes

- `BigInteger` is used everywhere combinatorics could overflow `long`. `Factorial(200)` is no problem.
- The force-directed graph layout runs `O(V² · iterations)` repulsive force calculation, with `iterations=200`. That's fine up to ~50 nodes — enough for any realistic teaching example.
- `PowerSet` rejects sets larger than 20 elements (which would generate 1+ million subsets and freeze the UI).
- All algorithm step recording is bounded by the size of the input. A graph with 10 nodes and 15 edges generates ~30 steps for BFS and ~25 for Kruskal.

## Future extensions

The architecture supports several natural extensions without breaking changes:

1. **More graph algorithms.** Add a new `XAdapter` and a new entry in `GraphAlgorithmKind`. The Core algorithm just needs to follow the `AlgorithmTrace` pattern.
2. **Graph isomorphism check.** Pure algorithm in Core; a UI hook in Graph Theory page.
3. **Set-builder notation parser.** Extend the Logic parser, expose in Sets page.
4. **3-SAT solver / DIMACS CNF support.** New page; reuses Logic's parser infrastructure.
5. **Algorithm comparison mode.** Run two algorithms side-by-side. Pure ViewModel work, no Core changes.
