using httpclient_signalr_app.Hub;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Polly;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
        });
});

//configuring signalR, HttpClient and Policy
builder.Services.AddSignalR();
builder.Services.AddHttpClient("client", client =>
{
    client.BaseAddress = new Uri("https://random-data-api.com/api/v2/");
})
.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(5, _ => TimeSpan.FromSeconds(2)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<MessageHub>("/hub");
});

//adding global exception middleware
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();


        if (contextFeature != null)
        {
            await context.Response.WriteAsync(new ErrorDetailsModel()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error."
            }.ToString());
        }
    });
});

app.MapGet("/banks", async ([FromQuery(Name = "size")] int size, IHttpClientFactory _httpClientFactory, IHubContext<MessageHub, IMessageHubClient> _messageHub) =>
{
    var httpClient = _httpClientFactory.CreateClient("client");

    var message = $"There are {size} new banks loaded.";
    await _messageHub.Clients.All.InformAboutUpdatedBanks(message);

    return await httpClient.GetFromJsonAsync<List<Bank>>($"banks?size={size}");
})
.WithName("GetBanks")
.WithOpenApi();

app.MapGet("/credit-cards", async ([FromQuery(Name = "size")] int size, IHttpClientFactory _httpClientFactory, IHubContext<MessageHub, IMessageHubClient> _messageHub) =>
{
    var httpClient = _httpClientFactory.CreateClient("client");

    var message = $"There are {size} new credit cards loaded.";
    await _messageHub.Clients.All.InformAboutUpdatedCreditCards(message);

    return await httpClient.GetFromJsonAsync<List<CreditCard>>($"credit_cards?size={size}");
})
.WithName("GetCreditCards")
.WithOpenApi();

app.Run();

internal record Bank
{
    public int? Id { get; set; }

    public Guid? Uid { get; set; }

    public string? Account_number { get; set; }

    public string? Iban { get; set; }

    public string? Bank_name { get; set; }

    public string? Routing_number { get; set; }

    public string? Swift_bic { get; set; }
}

internal record CreditCard
{
    public int? Id { get; set; }

    public Guid? Uid { get; set; }

    public string? Credit_card_number { get; set; }

    public DateTime? Credit_card_expiry_date { get; set; }

    public string? Credit_card_type { get; set; }
}

internal class ErrorDetailsModel
{
    public int? StatusCode { get; set; }
    public string? Message { get; set; }
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}