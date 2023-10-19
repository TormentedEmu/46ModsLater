using Mono.Cecil;

namespace FortySixModsLater
{
    public interface IPatcherMod
    {
        bool Patch(ModuleDefinition module);
    }
}
