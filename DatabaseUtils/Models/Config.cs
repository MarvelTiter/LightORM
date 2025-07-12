namespace DatabaseUtils.Models
{
    public class Config
    {
        public string? LastSelectedDb
        {
            get => Properties.Local.Default.LastSelectedDb;
            set
            {
                Properties.Local.Default.LastSelectedDb = value;
                Properties.Local.Default.Save();
            }
        }

        public string? Connectstring
        {
            get => Properties.Local.Default.Connectstring;
            set
            {
                Properties.Local.Default.Connectstring = value;
                Properties.Local.Default.Save();
            }
        }

        public string? Prefix
        {
            get => Properties.Local.Default.Prefix;
            set
            {
                Properties.Local.Default.Prefix = value;
                Properties.Local.Default.Save();
            }
        }

        public string? Separator
        {
            get => Properties.Local.Default.Separator ?? "_";
            set
            {
                Properties.Local.Default.Separator = value;
                Properties.Local.Default.Save();
            }
        }

        public string? Namespace
        {
            get => Properties.Local.Default.Namespace;
            set
            {
                Properties.Local.Default.Namespace = value;
                Properties.Local.Default.Save();
            }
        }

        public string SavedPath
        {
            get => Properties.Local.Default.SavedPath;
            set
            {
                Properties.Local.Default.SavedPath = value;
                Properties.Local.Default.Save();
            }
        }
    }
}