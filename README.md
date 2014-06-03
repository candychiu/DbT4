DbT4
====

A project that generates code based on database schemas read by the Database Schema Reader.   This projects uses the T4 Toolbox to break up the code generation logic.  It also borrowed code from the EF6 Utility include and the Reverse POCO templates.

Dependencies:
Database Schema Reader
T4 Toolbox

Troubleshooting:

Q: Unable to load provider XXX.
A: A provider needs to be installed to the GAC in order to be seen in the T4 compilation environment.