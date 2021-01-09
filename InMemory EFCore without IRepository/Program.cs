using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Linq;

[Table("TblStudent")]
public class Student
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	[Display(Name = "Id", Description = "Inform an integer between 1 and 99999.")]
	[Range(1, 99999)]
	public int Id { get; set; }

	[Display(Name = "Full Name", Description = "Name and Surname.")]
	[Required(ErrorMessage = "Complete name is required.")]
	[RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Numbers and Special Characteres are not allowed in Name")]
	public string Name { get; set; }

	[Required(ErrorMessage = "Email is Required")]
	[StringLength(100, MinimumLength = 5, ErrorMessage = "Email must have between 5 and 100 Characteres")]
	public string Email { get; set; }
}

[Table("TblTeacher")]
public class Teacher
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }
	[Display(Name = "Full Name", Description = "Name and Surname.")]
	[Required(ErrorMessage = "Complete name is required.")]
	[RegularExpression(@"^[a-zA-Z''-'\s]{1,40}$", ErrorMessage = "Numbers and Special Characteres are not allowed in Name")]
	public string Name { get; set; }
	[Required(ErrorMessage = "Email is Required")]
	[StringLength(100, MinimumLength = 5, ErrorMessage = "Email must have between 5 and 100 Characteres")]
	public string Email { get; set; }
}

[Table("TblTCourse")]
public class Course
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }
	public string Name { get; set; }
	[ForeignKey("TeacherRefId")]
	public Teacher Teacher { get; set; }
	private List<Student> _students = new List<Student>();

	public List<Student> Students { get => _students; set => _students = value; }
}

public class AppDbContext : DbContext, IDisposable
{
	public DbSet<Student> Students { get; set; }
	public DbSet<Teacher> Teachers { get; set; }
	public DbSet<Course> Courses { get; set; }
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public AppDbContext() { }
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseInMemoryDatabase(databaseName: "dbSchool");

	public override void Dispose() { Console.WriteLine("************** Context being disposed ************"); } // base.Dispose(); GC.SuppressFinalize(this); } 
}

public class Program
{
	public static void Main(string[] args)
	{
		using (var ctx = new AppDbContext())
		{
			ctx.Students.AddRange(new Student() { Name = "Camilo", Email = "Camilo@gmail.com" }, new Student() { Name = "Aline", Email = "Aline@gmail.com" });
			ctx.Teachers.Add(new Teacher() { Name = "Ozzy", Email = "dawg@gmail.com" });
			ctx.Courses.Add(new Course() { Name = "How to be a Rock Star" });
			ctx.SaveChanges();

			Course aCourse = (from d in ctx.Courses where d.Name == "How to be a Rock Star" select d).Single();
			aCourse.Teacher = ctx.Teachers.Single(x => x.Name == "Ozzy");
			aCourse.Students.Add(ctx.Students.Find(1));
			aCourse.Students.Add(ctx.Students.Single(x => x.Name == "Aline"));
			ctx.SaveChanges();

			Console.WriteLine(new String('-', 100));
			foreach (var student in ctx.Students) { Console.WriteLine($"Student Id: {student.Id} , Name: {student.Name} , Email: {student.Email}"); }
			foreach (var teacher in ctx.Teachers) { Console.WriteLine($"Teacher Id: {teacher.Id} , Name: {teacher.Name} , Email: {teacher.Email}"); }
			Console.WriteLine(new String('-', 100));
			foreach (var course in ctx.Courses)
			{
				Console.WriteLine($"Course Id: {course.Id}, Name: {course.Name}  Teacher: {course.Teacher.Name} ");
				Console.WriteLine("Enrolled Students:");
				foreach (var stdnt in course.Students) { Console.WriteLine($"Course: {course.Name}  Enrolled Student Name: {stdnt.Name} "); }
			}
		}

		using (var ctx = new AppDbContext())
		{
			Console.WriteLine($"Number or Students in School: {ctx.Students.Count()}");
		}
	}
}
