add horde.db

copy-paste the following code into your package manager console on visual studio

```
Add-Migration add-world -Context WorldContext
Update-Database add-world -Context WorldContext


Add-Migration add-economy -Context EconomyContext
Update-Database add-economy -Context EconomyContext

Add-Migration add-admin -Context AdminContext
Update-Database add-admin -Context AdminContext
```