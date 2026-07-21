namespace TaxWebAppCore8.Models.SharedDTO.BaranDB
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Store")]
    public partial class Store
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Store()
        {
            Store1 = new HashSet<Store>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int StoreID { get; set; }

        public int? ParentID { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        public Guid? PartyID { get; set; }

        [StringLength(50)]
        public string BusinessCode { get; set; }

        [StringLength(50)]
        public string Comment { get; set; }

        public Guid? DetailedAccountID { get; set; }

        public int? StatusID { get; set; }

        public byte[] StoreImage { get; set; }

        public DateTime? LastSynchTime { get; set; }

        public Guid? GroupID { get; set; }

        public DateTime? CreationDate { get; set; }

        public Guid? CreationUser { get; set; }

        public DateTime? ModifyDate { get; set; }

        public Guid? ModifyUser { get; set; }

        public decimal? MaxUsageAmount { get; set; }

        public int? TypeID { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        [StringLength(10)]
        public string CompanyCode { get; set; }

        public byte? Grade { get; set; }

        public Guid? ManagerID { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Store> Store1 { get; set; }

        public virtual Store Store2 { get; set; }
    }
}
