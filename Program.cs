using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text;
using System.Text.Json;


var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var baseUrl    = config["Acumatica:BaseUrl"]!;
var endpoint   = config["Acumatica:Endpoint"]!;
var version    = config["Acumatica:Version"]!;
var apiBase    = $"{baseUrl}/entity/{endpoint}/{version}";
var defaultApi = $"{baseUrl}/entity/Default/25.200.001";

var cookieContainer = new CookieContainer();
var handler = new HttpClientHandler { CookieContainer = cookieContainer };
using var client = new HttpClient(handler);
client.DefaultRequestHeaders.Add("Accept", "application/json");

await RunAsync();

async Task RunAsync()
{
    try
    {
        await LoginAsync();
        Pause("Presiona Enter para obtener las últimas 5 órdenes en Hold...");

        var orders = await GetOrdersOnHoldAsync();
        var targets = orders.Take(2).ToArray();
        Pause("Presiona Enter para actualizar las 2 primeras órdenes...");

        await UpdateOrdersAsync(targets);
        Pause("Ve a Acumatica y confirma que las órdenes siguen en On Hold. Presiona Enter para ejecutar Remove Hold...");

        await ReleaseHoldAsync(targets);
        Pause("Ve a Acumatica, refresca y verifica que las órdenes cambiaron a Abierto. Presiona Enter para cerrar sesión...");
    }
    finally
    {
        await LogoutAsync();
    }
}

async Task LoginAsync()
{
    PrintStep("1. LOGIN");

    var body = Serialize(new
    {
        name     = config["Acumatica:Username"],
        password = config["Acumatica:Password"],
        company  = config["Acumatica:Company"],
        branch   = config["Acumatica:Branch"]
    });

    var response = await client.PostAsync($"{baseUrl}/entity/auth/login", Body(body));
    EnsureSuccess(response, "Login");
    PrintSuccess("Sesión iniciada correctamente.");
}

async Task<SalesOrder[]> GetOrdersOnHoldAsync()
{
    PrintStep("2. ÚLTIMAS 5 ÓRDENES EN HOLD");

    var response = await client.GetAsync(
        $"{apiBase}/SalesOrder?$top=5&$filter=Hold eq true&$select=OrderNbr,OrderType,Status,Description,Hold"
    );
    EnsureSuccess(response, "GET SalesOrders");

    var json   = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<JsonElement[]>(json) ?? [];

    if (result.Length == 0)
        Abort("No se encontraron órdenes en estado On Hold.");

    var orders = result.Select(o => new SalesOrder(
        Id:        o.GetProperty("id").GetString()!,
        OrderNbr:  o.GetProperty("OrderNbr").GetProperty("value").GetString()!,
        OrderType: o.GetProperty("OrderType").GetProperty("value").GetString()!,
        Status:    o.GetProperty("Status").GetProperty("value").GetString()!
    )).ToArray();

    foreach (var o in orders)
        Console.WriteLine($"  {o.OrderNbr} | Tipo: {o.OrderType} | Status: {o.Status}");

    Console.WriteLine();
    return orders;
}

async Task UpdateOrdersAsync(SalesOrder[] orders)
{
    PrintStep("3. ACTUALIZAR DESCRIPCIÓN");

    foreach (var order in orders)
    {
        var body = Serialize(new
        {
            OrderType   = new { value = order.OrderType },
            OrderNbr    = new { value = order.OrderNbr },
            Description = new { value = $"Actualizado via API - PTClandBus [{DateTime.Now:yyyy-MM-dd HH:mm}]" }
        });

        var response = await client.PutAsync($"{apiBase}/SalesOrder", Body(body));
        EnsureSuccess(response, $"PUT {order.OrderNbr}");
        PrintSuccess($"{order.OrderNbr} actualizada.");
    }

    Console.WriteLine();
}

async Task ReleaseHoldAsync(SalesOrder[] orders)
{
    PrintStep("4. REMOVE HOLD");

    foreach (var order in orders)
    {
        var body     = Serialize(new { entity = new { id = order.Id } });
        var response = await client.PostAsync($"{defaultApi}/SalesOrder/ReleaseFromHold", Body(body));
        EnsureSuccess(response, $"ReleaseFromHold {order.OrderNbr}");
        PrintSuccess($"{order.OrderNbr} liberada. Status: Abierto.");
    }

    Console.WriteLine();
}

async Task LogoutAsync()
{
    PrintStep("5. LOGOUT");
    var response = await client.PostAsync($"{baseUrl}/entity/auth/logout", null);
    EnsureSuccess(response, "Logout");
    PrintSuccess("Sesión cerrada correctamente.");
    Console.WriteLine("\nProceso completado.");
}

void EnsureSuccess(HttpResponseMessage response, string operation)
{
    if (response.IsSuccessStatusCode)
        return;

    var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ERROR en {operation} - Status {(int)response.StatusCode}: {body}");
    Console.ResetColor();
    throw new Exception($"{operation} falló con status {response.StatusCode}.");
}

void PrintStep(string title)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n{title}");
    Console.ResetColor();
}

void PrintSuccess(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  {message}");
    Console.ResetColor();
}

void Pause(string message)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"\n{message}");
    Console.ResetColor();
    Console.ReadLine();
}

void Abort(string message)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ResetColor();
    Environment.Exit(1);
}

StringContent Body(string json) =>
    new(json, Encoding.UTF8, "application/json");

string Serialize(object obj) =>
    JsonSerializer.Serialize(obj);

record SalesOrder(string Id, string OrderNbr, string OrderType, string Status);
