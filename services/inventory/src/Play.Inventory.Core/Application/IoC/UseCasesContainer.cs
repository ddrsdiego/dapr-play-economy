namespace Play.Inventory.Core.Application.IoC
{
    using Common.Application.UseCase;
    using Microsoft.Extensions.DependencyInjection;
    using UseCases.CreateCatalogItem;
    using UseCases.CustomerUpdated;
    using UseCases.GetCustomerById;
    using UseCases.GetInventoryItemByUserId;
    using UseCases.GrantItem;

    public static class UseCasesContainer
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            services.AddTransient<IUseCaseExecutor<GrantItemRequest>, GrantItemUseCase>();
            services.AddTransient<IUseCaseExecutor<GetCustomerByIdRequest>, GetCustomerByIdUseCase>();
            services.AddTransient<IUseCaseExecutor<CreateCatalogItemReq>, CreateCatalogItemUseCase>();
            services.AddTransient<IUseCaseExecutor<GetInventoryItemByUserIdReq>, GetInventoryItemByUserIdUseCase>();
            services.AddTransient<IUseCaseExecutor<CustomerUpdatedReq>, CustomerUpdatedUseCase>();
            //
            return services;
        }
    }
}