namespace Model;

public class Person
{
    public Person(string name, string email, int age)
    {
        this.Age = age;        
        this.Name = name;
        this.Email = email;
    }
    internal int Age { get; set; }
    private string Name { get; set; } = String.Empty;
    protected string Email { get; set; } = String.Empty;
    public void SaySomething(string msg) => Console.WriteLine(msg);
    private string Greet(string name) => $"Hello {name}";

}