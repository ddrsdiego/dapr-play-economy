namespace Play.Catalog.Core.Application.Infra.Repositories
{
    using Domain.AggregatesModel.CatalogItemAggregate;

    internal static class CatalogItemEx
    {
        public static CatalogItemData ToCatalogItemData(this CatalogItem catalogItem)
        {
            return new CatalogItemData(catalogItem.Id)
            {
                CatalogItemId = catalogItem.Id,
                CatalogItemName = catalogItem.Description.Name,
                Description = catalogItem.Description.Value,
                Price = catalogItem.Price.Value,
                CreateAt = catalogItem.CreateAt,
            };
        }

        public static CatalogItem ToCatalogItem(this CatalogItemData catalogItemData)
        {
            return new CatalogItem(catalogItemData.CatalogItemId, catalogItemData.Price,
                catalogItemData.CatalogItemName, catalogItemData.Description, catalogItemData.CreateAt);
        }
    }
}