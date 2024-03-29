﻿using Holidays.Core.Events.OfferModel;
using Holidays.Eventing.Core;

namespace Holidays.Postgres.EventHandlers;

public class OfferStartedTrackingPostgresEventHandler : IEventHandler<OfferStartedTracking>
{
    private readonly PostgresConnectionFactory _connectionFactory;

    public OfferStartedTrackingPostgresEventHandler(PostgresConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task Handle(OfferStartedTracking @event, Func<Task> next, CancellationToken cancellationToken)
    {
        if (@event.OfferData is null)
        {
            throw new InvalidOperationException(
                "Received event without offer data.");
        }

        var offer = @event.ToOffer();

        await using var connection = await _connectionFactory.CreateConnection(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var offersRepository = new OffersPostgresRepository(connection, transaction);
        await using var offerChangesRepository = new OfferEventLogPostgresRepository(connection, transaction);
        await using var priceHistoryRepository = new PriceHistoryPostgresRepository(connection, transaction);

        try
        {
            await offersRepository.Add(offer);
            await offerChangesRepository.Add(@event);
            await priceHistoryRepository.Add(offer.Id, @event.Timestamp, offer.Price);

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
