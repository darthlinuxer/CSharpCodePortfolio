Full Public Git: https://github.com/darthlinuxer
# C# Code Portfolio

<details>
<summary>Collection of C# Codes in Fiddle</summary>
My Collection of C# Codes in [Fiddle](https://dotnetfiddle.net/)

|                                     C# Working Examples                                      |                         Learning                         |                          Concepts                           |
| :------------------------------------------------------------------------------------------: | :------------------------------------------------------: | :---------------------------------------------------------: |
|                           [Simplest .NET Google SMTP Sender][1.1]                            |           [Solid: Open Closed Principle][1.2]            |               [Expression Tree Example][1.3]                |
|                      [Dependency Injection .NET - Using Services][2.1]                       | [Solid: Violation of Liskov Substitution Principle][2.2] |        [Culture Effect on String Manipulation][2.3]         |
|                                   [.NET Zip Library][3.1]                                    |             [Design Pattern: Singleton][3.2]             |      [Read/Write Binary data from/to File/Memory][3.3]      |
|                       [Complete IRepository with InMemory EFCore][4.1]                       |              [Design Pattern: Facade][4.2]               |                 [Boxing and Unboxing][4.3]                  |
|                          [InMemory EFCore without IRepository][5.1]                          |              [Design Pattern: Bridge][5.2]               |           [Explicit and Implicit Operators][5.3]            |
|                   [InMemory EFCore without IRepository with Services][6.1]                   |             [Design Pattern: Strategy][6.2]              |                 [Deconstruct Methods][6.3]                  |
| [Dependency Inversion using Dependency Injection Services for Constructor and Property][7.1] |             [Design Pattern: Observer][7.2]              |              [Standard Interface Methods][7.3]              |
|                          [UNIT Tests with Reflection and Moq][8.1]                           |       [Design Pattern: Observer using Events][8.2]       |                   [Using Async Main][8.3]                   |
|                          [Client/Server Socket Communication][9.1]                           |            [Solid: Dependency Inversion][9.2]            |                     [Using Ranges][9.3]                     |
|               [Using Anonymous Pipes for Communication between Threads][10.1]                |      [Solid: Interface Segregation Principle][10.2]      | [ Extension Methods combined with Base64 conversions][10.3] |
|                          [Json Serializer and De-Serializer][11.1]                           |                                                          |                [Delegates and Events][11.3]                 |
|                                    [SQLite in .NET][12.1]                                    |                                                          |                   [Secure Strings][12.3]                    |
|                             [Convert List<T> to DataTable][13.1]                             |                                                          |                     [Exceptions][13.3]                      |
|                                  [Lambda Validators][14.1]                                   |                                                          |                        [Yield][14.3]                        |
|              [Creating string Pipes using reverse PipeBuilder recursion][15.1]               |                                                          |        [Return Option<> for safe null checks][15.3]         |
|                 [Replace If-Then-Else with Reflection and Attributes][16.1]                  |                                                          |
|       [Replacing If-Then-Else using Pipe Builder structure (simple input types)][17.1]       |                                                          |
|           [Replacing If-Then-Else for complex objects using Pipe Structure][18.1]            |                                                          |
|             [Replace If-Then-Else for complex objects (using Reflection)][19.1]              |                                                          |
|                            ! [BackgroundTasks with Timeout][20.1]                            |                                                          |
</details>

## Interview Questions in .NET C#

<details>
<summary>Basic Questions</summary>


<details>
<summary> What is the difference between C# and .NET ?  </summary>  

- .NET is a Framework , C# is a programming language <br>
- .NET is a collection of libraries and it has a runtime
</details>

<details>
<summary>Differentiate between .NET Framework 4.x, .NET Core 3.x, .NET 5 and above></summary>

- .NET Framework is ONLY for Windows, it is slow as compared to .NET Core (packaged as one big framework), it is desktop based with WPF and Winforms, does not support microservices <br>
- .NET Core 3.x is Cross Platform, has better performance (libraries are more modular and smaller in sizes, delivered via nuget), it is not desktop based, supports microservices, Full CLI command supported <br>
.NET 5> is the evolution of .NET Core , provides a uniform platform that unifies all .NET, it is multiplatform, developers no longer have to choose which platform they´re developing their applications <br>

```
.NET and .NET Core have better performance because it has divided large DLLs (libraries) into smaller specialized libraries so that the program can run only what is really necessary; e.g. what was previously a big System.Collections now has a span of options: .Concurrent, .Specialized, .Immutable
```
</details>

<details>
<summary>What is IL and what is the use of JIT?</summary>

- Intermediate Language (IL): When you compile your C# code, the compiler reads your source code and produces Microsoft Intermediate Language (MSIL), sometimes abbreviated as IL. This is a CPU-independent set of instructions that can be efficiently converted to native code. IL is a lower-level language than C#, but it’s still higher-level than machine code. It’s used by the .NET Framework to generate machine-independent code as the output of the compilation of the source code written in any .NET programming language.

- Just-In-Time (JIT) Compiler: The JIT compiler is a part of the Common Language Runtime (CLR) in .NET, which is responsible for managing the execution of .NET programs. The JIT compiler translates the MSIL code of an assembly into native code, specific to the computer environment that the JIT compiler runs on. This translation is done on a requirement basis, meaning the JIT compiler compiles the MSIL as required rather than compiling all of it at once. The compiled MSIL is stored so that it is available for subsequent calls. This process helps to speed up the code execution and provide support for multiple platforms.

```
In summary, when you write and compile C# code in the .NET environment, the code is first turned into IL. Then, when the program is run, the JIT compiler turns the IL into machine code that can be executed by the computer’s processor. This two-step process allows .NET to provide a high level of abstraction and portability, while still achieving good performance.
Because of IL, .NET supports multiple languagues: C#, F#, VB
```
</details>
<details>
<summary>What is CLR and why it is important ?</summary>
<br>
The Common Language Runtime (CLR) is a crucial component of the .NET Framework. It manages the execution of .NET applications and provides several important services  

- Managed Execution Environment: CLR provides a managed execution environment for .NET programs, regardless of the .NET programming language used12. This includes C#, VB.NET, F#, and others
- Memory Management: CLR handles memory allocation and deallocation for .NET applications. It automatically manages object layout and releases objects when they’re no longer being used
- Garbage Collection: CLR includes a garbage collector that automatically reclaims memory occupied by unused objects, eliminating common programming errors like memory leaks
- Type Safety: CLR ensures that code only accesses the memory locations it is authorized to access
- Exception Handling: CLR provides a framework for exception handling, allowing errors to be caught and handled in a structured manner14.
- Security: CLR provides a security model to protect resources from unauthorized access
- Just-In-Time (JIT) Compilation: CLR compiles the Microsoft Intermediate Language (MSIL) code into machine code on the fly as the program runs, optimizing performance
- Cross-Language Integration: CLR makes it easy to design components and applications whose objects interact across languages
```
Overall, CLR is responsible for ensuring that .NET applications are executed in a safe, secure, and efficient manner, making it a fundamental aspect of .NET programming
```
</details>

<details>
<summary>What is managed and unmanaged code ?</summary>
In the context of .NET and C#:

- **Managed Code**: This is code that is written to be managed by the Common Language Runtime (CLR) in the .NET Framework¹². Managed code is compiled into an intermediate language (MSIL), which is then executed by the CLR¹². The CLR provides various services to the managed code such as garbage collection, type checking, exception handling, bounds checking, and more². Managed code provides platform independence, improved security, automatic memory management, and easier debugging¹².

- **Unmanaged Code**: This is code that is directly executed by the operating system¹². Unmanaged code is compiled to native code that is specific to the architecture². It provides low-level access to the programmer and direct access to system resources¹². However, unmanaged code does not provide runtime services like garbage collection, exception handling, etc., and memory management is handled by the programmer¹². Debugging unmanaged code can be harder due to the lack of debugging tools¹.

In summary, managed code is controlled by the CLR and provides various benefits like automatic memory management and improved security, while unmanaged code is executed directly by the operating system and provides low-level access to the programmer¹².

Source:<br>
(1) Difference between Managed and Unmanaged code in .NET. https://www.geeksforgeeks.org/difference-between-managed-and-unmanaged-code-in-net/. <br>
(2) Managed code and Unmanaged code in .NET - GeeksforGeeks. https://www.geeksforgeeks.org/managed-code-and-unmanaged-code-in-net/. <br>
(3) Interoperating with unmanaged code - .NET Framework. https://learn.microsoft.com/en-us/dotnet/framework/interop/.<br>
(4) Managed and Unmanaged Code - Key Differences - ParTech. https://www.partech.nl/en/publications/2021/03/managed-and-unmanaged-code---key-differences. <br>
</details>

<details>
<summary>What is the importance of CTS ?</summary>

```
Basically: CTS ensures that data types defined in 2 different languages gets compiled to a common data type in IL
```

The Common Type System (CTS) is a fundamental component of the .NET framework and plays a crucial role in ensuring interoperability between different programming languages that target the .NET framework¹²³⁴⁵. Here are some key points about its importance:

1. **Cross-Language Integration**: CTS establishes a framework that enables cross-language integration¹. It ensures that objects written in different languages can interact with each other¹.

2. **Type Safety**: CTS provides a set of rules that all programming languages must follow when creating data types³. This ensures type safety, meaning that the code only accesses the memory locations it is authorized to access¹.

3. **High-Performance Code Execution**: By defining how types are declared, used, and managed in the runtime, CTS facilitates high-performance code execution².

4. **Standard Set of Data Types**: CTS represents a standard set of data types that can be used across all programming languages running on the .NET Framework³. This ensures that all languages using the .NET Framework can communicate and understand the same data types³.

5. **Object-Oriented Model**: CTS provides an object-oriented model that supports the complete implementation of many programming languages¹.

In summary, CTS is essential for ensuring language independence, type safety, and efficient code execution in the .NET environment¹²³⁴⁵.

Source:<br>
(1) Common Type System - .NET | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/standard/base-types/common-type-system. <br>
(2) What Are CTS And CLS In .NET - C# Corner. https://www.c-sharpcorner.com/blogs/what-are-cts-and-cls-in-net. <br>
(3) Exploring The Key Components Of .NET - CLR, CTS, And CLS - C# Corner. https://www.c-sharpcorner.com/article/exploring-the-key-components-of-net-clr-cts-and-cls/. <br>
(4) What is CTS in Dot Net core - C# Corner. https://www.c-sharpcorner.com/interview-question/what-is-cts-in-dot-net-core. <br>
(5) What is Common Type System (CTS) In .Net - Medium. https://nalawadeshivani98.medium.com/what-is-common-type-system-cts-in-net-cf56ba82fef. <br>



</details>

<details>
<summary>Explain the importance of CLS ?</summary>

```
Basically: CLS is a set of rules or guidelines that a language has to follow in order to be consumed by .NET
```
The Common Language Specification (CLS) is a key component of the .NET framework and plays a vital role in ensuring interoperability between different programming languages that target the .NET framework¹²⁴⁵⁶. Here are some key points about its importance:

1. **Interoperability**: CLS defines a set of rules that every .NET language must follow, which enables smooth communication between different .NET supported programming languages¹²⁴⁵⁶.

2. **Cross-Language Integration**: CLS ensures that language specifications defined in two different languages get compiled into a common language specification¹. This allows for cross-language integration or interoperability²⁴⁵⁶.

3. **Common Rules**: CLS defines some set of rules that must be followed by each .NET language to be a .NET compliant language²³⁵. These rules enable different .NET languages to use each other’s framework class library for application development³⁵.

4. **Language Independence**: The language specification of CLR is common for all programming languages and this is known as Common Language Specifications (CLS)¹. This helps in supporting language independence in .NET².

In summary, CLS is essential for ensuring language independence, interoperability, and efficient code execution in the .NET environment¹²⁴⁵⁶.

Source: <br>
(1) Common Language Specification in .NET - Dot Net Tutorials. https://dotnettutorials.net/lesson/common-language-specification/. <br>
(2) What Are CTS And CLS In .NET - C# Corner. https://www.c-sharpcorner.com/blogs/what-are-cts-and-cls-in-net. <br>
(3) CLS in .Net Framework: What is Common Language Specification?. https://www.webtrainingroom.com/dotnetframework/cls. <br>
(4) What are CTS and CLS In .NET? - Includehelp.com. https://www.includehelp.com/dot-net/define-cls-and-cts.aspx. <br>
(5) Common Language Specification (CLS)) - Computer Notes. https://ecomputernotes.com/csharp/dotnet/common-language-specification. <br>
(6) What are CTS and CLS In .NET? - Includehelp.com. https://bing.com/search?q=Importance+of+CLS+in+.NET. <br>
</details>
<details>
<summary>What is the difference between STACK and HEAP ?</summary>

```
Stack and Heap are memory types in an application. Stack memory stores datatypes like int, double, boolean etc.. while Heap store data types like strings, objects, arrays, etc..
```

- "Things" declared with the following list of type declarations are Value Types (because they are from System.ValueType):
bool, byte, char, decimal, double, enum, float, int, long, sbyte, short, struct, uint, ulong, ushort
- "Things" declared with following list of type declarations are Reference Types (and inherit from System.Object... except, of course, for object which is the System.Object object): class, interface, delegate, object, string

| Category          | Stack                                                            | Heap                                                              |
| ----------------- | ---------------------------------------------------------------- | ----------------------------------------------------------------- |
| Memory Allocation | Static, stored directly, variables can´t be resized, fast access | Dynamic, stored indirectly, variables can be resized, slow access |
| Visibility        | visible to the owner thread only                                 | visible to all threads                                            |
| When wiped out ?  | Local variables get wiped off once they loose the scope          | when collected by the garbage collector                           |

</details>
  
<details>
<summary> What is the concept of boxing and unboxing ?</summary>
In the context of C# and .NET:

- **Boxing**: Boxing is the process of converting a value type to a reference type¹². When the Common Language Runtime (CLR) boxes a value type, it wraps the value inside a System.Object instance and stores it on the managed heap¹². Boxing is an implicit conversion process¹². Here's an example of boxing:

```csharp
int i = 123; // The following line boxes i.
object o = i;
```

- **Unboxing**: Unboxing is the process of converting a reference type back into a value type¹². Unboxing extracts the value type from the object¹². Unboxing is an explicit conversion process¹². Here's an example of unboxing:

```csharp
object o = 123;
int i = (int)o; // unboxing
```

In summary, boxing and unboxing allow value types to be treated as objects, providing a unified view of the type system¹². However, they are computationally expensive processes. When a value type is boxed, a new object must be allocated and constructed. The cast required for unboxing is also computationally expensive¹.

Source: <br>
(1) Boxing and Unboxing - C# Programming Guide - C# | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing. <br>
(2) C# | Boxing And Unboxing - GeeksforGeeks. https://www.geeksforgeeks.org/c-sharp-boxing-unboxing/. <br>
(3) Boxing and Unboxing in C# - C# Corner. https://www.c-sharpcorner.com/article/boxing-and-unboxing-in-C-Sharp/. <br>
</details>

<details>
<summary>Explain casting, implicit casting and explicit casting in the context of .NET C# ?</summary>
In the context of .NET and C#, casting is the process of converting a value of one data type to another¹²³⁴⁵. There are two types of casting:

- **Implicit Casting**: This is automatically performed by the compiler when the conversion is safe and no data will be lost¹²³⁴⁵. For example, converting a smaller type to a larger type size (char -> int -> long -> float -> double) is an implicit cast¹²³⁴⁵. Here's an example of implicit casting:

```csharp
int i = 123;
long l = i; // Implicit casting from int to long
```

- **Explicit Casting**: This is performed manually by the programmer using the cast operator¹²³⁴⁵. Explicit casting is required when the conversion could lose data or when the conversion might not succeed for other reasons¹²³⁴⁵. For example, converting a larger type to a smaller size type (double -> float -> long -> int -> char) is an explicit cast¹²³⁴⁵. Here's an example of explicit casting:

```csharp
double d = 123.45;
int i = (int)d; // Explicit casting from double to int
```

In summary, implicit casting is done automatically when the conversion is safe and no data will be lost, while explicit casting is done manually when there's a risk of data loss¹²³⁴⁵.

Source: <br>
(1) Casting and type conversions - C# Programming Guide - C#. https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/casting-and-type-conversions. <br>
(2) c# - What is the difference between explicit and implicit type casts .... https://stackoverflow.com/questions/1584293/what-is-the-difference-between-explicit-and-implicit-type-casts. <br>
(3) C# Type Casting - W3Schools. https://www.w3schools.com/cs/cs_type_casting.php. <br>
(4) Type Casting in C# - Simple2Code. https://simple2code.com/csharp-tutorial/type-casting-in-csharp/. <br>
(5) Understanding Type Casting in C# with Examples - Techieclues. https://www.techieclues.com/blogs/type-casting-in-c-sharp. <br>
</details>

<details>
<summary> Explain Array vs ArrayList</summary>
Comparison table between `Array` and `ArrayList` in C#:

| Feature          | Array                                                               | ArrayList                                                                                   |
| ---------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| **Type Safety**  | Strongly-typed (can only store elements of the same data type)      | Not strongly-typed (can store elements of any data type)                                    |
| **Size**         | Fixed (determined at creation)                                      | Dynamic (can grow or shrink at runtime)                                                     |
| **Access Speed** | Fast (due to contiguous memory allocation)                          | Slower (due to non-contiguous memory allocation)                                            |
| **Flexibility**  | Less flexible (due to fixed size and type safety)                   | More flexible (due to dynamic size and ability to store different data types)               |
| **Namespace**    | System.Array                                                        | System.Collections                                                                          |
| **Example**      | `int[] intArray = new int[] {2}; intArray[0] = 1; intArray[2] = 2;` | `ArrayList Arrlst = new ArrayList(); Arrlst.Add("Sagar"); Arrlst.Add(1); Arrlst.Add(null);` |

- If you need a fixed-size collection of elements of the same data type, then an array may be the better choice. 
- If you need a dynamic collection that can grow or shrink in size and can hold elements of any data type, then an ArrayList may be a better choice.
</details>

<details>
<summary>Generic Collections</summary>

```
Provides the benefits of having a typed collection (no boxing and unboxing are necessary) and the benefits of being a dynamic collection with no fixed size
```
List of some of the most used generic collections in .NET C#, when they should be used, and an example of each:

| Collection                  | Description                                                                                                   | When to Use                                                    | Example                                                                                                                        |
| --------------------------- | ------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| **List<T>**                 | A generic list that contains elements of a specified type. It grows automatically as you add elements in it¹. | When you need a dynamic-size, ordered collection of elements¹. | `List<int> numbers = new List<int>(); numbers.Add(1); numbers.Add(2); numbers.Add(3);`                                         |
| **Dictionary<TKey,TValue>** | Contains key-value pairs¹.                                                                                    | When you need a collection of key-value pairs¹.                | `Dictionary<string, int> dict = new Dictionary<string, int>(); dict.Add("apple", 1); dict.Add("banana", 2);`                   |
| **SortedList<TKey,TValue>** | Stores key and value pairs. It automatically adds the elements in ascending order of key by default¹.         | When you need a sorted collection of key-value pairs¹.         | `SortedList<int, string> sortedList = new SortedList<int, string>(); sortedList.Add(1, "apple"); sortedList.Add(2, "banana");` |
| **Queue<T>**                | Stores the values in FIFO style (First In First Out). It keeps the order in which the values were added¹.     | When you need a first-in, first-out collection of objects¹.    | `Queue<int> queue = new Queue<int>(); queue.Enqueue(1); queue.Enqueue(2); queue.Enqueue(3);`                                   |
| **Stack<T>**                | Stores the values as LIFO (Last In First Out)¹.                                                               | When you need a last-in, first-out collection of objects¹.     | `Stack<int> stack = new Stack<int>(); stack.Push(1); stack.Push(2); stack.Push(3);`                                            |
| **HashSet<T>**              | Contains non-duplicate elements. It eliminates duplicate elements¹.                                           | When you need a collection of unique elements¹.                | `HashSet<int> set = new HashSet<int>(); set.Add(1); set.Add(2); set.Add(3);`                                                   |

These generic collections are recommended to use over non-generic collections because they perform faster and also minimize exceptions by giving compile-time errors¹. They are more type-safe, meaning you can't insert an element of the wrong type into a collection by mistake, and you don't have to cast elements to the correct type when you retrieve them².

Source: <br>
(1) Generic List Collection in C# with Examples - Dot Net Tutorials. https://dotnettutorials.net/lesson/list-collection-csharp/.<br>
(2) List<T> Class (System.Collections.Generic) | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1?view=net-8.0.<br>
(3) 6 Generic Collections in C# with Examples - DotNetCrunch. https://dotnetcrunch.in/generic-collections-in-csharp/.<br>
(4) When to Use Generic Collections - .NET | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/standard/collections/when-to-use-generic-collections.<br>
(5) Generic Collections in .NET - .NET | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/standard/generics/collections.<br>
</details>

<details>
<summary>What is Threading (Multithreading) in C# and what is a Task ?</summary>

Basically: If you want to run code parallely in a multicore processor.. use Threads
```
using System.Threading;

Thread newThread = new Thread(() =>
{
    // Code to be executed by the new thread
});

newThread.Start();
```

In C#, a **Thread** and a **Task** are both used to create parallel programs, but they serve different purposes and have different use cases¹².

**Thread**:
- A Thread is a single sequence of instructions that a process can execute¹.
- The `System.Threading.Thread` class is used for creating and manipulating a thread in Windows².
- Threads are used to perform multiple operations at the same time².
- Example of creating a thread:
```csharp
Thread thread = new Thread(new ThreadStart(getMyName));
thread.Start();
```

**Task**:
- A Task represents some asynchronous operation¹.
- Tasks are part of the Task Parallel Library, a set of APIs for running tasks asynchronously and in parallel².
- Tasks can return a result¹.
- Tasks support cancellation through the use of cancellation tokens².
- Example of creating a task:
```csharp
Task<string> obTask = Task.Run(() => (return "Hello"));
Console.WriteLine(obTask.result);
```

**Key Differences**:
- Tasks utilizes your multicore processor properly while Thread have CPU affinity
- A Task can have multiple processes happening at the same time, while Threads can only have one task running at a time².
- Tasks can return a result, while there is no direct mechanism to return the result from a thread². If you want to get a result from a thread you have to use delegates, events and so on.
- Tasks support cancellation through the use of cancellation tokens, but Threads don't².
- Tasks are generally preferred over threads for IO-bound operations (like reading and writing to a database), while threads are typically used for CPU-bound operations (like computations)¹.

In summary, a Task is a higher-level concept than a Thread. While a Thread represents a single sequence of instructions, a Task is an abstraction of a series of operations that will be executed¹². In summary, because of the benefits of Tasks, always use the TPL (Task Parallel Library) whenever you have a chance ... The TPL dynamically scales the degree of concurrency to use all the available processors most efficiently. It handles the partitioning of the work, the scheduling of threads on the ThreadPool, cancellation support, state management, and other low-level details. By using TPL, you can maximize the performance of your code while focusing on the work that your program is designed to accomplish.

Source: <br>
(1) c# - What is the difference between task and thread? - Stack Overflow. https://stackoverflow.com/questions/4130194/what-is-the-difference-between-task-and-thread.<br>
(2) Task And Thread In C# - C# Corner. https://www.c-sharpcorner.com/article/task-and-thread-in-c-sharp/.<br>
(3) Difference Between Task and Thread - Net-Informations.Com. https://net-informations.com/csharp/language/task.htm.<br>

</details>

<details>
<summary>Why to use OUT in C# ?</summary>
Usually a method has only one return type, with out, you can return multiple types

```
class OutReturnExample
{
    static void Method(out int i, out string s1, out string s2)
    {
        i = 44;
        s1 = "I've been returned";
        s2 = null;
    }

    static void Main()
    {
        int value;
        string str1, str2;
        Method(out value, out str1, out str2);

        // value is now 44
        // str1 is now "I've been returned"
        // str2 is (still) null;
    }
}
```
</details>
<details>
<summary>What is the difference between Abstract class and Interface ?</summary>
Abstract class is a half defined parent class while interface is a contract. 
</details>

</details>

<details>
<summary>Intermediate Questions</summary>

<details>
<summary>What is a Delegate ?</summary>
A Delegate is a Pointer to a Function, created to serve as callbacks which acts as a communication channel between concurrent async or parallel processes
</details>

<details>
<summary>What is the need of Delegates ?</summary>
Delegates in C# are used for several reasons:

1. **Encapsulate a method**: Delegates are objects that encapsulate a method¹². They allow methods to be passed as parameters¹²⁵, which can be useful when you want to pass a method as an argument to another method¹.
```csharp
public delegate void MyDelegate(string msg);  // declare a delegate

// set the delegate to a method
MyDelegate del = new MyDelegate(MethodA);

// invoke the method through the delegate
del("Hello World");

public void MethodA(string message)
{
    Console.WriteLine("MethodA says: " + message);
}
```
2. **Callback Mechanism**: Delegates can be used to define callback methods¹²⁴. This is useful in event-driven programming where you want a certain method to be called upon the occurrence of an event¹.

``` csharp
public delegate void MyDelegate(string msg);  // declare a delegate

public static void MethodWithCallback(int param1, int param2, MyDelegate callback)
{
	callback("The number is: " + (param1 + param2).ToString());
}

public static void DelegateMethod(string message)
{
	Console.WriteLine(message);
}

void Main()
{
	// Instantiate the delegate.
	MyDelegate handler = DelegateMethod;

	// Call the method with a callback
	MethodWithCallback(1, 2, handler);
}
```
3. **Abstract and Decouple Methods**: Delegates provide a way to abstract a method from the caller². This means the caller doesn't need to know the details of the method being called².

``` csharp
public delegate void MyDelegate(string msg);

public class MyClass
{
	private MyDelegate del;

	public MyClass(MyDelegate del)
	{
		this.del = del;
	}

	public void Run()
	{
		del("Hello World");
	}
}

public void MethodA(string message)
{
	Console.WriteLine("MethodA says: " + message);
}

public void Main()
{
	MyClass myClass = new MyClass(new MyDelegate(MethodA));
	myClass.Run();
}
```
4. **Event Handling**: Delegates are the foundation of .NET event handling². The .NET event model is based on delegates and is used to respond to user actions like button clicks or menu selections².

``` csharp
public delegate void MyDelegate(string msg);

public class MyClass
{
	public event MyDelegate MyEvent;

	public void Run()
	{
		MyEvent?.Invoke("Hello World");
	}
}

public void MethodA(string message)
{
	Console.WriteLine("MethodA says: " + message);
}

void Main()
{
	MyClass myClass = new MyClass();
	myClass.MyEvent += new MyDelegate(MethodA);
	myClass.Run();
}
```
5. **Asynchronous Programming**: Delegates are used in asynchronous programming to call methods asynchronously².

```csharp 
using System.Threading.Tasks;

public static async Task Main()
{
	Func<int, int, int> del = Sum;
	var task = Task.Run(() => del(1, 2));

	// You can do other work here while waiting

	int result = await task;
	Console.WriteLine("The result is: " + result);
}

public static int Sum(int num1, int num2)
{
	return num1 + num2;
}
```
6. **LINQ and Lambda Expressions**: Delegates are used extensively in LINQ queries and lambda expressions².
```csharp
Func<int, bool> isEven = num => num % 2 == 0;
int[] numbers = { 1, 2, 3, 4, 5, 6 };
IEnumerable<int> evenNumbers = numbers.Where(isEven);
evenNumbers.Dump(); //LinqPad execution
```

Source: <br>
(1) c# - When & why to use delegates? - Stack Overflow. https://stackoverflow.com/questions/2019402/when-why-to-use-delegates.<br>
(2) Why do we need C# delegates - Stack Overflow. https://stackoverflow.com/questions/4284493/why-do-we-need-c-sharp-delegates.<br>
(3) Delegates - C# Programming Guide - C# | Microsoft Learn. https://learn.microsoft.com/en-US/dotnet/csharp/programming-guide/delegates/.<br>
(4) C# delegates (With Examples) - Programiz. https://www.programiz.com/csharp-programming/delegates.<br>
(5) c# - what is the need of delegates? - Stack Overflow. https://stackoverflow.com/questions/36001027/what-is-the-need-of-delegates.<br>
</details>

<details>
<summary>What is a Multicast Delegate ?</summary>
A multicast delegate in C# is a delegate that holds the references of more than one function. When you invoke the multicast delegate, all the functions which are referenced by the delegate are going to be invoked. Here’s an example:

```csharp 
public delegate void MyDelegate(string msg);
public MyDelegate mydelegate = null;

void Main()
{
	mydelegate += PrintToConsole;
	mydelegate += PrintToConsole;
    
	mydelegate("Hello World!");
	
	void PrintToConsole(string msg) => Console.WriteLine(msg);
	
	mydelegate = null;
}

```


</details>

<details>
<summary>What are events ?</summary>
<b>Events are encapsulation over delegates, they use delegates internally. Events helps you implement Publisher-Subscriber mode</b>

Events in C# are a way for an object to notify other classes or objects when something of interest occurs¹². The class that sends (or raises) the event is called the publisher and the classes that receive (or handle) the event are called subscribers¹². 

Events are typically used to signal user actions such as button clicks or menu selections in graphical user interfaces². When an event has multiple subscribers, the event handlers are invoked synchronously when an event is raised². 

In C#, an event is an encapsulated delegate¹. It is dependent on the delegate. The delegate defines the signature for the event handler method of the subscriber class¹.

Here are some examples of declaring, raising, and consuming an event in C#:

**Declaring an Event**:
```csharp
public delegate void Notify(); // delegate

public class ProcessBusinessLogic
{
    public event Notify ProcessCompleted; // event
}
```
In this example, a delegate `Notify` is declared and then an event `ProcessCompleted` of delegate type `Notify` is declared using the `event` keyword in the `ProcessBusinessLogic` class¹.

**Raising an Event**:
```csharp
public delegate void Notify(); // delegate

public class ProcessBusinessLogic
{
    public event Notify ProcessCompleted; // event

    public void StartProcess()
    {
        Console.WriteLine("Process Started!");

        // some code here..

        OnProcessCompleted();
    }

    protected virtual void OnProcessCompleted() //protected virtual method
    {
        //if ProcessCompleted is not null then call delegate
        ProcessCompleted?.Invoke();
    }
}
```
In this example, the `StartProcess()` method calls the method `OnProcessCompleted()` at the end, which raises an event¹.

**Consuming an Event**:
```csharp
class Program
{
    public static void Main()
    {
        ProcessBusinessLogic bl = new ProcessBusinessLogic();

        bl.ProcessCompleted += bl_ProcessCompleted; // register with an event

        bl.StartProcess();
    }

    // event handler
    public static void bl_ProcessCompleted()
    {
        Console.WriteLine("Process Completed!");
    }
}
```
In this example, the subscriber class registers to `ProcessCompleted` event and handles it with the method `bl_ProcessCompleted` whose signature matches `Notify` delegate¹.

Source: <br>
(1) Events in C# - TutorialsTeacher.com. https://www.tutorialsteacher.com/csharp/csharp-event.<br>
(2) Events - C# Programming Guide - C# | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/events/.<br>
(3) Events in C# - javatpoint. https://www.javatpoint.com/events-in-c-sharp.<br>
(4) C# - Events - Online Tutorials Library. https://www.tutorialspoint.com/csharp/csharp_events.htm.<br>
(5) Events, Delegates and Event Handler in C# - Dot Net Tutorials. https://dotnettutorials.net/lesson/events-delegates-and-event-handler-in-csharp/.<br>
(6) Events in C# - Code Maze. https://code-maze.com/csharp-events/.<br>
(7) Introduction to events - C# | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/csharp/events-overview.<br>
(8) github.com. https://github.com/nccasia/ncc-net-basic/tree/03d28a32af69216c72b701d22d2b9eebc12f1af6/CSharpAdvanced%2FEvents%2FREADME.md.<br>
(9) github.com. https://github.com/ravuri-malleswari/.net-programming/tree/14c161f5bfe57b17641f2efc9e6c3cf78a222eb7/events.cs.<br>

</details>

<details>
<summary>Events vs Delegates</summary>

- Events uses Delegates
- Delegates are for callbacks, not encapsulated
- Events are encapsulated Delegates to help implement Pub-Sub mode
</details>

</details>

## Interview Questions in OOP

<details>
<summary>Basic Questions</summary>
<details>
<summary>Why do we need OOP?</summary>

Object-Oriented Programming (OOP) is a programming paradigm that offers several benefits:
1. It forces the developer to think in terms of real world objects
2. **Encapsulation**: OOP allows you to bundle code into a single unit where you can determine the scope of each piece of data¹.
3. **Abstraction**: By using classes, you are able to generalize your object types, simplifying your program¹.
4. **Inheritance**: A class can inherit attributes and behaviors from another class, enabling more code reuse¹.
5. **Polymorphism**: One class can be used to create many objects, all from the same flexible piece of code¹.

OOP helps manage the size and complexity of your software by breaking down the code into smaller, more manageable chunks⁵. It enhances code organization, facilitates modularity and scalability, ensures data security, promotes collaboration, and provides a natural way to model real-world systems³. It also makes code maintenance and extensibility easier². If changes need to be made to the system, specific classes can be modified or extended without affecting others². This reduces the chances of introducing bugs².

In summary, the need for OOP stems from its ability to improve the structure of code, enhance reusability, and provide better security, maintainability, and flexibility²³⁴..

Source: <br>
(1) Why Object-Oriented Programming? | Codecademy. https://www.codecademy.com/article/cpp-object-oriented-programming.<br>
(2) Object Oriented Programming: A Breakdown for Beginners. https://www.udacity.com/blog/2022/05/object-oriented-programming-a-breakdown-for-beginners.html.<br>
(3) Exploring the Need for Object-Oriented Programming - DZone. https://dzone.com/articles/exploring-the-need-of-object-oriented-programming.<br>
(4) Why do we need to learn Object Oriented Programming? - EnjoyAlgorithms. https://www.enjoyalgorithms.com/blog/why-should-we-learn-oops-concepts-in-programming/.<br>
(5) Advantages and Disadvantages of OOP - GeeksforGeeks. https://www.geeksforgeeks.org/benefits-advantages-of-oop/.<br>
</details>
<details>
<summary>What are the 4 pillars of OOP?</summary>

1. **Abstraction**: Abstraction is the process of modeling the relevant attributes and interactions of entities as classes to define an abstract representation of a system¹. Here's a simple example in C#:

```csharp
public abstract class Animal
{
    public abstract void animalSound();
    public void sleep()
    {
        Console.WriteLine("Zzz");
    }
}

public class Pig : Animal
{
    public override void animalSound()
    {
        Console.WriteLine("The pig says: wee wee");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Pig myPig = new Pig();
        myPig.animalSound();
        myPig.sleep();
    }
}
```

2. **Encapsulation**: Encapsulation is the process of hiding the internal state and functionality of an object and only allowing access through a public set of functions¹. Here's a simple example in C#:

```csharp
public class Employee
{
    private int ID;
    private string Name;
    private int Age;
    private double Salary;

    public string GetName()
    {
        return Name;
    }

    public void SetName(string Name)
    {
        if (string.IsNullOrEmpty(Name))
        {
            throw new Exception("The name cannot be blank");
        }
        this.Name = Name;
    }
}
```

3. **Inheritance**: Inheritance is the ability to create new abstractions based on existing ones¹. Here's a simple example in C#:

```csharp
public class Animal
{
    public virtual void animalSound()
    {
        Console.WriteLine("The animal makes a sound");
    }
}

public class Pig : Animal
{
    public override void animalSound()
    {
        Console.WriteLine("The pig says: wee wee");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Animal myAnimal = new Animal();
        Animal myPig = new Pig();

        myAnimal.animalSound();
        myPig.animalSound();
    }
}
```

4. **Polymorphism**: Polymorphism is the ability to implement inherited properties or methods in different ways across multiple abstractions¹. Here's a simple example in C#:

```csharp
public class Animal
{
    public virtual void animalSound()
    {
        Console.WriteLine("The animal makes a sound");
    }
}

public class Pig : Animal
{
    public override void animalSound()
    {
        Console.WriteLine("The pig says: wee wee");
    }
}

public class Dog : Animal
{
    public override void animalSound()
    {
        Console.WriteLine("The dog says: bow wow");
    }
}

class Program
{
    static void Main(string[] args)
    {
        Animal myAnimal = new Animal();
        Animal myPig = new Pig();
        Animal myDog = new Dog();

        myAnimal.animalSound();
        myPig.animalSound();
        myDog.animalSound();
    }
}
```

These pillars provide the foundation for writing maintainable and scalable code¹..

Source: <br>
(1) Object-Oriented Programming (C#) - C# | Microsoft Learn. https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/oop.<br>
(2) Pillars Of OOP/Overview Of OOP - C# Corner. https://www.c-sharpcorner.com/UploadFile/e6a07d/pillars-of-oop/.<br>
(3) The Four Pillars of OOP - jCode Library. https://jcode.stablenetwork.uk/library/csharp/four-pillars.<br>
(4) C# The Four Pillars of OOP Presentation. https://jcode.stablenetwork.uk/presentation/csharp/four-pillars.<br>
(5) github.com. https://github.com/biljanazivkovic/CSharp-Example18/tree/e8cf4865c5b4ffd899f995dee1fba5ac60e20911/Program.cs.<br>
(6) github.com. https://github.com/YugalShrestha0/Binod-Sir/tree/2ca62869aced793bd74178536c91e2bb6de14382/Assignment%2FPolymorphism%2FPolymorphism%2FProgram.cs.<br>
(7) github.com. https://github.com/sandeshvue/hello-world/tree/5a934f1a4b8905cc6c358a35837ff836a7ab8680/Polymorphism%2FPolymorphism%2FProgram.cs.<br>
</details>
<details>
<summary>What is a class and what is an object?</summary>
</details>
<details>
<summary>Abstraction vs Encapsulation</summary>
</details>
<details>
<summary>Explain Inheritance?</summary>
</details>
<details>
<summary>Explain virtual keyword</summary>
</details>
<details>
<summary>What is overriding ?</summary>
</details>
<details>
<summary>Explain overloading</summary>
</details>
<details>
<summary>Overloading vs Overriding</summary>
</details>
</details>
<details>
<summary>Intermediate Questions</summary>
</details>



[1.1]:https://dotnetfiddle.net/HW6qZ7
[1.2]:https://dotnetfiddle.net/5JF1bE
[1.3]:https://dotnetfiddle.net/4Ksrjg
[2.1]:https://dotnetfiddle.net/wtyP9n
[2.2]: https://dotnetfiddle.net/zKLjTo
[2.3]:https://dotnetfiddle.net/SIGT3W
[3.1]:https://dotnetfiddle.net/uBGf7N
[3.2]:https://dotnetfiddle.net/xfptVE
[3.3]:https://dotnetfiddle.net/QMWI8b
[4.1]:https://dotnetfiddle.net/uKCp83
[4.2]:https://dotnetfiddle.net/BZ807c
[4.3]:https://dotnetfiddle.net/k1Kv5G
[5.1]:https://dotnetfiddle.net/mV9HuX
[5.2]:https://dotnetfiddle.net/rVjiGW
[5.3]:https://dotnetfiddle.net/WdvMtE
[6.1]:https://dotnetfiddle.net/9tV0Vr
[6.2]: https://dotnetfiddle.net/QyynC4
[6.3]:https://dotnetfiddle.net/AgclA6
[7.1]:https://dotnetfiddle.net/lMu408
[7.2]:https://dotnetfiddle.net/MEukJ8
[7.3]:https://dotnetfiddle.net/I6u7Nz
[8.1]:https://dotnetfiddle.net/cGTi5Z
[8.2]:https://dotnetfiddle.net/mg7hw3
[8.3]:https://dotnetfiddle.net/lagX58
[9.1]:https://dotnetfiddle.net/SBFElN
[9.2]:https://dotnetfiddle.net/sHWtDU
[9.3]:https://dotnetfiddle.net/LFHPPE
[10.1]:https://dotnetfiddle.net/7nk1JC
[10.2]:https://dotnetfiddle.net/w717Kk
[10.3]:https://dotnetfiddle.net/DwSTrJ
[11.1]:https://dotnetfiddle.net/zBuJpV
[11.3]:https://dotnetfiddle.net/AX9w4W
[12.1]:https://dotnetfiddle.net/pXB6i5
[12.3]:https://dotnetfiddle.net/Z7JIJn
[13.1]:https://dotnetfiddle.net/4Fze9g
[13.3]:https://dotnetfiddle.net/GCVP7v
[14.1]:https://dotnetfiddle.net/1ITBkw
[14.3]:https://dotnetfiddle.net/Z67LW8
[15.1]:https://dotnetfiddle.net/bwA0sO
[15.3]:https://dotnetfiddle.net/NGFV4g
[16.1]:https://dotnetfiddle.net/jIL2AQ
[17.1]:https://dotnetfiddle.net/MlyOqU
[18.1]:https://dotnetfiddle.net/eUTwv4
[19.1]:https://dotnetfiddle.net/2ImjJD
[20.1]:https://dotnetfiddle.net/tlz0Uz







