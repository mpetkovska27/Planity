using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Planity.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public TaskType Type { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public int? SubjectId { get; set; }
        public Subject Subject { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsGroupTask { get; set; }
        public int? GroupId { get; set; }
        public Group Group { get; set; }
    }
}