using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(settings =>
{
    settings.Title = "Minimal API Demo";
    settings.Version = "v1";
    settings.Description = "Minimal API OpenAPI 整合範例";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // 開發環境下使用 Swagger
    // Add OpenAPI 3.0 document serving middleware
    // Available at: http://localhost:<port>/swagger/v1/swagger.json
    app.UseOpenApi();

    // 啟用 Swagger 網頁介面
    // Add web UIs to interact with the document
    // Available at: http://localhost:<port>/swagger
    app.UseSwaggerUi(); // UseSwaggerUI Protected by if (env.IsDevelopment())
}

app.UseFileServer(); // 啟用 wwwroot 靜態檔案伺服器

app.MapGet("/", () => "Hello World!")
    .ExcludeFromDescription(); // 從 OpenAPI 文件排除

app.MapGet("/guid", () =>
{
    return Guid.NewGuid();
})
.WithName("NewGuid")
.WithOpenApi()
// 指定作業識別碼為 NewGuid，並用預設值產生 OpenAPI 文件
.WithTags("生成函數");

app.MapGet("/randoms", (int count, int range = int.MaxValue) =>
{
    var random = new Random();
    var randoms = Enumerable.Range(0, count).Select(_ => random.Next(range));
    return randoms;
})
.WithOpenApi(op =>
{
    op.OperationId = "GetRandoms"; // 另一種指定作業識別碼的方式
    op.Summary = "產生隨機數"; // 摘要說明
    op.Description = "產生指定數量的隨機數"; // 詳細說明
    // 提供參數說明
    var pCount = op.Parameters[0];
    pCount.Description = "隨機數的數量";
    var pRange = op.Parameters[1];
    pRange.Description = "隨機數範圍(0 ~ range-1)";
    return op;
})
.WithTags("數學")
.WithTags("生成函數"); // 指定標籤

// 複雜一點的範例
// 參數及回應皆為自訂型別，檢核失敗回應 HTTP 400 並回傳 ApiError 物件
app.MapPost("workdays", (DateRange range) =>
{
    if (range.Start > range.End)
    {
        return Results.BadRequest(new ApiError
        {
            Code = 1487,
            Message = "起始日期不可大於結束日期"
        });
    }
    var workDays = new List<DateTime>();
    for (var date = range.Start.Date; date <= range.End.Date; date = date.AddDays(1))
    {
        if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
        {
            workDays.Add(date);
        }
    }
    // 傳回 HTTP 200 並回傳 WorkDaysInfo 物件
    return Results.Ok(new WorkDaysInfo
    {
        Start = range.Start,
        End = range.End,
        WorkDays = workDays.ToArray()
    });
})
// 列舉回應的型別
.Produces<WorkDaysInfo>()
.Produces<ApiError>(StatusCodes.Status400BadRequest)
.WithOpenApi(op =>
{
    op.OperationId = "GetWorkDays";
    op.Summary = "取得工作日";
    op.Description = "取得指定日期區間內的工作日";
    // 提供參數說明
    op.RequestBody.Description = "日期區間";
    // 提供回應說明
    var r200 = op.Responses["200"];
    r200.Description = "成功取得工作日";
    var r400 = op.Responses["400"];
    r400.Description = "請求失敗";
    return op;
}).WithTags("生成函數");

// 宣告回應型別共有 Ok<WorkDaysInfo> 及 BadRequest<ApiError> 兩種
app.MapPost("workdays2", Results<Ok<WorkDaysInfo>, BadRequest<ApiError>> (DateRange range) =>
{
    if (range.Start > range.End)
    {
        // 改 TypedRequests.BadRequest
        return TypedResults.BadRequest(new ApiError
        {
            Code = 1487,
            Message = "起始日期不可大於結束日期"
        });
    }
    var workDays = new List<DateTime>();
    for (var date = range.Start.Date; date <= range.End.Date; date = date.AddDays(1))
    {
        if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
        {
            workDays.Add(date);
        }
    }
    //TypedResults 主打自動依回傳型別產生 OpenAPI Metadata (可省去 Produces<T>())，並且能在編譯階段檢核回傳型別是否吻合
    return TypedResults.Ok(new WorkDaysInfo
    {
        Start = range.Start,
        End = range.End,
        WorkDays = workDays.ToArray()
    });
})
// 此處省略 Produces() 回應型別宣告
.WithOpenApi(op =>
{
    op.OperationId = "GetWorkDays";
    op.Summary = "取得工作日";
    op.Description = "取得指定日期區間內的工作日";
    // 提供參數說明
    op.RequestBody.Description = "日期區間";
    // 提供回應說明
    var r200 = op.Responses["200"];
    r200.Description = "成功取得工作日";
    var r400 = op.Responses["400"];
    r400.Description = "請求失敗";
    return op;
})
.WithTags("生成函數");

app.Run();

public class DateRange
{
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}
public class WorkDaysInfo : DateRange
{
    public DateTime[] WorkDays { get; set; } = [];
}

public class ApiError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
}
