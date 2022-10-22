namespace Laborator2b;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Shared;
using static Shared.IterationHelper;

public record struct Grade(int Value);

public class StudentsGroup
{
    public string[] Subjects;
    public string[] Students;
    public Grade[][][] Grades_BySubject_ByStudent;

    public StudentsGroup(string[] subjects, string[] students, Grade[][][] grades_BySubject_ByStudent)
    {
        Subjects = subjects;
        Students = students;
        Grades_BySubject_ByStudent = grades_BySubject_ByStudent;
    }
}

public static class Queries
{
    public static int GetStudentId(this StudentsGroup g, string name)
    {
        return Array.IndexOf(g.Students, name);
    }
    
    public static int GetSubjectId(this StudentsGroup g, string name)
    {
        return Array.IndexOf(g.Subjects, name);
    }

    public static double GetAverageGrade_OfStudent_ForSubject(this StudentsGroup g, int studentId, int subjectId)
    {
        Debug.Assert(g.Students.Length > studentId && studentId >= 0);
        Debug.Assert(g.Subjects.Length > subjectId && subjectId >= 0);
        return g.Grades_BySubject_ByStudent[studentId][subjectId]
            .Select(a => a.Value)
            .Average();
    }

    public static (double AvgGrade, int SubjectId) GetMaxAverageGrade_OfStudent_AcrossSubjects(this StudentsGroup g, int studentId)
    {
        Debug.Assert(g.Students.Length > studentId && studentId >= 0);
        int maxAvgIndex = -1;
        double maxAvg = double.NegativeInfinity;

        foreach (var i in Range(0, g.Subjects.Length))
        {
            var grades = g.Grades_BySubject_ByStudent[studentId][i];
            double avg = grades.Select(g => g.Value).Average();
            if (avg > maxAvg)
            {
                maxAvgIndex = i;
                maxAvg = avg;
            }
        }
        return (maxAvg, maxAvgIndex);
    }

    public static IEnumerable<(int StudentId, int FailedSubjectId, double FailedAverageGrade)> GetFailedSubjects(this StudentsGroup g, double minimumAcceptableAverage = 4.5)
    {
        foreach (var istudent in Range(0, g.Students.Length))
        foreach (var isubject in Range(0, g.Subjects.Length))
        {
            var avg = g.Grades_BySubject_ByStudent[istudent][isubject].Select(t => t.Value).Average();
            if (avg < minimumAcceptableAverage)
                yield return (istudent, isubject, avg);
        }
    }

    public static IEnumerable<(int StudentId, int SubjectId, double AverageGrade)> GetBestStudent_BySubject(this StudentsGroup g)
    {
        foreach (var isubject in Range(0, g.Subjects.Length))
        {
            int maxAvgIndex = -1;
            double maxAvg = double.NegativeInfinity;
            foreach (var istudent in Range(0, g.Students.Length))
            {
                var avg = g.Grades_BySubject_ByStudent[istudent][isubject].Select(t => t.Value).Average();
                if (avg > maxAvg)
                {
                    maxAvgIndex = istudent;
                    maxAvg = avg;
                }
            }
            yield return (maxAvgIndex, isubject, maxAvg);
        }
    }

    public static IEnumerable<(int StudentId, int SubjectId)> GetGradeOccurence(this StudentsGroup g, Grade maxGrade)
    {
        return FlattenGrades(g)
            .Where(t => t.Grade == maxGrade)
            .Select(t => (t.StudentId, t.SubjectId));
    }

    public static (int SubjectId, double AverageGrade) GetHardestSubject(this StudentsGroup g)
    {
        int minAvgIndex = -1;
        double minAvg = double.PositiveInfinity;
        foreach (var isubject in Range(0, g.Subjects.Length))
        {
            double avg = Enumerable.Range(0, g.Students.Length)
                .SelectMany(istudent => g.Grades_BySubject_ByStudent[isubject][istudent])
                .Select(g => g.Value)
                .Average();
            if (avg < minAvg)
            {
                minAvg = avg;
                minAvgIndex = isubject;
            }
        }
        return (minAvgIndex, minAvg);
    }

    public static (int StudentId, double AverageGrade) GetWeakestStudent(this StudentsGroup g)
    {
        int minAvgIndex = -1;
        double minAvg = double.PositiveInfinity;
        foreach (var istudent in Range(0, g.Students.Length))
        {
            double avg = Enumerable.Range(0, g.Subjects.Length)
                .SelectMany(isubject => g.Grades_BySubject_ByStudent[isubject][istudent])
                .Select(g => g.Value)
                .Average();
            if (avg < minAvg)
            {
                minAvg = avg;
                minAvgIndex = istudent;
            }
        }
        return (minAvgIndex, minAvg);
    }

    public record struct FlatValue(int StudentId, int SubjectId, Grade Grade);

    public static IEnumerable<FlatValue> FlattenGrades(this StudentsGroup g)
    {
        foreach (var istudent in Range(0, g.Students.Length))
        foreach (var isubject in Range(0, g.Subjects.Length))
        foreach (var grade in g.Grades_BySubject_ByStudent[istudent][isubject])
            yield return new FlatValue(istudent, isubject, grade);
    }

    public static IEnumerable<T> WithNoResultsMessage<T>(this IEnumerable<T> query)
    {
        var e = query.GetEnumerator();
        if (!e.MoveNext())
            Console.WriteLine("No results.");

        do
        {
            yield return e.Current;
        }
        while (e.MoveNext());
    }
}


class Program
{
    static void Main()
    {
        static string[]? ReadStringArray(string countMessage)
        {
            int? count = InputHelper.GetPositiveNumber(countMessage);
            if (count is null)
                return null;
            var arr = new string[count.Value];
            foreach (var i in Range(0, arr.Length))
            {
                Console.Write($"arr[{i}] = ");
                string? input = Console.ReadLine();
                if (input is null || input == "q")
                    return null;
                arr[i] = input;
            }
            return arr;
        }

        StudentsGroup g;
        {
            string[] students;
            {
                string[]? input = ReadStringArray("Enter the number of students: ");
                if (input is null)
                    return;
                students = input;
            }
            string[] subjects;
            {
                string[]? input = ReadStringArray("Enter the number of subjects: ");
                if (input is null)
                    return;
                subjects = input;
            }
            Grade[][][] grades_BySubject_ByStudent;
            {
                var rng = new Random(69_69);
                grades_BySubject_ByStudent = new Grade[students.Length][][];
                foreach (var istudent in Range(0, students.Length))
                {
                    var t0 = grades_BySubject_ByStudent[istudent] = new Grade[subjects.Length][];

                    foreach (var isubject in Range(0, subjects.Length))
                    {
                        int count = rng.Next() % 20;
                        var t1 = t0[isubject] = new Grade[count];

                        foreach (ref var grade in t1.AsSpan())
                            grade.Value = rng.Next() % 10 + 1;
                    }
                }
            }
            g = new StudentsGroup(subjects: subjects, students: students, grades_BySubject_ByStudent);

            var queryOptionSet = new InputHelper.OptionSet(
                new HashSet<string>
                {
                    "average_grade", // GetAverageGrade_OfStudent_ForSubject
                    "max_average_grade", // GetMaxAverageGrade_OfStudent_AcrossSubjects
                    "failed_subjects", // GetFailedSubjects
                    "best_student", // GetBestStudent_BySubject
                    "max_grades", // GetMaxGrades
                    "hardest_subject", // GetHardestSubject
                    "weakest_student", // GetWeakestStudent
                });
            var studentOptionSet = new InputHelper.OptionSet(students); 
            var subjectOptionSet = new InputHelper.OptionSet(subjects);

            int? ReadStudentId()
            {
                string? studentName = InputHelper.ReadOption("Enter student name", studentOptionSet);
                if (studentName is null)
                    return null;
                int studentId = g.GetStudentId(studentName);
                return studentId;
            }

            int? ReadSubjectId()
            {
                string? subject = InputHelper.ReadOption("Enter subject name", subjectOptionSet);
                if (subject is null)
                    return null;
                int subjectId = g.GetSubjectId(subject);
                return subjectId;
            }

            while (true)
            {
                switch (InputHelper.ReadOption("Enter query to run", queryOptionSet))
                {
                    case null:
                        return;
                    case "average_grade":
                    {
                        int? studentId = ReadStudentId();
                        if (studentId is null)
                            return;
                        
                        int? subjectId = ReadSubjectId();
                        if (subjectId is null)
                            return;

                        double avg = g.GetAverageGrade_OfStudent_ForSubject(studentId.Value, subjectId.Value);
                        Console.WriteLine($"The average is {avg:##.##}");

                        break;
                    }
                    case "max_average_grade":
                    {
                        int? studentId = ReadStudentId();
                        if (studentId is null)
                            return;

                        (double avg, int subjectId) = g.GetMaxAverageGrade_OfStudent_AcrossSubjects(studentId.Value);
                        Console.WriteLine($"The maximum average is {avg:##.##} for subject {g.Subjects[subjectId]}");
                        
                        break;
                    }
                    case "failed_subjects":
                    {
                        foreach (var t in g.GetFailedSubjects().WithNoResultsMessage())
                        {
                            Console.WriteLine($"Student {g.Students[t.StudentId]} failed the subject {g.Subjects[t.FailedSubjectId]} with average grade of {t.FailedAverageGrade:##.##}.");
                        }

                        break;
                    }
                    case "best_student":
                    {
                        foreach (var t in g.GetBestStudent_BySubject().WithNoResultsMessage())
                        {
                            Console.WriteLine($"Student {g.Students[t.StudentId]} is the best at {g.Subjects[t.SubjectId]} with an average grade of {t.AverageGrade:##.##}");
                        }
                        break;
                    }
                    case "max_grades":
                    {
                        var maxGrade = new Grade(10);
                        foreach (var t in g.GetGradeOccurence(maxGrade))
                            Console.WriteLine($"The student {g.Students[t.StudentId]} in {g.Subjects[t.SubjectId]}");
                        break;
                    }
                    case "hardest_subject":
                    {
                        var t = g.GetHardestSubject();
                        Console.WriteLine($"The hardest subject is {g.Subjects[t.SubjectId]} with an average grade of {t.AverageGrade:##.##}");
                        break;
                    }
                    case "weakest_student":
                    {
                        var t = g.GetWeakestStudent();
                        Console.WriteLine($"The weakest student is {g.Students[t.StudentId]} with an average grade of {t.AverageGrade:##.##}");
                        break;
                    }
                }
            }
        }
    }
}