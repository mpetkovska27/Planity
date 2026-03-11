using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Planity.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Group name is required!")]
        [StringLength(100)]
        public string Name { get; set; }
        [StringLength(500)]
        public string Description { get; set; }
        public string TeamLeaderId { get; set; }
        [ForeignKey("TeamLeaderId")]
        public virtual ApplicationUser TeamLeader { get; set; }
        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}