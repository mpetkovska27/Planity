using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Planity.Models
{
    public class Grade
    {
        [Key]
        public int Id { get; set; }
        public double Value { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}