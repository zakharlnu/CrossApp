using System.Security.Cryptography;
using Api.Contracts;
using Core.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrders(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapGet("/", async (
            IOrderStore store,
            CancellationToken cancellationToken) =>
            TypedResults.Ok((await store.ListAsync(cancellationToken))
                .OrderBy(order => order.Id, StringComparer.Ordinal)
                .Select(order => order.ToApiDto())
                .ToList()));

        group.MapGet("/{id}", async Task<Results<Ok<OrderDto>, NotFound>> (
            string id,
            IOrderStore store,
            CancellationToken cancellationToken) =>
        {
            Core.Domain.Order? order =
                await store.GetByIdAsync(id, cancellationToken);

            return order is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(order.ToApiDto());
        });

        group.MapPost("/", async Task<Results<Created<OrderDto>, ValidationProblem>> (
            SaveOrderRequest request,
            IOrderStore store,
            CancellationToken cancellationToken) =>
        {
            if (!request.TryValidate(out Dictionary<string, string[]> errors))
            {
                return TypedResults.ValidationProblem(errors);
            }

            string id = Convert.ToHexString(
                    RandomNumberGenerator.GetBytes(4))
                .ToLowerInvariant();
            Core.Domain.Order order = request.ToOrder(id);
            await store.AddAsync(order, cancellationToken);

            return TypedResults.Created(
                $"/api/orders/{order.Id}",
                order.ToApiDto());
        });

        group.MapPut(
            "/{id}",
            async Task<Results<Ok<OrderDto>, NotFound, ValidationProblem>> (
                string id,
                SaveOrderRequest request,
                IOrderStore store,
                CancellationToken cancellationToken) =>
        {
            if (!request.TryValidate(out Dictionary<string, string[]> errors))
            {
                return TypedResults.ValidationProblem(errors);
            }

            Core.Domain.Order? current =
                await store.GetByIdAsync(id, cancellationToken);
            if (current is null)
            {
                return TypedResults.NotFound();
            }

            Core.Domain.Order replacement = request.ToOrder(id, current.CreatedAt);
            await store.UpdateAsync(replacement, cancellationToken);
            return TypedResults.Ok(replacement.ToApiDto());
        });

        group.MapDelete("/{id}", async Task<Results<NoContent, NotFound>> (
            string id,
            IOrderStore store,
            CancellationToken cancellationToken) =>
            await store.RemoveAsync(id, cancellationToken)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }
}
