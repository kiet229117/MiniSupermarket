var builder = WebApplication.CreateBuilder(args);

// Thêm Controllers và dịch vụ Swagger Gen
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Bắt buộc phải có dòng này

var app = builder.Build();

// Cấu hình sử dụng Swagger ở môi trường phát triển
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Bắt buộc phải có dòng này để bật giao diện web
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
