﻿using System.Text;
using Holidays.Core.InfrastructureInterfaces;
using Holidays.Core.Monads;
using Holidays.Core.OfferModel;
using Holidays.Selenium;
using OpenQA.Selenium;

namespace Holidays.DataSource.Rainbow;

// ReSharper disable once UnusedType.Global

public class RainbowOffersDataSource : IOffersDataSource
{
    private readonly OffersDataSourceSettings _settings;
    private readonly WebDriverFactory _webDriverFactory;

    public RainbowOffersDataSource(
        OffersDataSourceSettings settings,
        WebDriverFactory webDriverFactory)
    {
        _settings = settings;
        _webDriverFactory = webDriverFactory;
    }

    public string WebsiteName => Constants.WebsiteName;

    public async Task<Result<Offers>> GetOffers(DateOnly maxDepartureDate)
    {
        try
        {
            var offers = await GetOffersOrThrow(maxDepartureDate);
            return Result.Success(offers);
        }
        catch (WebDriverException webDriverException)
        {
            return new WebScrapingError(webDriverException, Constants.WebsiteName);
        }
        catch (TimeoutException)
        {
            return new TimeoutError(Constants.WebsiteName);
        }
    }

    private async Task<Offers> GetOffersOrThrow(DateOnly maxDepartureDate)
    {
        using var webDriver = _webDriverFactory.Create();

        await Task.Delay(1_000);

        await OpenStartUrlAndCloseCookiePopup(webDriver);

        var collector = new OfferElementsCollector(webDriver);
        var extractor = new OfferDataExtractor();

        Offer? lastOffer;
        var offers = Enumerable.Empty<Offer>();

        var timeout = TimeSpan.FromSeconds(_settings.CollectingTimeoutSeconds);

        do
        {
            var collectedElements = await collector.Collect(timeout);

            var collectedOffers = collectedElements
                .Select(element => extractor.Extract(element))
                .ToArray();

            offers = offers.Concat(collectedOffers.Where(offer => offer.DepartureDate <= maxDepartureDate));

            lastOffer = collectedOffers.LastOrDefault();
        } while (lastOffer is not null && lastOffer.DepartureDate <= maxDepartureDate);

        return new Offers(offers);
    }

    private static async Task OpenStartUrlAndCloseCookiePopup(IWebDriver webDriver)
    {
        webDriver
            .Navigate()
            .GoToUrl(GetUrl());

        await Task.Delay(1_000);

        if (webDriver.TryFindElement(By.ClassName("rodo-alert__close"), out var closeCookiesElement))
        {
            closeCookiesElement.Click();
        }
    }

    private static string GetUrl()
    {
        var departureDateFrom = DateTime.Now.ToString("yyyy-MM-dd");

        return new StringBuilder()
            .Append("https://r.pl/all-inclusive")
            .Append("?liczbaPokoi=1")
            .Append("&sortowanie=termin-asc")
            .Append("&widok=bloczki")
            .Append("&cena[]=avg")
            .Append("&dlugoscPobytu[]=7-9")
            .Append("&terminWyjazdu[]=").Append(departureDateFrom)
            .Append("&typTransportu[]=AIR")
            .Append("&typTransportu[]=dreamliner")
            .Append("&wyzywienia[]=all-inclusive")
            .ToString();
    }
}
