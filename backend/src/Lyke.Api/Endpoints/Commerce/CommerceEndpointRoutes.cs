using Lyke.Api.Endpoints.Commerce.Clicks;
using Lyke.Api.Endpoints.Commerce.Products;
using Lyke.Api.Endpoints.Commerce.Retailers;

namespace Lyke.Api.Endpoints.Commerce;

public static class CommerceEndpointRoutes
{
    public static IEndpointRouteBuilder MapCommerceEndpoints(this IEndpointRouteBuilder app)
    {
        ClickEndpointRoutes.MapClickEndpoints(app);
        ProductEndpointRoutes.MapProductEndpoints(app);
        RetailerEndpointRoutes.MapRetailerEndpoints(app);

        return app;
    }
}
