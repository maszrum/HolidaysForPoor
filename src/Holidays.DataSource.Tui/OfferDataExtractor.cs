﻿using Holidays.Core.OfferModel;
using OpenQA.Selenium;

namespace Holidays.DataSource.Tui;

internal class OfferDataExtractor
{
    public Offer Extract(IWebElement element)
    {
        var hotelElement = element.FindElement(By.ClassName("offer-tile-body__hotel-name"));

        var (destinationCountry, detailedDestination) = ExtractDestinationCountryAndDetailedDestination(element);

        var (departureDate, days) = GetDepartureDateAndDays(element);

        var cityOfDepartureElement = element.FindElement(By.CssSelector("button.dropdown-field--same-day-offers span"));

        var priceElement = element.FindElement(By.ClassName("price-value__amount"));

        var detailsLinkElement = element.FindElement(By.ClassName("offer-tile-aside__button--cta"));

        var offer = new Offer(
            hotel: hotelElement.Text,
            destinationCountry: destinationCountry,
            detailedDestination: detailedDestination,
            departureDate: departureDate,
            days: days,
            cityOfDeparture: cityOfDepartureElement.Text,
            price: int.Parse(priceElement.Text.Replace(" ", string.Empty)),
            detailsUrl: detailsLinkElement.GetAttribute("href"),
            Constants.WebsiteName);

        return offer;
    }

    private static (string, string) ExtractDestinationCountryAndDetailedDestination(IWebElement offerElement)
    {
        var elements = offerElement.FindElements(By.CssSelector("li.breadcrumbs__item a"));

        var texts = elements
            .Select(e => e.Text)
            .ToArray();

        var destinationCountry = texts.Length == 0
            ? string.Empty
            : ToFirstWordLettersCapitals(texts[0]);

        var detailedDestination = texts.Length < 2
            ? string.Empty
            : ToFirstWordLettersCapitals(texts[1]);

        return (destinationCountry, detailedDestination);
    }

    private static (DateOnly, int) GetDepartureDateAndDays(IWebElement offerElement)
    {
        var spanElements = offerElement.FindElements(By.CssSelector("ul.offer-tile-body__info-list span"));

        var element = spanElements.Single(e =>
        {
            var text = e.Text;
            return text.Length > 0 && char.IsDigit(text[0]);
        });

        var text = element.Text;

        var indexOfSpace = text.IndexOf(' ');
        var departureDateText = text.Substring(0, indexOfSpace);

        var indexOfBracket = text.IndexOf('(');
        var daysText = text.Substring(indexOfBracket + 1);
        indexOfSpace = daysText.IndexOf(' ');
        daysText = daysText.Substring(0, indexOfSpace);

        return (
            DateOnly.ParseExact(departureDateText, "dd.MM.yyyy"),
            int.Parse(daysText));
    }

    private static string ToFirstWordLettersCapitals(string input)
    {
        var result = input.ToCharArray();
        var previousSpace = false;

        for (var i = 0; i < result.Length; i++)
        {
            if (!char.IsLetter(result[i]))
            {
                previousSpace = result[i] == ' ';
                continue;
            }

            result[i] = i == 0 || previousSpace ?
                char.ToUpperInvariant(result[i])
                : char.ToLowerInvariant(result[i]);

            previousSpace = false;
        }

        return new string(result);
    }
}
