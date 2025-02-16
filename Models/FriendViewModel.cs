using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FriendSQLiteMVCVSCode.Models {
    public class FriendViewModel {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column("Firstname")]
        public string Firstname { get; set; } = "";
        [Column("Lastname")]
        public string Lastname { get; set; } = "";
        [Column("Mobile")]
        public string? Mobile { get; set; }
        [Column("Email")]
        [DisplayName("E-mail")]
        public string? Email { get; set; }
        [Column("DateOfBirth")]
        [DisplayName("Date of Birth")]
        public string? DateOfBirth { get; set; }
        [Column("RegionOfBirth")]
        [DisplayName("Region of Birth")]
        public string? RegionOfBirth { get; set; }
        [Column("PhotoFilename")]
        [DisplayName("Photo Filename")]
        public string? PhotoFilename { get; set; }
    }
}