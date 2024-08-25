# Dependency Control

## Why Care About Code Dependencies?
The number one enemy of a developer is complexity.

Among the many sources of complexity, lets focus now on the structure of the codebase and its dependencies.
* To reduce complexity, the primary strategy is to decompose the code into smaller units (e.g., modules) and limit dependencies between them. 
* A codebase with well-defined modules and clear dependency rules is easier to understand, modify, and reuse. 
* However, without a dependency control tool acting as a safeguard, even a well-structured codebase can eventually degrade into a tangled mess.

## How to Control Code Dependencies?
Dependency control tools allow you to define rules for permitted and prohibited dependencies and alert you to any violations.

These tools can operate at various levels and on different input formats:
* Physical level: Projects, libraries, assemblies.
* Logical level: Namespaces, types.
* Input formats: Source code, compiled forms (binary or intermediary).

**This tool checks dependencies at the namespace level within the source code.**

It also supports some fine-tuning at the type level (see details below).

## What Is a Namespace Dependency?
Namespace **A** depends on Namespace **B** if any type declared in Namespace **A** uses any type declared in Namespace **B**.

In the example below **Namespace A depends on Namespace B** because type A1 uses types B1, B2, B3, B4 and B5.
```csharp
namespace A
{
    using B;

    class A1 : B1
    {
        B2 field1;
        B3 Property1 { get; set; }
        B4 MyMethod(B5 p) { ... }
    }
}
namespace B
{
    interface B1 {}
    class B2 {}
    struct B3 {}
    enum B4 {}
    delegate void B5();
}
```

Note that a "using B" directive *does not automatically imply* a dependency from A to B. If no members of Namespace B are actually used in class A1, then no real dependency exists. Modern code editors can warn about unnecessary using directives. This tool only considers actual code dependencies.

## Recommended approach
* **Define** the high-level structure of your system as a hierarchy of logical modules or packages. 
  * It's better to do this upfront to avoid extensive refactoring later. 
  * This tool won't assist with the initial design; use a modeling tool or even pen and paper for this step.
* **Implement** these logical modules/packages using C# namespaces.
  * Ensure a one-to-one correspondence between logical units and namespaces.
* [**Describe**](Help.md#dependency-rules) allowed namespace dependencies in config.nsdepcop files.
  * Use one config file per C# project. 
  * Common dependency rules can be placed in a ["master" config file](Help.md#config-inheritance).
* **Fix** illegal dependencies reported by the tool. 
  * This may require rethinking or redesigning parts of your architecture.
  * Avoid circular dependencies.

## Why Namespace Dependencies Instead of Type Dependencies?
Enforcing dependency rules at the type level can lead to verbose and fragile dependency descriptions.

Namespaces provide a useful level of abstraction above types and offer a hierarchical structure that simplifies dependency rules. This allows dependencies to be specified not just between individual logical packages but also between groups or subgroups of packages.

## Why Does This Tool Support Some Type-Level Dependency Control?
In cases where you are working with third-party or legacy code whose structure you cannot control, but still want to limit dependencies, this tool allows you to specify a subset of types within a namespace as the visible "surface" of that namespace. All other types in that namespace are then illegal to depend upon.
