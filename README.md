[![Release CLI](https://github.com/DuncanMcPherson/vectra-cli/actions/workflows/release.yml/badge.svg)](https://github.com/DuncanMcPherson/vectra-cli/actions/workflows/release.yml)

# Vectra CLI (vecc.exe)

Vectra CLI is a command-line interface for compiling and running Vectra programs. Vectra is a custom programming language with its own virtual machine runtime.

## Installation

The Vectra CLI tool is distributed as a standalone executable (`vecc.exe`) for Windows. You can download the latest version from the [Releases](https://github.com/DuncanMcPherson/vectra-cli/releases) page.

## Usage

Vectra CLI supports the following commands:

### Build Command

Compiles a Vectra source code file (`.vec`) into a bytecode file (`.vbc`).

```bash
vecc build <input-file.vec>
```

Arguments:
- `input-file.vec`: The Vectra source code file to compile (must have a `.vec` extension)

### Run Command

Executes a compiled Vectra bytecode file (`.vbc`).

```bash
vecc run <input-file.vbc>
```

Arguments:
- `input-file.vbc`: The compiled bytecode file to run (must have a `.vbc` extension)

## Example

Creating and running a simple Vectra program:

1. Create a file named `Program.vec` with the following content:

```
space System;

class Program {
    number main() {
        return 42;
    }
}
```

2. Compile the program:

```bash
vecc build Program.vec
```

This will generate a `Program.vbc` file.

3. Run the compiled program:

```bash
vecc run Program.vbc
```

## Version Information

Current version: 1.2.0

## Future Enhancements

The following features are planned for future releases:

- Support for `.vmod` and `.vproj` files
- Direct interpretation of source code without compilation step

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
