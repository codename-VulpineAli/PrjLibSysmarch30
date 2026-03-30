namespace prjLibrarySystem.Models
{
    public class Member
    {
        public int MemberID { get; set; }
        public string UserID { get; set; }
        public string FullName { get; set; }
        public string MemberType { get; set; }  // 'Student' or 'Teacher'

        // Course is now a FK to tblCourses — store both ID and name
        // NULL for Teachers
        public int? CourseID { get; set; }
        public string CourseName { get; set; }

        // YearLevel is now a FK to tblYearLevels — store both ID and name
        // NULL for Teachers
        public int? YearLevelID { get; set; }
        public string YearLevelName { get; set; }

        // IsActive lives in tblUsers, not tblMembers
        public string Email { get; set; }
        public bool IsActive { get; set; }

        public bool IsStudent => MemberType == "Student";
        public bool IsTeacher => MemberType == "Teacher";
    }
}