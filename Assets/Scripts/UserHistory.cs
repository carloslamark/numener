using System.Collections.Generic;
using System.Reflection.Emit;

public class UserHistory
{
    public string name;
    public List<PhaseResult> phaseList;

    public UserHistory(string name, List<PhaseResult> phaseList)
    {
        this.name = name;
        this.phaseList = phaseList;
    }
}