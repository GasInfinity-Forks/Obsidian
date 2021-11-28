using System.Reflection;

namespace Obsidian.Entities;

public static class EntityFactory
{
    private static readonly ConcurrentDictionary<EntityType, Type> entityLookup = new();

    public static Entity? GetEntity(EntityType entityType)
    {
        if (entityLookup.ContainsKey(entityType))
        {
            return (Entity?) Activator.CreateInstance(entityLookup[entityType]);
        }
        else
        {
            var assembly = Assembly.GetExecutingAssembly();
            Type? t = assembly.GetTypes().FirstOrDefault(t => t.Name.ToLower() == entityType.ToString().ToLower());
            if (t == null) return null;
            entityLookup.TryAdd(entityType, t);
            return (Entity?)Activator.CreateInstance(t);
        }
    }
}
