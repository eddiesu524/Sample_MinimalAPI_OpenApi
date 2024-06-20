# 支援 OpenAPI 標準的 Minimal API

### .NET 的兩個主要 OpenAPI 實作是 Swashbuckle 和 NSwag
因為，在 .NET 9後好像不再支援 [Swashbuckle][Swashbuckle]，為了避免之後要修改，所以本次選擇 [NSwag][NSwag]。
>[Announcement: Swashbuckle.AspNetCore is being removed in .NET 9](https://github.com/dotnet/aspnetcore/issues/54599)

### 實作
1. 新增和設定 Swagger 中介軟體
    ```C#
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApiDocument(settings =>
    {
        settings.Title = "Minimal API Demo";
        settings.Version = "v1";
        settings.Description = "Minimal API OpenAPI 整合範例";
    });
    ```
2. 開發環境下使用 Swagger
    ```C#
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
    ```

### 參考
- [黑暗執行緒-打造支援 OpenAPI 標準的 Minimal API](https://blog.darkthread.net/blog/min-api-openapi/)
- [基本 API 應用程式中的 OpenAPI 支援](https://learn.microsoft.com/zh-tw/aspnet/core/fundamentals/minimal-apis/openapi?view=aspnetcore-8.0&WT.mc_id=DOP-MVP-37580)
- [Swagger/OpenAPI 的 ASP.NET Core Web API 文件](https://learn.microsoft.com/zh-tw/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0)



[Swashbuckle]: <https://learn.microsoft.com/zh-tw/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio> "開始使用 Swashbuckle 及 ASP.NET Core"
[NSwag]: <https://learn.microsoft.com/zh-tw/aspnet/core/tutorials/getting-started-with-nswag?view=aspnetcore-8.0&tabs=visual-studio> "開始使用 NSwag 及 ASP.NET Core"