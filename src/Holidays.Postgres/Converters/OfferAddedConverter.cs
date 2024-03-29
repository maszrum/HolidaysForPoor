﻿using Holidays.Core.Events.OfferModel;
using Holidays.Postgres.DbRecords;

namespace Holidays.Postgres.Converters;

internal class OfferAddedConverter : EventConverterBase<OfferAdded>
{
    protected override Guid GetOfferId(OfferAdded @event) => @event.OfferId;

    protected override string GetEventParams(OfferAdded @event) => string.Empty;

    protected override OfferAdded ToObject(OfferEventLogRecord record) =>
        OfferAdded.WithoutOfferData(record.OfferId, record.EventTimestamp);
}
