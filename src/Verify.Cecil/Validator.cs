class Validator
{
    public List<string> Problems { get; } = [];

    public void Assembly(AssemblyDefinition assembly)
    {
        foreach (var module in assembly.Modules)
        {
            Module(module);
        }
    }

    public void Module(ModuleDefinition module)
    {
        foreach (var type in module.Types)
        {
            Type(type);
        }
    }

    public void Type(TypeDefinition type)
    {
        foreach (var method in type.Methods)
        {
            Method(method);
        }

        foreach (var nested in type.NestedTypes)
        {
            Type(nested);
        }
    }

    public void Property(PropertyDefinition property)
    {
        Accessor(property.GetMethod);
        Accessor(property.SetMethod);
    }

    public void Event(EventDefinition @event)
    {
        Accessor(@event.AddMethod);
        Accessor(@event.RemoveMethod);
        Accessor(@event.InvokeMethod);
    }

    void Accessor(MethodDefinition? method)
    {
        if (method != null)
        {
            Method(method);
        }
    }

    public void Method(MethodDefinition method) =>
        new MethodValidator(method, Problems).Run();
}
