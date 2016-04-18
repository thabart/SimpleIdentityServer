insert into clients (ClientId, ApplicationType, ClientName, ClientSecret, DefaultMaxAge, GrantTypes, IdTokenEncryptedResponseAlg, LogoUri, PolicyUri, RedirectionUrls, RequireAuthTime, ResponseTypes, TokenEndPointAuthMethod)
values ('SimpleUma', 0, 'Simple Uma', 'SimpleUma', 0, '0,1', 'RS256', 'http://img.over-blog-kiwi.com/1/47/73/14/20150513/ob_06dc4f_chiot-shiba-inu-a-vendre-prix-2015.jpg', 'http://openid.net', 'http://localhost:5002/callback,http://localhost:5001/swagger/ui/o2c.html', 0, '0,1,2', 1)

insert into clientScopes (ClientId, ScopeName) values ('SimpleUma', 'openid')