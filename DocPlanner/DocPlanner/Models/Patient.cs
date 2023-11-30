using DocPlanner.Interfaces;

namespace DocPlanner.Models;

public class Patient : IPatient
{
    public string Name { get; set; }
    public string SecondName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}