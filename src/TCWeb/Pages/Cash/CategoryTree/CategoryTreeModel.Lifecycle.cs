using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TradeControl.Web.AppServices.CategoryTree;

namespace TradeControl.Web.Pages.Cash.CategoryTree
{
    public partial class CategoryTreeModel
    {
        public Task<IActionResult> OnPostCloneVersionAsync(string expectedActiveVersionId,
            string description, string notes, bool confirmIsolation)
        {
            return RunLifecycleAsync(expectedActiveVersionId, confirmIsolation,
                "Confirm that the successor Draft is isolated from operational statements.",
                async () =>
                {
                    var result = await CategoryTreeService.CloneActiveAsync(expectedActiveVersionId,
                        description, notes, HttpContext.RequestAborted);
                    return (result.VersionId,
                        $"Draft {result.VersionId} was cloned from {expectedActiveVersionId}.");
                });
        }

        public Task<IActionResult> OnPostRefreshVersionAsync(string versionId, string changeNote,
            bool confirmRefresh)
        {
            return RunLifecycleAsync(versionId, confirmRefresh,
                $"Confirm that Draft {versionId} may be replaced from the active predecessor.",
                async () =>
                {
                    await CategoryTreeService.RefreshDraftAsync(versionId, changeNote,
                        HttpContext.RequestAborted);
                    return (versionId, $"Draft {versionId} was refreshed from its active predecessor.");
                });
        }

        public Task<IActionResult> OnPostValidateVersionAsync(string versionId)
        {
            return RunLifecycleAsync(versionId, true, string.Empty,
                async () =>
                {
                    var result = await CategoryTreeService.ValidateAsync(versionId,
                        HttpContext.RequestAborted);
                    return (versionId,
                        $"Version {versionId} passed validation ({result.CanonicalByteLength:N0} canonical bytes).");
                });
        }

        public Task<IActionResult> OnPostPublishVersionAsync(string versionId,
            string expectedPredecessorVersionId, string changeNote, bool confirmPublish)
        {
            return RunLifecycleAsync(versionId, confirmPublish,
                $"Confirm publication of Draft {versionId} over {expectedPredecessorVersionId}.",
                async () =>
                {
                    var workspace = await CategoryTreeService.GetWorkspaceAsync(versionId,
                        HttpContext.RequestAborted);
                    if (!string.Equals(workspace.ActiveVersionId, expectedPredecessorVersionId,
                            StringComparison.Ordinal)
                        || !string.Equals(workspace.SelectedVersion.PredecessorVersionId,
                            expectedPredecessorVersionId, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException(
                            "The active predecessor changed. Refresh the version workspace before publishing.");
                    }

                    await CategoryTreeService.PublishAsync(versionId, changeNote,
                        HttpContext.RequestAborted);
                    return (versionId, $"Version {versionId} is now the active Published tree.");
                });
        }

        public Task<IActionResult> OnPostDiscardVersionAsync(string versionId, bool confirmDiscard)
        {
            return RunLifecycleAsync(versionId, confirmDiscard,
                $"Confirm permanent discard of Draft {versionId}.",
                async () =>
                {
                    await CategoryTreeService.DiscardDraftAsync(versionId,
                        HttpContext.RequestAborted);
                    var workspace = await CategoryTreeService.GetWorkspaceAsync(null,
                        HttpContext.RequestAborted);
                    return (workspace.ActiveVersionId,
                        $"Draft {versionId} was discarded. No operational statement used it.");
                });
        }

        private async Task<IActionResult> RunLifecycleAsync(string returnVersionId, bool confirmed,
            string confirmationMessage, Func<Task<(string VersionId, string Message)>> command)
        {
            try
            {
                if (!confirmed)
                {
                    throw new InvalidOperationException(confirmationMessage);
                }

                var result = await command();
                CategoryTreeNotice = result.Message;
                return RedirectToPage("/Cash/CategoryTree/Index", new
                {
                    versionId = result.VersionId,
                    select = RootNodeKey
                });
            }
            catch (Exception exception)
            {
                await NodeContext.ErrorLog(exception);
                CategoryTreeError = CategoryTreeCommandFeedback.From(exception);
                return RedirectToPage("/Cash/CategoryTree/Index", new
                {
                    versionId = returnVersionId,
                    select = RootNodeKey
                });
            }
        }
    }
}
