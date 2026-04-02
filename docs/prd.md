# Product Requirements Document: Widget Depot Rewrite

**Project:** Widget Depot Website Rewrite
**Author:** TBD
**Status:** Draft
**Date:** 2026-04-01

---

## 1. Overview

Widget Depot is the customer-facing ordering website for Widgets Depot, Inc. (WDI). The existing site is being replaced with a fresh codebase. The new site must replicate the capabilities of the existing site while introducing improvements, and must integrate with WDI's existing ERP system via the established file transfer process.

Delivery of the new website will be structured as iterative milestones so that working functionality reaches production incrementally.

---

## 2. Goals

- Replace the existing website with a maintainable, modern codebase.
- Preserve all existing customer and staff workflows.
- Improve catalog management: replace manual CLI updates with a file upload tool.
- Deliver value incrementally via scoped milestones.

## 3. Out of Scope

- Migrating existing user accounts or order history from the old system.
- Changing the ERP file format or FTP integration protocol.
- Real-time ERP integration (daily batch remains the target).

---

## 4. Users & Roles

| Role | Description |
|---|---|
| **Customer** | Self-registered user who browses the catalog and places orders. |
| **WDI Staff (Admin)** | Internal staff with access to the admin section for user and order management, and catalog uploads. |
| **Warehouse Staff** | Receives problem report emails; uploads catalog CSV files via admin UI. |

---

## 5. Features

### 5.1 Public Catalog

- The widget catalog is publicly accessible without authentication.
- Catalog data is sourced from CSV files provided by warehouse staff (the website is not the source of truth).

### 5.2 User Accounts

- Customers can self-register by providing profile details including username and password.
- Authenticated customers can:
  - Edit their profile details.
  - Change their password.
- No migration of accounts from the old system; all users register fresh.

### 5.3 Order Creation

- Authenticated customers can create orders via a multi-step wizard.
- An order contains:
  - One or more widgets with desired quantities.
  - Shipping address.
  - Billing address.
  - Estimated shipping cost (calculated via third-party shipping API based on total widget weight and destination distance).
- After creation, orders can be submitted immediately or saved for later submission.
- Orders not submitted within **30 days** of creation are automatically removed.

### 5.4 Order Submission & ERP Export

- When a customer submits an order, the system generates an order file in the format expected by the ERP system.
- Generated files are written to a designated pickup directory.
- A scheduled task runs **once per day** and transmits all pending files to the ERP system via FTP.
- The file format and FTP configuration must remain identical to the existing system (see Constraint 7.1).

### 5.5 Problem Reporting

- Authenticated customers can report a problem with a submitted order via a wizard:
  1. Enter the original order number.
  2. View all items from that order.
  3. Select the affected items.
  4. For each affected item, specify the issue type: under-requested quantity, over-requested quantity, or damaged widgets.
  5. Optionally add free-text notes.
- On completion, the system sends an email to warehouse staff containing the full problem report.
- Problem reports can only be filed for orders placed through the new system. This limitation is accepted by WDI as a temporary condition during transition.

### 5.6 Admin - User Management

- Staff can view a list of all registered users.
- Staff can assist customers with account management tasks (e.g., password resets, profile corrections).

### 5.7 Admin - Order Lookup

- Staff can look up any order by entering an order number.

### 5.8 Admin - Catalog Management

- Staff can upload a CSV file to update the widget catalog.
- The upload process replaces the current manual CLI workflow.
- The CSV format is defined by the warehouse team and treated as a given input format.

---

## 6. Milestones

Milestones are ordered to deliver usable functionality as early as possible. Each milestone should be releasable to production independently.

### Milestone 1 - Foundation: Catalog & Accounts

**Delivers:** Customers can browse the catalog and create accounts.

- Public widget catalog (populated from an initial CSV import).
- Customer self-registration and login.
- Customer profile view and edit.
- Customer password change.

### Milestone 2 - Order Management

**Delivers:** Customers can place and manage orders.

- Order creation wizard (widget selection, quantities, shipping/billing addresses, shipping cost estimate).
- Save draft orders and return to submit later.
- 30-day auto-expiry of unsubmitted orders.
- Order submission (produces order file in pickup directory).

### Milestone 3 - ERP Integration

**Delivers:** Submitted orders flow to the ERP system automatically.

- Scheduled daily FTP task to transmit order files from the pickup directory to the ERP system.
- Order file format validated against ERP expectations.

### Milestone 4 - Admin Tools

**Delivers:** Staff can manage users, look up orders, and update the catalog.

- Admin user list and account management.
- Admin order lookup by order number.
- Admin catalog CSV upload.

### Milestone 5 - Problem Reporting

**Delivers:** Customers can report order issues to warehouse staff.

- Problem reporting wizard.
- Automated problem report email to warehouse staff.

---

## 7. Constraints

### 7.1 ERP Integration (Hard Constraint)

The ERP file transfer process must not change:
- Order files must use the same format as the existing website.
- Files must be transmitted to the ERP system via FTP on the same daily schedule.
- FTP configuration and credentials remain as-is.

### 7.2 CSV Catalog Upload (Hard Constraint)

The format of the CSV File must not change.

---

## 8. Out of Scope (This Rewrite)

- Real-time or API-based ERP integration.
- Customer-facing order history for orders placed on the old system.
- Problem reports for orders from the old system.
- Payment processing (not present in the existing system).
