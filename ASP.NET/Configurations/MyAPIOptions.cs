using System.ComponentModel.DataAnnotations;

public class MyApiOptions
{
    [Required]
    public string URL { get; set; }

    [Required]
    public string Key { get; set; }
}