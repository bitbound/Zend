using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zend.Models;

namespace Zend.Services
{
    public class AppDb : DbContext
    {
        public AppDb(DbContextOptions options)
            : base(options)
        {

        }

        public DbSet<SavedFile> SavedFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            {
                // SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
                // here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
                // To work around this, when the SQLite database provider is used, all model properties of type DateTimeOffset
                // use the DateTimeOffsetToBinaryConverter
                // Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
                // This only supports millisecond precision, but should be sufficient for most use cases.
                foreach (var entityType in builder.Model.GetEntityTypes())
                {
                    if (entityType.IsKeyless)
                    {
                        continue;
                    }
                    var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
                                                                                || p.PropertyType == typeof(DateTimeOffset?));
                    foreach (var property in properties)
                    {
                        builder
                             .Entity(entityType.Name)
                             .Property(property.Name)
                             .HasConversion(new DateTimeOffsetToStringConverter());
                    }
                }
            }
            base.OnModelCreating(builder);
        }
    }
}
