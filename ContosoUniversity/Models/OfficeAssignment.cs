using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    public class OfficeAssignment
    {
        // Can't ID this automatically; we need to explicitly state it.
        [Key]
        public int InstructorID { get; set; }

        // Forces a 50 character limit and renames the office location column name in display
        [StringLength(50)]
        [Display(Name = "Office Location")]
        public string Location { get; set; }

        public Instructor Instructor { get; set; }
    }
}