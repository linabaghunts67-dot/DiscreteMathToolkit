# DiscreteMathToolkit

## Overview

DiscreteMathToolkit is a C# console-based project that implements fundamental concepts from Discrete Mathematics in a practical and interactive way.

The project transforms theoretical topics such as sets, relations, graph traversal, propositional logic, and combinatorics into executable algorithms, allowing users to explore and validate results directly.

---

## Project Structure

The project is organized into three main files:

* **DiscreteMathToolkit.cs**
  Core implementation of all discrete mathematics modules

* **DiscreteMathTests.cs**
  Testing module to validate correctness of algorithms

* **DiscreteMathVisualization.cs**
  Visualization utilities for displaying results in a readable format

---

## Features

### 1. Set Operations

* Union
* Intersection
* Difference

Implemented using C# `HashSet` to reflect mathematical set behavior.

---

### 2. Relations Analyzer

Supports analysis of relations represented as ordered pairs.

Checks:

* Reflexive property
* Symmetric property
* Transitive property

---

### 3. Graph Theory

Graph representation using adjacency lists.

Algorithms implemented:

* Breadth-First Search (BFS)
* Depth-First Search (DFS)

---

### 4. Propositional Logic

* Generates truth tables
* Evaluates logical expressions such as AND, OR, NOT

---

### 5. Combinatorics

* Factorial calculation
* Permutations (nPr)
* Combinations (nCr)

---

### 6. Testing System

* Verifies correctness of implemented algorithms
* Includes assertions for:

  * Set operations
  * Relations
  * Combinatorics

---

### 7. Visualization

* Displays sets in mathematical format
* Prints relations as ordered pairs
* Shows graph structure (adjacency list)
* Outputs BFS and DFS traversal order
* Generates truth tables

---

## How to Run

1. Make sure you have .NET installed
2. Place all `.cs` files in the same project folder
3. Compile and run:

```bash
csc DiscreteMathToolkit.cs DiscreteMathTests.cs DiscreteMathVisualization.cs
./DiscreteMathToolkit.exe
```

Or use Visual Studio / Rider to run as a console application.

---

## Example Usage

When running the program, the user interacts with a menu:

```
1 - Set Operations
2 - Relations
3 - Graphs
4 - Logic
5 - Combinatorics
6 - Run Tests
0 - Exit
```

---

## Educational Value

This project demonstrates:

* Application of discrete mathematics theory in programming
* Implementation of fundamental algorithms
* Use of testing for validation
* Clear visualization of abstract concepts

---

## Possible Extensions

* Shortest path algorithms (Dijkstra)
* Graph cycle detection
* Boolean expression parser
* Matrix representation of relations
* Advanced combinatorics (binomial theorem)

---

## Author

Lina Baghunts

---
