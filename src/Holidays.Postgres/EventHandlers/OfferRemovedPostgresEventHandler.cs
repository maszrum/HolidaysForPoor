﻿using Holidays.Core.Events.OfferModel;
using Holidays.Eventing.Core;

namespace Holidays.Postgres.EventHandlers;

public class OfferRemovedPostgresEventHandler : IEventHandler<OfferRemoved>
{
    private readonly PostgresConnectionFactory _connectionFactory;

    public OfferRemovedPostgresEventHandler(PostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task Handle(OfferRemoved @event, Func<Task> next, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnection(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var offersRepository = new OffersPostgresRepository(connection, transaction);
        await using var offerChangesRepository = new OfferEventLogPostgresRepository(connection, transaction);
        await using var priceHistoryRepository = new PriceHistoryPostgresRepository(connection, transaction);

        try
        {
            await offersRepository.Remove(@event.OfferId);
            await offerChangesRepository.Add(@event);
            await priceHistoryRepository.Add(@event.OfferId, @event.Timestamp, 0);

            await next();

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
