using MessageBroker.Data;
using MessageBroker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("api/topics", async(AppDbContext context, [FromBody]Topic topic) => {
    await context.Topics.AddAsync(topic);
    await context.SaveChangesAsync();
    return Results.Created($"api/topics/{topic.Id}", topic);
});

app.MapGet("api/topics", async(AppDbContext context) => {
    var topics = await context.Topics.ToListAsync();
    return Results.Ok(topics);
});


// publish

app.MapPost("api/topics/{id}/message", async(AppDbContext context, int id, [FromBody] Message message) => {
    bool topics = await context.Topics.AnyAsync(t=> t.Id == id);
    if(!topics) 
        return Results.NotFound("Topic not found");
    
    var subs = context.Subscriptions.Where(s => s.TopicId == id);
    if(subs.Count() == 0)
    {
        // no subscribers for this topic
        return Results.NotFound("No subscribers found for this topic");
    }

    foreach(var sub in subs)
    {
        Message msg = new Message()
        {
            TopicMessage = message.TopicMessage,
            SubscriptionId = sub.Id,
            Expires = message.Expires,
            MessageStatus = message.MessageStatus
        };
        await context.Messages.AddAsync(msg);
    }

    await context.SaveChangesAsync();
    return Results.Ok("Message has been added");

});

// Create Subscription

app.MapPost("api/topics/{id}/subscribe", async(AppDbContext context, int id, [FromBody] Subscription subscription) => {
    bool topics = await context.Topics.AnyAsync(t=> t.Id == id);
    if(!topics) 
        return Results.NotFound("Topic not found");
    
    subscription.TopicId = id;
    await context.Subscriptions.AddAsync(subscription);
    await context.SaveChangesAsync();
    return Results.Created($"api/topics/{id}/subscribe", subscription);
});

app.MapGet("api/subscriptions/{id}/messages", async(AppDbContext context, int id) => {
    bool subs = await context.Subscriptions.AnyAsync(s=> s.Id == id);
    if(!subs)
        return Results.NotFound("Subscription not found");
    
    var messages = context.Messages.Where(s=> s.SubscriptionId == id && s.MessageStatus != "SENT");
    if(messages.Count() == 0)
        return Results.NotFound("No new messages");

    foreach(var msg in messages)
    {
        msg.MessageStatus = "REQUESTED";
    }
    await context.SaveChangesAsync();
    return Results.Ok(messages);
});

app.MapPost("api/subscriptions/{id}/messages", async(AppDbContext context, int id, int[] confirmations) => {
    if(confirmations.Length <= 0)
        return Results.BadRequest();
    
    int count=0;
    foreach(int i in confirmations)
    {
        var message = context.Messages.FirstOrDefault(m=> m.Id == i);
        if(message != null)
        {
            message.MessageStatus = "SENT";
            await context.SaveChangesAsync();
            count++;
        }
    }

    return Results.Ok($"Delivered {count}/{confirmations.Length} messages");
});

app.Run();