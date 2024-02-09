using System.Reflection;
using Model;

namespace consoleTest;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void AccessingPrivateMethods_ThroughReflection()
    {
        //Arrange
        var person = new Person("Anakin", "vader@deathstar.com", 40);

        //Act
        PropertyInfo ageInfo = person!.GetType()
                                          .GetProperty
                                                ("Age",
                                                BindingFlags.NonPublic |
                                                BindingFlags.Instance)!;

        PropertyInfo nameInfo = person!.GetType()
                                         .GetProperty
                                               ("Name",
                                               BindingFlags.NonPublic |
                                               BindingFlags.Instance)!;

        PropertyInfo emailInfo = person!.GetType()
                                         .GetProperty
                                               ("Email",
                                               BindingFlags.NonPublic |
                                               BindingFlags.Instance)!;

        //Assert
        Assert.IsTrue((int)ageInfo.GetValue(person)! == 40);
        Assert.IsTrue((string)nameInfo.GetValue(person)! == "Anakin");
        Assert.IsTrue((string)emailInfo.GetValue(person)! == "vader@deathstar.com");
    }

    [TestMethod]
    public void AccessingPrivateMethods_ThroughInternalsVisible()
    {
        //Arrange
        /*
         go to the obj folder and look for AssemblyInfo.cs and add the line below
         [assembly: InternalsVisibleTo("consoleTest")]
        */

        var person = new Person("Anakin", "vader@deathstar.com", 40);

        //Assert
        Assert.IsTrue(person.Age == 40);

    }
}