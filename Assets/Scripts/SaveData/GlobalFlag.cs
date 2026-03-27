using System;
using System.Collections.Generic;

[Serializable]
public class GlobalFlag
{
    public string id;
    public bool value;
}

[Serializable]
public class GlobalFlagsSaveData
{
    public List<GlobalFlag> entries = new();
}
