public static class SelectedPlayer
{
    private static int id = 0;
    public static int Id
    {
        get => id;
        set => id = (value == 0 || value == 1) ? value : 0;
    }
}