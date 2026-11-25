# Unity 6.3 — Core / Scripting Module Reference

**Last verified:** 2026-11-24
**Knowledge Gap:** C# and .NET profile limits, IL2CPP/AOT restrictions, Web platform threading, serialization rules
**Scope:** pure-C# rule code (no engine surface): language level, API profile, compilation units, scripting backend, stripping, serialization, Play Mode state, tests. Rendering, physics, input, UI live in their own modules.

> **Docs moved.** Every `docs.unity3d.com/6000.3/Documentation/Manual/...` URL below now
> answers 301 to `docs.unity.com/en-us/engine/6000.3/manual/...`. The Sources section
> lists the target URLs that were read.

---

## Overview

Unity 6.3 compiles C# with Roslyn at **C# 9.0**, against **.NET Standard 2.1** by
default. Platforms other than Windows, macOS, Linux x64 and Android Armv7 are
**IL2CPP-only** (ahead-of-time), which includes the Web platform. On the Web platform
managed threads do not run.

---

## C# Language Level

- **C# 9.0**, Roslyn compiler.
- **Unsupported C# 9 features** (compilation errors): suppress emitting `localsinit`
  flag; covariant return types; module initializers; extensible calling conventions for
  unmanaged function pointers; **init-only setters**.
- **Records:** full support needs `System.Runtime.CompilerServices.IsExternalInit`,
  which exists only in .NET 5+ ("which Unity doesn't support"). Declaring that type
  yourself is the documented workaround. **Do not use records in serialized types**
  (manual's own warning).
- Extra compiler options: the manual says the Editor passes a default option set and
  points to conditional compilation for more. See NOT SOURCEABLE below for `checked`.

```csharp
// ❌ Compile error in Unity 6.3: init-only setter
public sealed class Casilla { public int Columna { get; init; } }

// ✅ Readonly struct with constructor
public readonly struct Casilla
{
    public readonly int Columna;
    public readonly int Fila;
    public Casilla(int columna, int fila) { Columna = columna; Fila = fila; }
}
```

## .NET API Compatibility Level

| Level | What it is | Note |
|---|---|---|
| **.NET Standard 2.1** | **Default** | Manual: prefer it for all new projects |
| .NET Framework | .NET Framework 4.8 plus extra .NET Standard 2.1 APIs | A .NET Framework-compiled plug-in is "Limited support" under .NET Standard 2.1 |

Set in **Project Settings > Player > Other Settings > Configuration > Api Compatibility
Level**, or `PlayerSettings.SetAPICompatibilityLevel`.

**Implication for a shared core:** code that targets .NET Standard 2.1 and C# 9 can be
compiled both by Unity and by a plain .NET SDK project (e.g. a server build). Which
target framework the server project uses is an architecture decision, not sourced here.

---

## Assembly Definitions (asmdef)

| Option | Effect (6.3 manual) |
|---|---|
| **No Engine References** | Unity does not add references to `UnityEngine` or `UnityEditor` when compiling the assembly |
| **Platforms** | "Any Platform" = all platforms except those you exclude; deselect it and only the platforms you select are included |
| **Override References** | Manually choose which precompiled assemblies the asmdef references; otherwise it references all precompiled assemblies in the project |

A rules core with **No Engine References** enabled cannot call `UnityEngine` APIs by
accident (`Mathf`, `Random`, `Debug.Log`, `Time` all fail to compile), so the compiler
itself enforces the "pure C#" boundary.

---

## Scripting Backends and IL2CPP

- Mono is available only on Windows, macOS, Linux x64 and Android Armv7. **Every other
  platform is IL2CPP-only**, including the Web platform (inferred from that list; the
  manual names Web nowhere as an exception).
- IL2CPP limitations (AOT):
  - `System.Reflection.Emit` and dynamic methods: **not supported**.
  - `dynamic` keyword: **not supported** (needs JIT).
  - Reflection works, but code reached **only** through reflection may be stripped or
    not generated.
  - Generics: instances created via runtime reflection (`MakeGenericType` +
    `Activator.CreateInstance`, invoking generic methods by reflection) and some
    generic virtual calls that cannot be inferred at compile time may be missing.
  - Exception filters work, but filter side effects can run at a different time than
    on Mono.
  - `System.Diagnostics.Process`, `Marshal.Prelink/PrelinkAll`: not supported.
- **IL2CPP runtime checks** (`[Il2CppSetOption]`, scope: assembly, type, method,
  property; most local wins):

| Check | Default | Off means |
|---|---|---|
| Null checks | Enabled | No `NullReferenceException` |
| Array bounds checks | Enabled | No `IndexOutOfRangeException` |
| Divide by zero checks | **Disabled** | No `DivideByZeroException` (integer division by zero is not caught) |

**Implication:** with divide-by-zero checks off by default, rules code that divides by a
data value (a config field, a table length) must validate the divisor itself, or turn
the check on for that assembly.

---

## Web Platform (formerly "WebGL")

- The 6.3 manual calls it the **Web** platform (URLs still say `webgl`).
- **Managed threads are not supported.** `Thread.Start()` does not throw but the
  delegate never runs; `ThreadPool` callbacks never run; `Timer`, `System.Timers.Timer`,
  `Monitor`, `Mutex`, `Semaphore` not supported; `Interlocked`, `Volatile`,
  `MemoryBarrier` execute with no effect.
- **Tasks:** `ValueTask`, `TaskCompletionSource<T>`, `Task.FromResult`,
  `Task.CompletedTask` work. **Anything that schedules on the ThreadPool**
  (`Task.Run`, `Task.Delay`, `Parallel`, `TaskFactory.StartNew` and continuations)
  causes **"an unrecoverable browser hang"**. The manual recommends `Awaitable`.
- Concurrent collections compile but give no real thread safety.

```csharp
// ❌ Hangs the browser on the Web platform
var resultado = await Task.Run(() => reglas.Aplicar(estado, comando));

// ✅ Synchronous, single-threaded rules call
var resultado = reglas.Aplicar(estado, comando);
```

---

## Managed Code Stripping

**Project Settings > Player > Other Settings > Optimization > Managed Stripping Level**

| Level | Behavior |
|---|---|
| Disabled | Nothing removed. Mono only (its default) |
| **Minimal** | Searches only UnityEngine and .NET libraries; no user code removed. **Default for IL2CPP** |
| Low | Marked for future deprecation; manual says not to use it |
| Medium | Searches all assemblies; more patterns stripped |
| High | Removes as much as possible; test thoroughly, use `[Preserve]` and `link.xml` |

- `[Preserve]` on a type keeps the type and its default constructor; on a method keeps
  the method, its declaring type, return and argument types.
- `link.xml` must be under `Assets/` (not inside a package) and contain a `<linker>`
  element; several files are allowed.
- `[AlwaysLinkAssembly]` forces an assembly to be searched.

---

## Serialization

### Unity serializer (Inspector, ScriptableObject, JsonUtility)

- **Not supported:** `Dictionary<,>`, **multidimensional arrays**, jagged arrays,
  nested container types. Workaround: wrap in a class/struct or implement
  `ISerializationCallbackReceiver`.
- Not serialized: `readonly`, `static`, `const` fields; properties (use
  `[SerializeField]` on a backing field, or `[field: SerializeField]` on an
  auto-property).
- Private fields need `[SerializeField]`.
- Serializable: primitives, enums of 32 bits or less, fixed-size buffers, Unity
  built-ins, `[Serializable]` classes/structs (non-static; the attribute is **not
  inherited**), `UnityEngine.Object` references, arrays and `List<T>` of these.
- Polymorphism, null, shared or cyclic references need `[SerializeReference]` (adds
  overhead); without it a derived instance in a base-typed field comes back as the base.

```csharp
// ❌ Not serialized: 2D array
public int[,] casillas = new int[24, 15];

// ✅ Flat array, index = fila * columnas + columna
public int[] casillas = new int[24 * 15];
```

### JsonUtility

- Input: `MonoBehaviour`, `ScriptableObject`, or a `[Serializable]` class/struct.
- **No top-level arrays or primitives**: wrap them in a class.
- No dictionaries. Fields only.
- `FromJson` throws for `MonoBehaviour`/`ScriptableObject`: use `FromJsonOverwrite`.
- The manual suggests a general .NET JSON library for structures the serializer
  cannot handle.

### Newtonsoft Json package

`com.unity.nuget.newtonsoft-json`, docs page for **3.2.2** (wraps Newtonsoft.Json
13.0.2), published on Unity's package docs. Current latest version: see NOT SOURCEABLE.

---

## Enter Play Mode and Static State

**Project Settings > Editor > Enter Play Mode Settings > When entering Play Mode:**
"Reload Scene only", "Reload Domain only", or "Do not reload Domain or Scene".

With domain reload off: **static fields keep their values** between Play sessions,
static events keep subscribers, non-serialized fields keep their end-of-Play values.
Reset in code with
`[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`
(on entry), `EditorApplication.playModeStateChanged` (on exit), or
`[InitializeOnEnterPlayMode]` (Editor-only). Script changes still trigger a domain
reload on asset refresh.

**Implication:** a rules core with no static mutable state is unaffected by this
setting.

---

## Unity Test Framework

- A **core Unity package**, shipped with the Editor (the 6.3 manual names no version;
  the package docs page read is 1.4.6).
- A test assembly is **any assembly that references NUnit**.
- **Edit Mode** tests run only in the Editor, in `EditorApplication.update`, cannot
  run coroutines; asmdef references `nunit.framework.dll` and sets
  `"includePlatforms": ["Editor"]`.
- **Play Mode** tests run in the Editor or a Player; `[UnityTest]` runs as a coroutine;
  own asmdef referencing NUnit; cannot reference `Assembly-CSharp.dll`.
- Test Runner-created asmdefs reference `nunit.framework.dll`,
  `UnityEngine.TestRunner`, and `UnityEditor.TestRunner` (Edit Mode only). Default is
  Editor-only.
- Manual's advice: use `[Test]` unless the test needs frames, time or editor yields.

---

## NOT SOURCEABLE (not covered by the pages read)

Do not fill these from training data; verify before relying on them.

- **C# `checked` arithmetic / `OverflowException` under IL2CPP.** No 6.3 page read
  mentions it (C# compiler, .NET profile, IL2CPP limitations, IL2CPP runtime checks,
  additional IL2CPP args). Whether a `checked` block throws on the Web build is
  unconfirmed.
- **Floating-point determinism across platforms/backends.** No page read states it.
  (Integer-only rules sidestep the question; the claim that floats differ is not
  sourced here.)
- **Default "Enter Play Mode" setting for new 6.3 projects.** Not stated on the pages
  read. (VERSION.md records that 6.6 skips domain reload by default for new projects;
  that is 6.6, not 6.3.)
- **Latest `com.unity.nuget.newtonsoft-json` version** for 6.3.
- **`UNITY_INCLUDE_TESTS` define** and the Test Framework version bundled with 6000.3.
- **IL2CPP compiler options page**: `il2cpp-compiler-options.html` returns 404 in
  6000.3; the runtime-checks page above replaces it.

---

## Sources

- C# compiler: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/overview-of-dot-net-in-unity/csharp-compiler
- .NET profile support: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/overview-of-dot-net-in-unity/dotnet-profile-support
- Assembly Definition properties: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/script-compilation/assembly-definition-files/class-assembly-definition-importer
- Scripting backends intro: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/scripting-backends/intro
- IL2CPP limitations: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/scripting-backends/il2cpp/scripting-restrictions
- IL2CPP runtime checks: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/scripting-backends/il2cpp/il2cpp-runtime-checks
- Additional IL2CPP args: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/scripting-backends/il2cpp/handling-il2cpp-additional-args
- Web .NET API support: https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/webgl/develop/web-dotnet-api-support
- Managed code stripping: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/script-compilation/managed-code-stripping (and `/configure`, `/preserving`)
- JSON serialization: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/json-serialization
- Serialization rules: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/script-serialization/rules
- Configurable Enter Play Mode: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/configurable-enter-play-mode
- Domain reloading: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/code-reload-serialization/code-reloading-editor/domain-reloading
- Edit vs Play Mode tests: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/getting-started/edit-mode-vs-play-mode-tests
- Create a test assembly: https://docs.unity.com/en-us/engine/6000.3/manual/programming-environment/test-framework-introduction/getting-started/workflow-create-test-assembly
- Newtonsoft Json package: https://docs.unity3d.com/Packages/com.unity.nuget.newtonsoft-json@3.2/manual/index.html
- Test Framework package: https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/index.html
