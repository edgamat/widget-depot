# Widget Depot

Customer-facing ordering website for Widgets Depot, Inc. (WDI). This is a ground-up rewrite of the existing site, delivering functionality incrementally via scoped milestones.

## Overview

Widget Depot allows customers to browse a widget catalog, create accounts, and place orders. Submitted orders are exported daily to WDI's ERP system via FTP. Staff have an admin section for user management, order lookup, and catalog updates.

## Features

- **Public Catalog** - Browse all available widgets without an account
- **Customer Accounts** - Self-registration, profile management, password changes
- **Order Management** - Multi-step order wizard with shipping cost estimation; orders can be saved as drafts and submitted later (auto-expires after 30 days)
- **ERP Integration** - Submitted orders generate files that are transmitted to the ERP system via FTP on a daily schedule
- **Problem Reporting** - Wizard to report issues with submitted orders; sends an email to warehouse staff
- **Admin Tools** - User list, order lookup by order number, catalog CSV upload

## Milestones

| # | Name | Delivers |
|---|------|----------|
| 1 | Foundation | Public catalog, customer registration & login, profile management |
| 2 | Order Management | Order creation wizard, draft orders, 30-day expiry, order submission |
| 3 | ERP Integration | Scheduled daily FTP transfer of order files to ERP |
| 4 | Admin Tools | Admin user list, order lookup, catalog CSV upload |
| 5 | Problem Reporting | Problem reporting wizard, automated email to warehouse staff |

## Project Docs

- [Business Requirements](docs/business-requirements.md)
- [Product Requirements Document](docs/prd.md)

## Constraints

- **ERP file format and FTP process must not change.** The new system must produce order files in the same format as the existing website and transmit them on the same daily schedule.
- **Catalog CSV format must not change.** The warehouse team owns this format.

## Out of Scope

- Migrating user accounts or order history from the old system
- Real-time ERP integration (daily batch remains the target)
- Payment processing
- Problem reports for orders placed on the old system
