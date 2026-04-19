namespace TradeControl.Web.Pages.Shared.Tree
{
    public sealed record TreeNode(string Key, string Text, string IconClass, bool HasChildren, bool IsMapped)
    {
        public TreeNode(string Key, string Text)
            : this(Key, Text, "bi-dot", false, false)
        {
        }

        public TreeNode(string Key, string Text, string IconClass)
            : this(Key, Text, IconClass, false, false)
        {
        }

        public TreeNode(string Key, string Text, string IconClass, bool HasChildren)
            : this(Key, Text, IconClass, HasChildren, false)
        {
        }
    }
}
