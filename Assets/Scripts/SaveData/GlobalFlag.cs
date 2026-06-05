using System;
using System.Collections.Generic;

[Serializable]
public class GlobalFlag
{
    public string id;
    public int value;
}

[Serializable]
public class GlobalFlagsSaveData
{
    public List<GlobalFlag> entries = new();
}
