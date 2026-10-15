# Human review and Git history

- Leave implementation and evidence changes uncommitted so they remain visible for human review.
- Do not create, amend, squash, reset, rebase, merge, tag, or push Git history unless the user explicitly requests that specific action in the current turn after reviewing the changes.
- Earlier permission to commit or otherwise change Git history does not carry forward to later work.
- Apply this rule to this repository, its submodules, and independent repositories nested beneath it, including `.local`.
