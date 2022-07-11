﻿using NUnit.Framework;

namespace Holidays.Eventing.Tests.TestFixtures;

[TestFixture]
public class EventBusTests
{
    [Test]
    public async Task register_one_handler_and_published_event_should_arrive()
    {
        var handledEvents = new List<TestEvent>();

        var eventBus = CreateEventBus.WithRegisteredTestEventHandler(handledEvents);

        var @event = new TestEvent();
        await eventBus.Publish(@event);
        
        Assert.That(handledEvents, Has.Count.EqualTo(1));
        Assert.That(ReferenceEquals(handledEvents[0], @event), Is.True);
    }
    
    [Test]
    public async Task register_one_handler_and_published_many_events_concurrently_should_arrive()
    {
        var handledEvents = new List<TestEvent>();

        var eventBus = CreateEventBus.WithRegisteredTestEventHandler(handledEvents);

        var events = Enumerable
            .Repeat(0, 20)
            .Select(_ => new TestEvent())
            .ToArray();

        var tasks = events
            .Select(e => eventBus.Publish(e))
            .ToArray();

        await Task.WhenAll(tasks);
        
        Assert.That(handledEvents, Has.Count.EqualTo(20));
        CollectionAssert.AreEquivalent(events, handledEvents);
    }
    
    [Test]
    public async Task register_two_handler_and_published_event_should_arrive_in_correct_order()
    {
        var handledEvents = new List<TestEvent>();

        var eventBus = CreateEventBus.WithTwoRegisteredTestEventHandlers(handledEvents);

        var @event = new TestEvent();
        await eventBus.Publish(@event);
        
        Assert.That(handledEvents, Has.Count.EqualTo(2));
        Assert.That(ReferenceEquals(handledEvents[0], @event), Is.True);
        Assert.That(ReferenceEquals(handledEvents[1], @event), Is.True);
    }

    [Test]
    public void sending_event_that_was_not_registered_should_throw_exception()
    {
        var eventBus = new EventBusBuilder().Build();

        Assert.ThrowsAsync<InvalidOperationException>(
            () => eventBus.Publish(new TestEvent()));
    }
}