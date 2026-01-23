using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Planity.Models
{
    public class Subject
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Subject name is required!")]
        [Display(Name = "Subject Name")]
        public string Name { get; set; }
        [Required(ErrorMessage ="Subject Year is required!")]
        [Display(Name = "Year")]
        public int Year { get; set; }
        [Required(ErrorMessage ="Semestar is required!")]
        [Display(Name = "Semestar")]
        public int Semester { get; set; }
        [Display(Name = "Status")]
        public bool IsCompleted { get; set; }
        [Required(ErrorMessage = "Credits are required!")]
        [Range(1, 30, ErrorMessage = "Credits must be between 1 and 30")]
        [Display(Name = "ECTS Credits")]
        public int Credits { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
}