namespace Obsidian.Entities.Decoration;

public class ItemFrame : Entity
{
    public object Item { get; private set; }

    public ItemFrame(object item)
    {
        Type = EntityType.ItemFrame;
        Item = item;
    }
}
