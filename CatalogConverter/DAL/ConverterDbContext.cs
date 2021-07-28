namespace CatalogConverter.DAL
{
    using System.Data.Entity;

    class ConverterDbContext : DbContext
    {
        static ConverterDbContext() 
        {
            Database.SetInitializer<ConverterDbContext>(null);
        }

        public ConverterDbContext() : base("OkeyDbContext")
        {
        }

        public DbSet<InventTable> InventTables { get; set; }
        public DbSet<InventItemRange> InventItemRanges { get; set; }
        /*public DbSet<ActiveAssortment> ActiveAssortments { get; set; }*/
    }
}
