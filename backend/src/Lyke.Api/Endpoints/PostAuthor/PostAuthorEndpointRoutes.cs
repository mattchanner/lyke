using Lyke.Application.DTOs;
using Lyke.Application.DTOs.Post;

namespace Lyke.Api.Endpoints.PostAuthor;

public static class PostAuthorEndpointRoutes
{
    public static IEndpointRouteBuilder MapPostAuthorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts/v1/my")
            .WithTags("PostAuthor")
            .RequireAuthorization("ContentAuthor");

        group
            .MapPost("/", CreateAuthorPost.Handle)
            .WithName("CreateAuthorPost")
            .WithSummary("Create a new draft post")
            .Produces<ApiResponse<AuthorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest);

        group
            .MapGet("/", GetAuthorPosts.Handle)
            .WithName("GetAuthorPosts")
            .WithSummary("List own posts with optional status filter")
            .Produces<ApiResponse<IReadOnlyList<AuthorPostResponse>>>(StatusCodes.Status200OK);

        group
            .MapGet("/{id:guid}", GetAuthorPost.Handle)
            .WithName("GetAuthorPost")
            .WithSummary("Get own post detail")
            .Produces<ApiResponse<AuthorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPut("/{id:guid}", UpdateAuthorPost.Handle)
            .WithName("UpdateAuthorPost")
            .WithSummary("Update a draft post")
            .Produces<ApiResponse<AuthorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapDelete("/{id:guid}", DeleteAuthorPost.Handle)
            .WithName("DeleteAuthorPost")
            .WithSummary("Delete a post")
            .Produces<ApiResponse>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        group
            .MapPost("/{id:guid}/submit", SubmitAuthorPost.Handle)
            .WithName("SubmitAuthorPost")
            .WithSummary("Submit a draft post for review")
            .Produces<ApiResponse<AuthorPostResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
            .Produces<ApiResponse>(StatusCodes.Status404NotFound);

        return app;
    }
}
