namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal interface IPropertyDefinition
    {
        string Key { get; }

        bool IsFenceTypeProperty { get; }

        LibraBuildContext Apply(LibraBuildContext context,
            PropertyEntry entry,
            LibraBinder binder);
    }
}
