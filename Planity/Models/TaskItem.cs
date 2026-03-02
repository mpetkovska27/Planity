using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Planity.Models
{
    public enum TaskType
    {
        Lesson, Auditory, Homework, Lab, Project, Exam
    }

    public enum Priority
    {
        Low, Medium, High
    }

    public enum TaskStatus
    {
        ToDo, InProgress, Done, Overdue
    }
    public enum Repeat
    {
        None, Daily, Weekdays, Weekly, Monthly, Yearly
    }


    public class TaskItem
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Title is required!")]
        [Display(Name = "Task Name")]
        [StringLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public TaskType Type { get; set; }
        [Required]
        public Priority Priority { get; set; }
        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.ToDo;
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? DueDate { get; set; }
        [Display(Name = "Repeat")]
        public Repeat? Repeat { get; set; }
        //Nullable ja stavame za da ne mora da bide od nekoj predmet
        public int? SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; }
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public bool IsGroupTask { get; set; }
        public int? GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
        public int? ParentTaskId { get; set; }
        [ForeignKey("ParentTaskId")]
        public virtual TaskItem ParentTask { get; set; }
        public virtual ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();
        public string AttachedFilePath { get; set; }
    }
}