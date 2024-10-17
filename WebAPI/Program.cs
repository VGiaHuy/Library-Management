using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Helper;
using WebAPI.Models;
using WebAPI.Service;
using WebAPI.Service_Admin;
using Mapper = AutoMapper.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Thêm DistributedMemoryCache để sử dụng làm bộ nhớ đệm
builder.Services.AddDistributedMemoryCache();

// Thêm dịch vụ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true; // Cookie chỉ có thể truy cập qua HTTP
    options.Cookie.IsEssential = true; // Cookie cần thiết cho ứng dụng
});

// Đăng ký dịch vụ Email
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// đăng ký database
builder.Services.AddDbContext<QuanLyThuVienContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("QuanLyThuVien")), ServiceLifetime.Transient);
// đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Đăng ký dịch vụ cho IMapper
builder.Services.AddScoped<IMapper, Mapper>();

builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ThongKeService, ThongKeService>();
builder.Services.AddTransient<PhieuTraService, PhieuTraService>();
builder.Services.AddTransient<QuanLyPhieuTraService, QuanLyPhieuTraService>();
builder.Services.AddTransient<QuanLyPhieuMuonService, QuanLyPhieuMuonService>();
builder.Services.AddTransient<PhieuMuonService, PhieuMuonService>();
builder.Services.AddTransient<KhoSachService, KhoSachService>();
builder.Services.AddTransient<NhapSachService, NhapSachService>();
builder.Services.AddTransient<ThanhLySachService, ThanhLySachService>();
builder.Services.AddTransient<TheDocGiaService, TheDocGiaService>();
builder.Services.AddTransient<ThongTinDocGiaService, ThongTinDocGiaService>();
builder.Services.AddTransient<AccountService, AccountService>();
builder.Services.AddTransient<DangKyMuonSachService, DangKyMuonSachService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSession();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
