## ERP Order Format

Each order is sent to the ERP using an order file transmitted via FTP. Here is the file format:

## Naming Convention

The file name is EXT-{OrderNum}.TXT. Where OrderNum is the primary key of the order, left-padded
with zeros (10 characters in total). For example, the file name for order 42 is:

`EXT-0000000042.TXT`

## Line Separators

Each line in the file is separated by a carriage return character and a line feed character (\r\n).

## Fixed-Width Fields

Each field uses a fixed-width format. The line descriptions define how many characters
wide each field is, how the contents are aligned, and formatted.

## Header 

Each file starts with a header line:

| Field | Width | Aligned | Format |
| ----- | ----- | ------- | ----------- |
| OrderNum | 10 | RIGHT | left-padded with zeros |
| Email | 100 | LEFT | n/a |
| DateSubmitted | 10 | LEFT | yyyy/MM/dd |
| Total Weight | 10 | RIGHT | 0000000.00 |

## Shipping Address

The second line contains the shipping address:

| Field | Width | Aligned | Format |
| ----- | ----- | ------- | ----------- |
| OrderNum | 10 | RIGHT | left-padded with zeros |
| AddressType | 10 | LEFT | 'S' |
| Street1 | 200 | LEFT | n/a |
| Street2 | 200 | LEFT | n/a |
| City | 100 | LEFT | n/a |
| State | 2 | LEFT | n/a |
| Zip | 10 | LEFT | n/a |

## Billing Address

The third line contains the shipping address:

| Field | Width | Aligned | Format |
| ----- | ----- | ------- | ----------- |
| OrderNum | 10 | RIGHT | left-padded with zeros |
| AddressType | 10 | LEFT | 'B' |
| Street1 | 200 | LEFT | n/a |
| Street2 | 200 | LEFT | n/a |
| City | 100 | LEFT | n/a |
| State | 2 | LEFT | n/a |
| Zip | 10 | LEFT | n/a |

## Line Item

For each item in the order, add one line:

| Field | Width | Aligned | Format |
| ----- | ----- | ------- | ----------- |
| OrderNum | 10 | RIGHT | left-padded with zeros |
| WidgetID | 10 | RIGHT | left-padded with zeros |
| Quantity | 10 | RIGHT | left-padded with zeros |
