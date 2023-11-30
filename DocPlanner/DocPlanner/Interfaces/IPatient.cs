namespace DocPlanner.Interfaces;

public interface IPatient
{
    string Name { get; set; }
    string SecondName { get; set; }
    string Email { get; set; }
    string Phone { get; set; }
}