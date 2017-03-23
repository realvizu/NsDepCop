# Dependency Control

## Why care about code dependencies?
"Poor dependency management leads to code that is hard to change, fragile, and non-reusable," [said Uncle Bob](http://butunclebob.com/ArticleS.UncleBob.PrinciplesOfOod).

Dependency control tools help you enforce an organizing scheme so your code base doesn't degrade with time into a tangled mess.

If you want to learn more about dependency management I highly recommend Uncle Bob's [articles](https://docs.google.com/file/d/0BxR1naE0JfyzV2JVbkYwRE5odGM), [books](https://www.amazon.com/Agile-Principles-Patterns-Practices-C/dp/0131857258) and [training videos](https://cleancoders.com/videos/clean-code/solid-principles).

## How to control code dependencies?
Dependency control tools let you describe your intentions about allowed and disallowed dependencies and warn you about dependency violations.

They can work at different levels and on different input formats:
* at the physical level (projects, libraries, assemblies), or the logical level (namespaces and types),
* on the source code, or on the compiled (binary or intermediary) form.

**This tool checks dependencies at the namespace level, in the source code.**

It also supports some fine-tuning at the type level (see details below).

## So what is a namespace dependency?
Namespace 'A' depends on namespace 'B' (A->B) if any type declared in namespace 'A' uses any type declared in namespace 'B'.

In the example below **A->B** because type A1 uses types B1, B2, B3, B4, B5.
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

It is worth noting that the statement "using B" *does not automatically imply* an A->B dependency because it could be that despite the using no member of namespace B actually appear in class A1.

## What is the recommended approach?
* **Define** the high level structure of your system as a hierarchy of logical modules/packages.
  * Better upfront to avoid a lot of refactoring later.
  * NsDepCop won't help you with this. Use a modeling tool or just pen and paper.
* **Implement** the logical modules/packages with C# namespaces.
  * Maintain a 1-to-1 correspondence between logical units and namespaces.
* [**Describe**](Help.md#dependency-rules) the allowed namespace dependencies in **config.nsdepcop** files.
  * One config file per C# project.
  * You can put the common settings/rules into a "master" config file.
* **Fix** illegal dependencies reported by NsDepCop.
  * Sometimes it will require some rethinking/redesign in the architecture.
  * Beware of **circular dependencies** as those mean that none of the constituents can be safely changed without potentially affecting all the others.

## Why namespace dependencies instead of type dependencies?
In my experience enforcing dependency rules at the type level results in a verbose and brittle dependency description.
Namespaces are a better target for the dependency rules because they give you a level of abstraction above types and also a hierarchical organization that can simplify dependency rules by allowing dependencies to be specified not only between logical packages but also between trees/subtrees of packages.

## Why does this tool support some type level dependency control?
There's a scenario when you as the designer of a system's structure use some third party or legacy stuff whose structure you don't control but still want to limit your system's dependency on it. 
So when depending on a namespace you can define a subset of its types as the *visible part* or the *"surface"* of that namespace. All other types in that namespace will be illegal to depend upon.
