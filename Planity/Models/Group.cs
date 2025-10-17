using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Planity.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string TeamLeaderId { get; set; }
        public ApplicationUser TeamLeader { get; set; }
        public ICollection<GroupMember> Members { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
    }
}