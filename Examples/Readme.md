Add-Migration add-currency -Context EconomyContext
Script-Migration 0 add-currency -Context AdminContext

Add-Migration add-admin -Context AdminContext
Script-Migration 0 add-admin -Context AdminContext