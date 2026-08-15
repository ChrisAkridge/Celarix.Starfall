namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed class ForegroundPropertyDefinition : IPropertyDefinition
    {
        public string Key => "foreground";

        public bool IsFenceTypeProperty => false;

        public LibraBuildContext Apply(LibraBuildContext context,
            PropertyEntry entry,
            LibraBinder binder)
        {
            return context with { ForegroundColor = binder.BindColor(entry) };
        }
    }
}
