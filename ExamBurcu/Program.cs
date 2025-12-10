using ExamBurcu.Data;
using ExamBurcu.Interfaces;
using ExamBurcu.Services;
using Microsoft.EntityFrameworkCore;
using VaccineExam.Repository;
using VaccineExam.UnitOfWork;
using WebApplication1.AutoMapper;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));


        // Redis (IDistributedCache icin StackExchange Redis kullanimi)
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration["Redis:ConnectionString"];
            options.InstanceName = builder.Configuration["Redis:InstanceName"];
        });


        builder.Services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddAutoMapper(typeof(AutoMappingProfile));
        builder.Services.AddComponents();
         
        var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(",") ?? new[] { "*" };
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials(); // <-- EN ÖNEMLÝ EKLEME: SignalR için zorunlu
            });
        });

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<UnitOfWorkTransactionFilter>();
        });


        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            //app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vaccine API"); });
        }

        app.UseCors();
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}

public static class RegisterComponents
{
    public static void AddComponents(this IServiceCollection services)
    {
        #region Main Module Services
        services.AddTransient<IVaccineService, VaccineService>();
        services.AddTransient<IVaccineScheduleService, VaccineScheduleService>();
        services.AddTransient<IVaccineApplicationService, VaccineApplicationService>();
        services.AddTransient<IChildService, ChildService>();
        services.AddTransient<IDoctorService, DoctorService>();

        services.AddScoped<IExcelService, ExcelService>();

        //services.AddScoped<IAccountService, AccountService>();
        //services.AddScoped<IUserService, UserService>();
        //services.AddScoped<IAccountService, AccountService>();
        //services.AddScoped<IAuditService, AuditService>();
        //services.AddScoped<IOutboxMessageService, OutboxMessageService>();
        //services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        #endregion
    }
}