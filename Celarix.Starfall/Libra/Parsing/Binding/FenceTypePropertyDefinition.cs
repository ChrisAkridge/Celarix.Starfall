namespace Celarix.Starfall.Libra.Parsing.Binding
{
    internal sealed class FenceTypePropertyDefinition : IPropertyDefinition
    {
        public string Key => "fencetype";

        public bool IsFenceTypeProperty => true;

        public LibraBuildContext Apply(LibraBuildContext context,
            PropertyEntry entry,
            LibraBinder binder)
        {
            return context with { FenceType = binder.BindFenceType(entry) };
        }
    }
}
