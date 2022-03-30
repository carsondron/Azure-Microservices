using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Grpc.Protos;
using Orders.Application.Behaviours;
using Orders.Application.GrpcServices;
using Orders.Application.GrpcServices.Interfaces;
using System.Reflection;

namespace Orders.Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            services.AddGrpcClient<NotificationsProtoService.NotificationsProtoServiceClient>(
                o => o.Address = new Uri(configuration["GrpcSettings:NotificationsUrl"])
            );

            services.AddScoped<INotificationsGrpcService, NotificationsGrpcService>();

            return services;
        }
    }
}