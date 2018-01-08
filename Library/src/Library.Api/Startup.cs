using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Library.API.model;
using Library.API.Helpers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Formatters.Json;
using Microsoft.AspNetCore.Diagnostics;
using NLog.Extensions.Logging;

namespace Library.API
{
    public class Startup
    {
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                //esto es para los clientes del api que requieran el response en OutputFormatter diferentes a los permitidos
                setupAction.ReturnHttpNotAcceptable = true;
                //para permitir que el api retorne la data en formato xml (el default sigue siendo JSON)
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration.GetConnectionString("libraryDBConnectionString");
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();
            //services.AddAutoMapper();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {

            //asp.netcore 1.x this also work for asp.netcore 2.x
            //loggerFactory.AddConsole().AddDebug(LogLevel.Information);

            //configuracion de Nlog para almacenar los loggers en un file XML
            //loggerFactory.AddNLog();


            Mapper.Initialize((config) =>
            {
                config.CreateMap<Author, AuthorToReturn>()
                .ForMember(t => t.Age, Ex => Ex.MapFrom(t => DateTimeOffsetExtensions.GetCurrentAge(t.DateOfBirth)))
                .ForMember(t => t.Name, Ex => Ex.MapFrom(t => $"{t.FirstName} {t.LastName}"));

                config.CreateMap<Book, BookToReturn>().ReverseMap();

                config.CreateMap<AuthorDtoCreating, Author>().ReverseMap();
                config.CreateMap<BookCreationDto, Book>().ReverseMap();
                config.CreateMap<BookUpdate, Book>().ReverseMap();
                config.CreateMap<AuthorUpdate, Author>().ReverseMap();

            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler((AppBuilder) =>
                {
                    AppBuilder.Run(async context =>
                    {
                        var ExceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
                        if(ExceptionHandler != null)
                        {
                            var logger = loggerFactory.CreateLogger("general logger");
                            logger.LogError(500, ExceptionHandler.Error, 
                                ExceptionHandler.Error.Message);

                        }

                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("this is a global exception define in the StartUp CLas only for production environments");
                    });
                });
            }

            libraryContext.EnsureSeedDataForContext();

            app.UseMvc();
        }
    }
}
