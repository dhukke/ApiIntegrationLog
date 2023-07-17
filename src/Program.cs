using ApiIntegrationLog.Api.Database;
using ApiIntegrationLog.Api.Models;
using MongoDB.Driver;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddSingleton(serviceProvider =>
{
    var connectionString = "mongodb://localhost:27017";
    var databaseName = "apiintegrationlog";
    var logger = serviceProvider.GetRequiredService<ILogger<MongoDbContext>>();

    return new MongoDbContext(connectionString, databaseName, logger);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

const string ContentType = "application/json";
const string Tag = "Users";
const string BaseRoute = "users";

app.MapGet($"/{BaseRoute}/{{id:guid}}",
    async (MongoDbContext mongoDbContext, ILoggerFactory loggerFactory, Guid id) =>
{
    using (LogContext.PushProperty("GetUserById", id.ToString()))
    {
        var logger = loggerFactory.CreateLogger("ApiIntegrationLog.Api");

        var user = await mongoDbContext.Users.Find(x => x.Id == id).FirstOrDefaultAsync();

        if (user is null)
        {
            return Results.NotFound();
        }

        logger.LogInformation("User fetched: {@User}", user);

        return Results.Ok(user);
    }
})
.WithName("GetUser")
.Produces<User>(200).Produces(404)
.WithTags(Tag);

app.MapGet($"/{BaseRoute}",
    async (MongoDbContext mongoDbContext, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("ApiIntegrationLog.Api");

    var users = await mongoDbContext.Users.Find(_ => true).ToListAsync();

    logger.LogInformation("Fetched users: {@Users}", users);

    return users;
})
.WithName("GetUsers")
.Produces<IEnumerable<User>>(200)
.WithTags(Tag);

app.MapPost($"/{BaseRoute}", async (MongoDbContext mongoDbContext, CreateUserRequest request) =>
{
    var user = new User
    {
        Id = Guid.NewGuid(),
        Name = request.Name
    };

    await mongoDbContext.Users.InsertOneAsync(user);

    return Results.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser")
.Accepts<CreateUserRequest>(ContentType)
.Produces<User>(201)
.WithTags(Tag);

app.MapPut($"/{BaseRoute}/{{id:guid}}",
    async (MongoDbContext mongoDbContext, Guid id, UpdateUserRequest request) =>
{
    var user = await mongoDbContext.Users.Find(x => x.Id == id).FirstOrDefaultAsync();

    if (user is null)
    {
        return Results.NotFound();
    }

    user.Name = request.Name;

    var filter = Builders<User>.Filter.Eq(nameof(User.Id), id);
    var update = Builders<User>.Update.Set(nameof(User.Name), request.Name);

    await mongoDbContext.Users.UpdateOneAsync(filter, update);

    return Results.Ok(user);
})
.WithName("UpdateUser")
.Accepts<UpdateUserRequest>(ContentType)
.Produces<User>(200)
.WithTags(Tag);

app.MapDelete($"/{BaseRoute}/{{id:guid}}", async (MongoDbContext mongoDbContext, Guid id) =>
{
    var deleteResult = await mongoDbContext.Users.DeleteOneAsync(x => x.Id == id);

    return deleteResult.DeletedCount > 0 ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteUser")
.Produces(204)
.Produces(404)
.WithTags(Tag);

app.Run();

public record CreateUserRequest(string Name);

public record UpdateUserRequest(string Name);