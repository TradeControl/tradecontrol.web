---
layout: ../../layouts/Documentation.astro
title: Cash Manager - Statement
permalink: /cash/manager-statement
---

The Statement is the main working view in Cash Manager.

Use it to review cash activity, compare posted and unposted rows, inspect balances, and open related corrections.

## What the Statement shows

The Statement combines the selected cash account, year, period, and filters into one working view.

It shows:

- posted rows
- unposted rows
- running balance
- provisional balance
- namespace-aware activity
- linked Subjects and payment records

The Statement is the place to review cash position before making changes.

<div style="max-width: 1200px; margin: 1rem 0;">
  <img
    src="/images/cash-manager/cm-statement-desktop.png"
    alt="Cash Manager statement workspace on desktop"
    style="width: 100%; height: auto; display: block; border-radius: 8px;"
  />
</div>

## Filters and scope

You can narrow the Statement by:

- financial year
- period
- namespace filter
- cash code

Use the namespace filter when you need to work in a specific DAG context.

Use the cash code filter when you want to focus on a specific cash stream, for example `SALES`.

## Understanding the balances

The Statement header summarises the visible scope.

It shows:

- opening posted balance
- visible posted movement
- visible unposted movement
- provisional balance

The provisional balance helps you understand the effect of unposted activity before it is committed.

## Posted and unposted rows

Each row is shown with a status badge.

In general:

- `Posted` rows are part of committed history
- `Unposted` rows are still in the spool
- the visible balance reflects the selected scope

This means you can review work before it is posted.

## Namespace-aware activity

The Statement is namespace-aware.

If the same Subject appears in more than one namespace, the selected namespace context determines which activity is shown.

This is important when the same customer, supplier, or internal business entity is used in more than one branch of the structure.

## Opening related records

From the Statement you can open related records directly.

Examples include:

- opening the Subject Browser from the Subject code
- opening a payment for correction
- opening linked records from related views

This keeps the Statement as the centre of day-to-day cash review.

## Correcting a payment from the Statement

Where permissions allow, a payment can be opened directly from the Statement.

The correction workflow supports several modes:

- edit
- move
- payment
- delete

This lets you work from the Statement without leaving Cash Manager.

## Closed-period corrections

If a correction touches a closed or archived period, only an Administrator can confirm the overwrite.

Where that happens, the workflow also requests the required rebuild action.

For most users, this is an occasional exception rather than a normal workflow.

## Mobile

On mobile, the Statement becomes a single-column detail view.

The same information is available, but rows are presented as stacked cards and actions open in the mobile detail flow.

<div style="max-width: 600px; margin: 1rem 0;">
  <img
    src="/images/cash-manager/cm-statement-mobile.png"
    alt="Cash Manager statement workspace on mobile"
    style="width: 100%; height: auto; display: block; border-radius: 8px;"
  />
</div>

## Common outcomes

### I cannot see the row I expected

Check:

- the selected cash account
- the selected year and period
- the namespace filter
- the cash code filter

### The balance does not match the posted total

This usually means unposted rows are visible and the provisional balance includes them.

### I can see the payment but cannot edit it

This usually means:

- you do not have the required role
- the row is not editable in the current context
- the correction touches a closed period and requires administrative confirmation

## Related pages

- [Cash Manager overview](/cash/manager-overview)
- [Payments](/cash/manager-payments)
- [Assets](/cash/manager-assets)
- [Reference](/cash/manager-reference)
