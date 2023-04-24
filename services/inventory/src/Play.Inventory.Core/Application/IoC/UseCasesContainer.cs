namespace Play.Inventory.Core.Application.IoC
{
    using Common.Application.UseCase;
    using Microsoft.Extensions.DependencyInjection;
    using UseCases.CustomerUpdated;
    using UseCases.GetInventoryItemByUserId;

    public static class UseCasesContainer
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            services.AddTransient<IUseCaseExecutor<GetInventoryItemByUserIdReq>, GetInventoryItemByUserIdUseCase>();
            services.AddTransient<IUseCaseExecutor<CustomerUpdatedCommand>, CustomerUpdatedUseCase>();
            return services;
        }
    }
}