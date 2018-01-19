Create a catalog file for the driver package (more informatin here : https://technet.microsoft.com/en-us/library/dd919238(v=ws.10).aspx#bkmk_sub4b)

Inf2Cat /driver:c:\Projects\SimpleIdentityServer\driver /os:7_x86

Sign the catalog file by using SignTool

SignTool sign /f SimpleIdServer.pfx rfidchafon.cat