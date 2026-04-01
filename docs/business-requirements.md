# Widget Depot Rewrite

## Background

Widget Depot was built by Widgets Depot, Inc., (WDI for short) a business that sells widgets.
WDI has a large warehouse and they ship their widgets across the country. They have an existing ERP
system used to manage their warehouse and distribution of the orders. It suits their needs
very well but it does not have a means for customers to order widgets. That is why the
Widget Depot website was written.

Widget Depot allows customers to self-register an account. It is fairly basic, users
provide profile details including a username and password. Once an account is created,
users can change their password at any time. There is also a profile page where they
can edit any of their details. WDI staff have an admin section in the website where they can
see lists of all the users in the system and can assist people with managing their accounts.

Once customers sign in, they can create an order for the widgets they want. Each order consists of a list
of the widgets including a desired quantity. The website has a wizard that steps the customer
through the order creation process. Once an order is created, the customer can submit the
order, but they are not required to right away. If they want, they can come back later and
submit the order. An order will be automatically removed if it isn't submitted within 30 days.

In addition to the list of widgets, an order consists of shipping and billing address details,
and an estimate for the shipping costs. The shipping is based on the total weight of the 
widgets being ordered and the distance they need to be shipped. The website uses a third party
API from the shipping company to calculate the shipping costs.

Due to the limitations of the ERP system, there is no direct way to add new orders to the ERP
system. Instead, there is a file transfer process used to send  the order details. When an
order is submitted, the website creates a file containing all the order details. These files
are placed in a pickup directory. In the background there is a scheduled task that runs once 
a day that sends the files to the ERP system via FTP.

The website makes a catalog of the widgets publicly accessible. When
there are updates to the catalog, the warehouse staff provide a CSV file that contains
the current list of all widgets. Currently the operators of the website remote into the
server and update the catalog database using a command-line.

If a customer has a problem with an order, the website has a wizard to help the user
report the problem. The wizard asks the user to enter the original order number and
then lists all the contents of the order. The user can select which items have a problem
and describe the problem: under/over requested quantity, damaged widgets and optional notes to
help describe the issue. Once user has provided all the details, the website generates an
email that is sent to the warehouse staff for follow-up.

The WDI staff have an admin section where they can view existing orders by entering an 
order number. 

## Rewrite Goals

WDI wants to start with a fresh website codebase. It see values in getting the new website
up and running, even if it incomplete. They want to create milestone deliverables that
provide iterative functionality and value.

Nothing needs to be migrated from the old system to the new one. The catalog is maintained
by the warehouse (the website is not the source of truth). Users will have new accounts
setup in the new website. The existing set of orders are maintained by the warehouse
so if a user has a question about an order they can contact the warehouse for details. 
WDI recognizes that users will not be able to submit problem reports for orders submitted
from the old system. This is a temporary issue and is acceptable.

### New High-Level Requirements

The new website should provide the same capabilities as the existing website, although
it can choose to implement them using new designs and approaches. 

Rather than operators manually updating the catalog, the new website should include
a means for the warehouse staff to upload catalog update files.

### Constraints

1. The existing ERP system interaction must remain as-is:

 - Each day the new system will need to send order files to the ERP system via FTP using
   the same format used by the old website.
