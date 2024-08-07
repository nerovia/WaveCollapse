# Wave Collapse

This project serves as a sandbox to experiment and mess around with the wave-collapse algorithm. 

## Project Structure

### WaveLib

This package contains the framework for the algorithm. It defines the datastructures and algorithms to analyse and synthesize patterns. 

### WaveTest

This package contains unit tests for the WaveLib package.

### WaveCli

This is a simple demo app that let's you interact with WaveLib on the console. Keep in mind that this uses `Console.Write` and `Console.SetCursorPosition` to draw the output the screen, so it's not very fast.

### SadWave

This is another demo app based on Thraka's [SadConsole](https://github.com/Thraka/SadConsole). It can draw things much faster and also looks pretier.

## Gettig Started

There are three main classes you'll need. 

### Grid

`IGrid<T>` is used to represent the surface on which the algorithm operates. It implements a variaty of convenient functionality (manly for the algorithm).

```c#
using var stream = FileOpenRead(yourFilePath);
IGrid<char> grid = await GridReader.ReadAsync(stream); 
```

### WaveSchema

A `WaveSchema` defines the rules for a pattern. The easiest way to get a schema is by analyzing an existing pattern using `WaveSchema.Analyze`. To do this you pass the grid 
that contains the patter you want to analyze, and a *stencil* which describes what relationships between tiles matter. The *stencil* is really just a set of offsets for which the analyzer checks what tiles fit together.

```c#
var (schema, tileSet) = WaveSchema.Analyze(grid, WaveSchema.Stencil.Plus);
```

The method returns the actual `schema`, and a `tileSet`. The analyzer reduces all tiles it encounters to integer IDs and creates a tile set (lookup table) for you.

Another method for creating a schema is to parse it from a Rulz file using `WaveRulz.Parse`

### WaveSynthesizer

Once you have obtained a schema you can create a WaveSynthesizer that will generates patterns according to these rules.

```c#
var (width, height) = (10, 10);

// Creates a new synthesizer that operates on a grid with the specified dimensions.
var wave = new WaveSynthesizer(width, height, schema, Random.Shared)

// Initializes the cells of the synthisizer grid.
var wave.Reset();

// Collapses the next cell, and invokes the callback for each cell that is affected by the collapse (including the collapsed cell).
while (wave.CollapseNext(ChangeCallback));
```
