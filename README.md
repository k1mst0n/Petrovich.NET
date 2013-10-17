Petrovich.NET
=============

Petrovich.NET is .NET implementation library of [Petrovich](https://github.com/rocsci/petrovich) ruby gem, which inflects Russian names to given grammatical case.

## Installation

Reference Petrovich.NET dll in your project.
You also need to copy `rules.yml` to folder with Petrovich.NET dll.

## Building

Use Visual Studio 2012 or newer to build solution.
`Allow NuGet to download missing packages` should be turned on in package manager.

## Usage

```csharp
var p = new Petrovich();
Console.WriteLine(p.Lastname("Иванов", CASES.Dative));
Console.WriteLine(p.Firstname("Иван", CASES.Dative));
Console.WriteLine(p.Middlename("Иванович", CASES.Dative))
```

## Contributing

1. Fork it
2. Create your feature branch (git checkout -b my-new-feature)
3. Commit your changes (git commit -am 'Add some feature')
4. Push to the branch (git push origin my-new-feature)
5. Create new Pull Request

You can also support project by reporting issues or suggesting new features and improvements.
