# Template naming convention

Trade Control template stored procedures follow this pattern:

`proc_Template_{Entity}_{Scope}_{Class}_{Year}`

## Components

- **Entity:**  
  - `CO` = Company  
  - `ST` = Sole Trader  

- **Scope:**  
  - `MICRO` = Micro‑entity  
  - `ABRIDGED` = Abridged accounts  
  - `FULL` = Full accounts  

- **Class:**  
  - `CUR` = Current standard template  
  - `CUST` = Custom template (optionally with a descriptor, e.g. `CUST_GROCER`)  

- **Year:**  
  - Calendar year of the schema/template, e.g. `2026`

Historical templates are implied by the **Year** and do not need a special class code.

## Examples

### Current templates

- `proc_Template_CO_MICRO_CUR_2026`  
- `proc_Template_CO_ABRIDGED_CUR_2026`  
- `proc_Template_CO_FULL_CUR_2026`  
- `proc_Template_ST_MICRO_CUR_2026`  

### Custom templates

- `proc_Template_CO_MICRO_CUST_GROCER_2026`  
- `proc_Template_ST_FULL_CUST_CONTRACTOR_2026`  

### Historical templates (implicit via year)

- `proc_Template_CO_MICRO_2021`  
- `proc_Template_CO_FULL_2023`  

## Usage in App.tbTemplate

- **TemplateName:** stored procedure name (e.g. `proc_Template_CO_MICRO_CUR_2026`)  
- **TemplateDescription:** human‑readable description (e.g. `UK Company – Micro‑Entity (Current 2026 Schema)`)
