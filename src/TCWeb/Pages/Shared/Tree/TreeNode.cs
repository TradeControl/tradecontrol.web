namespace TradeControl.Web.Pages.Shared.Tree
{
    public sealed record TreeNode(string Key, string Text, string IconClass, bool HasChildren, bool IsMapped, bool IsEnabled, bool HasDisabledDescendants)
    {
        public TreeNode(string Key, string Text)
            : this(Key, Text, "bi-dot", false, false, true, false)
        {
        }

        public TreeNode(string Key, string Text, string IconClass)
            : this(Key, Text, IconClass, false, false, true, false)
        {
        }

        public TreeNode(string Key, string Text, string IconClass, bool HasChildren)
            : this(Key, Text, IconClass, HasChildren, false, true, false)
        {
        }

        public TreeNode(string Key, string Text, string IconClass, bool HasChildren, bool IsMapped)
            : this(Key, Text, IconClass, HasChildren, IsMapped, true, false)
        {
        }

        public TreeNode(string Key, string Text, string IconClass, bool HasChildren, bool IsMapped, bool IsEnabled)
            : this(Key, Text, IconClass, HasChildren, IsMapped, IsEnabled, false)
        {
        }
    }
}
