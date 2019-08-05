using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/*
 * Practically the same as the Student class but for instructors instead. Only
 * added thing is a getter/setter for two new addtl. items: OfficeAssignment and CourseAssignments.
 */
namespace ContosoUniversity.Models
{
    public class Instructor : Person
    {

        // hire date for the instructor stored here
        [DataType(DataType.Date), Display(Name = "Hire Date"), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime HireDate { get; set; }

        // An instructor can have multiple CourseAssignments, hence the collection
        public ICollection<CourseAssignment> CourseAssignments { get; set; }
        
        // An instructor can only have one office
        public OfficeAssignment OfficeAssignment { get; set; }
    }
}