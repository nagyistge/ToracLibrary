namespace ToracLibraryTest.UnitsTest.EntityFramework.DataContext
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Ref_Test
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Description2 { get; set; }

        public DateTime? CreateDate { get; set; }

        public bool? BooleanTest { get; set; }

        public int? NullId { get; set; }

        public int? SubObjectId { get; set; }

        public virtual Ref_SubObject Ref_SubObject { get; set; }
    }
}