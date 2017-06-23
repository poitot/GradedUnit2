using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace ACEDrivingSchool2.Models
{
    public class LessonViewModels
    {
        //holds data needed to display a lesson
        public class LessonViewModel
        {
            [Required]
            [DataType(DataType.DateTime)]
            public DateTime Start { get; set; }
            [Required]
            [DataType(DataType.DateTime)]
            public DateTime End { get; set; }
            public double Duration { get; set; }
            public string InstructorName { get; set; }
            public Boolean Test { get; set; }
            public string Type { get; set; }
            public double LessonCost { get; set; }
            public string StudentName { get; set; }

            //converts a lesson object into a lesson view model
            public static Expression<Func<Lesson, LessonViewModel>> ViewModel
            {
                get
                {
                    
                    return l => new LessonViewModel()
                    {
                        Start = l.Start,
                        Duration = l.End.Hour - l.Start.Hour,
                        End = l.End,
                        InstructorName = l.InstructorName,
                        Type = l.Type,
                        LessonCost = l.LessonCost
                    };
                }
            }


        }
    }
}