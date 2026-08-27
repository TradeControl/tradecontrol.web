# Canonical Treatment of Sole Trader Capital Introduced & Drawings  

## 1. Core Rule

Capital Introduced and Drawings are **not P&L items** and must not be classified as TRADE.  
They are **balance‑sheet movements** between the owner and the business.

They must therefore be implemented using:

- A **Nominal Category** of class **MONEY**  
- Cash Codes attached to that Category  
- A dedicated **Cash Account** of type **ASSET**

This ensures visibility, double‑entry integrity, and a running balance.

---

## 2. Required Category Setup

### Category

- **Type:** Nominal  
- **Class:** `MONEY`  
- **Purpose:** Holds Cash Codes for owner‑related movements  
- **Effect:**  
  - Not part of MIS (TRADE)  
  - Not part of P&L  
  - Appears on the Cash Statement  
  - Maintains a running balance via the linked Cash Account

### Cash Codes

Two Cash Codes under the MONEY category:

- `capitalIntroduced`  
- `drawings`

These are the operational codes used in transactions.

---

## 3. Required Cash Account Setup

Create a dedicated Cash Account:

- **Name:** `ownerCapitalAccount` (or similar)  
- **Type:** `ASSET`  
- **Purpose:** Represents the owner’s funds in the business  
- **Behaviour:**  
  - Increases when the owner injects money  
  - Decreases when the owner withdraws money  
  - Cannot go negative unless explicitly allowed  
  - Provides the running balance needed for constraint enforcement

This is the correct place to track the owner’s position.

---

## 4. Required Double‑Entry Behaviour

### Capital Introduced

Owner puts money into the business.

- **DR** Bank (Cash Account type: CASH)  
- **CR** Owner Capital Account (Cash Account type: ASSET)  
- Using Cash Code: `capitalIntroduced`

### Drawings

Owner takes money out of the business.

- **DR** Owner Capital Account (ASSET)  
- **CR** Bank (CASH)  
- Using Cash Code: `drawings`

This preserves double‑entry and ensures the owner’s balance is always visible.

---

## 5. Why TRADE Is Incorrect

If these items are placed under a TRADE Category:

- They do not connect to the P&L  
- They do not connect to the balance sheet  
- They do not appear on the Cash Statement  
- They have no running balance  
- Over‑drawings cannot be detected  
- Transactions become invisible to the recording surface

Therefore, TRADE classification is invalid.

---

## 6. Compliance Note

HMRC does **not** treat capital introduced or drawings as income or expenses.  
They do **not** appear on SA103F or EOPS.  
They must be handled entirely within the balance‑sheet layer.

---

## 7. Final Rule

**Capital Introduced and Drawings must be implemented as MONEY‑class Nominal Categories with Cash Codes, posting to a dedicated ASSET‑type Cash Account.  
They must not be TRADE categories and must not be connected to the P&L.
