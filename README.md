[![Build status](https://ci.appveyor.com/api/projects/status/5yvdh1b0v9tuflch?svg=true)](https://ci.appveyor.com/project/tsutomi/deveeldb) [![Join the chat! https://gitter.im/deveel/deveeldb](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/deveel/deveeldb?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

DeveelDB 2.0
==========

**NOTE**: *This version of the project is still under development. It is in fact a total rewrite since the original version of the project, that has been discontinued. The poor level of coverage, the architectural model, the limits of the parser, and other requirements made it impossible to maintain it. You can still find it as a branch of the project.*

DeveelDB is a complete embeddable SQL-99 RDBMS for .NET/Mono frameworks. The system is designed around the standards ISO/ANSI and supports the following features:

- ACID Transactions: `BEGIN`, `COMMIT`, `ROLLBACK` (Isolation Level *Serializable*)
- Data Definition Language (DDL): `CREATE/DROP SCHEMA`, `CREATE/DROP/ALTER TABLE`
- Data Manipulation Language (DML): `SELECT FROM`, `INSERT INTO`, `DELETE FROM`, `UPDATE`
- User Management: `GRANT/REVOKE` statements
- Support for structured variables (eg. `DECLARE var INT(200) NOT NULL`)
- Procedures and functions: with the (current) limitation of the body defined into .NET classes
- Cursors
- ADO.NET native client
- Direct Access: programmatically execute SQL statements (without ADO.NET client and text commands)

Although the core project is thought to be embedded in applications, the modular architecture allows extensions to other uses, such as providing databases through networks: an application is already included in the solution.

License
============

*DeveelDB* is released under the [Apache 2.0][http://www.apache.org/licenses/LICENSE-2.0] license. This is a very permissive licensing, that allows anyone to use the core library into commercial and non-commercial project. Other libraries (such as he GIS extension) are released under different licensing, due to commercial reasons or to dependencies from external tools.


Status and Issues
============

You can verify the current status of the application code by  [checking the project](https://ci.appveyor.com/project/tsutomi/deveeldb-3f7ew) at [AppVeyor Continuous Integration](http://ci.appveyor.com) (access as "guest" user: you will find the direct link below the login form).

Please, report any issue or feature request to our [Issue Tracker](http://github.com/deveel/deveeldb/issues)
