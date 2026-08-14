var builder = WebApplication.CreateBuilder(args);

// SignalR 서비스 추가
builder.Services.AddSignalR();

var app = builder.Build();

// 기본 파일(index.html 등)을 루트 경로(/)에서 열 수 있도록 허용
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

// SignalR 허브 매핑
app.MapHub<OmokHub>("/omokHub");

app.Run();