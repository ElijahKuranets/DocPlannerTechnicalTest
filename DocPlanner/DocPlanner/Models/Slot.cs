﻿using DocPlanner.Models;

namespace DocPlanner.Models;

public class Slot
{
    public Guid FacilityId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public Patient Patient { get; set; }
    public string Comments { get; set; }
}

