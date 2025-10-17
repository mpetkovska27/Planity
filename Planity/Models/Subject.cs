using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Planity.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<TaskItem> Tasks { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }
}