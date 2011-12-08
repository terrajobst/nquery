# NQuery Features

## Core Features

* Allows to execute `SELECT` statements against business objects.

* Allows executing simple expressions using a lightweight expression evaluator.

* Supports top, top with ties, inner joins, outer joins, full outer joins,
filtering, grouping, aggregation, sorting, all types of subqueries (`EXISTS`,
`ALL`, `ANY`, `SOME`), derived tables, and common table expressions (CTE).

* Supports standard .NET objects as data sources (`DataSet`, `DataTable`, and
`IEnumerable`).

* Provides code assistance services to integrate syntax editors.

## UI Features

* Provides integration with [Actipro's SyntaxEditor](http://www.actiprosoftware.com/Products/DotNet/WindowsForms/SyntaxEditor/default.aspx)
control with full support for syntax highlighting, error highlighting, member
completion and parameter info.

* Provides show plan control to visualize the execution plan similar to
Microsoft SQL Server Management Studio.

* Provides browser control to explore the scope of a query or expression.

## Performance Features

* For optimal performance the query engine internally represents the query as an
algebra tree and uses an optimizer to choose between different join  orders and
to perform subquery decorrelation.

* In order to remove the overhead of reflection calls at runtime all expressions
are compiled to Microsoft Intermediate Language (MSIL) using lightweight code
generation.

## Customization Features

* Provides advanced customization model for functions, aggregates, data sources,
property providers, method providers, and comparers.

* Automatically uses operator overloadings so that custom data types (such as
`Money`) can be used like in the source code.