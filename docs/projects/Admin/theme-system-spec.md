# SYSTEM INSTRUCTIONS FOR GPT‑5.4 (THEMES FEATURE)

You are modifying a C# / Razor Pages web application.  
The database, EF model, and Profile.cs theme logic are **already implemented**.  
Do NOT redesign or modify any backend logic.

Your task has **three steps**, which must be completed in order:

## STEP 1 — REFACTOR THE STYLESHEETS (SAFE, NON‑DESTRUCTIVE)

### 1.1 — DO NOT DELETE OR OVERWRITE EXISTING CSS FILES

Keep all existing CSS files intact until STEP 2 is confirmed working.  
You may create new files, but do not remove or wipe the originals.

### 1.2 — Use this suggested file structure

src/TCWeb/wwwroot/css/base.css  
src/TCWeb/wwwroot/css/components/blazorTree.css  
src/TCWeb/wwwroot/css/components/categoryTree.css  
src/TCWeb/wwwroot/css/modules/adminManager.css  
src/TCWeb/wwwroot/css/modules/subjectBrowser.css  
src/TCWeb/wwwroot/css/modules/taxConfigurator.css  
src/TCWeb/wwwroot/css/themes/theme-blue.css  
src/TCWeb/wwwroot/css/themes/theme-orange.css  
src/TCWeb/wwwroot/css/themes/theme-green.css  
src/TCWeb/wwwroot/css/themes/theme-dark.css

### 1.3 — Refactor rules

- Extract colour variables into `base.css`.
- Convert existing CSS to use variables.
- Create four theme files:
  - theme-blue.css
  - theme-orange.css
  - theme-green.css
  - theme-dark.css
- Themes must override ONLY variables.
- Do NOT rewrite selectors.
- Do NOT remove rules.
- Do NOT break mobile mode.
- Do NOT modify tree CSS except variable substitution.
- Do NOT rename files unless explicitly instructed.

## STEP 2 — INTEGRATE THE THEME INTO THE UI

### 2.1 — Replace site stylesheet references

- Remove reference to `site.css` in `_Layout.cshtml`.
- Add reference to `base.css`.
- Add reference to `/css/themes/theme-blue.css` **hard‑coded** for Test2.

This is ONLY to verify the refactor works.  
Dynamic theme loading will be added in Step 3.

### 2.2 - Replace module stylesheet references

Update ONLY the stylesheet paths in the following files:

- Pages.Cash.CategoryTree.CategoryTreeMode  
- Pages.Cash.Manager.IndexModel  
- Web.Pages.Admin.Manager.IndexModel  
- Pages.Tax.Configurator.IndexModel  

Do NOT modify any other logic in these files.

### 2.2 — Validation requirement

The UI must render correctly using the new theme system.  
Only after this works may old CSS be removed (but do NOT remove them in this task).

## STEP 3 — ADD THE THEME SELECTOR UI

### 3.1 — Use existing backend logic

Use the existing Profile class functions for:

- retrieving the user’s theme
- saving the user’s theme
- resolving the correct CSS file via `Usr_tbTheme.CssFile`
- publsh a themes list from the database

``` sql
        INSERT INTO Usr.tbTheme
        (
            ThemeCode,
            ThemeName,
            CssFile
        )
        VALUES
        ('ORANGE', 'Orange', 'theme-orange.css'),
        ('BLUE',   'Blue',   'theme-blue.css'),
        ('GREEN',  'Green',  'theme-green.css'),
        ('DARK',   'Dark',   'theme-dark.css');
```

Do NOT redesign Profile.cs.

### 3.2 — UI requirements

- Add a dropdown to the user profile page.
- Populate it with the themes from the ProfileClass
- Save the selected theme by calling Profile.UpdateUserThemeCode().
- Reload the page to apply the theme.
- Do NOT redesign the profile page layout.

## HARD BOUNDARIES (DO NOT BREAK THESE)

You must NOT:

- redesign the database
- redesign EF models
- redesign Profile.cs
- redesign the theme table
- redesign SQL scripts
- redesign the layout structure
- modify tree component logic
- break mobile mode
- rewrite selectors
- delete files
- rename files (unless explicitly instructed)
- invent new theme codes
- change Razor page logic

You MUST:

- keep changes minimal and scoped
- follow the three steps in order
- preserve all existing behaviour except theme loading
- ensure the UI remains functional at every step

## OUTPUT FORMAT

When modifying files, output ONLY valid aider patch blocks.

Do NOT produce explanations unless asked.

## Aider commands

/add docs/specs/theme-system-spec.md

### Stage 1-2

/add src/TCWeb/wwwroot/css/site.css  
/add src/TCWeb/wwwroot/css/blazorTree.css  
/add src/TCWeb/wwwroot/css/categoryTree.css  
/add src/sqlnode/src/tcNodeDb4/Usr/Tables/tbUser.sql  
/add src/sqlnode/src/tcNodeDb4/Usr/Tables/tbTheme.sql  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures\proc_NodeDataInit.sql"  
/add src/TCWeb/Models/Usr_tbUser.cs  
/add src/TCWeb/Models/Usr_tbTheme.cs  
/add src/TCWeb/Data/Profile.cs  
/add src/TCWeb/Pages/Shared/_Layout.cshtml  
/add src/TCWeb/Pages/Admin/Manager/Index.cshtml  
/add src/TCWeb/Pages/Cash/CategoryTree/Index.cshtml  
/add src/TCWeb/Pages/Cash/Manager/Index.cshtml  
/add src/TCWeb/Pages/Tax/Configurator/Index.cshtml  

### Stage 3

/add src/sqlnode/src/tcNodeDb4/Usr/Tables/tbUser.sql  
/add src/sqlnode/src/tcNodeDb4/Usr/Tables/tbTheme.sql  
/add "src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_NodeDataInit.sql"  
/add src/TCWeb/Models/Usr_tbUser.cs  
/add src/TCWeb/Models/Usr_tbTheme.cs  
/add src/TCWeb/Data/Profile.cs  
/add src/TCWeb/Areas/Identity/Pages/Account/Manage/Index.cshtml  
/add src/TCWeb/Areas/Identity/Pages/Account/Manage/Index.cshtml.cs

aider --no-git
