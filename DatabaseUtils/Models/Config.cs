namespace DatabaseUtils.Models
{
    public class Config
    {
        public static string? LastSelectedDb
        {
            get => Properties.Local.Default.LastSelectedDb;
            set
            {
                Properties.Local.Default.LastSelectedDb = value;
                Properties.Local.Default.Save();
            }
        }
        public static string? Connectstring
        {
            get => Properties.Local.Default.Connectstring;
            set
            {
                Properties.Local.Default.Connectstring = value;
                Properties.Local.Default.Save();
            }
        }
        public static string? Prefix
        {
            get => Properties.Local.Default.Prefix;
            set
            {
                Properties.Local.Default.Prefix = value;
                Properties.Local.Default.Save();
            }
        }
        public static string? Separator
        {
            get => Properties.Local.Default.Prefix ?? "_";
            set
            {
                Properties.Local.Default.Prefix = value;
                Properties.Local.Default.Save();
            }
        }

        public static string? Namespace
        {
            get => Properties.Local.Default.Namespace;
            set
            {
                Properties.Local.Default.Namespace = value;
                Properties.Local.Default.Save();
            }
        }
        public static string SavedPath
        {
            get => Properties.Local.Default.Namespace;
            set
            {
                Properties.Local.Default.Namespace = value;
                Properties.Local.Default.Save();
            }
        }

    }
}
