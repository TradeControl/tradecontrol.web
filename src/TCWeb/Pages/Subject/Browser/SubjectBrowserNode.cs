using TradeControl.Web.Data;
using TradeControl.Web.Pages.Shared.Tree;

namespace TradeControl.Web.Pages.Subject.Browser
{
    public sealed class SubjectBrowserNode
    {
        public string SubjectCode { get; init; } = string.Empty;
        public string BranchKey { get; init; } = string.Empty;
        public string NamespacePath { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string DisplayLabel { get; init; } = string.Empty;
        public NodeEnum.SubjectClass SubjectClass { get; init; }
        public int ChildCount { get; init; }
        public bool IsDefaultChild { get; init; }

        public bool HasChildren => ChildCount > 0;
        public bool IsStructural => SubjectClass == NodeEnum.SubjectClass.Structural;
        public bool IsReal => SubjectClass == NodeEnum.SubjectClass.Real;
        public bool IsVirtual => SubjectClass == NodeEnum.SubjectClass.Virtual;
        public string SubjectClassCode => ((short)SubjectClass).ToString();

        public string DisplayText => DisplayLabel;

        public TreeNode ToTreeNode()
        {
            return new TreeNode(
                BranchKey,
                DisplayText,
                GetIconClass(),
                HasChildren);
        }

        private string GetIconClass()
        {
            var iconClass = IsStructural
                ? "bi-diagram-3 tc-subject-browser-icon-structural"
                : IsReal
                    ? "bi-person-fill tc-subject-browser-icon-real"
                    : "bi-building tc-subject-browser-icon-virtual";

            if (IsDefaultChild)
            {
                iconClass = IsReal
                    ? $"{iconClass} tc-subject-browser-icon-default tc-subject-browser-icon-default-real"
                    : $"{iconClass} tc-subject-browser-icon-default tc-subject-browser-icon-default-light";
            }

            return iconClass;
        }
    }
}
