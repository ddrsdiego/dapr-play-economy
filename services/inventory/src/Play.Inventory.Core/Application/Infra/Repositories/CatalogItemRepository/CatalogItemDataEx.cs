namespace Play.Inventory.Core.Application.Infra.Repositories.CatalogItemRepository;

using Domain.AggregateModel.CatalogItemAggregate;

public static class CatalogItemDataEx
{
    public static CatalogItemData ToCatalogItemData(this CatalogItem? catalogItem)
    {
        return new CatalogItemData(catalogItem.CatalogItemId)
        {
                
            CatalogItemId = catalogItem.CatalogItemId,
            Description = catalogItem.Description,
            Name = catalogItem.Name,
            CreatedAt = catalogItem.CreatedAt
        };
    }

    public static CatalogItem ToStateEntry(this CatalogItemData catalogItemData)
    {
        var catalogItem = new CatalogItem(catalogItemData.CatalogItemId, catalogItemData.Name,
            catalogItemData.Description, catalogItemData.CreatedAt);

        return catalogItem;
    }
}