﻿using Holidays.Core.Events.OfferModel;
using Holidays.Core.OfferModel;

namespace Holidays.Postgres;

internal static class EventsExtensionMethods
{
    public static Offer ToOffer(this OfferStartedTracking @event)
    {
        var offerData = @event.OfferData!;

        return new Offer(
            offerData.Hotel,
            offerData.DestinationCountry,
            offerData.DetailedDestination,
            offerData.DepartureDate,
            offerData.Days,
            offerData.CityOfDeparture,
            offerData.Price,
            offerData.DetailsUrl,
            offerData.WebsiteName);
    }

    public static Offer ToOffer(this OfferAdded @event)
    {
        var offerData = @event.OfferData!;

        return new Offer(
            offerData.Hotel,
            offerData.DestinationCountry,
            offerData.DetailedDestination,
            offerData.DepartureDate,
            offerData.Days,
            offerData.CityOfDeparture,
            offerData.Price,
            offerData.DetailsUrl,
            offerData.WebsiteName);
    }
}
