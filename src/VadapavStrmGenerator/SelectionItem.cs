namespace VadapavStrmGenerator
{
    internal record SelectionItem
    {
        internal string Name { get; set; }
        internal Guid Id { get; set; }    
        internal bool IsDirectory { get; set; }

        public SelectionItem(string name, Guid id, bool isDirectory)
        {
            Name = name;
            Id = id;
            IsDirectory = isDirectory;
        }

        public override string? ToString()
        {
            return Name;
        }
    }
}
