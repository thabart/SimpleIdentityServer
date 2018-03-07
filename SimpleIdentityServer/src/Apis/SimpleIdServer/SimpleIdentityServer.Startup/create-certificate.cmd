makecert -n "CN=Lokit CA,O=AdvICT,OU=Dev" -cy authority -a sha1 -sv "LokitCA.pvk" -r "LokitCA.cer"
makecert -n "CN=localhost" -ic "LokitCA.cer" -iv "LokitCA.pvk" -a sha1 -sky exchange -pe -sv "SimpleIdServer.pvk" "SimpleIdServer.cer"
pvk2pfx -pvk "SimpleIdServer.pvk" -spc "SimpleIdServer.cer" -pfx "SimpleIdServer.pfx"