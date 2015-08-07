namespace ToracLibraryTest.UnitsTest.EntityFramework.DataContext
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class EntityFrameworkEntityDP : DbContext
    {
        public EntityFrameworkEntityDP()
            : base("name=EntityFrameworkEntityDP")
        {
        }

        public virtual DbSet<Ref_Test> Ref_Test { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ref_Test>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Ref_Test>()
                .Property(e => e.Description2)
                .IsUnicode(false);
        }
    }
}
