150..10000 |? {$_ -ne 3389 -and $_ -ne 445 -and $_ -ne 139} |%{
	& netsh interface portproxy add v4tov4 listenport=$_ listenaddress=10.0.x.x connectport=80 connectaddress=10.0.x.x
}