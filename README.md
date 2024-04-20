# ipk_omega
OMEGA: L4 scanner

## Návratové kódy
- `0` - vše proběhlo v pořádku
- `1` - Zadané rozhraní není aktivní nebo neexistuje např. l0 namísto lo
- `2` - Špatný format argumentu rozhraní (např. -i <nic>)
- `3` - Špatný formát argumentu portu (např. -u 22, , -u 22- , -u 22,-2, -u y-x)
- `4` - Nebyly poskytnuty nezbytné argumenty
- `5` - Cíl nedostupný z daného rozhraní
- `6` - Lokální rozhraní nemá přirazenou IPv6 adresu

## Zdroje
- [Získání informací o aktivních rozhraních](https://learn.microsoft.com/en-us/dotnet/api/system.net.networkinformation?view=net-8.0)
- [gitignore](https://github.com/github/gitignore/blob/main/VisualStudio.gitignore)
### Získaní informací o cíli
- [Dns Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.dns?view=net-8.0)
- [IPaddress Class](https://learn.microsoft.com/en-us/dotnet/api/system.net.ipaddress?view=net-8.0)
### Sokety
- [Raw Sokets](https://www.opensourceforu.com/2015/03/a-guide-to-using-raw-sockets/)
- [TCP](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services)