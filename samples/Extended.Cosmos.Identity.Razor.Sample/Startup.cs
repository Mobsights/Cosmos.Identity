using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mobsites.Cosmos.Identity;
using Extended.Cosmos.Identity.Razor.Sample.Extensions;
using Microsoft.Azure.Cosmos;

namespace Extended.Cosmos.Identity.Razor.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Register the extended storage provider, passing in setup options if any.
            // When extending the default Cosmos storage provider class "CosmosStorageProvdier", 
            // the default behavior without any setup options is to use the Azure Cosmos DB Emulator with default names for database, container, and partition key path.
            services
                .AddCosmosStorageProvider<ExtendedCosmosStorageProvider>(options =>
                {
                    //options.ConnectionString defaults to the default Azure Cosmos DB Emulator connection string, which is what is desired here for the sample.
                    options.CosmosClientOptions = new CosmosClientOptions
                    {
                        SerializerOptions = new CosmosSerializationOptions
                        {
                            IgnoreNullValues = false
                        }
                    };
                    options.DatabaseId = "ExtendedCosmosIdentity";
                    options.ContainerProperties = new ContainerProperties
                    {
                        Id = "Identity",
                        PartitionKeyPath = "/Discriminator"
                    };
                });


            // Add Cosmos Identity using the extended storage provider and extended identity models, passing in Identity options if any.
            services
                .AddCosmosIdentity<ExtendedCosmosStorageProvider, ApplicationUser, ApplicationRole, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationUserToken, ApplicationRoleClaim>(options =>
                {
                    // User settings
                    options.User.RequireUniqueEmail = true;

                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = true;

                    // Lockout settings
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                    options.Lockout.MaxFailedAccessAttempts = 5;

                })
                // Add other IdentityBuilder methods.
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            // Add Razor Pages
            services
                .AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RoleManager<ApplicationRole> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            // Add three roles.
            if (!roleManager.RoleExistsAsync("Admin").Result)
            {
                roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "Admin"
                }).Wait();
            }
            if (!roleManager.RoleExistsAsync("Employee").Result)
            {
                roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "Employee"
                }).Wait();
            }
            if (!roleManager.RoleExistsAsync("Customer").Result)
            {
                roleManager.CreateAsync(new ApplicationRole
                {
                    Name = "Customer"
                }).Wait();
            }
        }
    }
}
