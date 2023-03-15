using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;

namespace sem2_lab3;

public enum QueryResultKind
{
    SingleValue,
    MultipleSimpleValues,
    ComplexValue,
    MultipleComplexValues,
}

public static class StringHelper
{
    public static ReadOnlySpan<char> SliceOffFront(
        this ReadOnlySpan<char> span, 
        string prefix,
        StringComparison comparison = StringComparison.CurrentCulture)
    {
        if (span.StartsWith(prefix, comparison))
            return span[prefix.Length ..];
        return span;
    }
    
    public static string SliceOffFront(
        this string str, 
        string prefix,
        StringComparison comparison = StringComparison.CurrentCulture)
    {
        return SliceOffFront(str.AsSpan(), prefix, comparison).ToString();
    }
}

public static class Queries
{
    public record struct QueryInfo(string Name, QueryResultKind Kind, Func<IEnumerable<StudentModel>, object> Method);
    public static readonly QueryInfo[] QueryInfos = typeof(Queries)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Select(m =>
        {
            QueryResultKind GetKind()
            {
                var t = m.ReturnType;
                
                if (t == typeof(string))
                    return QueryResultKind.SingleValue;
                
                if (!t.IsAssignableTo(typeof(IEnumerable)))
                    return t.IsClass ? QueryResultKind.ComplexValue : QueryResultKind.SingleValue;

                var maybeSelfAsInterface = new List<Type>();
                if (t.IsInterface)
                    maybeSelfAsInterface.Add(t);
                var enumerableType = maybeSelfAsInterface.Concat(t.GetInterfaces())
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    .Select(i => i.GetGenericArguments()[0])
                    .FirstOrDefault() ?? typeof(object);

                if (enumerableType.IsClass && enumerableType != typeof(string))
                    return QueryResultKind.MultipleComplexValues;
                
                return QueryResultKind.MultipleSimpleValues;
            }

            var kind = GetKind();

            var name = m.Name
                .SliceOffFront("Get", StringComparison.OrdinalIgnoreCase)
                .Humanize(LetterCasing.Title);
            
            return new QueryInfo(name, kind, e =>
            {
                object result = m.Invoke(null, new object[] { e })!;
                switch (kind)
                {
                    case QueryResultKind.MultipleComplexValues:
                    case QueryResultKind.MultipleSimpleValues:
                        return ((IEnumerable) result).Cast<object>().ToArray();
                    
                    default:
                        return result;
                }
            });
        })
        .ToArray();
    
    private static string GetFullName(StudentModel s) => s.FirstName + " " + s.LastName;
    
    public static IEnumerable<StudentModel> GetStudentsWithHighGrade(IEnumerable<StudentModel> students)
    {
        return from student in students
            where student.AverageGrade >= 8
            select student;
    }
    
    public static IEnumerable<StudentModel> GetStudentsWithLowGrade(IEnumerable<StudentModel> students)
    {
        return from student in students
            where student.AverageGrade < 8
            select student;
    }
    
    public static IEnumerable<string> GetStudentNames(IEnumerable<StudentModel> students)
    {
        return from student in students
            select GetFullName(student);
    }

    public static string GetMostCommonStrongSubject(IEnumerable<StudentModel> students)
    {
        return (from student in students
            group student by student.StrongestSubject into g
            orderby g.Count() descending
            select g.Key).First();
    }
    
    public record StudentWithStrongestSubject(string Names, string Subject);
    public static IEnumerable<StudentWithStrongestSubject> GetStudentNamesWithStrongestSubject(IEnumerable<StudentModel> students)
    {
        return from student in students
            let fullName = GetFullName(student)
            group fullName by student.StrongestSubject into g
            orderby g.Key
            select new StudentWithStrongestSubject(string.Join(", ", g), g.Key);
        
        // return students.Select(student => new { student, fullName = GetFullName(student) })
        //     .GroupBy(@t => @t.student.StrongestSubject, @t => @t.fullName)
        //     .OrderBy(g => g.Key)
        //     .Select(g => new StudentWithStrongestSubject(string.Join(", ", g), g.Key));
    }
    
    // Query where students are joined with themselves
    public record StudentPair(string Student1Name, string Student2Name);
    public static IEnumerable<StudentPair> GetGoodStudentPairs(IEnumerable<StudentModel> students)
    {
        return from student in students
            join student2 in students
                on student.StrongestSubject equals student2.StrongestSubject
            where student != student2
            select new StudentPair(GetFullName(student), GetFullName(student2));
    }
    
    // First name must start with J
    public static IEnumerable<string> GetStudentNamesWhereFirstStartsWithJ(IEnumerable<StudentModel> students)
    {
        return from student in students
            where student.FirstName?.StartsWith("J", StringComparison.CurrentCultureIgnoreCase) ?? false
            select GetFullName(student);
    }
    
    // Last name must contain "OO"
    public static IEnumerable<string> GetStudentNamesWhereLastNameHasOO(IEnumerable<StudentModel> students)
    {
        return from student in students
            where student.LastName?.Contains("OO", StringComparison.CurrentCultureIgnoreCase) ?? false
            select GetFullName(student);
    }
    
    // Average grade must be between 5 and 7
    public static IEnumerable<string> GetStudentNamesWithAverageGradeBetween5And7(IEnumerable<StudentModel> students)
    {
        return from student in students
            where student.AverageGrade is >= 5 and <= 7
            select GetFullName(student);
    }
    
    // Students count whose names are the same
    public static int GetStudentsCountWithSameFirstName(IEnumerable<StudentModel> students)
    {
        return (from student in students
            group student by student.FirstName into g
            where g.Count() > 1
            select g).Count();
    }
    
    public record SubjectAndAverageGrade(string Subject, float AverageGrade);
    
    public static IEnumerable<SubjectAndAverageGrade> GetWeakestSubjectByGrade(IEnumerable<StudentModel> students)
    {
        return from student in students
            group student by student.StrongestSubject into g
            let gradeAverage = g.Average(s => s.AverageGrade)
            orderby gradeAverage descending
            select new SubjectAndAverageGrade(g.Key, gradeAverage);
    }
}