
public static class AdventureLibrary
{
    public static bool GetCCD_Entrust(object obj, out CCD_Entrust data)
    {
        data = obj as CCD_Entrust;
        return data != null && !data.NotPass;
    }
}
