namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed class BackgroundPropertyDefinition : IPropertyDefinition
    {
        public string Key => "background";

        public bool IsFenceTypeProperty => false;

        public LibraBuildContext Apply(LibraBuildContext context,
            PropertyEntry entry,
            LibraBinder binder)
        {
            return context with { BackgroundColor = binder.BindColor(entry) };
        }
    }
}
